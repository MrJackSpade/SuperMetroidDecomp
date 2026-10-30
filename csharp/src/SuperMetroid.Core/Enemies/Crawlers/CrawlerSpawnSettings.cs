namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Which surface a crawler starts on (#1471); Viola takes its surface from its properties instead.</summary>
/// <param name="Orientation">The surface the crawler starts on.</param>
internal sealed record CrawlerSpawnSettings(CrawlerSurfaceOrientation Orientation) : EnemySpawnSettings;
