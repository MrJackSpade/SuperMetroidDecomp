using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.Core.Fx;

/// <summary>One engine-owned room-FX animated-tile object: its frame size, frame schedule and the pictures its frames show.</summary>
public sealed class RoomFxAnimatedTileLoop
{
    /// <summary>Arithmetic view of timed records, retaining no presentation source pointers.</summary>
    internal readonly CalculatedFrames frames;

    /// <summary>Binds an animated-tile object to its frame size, frame schedule and distinct pictures.</summary>
    /// <param name="animatedObject">The object.</param>
    /// <param name="frameByteCount">The bytes of characters every frame shows.</param>
    /// <param name="frameCount">The timed frames in the loop.</param>
    /// <param name="frameDuration">The passes each frame is shown for.</param>
    /// <param name="imageCount">
    /// The distinct pictures the frames show. A loop with fewer pictures than frames plays them forward and back:
    /// the horizontal spikes' four frames show pictures 0, 1, 2 and 1.
    /// </param>
    internal RoomFxAnimatedTileLoop(
        AnimatedTileObject animatedObject,
        int frameByteCount,
        int frameCount,
        int frameDuration,
        int imageCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(frameCount);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(imageCount);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(imageCount, frameCount);

        AnimatedObject = animatedObject;
        FrameByteCount = frameByteCount;
        ImageCount = imageCount;
        frames = new CalculatedFrames(animatedObject, frameCount, frameDuration);
    }

    /// <summary>Bounded four-byte frame cursors with one native duration per object.</summary>
    /// <remarks>The seven simple native loops at $87:817E,8221,8235,8293,82B1,82CF,82ED
    /// have counts4,4,4,5,5,5,3. Spikes wait8 ticks, lava13, and all others10.
    /// Native timed entries consist of duration then artwork-source operand; goto
    /// follows the last entry. Both cursor and duration mappings are independently
    /// verified against NTSC J/U v1.0 and pinned bank_87.asm
    /// (362be646929cf8e483f692b73a6561cfc2dc1d0d). This view stores parameters only,
    /// never a generated frame table. Index bounds match the former read-only list.</remarks>
    internal sealed class CalculatedFrames(AnimatedTileObject animatedObject, int count, int duration)
        : IReadOnlyList<RoomFxAnimatedTileFrameDefinition>
    {
        /// <summary>Number of timed entries before the terminal goto command.</summary>
        public int Count => count;
        /// <summary>The bounded frame at <paramref name="index"/> with the object's shared duration.</summary>
        public RoomFxAnimatedTileFrameDefinition this[int index]
        {
            get
            {
                if ((uint)index >= (uint)count) throw new ArgumentOutOfRangeException(nameof(index));
                return new(animatedObject, index, duration);
            }
        }
        /// <summary>Yields frame cursors in native instruction order.</summary>
        public IEnumerator<RoomFxAnimatedTileFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < count; index++) yield return this[index];
        }
        /// <summary>Exposes the same native frame order to non-generic collection callers.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>The animated-tile object this definition schedules.</summary>
    public AnimatedTileObject AnimatedObject { get; }


    /// <summary>The bytes of characters every frame shows.</summary>
    public int FrameByteCount { get; }

    /// <summary>The distinct pictures the object's frames show, each held once in the installed atlas.</summary>
    public int ImageCount { get; }

    /// <summary>The timed frame-control records in cartridge execution order.</summary>
    public IReadOnlyList<RoomFxAnimatedTileFrameDefinition> Frames => frames;

    /// <summary>The distinct pictures the object's frames show, in order.</summary>
    public IEnumerable<AnimatedTileImage> Images => Enumerable.Range(0, ImageCount).Select(index => new AnimatedTileImage(AnimatedObject, index));

    /// <summary>The picture <paramref name="frame"/> shows: past the last picture, the loop plays its pictures back.</summary>
    /// <exception cref="InvalidDataException"><paramref name="frame"/> is not one of this object's frames.</exception>
    public AnimatedTileImage Image(RoomFxAnimatedTileFrameDefinition frame)
    {
        if (frame.Object != AnimatedObject || (uint)frame.Index >= (uint)Frames.Count)
            throw new InvalidDataException(
                $"Room-FX object {AnimatedObject} has no artwork frame {frame.Index} of {frame.Object}.");
        int index = frame.Index < ImageCount ? frame.Index : 2 * (ImageCount - 1) - frame.Index;
        return new(AnimatedObject, index);
    }
}
