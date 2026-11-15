using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// One looping block animation (#1988): the named frames it shows, in order, each with the passes it is shown for, and
/// when it starts showing them. The loop returns to its first frame after its last.
/// </summary>
public sealed class BlockAnimationLoop
{
    /// <summary>The loop's frames, in the order it shows them.</summary>
    private readonly TimedArtworkFrame[] frames;

    /// <summary>
    /// Builds the loop of <paramref name="animation"/>: <paramref name="frameCount"/> frames, each shown for
    /// <paramref name="frameDuration"/> passes, started as <paramref name="start"/> says.
    /// </summary>
    /// <param name="animation">The animation.</param>
    /// <param name="frameCount">The frames in the loop.</param>
    /// <param name="frameDuration">The passes each frame is shown for.</param>
    /// <param name="start">When the loop starts showing its frames.</param>
    /// <exception cref="ArgumentOutOfRangeException">The loop has no frame, or a frame is shown for no pass.</exception>
    internal BlockAnimationLoop(BlockAnimation animation, int frameCount, int frameDuration, BlockAnimationStart start)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(frameCount);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(frameDuration);

        Animation = animation;
        Start = start;
        frames = [.. Enumerable.Range(0, frameCount)
            .Select(frame => new TimedArtworkFrame(BlockAnimationFrames.Frame(animation, frame), frameDuration))];
    }

    /// <summary>The animation this loop shows.</summary>
    public BlockAnimation Animation { get; }

    /// <summary>When the loop starts showing its frames.</summary>
    public BlockAnimationStart Start { get; }

    /// <summary>The loop's frames, in the order it shows them, each with the passes it is shown for.</summary>
    public IReadOnlyList<TimedArtworkFrame> Frames => frames;
}
