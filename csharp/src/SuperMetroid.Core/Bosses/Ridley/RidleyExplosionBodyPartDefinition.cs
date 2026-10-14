namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>One facing-specific body fragment placement and the pose it shows.</summary>
internal readonly record struct RidleyExplosionBodyPartDefinition(
    Offset16<RoomPixels> Offset,
    RidleyBreakupFrame Frame);
