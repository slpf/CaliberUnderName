using BepInEx;
using System.Reflection;
using CaliberUnderName.Patches;

[assembly: AssemblyProduct("Caliber Under Name")]
[assembly: AssemblyTitle("Caliber Under Name")]
[assembly: AssemblyDescription("Adds a caliber label below the short name for ammo & ammo boxes")]
[assembly: AssemblyCopyright("SLPF")]
[assembly: AssemblyVersion("1.1.2")]
[assembly: AssemblyFileVersion("1.1.2")]
[assembly: AssemblyInformationalVersion("1.1.2")]

namespace CaliberUnderName;

[BepInPlugin("com.slpf.caliberundername", "CaliberUnderName", "1.1.2")]
[BepInDependency("xyz.drakia.Sense", BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
        Settings.Init(Config);

        new InitCalibersInSettingsPatch().Enable();
        
        CaliberInShortNamePatch.Enable();
        TradingShowNamePatch.Enable();
        
        new AmmoSortingComparatorPatch().Enable();
        new LootNamePatch().Enable();
        
        if (HarmonyLib.AccessTools.TypeByName("AmandsSense.Components.AmandsSenseItem") != null)
        {
            Settings.InitAmandsSense();
            new AmandsSensePatch().Enable();
        }
    }
    
    private void Update()
    {
        if (!Settings.EnableShowNamesInTrading.Value || !TradingShowNamePatch.InTraderScreen) return;

        bool keyNow = Settings.ShowNamesKeyBind.Value.IsPressed();
        if (keyNow == TradingShowNamePatch.KeyHeld) return;

        TradingShowNamePatch.KeyHeld = keyNow;
        TradingShowNamePatch.UpdateTradingViews();
    }
}