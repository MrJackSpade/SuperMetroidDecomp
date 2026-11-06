namespace SuperMetroid.Core.Assets;

/// <summary>
/// How one pixel of a <see cref="SpriteImage"/> was inked beside its colour (#1403, #1514): the palette entry it shows,
/// and, for a picture of a layer's tiles, whether the tile it lies in is one of the layer's front tiles.
/// </summary>
/// <remarks>
/// Installed images hold billions of pixels, so the two values share one word per pixel instead of a word each.
/// </remarks>
public readonly record struct SpriteImageInk
{
    /// <summary>The largest palette index.</summary>
    private const int PaletteIndexMaximum = byte.MaxValue;

    /// <summary>The word's flag marking a front tile's pixel, above the palette index.</summary>
    private const int FrontTileFlag = PaletteIndexMaximum + 1;

    /// <summary>The palette index, with the front-tile flag above it.</summary>
    private readonly int fields;

    /// <summary>Records one pixel's ink.</summary>
    /// <param name="paletteIndex">The pixel's colour group's place times sixteen plus its colour; zero where it is transparent.</param>
    /// <param name="frontTile">Whether the pixel lies in one of its layer's front tiles; false for every pixel of an actor's frame.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="paletteIndex"/> is negative or does not fit a byte.</exception>
    public SpriteImageInk(int paletteIndex, bool frontTile)
    {
        Ensure.AtLeastZero(paletteIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(paletteIndex, PaletteIndexMaximum);
        fields = paletteIndex | (frontTile ? FrontTileFlag : 0);
    }

    /// <summary>The pixel's colour group's place times sixteen plus its colour; zero where it is transparent.</summary>
    public int PaletteIndex => fields & PaletteIndexMaximum;

    /// <summary>
    /// Whether the pixel lies in one of its layer's front tiles (#1514). Which of a room's tiles are drawn in front of the
    /// actors passing behind them is level design, decided tile by tile; the gameplay scene draws the room's back tiles and
    /// its front tiles as layers of their own.
    /// </summary>
    public bool FrontTile => (fields & FrontTileFlag) != 0;
}
