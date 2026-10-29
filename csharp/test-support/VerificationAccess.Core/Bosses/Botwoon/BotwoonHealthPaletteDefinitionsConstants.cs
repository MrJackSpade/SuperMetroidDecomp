namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Cartridge values of <see cref="BotwoonHealthPaletteDefinitions"/> that only verification reads.</summary>
internal static class BotwoonHealthPaletteDefinitionsConstants
{
    /// <summary><c>BotwoonHealthThresholdsForPaletteChange</c> at $B3:981B.</summary>
    public const int NativeThresholdAddress = 0xb3981b;

    /// <summary>
    /// The CGRAM colour retail Botwoon's health bands start at, $F0: the first colour of the last sprite set, which holds
    /// its body's colours (#1464).
    /// </summary>
    public const int FirstColour = 240;

    /// <summary>Provides the native addresses used to verify Botwoon palette changes.</summary>
    extension(BotwoonHealthPaletteDefinitions)
    {
        /// <inheritdoc cref="BotwoonHealthPaletteDefinitionsConstants.NativeThresholdAddress"/>
        internal static int NativeThresholdAddress => BotwoonHealthPaletteDefinitionsConstants.NativeThresholdAddress;

        /// <inheritdoc cref="BotwoonHealthPaletteDefinitionsConstants.FirstColour"/>
        internal static int FirstColour => BotwoonHealthPaletteDefinitionsConstants.FirstColour;
    }
}
