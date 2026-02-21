using System;
using System.Collections.Generic;
using System.Reflection;
using Comfort.Common;
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
        return AccessTools.DeclaredMethod(typeof(MenuScreen), "Show", [typeof(MenuScreen).GetNestedType("GClass3877")]);
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
        var factory = Singleton<ItemFactoryClass>.Instance;
        if (factory == null) return;

        var calibers = new HashSet<string>();

        foreach (var kvp in factory.ItemTemplates)
        {
            try
            {
                if (kvp.Value is not MagazineTemplateClass mag) continue;

                calibers.Clear();
                var filters = mag.Cartridges[0].Filters;

                for (var i = 0; i < filters.Length; i++)
                {
                    var filter = filters[i];
                    if (filter?.Filter == null) continue;

                    for (var j = 0; j < filter.Filter.Length; j++)
                    {
                        var id = filter.Filter[j].ToString();
                        if (factory.ItemTemplates.TryGetValue(id, out var template) && template is AmmoTemplate ammoTemplate)
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