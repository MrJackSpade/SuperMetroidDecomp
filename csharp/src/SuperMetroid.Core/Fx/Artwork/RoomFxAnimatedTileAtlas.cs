using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>Editable two-bit characters for simple room-FX and Wrecked Ship treadmill frames.</summary>
public sealed class RoomFxAnimatedTileAtlas : IInstalledArtworkTransferSource
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

    /// <summary>Compiles indexed pixels back into their ordered native 2-bpp DMA frames.</summary>
    public static RoomFxAnimatedTileAtlas Load(Stream png,
        RoomFxAnimatedTileAtlas? stockForLegacyOverride = null)
    {
        Ensure.NotNull(png);
        long start = png.CanSeek ? png.Position : 0;
        try
        {
            IndexedPngImage image = IndexedPng.Read(png,
                new(RoomFxAnimatedTileAtlasFormat.Width, RoomFxAnimatedTileAtlasFormat.Height));
            return new(SnesPlanarTileEncoder.Encode(image.Pixels, image.Size,
                RoomFxAnimatedTileAtlasFormat.BitsPerPixel));
        }
        catch (InvalidDataException) when (stockForLegacyOverride is not null && png.CanSeek)
        {
            // Preserve previous user-edited PNGs from before the treadmill,
            // statue, spores or spike extension; inherit only the newly introduced tail.
            return TryLegacy(RoomFxAnimatedTileAtlasFormat.PreSpikesWidth)
                ?? TryLegacy(RoomFxAnimatedTileAtlasFormat.PreSporesWidth)
                ?? TryLegacy(RoomFxAnimatedTileAtlasFormat.PreStatueWidth)
                ?? TryLegacy(RoomFxAnimatedTileAtlasFormat.LegacyWidth)
                ?? throw new InvalidDataException("Room-FX PNG matches neither current nor supported legacy sheet geometry.");

            RoomFxAnimatedTileAtlas? TryLegacy(int width)
            {
                png.Position = start;
                try
                {
                    IndexedPngImage legacy = IndexedPng.Read(png, new(checked((ushort)width), RoomFxAnimatedTileAtlasFormat.Height));
                    byte[] legacyPlanar = SnesPlanarTileEncoder.Encode(legacy.Pixels,
                        legacy.Size, RoomFxAnimatedTileAtlasFormat.BitsPerPixel);
                    var combined = new byte[RoomFxAnimatedTileAtlasFormat.TotalByteCount];
                    legacyPlanar.CopyTo(combined, 0);
                    stockForLegacyOverride.characters.AsSpan(legacyPlanar.Length).CopyTo(
                        combined.AsSpan(legacyPlanar.Length));
                    return new(combined);
                }
                catch (InvalidDataException) { return null; }
            }
        }
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

    /// <summary>Publishes a selected installed frame through the synchronous liquid/rain owner.</summary>
    [PpuLayoutBoundary("codec: executes a native room-FX transfer record's encoded VRAM destination")]
    public ReadOnlyMemory<byte> Frame(int sourceAddress, ushort byteCount) =>
        TryResolve(sourceAddress, byteCount, out ReadOnlyMemory<byte> data) ? data
            : throw new InvalidDataException($"Room-FX frame ${sourceAddress:X6} is missing from installed artwork.");
}
