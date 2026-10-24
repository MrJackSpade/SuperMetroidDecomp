using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Map.Artwork;

namespace SuperMetroid.AssetExtraction.Hud;

/// <summary>Reads the installed HUD PNG into the HUD's two-bit characters and the cleared half its setup shows (#1409).</summary>
public static class HudTileAtlasPng
{
    /// <summary>Compiles a 256-by-64 indexed PNG into 256 row-major 8-by-8 2-bpp HUD/minimap characters and appends the native zero-filled clearing half.</summary>
    /// <param name="png">Readable PNG stream at its current position; it remains open and caller-owned. Pixel indices must be 0..3; PNG RGB palette colors do not replace runtime CGRAM.</param>
    /// <returns>Independently owned artwork: $1000 bytes of planar characters followed by $1000 zero bytes.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="png"/> is null.</exception>
    /// <exception cref="InvalidDataException">PNG is malformed, has unsupported features or incorrect dimensions, or uses a pixel index outside the 2-bpp range.</exception>
    public static HudTileAtlas Read(Stream png)
    {
        var image = IndexedPng.Read(png, new(MapTileAtlasFormat.Width, MapTileAtlasFormat.Height));
        byte[] planar = SnesPlanarTileEncoder.Encode(image.Pixels, image.Size, 2);
        var bytes = new byte[HudTileAtlasFormat.ByteCount];
        planar.CopyTo(bytes, 0);
        return new(bytes);
    }
}
