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

    /// <summary>Returns one complete native frame, rejecting altered frame geometry.</summary>
    public bool TryResolve(int sourceAddress, int byteCount, out ReadOnlyMemory<byte> data)
    {
        int frameOffset = 0;
        foreach (RoomFxAtlasSegment segment in RoomFxAnimatedTileAtlasFormat.Segments)
        {
            if (segment.IsFrame && segment.SourceAddress == sourceAddress)
            {
                if (byteCount != segment.ByteCount)
                    throw new InvalidDataException(
                        $"Room-FX artwork source ${sourceAddress:X6} requires {segment.ByteCount} bytes, not {byteCount}.");
                data = characters.AsMemory(frameOffset, byteCount);
                return true;
            }
            frameOffset += segment.ByteCount;
        }
        // Statue operands name overlapping windows of the shared strip.
        foreach (AnimatedTileObject statue in TourianStatueTileStripDefinitions.Statues)
        foreach (TourianStatueTilePicture picture in TourianStatueTileStripDefinitions.Pictures)
        {
            if (TourianStatueTileStripDefinitions.SourceAddress(statue, picture) != sourceAddress)
                continue;
            int windowByteCount = TourianStatueTileStripDefinitions.WindowByteCount(statue);
            if (byteCount != windowByteCount)
                throw new InvalidDataException(
                    $"Tourian statue art ${sourceAddress:X6} requires {windowByteCount} bytes, not {byteCount}.");
            int offset = RoomFxAnimatedTileAtlasFormat.PreStatueTileCount * 16 +
                sourceAddress - TourianStatueTileStripDefinitions.FirstSource;
            data = characters.AsMemory(offset, byteCount);
            return true;
        }
        data = default;
        return false;
    }
}
