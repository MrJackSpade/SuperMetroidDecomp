using SuperMetroid.Core.Game;

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
}
