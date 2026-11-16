namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Which kind of destroyable flame Phantoon spawns; the spawn also names the flame's direction, or its column and fall delay. Its values are arbitrary.</summary>
public enum PhantoonFlameSpawnType
{
    /// <summary>A casual flame that falls from Phantoon and bounces.</summary>
    Casual,
    /// <summary>An enraged wave flame, indexed by direction.</summary>
    Enraged,
    /// <summary>A rain flame, which names the column it falls in and its fall delay.</summary>
    Rain,
    /// <summary>A spiral flame, indexed by direction 0-7.</summary>
    Spiral,
}
