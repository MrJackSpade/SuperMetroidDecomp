namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// High byte of the destroyable-flame spawn parameter: the even jump-table offset
/// of the flame's initializer. The low byte is the type-specific index.
/// </summary>
public enum PhantoonFlameSpawnType
{
    /// <summary>A casual flame that falls from Phantoon and bounces.</summary>
    Casual = 0,
    /// <summary>An enraged wave flame, indexed by direction.</summary>
    Enraged = 2,
    /// <summary>A rain flame, indexed by packed column and fall delay.</summary>
    Rain = 4,
    /// <summary>A spiral flame, indexed by direction 0-7.</summary>
    Spiral = 6,
}
