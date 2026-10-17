using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// Power bar at the bottom center, live distance at the top, a result screen with a replay button,
// the intro screen, and the Esc pause menu (resume, main menu, volume).
public class LaunchPowerHUD : MonoBehaviour
{
    public PaperPlaneController plane;
    public Vector2 barSize = new Vector2(500f, 28f);
    public float bottomMargin = 80f;
    public float resultDelay = 0.8f;   // let the plane finish sliding before the result screen fades in

    CanvasGroup group;
    RectTransform barRoot, fill;
    Image fillImage;
    Text label;
    CanvasGroup distanceGroup;
    Text distanceLabel;
    CanvasGroup resultGroup;
    Text resultTitle;
    Text resultDistance;
    float landedTime = -1f;
    CanvasGroup startGroup;
    bool started;          // false while the intro screen is up
    CanvasGroup pauseGroup;
    bool paused;           // Esc menu is open; the game is frozen
    Text volumeLabel;

    void Start()
    {
        if (FindAnyObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        var canvasGo = new GameObject("LaunchPowerCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(transform, false);
        canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        CreateDistanceMeter(canvasGo.transform);

        barRoot = CreateRect("PowerBar", canvasGo.transform);
        group = barRoot.gameObject.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        barRoot.anchorMin = barRoot.anchorMax = barRoot.pivot = new Vector2(0.5f, 0f);
        barRoot.anchoredPosition = new Vector2(0f, bottomMargin);
        barRoot.sizeDelta = barSize;
        barRoot.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);

        fill = CreateRect("Fill", barRoot);
        fill.anchorMin = Vector2.zero;
        fill.anchorMax = new Vector2(0f, 1f);
        fill.offsetMin = new Vector2(3f, 3f);
        fill.offsetMax = new Vector2(-3f, -3f);
        fillImage = fill.gameObject.AddComponent<Image>();

        var labelRect = CreateRect("Label", barRoot);
        labelRect.anchorMin = new Vector2(0f, 1f);
        labelRect.anchorMax = Vector2.one;
        labelRect.pivot = new Vector2(0.5f, 0f);
        labelRect.anchoredPosition = new Vector2(0f, 6f);
        labelRect.sizeDelta = new Vector2(0f, 36f);
        label = labelRect.gameObject.AddComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 26;
        label.fontStyle = FontStyle.Bold;
        label.alignment = TextAnchor.LowerCenter;
        label.color = Color.white;
        labelRect.gameObject.AddComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.8f);

        CreateResultScreen(canvasGo.transform);
        CreateStartScreen(canvasGo.transform);
        CreatePauseMenu(canvasGo.transform);
        plane.enabled = false;
    }

    // Esc menu: resume, back to the intro screen, and the master volume slider.
    void CreatePauseMenu(Transform canvas)
    {
        var panel = CreateRect("PauseMenu", canvas);
        panel.anchorMin = Vector2.zero;
        panel.anchorMax = Vector2.one;
        panel.offsetMin = panel.offsetMax = Vector2.zero;
        panel.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);
        pauseGroup = panel.gameObject.AddComponent<CanvasGroup>();

