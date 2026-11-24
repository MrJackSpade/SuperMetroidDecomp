namespace Engine.World;

/// <summary>The questions a <see cref="Condition"/> can ask.</summary>
public enum ConditionKind
{
    /// <summary>Has the named fact been recorded in progress?</summary>
    FactRecorded,

    /// <summary>Does this room have no live enemies?</summary>
    RoomHasNoEnemies,

    /// <summary>Does the player own the named piece of equipment?</summary>
    PlayerOwns,

    /// <summary>Is the named thing placed in this room gone: dead, or opened?</summary>
    PlacedThingGone,

    /// <summary>Are all of the listed conditions true?</summary>
    All,

    /// <summary>Is any of the listed conditions true?</summary>
    Any,

    /// <summary>Is none of the listed conditions true?</summary>
    None,

    /// <summary>The player can carry some of a named amount: they have found at least one of what holds it.</summary>
    PlayerCarries,

    /// <summary>Have at least so many enemies been killed in this room since it was set up? The number is what the condition is about.</summary>
    EnemiesKilled,

    /// <summary>Was this room entered at the named entry point?</summary>
    EnteredAt,
}
