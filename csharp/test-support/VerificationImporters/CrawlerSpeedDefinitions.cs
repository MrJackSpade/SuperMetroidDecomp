namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Shared NTSC crawling magnitudes, in 8.8 pixels per frame.</summary>
public static class CrawlerSpeedDefinitions
{
    /// <summary>Number of authored speed records; trailing zero is an intentional stationary entry.</summary>
    public const int Count = 32;

    /// <summary>First parameter whose quarter-pixel ramp skips one step; an authored gap in the ramp.</summary>
    internal const int FirstGapParameter = 14;
    /// <summary>First parameter whose ramp has skipped four total quarter-pixel steps; an authored gap in the ramp.</summary>
    internal const int SecondGapParameter = 16;
    /// <summary>Penultimate speed is eight pixels rather than the continuing ramp; an authored speed.</summary>
    internal const int PenultimateMagnitude = 2048;
}
