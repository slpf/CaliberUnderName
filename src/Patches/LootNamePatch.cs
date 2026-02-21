using System.Linq;
using System.Reflection;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CaliberUnderName.Patches;

public class LootNamePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(GetActionsClass), "smethod_9");
    }
    
    [PatchPrefix]
    public static void Prefix(Item rootItem, ref string lootItemName)
    {
        if (!Settings.EnableCaliberInLoot.Value) return;
        
        var caliberKey = Helper.GetCaliber(rootItem);
        if (caliberKey == null) return;
        
        lootItemName = lootItemName.Localized();
        
        if (Settings.StripValueMarksInRaid.Value) lootItemName = Helper.RemoveMarks(lootItemName);

        var caliberName = string.Join(" / ", caliberKey.Split('/')
            .Select(Settings.GetCaliber)
            .Where(n => !string.IsNullOrEmpty(n)));
        
        if (string.IsNullOrEmpty(caliberName)) return;

        lootItemName = $"{Helper.StripAfter(lootItemName, " - ")} - {caliberName}";
    }
}