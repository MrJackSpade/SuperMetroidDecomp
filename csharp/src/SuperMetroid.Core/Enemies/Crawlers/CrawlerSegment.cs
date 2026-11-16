namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>
/// The Sciser, Zero, Viola, orange Zoomer and shared Zoomer/Zeela/Sova surface animations (#641): one segment per crawl, in authored order.
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
    /// <summary>The Viola's crawl, the same on every surface.</summary>
    ViolaNormal,
    /// <summary>The orange Zoomer crawling with its upper side right.</summary>
    OrangeZoomerUpsideRight,
    /// <summary>The orange Zoomer crawling with its upper side left.</summary>
    OrangeZoomerUpsideLeft,
    /// <summary>The orange Zoomer crawling with its upper side down.</summary>
    OrangeZoomerUpsideDown,
    /// <summary>The orange Zoomer crawling with its upper side up.</summary>
    OrangeZoomerUpsideUp,
    /// <summary>The Zoomer, Zeela or Sova crawling with its upper side right.</summary>
    SharedUpsideRight,
    /// <summary>The Zoomer, Zeela or Sova crawling with its upper side left.</summary>
    SharedUpsideLeft,
    /// <summary>The Zoomer, Zeela or Sova crawling with its upper side down.</summary>
    SharedUpsideDown,
    /// <summary>The Zoomer, Zeela or Sova crawling with its upper side up.</summary>
    SharedUpsideUp,
}
