namespace SuperMetroid.Core.Game;

/// <summary>The hit-count nibble that shares Crocomire's fight-flag word.</summary>
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
}
