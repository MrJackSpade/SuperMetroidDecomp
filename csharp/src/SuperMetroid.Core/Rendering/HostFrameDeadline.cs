namespace SuperMetroid.Core.Rendering;

/// <summary>Pure wall-clock scheduling decision after one actually completed emulated frame.</summary>
public readonly record struct HostFrameDeadline(double NextDeadline, double WaitSeconds, bool Rebased)
{
    public static HostFrameDeadline AfterFrame(double previousDeadline, double now)
    {
        double period = 1 / HostFrameTimingDefinitions.FramesPerSecond;
        double next = previousDeadline + period;
        double wait = next - now;
        if (wait < -HostFrameTimingDefinitions.MaximumCatchUpFrames * period)
            // Only obsolete wall-clock debt is discarded. The caller has executed
            // exactly one frame and retains every input/audio sample. Allow a full
            // period before the next frame so rebasing does not cause another burst.
            return new(now + period, period, true);
        return new(next, Math.Max(0, wait), false);
    }
}
