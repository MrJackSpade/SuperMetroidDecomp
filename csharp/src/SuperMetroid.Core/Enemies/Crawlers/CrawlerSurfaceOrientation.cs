namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>The four physical surfaces represented by crawler animation lists.</summary>
internal enum CrawlerSurfaceOrientation
{
    /// <summary>Native table index zero, top facing right on a vertical surface.</summary>
    UpsideRight = 0,
    /// <summary>Native table index one, top facing left on a vertical surface.</summary>
    UpsideLeft = 1,
    /// <summary>Native table index two, ceiling attachment.</summary>
    UpsideDown = 2,
    /// <summary>Native table index three, floor attachment.</summary>
    UpsideUp = 3,
}
