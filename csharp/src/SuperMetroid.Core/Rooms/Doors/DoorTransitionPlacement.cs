namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Final native door-scroll camera and 16.16 Samus coordinates.</summary>
internal readonly record struct DoorTransitionPlacement(
    Position16<RoomPixels> CameraPosition,
    FixedPosition<RoomPixels> SamusPosition);
