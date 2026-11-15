using SuperMetroid.Core.Fx;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// The import's view of the simple room-FX block-animation loops (#1502): the seven loops the atlas holds, in atlas order,
/// each loop's distinct pictures and their sizes, and the byte lengths of the treadmill frames and the Tourian statue strip. The game
/// resolves one loop at a time and draws the installed pictures.
/// </summary>
public static class RoomFxBlockAnimationLoops
{
    /// <summary>The bytes of characters every Wrecked Ship treadmill frame shows: two characters.</summary>
    public const int TreadmillFrameByteCount = 32;

    /// <summary>The Tourian statue strip's length in bytes.</summary>
    public const int TourianStatueStripByteCount = RoomFxBlockAnimationAtlasFormat.StatueTileCount * TourianStatueTileStripDefinitions.CharacterByteCount;

    /// <summary>The loop of an animation the atlas holds.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="animation"/> has no loop.</exception>
    internal static BlockAnimationLoop Require(BlockAnimation animation) => BlockAnimationLoops.Of(animation);

    /// <summary>Enumerates the seven loops whose every picture the atlas holds as its own, in atlas order.</summary>
    public static IEnumerable<BlockAnimationLoop> All
    {
        get
        {
            yield return Require(BlockAnimation.HorizontalSpikes);
            yield return Require(BlockAnimation.MaridiaSandCeiling);
            yield return Require(BlockAnimation.MaridiaSandFalling);
            yield return Require(BlockAnimation.Lava);
            yield return Require(BlockAnimation.Acid);
            yield return Require(BlockAnimation.Rain);
            yield return Require(BlockAnimation.Spores);
        }
    }

    /// <summary>The distinct pictures one loop's frames show, as the installed atlas stores them (#1527).</summary>
    extension(BlockAnimationLoop loop)
    {
        /// <summary>The distinct pictures the object's frames show, each held once in the installed atlas.</summary>
        /// <exception cref="InvalidDataException">
        /// The object has no pictures in the atlas, or its picture count is not between one and its frame count.
        /// </exception>
        internal int ImageCount
        {
            get
            {
                if (!RoomFxBlockAnimationPictureCountDefinitions.PictureCounts.TryGetValue(loop.Animation, out int count))
                    throw new InvalidDataException($"{loop.Animation} has no pictures in the room-FX atlas.");
                if (count <= 0 || count > loop.Frames.Count)
                    throw new InvalidDataException(
                        $"{loop.Animation} shows {count} pictures over {loop.Frames.Count} frames.");
                return count;
            }
        }

        /// <summary>The bytes of characters each of the object's pictures holds.</summary>
        /// <exception cref="InvalidDataException">The object has no pictures in the atlas.</exception>
        internal int PictureByteCount =>
            RoomFxBlockAnimationPictureSizeDefinitions.PictureByteCounts.TryGetValue(loop.Animation, out int byteCount) ? byteCount
                : throw new InvalidDataException($"{loop.Animation} has no pictures in the room-FX atlas.");

        /// <summary>The distinct pictures the object's frames show, in order.</summary>
        internal IEnumerable<AnimatedTileImage> Images =>
            Enumerable.Range(0, loop.ImageCount).Select(index => new AnimatedTileImage(loop.Animation, index));

        /// <summary>
        /// The picture the loop's frame <paramref name="frameIndex"/> shows: past the last picture, the loop plays its
        /// pictures back.
        /// </summary>
        /// <exception cref="InvalidDataException"><paramref name="frameIndex"/> is not one of this object's frames.</exception>
        internal AnimatedTileImage Image(int frameIndex)
        {
            if ((uint)frameIndex >= (uint)loop.Frames.Count)
                throw new InvalidDataException(
                    $"Room-FX object {loop.Animation} has no artwork frame {frameIndex}.");
            int count = loop.ImageCount;
            int index = frameIndex < count ? frameIndex : 2 * (count - 1) - frameIndex;
            return new(loop.Animation, index);
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
            if (loop.Animation == BlockAnimation.WreckedShipTreadmillRightwards)
                return new(BlockAnimation.WreckedShipTreadmillRightwards, frameIndex);
            if (loop.Animation == BlockAnimation.WreckedShipTreadmillLeftwards)
                return new(BlockAnimation.WreckedShipTreadmillRightwards, loop.Frames.Count - 1 - frameIndex);
            throw new InvalidDataException($"{loop.Animation} is not a Wrecked Ship treadmill.");
        }
    }
}
