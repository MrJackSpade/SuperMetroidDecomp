using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rendering;

/// <summary>Owned bank-$85 tile words and the centered reveal window, independent of input/timing state.</summary>
public sealed record MessageBoxRenderLayer : RenderLayer
{
    private readonly ushort[] tilemap;
    public ReadOnlySpan<ushort> Tilemap => tilemap;
    public int RowCount => tilemap.Length / GameplayMessageRomData.Layout.TilemapWidth;
    public int RadiusPixels { get; }

    public MessageBoxRenderLayer(ReadOnlySpan<ushort> tilemap, int radiusPixels)
    {
        int width = GameplayMessageRomData.Layout.TilemapWidth;
        if (tilemap.Length % width != 0 || tilemap.Length / width < GameplayMessageRomData.Layout.MinimumRows
            || tilemap.Length / width > GameplayMessageRomData.Layout.MaximumRows)
            throw new ArgumentException("Message tilemap must contain three through six complete rows.", nameof(tilemap));
        if (radiusPixels < 0 || radiusPixels > GameplayMessageRomData.Timing.MaximumRadiusPixels)
            throw new ArgumentOutOfRangeException(nameof(radiusPixels));
        this.tilemap = tilemap.ToArray();
        RadiusPixels = radiusPixels;
    }
}
