namespace SuperMetroid.DesktopVerification.Timing;

/// <summary>Deterministic result from the host timing counter regression.</summary>
/// <param name="EmulatedFramesPerSecond">Published update rate under the 1,000-tick clock.</param>
/// <param name="PaintedFramesPerSecond">Published display rate under the same clock.</param>
/// <param name="AverageEmulationMilliseconds">Mean update duration for the first reporting interval.</param>
/// <param name="WorstEmulationMilliseconds">Longest update duration in that interval.</param>
/// <param name="LateFrames">Observed discarded-frame count, including fractional debt.</param>
public readonly record struct FrameTimingCounterSmokeTestResult(
    double EmulatedFramesPerSecond,
    double PaintedFramesPerSecond,
    double AverageEmulationMilliseconds,
    double WorstEmulationMilliseconds,
    double LateFrames);
