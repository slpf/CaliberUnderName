using System;
using System.Collections.Generic;
using System.Linq;
using EFT.InventoryLogic;

namespace CaliberUnderName;

public static class Helper
{
    public static readonly Dictionary<string, string> MagCaliberCache = new();
    
    public static string GetCaliber(Item item)
    {
        if (item is AmmoItemClass ammo && Settings.EnableAmmo.Value)
        {
            return ammo.AmmoTemplate.Caliber;
        }

        if (item is AmmoBox ammoBox && Settings.EnableAmmoBoxes.Value)
        {
            var first = ammoBox.Cartridges?.Items?.FirstOrDefault() as AmmoItemClass;
            return first?.AmmoTemplate.Caliber;
        }
        
        if (item is MagazineItemClass magazine && Settings.EnableMagazines.Value)
        {
            return MagCaliberCache.GetValueOrDefault(magazine.TemplateId.ToString());
        }

        return null;
    }
    
    public static string RemoveMarks(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        var marks = Settings.ValueMarksToStrip.Value;
        if (string.IsNullOrEmpty(marks)) return text;
        
        var hasMark = false;
        for (var i = 0; i < text.Length; i++)
        {
            for (var j = 0; j < marks.Length; j++)
            {
                if (text[i] == marks[j])
                {
                    hasMark = true;
                    break;
                }
            }
            if (hasMark) break;
        }

        if (!hasMark) return text;
        
        var chars = new char[text.Length];
        var pos = 0;

        for (var i = 0; i < text.Length; i++)
        {
            var isMark = false;
            for (var j = 0; j < marks.Length; j++)
            {
                if (text[i] == marks[j])
                {
                    isMark = true;
                    break;
                }
            }
            if (!isMark) chars[pos++] = text[i];
        }

        return new string(chars, 0, pos);
    }
    
    public static string StripAfter(string text, string separator)
    {
        var idx = text.IndexOf(separator, StringComparison.Ordinal);
        return idx >= 0 ? text[..idx] : text;
    }
}