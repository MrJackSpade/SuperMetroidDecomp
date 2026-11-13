using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The room object that closes the door behind Samus, selected by the door's closing behaviour and travel direction.</summary>
public static class DoorClosingObjects
{
    /// <summary>
    /// The closer for <paramref name="orientation"/>: none for a door that does not close, the blue cap facing the travel
    /// direction, or the escape gate (whatever the direction).
    /// </summary>
    internal static DoorClosingObjectDefinition? GetDefinition(DoorOrientation orientation) => orientation.Closing switch
    {
        DoorClosingBehavior.None => null,
        DoorClosingBehavior.BlueDoorCloses => orientation.Direction switch
        {
            DoorDirection.Right => new(ColoredDoorOrientation.Right),
            DoorDirection.Left => new(ColoredDoorOrientation.Left),
            DoorDirection.Down => new(ColoredDoorOrientation.Down),
            DoorDirection.Up => new(ColoredDoorOrientation.Up),
            _ => throw new ArgumentOutOfRangeException(nameof(orientation), orientation, "Undefined door direction."),
        },
        DoorClosingBehavior.EscapeGateCloses => new(BlueCapFacing: null),
        _ => throw new ArgumentOutOfRangeException(nameof(orientation), orientation, "Undefined door closing behavior."),
    };
}
