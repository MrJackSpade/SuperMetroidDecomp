namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>Signs a Yard's crawling speed for its direction.</summary>
public static class YardCrawlVelocity
{
    /// <summary>The crawl velocity <paramref name="speed"/> gives on each axis in <paramref name="direction"/>; negation wraps at sixteen bits.</summary>
    public static EightEightVector<RoomPixels> Apply(SignedEightEight speed, YardDirection direction)
    {
        SignedEightEight negative = speed.Negated();
        YardCrawlHeading heading = Ensure.Listed(YardVelocityDefinitions.Headings, direction);
        return new(heading.Horizontal == HorizontalDirection.Left ? negative : speed,
            heading.Vertical == VerticalDirection.Up ? negative : speed);
    }
}
