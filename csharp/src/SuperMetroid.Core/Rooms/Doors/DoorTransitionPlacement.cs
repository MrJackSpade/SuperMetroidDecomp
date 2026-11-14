namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Final door-scroll camera and 1/65536-pixel Samus coordinates.</summary>
internal readonly record struct DoorTransitionPlacement(
    Position16<RoomPixels> CameraPosition,
    FixedPosition<RoomPixels> SamusPosition);
