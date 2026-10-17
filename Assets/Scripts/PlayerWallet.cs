using System;
using UnityEngine;

// Gold, bought cosmetics and the equipped one per category; saved in PlayerPrefs like GameSettings.
public static class PlayerWallet
{
    const string GoldKey = "gold";

    // Raised after gold changes or something is bought/equipped.
    public static event Action Changed;

    public static int Gold => PlayerPrefs.GetInt(GoldKey, 0);

    public static void AddGold(int amount)
    {
        if (amount <= 0) return;
        PlayerPrefs.SetInt(GoldKey, Gold + amount);
        PlayerPrefs.Save();
        Changed?.Invoke();
    }

    public static bool IsOwned(CosmeticItem item) => item.price == 0 || PlayerPrefs.GetInt("owned_" + item.id, 0) == 1;

    public static bool CanAfford(CosmeticItem item) => Gold >= item.price;

    // Pays for the item and equips it. False if it's already owned or there isn't enough gold.
    public static bool TryBuy(CosmeticItem item)
    {
        if (IsOwned(item) || !CanAfford(item)) return false;
        PlayerPrefs.SetInt(GoldKey, Gold - item.price);
        PlayerPrefs.SetInt("owned_" + item.id, 1);
        Equip(item);
        return true;
    }

    public static CosmeticItem Equipped(CosmeticCategory category)
    {
        var item = CosmeticCatalog.Find(PlayerPrefs.GetString("equipped_" + category, ""));
        return item != null && item.category == category && IsOwned(item) ? item : CosmeticCatalog.Default(category);
    }

    public static void Equip(CosmeticItem item)
    {
        if (!IsOwned(item)) return;
        PlayerPrefs.SetString("equipped_" + item.category, item.id);
        PlayerPrefs.Save();
        Changed?.Invoke();
    }
}
