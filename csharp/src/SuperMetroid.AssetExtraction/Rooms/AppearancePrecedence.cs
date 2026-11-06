namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Which appearance input's artwork shows where several reach the same characters (#1406, #1466), first to last: an
/// animation's selected frame shows over anything bound there; Kraid's backdrop, bound as he rises, over the room's load;
/// an encounter's look over the sprites' artwork the room loaded. A collectible is pictured only as itself, from its own
/// graphics alone (#1513), so its precedence orders it among nothing else.
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
