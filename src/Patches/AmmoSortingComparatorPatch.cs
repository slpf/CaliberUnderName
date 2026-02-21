using System;
using System.Collections.Generic;
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
        if (!Settings.EnableAmmoSort.Value) return true;

        var ammoX = GetAmmoTemplate(x);
        var ammoY = GetAmmoTemplate(y);
        
        if (ammoX == null || ammoY == null) return true;
        
        var primarySort = Settings.PrimarySort.Value;
        var primaryDir = Settings.PrimarySortDirection.Value;
        var secondarySort = Settings.SecondarySort.Value;
        var secondaryDir = Settings.SecondarySortDirection.Value;

        var num = ApplyDirection(CompareBy(primarySort, x, y, ammoX, ammoY), primaryDir);
        if (num == 0) num = ApplyDirection(CompareBy(secondarySort, x, y, ammoX, ammoY), secondaryDir);
        if (num == 0) num = string.Compare(x.ShortName.Localized(), y.ShortName.Localized(), StringComparison.OrdinalIgnoreCase);

        __result = num;
        return __result == 0;
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
    
    private static int ApplyDirection(int result, AmmoSortDirection direction)
    {
        return direction == AmmoSortDirection.Descending ? -result : result;
    }
    
    private static AmmoTemplate GetAmmoTemplate(Item item)
    {
        if (item is AmmoItemClass ammo) return ammo.AmmoTemplate;

        if (item is AmmoBox box)
        {
            var cartridges = box.Cartridges?.Items;
            if (cartridges == null) return null;

            foreach (var cartridge in cartridges)
            {
                if (cartridge is AmmoItemClass a) return a.AmmoTemplate;
            }
        }

        return null;
    }
    
    private static int GetAmmoCount(Item item)
    {
        if (item is AmmoItemClass) return item.StackObjectsCount;

        if (item is AmmoBox box)
        {
            var cartridges = box.Cartridges?.Items;
            if (cartridges == null) return 0;

            var total = 0;
            foreach (var cartridge in cartridges) total += cartridge.StackObjectsCount;
            return total;
        }

        return 0;
    }
    
    private static int GetRarityOrder(Item item)
    {
        EnsureRarityOrder();
        return _rarityOrder.GetValueOrDefault(item.BackgroundColor.ToString(), 10);
    }
    
    private static void EnsureRarityOrder()
    {
        if (!_rarityDirty && _rarityOrder != null) return;

        _rarityOrder ??= new Dictionary<string, int>(10);
        _rarityOrder.Clear();
        
        for (var i = 0; i < 10; i++) _rarityOrder[ColorToKey(Settings.RarityColors[i].Value)] = i;

        _rarityDirty = false;
    }
    
    private static string ColorToKey(Color c)
    {
        var r = Mathf.RoundToInt(c.r * 255f);
        var g = Mathf.RoundToInt(c.g * 255f);
        var b = Mathf.RoundToInt(c.b * 255f);
        return (r * 65536 + g * 256 + b + 12).ToString();
    }
}