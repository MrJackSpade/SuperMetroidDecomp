using static SuperMetroid.Core.Escape.EscapeTimerCadenceDefinitions;

namespace SuperMetroid.Core.Escape;

/// <summary>
/// Counts the escape timer down at the native cadence (<see cref="EscapeTimerCadenceDefinitions"/>): the centiseconds each frame
/// of the 128-frame period removes.
/// </summary>
internal static class EscapeTimerCadence
{
    /// <summary>
    /// The countdown selects its step by the frame counter's phase. Distribute the period budget by cumulative
    /// floor, split each quarter with its odd unit first, then difference a fractional
    /// accumulator at consecutive frames. No sampled per-frame corrections are stored.
    /// </summary>
    internal static int Centiseconds(int frameCounter)
    {
        int frame = frameCounter & (PeriodFrames - 1);
        int quarterFrames = PeriodFrames / QuarterCount;
        int halfFrames = quarterFrames / HalvesPerQuarter;
        int quarter = frame / quarterFrames;
        int quarterBudget = PeriodCentiseconds * (quarter + 1) / QuarterCount -
            PeriodCentiseconds * quarter / QuarterCount;
        int lowerBudget = quarterBudget / HalvesPerQuarter;
        int higherBudget = (quarterBudget + HalvesPerQuarter - 1) / HalvesPerQuarter;
        int budget = frame % quarterFrames < halfFrames ? higherBudget : lowerBudget;
        int highRateBudget = (PeriodCentiseconds + QuarterCount * HalvesPerQuarter - 1) /
            (QuarterCount * HalvesPerQuarter);
        int phase = budget == highRateBudget ? HigherBudgetPhase : LowerBudgetPhase;
        int localFrame = frame % halfFrames;
        return (budget * (localFrame + 1) + phase) / halfFrames -
            (budget * localFrame + phase) / halfFrames;
    }
}
