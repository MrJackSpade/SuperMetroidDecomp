namespace SuperMetroid.Core.Game;

/// <summary>
/// One compiled <c>$A9:CA24-$CA63</c> Baby Metroid ceiling-to-Samus route record.
/// </summary>
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

    /// <summary>First SNES address occupied by the route records.</summary>
    internal const int SourceAddress = 0xa9ca24;

    /// <summary>
    /// Complete source length, including the terminal <c>$CA66</c> word at
    /// <c>$A9:CA64-$CA65</c> observed through the final record's overlapping +8 read.
    /// </summary>
    internal const int SourceByteLength = 66;

    /// <summary><c>$A9:F45F</c>, gradual acceleration with wrong-way extra <c>$0008</c>.</summary>
    internal const ushort GradualAccelerationExtraEightFunction = 0xf45f;

    /// <summary><c>$A9:F466</c>, gradual acceleration with wrong-way extra <c>$0010</c>.</summary>
    internal const ushort GradualAccelerationExtraSixteenFunction = 0xf466;

    /// <summary><c>$A9:CA66</c>, the route's terminal latch-onto-Samus AI function.</summary>
    internal const ushort LatchOntoSamusFunction = 0xca66;

    /// <summary>
    /// Eight native records. For records zero through six, <c>FollowingWord</c> is the
    /// next record's X target because the cartridge reads one word beyond the record.
    /// The final value is the adjacent signed <c>$CA66</c> callback word.
    /// </summary>
    private static readonly BabyMetroidRouteRecord[] Records =
    [
        new(0x00a0, 0x0078, 0x0000, GradualAccelerationExtraSixteenFunction, 0x0130),
        new(0x0130, 0x007a, 0x0000, GradualAccelerationExtraSixteenFunction, 0x00c0),
        new(0x00c0, 0x0040, 0x0000, GradualAccelerationExtraSixteenFunction, 0x00c0),
        new(0x00c0, 0x0070, 0x0000, GradualAccelerationExtraSixteenFunction, 0x00e0),
        new(0x00e0, 0x0080, 0x0000, GradualAccelerationExtraSixteenFunction, 0x00cd),
        new(0x00cd, 0x0090, 0x0000, GradualAccelerationExtraEightFunction, 0x00cc),
        new(0x00cc, 0x00a0, 0x0000, GradualAccelerationExtraEightFunction, 0x00cb),
        new(0x00cb, 0x00b0, 0x0000, GradualAccelerationExtraEightFunction, LatchOntoSamusFunction),
    ];

    /// <summary>Returns the exact authored record identified by its native bank-$A9 pointer.</summary>
    internal static BabyMetroidRouteRecord GetRecord(ushort pointer)
    {
        int offset = pointer - FirstRecordPointer;
        if (offset < 0 || offset % RecordStride != 0)
            throw InvalidPointer(pointer);

        int index = offset / RecordStride;
        if ((uint)index >= RecordCount)
            throw InvalidPointer(pointer);
        return Records[index];
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

    private static InvalidDataException InvalidPointer(ushort pointer) => new(
        $"Baby route pointer ${pointer:X4} is not an authored $A9:CA24-$A9:CA63 record.");
}
