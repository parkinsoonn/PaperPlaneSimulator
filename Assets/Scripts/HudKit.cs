using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// The paper/craft HUD kit from Claude Design (specs in Docs/hud/*.json): palette, fonts, sprites
// (Assets/Resources/UI, rendered by Tools/render-hud-sprites.ps1) and builders that take the spec's
// fields directly: anchor name, pivot, pos (from the anchor, y up), size and rotation.
public static class HudKit
{
    public static readonly Color Paper = Hex("#F4EBDD");
    public static readonly Color Kraft = Hex("#C9A27A");
    public static readonly Color Ink = Hex("#2B2B3A");
    public static readonly Color Red = Hex("#B1180B");
    public static readonly Color Gold = Hex("#F2B630");
    public static readonly Color BlueInk = Hex("#2F5D9E");
    public static readonly Color ShadowColor = Hex("#0000004D");
    public static readonly Vector2 ShadowOffset = new Vector2(4f, -4f);

    static Font titleFont, bodyFont, counterFont;
    static readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

    // Caveat Brush for titles and buttons, Nunito Bold for body text, Nunito ExtraBold for counters (Assets/Resources/Fonts, OFL).
    public static Font TitleFont => titleFont != null ? titleFont : titleFont = LoadFont("Fonts/CaveatBrush-Regular");
    public static Font BodyFont => bodyFont != null ? bodyFont : bodyFont = LoadFont("Fonts/Nunito-Bold");
    public static Font CounterFont => counterFont != null ? counterFont : counterFont = LoadFont("Fonts/Nunito-ExtraBold");

    static Font LoadFont(string path)
    {
        var font = Resources.Load<Font>(path);
        return font != null ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    public static Sprite Sprite(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        if (!sprites.TryGetValue(id, out var sprite))
        {
            sprite = Resources.Load<Sprite>("UI/" + id);
            if (sprite == null) Debug.LogWarning($"HUD sprite missing: {id}");
            sprites[id] = sprite;
        }
        return sprite;
    }

    public static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.magenta;

    static Vector2 AnchorPoint(string anchor)
    {
        switch (anchor)
        {
            case "top": return new Vector2(0.5f, 1f);
            case "bottom": return new Vector2(0.5f, 0f);
            case "left": return new Vector2(0f, 0.5f);
            case "right": return new Vector2(1f, 0.5f);
            case "top-left": return new Vector2(0f, 1f);
            case "top-right": return new Vector2(1f, 1f);
            case "bottom-left": return new Vector2(0f, 0f);
            case "bottom-right": return new Vector2(1f, 0f);
            default: return new Vector2(0.5f, 0.5f);
        }
    }

    // Empty rect placed like a spec element. "stretch" fills the parent (pos/size ignored).
    public static RectTransform Rect(string name, Transform parent, string anchor = "center", Vector2 pos = default,
        Vector2 size = default, float rotation = 0f, Vector2? pivot = null)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        if (anchor == "stretch")
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
        else
        {
            rt.anchorMin = rt.anchorMax = AnchorPoint(anchor);
            rt.pivot = pivot ?? new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }
        rt.localEulerAngles = new Vector3(0f, 0f, rotation);
        return rt;
    }

    // Sprite image (sliced when the sprite has 9-slice borders). Decorations don't catch clicks.
    public static Image Image(string name, Transform parent, string sprite, string anchor = "center", Vector2 pos = default,
        Vector2 size = default, float rotation = 0f, Color? color = null, Vector2? pivot = null, bool shadow = false)
    {
        var image = Rect(name, parent, anchor, pos, size, rotation, pivot).gameObject.AddComponent<Image>();
        image.sprite = Sprite(sprite);
        image.type = image.sprite != null && image.sprite.border != Vector4.zero ? UnityEngine.UI.Image.Type.Sliced : UnityEngine.UI.Image.Type.Simple;
        image.color = color ?? Color.white;
        image.raycastTarget = false;
        if (shadow) AddShadow(image);
        return image;
    }

    public static Text Label(string name, Transform parent, string text, bool title, int fontSize, string anchor = "center",
        Vector2 pos = default, Vector2 size = default, Color? color = null, TextAnchor align = TextAnchor.MiddleCenter,
        Vector2? pivot = null)
    {
        var t = Rect(name, parent, anchor, pos, size, 0f, pivot).gameObject.AddComponent<Text>();
        t.font = title ? TitleFont : BodyFont;
        t.fontSize = fontSize;
        t.alignment = align;
        t.color = color ?? Ink;
        t.text = text;
        t.supportRichText = true;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    // Paper button: style is "primary" (gold), "secondary" (cream) or "danger" (red); hover/pressed/disabled
    // swap to the kit's state sprites and scale a little. It deselects itself after a click so Space doesn't re-press it.
    public static Button Button(string name, Transform parent, string text, string style, int fontSize, UnityAction onClick,
        string anchor = "center", Vector2 pos = default, Vector2 size = default, float rotation = 0f)
    {
        var rt = Rect(name, parent, anchor, pos, size, rotation);
        var image = rt.gameObject.AddComponent<Image>();
        image.type = UnityEngine.UI.Image.Type.Sliced;
        AddShadow(image);
        var button = rt.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.SpriteSwap;
        SetButtonStyle(button, style);
        button.onClick.AddListener(onClick);
        button.onClick.AddListener(() =>
        {
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        });
        rt.gameObject.AddComponent<UiHover>();

        var label = Label("Text", rt, text, true, fontSize, "stretch");
        label.rectTransform.offsetMin = new Vector2(0f, 6f);   // the sprite's raised top sits above its darker base
        return button;
    }

    public static void SetButtonStyle(Button button, string style)
    {
        string id = "button_" + style;
        ((Image)button.targetGraphic).sprite = Sprite(id);
        button.spriteState = new SpriteState
        {
            highlightedSprite = Sprite(id + "_hover"),
            pressedSprite = Sprite(id + "_pressed"),
            selectedSprite = Sprite(id),
            disabledSprite = Sprite(id + "_disabled"),
        };
    }

    public static void AddShadow(Graphic graphic)
    {
        var shadow = graphic.gameObject.AddComponent<Shadow>();
        shadow.effectColor = ShadowColor;
        shadow.effectDistance = ShadowOffset;
    }

    // Small icon (coin, lock, ...) next to a value.
    public static Image Icon(string name, Transform parent, string sprite, string anchor, Vector2 pos, float size) =>
        Image(name, parent, sprite, anchor, pos, new Vector2(size, size));
}
