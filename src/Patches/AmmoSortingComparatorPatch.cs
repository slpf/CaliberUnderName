using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace CaliberUnderName.Patches;

public class AmmoSortingComparatorPatch : ModulePatch
{
    private static Dictionary<string, int> _rarityOrder;
    private static bool _rarityDirty = true;

    public static void MarkRarityDirty() => _rarityDirty = true;
    
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(AccessTools.TypeByName("GClass3381+Class2438"), "Compare");
    }
    
    [PatchPrefix]
    public static bool Prefix(Item x, Item y, ref int __result)
    {
        if (!Settings.EnableCaliberSort.Value) return true;

        var ammoX = GetAmmoTemplate(x);
        var ammoY = GetAmmoTemplate(y);
        
        if (ammoX == null || ammoY == null) return true;

        int num = ApplyDirection(CompareBy(Settings.PrimarySort.Value, x, y, ammoX, ammoY), Settings.PrimarySortDirection.Value);
        if (num == 0) num = ApplyDirection(CompareBy(Settings.SecondarySort.Value, x, y, ammoX, ammoY), Settings.SecondarySortDirection.Value);
        if (num == 0) num = string.Compare(x.ShortName.Localized(), y.ShortName.Localized(), StringComparison.OrdinalIgnoreCase);

        __result = num;
        return __result == 0;
    }
    
    private static AmmoTemplate GetAmmoTemplate(Item item)
    {
        if (item is AmmoItemClass ammo)
            return ammo.AmmoTemplate;

        if (item is AmmoBox box)
        {
            var first = box.Cartridges?.Items?.FirstOrDefault() as AmmoItemClass;
            return first?.AmmoTemplate;
        }

        return null;
    }
    
    private static int GetAmmoCount(Item item)
    {
        if (item is AmmoItemClass)
            return item.StackObjectsCount;

        if (item is AmmoBox box)
            return box.Cartridges?.Items?.Sum(i => i.StackObjectsCount) ?? 0;

        return 0;
    }
    
    private static int ApplyDirection(int result, AmmoSortDirection direction)
    {
        return direction == AmmoSortDirection.Descending ? -result : result;
    }
    
    private static int CompareBy(AmmoSortMode mode, Item x, Item y, AmmoTemplate ammoX, AmmoTemplate ammoY)
    {
        return mode switch
        {
            AmmoSortMode.Caliber => string.Compare(
                ammoX.Caliber, ammoY.Caliber, StringComparison.OrdinalIgnoreCase),
            AmmoSortMode.Alphabetical => string.Compare(
                x.ShortName.Localized(), y.ShortName.Localized(), StringComparison.OrdinalIgnoreCase),
            AmmoSortMode.Count => GetAmmoCount(x).CompareTo(GetAmmoCount(y)),
            AmmoSortMode.Rarity => GetRarityOrder(x).CompareTo(GetRarityOrder(y)),
            AmmoSortMode.Damage => ammoX.Damage.CompareTo(ammoY.Damage),
            AmmoSortMode.Penetration => ammoX.PenetrationPower.CompareTo(ammoY.PenetrationPower),
            _ => 0
        };
    }
    
    private static int GetRarityOrder(Item item)
    {
        return GetRarityOrder().GetValueOrDefault(item.BackgroundColor.ToString(), 10);
    }
    
    private static Dictionary<string, int> GetRarityOrder()
    {
        if (!_rarityDirty && _rarityOrder != null) return _rarityOrder;

        _rarityOrder = new Dictionary<string, int>(10);
        for (int i = 0; i < 10; i++) _rarityOrder[ColorToKey(Settings.RarityColors[i].Value)] = i;

        _rarityDirty = false;
        return _rarityOrder;
    }
    
    private static string ColorToKey(Color c)
    {
        int r = Mathf.RoundToInt(c.r * 255f);
        int g = Mathf.RoundToInt(c.g * 255f);
        int b = Mathf.RoundToInt(c.b * 255f);
        return (r * 65536 + g * 256 + b + 12).ToString();
    }
}