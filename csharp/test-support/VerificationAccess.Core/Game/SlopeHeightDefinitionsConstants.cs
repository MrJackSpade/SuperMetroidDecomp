using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="SlopeHeightDefinitions"/> that only verification reads.</summary>
internal static class SlopeHeightDefinitionsConstants
{
    /// <summary>
    /// $94:8B2B SlopeDefinitions_SlopeTopXOffsetByYPixel: top Y offset at column X,
    /// despite the swapped-axis native label. Thirty-two sixteen-column profiles.
    /// </summary>
    public const int ReferenceAddress = 0x948b2b;

    /// <summary>Provides the native slope-height reference address for block geometry comparisons.</summary>
    extension(SlopeHeightDefinitions)
    {
        /// <inheritdoc cref="SlopeHeightDefinitionsConstants.ReferenceAddress"/>
        internal static int ReferenceAddress => SlopeHeightDefinitionsConstants.ReferenceAddress;
    }
}
