using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Map.Artwork;

/// <summary>Immutable compiled map characters from an indexed PNG, independent of any cartridge bus.</summary>
public sealed class MapTileAtlas
{
    /// <summary>Owned SNES 4-bpp character bytes compiled from indexed PNG pixels.</summary>
    private readonly byte[] planar;
    /// <summary>Retains an already compiled planar atlas for later VRAM transfers.</summary>
    internal MapTileAtlas(byte[] planar) => this.planar = planar;

    /// <summary>Compiles a 256-by-64 noninterlaced indexed PNG into 256 row-major 8-by-8 SNES 4-bpp characters, requiring pixel indices 0..15; PNG palette RGB values do not determine the runtime display palette.</summary>
    /// <param name="png">Caller-owned stream containing shared map artwork or the same-sized additional pause-menu atlas.</param>
    /// <returns>The immutable 8192-byte planar character atlas.</returns>
    public static MapTileAtlas Load(Stream png)
    {
        var image = IndexedPng.Read(png, new(MapTileAtlasFormat.Width, MapTileAtlasFormat.Height));
        return new(SnesPlanarTileEncoder.Encode(image.Pixels, image.Size, 4));
    }

    /// <summary>Copies compiled characters to the screen's own VRAM; source storage cannot be mutated by callers.</summary>
    /// <param name="destination">The screen's character region receiving the full 8192-byte atlas.</param>
    public void LoadTo(VramRegion destination) => destination.Load(planar);

    /// <summary>The planar 4-bpp characters (#1392).</summary>
    public ReadOnlySpan<byte> Characters => planar;
}
