namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// The behaviours the dead Torizo corpse's main AI runs.
/// Its values are arbitrary.
/// </summary>
internal enum DeadTorizoPhase
{
    /// <summary>Holds the intact corpse before its contact or shot trigger.</summary>
    Wait,

    /// <summary>Counts the contact delay before entering the rotting phase.</summary>
    PreRot,

    /// <summary>Rots the corpse row by row while its sand heap forms beneath it.</summary>
    Rotting,

    /// <summary>Has finished rotting: the corpse does nothing more.</summary>
    Finished,
}
