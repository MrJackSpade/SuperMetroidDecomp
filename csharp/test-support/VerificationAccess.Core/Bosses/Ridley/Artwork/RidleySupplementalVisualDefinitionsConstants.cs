using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>Cartridge values and helpers of <see cref="RidleySupplementalVisualDefinitions"/> that only verification reads.</summary>
internal static class RidleySupplementalVisualDefinitionsConstants
{
    /// <summary>Selects large, medium, or small tail-link art for six articulated segments.</summary>
    internal static ushort SegmentFrameAt(int index) => index switch
    {
        0 or 1 => RidleySupplementalVisualDefinitions.LargeSegment,
        2 or 3 => RidleySupplementalVisualDefinitions.MediumSegment,
        4 or 5 => RidleySupplementalVisualDefinitions.SmallSegment,
        _ => throw new InvalidDataException($"Ridley tail segment {index} is outside six drawn links."),
    };
}
