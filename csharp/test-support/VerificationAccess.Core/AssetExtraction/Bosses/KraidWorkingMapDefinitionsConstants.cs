namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>Cartridge values of <see cref="KraidWorkingMapDefinitions"/> that only verification reads.</summary>
internal static class KraidWorkingMapDefinitionsConstants
{
    /// <summary>
    /// First untouched word left by <c>$A7:AB19-$AB2C</c> after the lower stream is
    /// decompressed directly into the working tilemap.
    /// </summary>
    public const int PreservedLowerTailFirstWord = 0x0700;

    /// <summary>Provides the preserved lower-tail word used when checking Kraid background import fidelity.</summary>
    extension(KraidWorkingMapDefinitions)
    {
        /// <inheritdoc cref="KraidWorkingMapDefinitionsConstants.PreservedLowerTailFirstWord"/>
        internal static int PreservedLowerTailFirstWord => KraidWorkingMapDefinitionsConstants.PreservedLowerTailFirstWord;
    }
}
