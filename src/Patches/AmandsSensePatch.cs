using System.Linq;
using System.Reflection;
using EFT.Interactive;
using HarmonyLib;
using SPT.Reflection.Patching;
using TMPro;

namespace CaliberUnderName.Patches;

public class AmandsSensePatch : ModulePatch
{
    private static readonly FieldInfo _observedLootItemField = AccessTools.Field(
        AccessTools.TypeByName("AmandsSense.Components.AmandsSenseItem"), "observedLootItem");
    private static readonly FieldInfo _nameTextField = AccessTools.Field(
        AccessTools.TypeByName("AmandsSense.Components.AmandsSenseConstructor"),"nameText");
    private static readonly FieldInfo _descriptionTextField = AccessTools.Field(
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
        
        var caliberKey = Helper.GetCaliber(lootItem.Item);
        if (caliberKey == null) return;
        
        var nameText = _nameTextField.GetValue(__instance) as TextMeshPro;

        if (nameText != null)
        {
            nameText.text = CleanSenseName(nameText.text);
        }

        var caliberName = string.Join(" / ", caliberKey.Split('/')
            .Select(Settings.GetCaliber)
            .Where(n => !string.IsNullOrEmpty(n)));
        
        if (string.IsNullOrEmpty(caliberName)) return;

        var descText = _descriptionTextField.GetValue(__instance) as TextMeshPro;
        if (descText == null) return;

        descText.text = string.IsNullOrEmpty(descText.text) ? caliberName : $"{descText.text} - {caliberName}";
    }
    
    private static string CleanSenseName(string text)
    {
        var raw = text.Replace("<b>", "").Replace("</b>", "");
        raw = Helper.StripAfter(raw, " - ");
        if (Settings.StripValueMarksInRaid.Value) raw = Helper.RemoveMarks(raw);
        return "<b>" + raw + "</b>";
    }
}