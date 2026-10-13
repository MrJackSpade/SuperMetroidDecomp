namespace SuperMetroid.Core.Game;

/// <summary>
/// One compiled <c>$A9:CA24-$CA63</c> Baby Metroid ceiling-to-Samus route record.
/// </summary>
internal readonly record struct BabyMetroidRouteRecord(
    Position16<RoomPixels> Target,
    ushort AccelerationDivisorIndex,
    BabyMetroidRouteMovementFunction MovementFunction,
    ushort FollowingWord);

/// <summary>Bank-<c>$A9</c> movement callbacks named by the Baby Metroid route records.</summary>
internal enum BabyMetroidRouteMovementFunction : ushort
{
    /// <summary><c>$A9:F45F</c>, gradual acceleration with wrong-way extra <c>$0008</c>.</summary>
    GradualAccelerationExtraEight = 0xf45f,

    /// <summary><c>$A9:F466</c>, gradual acceleration with wrong-way extra <c>$0010</c>.</summary>
    GradualAccelerationExtraSixteen = 0xf466,
}

/// <summary>
/// Compiled gameplay route and callback identities used by the Baby Metroid after it
/// releases Mother Brain. These values control physical targets and acceleration; they
/// are engine definitions rather than editable sprite or animation data.
/// </summary>
internal static class BabyMetroidRouteDefinitions
{
    /// <summary><c>$A9:CA24</c>, first eight-byte ceiling-to-Samus route record.</summary>
    internal const ushort FirstRecordPointer = 0xca24;

    /// <summary>Native byte width of one overlapping route record.</summary>
    internal const ushort RecordStride = 8;

    /// <summary>Number of authored route records from <c>$A9:CA24</c> through <c>$A9:CA63</c>.</summary>
    internal const int RecordCount = 8;

    /// <summary><c>$A9:CA66</c>, the route's terminal latch-onto-Samus AI function.</summary>
    internal const ushort LatchOntoSamusFunction = 0xca66;

    /// <summary>
    /// The eight authored waypoints. Each native record is the waypoint, a zero acceleration
    /// divisor index and the movement callback; the cartridge reads one word beyond each record,
    /// so its following word is the next waypoint's X, and the last one's is the adjacent
    /// <c>$CA66</c> callback word. The first five legs use the wrong-way extra-$10 callback and
    /// the final three the extra-$08 callback.
    /// </summary>
    private static readonly Position16<RoomPixels>[] Waypoints =
    [
        new(0x00a0, 0x0078), new(0x0130, 0x007a), new(0x00c0, 0x0040), new(0x00c0, 0x0070),
        new(0x00e0, 0x0080), new(0x00cd, 0x0090), new(0x00cc, 0x00a0), new(0x00cb, 0x00b0),
    ];
    /// <summary>First route leg that switches from the extra-$10 to extra-$08 wrong-way correction.</summary>
    private const int FirstExtraEightLeg = 5;

    /// <summary>Combines an authored waypoint with its callback and the cartridge's overlapping following word.</summary>
    private static BabyMetroidRouteRecord Record(int index) => new(
        Waypoints[index],
        0x0000,
        index < FirstExtraEightLeg
            ? BabyMetroidRouteMovementFunction.GradualAccelerationExtraSixteen
            : BabyMetroidRouteMovementFunction.GradualAccelerationExtraEight,
        index + 1 < RecordCount ? Waypoints[index + 1].X : LatchOntoSamusFunction);

    /// <summary>Returns the exact authored record identified by its native bank-$A9 pointer.</summary>
    internal static BabyMetroidRouteRecord GetRecord(ushort pointer)
    {
        int offset = pointer - FirstRecordPointer;
        if (offset < 0 || offset % RecordStride != 0)
            throw InvalidPointer(pointer);

        int index = offset / RecordStride;
        if ((uint)index >= RecordCount)
            throw InvalidPointer(pointer);
        return Record(index);
    }

    /// <summary>Resolves a native movement callback to its wrong-way horizontal speed addition.</summary>
    internal static ushort GetWrongWayOffScreenXSpeed(BabyMetroidRouteMovementFunction movementFunction) =>
        movementFunction switch
        {
            BabyMetroidRouteMovementFunction.GradualAccelerationExtraEight => 0x0008,
            BabyMetroidRouteMovementFunction.GradualAccelerationExtraSixteen => 0x0010,
            _ => throw new InvalidDataException(
                $"Baby route names unknown movement function ${(int)movementFunction:X4}."),
        };

    /// <summary>Reports an unaligned or out-of-range bank-$A9 route pointer.</summary>
    private static InvalidDataException InvalidPointer(ushort pointer) => new(
        $"Baby route pointer ${pointer:X4} is not an authored $A9:CA24-$A9:CA63 record.");
}
