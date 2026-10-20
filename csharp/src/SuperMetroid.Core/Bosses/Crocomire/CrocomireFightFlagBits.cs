namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// The codec of Crocomire's fight-flag word: the saturating hit count the cartridge keeps in the word's low
/// nibble beside the independently set latch bits. It is the one place that splits the word into the latches
/// and the count and the one place that combines them, so the field layout is owned here.
/// </summary>
[SnesEncodingOwner]
public static class CrocomireFightFlagBits
{
    /// <summary>The highest count the nibble reaches.</summary>
    public const int MaximumHitCount = 15;

    /// <summary>The low-nibble hit count field of the word.</summary>
    private const ushort HitCountMask = 0x000f;

    /// <summary>The count plus one, saturating at fifteen: the value both mouth-hit routines compute.</summary>
    public static int NextHitCount(int hitCount) => Math.Min(hitCount + 1, MaximumHitCount);

    /// <summary>The latch bits of a stored word.</summary>
    public static CrocomireFightFlags Flags(ushort word) => (CrocomireFightFlags)(word & ~HitCountMask);

    /// <summary>The hit count of a stored word.</summary>
    public static int HitCount(ushort word) => word & HitCountMask;

    /// <summary>The latches and hit count as the single word the body slot stores them in.</summary>
    public static ushort ToNativeWord(CrocomireFightFlags flags, int hitCount) =>
        (ushort)((ushort)flags | Ensure.BetweenInclusive(0, MaximumHitCount, hitCount));
}
