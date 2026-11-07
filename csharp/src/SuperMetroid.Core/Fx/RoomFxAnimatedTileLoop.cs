namespace SuperMetroid.Core.Fx;

/// <summary>
/// One engine-owned room-FX animated-tile object: its frame size and frame schedule. Every frame of a loop is shown for
/// the same number of passes, and the loop returns to its first frame after its last.
/// </summary>
public sealed class RoomFxAnimatedTileLoop
{
    /// <summary>Binds an animated-tile object to its frame size and frame schedule.</summary>
    /// <param name="animatedObject">The object.</param>
    /// <param name="frameByteCount">The bytes of characters every frame shows.</param>
    /// <param name="frameCount">The timed frames in the loop.</param>
    /// <param name="frameDuration">The passes each frame is shown for.</param>
    /// <exception cref="ArgumentOutOfRangeException">The loop has no frame, or a frame is shown for no pass.</exception>
    internal RoomFxAnimatedTileLoop(
        AnimatedTileObject animatedObject,
        int frameByteCount,
        int frameCount,
        int frameDuration)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(frameCount);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(frameDuration);

        AnimatedObject = animatedObject;
        FrameByteCount = frameByteCount;
        FrameCount = frameCount;
        FrameDuration = frameDuration;
    }

    /// <summary>The animated-tile object this definition schedules.</summary>
    public AnimatedTileObject AnimatedObject { get; }

    /// <summary>The bytes of characters every frame shows.</summary>
    public int FrameByteCount { get; }

    /// <summary>The timed frames in the loop.</summary>
    public int FrameCount { get; }

    /// <summary>The passes each frame is shown for.</summary>
    public int FrameDuration { get; }
}
