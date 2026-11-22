using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// The import's view of the simple room-FX block-animation loops (#1502, #2422): the seven loops the atlas holds, in atlas
/// order, each loop's frames, its distinct pictures and their sizes, and the byte lengths of the treadmill frames and the
/// Tourian statue strip. The game plays one loop at a time and draws the installed pictures.
/// </summary>
public static partial class RoomFxBlockAnimationLoops
{
    /// <summary>The bytes of characters every Wrecked Ship treadmill frame shows: two characters.</summary>
    public const int TreadmillFrameByteCount = 32;

    /// <summary>The Tourian statue strip's length in bytes.</summary>
    public const int TourianStatueStripByteCount = RoomFxBlockAnimationAtlasFormat.StatueTileCount * TourianStatueTileStripDefinitions.CharacterByteCount;

    /// <summary><paramref name="animation"/>, an animation the atlas holds a loop of.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="animation"/> has no loop.</exception>
    internal static BlockAnimation Require(BlockAnimation animation) => BlockAnimationLoops.HasLoop(animation)
        ? animation
        : throw new InvalidOperationException($"{animation} has no block animation loop.");

    /// <summary>One loop's frames and the distinct pictures they show, as the import's atlas stores them (#1527).</summary>
    extension(BlockAnimation loop)
    {
        /// <summary>The frames the loop shows, in order, each with the passes it is shown for (<see cref="BlockAnimationLoops.Frames"/>).</summary>
        /// <exception cref="InvalidOperationException">The animation has no loop.</exception>
        internal IReadOnlyList<TimedArtworkFrame> Frames => BlockAnimationLoops.Frames(loop);

        /// <summary>The distinct pictures the object's frames show, each held once in the import's atlas.</summary>
        /// <exception cref="InvalidDataException">
        /// The object has no pictures in the atlas, or its picture count is not between one and its frame count.
        /// </exception>
        internal int ImageCount
        {
            get
            {
                if (!RoomFxBlockAnimationPictureCountDefinitions.PictureCounts.TryGetValue(loop, out int count))
                    throw new InvalidDataException($"{loop} has no pictures in the room-FX atlas.");
                if (count <= 0 || count > loop.Frames.Count)
                    throw new InvalidDataException(
                        $"{loop} shows {count} pictures over {loop.Frames.Count} frames.");
                return count;
            }
        }

        /// <summary>The bytes of characters each of the object's pictures holds.</summary>
        /// <exception cref="InvalidDataException">The object has no pictures in the atlas.</exception>
        internal int PictureByteCount =>
            RoomFxBlockAnimationPictureSizeDefinitions.PictureByteCounts.TryGetValue(loop, out int byteCount) ? byteCount
                : throw new InvalidDataException($"{loop} has no pictures in the room-FX atlas.");

        /// <summary>The distinct pictures the object's frames show, in order.</summary>
        internal IEnumerable<AnimatedTileImage> Images =>
            Enumerable.Range(0, loop.ImageCount).Select(index => new AnimatedTileImage(loop, index));

        /// <summary>
        /// The picture the loop's frame <paramref name="frameIndex"/> shows: past the last picture, the loop plays its
        /// pictures back.
        /// </summary>
        /// <exception cref="InvalidDataException"><paramref name="frameIndex"/> is not one of this object's frames.</exception>
        internal AnimatedTileImage Image(int frameIndex)
        {
            if ((uint)frameIndex >= (uint)loop.Frames.Count)
                throw new InvalidDataException(
                    $"Room-FX object {loop} has no artwork frame {frameIndex}.");
            int count = loop.ImageCount;
            int index = frameIndex < count ? frameIndex : 2 * (count - 1) - frameIndex;
            return new(loop, index);
        }

        /// <summary>
        /// The picture a Wrecked Ship treadmill loop's frame <paramref name="frameIndex"/> shows (#1527): both directions
        /// show the rightward treadmill's pictures, the leftward loop in reverse order.
        /// </summary>
        /// <exception cref="InvalidDataException">The loop is not a treadmill's.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="frameIndex"/> is not one of the loop's frames.</exception>
        internal AnimatedTileImage TreadmillImage(int frameIndex)
        {
            if ((uint)frameIndex >= (uint)loop.Frames.Count)
                throw new ArgumentOutOfRangeException(nameof(frameIndex));
            if (loop == BlockAnimation.WreckedShipTreadmillRightwards)
                return new(BlockAnimation.WreckedShipTreadmillRightwards, frameIndex);
            if (loop == BlockAnimation.WreckedShipTreadmillLeftwards)
                return new(BlockAnimation.WreckedShipTreadmillRightwards, loop.Frames.Count - 1 - frameIndex);
            throw new InvalidDataException($"{loop} is not a Wrecked Ship treadmill.");
        }
    }
}
