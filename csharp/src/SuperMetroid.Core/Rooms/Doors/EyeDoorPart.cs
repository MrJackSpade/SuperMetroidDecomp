namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>An eye door's body or bottom and the way its door faces (#2481).</summary>
/// <param name="BodyPart">Which part of the door's body it is: its middle or its bottom.</param>
/// <param name="Orientation">The way the door faces.</param>
internal readonly partial record struct EyeDoorPart(EyeDoorBodyPart BodyPart, EyeDoorOrientation Orientation);
