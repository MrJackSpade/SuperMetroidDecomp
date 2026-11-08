namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Cartridge values of <see cref="BotwoonHealthBandDefinitions"/> that only verification reads.</summary>
internal static class BotwoonHealthBandDefinitionsConstants
{
    /// <summary><c>BotwoonHealthThresholdsForPaletteChange</c> at $B3:981B.</summary>
    public const int NativeThresholdAddress = 0xb3981b;

    /// <summary>
    /// The CGRAM colour retail Botwoon's health bands start at, $F0: the first colour of the last sprite set, which holds
    /// its body's colours (#1464).
    /// </summary>
    public const int FirstColour = 240;

    /// <summary>Provides the native addresses used to verify Botwoon palette changes.</summary>
    extension(BotwoonHealthBandDefinitions)
    {
        /// <inheritdoc cref="BotwoonHealthBandDefinitionsConstants.NativeThresholdAddress"/>
        internal static int NativeThresholdAddress => BotwoonHealthBandDefinitionsConstants.NativeThresholdAddress;

        /// <inheritdoc cref="BotwoonHealthBandDefinitionsConstants.FirstColour"/>
        internal static int FirstColour => BotwoonHealthBandDefinitionsConstants.FirstColour;
    }
}
