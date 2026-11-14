namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// The behaviours the dead Torizo corpse's main AI dispatches.
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

    /// <summary>Leaves the finished corpse slot inert.</summary>
    NoOperation,
}
