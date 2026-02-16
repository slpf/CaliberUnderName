using System.Reflection;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CaliberUnderName.Patches;

public class InitCalibersInSettingsPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(EFT.UI.MenuScreen), nameof(EFT.UI.MenuScreen.Show));
    }

    [PatchPostfix]
    public static void Postfix() => Settings.InitCalibers();
}