using static SuperMetroid.Core.Enemies.Crawlers.CrawlerSpeedDefinitions;

namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>A crawler's or Yard's crawling magnitude, in 8.8 pixels per frame, by speed parameter.</summary>
public static class CrawlerSpeeds
{
    /// <summary>
    /// Both native copies consist of quarter-pixel ramps separated by two gaps,
    /// followed by a repeated eight-pixel speed and a stationary entry. Each ramp
    /// calculates; the two gap points and the penultimate magnitude are authored tuning
    /// with no rule in the ramp (see residualScalarInputsReview).
    /// </summary>
    public static int ForParameter(int parameter)
    {
        if (parameter >= Count)
            throw new InvalidDataException($"Crawler speed parameter ${parameter:X4} exceeds the 32 authored records.");
        if (parameter == Count - 1) return 0;
        if (parameter == Count - 2) return PenultimateMagnitude;
        int skippedSteps = parameter < FirstGapParameter ? 0 : parameter < SecondGapParameter ? 1 : 4;
        return WordWrap.Unsigned((parameter + 1 + skippedSteps) * 64);
    }
}
