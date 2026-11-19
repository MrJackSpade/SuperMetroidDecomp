namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>
/// The Sciser, Zero, Viola and spiny crawlers' surface animations (#641): one segment per crawl, in authored order; the orange
/// Zoomer and the Zoomers, Zeelas and Sovas crawl each surface by the same one (#2283).
/// Their steps are <see cref="CrawlerAnimations"/>.
/// </summary>
internal enum CrawlerSegment
{
    /// <summary>The Sciser crawling with its upper side right.</summary>
    SciserOnLeftWall,
    /// <summary>The Sciser crawling with its upper side left.</summary>
    SciserOnRightWall,
    /// <summary>The Sciser crawling with its upper side down.</summary>
    SciserOnCeiling,
    /// <summary>The Sciser crawling with its upper side up.</summary>
    SciserOnFloor,
    /// <summary>The Zero crawling with its upper side right.</summary>
    ZeroOnLeftWall,
    /// <summary>The Zero crawling with its upper side left.</summary>
    ZeroOnRightWall,
    /// <summary>The Zero crawling with its upper side down.</summary>
    ZeroOnCeiling,
    /// <summary>The Zero crawling with its upper side up.</summary>
    ZeroOnFloor,
    /// <summary>The Viola's idle crawl (#2477), the same on every surface.</summary>
    ViolaIdle,
    /// <summary>A Zoomer, Zeela, Sova or orange Zoomer crawling with its upper side right (#2283).</summary>
    SpinyCrawlerOnLeftWall,
    /// <summary>A Zoomer, Zeela, Sova or orange Zoomer crawling with its upper side left (#2283).</summary>
    SpinyCrawlerOnRightWall,
    /// <summary>A Zoomer, Zeela, Sova or orange Zoomer crawling with its upper side down (#2283).</summary>
    SpinyCrawlerOnCeiling,
    /// <summary>A Zoomer, Zeela, Sova or orange Zoomer crawling with its upper side up (#2283).</summary>
    SpinyCrawlerOnFloor,
}
