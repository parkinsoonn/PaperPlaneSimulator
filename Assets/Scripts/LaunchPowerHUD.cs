using UnityEngine;
using UnityEngine.UI;

// Power bar at the bottom center of the screen that fills while Space is held.
public class LaunchPowerHUD : MonoBehaviour
{
    public PaperPlaneController plane;
    public Vector2 barSize = new Vector2(500f, 28f);
    public float bottomMargin = 80f;

    CanvasGroup group;
    RectTransform barRoot, fill;
    Image fillImage;
    Text label;

    void Start()
    {
        var canvasGo = new GameObject("LaunchPowerCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
        canvasGo.transform.SetParent(transform, false);
        canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        group = canvasGo.GetComponent<CanvasGroup>();
        group.blocksRaycasts = false;

        barRoot = CreateRect("PowerBar", canvasGo.transform);
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

        // Visible only while waiting on the ground.
        float targetAlpha = plane.IsFlying ? 0f : 1f;
        group.alpha = Mathf.MoveTowards(group.alpha, targetAlpha, 4f * Time.unscaledDeltaTime);

        float c = plane.Charge;
        fill.anchorMax = new Vector2(c, 1f);
        fillImage.color = c < 0.5f
            ? Color.Lerp(new Color(0.3f, 0.85f, 0.3f), new Color(1f, 0.85f, 0.2f), c * 2f)
            : Color.Lerp(new Color(1f, 0.85f, 0.2f), new Color(0.95f, 0.2f, 0.15f), (c - 0.5f) * 2f);

        label.text = plane.IsCharging || c > 0f
            ? $"GÜÇ %{Mathf.RoundToInt(c * 100f)}"
            : "Fırlatmak için SPACE basılı tut";

        // Pulse at full power.
        float pulse = c >= 1f ? 1f + 0.04f * Mathf.Sin(Time.unscaledTime * 18f) : 1f;
        barRoot.localScale = new Vector3(pulse, pulse, 1f);
    }
}
