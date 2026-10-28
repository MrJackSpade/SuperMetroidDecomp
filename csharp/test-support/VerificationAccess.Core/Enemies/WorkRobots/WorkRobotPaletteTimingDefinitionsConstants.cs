
namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>Cartridge values of <see cref="WorkRobotPaletteTimingDefinitions"/> that only verification reads.</summary>
internal static class WorkRobotPaletteTimingDefinitionsConstants
{
    /// <summary>First timer word, at $A8:CCC9, in the six ten-byte palette records.</summary>
    public const int NativeFirstTimerAddress = 0xa8ccc9;
    /// <summary>The negative wrap marker at $A8:CCFD following the six records.</summary>
    public const int NativeTerminatorAddress = 0xa8ccfd;

    /// <summary>Provides the native Work Robot palette timer start and terminator addresses.</summary>
    extension(WorkRobotPaletteTimingDefinitions)
    {
        /// <inheritdoc cref="WorkRobotPaletteTimingDefinitionsConstants.NativeFirstTimerAddress"/>
        internal static int NativeFirstTimerAddress => WorkRobotPaletteTimingDefinitionsConstants.NativeFirstTimerAddress;
        /// <inheritdoc cref="WorkRobotPaletteTimingDefinitionsConstants.NativeTerminatorAddress"/>
        internal static int NativeTerminatorAddress => WorkRobotPaletteTimingDefinitionsConstants.NativeTerminatorAddress;
    }
    /// <summary>
    /// Resolves the native terminator offset back to record zero and rejects every other
    /// non-record offset instead of interpreting adjacent bank-$A8 code as palette data.
    /// </summary>
    public static ushort NormalizeByteOffset(ushort byteOffset)
    {
        if (byteOffset == WorkRobotPaletteTimingDefinitions.RecordCount * WorkRobotPaletteTimingDefinitions.RecordByteCount)
            return 0;

        _ = WorkRobotPaletteTiming.DurationForByteOffset(byteOffset);
        return byteOffset;
    }
}
