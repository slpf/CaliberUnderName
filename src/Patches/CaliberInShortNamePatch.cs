using System.Linq;
using System.Reflection;
using EFT.InventoryLogic;
using EFT.UI.DragAndDrop;
using HarmonyLib;
using SPT.Reflection.Patching;
using TMPro;
using UnityEngine;

namespace CaliberUnderName.Patches;

public static class CaliberInShortNamePatch
{
    public static void Enable()
    {
        new AddCaliberInShortName().Enable();
        new CaptionMaxLinesPatch().Enable();
    }
    
    public class AddCaliberInShortName : ModulePatch
    {
        private static readonly FieldInfo CaptionField = AccessTools.Field(typeof(GridItemView), "Caption");

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GridItemView), "method_27");
        }

        [PatchPostfix]
        public static void Postfix(GridItemView __instance, ref string __result)
        {
            if (__instance.GetType() != typeof(GridItemView) && __instance is not TradingItemView) return;
            if (__instance is TradingItemView && !Settings.EnableShowNamesInTrading.Value) return;

            var caliber = GetCaliber(__instance.Item);
            if (caliber == null) return;

            Settings.InitCalibers();

            var caliberName = Settings.GetCaliber(caliber);
            if (string.IsNullOrEmpty(caliberName)) return;

            var caption = (TextMeshProUGUI) CaptionField.GetValue(__instance);
            if (caption == null) return;

            var maxWidth = ((RectTransform) __instance.transform).rect.width;
            if (maxWidth <= 0f) return;

            __result = RemoveMarks(__result);

            var truncResult = TruncateToFit(caption, __result, maxWidth);
            var truncCalName = TruncateToFit(caption, caliberName, maxWidth);
            var hexColor = $"#{ColorUtility.ToHtmlStringRGBA(Settings.CaliberColor.Value)}";

            __result = $"{truncResult}\n<color={hexColor}>{truncCalName}</color>";
        }

        private static string TruncateToFit(TextMeshProUGUI tmp, string text, float maxWidth)
        {
            if (tmp.GetPreferredValues(text).x <= maxWidth)
                return text;

            for (var i = text.Length - 1; i > 0; i--)
            {
                if (tmp.GetPreferredValues(text[..i]).x <= maxWidth)
                    return text[..i];
            }

            return text[..1];
        }

        private static string RemoveMarks(string text)
        {
            if (!Settings.StripValueMarks.Value || string.IsNullOrEmpty(text)) return text;

            var marks = Settings.ValueMarksToStrip.Value;

            return marks.Aggregate(text, (current, m) => current.Replace(m.ToString(), ""));
        }
    }
    
    public class CaptionMaxLinesPatch : ModulePatch
    {
        private static readonly FieldInfo CaptionField = AccessTools.Field(typeof(GridItemView), "Caption");

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GridItemView), "method_28");
        }

        [PatchPostfix]
        public static void Postfix(GridItemView __instance)
        {
            if (__instance.GetType() != typeof(GridItemView)) return;

            Settings.InitCalibers();

            var caption = (TextMeshProUGUI)CaptionField.GetValue(__instance);
            if (caption == null) return;

            var caliberKey = GetCaliber(__instance.Item);
            var hasCaliber = caliberKey != null && !string.IsNullOrEmpty(Settings.GetCaliber(caliberKey));

            if (hasCaliber)
            {
                caption.overflowMode = TextOverflowModes.Overflow;
                caption.rectTransform.sizeDelta = new Vector2(caption.rectTransform.sizeDelta.x, 16f + caption.fontSize);
            }
            else
            {
                caption.overflowMode = TextOverflowModes.Truncate;
                caption.rectTransform.sizeDelta = new Vector2(caption.rectTransform.sizeDelta.x, 16f);
            }
        }
    }
    
    public static string GetCaliber(Item item)
    {
        if (item is AmmoItemClass ammo)
            return ammo.AmmoTemplate.Caliber;

        if (item is AmmoBox ammoBox)
        {
            var first = ammoBox.Cartridges?.Items?.FirstOrDefault() as AmmoItemClass;
            return first?.AmmoTemplate.Caliber;
        }

        return null;
    }
}