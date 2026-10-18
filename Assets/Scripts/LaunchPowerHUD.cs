using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// Power bar at the bottom center, live distance at the top, a result screen with a replay button,
// the intro screen (start, shop, gold), and the Esc pause menu (resume, main menu, volume).
// Layout and animations follow the paper/craft HUD kit specs in Docs/hud/*.json (built with HudKit).
public class LaunchPowerHUD : MonoBehaviour
{
    public PaperPlaneController plane;
    public float resultDelay = 0.8f;   // let the plane finish sliding before the result screen fades in

    const float FillWidth = 552f;      // power bar fill at 100%
    const float SweetSpot = 0.9f;      // power from here on is the "release now" zone

    static readonly Color PowerLow = new Color(0.3f, 0.85f, 0.3f);
    static readonly Color PowerMid = new Color(1f, 0.85f, 0.2f);
    static readonly Color PowerHigh = new Color(0.95f, 0.2f, 0.15f);

    // Flight HUD
    CanvasGroup powerGroup;
    RectTransform fill, sweetMarker;
    Image fillImage, sweetZone;
    Text label;
    bool labelCharging;
    CanvasGroup distanceGroup;
    RectTransform distanceMeter;
    Text distanceLabel;
    int lastHundred;
    bool distanceShown;

    // Result screen
    CanvasGroup resultGroup;
    RectTransform resultCard, resultGoldPanel, crashStamp;
    Image resultCardImage, crumpleMarks, cornerFold;
    RectTransform resultTape1, resultTape2;
    Text resultTitle, resultDistance, resultGold;
    Image resultGoldIcon;
    Button replayButton;
    bool resultShown;
    int earnedGold;        // this throw's star gold, paid into the wallet once on landing
    float landedTime = -1f;

    // Intro screen
    CanvasGroup startGroup;
    RectTransform skyline, titleCard, goldPanel, controlsNote;
    Image flightPath;
    Button startButton, shopButton, quitButton;
    Text startGoldLabel;
    int shownGold = -1;
    ShopMenu shop;
    bool started;          // false while the intro screen is up

    // Pause menu
    CanvasGroup pauseGroup;
    RectTransform pauseBox, pauseTitleStrip, volumeHandle;
    Button resumeButton, mainMenuButton;
    Text volumeLabel;
    bool paused;           // Esc menu is open; the game is frozen

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

