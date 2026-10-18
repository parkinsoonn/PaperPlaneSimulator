using UnityEngine;
using UnityEngine.UI;

// Shop opened from the intro screen: color / shape / effect tabs, a paper card per cosmetic with
// buy or equip, and the player's gold. Built by LaunchPowerHUD on its canvas; layout from Docs/hud/shop_menu.json.
public class ShopMenu : MonoBehaviour
{
    static readonly string[] TabNames = { "RENK", "ŞEKİL", "EFEKT" };
    static readonly float[] CardTilt = { 1f, -1f, 0.5f, -0.5f, 1f, -1f };   // the mockup's CSS tilts, in Unity's rotation direction
    static readonly Color EquippedFill = HudKit.Hex("#BFE3B5");
    static readonly Color EquippedText = HudKit.Hex("#1E5A2B");

    CanvasGroup group;
    RectTransform titleStrip, backButton, goldPanel, board;
    Text goldLabel;
    int shownGold = -1;
    RectTransform cards;
    readonly Button[] tabs = new Button[3];
    CosmeticCategory tab = CosmeticCategory.Color;

    public void Build(Transform canvas)
    {
        var screen = HudKit.Image("ShopMenu", canvas, "bg_sunset", "stretch");
        screen.raycastTarget = true;
        group = screen.gameObject.AddComponent<CanvasGroup>();
        var root = screen.transform;

        titleStrip = HudKit.Image("TitleStrip", root, "strip_kraft", "top", new Vector2(0f, -72f), new Vector2(520f, 110f), -1.5f, shadow: true).rectTransform;
        HudKit.Label("Title", titleStrip, "MAĞAZA", true, 72, "center", new Vector2(0f, 2f), new Vector2(460f, 90f));

        var back = HudKit.Button("BackButton", root, "GERİ", "secondary", 46, Close, "top-left", new Vector2(160f, -72f), new Vector2(240f, 84f), 1.5f);
        backButton = (RectTransform)back.transform;
        back.GetComponentInChildren<Text>().rectTransform.anchoredPosition += new Vector2(24f, 0f);
        HudKit.Icon("BackIcon", backButton, "icon_back", "left", new Vector2(48f, 2f), 40f);

        var gold = HudKit.Image("GoldPanel", root, "panel_label", "top-right", new Vector2(-240f, -72f), new Vector2(400f, 76f), -1.5f, shadow: true);
        goldPanel = gold.rectTransform;
        HudKit.Icon("GoldIcon", gold.transform, "icon_coin", "left", new Vector2(44f, 0f), 48f);
        goldLabel = HudKit.Label("Gold", gold.transform, "", false, 34, "right", new Vector2(-28f, 0f), new Vector2(290f, 56f),
            align: TextAnchor.MiddleRight, pivot: new Vector2(1f, 0.5f));
        goldLabel.font = HudKit.CounterFont;

        var boardImage = HudKit.Image("ShopBoard", root, "panel_board", "top", new Vector2(0f, -632f), new Vector2(1760f, 800f));
        board = boardImage.rectTransform;
        board.gameObject.AddComponent<CanvasGroup>();

        // Tabs sit on the board's top edge, so they are drawn after it.
        for (int i = 0; i < TabNames.Length; i++)
        {
            var category = (CosmeticCategory)i;
            var button = HudKit.Button("Tab" + TabNames[i], root, TabNames[i], "secondary", 44, () => SelectTab(category),
                "top", new Vector2((i - 1) * 280f, -197f), new Vector2(280f, 86f));
            button.transition = Selectable.Transition.None;
            Destroy(button.GetComponent<Shadow>());
            var hover = button.GetComponent<UiHover>();
            hover.hoverScale = 1.04f;
            hover.pressedScale = 0.97f;
            tabs[i] = button;
        }

        cards = HudKit.Rect("Cards", board, "top-left", new Vector2(40f, -30f), new Vector2(1680f, 724f), pivot: new Vector2(0f, 1f));
        var grid = cards.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(540f, 350f);
        grid.spacing = new Vector2(30f, 24f);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;

        SetVisible(false);
    }

    void OnEnable() => PlayerWallet.Changed += OnWalletChanged;
    void OnDisable() => PlayerWallet.Changed -= OnWalletChanged;

    public void Open()
    {
        shownGold = PlayerWallet.Gold;
        goldLabel.text = $"ALTIN: {shownGold}";
        Refresh(true);
        SetVisible(true);
        foreach (Object target in new Object[] { group, titleStrip, backButton, goldPanel, board })
            UiAnim.Kill(target);
        UiAnim.Fade(group, 0f, 1f, 0.25f);
        UiAnim.SlideIn(titleStrip, new Vector2(0f, 120f), 0.3f, 0f, Ease.OutBack);
        UiAnim.SlideIn(backButton, new Vector2(-200f, 0f), 0.25f, 0.1f);
        UiAnim.SlideIn(goldPanel, new Vector2(200f, 0f), 0.3f, 0.1f);
        UiAnim.SlideIn(board, new Vector2(0f, -80f), 0.3f);
        UiAnim.Fade(board.GetComponent<CanvasGroup>(), 0f, 1f, 0.3f);
    }

    public void Close() => SetVisible(false);

    void SetVisible(bool visible)
    {
        group.alpha = visible ? 1f : 0f;
        group.interactable = group.blocksRaycasts = visible;
    }

