namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>A Yard's crawling velocity along its direction.</summary>
public static class YardCrawlVelocity
{
    /// <summary>
    /// The crawl velocity <paramref name="speed"/> gives in <paramref name="direction"/>: the speed taken toward the
    /// direction's heading on each axis.
    /// </summary>
    public static PixelVelocity<RoomPixels> Apply(SignedEightEight speed, YardDirection direction)
    {
        YardCrawlHeading heading = YardVelocityDefinitions.Headings[direction];
        return new(speed.Toward(heading.Horizontal), speed.Toward(heading.Vertical));
    }
}
