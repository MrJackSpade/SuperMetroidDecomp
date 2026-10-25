namespace SuperMetroid.Core.Fx;

/// <summary>
/// Compiled fixed-color shade definitions for the Fireflea room effect. The flashing
/// phase and enemy-death darkness level are mutable state in <see cref="FirefleaFxState"/>.
/// </summary>
internal static class FirefleaFxDefinitions
{
    /// <summary>
    /// The shade level at darkness offset twelve, one entry past the six-step ramp: the cartridge reads the first byte
    /// of the next routine there, 194, whose five-bit intensity is two.
    /// </summary>
    private const byte PastRampShadeLevel = 194;
    /// <summary>$88:B07F/$88:B0D5: six effect passes per flashing shade.</summary>
    internal const ushort FlashDuration = 6;

    /// <summary>$88:B0EC: twelve entries before the flashing cycle wraps.</summary>
    internal const ushort FlashCount = 12;

    /// <summary>$88:B0DE: darkness byte offset at which flashing stops.</summary>
    internal const ushort SteadyDarknessThreshold = 10;

    /// <summary>$88:B0E3: fixed flashing-table index used at maximum darkness.</summary>
    internal const ushort SteadyFlashIndex = 6;

    /// <summary>Returns the triangular flashing shade level 6-abs(index-6), index 0..11.</summary>
    /// <remarks>Independently checked for #1165 against every original NTSC J/U v1.0
    /// word and pinned bank_88.asm. The six-frame timer and phase wrap are caller-owned;
    /// this exact integer wave requires no sampled floating-point construction.</remarks>
    internal static byte FlashingShade(int index)
    {
        if (index is < 0 or >= FlashCount)
        {
            throw new InvalidDataException(
                $"Fireflea flashing index {index} is outside the {FlashCount}-entry native cycle.");
        }

        return (byte)(6 - Math.Abs(index - 6));
    }

    /// <summary>Returns the capped six-step darkness shade level, plus the level read past the ramp at offset 12.</summary>
    /// <remarks>Independently checked for #1165 against all seven original physical reads
    /// and pinned bank_88.asm. Even byte offsets 0..10 use min(3*offset,25).
    /// Offset 12 reads past the ramp, not an extrapolated ramp value. The death
    /// counter permits that offset. Reject odd offsets and offsets above 12; retain the
    /// caller's wrapped flash-plus-darkness level addition.</remarks>
    internal static byte DarknessShade(int byteOffset)
    {
        if ((byteOffset & 1) != 0 || byteOffset is < 0 or > 12)
        {
            throw new InvalidDataException(
                $"Fireflea darkness byte offset {byteOffset} is outside the retail 0-through-12 even domain.");
        }

        return byteOffset == 12 ? PastRampShadeLevel : (byte)Math.Min(3 * byteOffset, 25);
    }

}
