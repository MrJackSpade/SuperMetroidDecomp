using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Cartridge table semantics for <c>$8F:E68A Door_Closing_PLMs</c>.
/// </summary>
/// <remarks>
/// The table is indexed by the whole orientation byte: <see cref="CartridgeDoorOrientation"/>
/// decodes it into its closing behavior (bits 2-3) and travel direction (bits 0-1), and
/// rejects bytes beyond the twelve entries when the header is loaded.
/// </remarks>
public static class DoorClosingPlmRomData
{





    /// <summary>
    /// The <c>$8F:E68A</c> entry for <paramref name="orientation"/>: none for the four
    /// non-closing entries, the blue closer facing the travel direction, or the escape gate
    /// (all four escape entries name the same PLM). No persistent table/cache remains.
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