    void SelectTab(CosmeticCategory category)
    {
        tab = category;
        Refresh(true);
        UiAnim.ScaleIn(tabs[(int)category].transform, 0.95f, 0.12f);
    }

    // A purchase or equip: redraw the cards in place and count the gold down to the new amount.
    void OnWalletChanged()
    {
        if (group == null) return;
        int gold = PlayerWallet.Gold;
        if (gold != shownGold && shownGold >= 0)
        {
            UiAnim.Count(goldLabel, shownGold, gold, 0.4f, v => $"ALTIN: {v}");
            UiAnim.Punch(goldPanel, 1.12f, 0.4f);
        }
        else goldLabel.text = $"ALTIN: {gold}";
        shownGold = gold;
        Refresh(false);
    }

    // Redraws the selected tab and its cards; animate = cards pop in one after another (open, tab switch).
    void Refresh(bool animate)
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            bool selected = (CosmeticCategory)i == tab;
            var rt = (RectTransform)tabs[i].transform;
            ((Image)tabs[i].targetGraphic).sprite = HudKit.Sprite(selected ? "tab_on" : "tab_off");
            rt.sizeDelta = new Vector2(280f, selected ? 86f : 72f);
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, selected ? -197f : -204f);
            // The tab sprites leave their top transparent (more on the closed tab); center the label on the paper.
            var label = tabs[i].GetComponentInChildren<Text>().rectTransform;
            label.offsetMin = Vector2.zero;
            label.offsetMax = new Vector2(0f, selected ? -3f : -15f);
        }

        foreach (Transform child in cards)
        {
            child.gameObject.SetActive(false);   // out of the grid layout right away
            Destroy(child.gameObject);
        }
        int index = 0;
        foreach (var item in CosmeticCatalog.Items)
        {
            if (item.category != tab) continue;
            if (item.category == CosmeticCategory.Shape && !PlaneCosmetics.HasShape(item.shapeId)) continue;   // model not delivered yet
            var card = CreateCard(item, CardTilt[index % CardTilt.Length]);
            if (animate) UiAnim.ScaleIn(card, 0.9f, 0.2f, 0.15f + index * 0.04f);
            index++;
        }
    }

    Transform CreateCard(CosmeticItem item, float tilt)
    {
        var cardImage = HudKit.Image("Card_" + item.id, cards, "panel_large");
        cardImage.raycastTarget = true;
        var card = cardImage.rectTransform;
        card.localEulerAngles = new Vector3(0f, 0f, tilt);
        var hover = card.gameObject.AddComponent<UiHover>();
        hover.hoverScale = 1.03f;
        hover.pressedScale = 1.03f;

        HudKit.Image("CardTape", card, "tape_2", "top", new Vector2(0f, -8f), new Vector2(130f, 44f), -2f);
        HudKit.Label("Name", card, item.name, true, 46, "top", new Vector2(0f, -48f), new Vector2(480f, 56f));
        if (item.category == CosmeticCategory.Color)
        {
            var swatch = HudKit.Image("Swatch", card, "swatch_blot", "top", new Vector2(0f, -161f), new Vector2(220f, 150f), 3f, item.color);
            HudKit.Image("SwatchOutline", swatch.transform, "swatch_outline", "stretch");   // keeps dark colors visible on the paper
        }
        else
        {
            HudKit.Image("Preview", card, "preview_" + item.id, "top", new Vector2(0f, -142f), new Vector2(160f, 120f));
            HudKit.Label("Description", card, item.description, false, 26, "top", new Vector2(0f, -225f), new Vector2(480f, 34f));
        }

        bool owned = PlayerWallet.IsOwned(item);
        bool equipped = PlayerWallet.Equipped(item.category) == item;
        bool affordable = PlayerWallet.CanAfford(item);
        var button = HudKit.Button("ActionButton", card, "", owned ? "secondary" : "primary", 40, () =>
        {
            if (PlayerWallet.IsOwned(item)) PlayerWallet.Equip(item);
            else PlayerWallet.TryBuy(item);
        }, "bottom", new Vector2(0f, 48f), new Vector2(460f, 80f));
        var text = button.GetComponentInChildren<Text>();
        Vector2 textShift = Vector2.zero;
        string icon = null;
        Vector2 iconPos = new Vector2(150f, 0f);

        if (equipped)
        {
            text.text = "KULLANILIYOR";
            text.color = EquippedText;
            ((Image)button.targetGraphic).color = EquippedFill;
            button.GetComponent<UiHover>().hoverScale = 1f;
            textShift = new Vector2(22f, 0f);
            icon = "icon_check";
            iconPos = new Vector2(-160f, 0f);
        }
        else if (owned)
            text.text = "KULLAN";
        else if (affordable)
        {
            text.text = $"SATIN AL · {item.price}";
            textShift = new Vector2(-26f, 0f);
            icon = "icon_coin";
        }
        else
        {
            text.text = $"SATIN AL · <color=#B1180B>{item.price}</color>";
            textShift = new Vector2(-26f, 0f);
            icon = "icon_lock";
            button.interactable = false;   // swaps to the disabled sprite
            button.gameObject.AddComponent<CanvasGroup>().alpha = 0.85f;
        }
        text.rectTransform.anchoredPosition += textShift;
        if (icon != null) HudKit.Icon("ActionIcon", button.transform, icon, "center", iconPos, 44f);
        return card;
    }
}
