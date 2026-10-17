using UnityEngine;

public enum CosmeticCategory { Color, Shape, Effect }

// One shop item. Only the fields of its own category are used.
public class CosmeticItem
{
    public string id;
    public string name;
    public string description;
    public CosmeticCategory category;
    public int price;              // 0 = owned from the start

    public Color color;            // Color: paper tint
    public string shapeId;         // Shape: Resources/Planes/<shapeId>.json, empty = classic model
    public Gradient trailColors;   // Effect: null keeps the classic white streak
    public float trailWidth;
    public float trailTime;
}

// Everything the shop sells. Adding a cosmetic = adding a line here (plus its JSON model, for a new shape).
public static class CosmeticCatalog
{
    public static readonly CosmeticItem[] Items =
    {
        Paint("color_classic", "Klasik", 0, new Color(0.776f, 0.723f, 0.631f)),
        Paint("color_sky", "Gök Mavisi", 150, new Color(0.45f, 0.72f, 1f)),
        Paint("color_mint", "Nane Yeşili", 150, new Color(0.5f, 0.92f, 0.68f)),
        Paint("color_rose", "Gül Pembe", 200, new Color(1f, 0.55f, 0.75f)),
        Paint("color_night", "Gece Siyahı", 250, new Color(0.16f, 0.17f, 0.22f)),
        Paint("color_gold", "Altın Sarısı", 300, new Color(1f, 0.8f, 0.25f)),

        Form("shape_classic", "Klasik", "Bildiğin kâğıt uçak", 0, ""),
        Form("shape_dart", "Ok", "Dar ve sivri", 400, "dart"),
        Form("shape_swallow", "Kırlangıç", "Çatallı kuyruk", 600, "swallow"),
        Form("shape_glider", "Planör", "Geniş kanatlar", 800, "glider"),
        Form("shape_jet", "Avcı", "Delta kanat, çift kuyruk", 1000, "jet"),
        Form("shape_manta", "Vatoz", "Kısa ve çok geniş", 1200, "manta"),

        Trail("effect_classic", "Klasik", "İnce rüzgar izi", 0, null, 0f, 0f),   // WindTrail's own settings
        Trail("effect_rainbow", "Gökkuşağı İzi", "Renk renk uzun iz", 450, Rainbow(), 0.07f, 0.8f),
        Trail("effect_gold", "Altın İz", "Parlayan altın toz", 600, GoldGlow(), 0.09f, 0.6f),
    };

    // The free item of each category, used until something else is equipped.
    public static CosmeticItem Default(CosmeticCategory category)
    {
        foreach (var item in Items)
            if (item.category == category && item.price == 0) return item;
        return null;
    }

    public static CosmeticItem Find(string id)
    {
        foreach (var item in Items)
            if (item.id == id) return item;
        return null;
    }

    static CosmeticItem Paint(string id, string name, int price, Color color) =>
        new CosmeticItem { id = id, name = name, category = CosmeticCategory.Color, price = price, color = color };

    static CosmeticItem Form(string id, string name, string description, int price, string shapeId) =>
        new CosmeticItem { id = id, name = name, description = description, category = CosmeticCategory.Shape, price = price, shapeId = shapeId };

    static CosmeticItem Trail(string id, string name, string description, int price, Gradient colors, float width, float time) =>
        new CosmeticItem
        {
            id = id, name = name, description = description, category = CosmeticCategory.Effect, price = price,
            trailColors = colors, trailWidth = width, trailTime = time
        };

    static Gradient Rainbow()
    {
        var g = new Gradient();
        g.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(1f, 0.25f, 0.25f), 0f),
                new GradientColorKey(new Color(1f, 0.85f, 0.2f), 0.25f),
                new GradientColorKey(new Color(0.3f, 1f, 0.4f), 0.5f),
                new GradientColorKey(new Color(0.3f, 0.6f, 1f), 0.75f),
                new GradientColorKey(new Color(0.75f, 0.35f, 1f), 1f),
            },
            new[] { new GradientAlphaKey(0.75f, 0f), new GradientAlphaKey(0f, 1f) });
        return g;
    }

    static Gradient GoldGlow()
    {
        var g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(new Color(1f, 0.95f, 0.6f), 0f), new GradientColorKey(new Color(1f, 0.65f, 0.1f), 1f) },
            new[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0f, 1f) });
        return g;
    }
}
