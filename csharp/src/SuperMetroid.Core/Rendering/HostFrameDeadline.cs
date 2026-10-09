namespace SuperMetroid.Core.Rendering;

/// <summary>Pure wall-clock scheduling decision after one actually completed emulated frame.</summary>
/// <param name="NextDeadline">Absolute next-frame deadline in seconds on the caller's monotonic clock.</param>
/// <param name="WaitSeconds">Requested nonnegative host wait in seconds; zero permits bounded catch-up without skipping an emulated frame.</param>
/// <param name="Rebased">Whether excessive wall-clock debt was discarded by scheduling one full frame period after the completion timestamp.</param>
public readonly record struct HostFrameDeadline(double NextDeadline, double WaitSeconds, bool Rebased)
{
    /// <summary>Computes the next host deadline at <see cref="HostFrameTimingDefinitions.FramesPerSecond"/> after exactly one completed emulated frame.</summary>
    /// <param name="previousDeadline">Previous absolute scheduled deadline in seconds, using the same monotonic clock as <paramref name="now"/>.</param>
    /// <param name="now">Completion timestamp of the frame just executed, in seconds.</param>
    /// <returns>The next absolute deadline, required host wait, and whether its schedule was rebased.</returns>
    /// <remarks>Advances the prior deadline by one period unless that candidate is more than two periods behind now; then it rebases to now plus one period. Ordinary lateness yields zero wait, not discarded gameplay, input, or audio. Clock values are not validated.</remarks>
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
