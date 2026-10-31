namespace SuperMetroid.Core.Escape;

/// <summary>Native $80:9EEC..9F6B countdown quantization expressed as hierarchical rational scheduling.</summary>
internal static class EscapeTimerCadenceDefinitions
{
    /// <summary>The countdown repeats its schedule every 128 frames of the frame counter.</summary>
    internal const int PeriodFrames = 128;
    /// <summary>Decimal timer time unit: one second contains100 centiseconds.</summary>
    internal const int CentisecondsPerSecond = 100;
    /// <summary>Nominal NTSC video cadence used to quantize the native countdown rate; this is a time-unit parameter, not a host clock dependency.</summary>
    internal const int NominalFramesPerSecond = 60;
    /// <summary>$80:9EEC..9F6B: round one128-frame period to the nearest integer centisecond; result213 (43 one-unit and85 two-unit corrections).</summary>
    internal const int PeriodCentiseconds = (PeriodFrames * CentisecondsPerSecond + NominalFramesPerSecond / 2) / NominalFramesPerSecond;
    /// <summary>The native period partitions into four32-frame budgets53,53,53,54; an exact native-compatible quantizer subdivision.</summary>
    internal const int QuarterCount = 4;
    /// <summary>Each quarter divides into two16-frame accumulators, front-loading an odd budget's extra unit; the exact native-compatible budget ordering.</summary>
    internal const int HalvesPerQuarter = 2;
    /// <summary>Native27-centisecond halves use accumulator preload1 in a16-unit denominator; an exact native-compatible accumulator initial condition.</summary>
    internal const int HigherBudgetPhase = 1;
    /// <summary>Native26-centisecond halves use accumulator preload4 in a16-unit denominator; an exact native-compatible accumulator initial condition.</summary>
    internal const int LowerBudgetPhase = 4;
}
