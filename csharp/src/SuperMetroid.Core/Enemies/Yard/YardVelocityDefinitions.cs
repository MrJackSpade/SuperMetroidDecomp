namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>Yard's native eight-direction crawling sign selection.</summary>
public static class YardVelocityDefinitions
{
    /// <summary>The heading of each direction's crawl, by direction number.</summary>
    internal static readonly YardCrawlHeading[] Headings =
    [
        new(HorizontalDirection.Left, VerticalDirection.Up),
        new(HorizontalDirection.Left, VerticalDirection.Down),
        new(HorizontalDirection.Right, VerticalDirection.Up),
        new(HorizontalDirection.Right, VerticalDirection.Down),
        new(HorizontalDirection.Left, VerticalDirection.Up),
        new(HorizontalDirection.Right, VerticalDirection.Up),
        new(HorizontalDirection.Left, VerticalDirection.Down),
        new(HorizontalDirection.Right, VerticalDirection.Down),
    ];
}
