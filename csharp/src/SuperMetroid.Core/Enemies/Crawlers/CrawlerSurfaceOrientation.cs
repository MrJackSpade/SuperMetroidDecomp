namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>The four physical surfaces represented by crawler animations.</summary>
internal enum CrawlerSurfaceOrientation
{
    /// <summary>Clinging to a vertical surface with its top facing right.</summary>
    UpsideRight = 0,
    /// <summary>Clinging to a vertical surface with its top facing left.</summary>
    UpsideLeft = 1,
    /// <summary>Clinging to a ceiling.</summary>
    UpsideDown = 2,
    /// <summary>Standing on a floor.</summary>
    UpsideUp = 3,
}
