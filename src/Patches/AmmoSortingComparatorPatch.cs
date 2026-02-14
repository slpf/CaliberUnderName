using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CaliberUnderName.Patches;

public class AmmoSortingComparatorPatch : ModulePatch
{
    private static readonly Dictionary<string, int> RarityOrder = new()
    {
        { "1090630", 9 },
        { "13268011", 8 },
        { "13246263", 7 },
        { "16728140", 6 },
        { "3800864", 5 },
        { "16766732", 4 },
        { "16183657", 3 },
        { "10443483", 2 },
        { "2528486", 1 },
        { "16777227", 0 },
    };
    
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
        return RarityOrder.GetValueOrDefault(item.BackgroundColor.ToString(), 10);
    }
}