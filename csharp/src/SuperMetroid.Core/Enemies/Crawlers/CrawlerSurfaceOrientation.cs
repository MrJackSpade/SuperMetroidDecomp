namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>The four physical surfaces represented by crawler animations.</summary>
internal enum CrawlerSurfaceOrientation
{
    /// <summary>Table index zero, top facing right on a vertical surface.</summary>
    UpsideRight = 0,
    /// <summary>Table index one, top facing left on a vertical surface.</summary>
    UpsideLeft = 1,
    /// <summary>Table index two, ceiling attachment.</summary>
    UpsideDown = 2,
    /// <summary>Table index three, floor attachment.</summary>
    UpsideUp = 3,
}
