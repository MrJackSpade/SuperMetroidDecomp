namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// Compiled six-transfer schedule for Func142's post-credits Super Metroid logo.
/// Artwork bytes remain separate presentation assets; this table fixes only
/// their native ordering, lengths and sources.
/// </summary>
internal static class EndingPostShotUploadDefinitions
{

    /// <summary>Func142 uploads the subtitle, four logo-tile chunks, then the logo map.</summary>
    public const int Count = 6;

    /// <summary>Subtitle font transfer length at <c>$8B:E45A</c>.</summary>
    internal const ushort SubtitleLength = 0x0400;

    /// <summary>Logo-tile chunk length at <c>$8B:E462</c> through <c>$8B:E47A</c>.</summary>
    private const ushort LogoTileChunkLength = 0x0800;

    /// <summary>Logo-map transfer length at <c>$8B:E482</c>.</summary>
    private const ushort LogoMapLength = 0x0800;

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
