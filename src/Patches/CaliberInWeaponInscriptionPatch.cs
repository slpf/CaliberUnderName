using System.Reflection;
using EFT.UI.DragAndDrop;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CaliberUnderName.Patches;

public class CaliberInWeaponInscriptionPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(GridItemView), "SetInscription");
    }
        
    [PatchPrefix]
    public static void Prefix(GridItemView __instance, ref string inscription)
    {
        if (!Settings.EnableWeapons.Value) return;
        
        var caliberName = Settings.GetCaliber($"Caliber{inscription}"); 
        
        if (caliberName.IsNullOrEmpty()) return;
        
        inscription = caliberName;
    }
}