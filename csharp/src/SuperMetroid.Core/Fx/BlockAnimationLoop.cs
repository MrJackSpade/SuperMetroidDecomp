using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// One looping block animation (#1988, #2361): the named frames it shows, in order, each with the passes it is shown for,
/// and when it starts showing them. The loop returns to its first frame after its last.
/// </summary>
public sealed class BlockAnimationLoop
{
    /// <summary>Builds the loop of <paramref name="animation"/> over its authored <paramref name="frames"/>.</summary>
    /// <param name="animation">The animation.</param>
    /// <param name="start">When the loop starts showing its frames.</param>
    /// <param name="frames">The loop's frames in the order it shows them, each with the passes it is shown for.</param>
    /// <exception cref="ArgumentException">The loop has no frame, a frame belongs to another animation, or a frame is shown for no pass.</exception>
    internal BlockAnimationLoop(BlockAnimation animation, BlockAnimationStart start, ImmutableArray<TimedArtworkFrame> frames)
    {
        if (frames.IsDefaultOrEmpty)
            throw new ArgumentException($"The {animation} loop shows at least one frame.", nameof(frames));
        foreach (TimedArtworkFrame frame in frames)
        {
            if (frame.Frame.Animation != BlockAnimationFrames.Of(animation))
                throw new ArgumentException($"{frame.Frame} is not a frame of the {animation} loop.", nameof(frames));
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(frame.Duration, nameof(frames));
        }
        Animation = animation;
        Start = start;
        Frames = frames;
    }

    /// <summary>The animation this loop shows.</summary>
    public BlockAnimation Animation { get; }

    /// <summary>When the loop starts showing its frames.</summary>
    public BlockAnimationStart Start { get; }

    /// <summary>The loop's frames, in the order it shows them, each with the passes it is shown for.</summary>
    public IReadOnlyList<TimedArtworkFrame> Frames { get; }
}
