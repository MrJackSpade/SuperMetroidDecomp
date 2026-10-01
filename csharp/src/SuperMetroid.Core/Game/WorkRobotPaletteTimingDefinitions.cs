namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled cadence and wrap metadata for the Work Robot palette cycle. The four colors
/// in each record remain presentation data; this catalog owns the engine timing consumed
/// by <c>AnimatePalette</c> at <c>$A8:CC67</c>.
/// </summary>
internal static class WorkRobotPaletteTimingDefinitions
{
    /// <summary>First timer word, at $A8:CCC9, in the six ten-byte palette records.</summary>
    public const int NativeFirstTimerAddress = 0xa8ccc9;

    /// <summary>The negative wrap marker at $A8:CCFD following the six records.</summary>
    public const int NativeTerminatorAddress = 0xa8ccfd;

    /// <summary>Number of authored palette records before the terminator.</summary>
    public const ushort RecordCount = 6;

    /// <summary>Four color words plus one timer word per record.</summary>
    public const ushort RecordByteCount = 10;

    /// <summary>Returns the duration for one aligned authored palette-record offset.</summary>
    /// <remarks>
    /// Independently reviewed for #1165 against NTSC J/U v1.0 and pinned bank_A8.asm:
    /// the six unsigned timer words at $A8:CCC9+10*i repeat a three-record cadence,
    /// 64 ticks at i%3==0 and 16 otherwise. Validate offsets before division/modulo;
    /// only 0,10,20,30,40,50 are durations. $A8:CCFD is the separate color terminator,
    /// resolved by NormalizeByteOffset at offset 60, never a seventh duration.
    /// </remarks>
    public static ushort DurationForByteOffset(ushort byteOffset)
    {
        if (byteOffset % RecordByteCount != 0 ||
            byteOffset >= RecordCount * RecordByteCount)
        {
            throw new InvalidDataException(
                $"Work Robot palette timing offset {byteOffset} is not one of the " +
                $"{RecordCount} authored {RecordByteCount}-byte records.");
        }

        int record = byteOffset / RecordByteCount;
        return (ushort)(record % 3 == 0 ? 64 : 16);
    }

    /// <summary>
    /// Resolves the native terminator offset back to record zero and rejects every other
    /// non-record offset instead of interpreting adjacent bank-$A8 code as palette data.
    /// </summary>
    public static ushort NormalizeByteOffset(ushort byteOffset)
    {
        if (byteOffset == RecordCount * RecordByteCount)
            return 0;

        _ = DurationForByteOffset(byteOffset);
        return byteOffset;
    }
}
