using static SuperMetroid.AssetExtraction.Frontend.Ending.EndingPostShotUploadDefinitions;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// Selects the post-shot transfers of the ending (<see cref="EndingPostShotUploadDefinitions"/>) in native order.
/// </summary>
internal static class EndingPostShotUploads
{
    /// <summary>The post-shot upload at <paramref name="index"/>: the subtitle, the logo's four consecutive chunks of
    /// equal length, then the tilemap. The chunks' source offsets advance by the chunk length; the subtitle and the map
    /// are their own transfer cases, not offsets between the chunks.</summary>
    public static EndingPostShotUploadDefinition Get(int index)
    {
        if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
        if (index == 0) return new(SubtitleLength, 0, EndingPostShotUploadSource.Subtitle);
        if (index == Count - 1) return new(LogoMapLength, 0, EndingPostShotUploadSource.LogoMap);
        int offset = (index - 1) * LogoTileChunkLength;
        return new(LogoTileChunkLength, offset, EndingPostShotUploadSource.LogoTiles);
    }
}
