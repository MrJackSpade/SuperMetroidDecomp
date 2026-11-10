namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>Cartridge values of <see cref="WorkRobotGlowDefinitions"/> that only verification reads.</summary>
internal static class WorkRobotPaletteTimingDefinitionsConstants
{
    /// <summary>First timer word, at $A8:CCC9, in the six ten-byte palette records.</summary>
    public const int NativeFirstTimerAddress = 0xa8ccc9;
    /// <summary>The negative wrap marker at $A8:CCFD following the six records.</summary>
    public const int NativeTerminatorAddress = 0xa8ccfd;
    /// <summary>Four colour words plus one timer word per native record (#1512: the port counts glow steps, not bytes).</summary>
    public const int NativeRecordByteCount = 10;

    /// <summary>Provides the native Work Robot palette timer start, terminator and record size.</summary>
    extension(WorkRobotGlowDefinitions)
    {
        /// <inheritdoc cref="WorkRobotPaletteTimingDefinitionsConstants.NativeFirstTimerAddress"/>
        internal static int NativeFirstTimerAddress => WorkRobotPaletteTimingDefinitionsConstants.NativeFirstTimerAddress;
        /// <inheritdoc cref="WorkRobotPaletteTimingDefinitionsConstants.NativeTerminatorAddress"/>
        internal static int NativeTerminatorAddress => WorkRobotPaletteTimingDefinitionsConstants.NativeTerminatorAddress;
        /// <inheritdoc cref="WorkRobotPaletteTimingDefinitionsConstants.NativeRecordByteCount"/>
        internal static int RecordByteCount => NativeRecordByteCount;
    }
}
