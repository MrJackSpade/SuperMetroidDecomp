namespace SuperMetroid.Core.Frontend;

/// <summary>The three WRAM staging buffers the post-shot transfers read from.</summary>
internal enum EndingPostShotUploadSource
{
    /// <summary>The subtitle font page staged at <c>$7F:D000</c>.</summary>
    Subtitle,
    /// <summary>The decompressed logo tiles staged from <c>$7E:6000</c>.</summary>
    LogoTiles,
    /// <summary>The decompressed logo tilemap staged at <c>$7E:8000</c>.</summary>
    LogoMap,
}
