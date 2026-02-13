using System.Reflection;
using EFT.UI;
using EFT.UI.DragAndDrop;
using HarmonyLib;
using SPT.Reflection.Patching;
using TMPro;
using UnityEngine.UI;

namespace CaliberUnderName.Patches;

public static class TradingShowNamePatch
{
    internal static bool InTraderScreen = false;
    internal static bool KeyHeld = false;

    public static void Enable()
    {
        new TradingScreenShowPatch().Enable();
        new TradingScreenClosePatch().Enable();
        new TradingUpdateInfoPatch().Enable();
    }
    
    public class TradingScreenShowPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(TraderScreensGroup), nameof(TraderScreensGroup.Show));
        }

        [PatchPostfix]
        public static void Postfix()
        {
            InTraderScreen = true;
        } 
    }

    public class TradingScreenClosePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(TraderScreensGroup), nameof(TraderScreensGroup.Close));
        }

        [PatchPostfix]
        public static void Postfix()
        {
            InTraderScreen = false;
            KeyHeld = false;
        }
    }
    
    public class TradingUpdateInfoPatch : ModulePatch
    {
        private static readonly FieldInfo CaptionField = AccessTools.Field(typeof(GridItemView), "Caption");
        private static readonly FieldInfo PriceField = AccessTools.Field(typeof(TradingItemView), "_price");
        private static readonly FieldInfo CurrencyField = AccessTools.Field(typeof(TradingItemView), "_currency");
        private static readonly FieldInfo SchemeIconField = AccessTools.Field(typeof(TradingItemView), "_schemeIcon");
        
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(TradingItemView), nameof(TradingItemView.UpdateInfo));
        }
        
        [PatchPostfix]
        public static void Postfix(TradingItemView __instance)
        {
            if (!Settings.EnableShowNamesInTrading.Value || !KeyHeld) return;
    
            var caption = (TextMeshProUGUI) CaptionField.GetValue(__instance);
            var price = (TextMeshProUGUI) PriceField.GetValue(__instance);
            var currency = (TextMeshProUGUI) CurrencyField.GetValue(__instance);
            var schemeIcon = (Image) SchemeIconField.GetValue(__instance);

            if (caption != null)
            {
                caption.gameObject.SetActive(true);
                var caliberKey = CaliberInShortNamePatch.GetCaliber(__instance.Item);
                var hasCaliber = caliberKey != null && !string.IsNullOrEmpty(Settings.GetCaliber(caliberKey));
                caption.overflowMode = hasCaliber ? TextOverflowModes.Overflow : TextOverflowModes.Truncate;
            }
            
            if (price != null) price.gameObject.SetActive(false);
            if (currency != null) currency.gameObject.SetActive(false);
            if (schemeIcon != null) schemeIcon.gameObject.SetActive(false);
        }
    }
}