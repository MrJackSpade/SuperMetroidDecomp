using SuperMetroid.Core.Input;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Desktop.Timing;

/// <summary>
/// Runs one bounded host catch-up batch, retaining only the last completed frame
/// for presentation. The delegates expose the live polling boundary to deterministic
/// tests without a window, physical controller, audio device, or wall-clock sleeps.
/// </summary>
internal static class PlaybackFrameBatch
{
    /// <summary>Advances up to the requested count, polling input anew for each frame and retaining only the last completed result.</summary>
    /// <remarks>Stops before polling when the host gate closes, or after an advance returns no frame; zero requested frames do not poll.</remarks>
    internal static PlaybackBatchResult Run(
        int requestedFrames,
        Func<SnesButton> readInput,
        Func<SnesButton, FrontendFrame?> advanceFrame,
        Func<bool>? canAdvance = null)
    {
        Ensure.AtLeastZero(requestedFrames);
        Ensure.NotNull(readInput);
        Ensure.NotNull(advanceFrame);
        if (requestedFrames == 0) return new PlaybackBatchResult(0, null);
        FrontendFrame? lastFrame = null;
        int completed = 0;
        for (int index = 0; index < requestedFrames; index++)
        {
            if (canAdvance is not null && !canAdvance()) break;
            // Rendering/audio can consume a complete pad tap inside a catch-up batch.
            FrontendFrame? next = advanceFrame(readInput());
            if (next is null) break;
            lastFrame = next;
            completed++;
        }
        return new PlaybackBatchResult(completed, lastFrame);
    }
}
