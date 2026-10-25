using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Decodes the fields of a bank-$83 door record into the door's domain values (#1414): the orientation byte into a
/// direction and closing behavior, and the Samus-distance word into a push speed.
/// </summary>
[CpuBusBoundary("codec: bank-$83 door record orientation byte and Samus-distance word → DoorOrientation, DoorSamusSpeed")]
internal static class DoorRecordCodec
{
    /// <summary>Rows of <c>Door_Closing_PLMs</c> ($8F:E68A); a larger orientation byte would index past the table.</summary>
    private const int OrientationCount = 12;

    /// <summary>Bits 0-1 of the orientation byte: the direction of travel.</summary>
    private const int DirectionMask = 3;

    /// <summary>Bits 2-3 of the orientation byte select the <c>Door_Closing_PLMs</c> row group.</summary>
    private const int ClosingShift = 2;

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
    /// Decodes the Samus-distance word: the door transition treats a negative word as the axis default and any other
    /// word as the push in 1/256-pixel units per step.
    /// </summary>
    internal static DoorSamusSpeed DecodeSamusSpeed(ushort word) => unchecked((short)word) switch
    {
        < 0 => DoorSamusSpeed.AxisDefault,
        short speed => DoorSamusSpeed.SubpixelsPerStep(speed),
    };
}
