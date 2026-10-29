namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Which appearance input's artwork shows where several reach the same characters (#1406, #1466), first to last: an
/// animation's selected frame shows over anything bound there; Kraid's backdrop, bound as he rises, over the collectibles
/// the room's PLMs bound as it loaded; an encounter's look over the sprites' artwork the room loaded.
/// </summary>
internal enum AppearancePrecedence
{
    /// <summary>An animation's selected frame.</summary>
    Animation,

    /// <summary>Kraid's backdrop.</summary>
    KraidBackdrop,

    /// <summary>An encounter's look on the sprites' artwork.</summary>
    EncounterLook,

    /// <summary>A collectible in its graphics slot.</summary>
    CollectibleSlot,
}
