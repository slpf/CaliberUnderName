using System.Reflection;
using EFT.Interactive;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using TMPro;

namespace CaliberUnderName.Patches;

public class AmandsSensePatch : ModulePatch
{
    private static FieldInfo _observedLootItemField = AccessTools.Field(
        AccessTools.TypeByName("AmandsSense.Components.AmandsSenseItem"), "observedLootItem");
    private static FieldInfo _descriptionTextField = AccessTools.Field(
        AccessTools.TypeByName("AmandsSense.Components.AmandsSenseConstructor"), "descriptionText");
    
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(
            AccessTools.TypeByName("AmandsSense.Components.AmandsSenseItem"), "SetSense", [typeof(ObservedLootItem)]);
    }
    
    [PatchPostfix]
    public static void Postfix(object __instance)
    {
        if (!Settings.EnableCaliberInSense.Value) return;

        var lootItem = _observedLootItemField.GetValue(__instance) as ObservedLootItem;
        if (lootItem?.Item == null) return;

        if (lootItem.Item is AmmoBox) return;
        
        var caliberKey = CaliberInShortNamePatch.GetCaliber(lootItem.Item);
        if (caliberKey == null) return;

        var caliberName = Settings.GetCaliber(caliberKey);
        if (string.IsNullOrEmpty(caliberName)) return;

        var descText = _descriptionTextField.GetValue(__instance) as TextMeshPro;
        if (descText == null) return;

        if (string.IsNullOrEmpty(descText.text)) descText.text = caliberName;
        
    }
    
}