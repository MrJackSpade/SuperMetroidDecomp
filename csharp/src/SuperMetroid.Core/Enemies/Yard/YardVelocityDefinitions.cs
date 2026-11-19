namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>Yard's eight-direction crawling sign selection and its aggressive crawl speed.</summary>
public static class YardVelocityDefinitions
{
    /// <summary>
    /// The speed a kicked Yard crawls at after it lands, two and a quarter pixels per update.
    /// </summary>
    internal static readonly SignedEightEight AggressiveCrawlSpeed = SignedEightEight.FromPixelFraction(9, 4);

    /// <summary>The heading of each direction's crawl.</summary>
    internal static readonly Dictionary<YardDirection, YardCrawlHeading> Headings = new()
    {
        [YardDirection.OnLeftWallMovingUp] = new(HorizontalDirection.Left, VerticalDirection.Up),
        [YardDirection.OnLeftWallMovingDown] = new(HorizontalDirection.Left, VerticalDirection.Down),
        [YardDirection.OnRightWallMovingUp] = new(HorizontalDirection.Right, VerticalDirection.Up),
        [YardDirection.OnRightWallMovingDown] = new(HorizontalDirection.Right, VerticalDirection.Down),
        [YardDirection.OnCeilingMovingLeft] = new(HorizontalDirection.Left, VerticalDirection.Up),
        [YardDirection.OnCeilingMovingRight] = new(HorizontalDirection.Right, VerticalDirection.Up),
        [YardDirection.OnFloorMovingLeft] = new(HorizontalDirection.Left, VerticalDirection.Down),
        [YardDirection.OnFloorMovingRight] = new(HorizontalDirection.Right, VerticalDirection.Down),
    };
}
