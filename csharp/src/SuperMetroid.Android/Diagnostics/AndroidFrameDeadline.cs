namespace SuperMetroid.Android.Diagnostics;

/// <summary>The Android host's wall-clock scheduling decision after one actually completed simulation step.</summary>
/// <param name="NextDeadline">Absolute next-frame deadline in seconds on the caller's monotonic clock.</param>
/// <param name="WaitSeconds">Requested nonnegative host wait in seconds; zero permits bounded catch-up without skipping a simulation step.</param>
/// <param name="Rebased">Whether excessive wall-clock debt was discarded by scheduling one full frame period after the completion timestamp.</param>
internal readonly record struct AndroidFrameDeadline(double NextDeadline, double WaitSeconds, bool Rebased)
{
    /// <summary>Computes the next host deadline at <see cref="AndroidFrameTimingDefinitions.FramesPerSecond"/> after exactly one completed simulation step.</summary>
    /// <param name="previousDeadline">Previous absolute scheduled deadline in seconds, using the same monotonic clock as <paramref name="now"/>.</param>
    /// <param name="now">Completion timestamp of the step just executed, in seconds.</param>
    /// <returns>The next absolute deadline, required host wait, and whether its schedule was rebased.</returns>
    /// <remarks>Advances the prior deadline by one period unless that candidate is more than two periods behind now; then it rebases to now plus one period. Ordinary lateness yields zero wait, not discarded gameplay, input, or audio. Clock values are not validated.</remarks>
    public static AndroidFrameDeadline AfterFrame(double previousDeadline, double now)
    {
        double period = 1 / AndroidFrameTimingDefinitions.FramesPerSecond;
        double next = previousDeadline + period;
        double wait = next - now;
        if (wait < -AndroidFrameTimingDefinitions.MaximumCatchUpFrames * period)
            // Only obsolete wall-clock debt is discarded. The caller has executed
            // exactly one step and retains every input/audio sample. Allow a full
            // period before the next step so rebasing does not cause another burst.
            return new(now + period, period, true);
        return new(next, Math.Max(0, wait), false);
    }
}
