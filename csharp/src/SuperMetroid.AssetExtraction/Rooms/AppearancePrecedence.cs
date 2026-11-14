namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Which appearance input's artwork shows where several reach the same characters (#1406, #1466), first to last: an
/// animation's selected frame shows over anything bound there; Kraid's backdrop, bound as he rises, over the room's load;
/// an encounter's look, Ceres Ridley's in his arena, over what the arena loaded. A collectible is pictured as its kind alone (#1538), by
/// no input.
/// </summary>
internal enum AppearancePrecedence
{
    /// <summary>An animation's selected frame.</summary>
    Animation,

    /// <summary>Kraid's backdrop.</summary>
    KraidBackdrop,

    /// <summary>An encounter's look, as Ceres Ridley's over his arena's blocks.</summary>
    EncounterLook,
}
