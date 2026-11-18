namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>
/// The Sciser, Zero, Viola and spiny crawlers' surface animations (#641): one segment per crawl, in authored order; the orange
/// Zoomer and the Zoomers, Zeelas and Sovas crawl each surface by the same one (#2283).
/// Their steps are <see cref="CrawlerAnimations"/>.
/// </summary>
internal enum CrawlerSegment
{
    /// <summary>The Sciser crawling with its upper side right.</summary>
    SciserUpsideRight,
    /// <summary>The Sciser crawling with its upper side left.</summary>
    SciserUpsideLeft,
    /// <summary>The Sciser crawling with its upper side down.</summary>
    SciserUpsideDown,
    /// <summary>The Sciser crawling with its upper side up.</summary>
    SciserUpsideUp,
    /// <summary>The Zero crawling with its upper side right.</summary>
    ZeroUpsideRight,
    /// <summary>The Zero crawling with its upper side left.</summary>
    ZeroUpsideLeft,
    /// <summary>The Zero crawling with its upper side down.</summary>
    ZeroUpsideDown,
    /// <summary>The Zero crawling with its upper side up.</summary>
    ZeroUpsideUp,
    /// <summary>The Viola's idle crawl (#2477), the same on every surface.</summary>
    ViolaIdle,
    /// <summary>A Zoomer, Zeela, Sova or orange Zoomer crawling with its upper side right (#2283).</summary>
    SpinyCrawlerUpsideRight,
    /// <summary>A Zoomer, Zeela, Sova or orange Zoomer crawling with its upper side left (#2283).</summary>
    SpinyCrawlerUpsideLeft,
    /// <summary>A Zoomer, Zeela, Sova or orange Zoomer crawling with its upper side down (#2283).</summary>
    SpinyCrawlerUpsideDown,
    /// <summary>A Zoomer, Zeela, Sova or orange Zoomer crawling with its upper side up (#2283).</summary>
    SpinyCrawlerUpsideUp,
}
