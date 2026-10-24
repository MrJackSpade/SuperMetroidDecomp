using SuperMetroid.Core;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>Reads the installed room-FX PNG into the two-bit characters of its ordered native frames (#1409).</summary>
public static class RoomFxAnimatedTileAtlasPng
{
    /// <summary>Compiles indexed pixels back into their ordered native 2-bpp DMA frames.</summary>
    /// <param name="png">The installed or override room-FX PNG.</param>
    /// <param name="stockForLegacyOverride">Current stock characters, whose newer tail an older override inherits.</param>
    /// <exception cref="InvalidDataException">The PNG matches neither the current nor a supported legacy sheet.</exception>
    public static RoomFxAnimatedTileAtlas Read(Stream png,
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
}
