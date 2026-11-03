namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>Cartridge values of <see cref="WorkRobotPaletteTimingDefinitions"/> that only verification reads.</summary>
internal static class WorkRobotPaletteTimingDefinitionsConstants
{
    /// <summary>First timer word, at $A8:CCC9, in the six ten-byte palette records.</summary>
    public const int NativeFirstTimerAddress = 0xa8ccc9;
    /// <summary>The negative wrap marker at $A8:CCFD following the six records.</summary>
    public const int NativeTerminatorAddress = 0xa8ccfd;
    /// <summary>Four colour words plus one timer word per native record (#1512: the port counts records, not bytes).</summary>
    public const int NativeRecordByteCount = 10;

    /// <summary>Provides the native Work Robot palette timer start, terminator and record size.</summary>
    extension(WorkRobotPaletteTimingDefinitions)
    {
        /// <inheritdoc cref="WorkRobotPaletteTimingDefinitionsConstants.NativeFirstTimerAddress"/>
        internal static int NativeFirstTimerAddress => WorkRobotPaletteTimingDefinitionsConstants.NativeFirstTimerAddress;
        /// <inheritdoc cref="WorkRobotPaletteTimingDefinitionsConstants.NativeTerminatorAddress"/>
        internal static int NativeTerminatorAddress => WorkRobotPaletteTimingDefinitionsConstants.NativeTerminatorAddress;
        /// <inheritdoc cref="WorkRobotPaletteTimingDefinitionsConstants.NativeRecordByteCount"/>
        internal static int RecordByteCount => NativeRecordByteCount;
    }

    /// <summary>The native record-offset reading of the glow's durations, as the cartridge indexes them.</summary>
    extension(WorkRobotPaletteTiming)
    {
        /// <summary>The duration at native byte offset <paramref name="byteOffset"/>; offsets are validated before division.</summary>
        /// <exception cref="InvalidDataException"><paramref name="byteOffset"/> is not one of the six record offsets.</exception>
        internal static ushort DurationForByteOffset(int byteOffset)
        {
            if (byteOffset % NativeRecordByteCount != 0 || byteOffset < 0 ||
                byteOffset >= WorkRobotPaletteTimingDefinitions.RecordCount * NativeRecordByteCount)
            {
                throw new InvalidDataException(
                    $"Work Robot palette timing offset {byteOffset} is not one of the " +
                    $"{WorkRobotPaletteTimingDefinitions.RecordCount} authored {NativeRecordByteCount}-byte records.");
            }
            return checked((ushort)WorkRobotPaletteTiming.DurationOf(byteOffset / NativeRecordByteCount));
        }
    }

    /// <summary>
    /// Resolves the native terminator offset back to record zero and rejects every other
    /// non-record offset instead of interpreting adjacent bank-$A8 code as palette data.
    /// </summary>
    public static ushort NormalizeByteOffset(ushort byteOffset)
    {
        if (byteOffset == WorkRobotPaletteTimingDefinitions.RecordCount * NativeRecordByteCount)
            return 0;

        _ = WorkRobotPaletteTiming.DurationForByteOffset(byteOffset);
        return byteOffset;
    }
}
