using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Shared normal-suit CGRAM copy; every owner uses the same selected visual source.</summary>
public static class SamusNormalSuitPalette
{
    public static ushort Load(ISnesAddressSpace bus, SnesCgram cgram, ushort equippedItems,
        SamusSuitColorCatalog? colors = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(cgram);
        ushort offset = equippedItems.GetSuitPaletteTableOffset();
        ushort pointer = SamusPaletteRomData.Common.NormalSuitPalettePointer(offset);
        if (colors is null)
            cgram.LoadFromBus(bus, SamusPaletteRomData.Banks.Palette | pointer,
                SamusPaletteRomData.Common.ColorsPerObjPalette,
                SamusPaletteRomData.Common.SamusObjPaletteStart);
        else
            colors.Apply(cgram, offset);
        return pointer;
    }

    public static void LoadPower(ISnesAddressSpace bus, SnesCgram cgram,
        SamusSuitColorCatalog? colors = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(cgram);
        if (colors is null)
            cgram.LoadFromBus(bus, SamusRenderingRomData.Body.PowerSuitPalette,
                SamusPaletteRomData.Common.ColorsPerObjPalette,
                SamusPaletteRomData.Common.SamusObjPaletteStart);
        else
            colors.Apply(cgram, 0);
    }
}
