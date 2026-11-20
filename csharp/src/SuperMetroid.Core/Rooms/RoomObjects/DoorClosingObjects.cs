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
            DoorDirection.Right => new DoorClosingObjectDefinition.BlueCap(ColoredDoorOrientation.Right),
            DoorDirection.Left => new DoorClosingObjectDefinition.BlueCap(ColoredDoorOrientation.Left),
            DoorDirection.Down => new DoorClosingObjectDefinition.BlueCap(ColoredDoorOrientation.Down),
            DoorDirection.Up => new DoorClosingObjectDefinition.BlueCap(ColoredDoorOrientation.Up),
            _ => throw new ArgumentOutOfRangeException(nameof(orientation), orientation, "Undefined door direction."),
        },
        DoorClosingBehavior.EscapeGateCloses => DoorClosingObjectDefinition.EscapeGate,
        _ => throw new ArgumentOutOfRangeException(nameof(orientation), orientation, "Undefined door closing behavior."),
    };
}
