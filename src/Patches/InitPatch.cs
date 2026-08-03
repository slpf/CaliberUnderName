using System;
using System.Collections.Generic;
using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CaliberUnderName.Patches;

public class InitPatch : ModulePatch
{
    private static bool _initialized;
    
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.DeclaredMethod(typeof(MenuScreen), "Show", [typeof(MenuScreen).GetNestedType("MainMenuBaseScreenController")]);
    }

    [PatchPostfix]
    public static void Postfix()
    {
        if (_initialized) return;
        
        _initialized = true;
        
        Settings.InitCalibers();
        InitMagazines();
    }

    private static void InitMagazines()
    {
        var factory = Singleton<ItemFactory>.Instance;
        if (factory == null) return;

        var calibers = new HashSet<string>();

        foreach (var kvp in factory.ItemTemplates)
        {
            try
            {
                if (kvp.Value is not MagazineTemplate mag) continue;

                calibers.Clear();
                var cartridges = mag.Cartridges;
                if (cartridges == null || cartridges.Length == 0) continue;

                var filters = cartridges[0]?.Filters;
                if (filters == null) continue;

                for (var i = 0; i < filters.Length; i++)
                {
                    var allowed = filters[i]?.Filter;
                    if (allowed == null) continue;

                    for (var j = 0; j < allowed.Length; j++)
                    {
                        if (!factory.ItemTemplates.TryGetValue(allowed[j].ToString(), out var template)) continue;
                        if (template is AmmoTemplate ammoTemplate && !string.IsNullOrEmpty(ammoTemplate.Caliber))
                        {
                            calibers.Add(ammoTemplate.Caliber);
                        }
                    }
                }

                if (calibers.Count > 0) Helper.MagCaliberCache[kvp.Key] = string.Join("/", calibers);
            }
            catch (Exception e)
            {
                Plugin.LogSource.LogError($"Error on magazine {kvp.Key}: {e.Message}");
            }
        }
    }
}