using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Shared normal-suit CGRAM copy; every owner uses the same selected visual source.</summary>
public static class SamusNormalSuitPalette
{
    /// <summary>Restores Samus's sixteen normal OBJ palette colors from installed artwork, selecting Gravity before Varia before Power without changing equipment or animation state.</summary>
    /// <param name="cgram">CGRAM receiving Samus's OBJ palette-four slots, colors 192..207, including the transparent-slot payload.</param>
    /// <param name="equippedItems">Native equipped-item bitfield; only the Gravity/Varia suit bits select the normal palette.</param>
    /// <param name="colors">Required installed suit-color catalog; displayed words may differ from stock cartridge colors.</param>
    /// <returns>The stock bank-$9B palette pointer identity ($9400, $9520, or $9800) for diagnostics, not an address read to obtain the displayed colors.</returns>
    /// <exception cref="ArgumentNullException">CGRAM is null.</exception>
    /// <exception cref="InvalidOperationException">Installed suit colors are absent.</exception>
    public static ushort Load(SnesCgram cgram, ushort equippedItems,
        SamusSuitColorCatalog? colors)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        if (colors is null)
            throw new InvalidOperationException(
                "Normal suit palette requires installed Samus suit colors.");
        ushort offset = equippedItems.GetSuitPaletteTableOffset();
        ushort pointer = SamusPaletteRomData.Common.NormalSuitPalettePointer(offset);
        colors.Apply(cgram, offset);
        return pointer;
    }

    /// <summary>Restores the installed normal Power-suit colors to Samus's sixteen OBJ palette slots regardless of equipped suit flags, without changing gameplay state.</summary>
    /// <param name="cgram">CGRAM receiving colors 192..207, including the transparent-slot payload.</param>
    /// <param name="colors">Required installed normal-suit artwork; selects its Power palette at native table offset zero.</param>
    /// <exception cref="ArgumentNullException">CGRAM is null.</exception>
    /// <exception cref="InvalidOperationException">Installed suit colors are absent.</exception>
    public static void LoadPower(SnesCgram cgram, SamusSuitColorCatalog? colors)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        if (colors is null)
            throw new InvalidOperationException(
                "Power suit palette requires installed Samus suit colors.");
        colors.Apply(cgram, 0);
    }
}
