namespace SuperMetroid.Core.Rooms;

/// <summary>One resident door header and the secondary list used while entering its room.</summary>
internal readonly record struct ResidentDoorClosingDefinition(
    ushort Header,
    ushort ClosingInstructionList);

/// <summary>
/// Fixed secondary-list metadata selected by <c>$82:E8EB Spawn_Door_Closing_PLM</c>
/// for resident grey/coloured/eye doors and the Mother Brain escape gate.
/// </summary>
/// <remarks>
/// These dispatcher identities select bounded compiled timer, sound, draw and branch
/// programs. Neither selecting nor executing a list reads an executable cartridge header.
/// </remarks>
public static class ResidentDoorClosingDefinitions
{
    /// <summary>Number of retail resident door/gate headers with a secondary closing list.</summary>
    public const int Count = 20;

    /// <summary>$84:BA4C, Bomb Torizo's Bomb-gated right-facing closing program.</summary>
    internal const ushort BombTorizoGreyDoor = 0xba4c;

    /// <summary>$84:BE59, ordinary grey door facing left closing program.</summary>
    internal const ushort GreyFacingLeft = 0xbe59;

    /// <summary>$84:BEC2, ordinary grey door facing right closing program.</summary>
    internal const ushort GreyFacingRight = 0xbec2;

    /// <summary>$84:BF2B, ordinary grey door facing up closing program.</summary>
    internal const ushort GreyFacingUp = 0xbf2b;

    /// <summary>$84:BF94, ordinary grey door facing down closing program.</summary>
    internal const ushort GreyFacingDown = 0xbf94;

    /// <summary>$84:BFFD, yellow door facing left closing program.</summary>
    internal const ushort YellowFacingLeft = 0xbffd;

    /// <summary>$84:C060, yellow door facing right closing program.</summary>
    internal const ushort YellowFacingRight = 0xc060;

    /// <summary>$84:C0C3, yellow door facing up closing program.</summary>
    internal const ushort YellowFacingUp = 0xc0c3;

    /// <summary>$84:C122, yellow door facing down closing program.</summary>
    internal const ushort YellowFacingDown = 0xc122;

    /// <summary>$84:C185, green door facing left closing program.</summary>
    internal const ushort GreenFacingLeft = 0xc185;

    /// <summary>$84:C1E4, green door facing right closing program.</summary>
    internal const ushort GreenFacingRight = 0xc1e4;

    /// <summary>$84:C243, green door facing up closing program.</summary>
    internal const ushort GreenFacingUp = 0xc243;

    /// <summary>$84:C2A2, green door facing down closing program.</summary>
    internal const ushort GreenFacingDown = 0xc2a2;

    /// <summary>$84:C301, red door facing left closing program.</summary>
    internal const ushort RedFacingLeft = 0xc301;

    /// <summary>$84:C363, red door facing right closing program.</summary>
    internal const ushort RedFacingRight = 0xc363;

    /// <summary>$84:C3C5, red door facing up closing program.</summary>
    internal const ushort RedFacingUp = 0xc3c5;

    /// <summary>$84:C427, red door facing down closing program.</summary>
    internal const ushort RedFacingDown = 0xc427;

