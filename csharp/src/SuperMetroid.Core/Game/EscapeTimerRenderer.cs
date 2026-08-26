using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Emits the escape timer's ROM-defined label and digit spritemaps into an OAM buffer,
/// porting <c>$80:9F6C-$80:9FD3</c>.
/// </summary>
public static class EscapeTimerRenderer
{
    private const int TimerDigitPointerTable = 0x809fd4;
    private const int TimerLabelSpritemap = 0x80a060;
    private const ushort TimerPaletteBits = 0x0a00;

    /// <summary>Draws "TIME mm:ss:cc" using the timer's current fixed-point position.</summary>
    public static void Draw(EscapeTimer timer, OamBuffer oam, ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(timer);
        ArgumentNullException.ThrowIfNull(oam);
        ArgumentNullException.ThrowIfNull(bus);

        DrawAtOffset(timer, oam, bus, xOffset: 0, TimerLabelSpritemap);
        DrawTwoDigits(timer, oam, bus, timer.MinutesBcd, xOffset: unchecked((short)0xffe4));
        DrawTwoDigits(timer, oam, bus, timer.SecondsBcd, xOffset: unchecked((short)0xfffc));
        DrawTwoDigits(timer, oam, bus, timer.CentisecondsBcd, xOffset: 0x0014);
    }

    private static void DrawTwoDigits(
        EscapeTimer timer,
        OamBuffer oam,
        ISnesAddressSpace bus,
        byte packedBcd,
        short xOffset)
    {
        // The pointer table has one 16-bit bank-$80 address per decimal digit. Timer values
        // are already BCD, so each nibble directly selects a glyph without division.
        int tens = (packedBcd >> 4) & 0x0f;
        int ones = packedBcd & 0x0f;
        if (tens > 9 || ones > 9)
            throw new InvalidOperationException($"Cannot draw invalid packed-BCD timer byte ${packedBcd:X2}.");

        DrawAtOffset(timer, oam, bus, xOffset, ReadDigitSpritemapPointer(bus, tens));
        DrawAtOffset(timer, oam, bus, unchecked((short)(xOffset + 8)), ReadDigitSpritemapPointer(bus, ones));
    }

    private static ushort ReadDigitSpritemapPointer(ISnesAddressSpace bus, int digit)
    {
        int pointerAddress = TimerDigitPointerTable + digit * 2;
        return (ushort)(bus.ReadByte(pointerAddress) | (bus.ReadByte(pointerAddress + 1) << 8));
    }

    private static void DrawAtOffset(
        EscapeTimer timer,
        OamBuffer oam,
        ISnesAddressSpace bus,
        short xOffset,
        int spritemapAddress)
    {
        ushort originX = unchecked((ushort)(timer.XPixel + xOffset));
        ushort originY = timer.YPixel;

        // Digit pointers contain only a 16-bit address and inherit data bank $80 from the
        // caller. The label constant is already a complete 24-bit address.
        int fullAddress = spritemapAddress <= 0xffff ? 0x800000 | spritemapAddress : spritemapAddress;
        oam.AddOnScreenSpritemap(bus, fullAddress, originX, originY, TimerPaletteBits);
    }
}
