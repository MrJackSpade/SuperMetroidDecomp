namespace SuperMetroid.Core.Game;

/// <summary>Domain-owned transitions over <see cref="CrawlerSurfaceOrientation"/>.</summary>
internal static class CrawlerSurfaceOrientations
{
    /// <summary>The surface at <paramref name="index"/> of a four-entry surface-ordered table.</summary>
    internal static CrawlerSurfaceOrientation AtTableIndex(int index) => index switch
    {
        0 => CrawlerSurfaceOrientation.UpsideRight,
        1 => CrawlerSurfaceOrientation.UpsideLeft,
        2 => CrawlerSurfaceOrientation.UpsideDown,
        3 => CrawlerSurfaceOrientation.UpsideUp,
        _ => throw new InvalidOperationException($"Crawler surface table index {index} has no {nameof(CrawlerSurfaceOrientation)}."),
    };
}
