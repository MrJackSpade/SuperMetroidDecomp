namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// How long the drained Samus's rainbow holds each of its shades (#2387): one update as the rainbow starts, one update
/// longer each time the Baby slows it, up to the longest hold the Baby allows, and none once the rainbow stops.
/// </summary>
public readonly record struct RainbowGlowHold
{
    /// <summary>A hold of <paramref name="updates"/> updates.</summary>
    private RainbowGlowHold(int updates) => Updates = updates;

    /// <summary>No hold: the rainbow is not glowing.</summary>
    public static RainbowGlowHold None => default;

    /// <summary>The hold as the rainbow starts: each shade shows for one update.</summary>
    public static RainbowGlowHold Starting { get; } = new(1);

    /// <summary>The updates each shade is held for; zero when there is no rainbow.</summary>
    public int Updates { get; }

    /// <summary>This hold one update longer, stopping at <paramref name="longestHold"/> updates.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="longestHold"/> is below one.</exception>
    public RainbowGlowHold Lengthened(int longestHold) =>
        new(Math.Min(Updates + 1, Ensure.BetweenInclusive(1, int.MaxValue, longestHold)));
}
