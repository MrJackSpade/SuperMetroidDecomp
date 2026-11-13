namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// What a grapple block does with its behaviour (#1722): whether the grapple beam holds onto it, breaks it or is rejected.
/// Its values are arbitrary; a behaviour written for another collision kind is read as one of these through
/// <see cref="RoomBlockBehavior"/>.
/// </summary>
public enum GrappleReaction
{
    /// <summary>A grapple block the beam holds onto without changing it; the X-ray scope reveals it.</summary>
    Anchor,

    /// <summary>A grapple block that breaks under the beam and grows back.</summary>
    RespawningBreakableAnchor,

    /// <summary>A grapple block that breaks under the beam for good.</summary>
    PermanentBreakableAnchor,

    /// <summary>A grapple block the beam holds onto without changing it, which the X-ray scope does not reveal.</summary>
    UnrevealedAnchor,

    /// <summary>A reaction past the end of the grapple table: the beam reaching it rejects it.</summary>
    PastTable,

    /// <summary>One of the reactions the room's area gives grapple blocks: the beam passes through it.</summary>
    Rejected,
}
