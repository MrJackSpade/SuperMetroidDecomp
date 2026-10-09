using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rendering;

/// <summary>Owned bank-$85 tile words and the centered reveal window, independent of input/timing state.</summary>
public sealed record MessageBoxRenderLayer : RenderLayer
{
    /// <summary>Owns the packed BG tile words displayed by this layer.</summary>
    private readonly ushort[] tilemap;
    /// <summary>Gets the owned packed BG tile words in 32-column row order.</summary>
    public ReadOnlySpan<ushort> Tilemap => tilemap;
    /// <summary>Gets the validated message height, from three through six eight-pixel rows.</summary>
    public int RowCount => tilemap.Length / GameplayMessageRomData.Layout.TilemapWidth;
    /// <summary>Gets the current centered reveal half-width in pixels, from zero through 24.</summary>
    public int RadiusPixels { get; }

    /// <summary>Copies a complete message tilemap and captures its current reveal radius for rendering.</summary>
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
