using System.Collections.Generic;
using System.Reflection;
using EFT.UI;
using EFT.UI.DragAndDrop;
using HarmonyLib;
using SPT.Reflection.Patching;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CaliberUnderName.Patches;

public static class TradingShowNamePatch
{
    internal static bool InTraderScreen = false;
    internal static bool KeyHeld = false;
    
    internal static TraderDealScreen DealScreen;

    private static readonly FieldInfo TraderGridField = AccessTools.Field(typeof(TraderDealScreen), "_traderGridView");
    private static readonly FieldInfo StashGridField = AccessTools.Field(typeof(TraderDealScreen), "_stashGridView");
    private static readonly FieldInfo ItemViewsField = AccessTools.Field(typeof(GridView), "ItemViews");

    public static void Enable()
    {
        new TradingScreenShowPatch().Enable();
        new TradingScreenClosePatch().Enable();
        new TradingUpdateInfoPatch().Enable();
    }
    
    public static void UpdateTradingViews()
    {
        if (DealScreen == null) return;

        UpdateGrid(TraderGridField.GetValue(DealScreen));
        UpdateGrid(StashGridField.GetValue(DealScreen));
    }

    private static void UpdateGrid(object gridView)
    {
        if (gridView == null) return;
        var dict = (Dictionary<string, ItemView>) ItemViewsField.GetValue(gridView);
        if (dict == null) return;

        foreach (var view in dict.Values)
        {
            if (view is TradingItemView tradingView) tradingView.UpdateInfo();
        }
    }
    
    public class TradingScreenShowPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(TraderDealScreen), nameof(TraderDealScreen.Show));
        }

        [PatchPostfix]
        public static void Postfix(TraderDealScreen __instance)
        {
            InTraderScreen = true;
            DealScreen = __instance;
        } 
    }

    public class TradingScreenClosePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(TraderDealScreen), nameof(TraderDealScreen.Close));
        }

        [PatchPostfix]
        public static void Postfix()
        {
            InTraderScreen = false;
            KeyHeld = false;
            DealScreen = null;
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
                
                caption.rectTransform.offsetMin = new Vector2(2, caption.rectTransform.offsetMin.y);
                caption.rectTransform.offsetMax = new Vector2(-2, caption.rectTransform.offsetMax.y);
            }
            
            if (price != null) price.gameObject.SetActive(false);
            if (currency != null) currency.gameObject.SetActive(false);
            if (schemeIcon != null) schemeIcon.gameObject.SetActive(false);
        }
    }
}