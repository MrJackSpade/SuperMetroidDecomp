namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Four surface-oriented animations for one crawler family or species.</summary>
internal readonly record struct CrawlerAnimationDefinition(
    CrawlerSegment OnLeftWall,
    CrawlerSegment OnRightWall,
    CrawlerSegment OnCeiling,
    CrawlerSegment OnFloor);
