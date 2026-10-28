namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>
/// The installed two-bit characters of the simple room-FX, Tourian statue and Wrecked Ship treadmill frames (#1409),
/// compiled by the import pipeline. They remain bytes here because the Tourian statue's OBJ character source resolves them.
/// </summary>
public sealed class RoomFxAnimatedTileAtlas
{
    /// <summary>Concatenated edited tile bytes in the native segment order.</summary>
    internal readonly byte[] characters;

    /// <summary>Accepts the compiled character strip only when its length matches the ordered frame and statue segments.</summary>
    internal RoomFxAnimatedTileAtlas(byte[] characters)
    {
        this.characters = characters;
        int offset = 0;
        foreach (RoomFxAtlasSegment segment in RoomFxAnimatedTileAtlasFormat.Segments)
            offset += segment.ByteCount;
        if (offset != characters.Length)
            throw new InvalidDataException(
                $"Room-FX artwork has {characters.Length} bytes, expected {offset}.");
    }

    /// <summary>Returns the characters of one complete picture, rejecting altered picture geometry.</summary>
    public bool TryResolve(AnimatedTileImage image, int byteCount, out ReadOnlyMemory<byte> data)
    {
        int offset = 0;
        foreach (RoomFxAtlasSegment segment in RoomFxAnimatedTileAtlasFormat.Segments)
        {
            if (segment is RoomFxAtlasSegment.Image held && held.Picture == image)
            {
                if (byteCount != segment.ByteCount)
                    throw new InvalidDataException(
                        $"Room-FX picture {image} requires {segment.ByteCount} bytes, not {byteCount}.");
                data = characters.AsMemory(offset, byteCount);
                return true;
            }
            offset += segment.ByteCount;
        }
        data = default;
        return false;
    }

    /// <summary>Returns the characters of <paramref name="picture"/> of <paramref name="statue"/>, a window of the shared statue strip.</summary>
    /// <exception cref="InvalidDataException"><paramref name="byteCount"/> is not the statue's picture size, or <paramref name="statue"/> is not a statue.</exception>
    public ReadOnlyMemory<byte> TourianStatuePicture(AnimatedTileObject statue, TourianStatueTilePicture picture, int byteCount)
    {
        int windowByteCount = TourianStatueTileStrip.WindowByteCount(statue);
        if (byteCount != windowByteCount)
            throw new InvalidDataException(
                $"Tourian statue {statue} {picture} requires {windowByteCount} bytes, not {byteCount}.");
        int offset = (RoomFxAnimatedTileAtlasFormat.PreStatueTileCount + TourianStatueTileStrip.FirstTile(statue, picture)) *
            TourianStatueTileStripDefinitions.BytesPerTile;
        return characters.AsMemory(offset, byteCount);
    }
}
