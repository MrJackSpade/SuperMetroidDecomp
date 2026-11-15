namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// The phases a dead Torizo's corpse passes through, from lying intact to having rotted away.
/// Its values are arbitrary.
/// </summary>
internal enum DeadTorizoPhase
{
    /// <summary>Holds the intact corpse before its contact or shot trigger.</summary>
    Wait,

    /// <summary>Counts the contact delay before entering the rotting phase.</summary>
    PreRot,

    /// <summary>Advances row rot, sand-line copies, and the phases that show the rotted rows.</summary>
    Rotting,

    /// <summary>Has finished rotting: the corpse does nothing more.</summary>
    Finished,
}
