namespace SuperMetroid.DesktopVerification.Timing;

/// <summary>Deterministic result from the host timing counter regression.</summary>
/// <param name="UpdatesPerSecond">Published update rate under the 1,000-tick clock.</param>
/// <param name="AverageUpdateMilliseconds">Mean update duration for the first reporting interval.</param>
/// <param name="WorstUpdateMilliseconds">Longest update duration in that interval.</param>
/// <param name="LateFrames">Observed discarded-frame count, including fractional debt.</param>
public readonly record struct FrameTimingCounterSmokeTestResult(
    double UpdatesPerSecond,
    double AverageUpdateMilliseconds,
    double WorstUpdateMilliseconds,
    double LateFrames);