        var box = CreateRect("Box", panel);
        box.anchoredPosition = new Vector2(0f, 30f);
        box.sizeDelta = new Vector2(680f, 480f);
        box.gameObject.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.16f, 0.95f);

        CreateText("Title", box, "DURAKLATILDI", 56, new Vector2(0f, 200f), new Vector2(640f, 80f));
        CreateButton("ResumeButton", box, "DEVAM ET", new Vector2(0f, 80f), () => SetPaused(false), new Color(1f, 0.78f, 0.2f));
        CreateButton("MainMenuButton", box, "ANA MENÜ", new Vector2(0f, -20f), ReturnToMainMenu);

        CreateText("VolumeTitle", box, "SES", 32, new Vector2(-230f, -150f), new Vector2(120f, 50f));
        CreateVolumeSlider(box, new Vector2(20f, -150f), new Vector2(340f, 28f));
        volumeLabel = CreateText("VolumeValue", box, "", 30, new Vector2(250f, -150f), new Vector2(120f, 50f));
        UpdateVolumeLabel(GameSettings.MasterVolume);

        SetPauseVisible(false);
    }

    void CreateVolumeSlider(Transform parent, Vector2 position, Vector2 size)
    {
        var root = CreateRect("VolumeSlider", parent);
        root.anchoredPosition = position;
        root.sizeDelta = size;
        var background = root.gameObject.AddComponent<Image>();
        background.color = new Color(0.25f, 0.25f, 0.3f);

        var fillArea = CreateRect("FillArea", root);
        fillArea.anchorMin = Vector2.zero;
        fillArea.anchorMax = Vector2.one;
        fillArea.offsetMin = new Vector2(4f, 4f);
        fillArea.offsetMax = new Vector2(-4f, -4f);
        var fillRect = CreateRect("Fill", fillArea);
        fillRect.offsetMin = fillRect.offsetMax = Vector2.zero;
        fillRect.gameObject.AddComponent<Image>().color = new Color(1f, 0.78f, 0.2f);

        var handleArea = CreateRect("HandleArea", root);
        handleArea.anchorMin = Vector2.zero;
        handleArea.anchorMax = Vector2.one;
        handleArea.offsetMin = new Vector2(12f, 0f);
        handleArea.offsetMax = new Vector2(-12f, 0f);
        var handle = CreateRect("Handle", handleArea);
        handle.sizeDelta = new Vector2(24f, 0f);
        var handleImage = handle.gameObject.AddComponent<Image>();
        handleImage.color = Color.white;

        var slider = root.gameObject.AddComponent<Slider>();
        slider.fillRect = fillRect;
        slider.handleRect = handle;
        slider.targetGraphic = handleImage;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = GameSettings.MasterVolume;
        slider.onValueChanged.AddListener(v =>
        {
            GameSettings.MasterVolume = v;
            UpdateVolumeLabel(v);
        });
    }

    void UpdateVolumeLabel(float v) => volumeLabel.text = $"%{Mathf.RoundToInt(v * 100f)}";

    void SetPauseVisible(bool visible)
    {
        pauseGroup.alpha = visible ? 1f : 0f;
        pauseGroup.interactable = pauseGroup.blocksRaycasts = visible;
    }

    // Freezes the game (physics, traffic, stars) and blocks plane input while the menu is open.
    void SetPaused(bool value)
    {
        paused = value;
        Time.timeScale = value ? 0f : 1f;
        plane.enabled = started && !value;
        SetPauseVisible(value);
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }

    // Back to the intro screen with the plane on the pad (street, traffic and stars reset with it).
    void ReturnToMainMenu()
    {
        SetPaused(false);
        plane.ResetToPad();
        SetResultVisible(0f);
        started = false;
        plane.enabled = false;
        startGroup.interactable = startGroup.blocksRaycasts = true;
    }

    void OnDestroy() => Time.timeScale = 1f;

    // Full-screen dimmed panel shown after landing: distance and a replay button.
    void CreateResultScreen(Transform canvas)
    {
        var panel = CreateRect("ResultScreen", canvas);
        panel.anchorMin = Vector2.zero;
        panel.anchorMax = Vector2.one;
        panel.offsetMin = panel.offsetMax = Vector2.zero;
        panel.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.9f);
        resultGroup = panel.gameObject.AddComponent<CanvasGroup>();
        SetResultVisible(0f);

        resultTitle = CreateText("Title", panel, "Gidilen mesafe", 36, new Vector2(0f, 200f), new Vector2(800f, 60f));
        resultDistance = CreateText("Distance", panel, "", 110, new Vector2(0f, 110f), new Vector2(800f, 130f));

        CreateButton("ReplayButton", panel, "TEKRAR OYNA", new Vector2(0f, -110f), () =>
        {
            plane.ResetToPad();
            SetResultVisible(0f);
        });
    }

    // Intro screen shown on Play; the plane stays disabled until BAŞLA is pressed.
    void CreateStartScreen(Transform canvas)
    {
        var panel = CreateRect("StartScreen", canvas);
        panel.anchorMin = Vector2.zero;
        panel.anchorMax = Vector2.one;
        panel.offsetMin = panel.offsetMax = Vector2.zero;
        panel.gameObject.AddComponent<RawImage>().texture = CreateGradient(
            new Color(0.16f, 0.12f, 0.45f),    // top: deep indigo
            new Color(0.85f, 0.3f, 0.6f),      // middle: magenta
            new Color(1f, 0.6f, 0.22f));       // bottom: warm orange
        startGroup = panel.gameObject.AddComponent<CanvasGroup>();

        CreateText("Title", panel, "KAĞIT UÇAK", 120, new Vector2(0f, 170f), new Vector2(1200f, 150f))
            .color = new Color(1f, 0.9f, 0.3f);
        CreateText("Controls", panel, "SPACE basılı tut: güç   •   Fare / WASD: yönlendir   •   Yıldızlar hız verir",
            30, new Vector2(0f, 40f), new Vector2(1400f, 50f));
        CreateButton("StartButton", panel, "BAŞLA", new Vector2(0f, -110f), () =>
        {
            started = true;
            plane.enabled = true;
            startGroup.interactable = startGroup.blocksRaycasts = false;
        }, new Color(1f, 0.78f, 0.2f));
    }

    // Vertical three-color gradient (top -> middle -> bottom) as a small stretched texture.
    static Texture2D CreateGradient(Color top, Color middle, Color bottom)
    {
        const int height = 256;
        var tex = new Texture2D(1, height, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };
        for (int y = 0; y < height; y++)
        {
            float t = y / (height - 1f);   // 0 = bottom, 1 = top
            tex.SetPixel(0, y, t < 0.5f ? Color.Lerp(bottom, middle, t * 2f) : Color.Lerp(middle, top, (t - 0.5f) * 2f));
        }
        tex.Apply();
        return tex;
    }

    // Light button with dark text; deselects itself after a click so Space doesn't re-press it.
    void CreateButton(string name, Transform parent, string text, Vector2 position, UnityEngine.Events.UnityAction onClick,
        Color? color = null)
    {
        var button = CreateRect(name, parent);
        button.anchoredPosition = position;
        button.sizeDelta = new Vector2(340f, 80f);
        var buttonImage = button.gameObject.AddComponent<Image>();
        buttonImage.color = color ?? new Color(0.95f, 0.93f, 0.88f);
        var btn = button.gameObject.AddComponent<Button>();
        btn.targetGraphic = buttonImage;
        btn.onClick.AddListener(onClick);
        btn.onClick.AddListener(() =>
        {
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        });
        var buttonText = CreateText("Text", button, text, 34, Vector2.zero, Vector2.zero);
        var buttonTextRect = buttonText.rectTransform;
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonText.color = new Color(0.15f, 0.15f, 0.15f);
        Destroy(buttonText.GetComponent<Outline>());
    }

    void SetResultVisible(float alpha)
    {
        resultGroup.alpha = alpha;
        resultGroup.interactable = resultGroup.blocksRaycasts = alpha > 0.99f;
    }

    // Centered white bold text with an outline, positioned relative to the parent's center.
    static Text CreateText(string name, Transform parent, string text, int size, Vector2 position, Vector2 boxSize)
    {
        var rect = CreateRect(name, parent);
        rect.anchoredPosition = position;
        rect.sizeDelta = boxSize;
        var t = rect.gameObject.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = size;
        t.fontStyle = FontStyle.Bold;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        t.text = text;
        rect.gameObject.AddComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.8f);
        return t;
    }

    // Distance box at the top center: live meters during the throw.
    void CreateDistanceMeter(Transform canvas)
    {
        var box = CreateRect("DistanceMeter", canvas);
        box.anchorMin = box.anchorMax = box.pivot = new Vector2(0.5f, 1f);
        box.anchoredPosition = new Vector2(0f, -40f);
        box.sizeDelta = new Vector2(360f, 70f);
        box.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);
        distanceGroup = box.gameObject.AddComponent<CanvasGroup>();
        distanceGroup.blocksRaycasts = false;
        distanceGroup.alpha = 0f;

        var textRect = CreateRect("Text", box);
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = textRect.offsetMax = Vector2.zero;
        distanceLabel = textRect.gameObject.AddComponent<Text>();
        distanceLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        distanceLabel.fontSize = 40;
        distanceLabel.fontStyle = FontStyle.Bold;
        distanceLabel.alignment = TextAnchor.MiddleCenter;
        distanceLabel.color = Color.white;
        textRect.gameObject.AddComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.8f);
    }

    // Esc opens/closes the pause menu once the game has started. The cursor is hidden and locked to
    // the game (mouse steers the plane), except on the intro, result and pause screens.
    void UpdateCursor()
    {
        var kb = Keyboard.current;
        if (started && kb != null && kb.escapeKey.wasPressedThisFrame) SetPaused(!paused);

        bool locked = started && !paused && resultGroup.alpha <= 0f;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    static RectTransform CreateRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    void Update()
    {
        if (plane == null || group == null) return;

        UpdateCursor();
        startGroup.alpha = Mathf.MoveTowards(startGroup.alpha, started ? 0f : 1f, 3f * Time.unscaledDeltaTime);

        // Visible only while waiting on the pad (after the intro screen).
        float targetAlpha = !started || plane.IsFlying || plane.HasLanded ? 0f : 1f;
        group.alpha = Mathf.MoveTowards(group.alpha, targetAlpha, 4f * Time.unscaledDeltaTime);

        float c = plane.Charge;
        fill.anchorMax = new Vector2(c, 1f);
        fillImage.color = c < 0.5f
            ? Color.Lerp(new Color(0.3f, 0.85f, 0.3f), new Color(1f, 0.85f, 0.2f), c * 2f)
            : Color.Lerp(new Color(1f, 0.85f, 0.2f), new Color(0.95f, 0.2f, 0.15f), (c - 0.5f) * 2f);

        label.text = plane.IsCharging || c > 0f
            ? $"GÜÇ %{Mathf.RoundToInt(c * 100f)}"
            : "Fırlatmak için SPACE basılı tut";

        int meters = Mathf.RoundToInt(plane.FlightDistance);
        distanceLabel.text = $"{meters} m";
        bool inThrow = plane.IsFlying || (plane.HasLanded && resultGroup.alpha <= 0f);
        distanceGroup.alpha = Mathf.MoveTowards(distanceGroup.alpha, inThrow ? 1f : 0f, 4f * Time.unscaledDeltaTime);

        if (!plane.HasLanded)
        {
            landedTime = -1f;
            if (resultGroup.alpha > 0f) SetResultVisible(0f);
            return;
        }
        if (landedTime < 0f) landedTime = Time.unscaledTime;
        resultDistance.text = $"{meters} m";
        resultTitle.text = plane.Crashed ? "ÇARPTIN!  Gidilen mesafe" : "Gidilen mesafe";
        resultTitle.color = plane.Crashed ? new Color(1f, 0.35f, 0.3f) : Color.white;
        if (Time.unscaledTime - landedTime >= resultDelay)
            SetResultVisible(Mathf.MoveTowards(resultGroup.alpha, 1f, 3f * Time.unscaledDeltaTime));
    }
}
