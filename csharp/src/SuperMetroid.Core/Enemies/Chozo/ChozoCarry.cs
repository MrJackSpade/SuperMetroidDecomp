using static SuperMetroid.Core.Enemies.Chozo.ChozoCarryMotionDefinitions;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>A carrying Chozo statue's step: its velocity and where it holds Samus, derived from its planted-foot stride.</summary>
public static class ChozoCarry
{
    /// <summary>Derives the authored statue stride burst from adjacent planted-foot positions.</summary>
    private static short Magnitude(int local)
    {
        if (local is < 4 or >= 12) return 0;
        int phase = local % 4;
        int pixels = phase == 0 ? WeightShiftAdvance :
            ChozoStride.SupportFootX(phase) -
            ChozoStride.SupportFootX(phase - 1) +
            (phase == 3 ? FinalPlantSlide : 0);
        return (short)EightEight.FromWhole(pixels);
    }
    /// <summary>
    /// $AA:E6B0: Samus's vertical hand offset, shared by both facing halves.
    /// The two acquisition/release poses place Samus32 and25 pixels above the statue.
    /// The carried pose is23 pixels above, with a two-pixel triangular bob during the
    /// two four-pose stride cycles at local indices4..11. Remaining poses hold still.
    /// Native movement instructions in $AA:E4C1-$E54D traverse those stride poses;
    /// $AA:E55B-$E573 reverses the acquisition poses when releasing Samus.
    /// </summary>
    private static short SamusYOffset(int local)
    {
        if (local == 0)
            return -32;
        if (local == 1)
            return -25;
        int bob = local is >= 4 and < 12 ? 2 - Math.Abs(2 - local % 4) : 0;
        return (short)(-23 - bob);
    }
    /// <summary>$AA:E630/E670/E6B0: one of 32 velocity/X-joint/Y-joint records, selected by its record number.</summary>
    /// <remarks>
    /// The carried-Samus X field at <c>$AA:E670</c> is exactly
    /// <c>sign * (local == 0 ? 28 : local == 1 ? 30 : 32)</c>, where
    /// <c>local = record % 16</c> and sign is negative for the
    /// first sixteen records, positive for the second. All 32 native words
    /// match the pinned NTSC J/U v1.0 ROM. The caller adds this signed whole
    /// pixel offset after moving the statue, with 16-bit position wrapping.
    /// Records outside the table are rejected. Investigation: #625 / #666.
    /// </remarks>
    public static ChozoCarryStepDefinition Read(int record)
    {
        if (record is < 0 or >= ChozoCarryMotionDefinitions.RecordCount)
            throw new InvalidDataException($"Chozo carry record {record} is outside its {ChozoCarryMotionDefinitions.RecordCount} records.");
        int index = record;
        int local = index % 16;
        int sign = index < 16 ? -1 : 1;
        int x = local == 0 ? 28 : local == 1 ? 30 : 32;
        return new((short)(sign * Magnitude(local)), new((short)(sign * x), SamusYOffset(local)));
    }
}
