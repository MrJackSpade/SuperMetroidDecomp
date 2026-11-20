using static SuperMetroid.AssetExtraction.Frontend.Ending.EndingPostShotUploadDefinitions;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// Selects the post-shot transfers of the ending (<see cref="EndingPostShotUploadDefinitions"/>) in native order.
/// </summary>
internal static class EndingPostShotUploads
{
    /// <summary>The subtitle's characters.</summary>
    internal static EndingPostShotUploadDefinition Subtitle { get; } = new(SubtitleLength, 0, EndingPostShotUploadSource.Subtitle);

    /// <summary>
    /// The logo's characters, in four consecutive chunks of equal length, each starting where the one before it ends.
    /// </summary>
    internal static IReadOnlyList<EndingPostShotUploadDefinition> LogoTileChunks { get; } =
        [.. Enumerable.Range(0, Count - 2).Select(chunk => new EndingPostShotUploadDefinition(
            LogoTileChunkLength, chunk * LogoTileChunkLength, EndingPostShotUploadSource.LogoTiles))];

    /// <summary>The logo's tilemap.</summary>
    internal static EndingPostShotUploadDefinition LogoMap { get; } = new(LogoMapLength, 0, EndingPostShotUploadSource.LogoMap);

    /// <summary>The post-shot uploads in the order the ending makes them: the subtitle, the logo's chunks, then its tilemap.</summary>
    public static IReadOnlyList<EndingPostShotUploadDefinition> InOrder { get; } = [Subtitle, .. LogoTileChunks, LogoMap];
}
