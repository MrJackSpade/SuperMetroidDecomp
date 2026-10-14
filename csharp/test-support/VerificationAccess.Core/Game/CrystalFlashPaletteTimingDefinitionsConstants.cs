using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="CrystalFlashPaletteTimingDefinitions"/> that only verification reads.</summary>
internal static class CrystalFlashPaletteTimingDefinitionsConstants
{
    /// <summary>
    /// Timer words in the ten four-byte pointer/timer records at $91:DC00-$91:DC27.
    /// The first timer is at $91:DC02 and subsequent timers have a four-byte stride.
    /// </summary>
    public const int NativeFirstTimerAddress = 0x91dc02;

    /// <summary>Provides the native first timer address for Crystal Flash palette timing verification.</summary>
    extension(CrystalFlashPaletteTimingDefinitions)
    {
        /// <inheritdoc cref="CrystalFlashPaletteTimingDefinitionsConstants.NativeFirstTimerAddress"/>
        internal static int NativeFirstTimerAddress => CrystalFlashPaletteTimingDefinitionsConstants.NativeFirstTimerAddress;
    }
}
