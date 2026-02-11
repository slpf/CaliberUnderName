using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using System.Reflection;
using BepInEx.Configuration;
using Comfort.Common;
using EFT.InventoryLogic;
using EFT.UI.DragAndDrop;
using HarmonyLib;
using SPT.Reflection.Patching;
using TMPro;
using UnityEngine;

[assembly: AssemblyProduct("Caliber Under Name")]
[assembly: AssemblyTitle("Caliber Under Name")]
[assembly: AssemblyDescription("Adds a caliber label below the short name for ammo")]
[assembly: AssemblyCopyright("SLPF")]
[assembly: AssemblyVersion("1.0.0")]
[assembly: AssemblyFileVersion("1.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0")]

namespace CaliberUnderName;

[BepInPlugin("com.slpf.caliberundername", "CaliberUnderName", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    private static Plugin _instance;
    
    public static ConfigEntry<bool> StripValueMarks;
    public static ConfigEntry<string> ValueMarksToStrip;
    public static ConfigEntry<Color> CaliberColor;

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

    private static readonly Dictionary<string, ConfigEntry<string>> CaliberConfigs = new();
    private static bool _checked;
    
    private void Awake()
    {
        _instance = this;
        
        StripValueMarks = Config.Bind(
            "1. General", "Remove Value Marks", false,
            "Remove value marks (★☆●○) from item short names");

        ValueMarksToStrip = Config.Bind(
            "1. General", "Value Marks", "★☆●○",
            "Characters to strip from the beginning of item names");

        CaliberColor = Config.Bind(
            "1. General", "Caliber Color", new Color(0.63f, 0.63f, 0.63f, 1f),
            "Color for the caliber text");
            
        new CaptionMaxLinesPatch().Enable();
        new CaliberInShortNamePatch().Enable();
    }
    
    private static ConfigEntry<string> AddCaliberToConfig(string caliber)
    {
        var caliberName = DefaultCaliberMap.GetValueOrDefault(caliber, caliber);

        return _instance.Config.Bind(
            "2. Caliber Names", caliber, caliberName,
            $"Display name for {caliber}");
    }
    
    public static void CheckCalibers()
    {
        if (_checked) return;
        
        _checked = true;

        var factory = Singleton<ItemFactoryClass>.Instance;
        
        if (factory == null) return;
        
        foreach (var kvp in factory.ItemTemplates)
        {
            if (kvp.Value is AmmoTemplate ammo && !CaliberConfigs.ContainsKey(ammo.Caliber))
            {
                CaliberConfigs[ammo.Caliber] = AddCaliberToConfig(ammo.Caliber);
            }
        }
    }
    
    public static string GetCaliberFromConfig(string caliber)
    {
        return CaliberConfigs.TryGetValue(caliber, out var config) ? config.Value : caliber;
    }
}
public class CaliberInShortNamePatch : ModulePatch
{
    private static readonly FieldInfo CaptionField = AccessTools.Field(typeof(GridItemView), "Caption");

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(GridItemView), "method_27");
    }
    
    [PatchPostfix]
    public static void Postfix(GridItemView __instance, ref string __result)
    {
        if (__instance.Item is not AmmoItemClass ammo) return;
        
        Plugin.CheckCalibers();

        var caption = (TextMeshProUGUI) CaptionField.GetValue(__instance);
        if (caption == null) return;

        var maxWidth = caption.rectTransform.rect.width;
        
        if (maxWidth <= 0f) return;

        __result = RemoveMarks(__result);

        var caliber = ammo.AmmoTemplate.Caliber;
        var caliberName = Plugin.GetCaliberFromConfig(caliber);

        var truncResult = TruncateToFit(caption, __result, maxWidth);
        var truncCalName = TruncateToFit(caption, caliberName, maxWidth);
        var hexColor = $"#{ColorUtility.ToHtmlStringRGBA(Plugin.CaliberColor.Value)}";
        
        __result = $"{truncResult}\n<color={hexColor}>{truncCalName}</color>";
    }
    
    private static string TruncateToFit(TextMeshProUGUI tmp, string text, float maxWidth)
    {
        if (tmp.GetPreferredValues(text).x <= maxWidth)
        {
            return text;
        }

        for (var i = text.Length - 1; i > 0; i--)
        {
            if (tmp.GetPreferredValues(text[..i]).x <= maxWidth)
            {
                return text[..i];
            }
        }
        
        return text[..1];
    }

    private static string RemoveMarks(string text)
    {
        if (!Plugin.StripValueMarks.Value || string.IsNullOrEmpty(text))
            return text;

        var marks = Plugin.ValueMarksToStrip.Value;

        text = marks.Aggregate(text, (current, m) => current.Replace(m.ToString(), ""));

        return text;
    }
}

public class CaptionMaxLinesPatch : ModulePatch
{
    private static readonly FieldInfo CaptionField = AccessTools.Field(typeof(GridItemView), "Caption");
    
    private static float _defaultSizeDeltaY = 0f;

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(GridItemView), "method_28");
    }

    [PatchPostfix]
    public static void Postfix(GridItemView __instance)
    {
        var caption = (TextMeshProUGUI) CaptionField.GetValue(__instance);
        if (caption == null)
        {
            return;
        }

        if (_defaultSizeDeltaY == 0f)
        {
            _defaultSizeDeltaY = caption.rectTransform.sizeDelta.y;
        }

        if (__instance.Item is AmmoItemClass)
        {
            caption.enableWordWrapping = false;
            caption.overflowMode = TextOverflowModes.Overflow;
            caption.rectTransform.sizeDelta = new Vector2(
                caption.rectTransform.sizeDelta.x, 
                _defaultSizeDeltaY + caption.fontSize);
        }
        else
        {
            caption.enableWordWrapping = false;
            caption.overflowMode = TextOverflowModes.Truncate;
            caption.rectTransform.sizeDelta = new Vector2(
                caption.rectTransform.sizeDelta.x, 
                _defaultSizeDeltaY);
        }
    }
}