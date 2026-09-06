using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Desktop;

/// <summary>
/// Runs one bounded host catch-up batch, retaining only the last completed frame
/// for presentation. The delegates expose the live polling boundary to deterministic
/// tests without a window, physical controller, audio device, or wall-clock sleeps.
/// </summary>
internal static class PlaybackFrameBatch
{
    internal static (int CompletedFrames, FrontendFrame? LastFrame) Run(
        int requestedFrames,
        Func<ushort> readInput,
        Func<ushort, FrontendFrame?> advanceFrame)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(requestedFrames);
        ArgumentNullException.ThrowIfNull(readInput);
        ArgumentNullException.ThrowIfNull(advanceFrame);
        if (requestedFrames == 0) return (0, null);
        FrontendFrame? lastFrame = null;
        int completed = 0;
        for (int index = 0; index < requestedFrames; index++)
        {
            // Rendering/audio can consume enough wall time for a complete pad tap
            // inside this batch. Poll at each emulated frame boundary, just as the
            // single-step path does; never reuse a stale word for catch-up frames.
            FrontendFrame? next = advanceFrame(readInput());
            if (next is null) break;
            lastFrame = next;
            completed++;
        }
        return (completed, lastFrame);
    }
}
