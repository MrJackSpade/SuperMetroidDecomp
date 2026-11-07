using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The PLM that closes the door behind Samus, selected by the door's closing behaviour and travel direction.</summary>
public static class DoorClosingObjects
{
    /// <summary>
    /// The closer for <paramref name="orientation"/>: none for a door that does not close, the blue closer facing the
    /// travel direction, or the escape gate (whatever the direction).
    /// </summary>
    internal static DoorClosingObjectDefinition? GetDefinition(DoorOrientation orientation) => orientation.Closing switch
    {
        DoorClosingBehavior.None => null,
        DoorClosingBehavior.BlueDoorCloses => orientation.Direction switch
        {
            DoorDirection.Right => new(RoomObjectHeaderId.BlueDoorClosingFacingRight, RoomObjectProgramStart.Typed(BlueDoorObjectProgramDefinitions.ClosingPrograms[ColoredDoorOrientation.Right])),
            DoorDirection.Left => new(RoomObjectHeaderId.BlueDoorClosingFacingLeft, RoomObjectProgramStart.Typed(BlueDoorObjectProgramDefinitions.ClosingPrograms[ColoredDoorOrientation.Left])),
            DoorDirection.Down => new(RoomObjectHeaderId.BlueDoorClosingFacingDown, RoomObjectProgramStart.Typed(BlueDoorObjectProgramDefinitions.ClosingPrograms[ColoredDoorOrientation.Down])),
            DoorDirection.Up => new(RoomObjectHeaderId.BlueDoorClosingFacingUp, RoomObjectProgramStart.Typed(BlueDoorObjectProgramDefinitions.ClosingPrograms[ColoredDoorOrientation.Up])),
            _ => throw new ArgumentOutOfRangeException(nameof(orientation), orientation, "Undefined door direction."),
        },
        DoorClosingBehavior.EscapeGateCloses => new(RoomObjectHeaderId.MotherBrainEscapeRoomGateClosing,
            RoomObjectProgramStart.Typed(ScriptedRoomObjectProgram.MotherBrainEscapeGateClosing)),
        _ => throw new ArgumentOutOfRangeException(nameof(orientation), orientation, "Undefined door closing behavior."),
    };
}