    /// <summary>Enumerates the twenty supported identities in original header order without cached records.</summary>
    internal static IEnumerable<ResidentDoorClosingDefinition> All
    {
        get
        {
            yield return new(RoomPlmHeaders.BombTorizoGreyDoor, Resolve(RoomPlmHeaders.BombTorizoGreyDoor));
            yield return new(RoomPlmHeaders.GreyDoorFacingLeft, Resolve(RoomPlmHeaders.GreyDoorFacingLeft));
            yield return new(RoomPlmHeaders.GreyDoorFacingRight, Resolve(RoomPlmHeaders.GreyDoorFacingRight));
            yield return new(RoomPlmHeaders.GreyDoorFacingUp, Resolve(RoomPlmHeaders.GreyDoorFacingUp));
            yield return new(RoomPlmHeaders.GreyDoorFacingDown, Resolve(RoomPlmHeaders.GreyDoorFacingDown));
            yield return new(RoomPlmHeaders.YellowDoorFacingLeft, Resolve(RoomPlmHeaders.YellowDoorFacingLeft));
            yield return new(RoomPlmHeaders.YellowDoorFacingRight, Resolve(RoomPlmHeaders.YellowDoorFacingRight));
            yield return new(RoomPlmHeaders.YellowDoorFacingUp, Resolve(RoomPlmHeaders.YellowDoorFacingUp));
            yield return new(RoomPlmHeaders.YellowDoorFacingDown, Resolve(RoomPlmHeaders.YellowDoorFacingDown));
            yield return new(RoomPlmHeaders.GreenDoorFacingLeft, Resolve(RoomPlmHeaders.GreenDoorFacingLeft));
            yield return new(RoomPlmHeaders.GreenDoorFacingRight, Resolve(RoomPlmHeaders.GreenDoorFacingRight));
            yield return new(RoomPlmHeaders.GreenDoorFacingUp, Resolve(RoomPlmHeaders.GreenDoorFacingUp));
            yield return new(RoomPlmHeaders.GreenDoorFacingDown, Resolve(RoomPlmHeaders.GreenDoorFacingDown));
            yield return new(RoomPlmHeaders.RedDoorFacingLeft, Resolve(RoomPlmHeaders.RedDoorFacingLeft));
            yield return new(RoomPlmHeaders.RedDoorFacingRight, Resolve(RoomPlmHeaders.RedDoorFacingRight));
            yield return new(RoomPlmHeaders.RedDoorFacingUp, Resolve(RoomPlmHeaders.RedDoorFacingUp));
            yield return new(RoomPlmHeaders.RedDoorFacingDown, Resolve(RoomPlmHeaders.RedDoorFacingDown));
            yield return new(RoomPlmHeaders.MotherBrainEscapeRoomGate, Resolve(RoomPlmHeaders.MotherBrainEscapeRoomGate));
            yield return new(RoomPlmHeaders.EyeDoorFacingRight, Resolve(RoomPlmHeaders.EyeDoorFacingRight));
            yield return new(RoomPlmHeaders.EyeDoorFacingLeft, Resolve(RoomPlmHeaders.EyeDoorFacingLeft));
        }
    }

    /// <summary>
    /// Semantic header cases select the original bank-84 header+4 word consumed
    /// by bank-82 E91C. Only these twenty resident grey/coloured/eye-door/gate identities
    /// are supported; all other ushort values throw, including blue collision PLMs.
    /// </summary>
    internal static ushort Resolve(ushort header) => header switch
    {
        RoomPlmHeaders.BombTorizoGreyDoor => BombTorizoGreyDoor,
        RoomPlmHeaders.GreyDoorFacingLeft => GreyFacingLeft,
        RoomPlmHeaders.GreyDoorFacingRight => GreyFacingRight,
        RoomPlmHeaders.GreyDoorFacingUp => GreyFacingUp,
        RoomPlmHeaders.GreyDoorFacingDown => GreyFacingDown,
        RoomPlmHeaders.YellowDoorFacingLeft => YellowFacingLeft,
        RoomPlmHeaders.YellowDoorFacingRight => YellowFacingRight,
        RoomPlmHeaders.YellowDoorFacingUp => YellowFacingUp,
        RoomPlmHeaders.YellowDoorFacingDown => YellowFacingDown,
        RoomPlmHeaders.GreenDoorFacingLeft => GreenFacingLeft,
        RoomPlmHeaders.GreenDoorFacingRight => GreenFacingRight,
        RoomPlmHeaders.GreenDoorFacingUp => GreenFacingUp,
        RoomPlmHeaders.GreenDoorFacingDown => GreenFacingDown,
        RoomPlmHeaders.RedDoorFacingLeft => RedFacingLeft,
        RoomPlmHeaders.RedDoorFacingRight => RedFacingRight,
        RoomPlmHeaders.RedDoorFacingUp => RedFacingUp,
        RoomPlmHeaders.RedDoorFacingDown => RedFacingDown,
        RoomPlmHeaders.MotherBrainEscapeRoomGate => RoomPlmInstructionLists.MotherBrainEscapeRoomGateClosing,
        // $84:DB4C/DB5A carry $84:AAE3 as their secondary list: $82:E91C deletes a resident
        // eye door on entry instead of animating a closing cap.
        RoomPlmHeaders.EyeDoorFacingRight => RoomPlmInstructionLists.Delete,
        RoomPlmHeaders.EyeDoorFacingLeft => RoomPlmInstructionLists.Delete,
        _ => throw new InvalidDataException(
            $"Resident door header $84:{header:X4} has no compiled closing definition."),
    };
}