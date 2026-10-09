using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Immutable compiled map characters from an indexed PNG, independent of any cartridge bus.</summary>
public sealed class MapTileAtlas
{
    /// <summary>SNES 4-bpp planar character data in row-major tile order, retained as the atlas's immutable transfer source.</summary>
    private readonly byte[] planar;

    /// <summary>Wraps the planar characters produced by the indexed-PNG compiler.</summary>
    /// <param name="planar">Encoded bytes for all 256 characters, with 32 bytes per 8-by-8 tile.</param>
    private MapTileAtlas(byte[] planar) => this.planar = planar;

    /// <summary>Compiles a 256-by-64 noninterlaced indexed PNG into 256 row-major 8-by-8 SNES 4-bpp characters, requiring pixel indices 0..15; PNG palette RGB values do not determine the runtime display palette.</summary>
    /// <param name="png">Caller-owned stream containing shared map artwork or the same-sized additional pause-menu atlas.</param>
    /// <returns>The immutable 8192-byte planar character atlas.</returns>
    public static MapTileAtlas Load(Stream png)
    {
        var image = IndexedPng.Read(png, MapTileAtlasFormat.Width, MapTileAtlasFormat.Height);
        return new(SnesPlanarTileEncoder.Encode(image.Pixels, image.Width, image.Height, 4));
    }

    /// <summary>Copies compiled characters to the screen's own VRAM; source storage cannot be mutated by callers.</summary>
    /// <param name="vram">Destination VRAM buffer.</param>
    /// <param name="destinationByteAddress">Byte address of the first character, not a PPU word address; the full 8192-byte atlas is copied.</param>
    public void LoadTo(SnesVram vram, int destinationByteAddress) => vram.LoadBytes(destinationByteAddress, planar);
}

/// <summary>Shared pause/file-select character atlas geometry and fixed installed filename.</summary>
public static class MapTileAtlasFormat
{
    /// <summary>Asset filename for the shared pause/file-select map character artwork.</summary>
    public const string FileName = "map-tiles.png";
    /// <summary>Thirty-two 8-pixel character columns; character indices advance left to right before proceeding to the next row.</summary>
    public const int TileColumns = 32;
    /// <summary>Required PNG width of 256 pixels, covering the thirty-two character columns.</summary>
    public const int Width = TileColumns * 8;
    /// <summary>Required PNG height of 64 pixels, covering eight character rows.</summary>
    public const int Height = 64;
    /// <summary>Sixteen permitted pixel indices for the 4-bpp characters; runtime CGRAM palettes are installed separately.</summary>
    public const int ColorCount = 16;
    /// <summary>8192 compiled bytes: 256 characters with 32 planar bytes per 8-by-8 4-bpp character.</summary>
    public const int ByteCount = Width * Height / 2;
    /// <summary>$B6:8000: first 256 shared map/menu 4-bpp characters, imported once rather than reread at runtime.</summary>
    public const int SourceAddress = MapTileAtlasRomData.CharacterData;
}

/// <summary>Additional pause-menu characters, separate from the shared map atlas and its overrides.</summary>
public static class PauseTileAtlasFormat
{
    /// <summary>Asset filename for pause-menu characters 256..511, loaded with the same PNG geometry as the shared map atlas.</summary>
    public const string FileName = "pause-ui-tiles.png";
    /// <summary>$B6:A000, characters 256..511 in GameState_13's background-character transfer.</summary>
    public const int SourceAddress = MapTileAtlasRomData.PauseInterfaceCharacterData;
    /// <summary>Byte $2000 immediately follows the shared map characters in pause VRAM.</summary>
    public const int DestinationByte = MapTileAtlasFormat.ByteCount;
    /// <summary>8192 compiled bytes for the second 256-character half of the pause-menu background artwork.</summary>
    public const int ByteCount = MapTileAtlasFormat.ByteCount;
}
