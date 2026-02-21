using BepInEx;
using System.Reflection;
using BepInEx.Logging;
using CaliberUnderName.Patches;

[assembly: AssemblyProduct("Caliber Under Name")]
[assembly: AssemblyTitle("Caliber Under Name")]
[assembly: AssemblyDescription("Adds a caliber label below the short name for ammo & ammo boxes")]
[assembly: AssemblyCopyright("SLPF")]
[assembly: AssemblyVersion("1.1.4")]
[assembly: AssemblyFileVersion("1.1.4")]
[assembly: AssemblyInformationalVersion("1.1.4")]

namespace CaliberUnderName;

[BepInPlugin("com.slpf.caliberundername", "CaliberUnderName", "1.1.4")]
[BepInDependency("xyz.drakia.Sense", BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BaseUnityPlugin
{
    public static ManualLogSource LogSource;
    
    private void Awake()
    {
        LogSource = Logger;
        
        Settings.Init(Config);

        new InitPatch().Enable();
        new CaliberInWeaponInscriptionPatch().Enable();
        new AmmoSortingComparatorPatch().Enable();
        new LootNamePatch().Enable();
        
        CaliberInNamePatch.Enable();
        TradingShowNamePatch.Enable();
        
        if (HarmonyLib.AccessTools.TypeByName("AmandsSense.Components.AmandsSenseItem") != null)
        {
            Settings.InitAmandsSense();
            new AmandsSensePatch().Enable();
        }
    }
    
    private void Update()
    {
        if (!Settings.EnableShowNamesInTrading.Value || !TradingShowNamePatch.InTraderScreen) return;

        var keyNow = Settings.ShowNamesKeyBind.Value.IsPressed();
        if (keyNow == TradingShowNamePatch.KeyHeld) return;

        TradingShowNamePatch.KeyHeld = keyNow;
        TradingShowNamePatch.UpdateTradingViews();
    }
}