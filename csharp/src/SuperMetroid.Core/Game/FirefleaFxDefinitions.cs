namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled fixed-color shade definitions for the Fireflea room effect. The flashing
/// phase and enemy-death darkness level remain mutable WRAM state in
/// <see cref="FirefleaRoomFx"/>.
/// </summary>
internal static class FirefleaFxDefinitions
{
    /// <summary>$88:B07C PHP / REP opcode bytes read at darkness offset twelve.</summary>
    private const ushort AdjacentOpcodeShade = 0xc208;
    /// <summary>$88:B07F/$88:B0D5: six effect passes per flashing shade.</summary>
    internal const ushort FlashDuration = 6;

    /// <summary>$88:B0EC: twelve entries before the flashing cycle wraps.</summary>
    internal const ushort FlashCount = 12;

    /// <summary>$88:B0DE: darkness byte offset at which flashing stops.</summary>
    internal const ushort SteadyDarknessThreshold = 10;

    /// <summary>$88:B0E3: fixed flashing-table index used at maximum darkness.</summary>
    internal const ushort SteadyFlashIndex = 6;

    /// <summary>Returns the packed triangular shade 256*(6-abs(index-6)), index 0..11.</summary>
    /// <remarks>Independently checked for #1165 against every original NTSC J/U v1.0
    /// word and pinned bank_88.asm. The six-frame timer and phase wrap are caller-owned;
    /// this exact integer wave requires no sampled floating-point construction.</remarks>
    internal static ushort FlashingShade(ushort index)
    {
        if (index >= FlashCount)
        {
            throw new InvalidDataException(
                $"Fireflea flashing index {index} is outside the {FlashCount}-entry native cycle.");
        }

        return (ushort)((6 - Math.Abs(index - 6)) << 8);
    }

    /// <summary>Returns the capped six-step darkness ramp, plus the native offset-12 alias.</summary>
    /// <remarks>Independently checked for #1165 against all seven original physical reads
    /// and pinned bank_88.asm. Even byte offsets 0..10 use 256*min(3*offset,25).
    /// Offset 12 reads PHP/REP opcode bytes, not an extrapolated ramp value. The death
    /// counter permits that offset. Reject odd offsets and offsets above 12; retain the
    /// caller's wrapped 16-bit flash-plus-darkness addition before taking the high byte.</remarks>
    internal static ushort DarknessShade(ushort byteOffset)
    {
        if ((byteOffset & 1) != 0 || byteOffset > 12)
        {
            throw new InvalidDataException(
                $"Fireflea darkness byte offset {byteOffset} is outside the retail 0-through-12 even domain.");
        }

        return byteOffset == 12 ? AdjacentOpcodeShade : (ushort)(Math.Min(3 * byteOffset, 25) << 8);
    }
}
