using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The PLM that closes the door behind Samus, selected by the door's closing behaviour and travel direction.</summary>
public static class DoorClosingPlms
{
    /// <summary>
    /// The closer for <paramref name="orientation"/>: none for a door that does not close, the blue closer facing the
    /// travel direction, or the escape gate (whatever the direction).
    /// </summary>
    internal static DoorClosingPlmDefinition? GetDefinition(CartridgeDoorOrientation orientation) => orientation.Closing switch
    {
        DoorClosingBehavior.None => null,
        DoorClosingBehavior.BlueDoorCloses => orientation.Direction switch
        {
            DoorDirection.Right => new(PlmHeaderId.BlueDoorClosingFacingRight, PlmProgramStart.Typed(BlueDoorPlmProgramDefinitions.Closing(ColoredDoorOrientation.Right))),
            DoorDirection.Left => new(PlmHeaderId.BlueDoorClosingFacingLeft, PlmProgramStart.Typed(BlueDoorPlmProgramDefinitions.Closing(ColoredDoorOrientation.Left))),
            DoorDirection.Down => new(PlmHeaderId.BlueDoorClosingFacingDown, PlmProgramStart.Typed(BlueDoorPlmProgramDefinitions.Closing(ColoredDoorOrientation.Down))),
            DoorDirection.Up => new(PlmHeaderId.BlueDoorClosingFacingUp, PlmProgramStart.Typed(BlueDoorPlmProgramDefinitions.Closing(ColoredDoorOrientation.Up))),
            _ => throw new ArgumentOutOfRangeException(nameof(orientation), orientation, "Undefined door direction."),
        },
        DoorClosingBehavior.EscapeGateCloses => new(PlmHeaderId.MotherBrainEscapeRoomGateClosing,
            PlmProgramStart.Typed(ScriptedRoomPlmProgram.MotherBrainEscapeGateClosing)),
        _ => throw new ArgumentOutOfRangeException(nameof(orientation), orientation, "Undefined door closing behavior."),
    };
}
