namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Mutually exclusive crawler animation families, each with its own four surface animations.</summary>
public enum CrawlerAnimationFamily
{
    /// <summary>The common surface animations of Zoomers, Zeelas, Sovas, M-Zoomers and the orange Zoomer.</summary>
    Shared,
    /// <summary>Viola's own surface animations.</summary>
    Viola,
    /// <summary>Sciser's own surface animations.</summary>
    Sciser,
    /// <summary>Zero's own surface animations.</summary>
    Zero,
}
