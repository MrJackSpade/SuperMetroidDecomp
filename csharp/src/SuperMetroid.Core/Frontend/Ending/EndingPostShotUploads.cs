using static SuperMetroid.Core.Frontend.Ending.EndingPostShotUploadDefinitions;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// Selects the post-shot transfers of the ending (<see cref="EndingPostShotUploadDefinitions"/>) in native order.
/// </summary>
internal static class EndingPostShotUploads
{
    /// <summary>Subtitle, four consecutive2048-byte logo chunks, then tilemap.
    /// Source byte offsets advance by chunk length. Native8B:E45A..E489 independently confirms all six records for #1165.
    /// The subtitle/map are semantic transfer cases, not interpolated chunk addresses.</summary>
    public static EndingPostShotUploadDefinition Get(int index)
    {
        if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
        if (index == 0) return new(SubtitleLength, 0, EndingPostShotUploadSource.Subtitle);
        if (index == Count - 1) return new(LogoMapLength, 0, EndingPostShotUploadSource.LogoMap);
        int offset = (index - 1) * LogoTileChunkLength;
        return new(LogoTileChunkLength, offset, EndingPostShotUploadSource.LogoTiles);
    }
}
