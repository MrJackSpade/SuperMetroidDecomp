using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Immutable compiled map characters from an indexed PNG, independent of any cartridge bus.</summary>
public sealed class MapTileAtlas
{
    private readonly byte[] planar;
    private MapTileAtlas(byte[] planar) => this.planar = planar;

    public static MapTileAtlas Load(Stream png)
    {
        var image = IndexedPng.Read(png, MapTileAtlasFormat.Width, MapTileAtlasFormat.Height);
        var bytes = new byte[MapTileAtlasFormat.ByteCount];
        for (int y = 0; y < image.Height; y++)
        for (int x = 0; x < image.Width; x++)
        {
            byte pixel = image.Pixels[y * image.Width + x];
            if (pixel >= MapTileAtlasFormat.ColorCount)
                throw new InvalidDataException($"Map atlas pixel ({x},{y}) exceeds its 16-color index range.");
            int tile = y / 8 * MapTileAtlasFormat.TileColumns + x / 8;
            for (int plane = 0; plane < 4; plane++)
                bytes[tile * 32 + plane / 2 * 16 + y % 8 * 2 + plane % 2] |=
                    (byte)(((pixel >> plane) & 1) << (7 - x % 8));
        }
        return new(bytes);
    }

    /// <summary>Copies compiled characters to the screen's own VRAM; source storage cannot be mutated by callers.</summary>
    public void LoadTo(SnesVram vram, int destinationByteAddress) => vram.LoadBytes(destinationByteAddress, planar);
}

/// <summary>Shared pause/file-select character atlas geometry and fixed installed filename.</summary>
public static class MapTileAtlasFormat
{
    public const string FileName = "map-tiles.png";
    public const int TileColumns = 32;
    public const int Width = TileColumns * 8;
    public const int Height = 64;
    public const int ColorCount = 16;
    public const int ByteCount = Width * Height / 2;
    /// <summary>$B6:8000: first 256 shared map/menu 4-bpp characters, imported once rather than reread at runtime.</summary>
    public const int SourceAddress = MapTileAtlasRomData.CharacterData;
}
