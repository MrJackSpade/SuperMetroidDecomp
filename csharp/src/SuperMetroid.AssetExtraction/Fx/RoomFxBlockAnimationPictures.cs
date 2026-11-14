using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// The characters of each picture the installed room-FX atlas holds (#1415): the import pipeline composes room sheets and
/// the Tourian statues' OBJ pictures from them. Pictures are found by their place in the atlas's segment order, and the
/// statues' pictures as windows of the shared statue strip (<see cref="TourianStatueTileStripDefinitions"/>).
/// </summary>
internal static class RoomFxBlockAnimationPictures
{
    /// <summary>The pictures each Tourian statue shows, each one window of the shared strip.</summary>
    internal static IReadOnlyList<TourianStatueBlockPicture> TourianStatuePictures { get; } = Enum.GetValues<TourianStatueBlockPicture>();

    /// <summary>The windows of <paramref name="statue"/> in the shared strip.</summary>
    /// <exception cref="InvalidDataException"><paramref name="statue"/> is not one statue.</exception>
    internal static TourianStatueTileWindow TourianStatueWindow(TourianGreyStatues statue) =>
        TourianStatueTileStripDefinitions.Windows.TryGetValue(statue, out TourianStatueTileWindow window)
            ? window : throw new InvalidDataException($"Unknown Tourian statue {statue}.");

    /// <summary>The strip character <paramref name="picture"/> of <paramref name="statue"/> starts at.</summary>
    /// <exception cref="InvalidDataException"><paramref name="statue"/> is not a statue, or the picture is undefined.</exception>
    internal static int TourianStatueFirstTile(TourianGreyStatues statue, TourianStatueBlockPicture picture)
    {
        TourianStatueTileWindow window = TourianStatueWindow(statue);
        return picture switch
        {
            TourianStatueBlockPicture.OscillationFirst => window.First,
            TourianStatueBlockPicture.OscillationSecond => window.First + window.Tiles,
            TourianStatueBlockPicture.OscillationThird => window.First + 2 * window.Tiles,
            TourianStatueBlockPicture.Released => window.Released,
            _ => throw new InvalidDataException($"Undefined {nameof(TourianStatueBlockPicture)} {picture}."),
        };
    }

    /// <summary>The bytes of characters one picture of <paramref name="statue"/> holds.</summary>
    /// <exception cref="InvalidDataException"><paramref name="statue"/> is not one statue.</exception>
    internal static int TourianStatuePictureByteCount(TourianGreyStatues statue) =>
        TourianStatueWindow(statue).Tiles * TourianStatueTileStripDefinitions.CharacterByteCount;

    /// <summary>The pictures of one installed atlas.</summary>
    extension(RoomFxBlockAnimationAtlas atlas)
    {
        /// <summary>Returns the characters of one complete picture, rejecting altered picture geometry.</summary>
        /// <exception cref="InvalidDataException"><paramref name="byteCount"/> is not the picture's size.</exception>
        internal bool TryResolve(AnimatedTileImage image, int byteCount, out ReadOnlyMemory<byte> data)
        {
            int offset = 0;
            foreach (RoomFxAtlasSegment segment in RoomFxBlockAnimationAtlasFile.Segments)
            {
                if (segment is RoomFxAtlasSegment.Image held && held.Picture == image)
                {
                    if (byteCount != segment.ByteCount)
                        throw new InvalidDataException(
                            $"Room-FX picture {image} requires {segment.ByteCount} bytes, not {byteCount}.");
                    data = atlas.characters.AsMemory(offset, byteCount);
                    return true;
                }
                offset += segment.ByteCount;
            }
            data = default;
            return false;
        }

        /// <summary>Returns the characters of <paramref name="picture"/> of <paramref name="statue"/>, a window of the shared statue strip.</summary>
        /// <exception cref="InvalidDataException"><paramref name="statue"/> is not one statue.</exception>
        internal ReadOnlyMemory<byte> TourianStatuePicture(TourianGreyStatues statue, TourianStatueBlockPicture picture)
        {
            int offset = (RoomFxBlockAnimationAtlasFormat.PreStatueTileCount + TourianStatueFirstTile(statue, picture)) *
                TourianStatueTileStripDefinitions.CharacterByteCount;
            return atlas.characters.AsMemory(offset, TourianStatuePictureByteCount(statue));
        }
    }
}
