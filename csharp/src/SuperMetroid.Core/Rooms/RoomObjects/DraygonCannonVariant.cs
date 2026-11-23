namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Which wall a Draygon cannon faces out from and whether it starts destroyed (#2400), which choose its kind (#2557).</summary>
/// <param name="Orientation">The wall the cannon faces out from.</param>
/// <param name="Start">Whether the cannon starts whole or already destroyed.</param>
internal readonly partial record struct DraygonCannonVariant(DraygonCannonOrientation Orientation, DraygonCannonStart Start);
