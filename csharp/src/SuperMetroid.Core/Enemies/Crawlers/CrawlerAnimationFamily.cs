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
    /// <summary>The orange Zoomer's surface animations, which it selects itself; shared corner turns never use this family.</summary>
    HZoomer,
}
