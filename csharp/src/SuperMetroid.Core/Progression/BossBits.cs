namespace SuperMetroid.Core.Progression;

/// <summary>
/// Bits in each area's byte at SRAM mirror <c>$7E:D828-$7E:D82F</c>.
/// </summary>
[Flags]
public enum BossBits : byte
{
    /// <summary>Empty boss-state mask: setting or clearing it leaves the area unchanged, and querying it reports no matching boss.</summary>
    None = 0,

    /// <summary>Kraid, Phantoon, Draygon, or either Ridley.</summary>
    AreaBoss = 1 << 0,

    /// <summary>Spore Spawn, Botwoon, Crocomire, or Mother Brain.</summary>
    AreaMiniBoss = 1 << 1,

    /// <summary>Bomb Torizo or Golden Torizo.</summary>
    AreaTorizo = 1 << 2,
}
