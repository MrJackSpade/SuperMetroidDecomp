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
