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
        return new(SnesPlanarTileEncoder.Encode(image.Pixels, image.Width, image.Height, 4));
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

/// <summary>Additional pause-menu characters, separate from the shared map atlas and its overrides.</summary>
public static class PauseTileAtlasFormat
{
    public const string FileName = "pause-ui-tiles.png";
    /// <summary>$B6:A000, characters 256..511 in GameState_13's background-character transfer.</summary>
    public const int SourceAddress = MapTileAtlasRomData.PauseInterfaceCharacterData;
    /// <summary>Byte $2000 immediately follows the shared map characters in pause VRAM.</summary>
    public const int DestinationByte = MapTileAtlasFormat.ByteCount;
    public const int ByteCount = MapTileAtlasFormat.ByteCount;
}
