using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>Reads the installed room-FX PNG into the two-bit characters of its ordered native frames (#1409).</summary>
public static class RoomFxBlockAnimationAtlasPng
{
    /// <summary>Compiles indexed pixels back into their ordered native 2-bpp DMA frames.</summary>
    /// <param name="png">The installed room-FX PNG.</param>
    /// <exception cref="InvalidDataException">The PNG does not have the current sheet geometry.</exception>
    public static RoomFxBlockAnimationAtlas Read(Stream png)
    {
        Ensure.NotNull(png);
        IndexedPngImage image = IndexedPng.Read(png,
            new(RoomFxBlockAnimationAtlasFile.Width, RoomFxBlockAnimationAtlasFile.Height));
        return new(SnesPlanarTileEncoder.Encode(image.Pixels, image.Size,
            RoomFxBlockAnimationAtlasFile.BitsPerPixel));
    }
}
