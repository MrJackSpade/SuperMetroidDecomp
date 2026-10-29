using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>Shared normal-suit colour restore; every owner shows the same selected suit colours.</summary>
public static class SamusNormalSuitPalette
{
    /// <summary>Restores Samus's sixteen normal suit colours from installed artwork for the suit her equipment shows, without changing equipment or animation state.</summary>
    /// <param name="suit">Samus's suit colours, all sixteen of which change.</param>
    /// <param name="equippedItems">Samus's equipment; only the Gravity and Varia suits choose the suit shown.</param>
    /// <param name="colors">Required installed suit-color catalog; displayed colours may differ from stock cartridge colors.</param>
    /// <exception cref="InvalidOperationException">Installed suit colors are absent.</exception>
    public static void Load(ColourRun suit, SamusEquipmentFlags equippedItems,
        InstalledSamusSuitColors? colors)
    {
        if (colors is null)
            throw new InvalidOperationException(
                "Normal suit palette requires installed Samus suit colors.");
        colors.Apply(suit, equippedItems.ShownSuit());
    }

    /// <summary>Restores the installed normal Power-suit colours regardless of equipped suits, without changing gameplay state.</summary>
    /// <param name="suit">Samus's suit colours, all sixteen of which change.</param>
    /// <param name="colors">Required installed normal-suit artwork.</param>
    /// <exception cref="InvalidOperationException">Installed suit colors are absent.</exception>
    public static void LoadPower(ColourRun suit, InstalledSamusSuitColors? colors)
    {
        if (colors is null)
            throw new InvalidOperationException(
                "Power suit palette requires installed Samus suit colors.");
        colors.Apply(suit, SamusSuit.Power);
    }
}
