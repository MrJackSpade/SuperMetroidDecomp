using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>One immutable eight-tile beam sheet compiled from indexed PNG artwork.</summary>
public sealed class BeamTileAtlas
{
    private readonly byte[] tiles;
    private BeamTileAtlas(byte[] tiles) => this.tiles = tiles;
    public ReadOnlyMemory<byte> Transfer => tiles;
    public void LoadTo(SnesVram vram) => vram.LoadBytes(BeamTileAtlasDefinitions.DestinationWord * 2, tiles);

    public static BeamTileAtlas Load(Stream png)
    {
        var image = IndexedPng.Read(png, BeamTileAtlasDefinitions.Width, BeamTileAtlasDefinitions.Height);
        return new(SnesPlanarTileEncoder.Encode(image.Pixels, image.Width, image.Height, 4));
    }
}

/// <summary>Native $90:AC8D beam-character upload geometry, separate from projectile mechanics.</summary>
public static class BeamTileAtlasDefinitions
{
    public const int Width = 64;
    public const int Height = 8;
    public const int ByteCount = 256;
    /// <summary>$90:AC8D writes eight 4-bpp tiles to VRAM word $6300.</summary>
    public const ushort DestinationWord = 0x6300;
    /// <summary>$90:C3B1 contains twelve legal beam-combination pointers before palette data.</summary>
    public const int SelectionCount = 12;
    public static string FileName(int selection)
    {
        if ((uint)selection >= SelectionCount) throw new ArgumentOutOfRangeException(nameof(selection));
        return $"beam-{selection:X2}-tiles.png";
    }
}