        var canvas = canvasGo.transform;
        CreateDistanceMeter(canvas);
        CreatePowerBar(canvas);
        CreateResultScreen(canvas);
        CreateStartScreen(canvas);
        shop = gameObject.AddComponent<ShopMenu>();
        shop.Build(canvas);
        CreatePauseMenu(canvas);
        plane.enabled = false;
        PlayStartIntro();
    }

    // ---------------------------------------------------------------- flight HUD (flight_hud.json)

    // Power bar with its label card, both fading out together once the plane is thrown.
    void CreatePowerBar(Transform canvas)
    {
        var root = HudKit.Rect("PowerHud", canvas, "stretch");
        powerGroup = root.gameObject.AddComponent<CanvasGroup>();
        powerGroup.blocksRaycasts = false;

        var bar = HudKit.Image("PowerBar", root, "bar_track", "bottom", new Vector2(0f, 102f), new Vector2(560f, 44f), shadow: true).rectTransform;
        fillImage = HudKit.Image("Fill", bar, "bar_fill", "left", new Vector2(4f, 0f), new Vector2(0f, 36f), pivot: new Vector2(0f, 0.5f));
        fill = fillImage.rectTransform;
        sweetZone = HudKit.Image("SweetZone", bar, "bar_zone", "left", new Vector2(500f, 0f), new Vector2(56f, 36f), pivot: new Vector2(0f, 0.5f));
        sweetMarker = HudKit.Image("SweetMarker", bar, "mark_flag", "left", new Vector2(528f, 24f), new Vector2(32f, 32f), pivot: new Vector2(0.5f, 0f)).rectTransform;

        var card = HudKit.Image("LabelCard", root, "panel_label", "bottom", new Vector2(0f, 202f), new Vector2(560f, 64f), 1f, shadow: true);
        label = HudKit.Label("Label", card.transform, "", false, 28, "center", Vector2.zero, new Vector2(520f, 48f));
    }

    // Distance card at the top center: live meters during the throw.
    void CreateDistanceMeter(Transform canvas)
    {
        var card = HudKit.Image("DistanceMeter", canvas, "panel_label", "top", new Vector2(0f, -75f), new Vector2(360f, 70f), 1f, shadow: true);
        distanceMeter = card.rectTransform;
        distanceGroup = card.gameObject.AddComponent<CanvasGroup>();
        distanceGroup.blocksRaycasts = false;
        distanceGroup.alpha = 0f;
        HudKit.Image("DistanceTape", card.transform, "tape_2", "top", new Vector2(10f, -8f), new Vector2(130f, 44f), -3f);
        distanceLabel = HudKit.Label("Text", card.transform, "", false, 44, "center", new Vector2(0f, -2f), new Vector2(320f, 56f));
        distanceLabel.font = HudKit.CounterFont;
    }

    void UpdateFlightHud()
    {
        // Visible only while waiting on the pad (after the intro screen).
        float targetAlpha = !started || plane.IsFlying || plane.HasLanded ? 0f : 1f;
        powerGroup.alpha = Mathf.MoveTowards(powerGroup.alpha, targetAlpha, 4f * Time.unscaledDeltaTime);

        float c = plane.Charge;
        fill.sizeDelta = new Vector2(c > 0f ? Mathf.Max(36f, FillWidth * c) : 0f, fill.sizeDelta.y);
        fillImage.enabled = c > 0f;
        fillImage.color = c < 0.5f ? Color.Lerp(PowerLow, PowerMid, c * 2f) : Color.Lerp(PowerMid, PowerHigh, (c - 0.5f) * 2f);

        bool sweet = c >= SweetSpot;
        float t = Time.unscaledTime;
        sweetZone.color = new Color(1f, 1f, 1f, sweet ? 0.8f + 0.2f * Mathf.Sin(t * Mathf.PI * 5f) : 1f);
        sweetMarker.anchoredPosition = new Vector2(528f, 28f + 4f * Mathf.Sin(t * Mathf.PI * 2.5f));
        float markerScale = Mathf.MoveTowards(sweetMarker.localScale.x, sweet ? 1.3f : 1f, 3f * Time.unscaledDeltaTime);
        sweetMarker.localScale = new Vector3(markerScale, markerScale, 1f);

        bool charging = plane.IsCharging || c > 0f;
        if (charging != labelCharging)
        {
            labelCharging = charging;
            label.font = charging ? HudKit.TitleFont : HudKit.BodyFont;
            label.fontSize = charging ? 40 : 28;
            UiAnim.Punch(label.transform.parent, 1.08f, 0.12f);
        }
        label.text = charging ? $"GÜÇ %{Mathf.RoundToInt(c * 100f)}" : "Fırlatmak için SPACE basılı tut";
        label.color = charging && sweet ? HudKit.Red : HudKit.Ink;

        int meters = Mathf.RoundToInt(plane.FlightDistance);
        distanceLabel.text = $"{meters} m";
        bool inThrow = plane.IsFlying || (plane.HasLanded && resultGroup.alpha <= 0f);
        if (inThrow && !distanceShown)
        {
            UiAnim.SlideIn(distanceMeter, new Vector2(0f, 100f), 0.35f, 0f, Ease.OutBack);
            lastHundred = 0;
        }
        distanceShown = inThrow;
        distanceGroup.alpha = Mathf.MoveTowards(distanceGroup.alpha, inThrow ? 1f : 0f, 4f * Time.unscaledDeltaTime);
        if (inThrow && meters / 100 > lastHundred)
        {
            lastHundred = meters / 100;
            UiAnim.Punch(distanceLabel.transform, 1.15f, 0.2f);
        }
    }

    // ---------------------------------------------------------------- intro screen (start_screen.json)

    // Shown on Play; the plane stays disabled until BAŞLA is pressed.
    void CreateStartScreen(Transform canvas)
    {
        var screen = HudKit.Image("StartScreen", canvas, "bg_sunset", "stretch");
        screen.raycastTarget = true;
        startGroup = screen.gameObject.AddComponent<CanvasGroup>();
        var root = screen.transform;

        var cloud1 = HudKit.Image("Cloud1", root, "cloud_1", "top-left", new Vector2(260f, -225f), new Vector2(360f, 180f));
        var cloud2 = HudKit.Image("Cloud2", root, "cloud_2", "top-right", new Vector2(-260f, -455f), new Vector2(260f, 140f));
        HudKit.Image("Cloud3", root, "cloud_2", "left", new Vector2(180f, -15f), new Vector2(260f, 140f), color: HudKit.Hex("#FFFFFFCC"));
        flightPath = HudKit.Image("FlightPath", root, "flight_path", "center", new Vector2(40f, -200f), new Vector2(1400f, 300f), color: HudKit.Hex("#FFFFFFB3"));
        var planeSmall = HudKit.Image("PlaneSmall", root, "plane_small", "center", new Vector2(700f, 40f), new Vector2(160f, 100f), 12f);
        skyline = HudKit.Image("Skyline", root, "skyline_strip", "bottom", Vector2.zero, new Vector2(2560f, 220f), pivot: new Vector2(0.5f, 0f)).rectTransform;

        titleCard = HudKit.Image("TitleCard", root, "title_card", "top", new Vector2(0f, -155f), new Vector2(1060f, 270f), 2f, shadow: true).rectTransform;
        HudKit.Image("TitleTape1", titleCard, "tape_1", "top-left", new Vector2(36f, -8f), new Vector2(192f, 64f), 28f);
        HudKit.Image("TitleTape2", titleCard, "tape_2", "top-right", new Vector2(-40f, -14f), new Vector2(160f, 56f), -24f);
        var title = HudKit.Label("Title", titleCard, "KAĞIT UÇAK", true, 170, "center", Vector2.zero, new Vector2(960f, 200f), HudKit.Red);
        var outline = title.gameObject.AddComponent<Outline>();
        outline.effectColor = HudKit.Ink;
        outline.effectDistance = new Vector2(4f, 4f);
        HudKit.AddShadow(title);

        startButton = HudKit.Button("StartButton", root, "BAŞLA", "primary", 64, () =>
        {
            started = true;
            plane.enabled = true;
            startGroup.interactable = startGroup.blocksRaycasts = false;
        }, "center", new Vector2(-30f, -65f), new Vector2(440f, 120f), 2f);
        shopButton = HudKit.Button("ShopButton", root, "MAĞAZA", "secondary", 48, () => shop.Open(),
            "center", new Vector2(-15f, -196f), new Vector2(340f, 92f), -1.5f);
        // Top-left, mirroring the gold panel (same spot as the shop's GERİ button).
        quitButton = HudKit.Button("QuitButton", root, "ÇIKIŞ", "danger", 46, QuitGame,
            "top-left", new Vector2(160f, -72f), new Vector2(240f, 84f), 1.5f);
        quitButton.GetComponentInChildren<Text>().color = HudKit.Paper;

        var note = HudKit.Image("ControlsNote", root, "panel_label", "bottom", new Vector2(0f, 208f), new Vector2(1060f, 84f), 1f, shadow: true);
        controlsNote = note.rectTransform;
        note.gameObject.AddComponent<CanvasGroup>();
        HudKit.Image("ControlsClip", note.transform, "clip", "top-left", new Vector2(63f, -17f), new Vector2(26f, 70f), -8f);
        HudKit.Label("Controls", note.transform, "SPACE basılı tut: güç   •   Fare / WASD: yönlendir   •   Yıldızlar hız verir", false, 26,
            "center", Vector2.zero, new Vector2(980f, 56f));

        var gold = HudKit.Image("GoldPanel", root, "panel_label", "top-right", new Vector2(-230f, -70f), new Vector2(400f, 76f), 1.5f, shadow: true);
        goldPanel = gold.rectTransform;
        HudKit.Icon("GoldIcon", gold.transform, "icon_coin", "left", new Vector2(44f, 0f), 48f);
        startGoldLabel = HudKit.Label("Gold", gold.transform, "", false, 34, "right", new Vector2(-28f, 0f), new Vector2(290f, 56f),
            align: TextAnchor.MiddleRight, pivot: new Vector2(1f, 0.5f));
        startGoldLabel.font = HudKit.CounterFont;

        // Endless drift of the collage pieces.
        UiAnim.Loop(cloud1.rectTransform, new Vector2(30f, 0f), 0f, 6f);
        UiAnim.Loop(cloud2.rectTransform, new Vector2(24f, 0f), 0f, 7.5f);
        UiAnim.Loop(planeSmall.rectTransform, new Vector2(0f, 14f), 4f, 1.5f);
        UiAnim.Pulse(startButton.GetComponent<UiHover>(), 1.03f, 1.6f);
    }

    // Opening animation of the intro screen (on Play and on every return to the main menu).
    void PlayStartIntro()
    {
        foreach (Object target in new Object[] { startGroup, skyline, titleCard, flightPath, goldPanel, controlsNote, quitButton.transform })
            UiAnim.Kill(target);
        UiAnim.Fade(startGroup, 0f, 1f, 0.3f);
        UiAnim.SlideIn(skyline, new Vector2(0f, -220f), 0.5f);
        UiAnim.SlideIn(titleCard, new Vector2(0f, 300f), 0.5f, 0f, Ease.OutBack);
        UiAnim.Play(flightPath, 0.6f, k => flightPath.color = new Color(1f, 1f, 1f, 0.7f * k), 0.5f);
        UiAnim.SlideIn(goldPanel, new Vector2(200f, 0f), 0.3f, 0.2f);
        UiAnim.SlideIn((RectTransform)quitButton.transform, new Vector2(-200f, 0f), 0.3f, 0.2f);
        UiAnim.ScaleIn(startButton.transform, 0.8f, 0.3f, 0.55f);
        UiAnim.ScaleIn(shopButton.transform, 0.8f, 0.3f, 0.7f);
        UiAnim.SlideIn(controlsNote, new Vector2(0f, -20f), 0.3f, 0.9f);
        UiAnim.Fade(controlsNote.GetComponent<CanvasGroup>(), 0f, 1f, 0.3f, 0.9f);
    }

    // ---------------------------------------------------------------- result screen (result_screen.json)

    // Paper card that drops in after landing: distance, gold earned and a replay button; crumpled with a stamp after a crash.
    void CreateResultScreen(Transform canvas)
    {
        var screen = HudKit.Rect("ResultScreen", canvas, "stretch");
        screen.gameObject.AddComponent<Image>().color = HudKit.Hex("#000000CC");
        resultGroup = screen.gameObject.AddComponent<CanvasGroup>();

        resultCardImage = HudKit.Image("ResultCard", screen, "panel_large", "center", new Vector2(0f, 50f), new Vector2(900f, 620f), 2f, shadow: true);
        resultCard = resultCardImage.rectTransform;
        crumpleMarks = HudKit.Image("CrumpleMarks", resultCard, "crumple_marks", "stretch");
        resultTape1 = HudKit.Image("CardTape1", resultCard, "tape_1", "top-left", new Vector2(70f, -2f), new Vector2(192f, 64f), 30f).rectTransform;
        resultTape2 = HudKit.Image("CardTape2", resultCard, "tape_2", "top-right", new Vector2(-66f, -2f), new Vector2(160f, 56f), -26f).rectTransform;
        cornerFold = HudKit.Image("CornerFold", resultCard, "corner_fold", "bottom-right", new Vector2(-42f, 42f), new Vector2(72f, 72f), -90f);   // turned about its center, like the mockup

        resultTitle = HudKit.Label("Title", resultCard, "Gidilen mesafe", true, 60, "top", new Vector2(0f, -75f), new Vector2(800f, 90f));
        resultDistance = HudKit.Label("Distance", resultCard, "", false, 170, "center", new Vector2(0f, 40f), new Vector2(800f, 240f));
        resultDistance.font = HudKit.CounterFont;
        var distanceShadow = resultDistance.gameObject.AddComponent<Shadow>();
        distanceShadow.effectColor = HudKit.Kraft;
        distanceShadow.effectDistance = new Vector2(6f, -6f);

        var goldStrip = HudKit.Image("GoldPanel", resultCard, "strip_kraft", "bottom", new Vector2(0f, 126f), new Vector2(700f, 88f), -1f);
        resultGoldPanel = goldStrip.rectTransform;
        goldStrip.gameObject.AddComponent<CanvasGroup>();
        resultGoldIcon = HudKit.Icon("GoldIcon", goldStrip.transform, "icon_coin", "left", new Vector2(64f, 0f), 52f);
        resultGold = HudKit.Label("Gold", goldStrip.transform, "", false, 36, "center", new Vector2(28f, 0f), new Vector2(560f, 60f));
        resultGold.font = HudKit.CounterFont;

        crashStamp = HudKit.Image("CrashStamp", resultCard, "stamp_crash", "top-right", new Vector2(-70f, -30f), new Vector2(200f, 200f), 14f,
            HudKit.Hex("#FFFFFFE6")).rectTransform;

        replayButton = HudKit.Button("ReplayButton", screen, "TEKRAR OYNA", "primary", 60, () =>
        {
            plane.ResetToPad();
            SetResultVisible(0f);
        }, "center", new Vector2(0f, -350f), new Vector2(560f, 120f), 1.5f);
        UiAnim.Pulse(replayButton.GetComponent<UiHover>(), 1.03f, 1.6f);

        SetResultVisible(0f);
    }

    // Fills in and animates the result card the moment it starts fading in.
    void ShowResult()
    {
        bool crash = plane.Crashed;
        int meters = Mathf.RoundToInt(plane.FlightDistance);
        int total = PlayerWallet.Gold;

        resultCardImage.color = crash ? HudKit.Hex("#EFE3CC") : Color.white;
        crumpleMarks.gameObject.SetActive(crash);
        cornerFold.gameObject.SetActive(crash);
        crashStamp.gameObject.SetActive(crash);
        resultTitle.fontSize = crash ? 56 : 60;
        resultTitle.text = crash ? "<color=#B1180B>ÇARPTIN!</color>  Gidilen mesafe" : "Gidilen mesafe";

        UiAnim.Kill(resultCard);
        resultCard.anchoredPosition = new Vector2(0f, 50f);
        resultCard.localEulerAngles = new Vector3(0f, 0f, 2f);
        UiAnim.SlideIn(resultCard, new Vector2(0f, 700f), 0.45f, 0f, Ease.OutBack);
        UiAnim.RotateIn(resultCard, 8f, 0.45f);
        UiAnim.ScaleIn(resultTape1, 0f, 0.15f, 0.45f);
        UiAnim.ScaleIn(resultTape2, 0f, 0.15f, 0.5f);
        UiAnim.Play(resultTitle, 0.2f, k => resultTitle.color = new Color(HudKit.Ink.r, HudKit.Ink.g, HudKit.Ink.b, k), 0.45f);
        if (crash)
        {
            UiAnim.Shake(resultTitle.rectTransform, 6f, 0.3f, 4, 0.6f);
            crashStamp.localScale = Vector3.zero;   // stamped in after the card lands
            UiAnim.ScaleIn(crashStamp, 2f, 0.18f, 0.5f, Ease.InQuad, applyNow: false);
            UiAnim.Shake(resultCard, 8f, 0.1f, 2, 0.7f);
        }

        resultDistance.transform.localScale = Vector3.one;
        UiAnim.Count(resultDistance, 0f, meters, 1.2f, v => $"{v} m", 0.3f,
            () => UiAnim.Punch(resultDistance.transform, 1.12f, 0.2f));

        resultGoldPanel.anchoredPosition = new Vector2(0f, 130f);
        UiAnim.SlideIn(resultGoldPanel, new Vector2(-300f, 0f), 0.25f, 1f);
        UiAnim.Fade(resultGoldPanel.GetComponent<CanvasGroup>(), 0f, 1f, 0.25f, 1f);
        int before = total - earnedGold;
        UiAnim.Play(resultGold, 0.6f, k =>
            resultGold.text = $"+{Mathf.RoundToInt(earnedGold * k)} ALTIN  (Toplam {Mathf.RoundToInt(Mathf.Lerp(before, total, k))})",
            1.1f, Ease.OutCubic, done: () => UiAnim.Punch(resultGoldIcon.transform, 1.2f, 0.3f));

        UiAnim.ScaleIn(replayButton.transform, 0.8f, 0.3f, 1.4f);
    }

    void SetResultVisible(float alpha)
    {
        resultGroup.alpha = alpha;
        resultGroup.interactable = resultGroup.blocksRaycasts = alpha > 0.99f;
    }

    void UpdateResult()
    {
        if (!plane.HasLanded)
        {
            landedTime = -1f;
            resultShown = false;
            if (resultGroup.alpha > 0f) SetResultVisible(0f);
            return;
        }
        if (landedTime < 0f)
        {
            landedTime = Time.unscaledTime;
            earnedGold = plane.ThrowGold;
            PlayerWallet.AddGold(earnedGold);
        }
        if (Time.unscaledTime - landedTime < resultDelay) return;
        if (!resultShown)
        {
            resultShown = true;
            ShowResult();
        }
        SetResultVisible(Mathf.MoveTowards(resultGroup.alpha, 1f, Time.unscaledDeltaTime / 0.4f));
    }

    // ---------------------------------------------------------------- pause menu (pause_menu.json)

    // Esc menu: resume, back to the intro screen, and the master volume slider.
    void CreatePauseMenu(Transform canvas)
    {
        var screen = HudKit.Rect("PauseMenu", canvas, "stretch");
        screen.gameObject.AddComponent<Image>().color = HudKit.Hex("#00000099");
        pauseGroup = screen.gameObject.AddComponent<CanvasGroup>();
        pauseGroup.alpha = 0f;

        pauseBox = HudKit.Image("Box", screen, "panel_large", "center", new Vector2(0f, 110f), new Vector2(720f, 640f), 1.5f, shadow: true).rectTransform;
        HudKit.Image("BoxTape1", pauseBox, "tape_1", "top-left", new Vector2(40f, 10f), new Vector2(192f, 64f), 32f);
        HudKit.Image("BoxTape2", pauseBox, "tape_2", "bottom-right", new Vector2(-24f, -2f), new Vector2(160f, 56f), 30f);
        pauseTitleStrip = HudKit.Image("TitleStrip", pauseBox, "strip_kraft", "top", new Vector2(0f, 10f), new Vector2(500f, 104f), -1.5f, shadow: true).rectTransform;
        HudKit.Label("Title", pauseTitleStrip, "DURAKLATILDI", true, 56, "center", new Vector2(0f, 2f), new Vector2(440f, 80f));

        resumeButton = HudKit.Button("ResumeButton", pauseBox, "DEVAM ET", "primary", 58, () => SetPaused(false),
            "top", new Vector2(0f, -170f), new Vector2(480f, 112f), 1.5f);
        mainMenuButton = HudKit.Button("MainMenuButton", pauseBox, "ANA MENÜ", "secondary", 46, ReturnToMainMenu,
            "top", new Vector2(0f, -300f), new Vector2(400f, 92f), -1f);

        var volumeIcon = HudKit.Icon("VolumeIcon", pauseBox, "icon_sound", "top", new Vector2(-122f, -414f), 56f);
        HudKit.Label("VolumeTitle", pauseBox, "SES", true, 48, "top", new Vector2(-82f, -414f), new Vector2(160f, 64f),
            align: TextAnchor.MiddleLeft, pivot: new Vector2(0f, 0.5f));
        CreateVolumeSlider(pauseBox, volumeIcon.transform);
        volumeLabel = HudKit.Label("VolumeValue", pauseBox, "", false, 40, "top", new Vector2(262f, -484f), new Vector2(130f, 64f));
        volumeLabel.font = HudKit.CounterFont;
        UpdateVolumeLabel(GameSettings.MasterVolume);

        SetPauseVisible(false);
    }

    void CreateVolumeSlider(Transform parent, Transform volumeIcon)
    {
        var track = HudKit.Image("VolumeSlider", parent, "slider_track", "top", new Vector2(-50f, -484f), new Vector2(480f, 32f));
        track.raycastTarget = true;
        var root = track.rectTransform;

        var fillArea = HudKit.Rect("FillArea", root, "stretch");
        fillArea.offsetMin = new Vector2(3f, 4f);
        fillArea.offsetMax = new Vector2(-3f, -4f);
        var fillRect = HudKit.Image("Fill", fillArea, "slider_fill", "stretch", color: HudKit.BlueInk).rectTransform;

        var handleArea = HudKit.Rect("HandleArea", root, "stretch");
        handleArea.offsetMin = new Vector2(28f, 0f);
        handleArea.offsetMax = new Vector2(-28f, 0f);   // keeps the handle on the track at 100%
        var handleImage = HudKit.Image("Handle", handleArea, "slider_handle", "center", Vector2.zero, new Vector2(64f, 64f));
        handleImage.raycastTarget = true;
        volumeHandle = handleImage.rectTransform;

        var slider = root.gameObject.AddComponent<Slider>();
        slider.fillRect = fillRect;
        slider.handleRect = volumeHandle;
        slider.targetGraphic = handleImage;
        var colors = slider.colors;
        colors.pressedColor = HudKit.Hex("#FFE7A8");
        slider.colors = colors;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = GameSettings.MasterVolume;
        slider.onValueChanged.AddListener(v =>
        {
            GameSettings.MasterVolume = v;
            UpdateVolumeLabel(v);
            UiAnim.Punch(volumeIcon, 1.15f, 0.15f);
        });

        // While dragging: bigger handle and a red value.
        var trigger = root.gameObject.AddComponent<EventTrigger>();
        AddTrigger(trigger, EventTriggerType.PointerDown, () => SetVolumeDragging(true));
        AddTrigger(trigger, EventTriggerType.PointerUp, () => SetVolumeDragging(false));
    }

    static void AddTrigger(EventTrigger trigger, EventTriggerType type, System.Action action)
    {
        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(_ => action());
        trigger.triggers.Add(entry);
    }

    void SetVolumeDragging(bool dragging)
    {
        UiAnim.Kill(volumeHandle);
        UiAnim.ScaleIn(volumeHandle, volumeHandle.localScale.x, 0.1f, 0f, Ease.OutBack, dragging ? 1.2f : 1f);
        volumeLabel.color = dragging ? HudKit.Red : HudKit.Ink;
    }

    void UpdateVolumeLabel(float v) => volumeLabel.text = $"%{Mathf.RoundToInt(v * 100f)}";

    void SetPauseVisible(bool visible)
    {
        pauseGroup.interactable = pauseGroup.blocksRaycasts = visible;
        UiAnim.Kill(pauseGroup);
        if (!visible)
        {
            UiAnim.Fade(pauseGroup, pauseGroup.alpha, 0f, 0.15f, 0f, Ease.InQuad);
            return;
        }
        UiAnim.Fade(pauseGroup, 0f, 1f, 0.2f);
        UiAnim.Kill(pauseBox);
        pauseBox.localEulerAngles = new Vector3(0f, 0f, 1.5f);
        UiAnim.ScaleIn(pauseBox, 0.8f, 0.3f);
        UiAnim.RotateIn(pauseBox, 6f, 0.3f);
        UiAnim.Kill(pauseTitleStrip);
        pauseTitleStrip.anchoredPosition = new Vector2(0f, 10f);
        UiAnim.SlideIn(pauseTitleStrip, new Vector2(0f, 30f), 0.2f, 0.3f);
        UiAnim.ScaleIn(resumeButton.transform, 0.8f, 0.25f, 0.15f);
        UiAnim.ScaleIn(mainMenuButton.transform, 0.8f, 0.25f, 0.25f);
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
        PlayStartIntro();
    }

    // ÇIKIŞ on the intro screen: closes the game (stops Play mode in the Editor).
    static void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void OnDestroy() => Time.timeScale = 1f;

    // ---------------------------------------------------------------- per frame

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

    void Update()
    {
        if (plane == null || powerGroup == null) return;

        UpdateCursor();
        int gold = PlayerWallet.Gold;
        if (gold != shownGold)
        {
            if (shownGold >= 0) UiAnim.Punch(startGoldLabel.transform, 1.15f, 0.2f);
            shownGold = gold;
            startGoldLabel.text = $"ALTIN: {gold}";
        }
        // The intro fades out once the game starts; its fade-in is part of PlayStartIntro.
        if (started) startGroup.alpha = Mathf.MoveTowards(startGroup.alpha, 0f, 3f * Time.unscaledDeltaTime);

        UpdateFlightHud();
        UpdateResult();
    }
}
