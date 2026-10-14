using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="BotwoonHealthPaletteDefinitions"/> that only verification reads.</summary>
internal static class BotwoonHealthPaletteDefinitionsConstants
{
    /// <summary><c>BotwoonHealthThresholdsForPaletteChange</c> at $B3:981B.</summary>
    public const int NativeThresholdAddress = 0xb3981b;

    /// <summary>Provides the native health threshold table address used to verify Botwoon palette changes.</summary>
    extension(BotwoonHealthPaletteDefinitions)
    {
        /// <inheritdoc cref="BotwoonHealthPaletteDefinitionsConstants.NativeThresholdAddress"/>
        internal static int NativeThresholdAddress => BotwoonHealthPaletteDefinitionsConstants.NativeThresholdAddress;
    }
}
