using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Shared normal-suit CGRAM copy; every owner uses the same selected visual source.</summary>
public static class SamusNormalSuitPalette
{
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

    public static void LoadPower(SnesCgram cgram, SamusSuitColorCatalog? colors)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        if (colors is null)
            throw new InvalidOperationException(
                "Power suit palette requires installed Samus suit colors.");
        colors.Apply(cgram, 0);
    }
}
