namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Four surface-oriented animations for one crawler family or species.</summary>
internal readonly record struct CrawlerAnimationDefinition(
    CrawlerProgram UpsideRight,
    CrawlerProgram UpsideLeft,
    CrawlerProgram UpsideDown,
    CrawlerProgram UpsideUp);
