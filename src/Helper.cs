using System;
using System.Linq;
using EFT.InventoryLogic;

namespace CaliberUnderName;

public static class Helper
{
    public static string GetCaliber(Item item)
    {
        if (item is AmmoItemClass ammo)
            return ammo.AmmoTemplate.Caliber;

        if (item is AmmoBox ammoBox)
        {
            var first = ammoBox.Cartridges?.Items?.FirstOrDefault() as AmmoItemClass;
            return first?.AmmoTemplate.Caliber;
        }

        return null;
    }
    
    public static string RemoveMarks(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        var marks = Settings.ValueMarksToStrip.Value;
        return marks.Aggregate(text, (current, m) => current.Replace(m.ToString(), ""));
    }
    
    public static string StripAfter(string text, string separator)
    {
        int idx = text.IndexOf(separator, StringComparison.Ordinal);
        return idx >= 0 ? text[..idx] : text;
    }
}