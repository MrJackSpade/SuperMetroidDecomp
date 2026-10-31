namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Shared NTSC crawling magnitudes, in 8.8 pixels per frame.</summary>
public static class CrawlerSpeedDefinitions
{
    /// <summary>$A3:E67A parameter sentinel: preserve existing velocity before applying property signs.</summary>
    public const ushort PreserveVelocity = 255;
    /// <summary>Number of authored speed records; trailing zero is an intentional stationary entry.</summary>
    public const int Count = 32;

    /// <summary>$A3:E60C: first parameter whose quarter-pixel ramp skips one step; an authored gap in the ramp.</summary>
    internal const int FirstGapParameter = 14;
    /// <summary>$A3:E610: first parameter whose ramp has skipped four total quarter-pixel steps; an authored gap in the ramp.</summary>
    internal const int SecondGapParameter = 16;
    /// <summary>$A3:E62C: penultimate speed is eight pixels rather than the continuing ramp; an authored speed.</summary>
    internal const ushort PenultimateMagnitude = 2048;
}
