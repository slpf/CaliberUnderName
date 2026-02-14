using System.Collections.Generic;
using BepInEx.Configuration;
using Comfort.Common;
using EFT.InventoryLogic;
using UnityEngine;

namespace CaliberUnderName;

public static class Settings
{
    private static readonly Dictionary<string, string> DefaultCaliberMap = new()
    {
        { "Caliber1036x77", ".408" },
        { "Caliber1143x23ACP", ".45" },
        { "Caliber11x33R", ".44" },
        { "Caliber127x108", "12.7" },
        { "Caliber127x33", ".50AE" },
        { "Caliber127x55", "PS12" },
        { "Caliber127x99", ".50" },
        { "Caliber12g", "12ga" },
        { "Caliber20g", "20ga" },
        { "Caliber20x1mm", "20x1" },
        { "Caliber23x75", "KS23" },
        { "Caliber25x59", "" },
        { "Caliber26x75", "" },
        { "Caliber30x29", "" },
        { "Caliber366TKM", ".366" },
        { "Caliber40mmRU", "" },
        { "Caliber40x46", "" },
        { "Caliber46x30", "4.6" },
        { "Caliber545x39", "5.45" },
        { "Caliber556x45NATO", "5.56" },
        { "Caliber57x28", "5.7" },
        { "Caliber68x51", "6.8" },
        { "Caliber725", "" }, // 72.5mm rocket launcher
        { "Caliber762x25TT", "TT" },
        { "Caliber762x35", ".300" },
        { "Caliber762x39", "7.62" },
        { "Caliber762x51", ".308" },
        { "Caliber762x54R", "7.62R" },
        { "Caliber762x67B", ".300WM" },
        { "Caliber784x49", ".308ME" },
        { "Caliber792x57", "8mm" },
        { "Caliber86x63", "8.6x63" },
        { "Caliber86x70", ".338" },
        { "Caliber93x64", "9.3" },
        { "Caliber9x18PM", "9x18" },
        { "Caliber9x19PARA", "9mm" },
        { "Caliber9x21", "9x21" },
        { "Caliber9x33R", ".357" },
        { "Caliber9x39", "9x39" },
    };
    
    public static ConfigEntry<bool> StripValueMarks;
    public static ConfigEntry<string> ValueMarksToStrip;
    public static ConfigEntry<Color> CaliberColor;
    public static ConfigEntry<bool> SwapName;
    public static ConfigEntry<KeyboardShortcut> ShowNamesKeyBind;
    public static ConfigEntry<bool> EnableShowNamesInTrading;
    public static ConfigEntry<bool> EnableCaliberSort;
    public static ConfigEntry<AmmoSortMode> PrimarySort;
    public static ConfigEntry<AmmoSortMode> SecondarySort;
    public static ConfigEntry<AmmoSortDirection> PrimarySortDirection;
    public static ConfigEntry<AmmoSortDirection> SecondarySortDirection;
    
    private static ConfigFile _config;
    
    private static readonly Dictionary<string, ConfigEntry<string>> CaliberConfigs = new();
    
    private static bool _checked;

    public static void Init(ConfigFile config)
    {
        _config = config;

        StripValueMarks = _config.Bind(
            "1. General", "Remove value marks", false,
            "Remove value marks (★☆●○) from ammo short names");

        ValueMarksToStrip = _config.Bind(
            "1. General", "Value marks", "★☆●○",
            "Characters to strip from the beginning of ammo names");

        CaliberColor = _config.Bind(
            "1. General", "Caliber color", new Color(0.63f, 0.63f, 0.63f, 1f),
            "Color for the caliber text");

        SwapName = _config.Bind(
            "1. General", "Swap name", false, "Swap name with caliber");

        EnableShowNamesInTrading = _config.Bind("2. Item Names In Trading", "Enable Names In Trading", true,
            "Hold a key to show item names instead of prices at traders");

        ShowNamesKeyBind = _config.Bind(
            "2. Item Names In Trading", "Show names keybind", new KeyboardShortcut(KeyCode.LeftAlt),
            "Keybind to show item names");
        
        EnableCaliberSort = _config.Bind(
            "3. Ammo Sorting", "Enable ammo sorting", true,
            "Alternative comparator for ammo sorting");

        PrimarySort = _config.Bind(
            "3. Ammo Sorting", "Primary Sort", AmmoSortMode.Caliber);

        PrimarySortDirection = _config.Bind(
            "3. Ammo Sorting", "Primary sort direction", AmmoSortDirection.Ascending);
        
        SecondarySort = _config.Bind(
            "3. Ammo Sorting", "Secondary Sort", AmmoSortMode.Count);
        
        SecondarySortDirection = _config.Bind(
            "3. Ammo Sorting", "Secondary sort direction", AmmoSortDirection.Descending);
    }

    public static void InitCalibers()
    {
        if (_checked) return;
        
        _checked = true;

        var factory = Singleton<ItemFactoryClass>.Instance;
        
        if (factory == null) return;
        
        foreach (var kvp in factory.ItemTemplates)
        {
            if (kvp.Value is AmmoTemplate ammo && !CaliberConfigs.ContainsKey(ammo.Caliber))
            {
                var caliberKey = ammo.Caliber;
                var caliberName = DefaultCaliberMap.GetValueOrDefault(caliberKey, "");
                
                CaliberConfigs[caliberKey] = _config.Bind(
                    "4. Caliber Names", caliberKey, caliberName,
                    $"Display name for {caliberKey}");
            }
        }
    }
    
    public static string GetCaliber(string caliber)
    {
        return CaliberConfigs.TryGetValue(caliber, out var config) ? config.Value : "";
    }
}

public enum AmmoSortMode
{
    Caliber,
    Alphabetical,
    Count,
    Rarity,
    Damage,
    Penetration
}

public enum AmmoSortDirection
{
    Ascending,
    Descending
}