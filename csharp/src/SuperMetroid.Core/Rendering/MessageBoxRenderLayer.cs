using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hud;

namespace SuperMetroid.Core.Rendering;

/// <summary>The message box's text rows as a background layer over the HUD characters, with its centered reveal window.</summary>
public sealed record MessageBoxRenderLayer : RenderLayer
{
    /// <summary>Checks the message shape and reveal radius.</summary>
    /// <param name="text">The text rows, shown on the reveal window's scanlines.</param>
    /// <param name="radiusPixels">The centered reveal half-height in pixels, from zero through 24.</param>
    /// <exception cref="ArgumentException">The layer is not three through six full-width rows.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The radius is out of range.</exception>
    public MessageBoxRenderLayer(BgLayer text, int radiusPixels)
    {
        Ensure.NotNull(text);
        if (text.Size.X != GameplayMessageRomData.Layout.TilemapWidth || text.Size.Y < GameplayMessageRomData.Layout.MinimumRows
            || text.Size.Y > GameplayMessageRomData.Layout.MaximumRows)
            throw new ArgumentException("Message text must be three through six complete rows.", nameof(text));
        if (radiusPixels is < 0 or > GameplayMessageRomData.Timing.MaximumRadiusPixels)
            throw new ArgumentOutOfRangeException(nameof(radiusPixels));
        Text = text;
        RadiusPixels = radiusPixels;
    }

    /// <summary>The text rows as a background layer.</summary>
    public BgLayer Text { get; }
    /// <summary>The current centered reveal half-height in pixels, from zero through 24.</summary>
    public int RadiusPixels { get; }

    /// <summary>
    /// The text layer for a message: its tilemap words as cells over the HUD characters, centered on the window and
    /// shown only on the reveal window's scanlines.
    /// </summary>
    /// <param name="tilemap">The message's packed tilemap words, 32 per row.</param>
    /// <param name="radiusPixels">The reveal half-height in pixels.</param>
    /// <param name="hudCharacters">The installed BG3 characters the words name.</param>
    public static MessageBoxRenderLayer FromTilemap(ReadOnlySpan<ushort> tilemap, int radiusPixels, BgArtwork hudCharacters)
    {
        Ensure.NotNull(hudCharacters);
        int width = GameplayMessageRomData.Layout.TilemapWidth;
        if (tilemap.Length % width != 0)
            throw new ArgumentException("Message tilemap must contain complete rows.", nameof(tilemap));
        var cells = new BgCell[tilemap.Length];
        for (int index = 0; index < cells.Length; index++)
            cells[index] = BgCell.FromTilemapWord(new SnesBgTilemapWord(tilemap[index]));
        int rows = tilemap.Length / width;
        int top = GameplayMessageRomData.Layout.WindowCenterY - rows * GameplayMessageRomData.Layout.TilePixels / 2;
        return new MessageBoxRenderLayer(new BgLayer(new BgLayerSource(BgLayerSourceKind.MessageBox), hudCharacters, width,
            System.Runtime.InteropServices.ImmutableCollectionsMarshal.AsImmutableArray(cells),
            new Position16<ScreenPixels>(0, unchecked((ushort)top)),
            GameplayMessageRomData.Layout.WindowCenterY - radiusPixels,
            GameplayMessageRomData.Layout.WindowCenterY + radiusPixels), radiusPixels);
    }
}
