namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Where the door's scroll leaves the camera and Samus, her fraction of a pixel included.</summary>
internal readonly partial record struct DoorTransitionPlacement(
    Position16<RoomPixels> CameraPosition,
    FixedPosition<RoomPixels> SamusPosition);
