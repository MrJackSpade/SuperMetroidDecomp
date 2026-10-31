using static SuperMetroid.Core.Fx.FirefleaFxDefinitions;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The Fireflea room's shade levels (<see cref="FirefleaFxDefinitions"/>): the triangular flashing wave and the capped darkness
/// ramp enemy deaths deepen.
/// </summary>
internal static class FirefleaShades
{
    /// <summary>Returns the triangular flashing shade level 6-abs(index-6), index 0..11.</summary>
    /// <remarks>Independently checked for #1165 against every original NTSC J/U v1.0
    /// word and pinned bank_88.asm. The six-frame timer and phase wrap are caller-owned;
    /// this exact integer wave requires no sampled floating-point construction.</remarks>
    internal static int FlashingShade(int index)
    {
        if (index is < 0 or >= FlashCount)
        {
            throw new InvalidDataException(
                $"Fireflea flashing index {index} is outside the {FlashCount}-entry native cycle.");
        }

        return WordWrap.LowByte(6 - Math.Abs(index - 6));
    }

    /// <summary>Returns the capped six-step darkness shade level, plus the level read past the ramp at offset 12.</summary>
    /// <remarks>Independently checked for #1165 against all seven original physical reads
    /// and pinned bank_88.asm. Even byte offsets 0..10 use min(3*offset,25).
    /// Offset 12 reads past the ramp, not an extrapolated ramp value. The death
    /// counter permits that offset. Reject odd offsets and offsets above 12; retain the
    /// caller's wrapped flash-plus-darkness level addition.</remarks>
    internal static int DarknessShade(int byteOffset)
    {
        if ((byteOffset & 1) != 0 || byteOffset is < 0 or > 12)
        {
            throw new InvalidDataException(
                $"Fireflea darkness byte offset {byteOffset} is outside the retail 0-through-12 even domain.");
        }

        return byteOffset == 12 ? PastRampShadeLevel : WordWrap.LowByte(Math.Min(3 * byteOffset, 25));
    }
}
