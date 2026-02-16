using System.Reflection;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CaliberUnderName.Patches;

public class InitCalibersInSettingsPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.DeclaredMethod(typeof(MenuScreen), "Show", [typeof(MenuScreen).GetNestedType("GClass3877")
        ]);
    }

    [PatchPostfix]
    public static void Postfix() => Settings.InitCalibers();
}