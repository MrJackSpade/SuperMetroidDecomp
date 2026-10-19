namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// The codec of Crocomire's fight-flag word: the saturating hit count the cartridge keeps in the word's low
/// nibble beside the independently set latch bits. It is the one place that reads the nibble out of the flags
/// and the one place that turns the flags into the stored word, so the field layout is owned here.
/// </summary>
[SnesEncodingOwner]
public static class CrocomireFightFlagBits
{
    /// <summary>The highest count the nibble reaches.</summary>
    private const int MaximumHitCount = 15;

    /// <summary>The hit count stored in the low nibble.</summary>
    public static int HitCount(this CrocomireFightFlags flags) => (int)(flags & CrocomireFightFlags.HitCountField);

    /// <summary>The count plus one, saturating at fifteen: the value both mouth-hit routines compute.</summary>
    public static int NextHitCount(this CrocomireFightFlags flags) => Math.Min(flags.HitCount() + 1, MaximumHitCount);

    /// <summary>A hit count placed in its nibble so it can be combined with the latches.</summary>
    public static CrocomireFightFlags AsHitCountField(int count) =>
        (CrocomireFightFlags)Ensure.BetweenInclusive(0, MaximumHitCount, count);

    /// <summary>The latches and hit count as the single word the body slot stores them in.</summary>
    public static ushort ToNativeWord(this CrocomireFightFlags flags) => (ushort)flags;
}
