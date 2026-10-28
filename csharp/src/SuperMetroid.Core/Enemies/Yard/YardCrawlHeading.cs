namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>The way a Yard crawls along each axis in one of its eight directions.</summary>
/// <param name="Horizontal">The way the crawl moves along X.</param>
/// <param name="Vertical">The way the crawl moves along Y.</param>
internal readonly record struct YardCrawlHeading(HorizontalDirection Horizontal, VerticalDirection Vertical);
