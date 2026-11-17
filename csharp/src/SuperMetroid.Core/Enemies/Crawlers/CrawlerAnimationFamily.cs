namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Mutually exclusive crawler animation families, each with its own four surface animations.</summary>
public enum CrawlerAnimationFamily
{
    /// <summary>The common surface animations of Zoomers, Zeelas, Sovas and M-Zoomers.</summary>
    Shared,
    /// <summary>Viola's own surface animations.</summary>
    Viola,
    /// <summary>Sciser's own surface animations.</summary>
    Sciser,
    /// <summary>Zero's own surface animations.</summary>
    Zero,
    /// <summary>The orange Zoomer's own surface table, which selects the crawls the shared family does (#2283).</summary>
    OrangeZoomer,
}
