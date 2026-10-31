namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>Yard's native eight-direction crawling sign selection.</summary>
public static class YardVelocityDefinitions
{
    /// <summary>The heading of each direction's crawl.</summary>
    internal static readonly Dictionary<YardDirection, YardCrawlHeading> Headings = new()
    {
        [YardDirection.UpsideRightMovingUp] = new(HorizontalDirection.Left, VerticalDirection.Up),
        [YardDirection.UpsideRightMovingDown] = new(HorizontalDirection.Left, VerticalDirection.Down),
        [YardDirection.UpsideLeftMovingUp] = new(HorizontalDirection.Right, VerticalDirection.Up),
        [YardDirection.UpsideLeftMovingDown] = new(HorizontalDirection.Right, VerticalDirection.Down),
        [YardDirection.UpsideDownMovingLeft] = new(HorizontalDirection.Left, VerticalDirection.Up),
        [YardDirection.UpsideDownMovingRight] = new(HorizontalDirection.Right, VerticalDirection.Up),
        [YardDirection.UpsideUpMovingLeft] = new(HorizontalDirection.Left, VerticalDirection.Down),
        [YardDirection.UpsideUpMovingRight] = new(HorizontalDirection.Right, VerticalDirection.Down),
    };
}
