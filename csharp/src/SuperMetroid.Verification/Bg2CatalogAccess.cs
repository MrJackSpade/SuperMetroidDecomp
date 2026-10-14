using SuperMetroid.Core.Enemies.Artwork;

/// <summary>How a generated boss BG2 catalog is queried and re-extracted (#633).</summary>
internal readonly record struct Bg2CatalogAccess
{
    /// <summary>Is frame.</summary>
    public required Func<ushort, bool> IsFrame { get; init; }

    /// <summary>Frame at.</summary>
    public required Func<int, EnemyBg2FrameDefinition> FrameAt { get; init; }

    /// <summary>Extract.</summary>
    public required Func<byte[]> Extract { get; init; }
}
