namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>The four physical surfaces represented by crawler animations.</summary>
internal enum CrawlerSurfaceOrientation
{
    /// <summary>Clinging to a vertical surface with its top facing right.</summary>
    OnLeftWall,
    /// <summary>Clinging to a vertical surface with its top facing left.</summary>
    OnRightWall,
    /// <summary>Clinging to a ceiling.</summary>
    OnCeiling,
    /// <summary>Standing on a floor.</summary>
    OnFloor,
}
