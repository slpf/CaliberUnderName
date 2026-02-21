using System.Linq;
using System.Reflection;
using EFT.UI.DragAndDrop;
using HarmonyLib;
using SPT.Reflection.Patching;
using TMPro;
using UnityEngine;

namespace CaliberUnderName.Patches;

public static class CaliberInNamePatch
{
    public static void Enable()
    {
        new CaliberInShortName().Enable();
        new CaptionMaxLinesPatch().Enable();
    }
    
    public class CaliberInShortName : ModulePatch
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
            
            var caption = (TextMeshProUGUI) CaptionField.GetValue(__instance);
            if (caption == null) return;
            
            var maxWidth = ((RectTransform) __instance.transform).rect.width;
            if (maxWidth <= 0f) return;
            
            var caliber = Helper.GetCaliber(__instance.Item);
            if (caliber != null)
            {
                __result = Helper.StripAfter(__result, " - ");
                if (Settings.StripValueMarks.Value) __result = Helper.RemoveMarks(__result);
            }
            
            __result = TruncateToFit(caption, __result, maxWidth);
            
            if (caliber == null) return;
            
            var caliberNames = caliber.Split('/')
                .Select(Settings.GetCaliber)
                .Where(n => !string.IsNullOrEmpty(n))
                .Distinct()
                .ToList();
            
            if (caliberNames.Count == 0) return;
            
            var hexColor = $"#{ColorUtility.ToHtmlStringRGBA(Settings.CaliberColor.Value)}";
            var caliberBlock = string.Join("\n", caliberNames
                .Select(n => $"<color={hexColor}>{TruncateToFit(caption, n, maxWidth)}</color>"));

            __result = Settings.SwapName.Value ? $"{caliberBlock}\n{__result}" : $"{__result}\n{caliberBlock}";
        }

        private static string TruncateToFit(TextMeshProUGUI tmp, string text, float maxWidth)
        {
            if (tmp.GetPreferredValues(text).x <= maxWidth) return text;

            var lo = 1; 
            var hi = text.Length - 1;

            while (lo < hi)
            {
                var mid = (lo + hi + 1) / 2;
                if (tmp.GetPreferredValues(text[..mid]).x <= maxWidth)
                {
                    lo = mid;
                }
                else
                {
                    hi = mid - 1;
                }
            }

            return text[..lo];
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

            var caption = (TextMeshProUGUI) CaptionField.GetValue(__instance);
            if (caption == null) return;

            var caliber = Helper.GetCaliber(__instance.Item);
            
            if (caliber != null)
            {
                var caliberCount = caliber.Split('/')
                    .Select(Settings.GetCaliber)
                    .Count(n => !string.IsNullOrEmpty(n));

                caption.overflowMode = TextOverflowModes.Overflow;
                caption.rectTransform.sizeDelta = new Vector2(caption.rectTransform.sizeDelta.x, 16f + caption.fontSize * caliberCount);
            }
            else
            {
                caption.overflowMode = TextOverflowModes.Truncate;
                caption.rectTransform.sizeDelta = new Vector2(caption.rectTransform.sizeDelta.x, 16f);
            }
        }
    }
}