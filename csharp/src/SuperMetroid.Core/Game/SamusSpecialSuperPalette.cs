using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Ports the nonnegative <c>samus_special_super_palette_flags</c> branch of
/// <c>HandleMiscSamusPalette</c> at <c>$91:D8A5</c>. Ordinary Metroid attachment is the
/// retail producer: odd calls load the speed-boost palette, even calls restore the normal
/// suit, and the complete word increments until the Metroid releases Samus.
/// </summary>
public static class SamusSpecialSuperPalette
{
    /// <returns>True when the special branch owned—and wrote—the visible Samus palette.</returns>
    public static bool Update(
        ISnesAddressSpace bus,
        SnesCgram cgram,
        SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(cgram);
        ArgumentNullException.ThrowIfNull(samus);

        ushort flags = samus.SpecialSuperPaletteFlags;
        if (flags == 0 || (flags & 0x8000) != 0)
            return false;

        ushort suitOffset = samus.EquippedItems.GetSuitPaletteTableOffset();
        int pointerTable = (flags & 1) != 0
            ? SamusPaletteRomData.FullBodyCycles.SpeedBoostPointers
            : SamusPaletteRomData.Common.NormalSuitPointers;
        ushort palettePointer = ReadWord(bus, pointerTable + suitOffset);
        cgram.LoadFromBus(
            bus,
            SamusPaletteRomData.Banks.Palette | palettePointer,
            SamusPaletteRomData.Common.ColorsPerObjPalette,
            SamusPaletteRomData.Common.SamusObjPaletteStart);
        samus.SpecialSuperPaletteFlags = unchecked((ushort)(flags + 1));
        return true;
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}
