using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>Shared normal-suit CGRAM copy; every owner uses the same selected visual source.</summary>
public static class SamusNormalSuitPalette
{
    /// <summary>Restores Samus's sixteen normal OBJ palette colors from installed artwork, selecting Gravity before Varia before Power without changing equipment or animation state.</summary>
    /// <param name="suit">Samus's suit colours, all sixteen of which change.</param>
    /// <param name="equippedItems">Native equipped-item bitfield; only the Gravity/Varia suit bits select the normal palette.</param>
    /// <param name="colors">Required installed suit-color catalog; displayed words may differ from stock cartridge colors.</param>
    /// <exception cref="ArgumentNullException">CGRAM is null.</exception>
    /// <exception cref="InvalidOperationException">Installed suit colors are absent.</exception>
    public static void Load(ColourRun suit, SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems,
        SamusSuitColorCatalog? colors)
    {
        if (colors is null)
            throw new InvalidOperationException(
                "Normal suit palette requires installed Samus suit colors.");
        colors.Apply(suit, equippedItems.GetSuitPaletteTableOffset());
    }

    /// <summary>Restores the installed normal Power-suit colors to Samus's sixteen OBJ palette slots regardless of equipped suit flags, without changing gameplay state.</summary>
    /// <param name="suit">Samus's suit colours, all sixteen of which change.</param>
    /// <param name="colors">Required installed normal-suit artwork; selects its Power palette at native table offset zero.</param>
    /// <exception cref="ArgumentNullException">CGRAM is null.</exception>
    /// <exception cref="InvalidOperationException">Installed suit colors are absent.</exception>
    public static void LoadPower(ColourRun suit, SamusSuitColorCatalog? colors)
    {
        if (colors is null)
            throw new InvalidOperationException(
                "Power suit palette requires installed Samus suit colors.");
        colors.Apply(suit, 0);
    }
}
