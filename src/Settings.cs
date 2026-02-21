using System.Collections.Generic;
using BepInEx.Configuration;
using CaliberUnderName.Patches;
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

    private static readonly string[] DefaultRarityHex =
    {
        "#FFFFFF", // 0
        "#2694da", // 1
        "#9F5ACF", // 2
        "#f6f15d", // 3
        "#FFD700", // 4
        "#39FF14", // 5
        "#FF4040", // 6
        "#ca1f2b", // 7
        "#ca741f", // 8
        "#10a43a", // 9
    };

    private const string CategoryToggles = "0. Toggles";
    private const string CategoryGeneral = "1. General";
    private const string CategoryInTrading = "2. Item Names In Trading";
    private const string CategoryAmmoSorting = "3. Ammo Sorting";
    private const string CategoryRarityColor = "3. Ammo Sorting - Rarity Colors";
    private const string CategoryCaliberNames = "4. Caliber Names";
    
    public static ConfigEntry<bool> EnableAmmo;
    public static ConfigEntry<bool> EnableAmmoBoxes;
    public static ConfigEntry<bool> EnableMagazines;
    public static ConfigEntry<bool> EnableWeapons;
    public static ConfigEntry<bool> EnableCaliberInSense;
    public static ConfigEntry<bool> EnableCaliberInLoot;
    public static ConfigEntry<bool> EnableShowNamesInTrading;
    public static ConfigEntry<bool> EnableAmmoSort;
    
    public static ConfigEntry<bool> StripValueMarks;
    public static ConfigEntry<bool> StripValueMarksInRaid;
    public static ConfigEntry<string> ValueMarksToStrip;
    public static ConfigEntry<Color> CaliberColor;
    public static ConfigEntry<bool> SwapName;
    
    public static ConfigEntry<KeyboardShortcut> ShowNamesKeyBind;
    
    public static ConfigEntry<AmmoSortMode> PrimarySort;
    public static ConfigEntry<AmmoSortMode> SecondarySort;
    public static ConfigEntry<AmmoSortDirection> PrimarySortDirection;
    public static ConfigEntry<AmmoSortDirection> SecondarySortDirection;
    
    private static readonly Dictionary<string, ConfigEntry<string>> CaliberConfigs = new();
    public static readonly ConfigEntry<Color>[] RarityColors = new ConfigEntry<Color>[10];
    
    private static ConfigFile _config;

    public static void Init(ConfigFile config)
    {
        _config = config;

        EnableAmmo = _config.Bind(CategoryToggles, "Enable caliber on ammo", true,
            "Add caliber values to the ammo short name.");

        EnableAmmoBoxes = _config.Bind(CategoryToggles, "Enable caliber on ammo boxes", true,
            "Add caliber values to the ammo boxes short name.");
        
        EnableMagazines = _config.Bind(CategoryToggles, "Enable caliber on magazines", true,
            "Add caliber values to the magazines short name.");
        
        EnableWeapons = _config.Bind(CategoryToggles, "Enable caliber on weapons", true, 
            "Replace the default caliber values on weapons with the values from the mod.");
        
        EnableCaliberInLoot = _config.Bind(CategoryToggles, "Enable caliber on in-raid loot", true,
            "Display caliber when hovering over loot in raid");
        
        EnableAmmoSort = _config.Bind(CategoryToggles, "Enable ammo & ammo boxes sorting", true,
            "Alternative comparator for ammo sorting");
        
        EnableShowNamesInTrading = _config.Bind(CategoryToggles, "Enable names in trading", true,
            "Hold a key to show item names instead of prices at traders");
        
        StripValueMarks = _config.Bind(CategoryGeneral, "Remove value marks", false,
            "Remove value marks (★☆●○) from ammo short names");
        
        StripValueMarksInRaid = _config.Bind(CategoryGeneral, "Remove value marks in raid", false,
            "Remove value marks from ammo names in raid");

        ValueMarksToStrip = _config.Bind(CategoryGeneral, "Value marks", "★☆●○",
            "Characters to strip from the beginning of ammo names");

        CaliberColor = _config.Bind(CategoryGeneral, "Caliber color", 
            new Color(0.63f, 0.63f, 0.63f, 1f),
            "Color for the caliber text in ammo");

        SwapName = _config.Bind(CategoryGeneral, "Swap name", false, 
            "Swap name with caliber");
        
        ShowNamesKeyBind = _config.Bind(CategoryInTrading, "Show names keybind", new KeyboardShortcut(KeyCode.LeftAlt),
            "Keybind to show item names");
        
        PrimarySort = _config.Bind(CategoryAmmoSorting, "Primary Sort", AmmoSortMode.Caliber);

        PrimarySortDirection = _config.Bind(CategoryAmmoSorting, "Primary sort direction", AmmoSortDirection.Ascending);
        
        SecondarySort = _config.Bind(CategoryAmmoSorting, "Secondary Sort", AmmoSortMode.Count);
        
        SecondarySortDirection = _config.Bind(CategoryAmmoSorting, "Secondary sort direction", AmmoSortDirection.Descending);
        
        for (var i = 0; i < 10; i++)
        {
            RarityColors[i] = config.Bind(CategoryRarityColor, $"{i}. Rarity color - rank {i + 1}",
                HexToColor(DefaultRarityHex[i]),
                new ConfigDescription(
                    $"Background color for rarity rank {i + 1}",
                    null,
                    new ConfigurationManagerAttributes { IsAdvanced = true }));
            
            RarityColors[i].SettingChanged += (_, _) => AmmoSortingComparatorPatch.MarkRarityDirty();
        }
    }

    public static void InitAmandsSense()
    {
        EnableCaliberInSense = _config.Bind(CategoryToggles, "Enable caliber with AmandsSense", true,
            "Show caliber for ammo in AmandsSense description");
    }
    
    public static void InitCalibers()
    {
        var factory = Singleton<ItemFactoryClass>.Instance;
        if (factory == null) return;
        
        foreach (var kvp in factory.ItemTemplates)
        {
            if (kvp.Value is AmmoTemplate ammo && !CaliberConfigs.ContainsKey(ammo.Caliber))
            {
                var caliberKey = ammo.Caliber;
                var caliberName = DefaultCaliberMap.GetValueOrDefault(caliberKey, "");
                
                CaliberConfigs[caliberKey] = _config.Bind(
                    CategoryCaliberNames, caliberKey, caliberName,
                    $"Display name for {caliberKey}");
            }
        }
    }
    
    public static string GetCaliber(string caliber)
    {
        return CaliberConfigs.TryGetValue(caliber, out var config) ? config.Value : "";
    }
    
    private static Color HexToColor(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out var c);
        return c;
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