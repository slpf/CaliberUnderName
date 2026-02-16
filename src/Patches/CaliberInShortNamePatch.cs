using System.Reflection;
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
            
            var caliberName = Settings.GetCaliber(caliber);
            if (string.IsNullOrEmpty(caliberName)) return;
            
            var truncCalName = TruncateToFit(caption, caliberName, maxWidth);
            var hexColor = $"#{ColorUtility.ToHtmlStringRGBA(Settings.CaliberColor.Value)}";

            if (Settings.SwapName.Value)
            {
                __result = $"<color={hexColor}>{truncCalName}</color>\n{__result}";
            }
            else
            {
                __result = $"{__result}\n<color={hexColor}>{truncCalName}</color>";
            }
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

            var caption = (TextMeshProUGUI)CaptionField.GetValue(__instance);
            if (caption == null) return;

            var caliberKey = Helper.GetCaliber(__instance.Item);
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
    
    
}