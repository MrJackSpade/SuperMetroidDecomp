namespace SuperMetroid.Core.Game;

/// <summary>Shared NTSC crawling magnitudes, in 8.8 pixels per frame.</summary>
public static class CrawlerSpeedDefinitions
{
    /// <summary>$A3:E67A parameter sentinel: preserve existing velocity before applying property signs.</summary>
    public const ushort PreserveVelocity = 0xff;
    /// <summary>Number of authored speed records; trailing zero is an intentional stationary entry.</summary>
    public const int Count = 32;

    /// <summary>$A3:E60C: first parameter whose quarter-pixel ramp skips one step; an authored gap in the ramp.</summary>
    private const int FirstGapParameter = 14;
    /// <summary>$A3:E610: first parameter whose ramp has skipped four total quarter-pixel steps; an authored gap in the ramp.</summary>
    private const int SecondGapParameter = 16;
    /// <summary>$A3:E62C: penultimate speed is eight pixels rather than the continuing ramp; an authored speed.</summary>
    private const ushort PenultimateMagnitude = 0x0800;

    /// <summary>
    /// Both native copies consist of quarter-pixel ramps separated by two gaps,
    /// followed by a repeated eight-pixel speed and a stationary entry. Each ramp
    /// calculates; the two gap points and the penultimate magnitude are authored tuning
    /// with no rule in the ramp (see residualScalarInputsReview).
    /// </summary>
    public static ushort ForParameter(ushort parameter)
    {
        if (parameter >= Count)
            throw new InvalidDataException($"Crawler speed parameter ${parameter:X4} exceeds the 32 authored records.");
        if (parameter == Count - 1) return 0;
        if (parameter == Count - 2) return PenultimateMagnitude;
        int skippedSteps = parameter < FirstGapParameter ? 0 : parameter < SecondGapParameter ? 1 : 4;
        return (ushort)((parameter + 1 + skippedSteps) * 64);
    }
}
