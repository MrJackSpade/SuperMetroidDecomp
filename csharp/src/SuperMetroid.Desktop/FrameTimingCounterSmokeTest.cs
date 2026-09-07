namespace SuperMetroid.Desktop;

/// <summary>Deterministic result from the host timing counter regression.</summary>
public readonly record struct FrameTimingCounterSmokeTestResult(
    double EmulatedFramesPerSecond,
    double PaintedFramesPerSecond,
    double AverageEmulationMilliseconds,
    double WorstEmulationMilliseconds,
    double LateFrames);

/// <summary>
/// Exercises timing accumulation with an artificial one-kilohertz clock. No ROM, window,
/// scheduler, or workstation performance can affect the expected values.
/// </summary>
public static class FrameTimingCounterSmokeTest
{
    public static FrameTimingCounterSmokeTestResult Run()
    {
        const long TimestampFrequency = 1_000;
        var counter = new FrameTimingCounter(TimestampFrequency, TimeSpan.FromSeconds(1));
        double observedDiscards = 0;
        counter.LateFramesRecorded += count => observedDiscards += count;
        counter.Reset(timestamp: 10_000);

        for (int frame = 0; frame < 60; frame++)
            counter.RecordEmulatedFrame(elapsedTicks: frame == 59 ? 8 : 4);
        for (int paint = 0; paint < 30; paint++)
            counter.RecordPaint(elapsedTicks: paint == 29 ? 10 : 2);
        counter.RecordLateFrames(2.5);

        if (counter.TryTakeSnapshot(timestamp: 10_999, out _))
            throw new InvalidDataException("Timing counter published before its reporting interval.");
        if (!counter.TryTakeSnapshot(timestamp: 11_000, out FrameTimingSnapshot snapshot))
            throw new InvalidDataException("Timing counter did not publish at its reporting interval.");

        AssertNear(snapshot.EmulatedFramesPerSecond, 60, "emulated FPS");
        AssertNear(snapshot.PaintedFramesPerSecond, 30, "paint FPS");
        AssertNear(snapshot.AverageEmulationMilliseconds, 244.0 / 60.0, "average step time");
        AssertNear(snapshot.WorstEmulationMilliseconds, 8, "worst step time");
        AssertNear(snapshot.AveragePaintMilliseconds, 68.0 / 30.0, "average paint time");
        AssertNear(snapshot.WorstPaintMilliseconds, 10, "worst paint time");
        AssertNear(snapshot.LateFrames, 2.5, "late frames");

        // Publication starts a new interval instead of leaking the previous counts into the
        // next readout. This matters after an isolated RDP or debugger stall.
        if (!counter.TryTakeSnapshot(timestamp: 12_000, out FrameTimingSnapshot empty))
            throw new InvalidDataException("Timing counter did not start a second interval.");
        AssertNear(empty.EmulatedFramesPerSecond, 0, "cleared emulated FPS");
        AssertNear(empty.PaintedFramesPerSecond, 0, "cleared paint FPS");
        AssertNear(empty.LateFrames, 0, "cleared late frames");
        AssertNear(observedDiscards, 2.5, "whole-run observer survives interval reset");

        return new FrameTimingCounterSmokeTestResult(
            snapshot.EmulatedFramesPerSecond,
            snapshot.PaintedFramesPerSecond,
            snapshot.AverageEmulationMilliseconds,
            snapshot.WorstEmulationMilliseconds,
            snapshot.LateFrames);
    }

    private static void AssertNear(double actual, double expected, string subject)
    {
        if (Math.Abs(actual - expected) > 0.000_001)
            throw new InvalidDataException($"Timing counter {subject} was {actual}, expected {expected}.");
    }
}
