namespace SuperMetroid.Core.Rooms;

/// <summary>One direction-selected fallback door-closing PLM definition.</summary>
internal readonly record struct DoorClosingPlmDefinition(
    ushort Header,
    ushort InitialInstructionList);

/// <summary>
/// Cartridge table semantics for <c>$8F:E68A Door_Closing_PLMs</c>.
/// </summary>
/// <remarks>
/// The low two bits retain physical travel direction. Values zero through three are
/// non-closing doors, four through seven request a blue cap, and eight through eleven
/// request the Mother Brain escape gate. Named direction cases replace stored records;
/// the exact byte domain remains 0..11, with no masked or extrapolated inputs.
/// </remarks>
public static class DoorClosingPlmRomData
{

    /// <summary>Number of entries in the retail door-closing header table.</summary>
    public const int DirectionCount = 12;

    /// <summary>$84:C4CF, closing program selected by header $84:C8BE.</summary>
    internal const ushort BlueFacingRightInstructionList = 0xc4cf;

    /// <summary>$84:C49E, closing program selected by header $84:C8BA.</summary>
    internal const ushort BlueFacingLeftInstructionList = 0xc49e;

    /// <summary>$84:C531, closing program selected by header $84:C8C6.</summary>
    internal const ushort BlueFacingDownInstructionList = 0xc531;

    /// <summary>$84:C500, closing program selected by header $84:C8C2.</summary>
    internal const ushort BlueFacingUpInstructionList = 0xc500;

    /// <summary>
    /// Decode the twelve native direction cases at $8F:E68A. Zero through three
    /// select no actor, four through seven select oriented blue closers, and eight
    /// through eleven all select the escape gate. No persistent table/cache remains.
    /// </summary>
    internal static DoorClosingPlmDefinition GetDefinition(byte direction) => direction switch
    {
        < 4 => default,
        4 => new(RoomPlmHeaders.BlueDoorClosingFacingRight, BlueFacingRightInstructionList),
        5 => new(RoomPlmHeaders.BlueDoorClosingFacingLeft, BlueFacingLeftInstructionList),
        6 => new(RoomPlmHeaders.BlueDoorClosingFacingDown, BlueFacingDownInstructionList),
        7 => new(RoomPlmHeaders.BlueDoorClosingFacingUp, BlueFacingUpInstructionList),
        < DirectionCount => new(RoomPlmHeaders.MotherBrainEscapeRoomGateClosing,
            RoomPlmInstructionLists.MotherBrainEscapeRoomGateClosing),
        _ => throw new InvalidDataException(
            $"Door direction ${direction:X2} indexes beyond the " +
            $"{DirectionCount}-entry retail closing-PLM table."),
    };
}
