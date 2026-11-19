namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>What Crocomire's body is as he sinks, rests and rises in the acid during his death (#2540).</summary>
internal enum CrocomireBodyInAcid
{
    /// <summary>
    /// Still whole, before his first melt: his body shows its own frames, the acid's burning sound plays at its interval,
    /// and his hidden tongue is held at his body while he rests at the acid's floor.
    /// </summary>
    Whole,

    /// <summary>Melted once: his body shows his flesh's melting picture of the rows still above the acid, started afresh each update.</summary>
    MeltingFlesh,

    /// <summary>Melted to the skull: his body shows his bare skull's melting picture of the rows still above the acid, started afresh each update.</summary>
    MeltingBareSkull,
}
