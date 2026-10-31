namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>Signs a Yard's crawling speed for its direction.</summary>
public static class YardCrawlVelocity
{
    /// <summary>Equivalent to native XOR $FFFF then increment; negation wraps at sixteen bits.</summary>
    public static EightEightVector<RoomPixels> Apply(int magnitude, YardDirection direction)
    {
        var positive = new SignedEightEight(magnitude);
        var negative = SignedEightEight.Wrapped(-magnitude);
        YardCrawlHeading heading = Ensure.Listed(YardVelocityDefinitions.Headings, direction);
        return new(heading.Horizontal == HorizontalDirection.Left ? negative : positive,
            heading.Vertical == VerticalDirection.Up ? negative : positive);
    }
}
