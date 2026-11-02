using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// The characters of each picture the installed room-FX atlas holds (#1415): the import pipeline composes room sheets and
/// the Tourian statues' OBJ pictures from them. Pictures are found by their place in the atlas's segment order, and the
/// statues' pictures as windows of the shared statue strip.
/// </summary>
internal static class RoomFxAnimatedTilePictures
{
    /// <summary>The pictures each Tourian statue shows, each one window of the shared strip.</summary>
    internal static IReadOnlyList<TourianStatueTilePicture> TourianStatuePictures { get; } = Enum.GetValues<TourianStatueTilePicture>();

    /// <summary>The strip character <paramref name="picture"/> of <paramref name="statue"/> starts at.</summary>
    /// <exception cref="InvalidDataException"><paramref name="statue"/> is not a statue, or the picture is undefined.</exception>
    internal static int TourianStatueFirstTile(AnimatedTileObject statue, TourianStatueTilePicture picture)
    {
        TourianStatueTileWindow window = TourianStatueTileStripDefinitions.Windows.TryGetValue(statue, out TourianStatueTileWindow found)
            ? found : throw new InvalidDataException($"Unknown Tourian statue {statue}.");
        return picture switch
        {
            TourianStatueTilePicture.OscillationFirst => window.First,
            TourianStatueTilePicture.OscillationSecond => window.First + window.Tiles,
            TourianStatueTilePicture.OscillationThird => window.First + 2 * window.Tiles,
            TourianStatueTilePicture.Released => window.Released,
            _ => throw new InvalidDataException($"Undefined {nameof(TourianStatueTilePicture)} {picture}."),
        };
    }

    /// <summary>The pictures of one installed atlas.</summary>
    extension(RoomFxAnimatedTileAtlas atlas)
    {
        /// <summary>Returns the characters of one complete picture, rejecting altered picture geometry.</summary>
        /// <exception cref="InvalidDataException"><paramref name="byteCount"/> is not the picture's size.</exception>
        internal bool TryResolve(AnimatedTileImage image, int byteCount, out ReadOnlyMemory<byte> data)
        {
            int offset = 0;
            foreach (RoomFxAtlasSegment segment in RoomFxAnimatedTileAtlasFile.Segments)
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
        /// <exception cref="InvalidDataException"><paramref name="byteCount"/> is not the statue's picture size, or <paramref name="statue"/> is not a statue.</exception>
        internal ReadOnlyMemory<byte> TourianStatuePicture(AnimatedTileObject statue, TourianStatueTilePicture picture, int byteCount)
        {
            int windowByteCount = TourianStatueTileStrip.WindowByteCount(statue);
            if (byteCount != windowByteCount)
                throw new InvalidDataException(
                    $"Tourian statue {statue} {picture} requires {windowByteCount} bytes, not {byteCount}.");
            int offset = (RoomFxAnimatedTileAtlasFormat.PreStatueTileCount + TourianStatueFirstTile(statue, picture)) *
                TourianStatueTileStripDefinitions.BytesPerTile;
            return atlas.characters.AsMemory(offset, byteCount);
        }
    }
}
