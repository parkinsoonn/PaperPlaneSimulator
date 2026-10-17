using UnityEngine;
using UnityEngine.UI;

// Shop opened from the intro screen: color / shape / effect tabs, a card per cosmetic with
// buy or equip, and the player's gold. Built by LaunchPowerHUD on its canvas with the same UI helpers.
public class ShopMenu : MonoBehaviour
{
    static readonly Color Gold = new Color(1f, 0.78f, 0.2f);
    static readonly Color Light = new Color(0.95f, 0.93f, 0.88f);
    static readonly Vector2 CardSize = new Vector2(460f, 250f);

    CanvasGroup group;
    Text goldLabel;
    RectTransform cards;
    readonly Image[] tabImages = new Image[3];
    CosmeticCategory tab = CosmeticCategory.Color;

    public void Build(Transform canvas)
    {
        var panel = LaunchPowerHUD.CreateRect("ShopMenu", canvas);
        panel.anchorMin = Vector2.zero;
        panel.anchorMax = Vector2.one;
        panel.offsetMin = panel.offsetMax = Vector2.zero;
        panel.gameObject.AddComponent<Image>().color = new Color(0.08f, 0.08f, 0.14f);
        group = panel.gameObject.AddComponent<CanvasGroup>();

        LaunchPowerHUD.CreateText("Title", panel, "MAĞAZA", 80, new Vector2(0f, 430f), new Vector2(800f, 110f)).color = Gold;
        goldLabel = LaunchPowerHUD.CreateText("Gold", panel, "", 40, new Vector2(620f, 430f), new Vector2(500f, 60f));
        goldLabel.alignment = TextAnchor.MiddleRight;
        goldLabel.color = Gold;

        string[] tabNames = { "RENK", "ŞEKİL", "EFEKT" };
        for (int i = 0; i < tabNames.Length; i++)
        {
            var category = (CosmeticCategory)i;
            var button = LaunchPowerHUD.CreateButton("Tab" + tabNames[i], panel, tabNames[i], new Vector2((i - 1) * 380f, 310f),
                () => { tab = category; Refresh(); });
            tabImages[i] = (Image)button.targetGraphic;
        }

        cards = LaunchPowerHUD.CreateRect("Cards", panel);
        LaunchPowerHUD.CreateButton("BackButton", panel, "GERİ", new Vector2(0f, -430f), Close);

        SetVisible(false);
    }

    void OnEnable() => PlayerWallet.Changed += Refresh;
    void OnDisable() => PlayerWallet.Changed -= Refresh;

    public void Open()
    {
        Refresh();
        SetVisible(true);
    }

    public void Close() => SetVisible(false);

    void SetVisible(bool visible)
    {
        group.alpha = visible ? 1f : 0f;
        group.interactable = group.blocksRaycasts = visible;
    }

    // Redraws the gold, the selected tab and its cards (after a tab switch, purchase or equip).
    void Refresh()
    {
        if (group == null) return;
        goldLabel.text = $"ALTIN: {PlayerWallet.Gold}";
        for (int i = 0; i < tabImages.Length; i++)
            tabImages[i].color = (CosmeticCategory)i == tab ? Gold : Light;

        foreach (Transform child in cards) Destroy(child.gameObject);
        int index = 0;
        foreach (var item in CosmeticCatalog.Items)
        {
            if (item.category != tab) continue;
            if (item.category == CosmeticCategory.Shape && !PlaneCosmetics.HasShape(item.shapeId)) continue;   // model not delivered yet
            // Three columns, rows going down.
            var position = new Vector2((index % 3 - 1) * 500f, 110f - index / 3 * 290f);
            CreateCard(item, position);
            index++;
        }
    }

    void CreateCard(CosmeticItem item, Vector2 position)
    {
        var card = LaunchPowerHUD.CreateRect("Card_" + item.id, cards);
        card.anchoredPosition = position;
        card.sizeDelta = CardSize;
        card.gameObject.AddComponent<Image>().color = new Color(0.16f, 0.16f, 0.24f);

        LaunchPowerHUD.CreateText("Name", card, item.name, 36, new Vector2(0f, 80f), new Vector2(440f, 50f));
        if (item.category == CosmeticCategory.Color)
        {
            var swatch = LaunchPowerHUD.CreateRect("Swatch", card);
            swatch.anchoredPosition = new Vector2(0f, 5f);
            swatch.sizeDelta = new Vector2(140f, 70f);
            swatch.gameObject.AddComponent<Image>().color = item.color;
            var border = swatch.gameObject.AddComponent<Outline>();   // keeps dark colors visible on the card
            border.effectColor = new Color(1f, 1f, 1f, 0.6f);
            border.effectDistance = new Vector2(3f, -3f);
        }
        else
            LaunchPowerHUD.CreateText("Description", card, item.description, 26, new Vector2(0f, 5f), new Vector2(440f, 70f))
                .color = new Color(0.8f, 0.8f, 0.85f);

        bool owned = PlayerWallet.IsOwned(item);
        bool equipped = PlayerWallet.Equipped(item.category) == item;
        string label = equipped ? "KULLANILIYOR" : owned ? "KULLAN" : $"SATIN AL · {item.price}";
        var button = LaunchPowerHUD.CreateButton("ActionButton", card, label, new Vector2(0f, -80f), () =>
        {
            if (PlayerWallet.IsOwned(item)) PlayerWallet.Equip(item);
            else PlayerWallet.TryBuy(item);
        }, equipped ? new Color(0.45f, 0.8f, 0.45f) : owned ? Light : Gold);
        ((RectTransform)button.transform).sizeDelta = new Vector2(380f, 70f);
        button.interactable = owned || PlayerWallet.CanAfford(item);
        if (!owned && !PlayerWallet.CanAfford(item))
            button.GetComponentInChildren<Text>().color = new Color(0.75f, 0.2f, 0.2f);
    }
}
