using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// One looping room-FX animation: the named frames it shows, in order, each with the passes it is shown for. The loop
/// returns to its first frame after its last.
/// </summary>
public sealed class RoomFxBlockAnimationLoop
{
    /// <summary>The loop's frames, in the order it shows them.</summary>
    private readonly TimedArtworkFrame[] frames;

    /// <summary>
    /// Builds the loop of <paramref name="animatedObject"/>: <paramref name="frameCount"/> frames, each shown for
    /// <paramref name="frameDuration"/> passes.
    /// </summary>
    /// <param name="animatedObject">The animation.</param>
    /// <param name="frameCount">The frames in the loop.</param>
    /// <param name="frameDuration">The passes each frame is shown for.</param>
    /// <exception cref="ArgumentOutOfRangeException">The loop has no frame, or a frame is shown for no pass.</exception>
    internal RoomFxBlockAnimationLoop(
        BlockAnimation animatedObject,
        int frameCount,
        int frameDuration)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(frameCount);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(frameDuration);

        AnimatedObject = animatedObject;
        frames = [.. Enumerable.Range(0, frameCount)
            .Select(frame => new TimedArtworkFrame(BlockAnimationFrames.Frame(animatedObject, frame), frameDuration))];
    }

    /// <summary>The animation this loop shows.</summary>
    public BlockAnimation AnimatedObject { get; }

    /// <summary>The loop's frames, in the order it shows them, each with the passes it is shown for.</summary>
    public IReadOnlyList<TimedArtworkFrame> Frames => frames;
}
