namespace SuperMetroid.Core.Rooms;

/// <summary>One direction-selected fallback door-closing PLM definition.</summary>
internal readonly record struct DoorClosingPlmDefinition(
    PlmHeaderId Header,
    ushort InitialInstructionList);

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

    /// <summary>$84:C4CF, closing program selected by header $84:C8BE.</summary>
    internal const ushort BlueFacingRightInstructionList = 0xc4cf;

    /// <summary>$84:C49E, closing program selected by header $84:C8BA.</summary>
    internal const ushort BlueFacingLeftInstructionList = 0xc49e;

    /// <summary>$84:C531, closing program selected by header $84:C8C6.</summary>
    internal const ushort BlueFacingDownInstructionList = 0xc531;

    /// <summary>$84:C500, closing program selected by header $84:C8C2.</summary>
    internal const ushort BlueFacingUpInstructionList = 0xc500;

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
            DoorDirection.Right => new(PlmHeaderId.BlueDoorClosingFacingRight, BlueFacingRightInstructionList),
            DoorDirection.Left => new(PlmHeaderId.BlueDoorClosingFacingLeft, BlueFacingLeftInstructionList),
            DoorDirection.Down => new(PlmHeaderId.BlueDoorClosingFacingDown, BlueFacingDownInstructionList),
            DoorDirection.Up => new(PlmHeaderId.BlueDoorClosingFacingUp, BlueFacingUpInstructionList),
            _ => throw new ArgumentOutOfRangeException(nameof(orientation), orientation, "Undefined door direction."),
        },
        DoorClosingBehavior.EscapeGateCloses => new(PlmHeaderId.MotherBrainEscapeRoomGateClosing,
            RoomPlmInstructionLists.MotherBrainEscapeRoomGateClosing),
        _ => throw new ArgumentOutOfRangeException(nameof(orientation), orientation, "Undefined door closing behavior."),
    };
}
