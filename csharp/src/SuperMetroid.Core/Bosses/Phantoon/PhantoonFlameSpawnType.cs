namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// High byte of the destroyable-flame spawn parameter: the even jump-table offset
/// of the flame's initializer. The low byte is the type-specific index.
/// </summary>
public enum PhantoonFlameSpawnType
{
    /// <summary>$00: a casual flame that falls from Phantoon and bounces.</summary>
    Casual = 0,
    /// <summary>$02: an enraged wave flame ($A7:D8C0/$D8D5), indexed by direction.</summary>
    Enraged = 2,
    /// <summary>$04: a rain flame, indexed by packed column and fall delay.</summary>
    Rain = 4,
    /// <summary>$06: a spiral flame, indexed by direction 0-7.</summary>
    Spiral = 6,
}
