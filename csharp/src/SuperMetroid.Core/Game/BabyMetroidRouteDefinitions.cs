namespace SuperMetroid.Core.Game;

/// <summary>
/// One compiled <c>$A9:CA24-$CA63</c> Baby Metroid ceiling-to-Samus route record.
/// </summary>
/// <param name="TargetX">Horizontal waypoint coordinate pursued by this route leg.</param>
/// <param name="TargetY">Vertical waypoint coordinate pursued by this route leg.</param>
/// <param name="AccelerationDivisorIndex">Index selecting the native acceleration divisor; authored records use zero.</param>
/// <param name="MovementFunction">Native callback pointer that applies the leg's wrong-way speed adjustment.</param>
/// <param name="FollowingWord">Word read beyond the record, which supplies the next waypoint X or terminal callback.</param>
internal readonly record struct BabyMetroidRouteRecord(
    ushort TargetX,
    ushort TargetY,
    ushort AccelerationDivisorIndex,
    ushort MovementFunction,
    ushort FollowingWord);

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

    /// <summary><c>$A9:F45F</c>, gradual acceleration with wrong-way extra <c>$0008</c>.</summary>
    internal const ushort GradualAccelerationExtraEightFunction = 0xf45f;

    /// <summary><c>$A9:F466</c>, gradual acceleration with wrong-way extra <c>$0010</c>.</summary>
    internal const ushort GradualAccelerationExtraSixteenFunction = 0xf466;

    /// <summary><c>$A9:CA66</c>, the route's terminal latch-onto-Samus AI function.</summary>
    internal const ushort LatchOntoSamusFunction = 0xca66;

    /// <summary>
    /// The eight authored waypoints. Each native record is the waypoint, a zero acceleration
    /// divisor index and the movement callback; the cartridge reads one word beyond each record,
    /// so its following word is the next waypoint's X, and the last one's is the adjacent
    /// <c>$CA66</c> callback word. The first five legs use the wrong-way extra-$10 callback and
    /// the final three the extra-$08 callback.
    /// </summary>
    private static ReadOnlySpan<ushort> WaypointX => [0x00a0, 0x0130, 0x00c0, 0x00c0, 0x00e0, 0x00cd, 0x00cc, 0x00cb];
    /// <summary>Vertical coordinates paired by index with the eight authored ceiling-to-Samus waypoints.</summary>
    private static ReadOnlySpan<ushort> WaypointY => [0x0078, 0x007a, 0x0040, 0x0070, 0x0080, 0x0090, 0x00a0, 0x00b0];

    /// <summary>First route-leg index using the wrong-way extra-$0008 movement callback.</summary>
    private const int FirstExtraEightLeg = 5;

    /// <summary>Assembles one native route record, including its overlapping next-X or terminal-function word.</summary>
    /// <param name="index">Zero-based index of an authored waypoint.</param>
    /// <returns>The compiled values consumed by the native route logic for that leg.</returns>
    private static BabyMetroidRouteRecord Record(int index) => new(
        WaypointX[index],
        WaypointY[index],
        0x0000,
        index < FirstExtraEightLeg ? GradualAccelerationExtraSixteenFunction : GradualAccelerationExtraEightFunction,
        index + 1 < RecordCount ? WaypointX[index + 1] : LatchOntoSamusFunction);

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
    internal static ushort GetWrongWayOffScreenXSpeed(ushort movementFunction) =>
        movementFunction switch
        {
            GradualAccelerationExtraEightFunction => 0x0008,
            GradualAccelerationExtraSixteenFunction => 0x0010,
            _ => throw new InvalidDataException(
                $"Baby route names unknown movement function ${movementFunction:X4}."),
        };

    /// <summary>Creates the data error used when a pointer does not identify an aligned authored route record.</summary>
    /// <param name="pointer">The bank-relative route pointer that failed validation.</param>
    /// <returns>An exception describing the unsupported pointer range or alignment.</returns>
    private static InvalidDataException InvalidPointer(ushort pointer) => new(
        $"Baby route pointer ${pointer:X4} is not an authored $A9:CA24-$A9:CA63 record.");
}
