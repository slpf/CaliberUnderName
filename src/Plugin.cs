using BepInEx;
using System.Reflection;
using CaliberUnderName.Patches;
using EFT.UI.DragAndDrop;

[assembly: AssemblyProduct("Caliber Under Name")]
[assembly: AssemblyTitle("Caliber Under Name")]
[assembly: AssemblyDescription("Adds a caliber label below the short name for ammo & ammo boxes")]
[assembly: AssemblyCopyright("SLPF")]
[assembly: AssemblyVersion("1.0.2")]
[assembly: AssemblyFileVersion("1.0.2")]
[assembly: AssemblyInformationalVersion("1.0.2")]

namespace CaliberUnderName;

[BepInPlugin("com.slpf.caliberundername", "CaliberUnderName", "1.0.2")]
public class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
        Settings.Init(Config);

        CaliberInShortNamePatch.Enable();
        TradingShowNamePatch.Enable();
    }
    
    private void Update()
    {
        if (!Settings.EnableShowNamesInTrading.Value || !TradingShowNamePatch.InTraderScreen) return;

        bool keyNow = Settings.ShowNamesKeyBind.Value.IsPressed();
        if (keyNow == TradingShowNamePatch.KeyHeld) return;

        TradingShowNamePatch.KeyHeld = keyNow;

        foreach (var view in FindObjectsOfType<TradingItemView>()) view.UpdateInfo();
    }
}