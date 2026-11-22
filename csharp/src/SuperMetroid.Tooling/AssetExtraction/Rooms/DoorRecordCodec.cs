using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Decodes the fields of a bank-$83 door record into the door's domain values (#1414): the orientation byte into a
/// direction and closing behavior, the closing block into the block the arrival closes, the elevator-properties byte into
/// whether the door exchanges explored maps, and the Samus-distance word into a push speed.
/// </summary>
internal static class DoorRecordCodec
{
    /// <summary>Rows of <c>Door_Closing_PLMs</c> ($8F:E68A); a larger orientation byte would index past the table.</summary>
    private const int OrientationCount = 12;

    /// <summary>Bits 0-1 of the orientation byte: the direction of travel.</summary>
    private const int DirectionMask = 3;

    /// <summary>Bits 2-3 of the orientation byte select the <c>Door_Closing_PLMs</c> row group.</summary>
    private const int ClosingShift = 2;

    /// <summary>Bit 6 of the elevator-properties byte: passing the door moves Samus to another area's explored map.</summary>
    private const byte ExchangesExploredMapsBit = 0x40;

    /// <summary>Decodes an orientation byte, rejecting values outside <c>Door_Closing_PLMs</c>.</summary>
    internal static DoorOrientation DecodeOrientation(byte raw)
    {
        if (raw >= OrientationCount)
            throw new InvalidDataException($"Door orientation byte ${raw:X2} is outside Door_Closing_PLMs ($00-$0B).");
        DoorDirection direction = (raw & DirectionMask) switch
        {
            0 => DoorDirection.Right,
            1 => DoorDirection.Left,
            2 => DoorDirection.Down,
            _ => DoorDirection.Up,
        };
        DoorClosingBehavior closing = (raw >> ClosingShift) switch
        {
            0 => DoorClosingBehavior.None,
            1 => DoorClosingBehavior.BlueDoorCloses,
            _ => DoorClosingBehavior.EscapeGateCloses,
        };
        return new DoorOrientation(direction, closing);
    }

    /// <summary>
    /// Decodes the closing block at offsets four and five: a door that closes nothing stores block (0, 0) there, which
    /// names no block its arrival closes (#2558); any other door names the block its stored coordinates give.
    /// </summary>
    internal static Position16<RoomBlocks>? DecodeClosingBlock(DoorOrientation orientation, byte x, byte y) =>
        orientation.Closing ==DoorClosingBehavior.None && x == 0 && y == 0 ? null : new(x, y);

    /// <summary>
    /// Decodes the elevator-properties byte at offset two: a door whose byte carries <see cref="ExchangesExploredMapsBit"/>
    /// exchanges explored maps as Samus passes it (#1500).
    /// </summary>
    internal static bool DecodeExchangesExploredMaps(byte elevatorProperties) => (elevatorProperties & ExchangesExploredMapsBit) != 0;

    /// <summary>
    /// Decodes the Samus-distance word: the door transition treats a negative word as the axis default and any other
    /// word as the push in 1/256-pixel units per step.
    /// </summary>
    internal static DoorSamusSpeed DecodeSamusSpeed(ushort word) => unchecked((short)word) switch
    {
        < 0 => DoorSamusSpeed.AxisDefault,
        short speed => DoorSamusSpeed.PerStep(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(speed)),
    };
}
