namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// An eye door's bottom (#1554), below its body: it pulses and leaves with the body but stands in no door, so Samus's
/// arrival through the door never asks it to close (#2493).
/// </summary>
/// <param name="orientation">The way the door faces.</param>
/// <param name="door">The door the bottom's placement named.</param>
internal sealed class EyeDoorBottomObject(EyeDoorOrientation orientation, PlacedDoor door)
    : EyeDoorPartObject(new EyeDoorPart(EyeDoorBodyPart.Bottom, orientation), door);
