
namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Cartridge values of <see cref="SlopeHeights"/> that only verification reads.</summary>
internal static class SlopeHeightsConstants
{
    /// <summary>
    /// $94:8B2B SlopeDefinitions_SlopeTopXOffsetByYPixel: top Y offset at column X,
    /// despite the swapped-axis native label. Thirty-two sixteen-column profiles.
    /// </summary>
    public const int ReferenceAddress = 0x948b2b;

    /// <summary>Provides the native slope-height reference address for block geometry comparisons.</summary>
    extension(SlopeHeights)
    {
        /// <inheritdoc cref="SlopeHeightsConstants.ReferenceAddress"/>
        internal static int ReferenceAddress => SlopeHeightsConstants.ReferenceAddress;
    }
}
