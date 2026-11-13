namespace SuperMetroid.Core.Progression;

/// <summary>
/// The bosses defeated in one area: any of its area boss, its mini-boss and its Torizo. Progress keeps one for each area,
/// and a set names several bosses at once, as a room state or a statue that waits for any of them does. The values are
/// arbitrary and nothing reads them: the stored form and the sets that test it are the import's progression table codec's.
/// </summary>
[Flags]
public enum DefeatedBosses
{
    /// <summary>No boss: recording or forgetting it leaves the area unchanged, and asking for it finds no defeated boss.</summary>
    None = 0,

    /// <summary>Kraid, Phantoon, Draygon, or either Ridley.</summary>
    AreaBoss = 1 << 0,

    /// <summary>Spore Spawn, Botwoon, Crocomire, or Mother Brain.</summary>
    AreaMiniBoss = 1 << 1,

    /// <summary>Bomb Torizo or Golden Torizo.</summary>
    AreaTorizo = 1 << 2,
}
