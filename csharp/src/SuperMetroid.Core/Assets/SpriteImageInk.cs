namespace SuperMetroid.Core.Assets;

/// <summary>
/// What one pixel of a <see cref="SpriteImage"/> is beside its pre-coloured colour (#1403, #1514, #1544): which of the
/// image's own colours it shows, whether it is clear, whether it takes part in colour blends, and, for a picture of a
/// layer's tiles, whether the tile it lies in is one of the layer's front tiles.
/// </summary>
/// <remarks>
/// <para>
/// A clear pixel shows what lies behind it. Most clear pixels have no colour: they are transparent wherever they are
/// drawn. A clear pixel of a HUD or room effect picture keeps a colour of its own, which it shows where its layer is drawn
/// opaque, as gameplay's HUD band is. A pixel that is not clear always has a colour.
/// </para>
/// <para>Installed images hold billions of pixels, so the values share one word per pixel instead of a word each.</para>
/// </remarks>
public readonly record struct SpriteImageInk
{
    /// <summary>The largest place of a colour in an image's colours.</summary>
    internal const int ColourMaximum = byte.MaxValue;

    /// <summary>The word's flag marking a pixel that has a colour, above the colour's place.</summary>
    private const int ColouredFlag = ColourMaximum + 1;

    /// <summary>The word's flag marking a clear pixel.</summary>
    private const int ClearFlag = ColouredFlag << 1;

    /// <summary>The word's flag marking a front tile's pixel.</summary>
    private const int FrontTileFlag = ClearFlag << 1;

    /// <summary>The word's flag marking a pixel that takes part in colour blends.</summary>
    private const int BlendsFlag = FrontTileFlag << 1;

    /// <summary>The colour's place, with the flags above it.</summary>
    private readonly int fields;

    /// <summary>Records one pixel.</summary>
    /// <param name="colour">The place among the image's colours of the colour the pixel shows; null for a pixel with none.</param>
    /// <param name="clear">Whether the pixel shows what lies behind it; a pixel with no colour is clear.</param>
    /// <param name="frontTile">Whether the pixel lies in one of its layer's front tiles; false for every pixel of an actor's frame.</param>
    /// <param name="blends">Whether the pixel takes part in colour blends where it is drawn as an actor's.</param>
    /// <exception cref="ArgumentOutOfRangeException">The colour's place is negative or past the largest an image holds.</exception>
    /// <exception cref="ArgumentException">A pixel with no colour is not clear.</exception>
    public SpriteImageInk(int? colour, bool clear, bool frontTile, bool blends)
    {
        if (colour is not { } place)
        {
            if (!clear)
                throw new ArgumentException("A pixel with no colour of its image's is clear.", nameof(clear));
            place = 0;
        }
        else
        {
            Ensure.AtLeastZero(place);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(place, ColourMaximum, nameof(colour));
        }
        fields = place | (colour is null ? 0 : ColouredFlag) | (clear ? ClearFlag : 0) | (frontTile ? FrontTileFlag : 0)
            | (blends ? BlendsFlag : 0);
    }

    /// <summary>A transparent pixel: clear, with no colour.</summary>
    /// <param name="frontTile">Whether the pixel lies in one of its layer's front tiles.</param>
    public static SpriteImageInk Transparent(bool frontTile) => new(null, clear: true, frontTile, blends: false);

    /// <summary>The place among the image's colours of the colour the pixel shows; null for a transparent pixel.</summary>
    public int? Colour => (fields & ColouredFlag) != 0 ? fields & ColourMaximum : null;

    /// <summary>Whether the pixel shows what lies behind it, wherever its layer does not draw it opaque.</summary>
    public bool Clear => (fields & ClearFlag) != 0;

    /// <summary>Whether the pixel is transparent: clear, with no colour of its own.</summary>
    public bool IsTransparent => (fields & ColouredFlag) == 0;

    /// <summary>
    /// Whether the pixel lies in one of its layer's front tiles (#1514). Which of a room's tiles are drawn in front of the
    /// actors passing behind them is level design, decided tile by tile; the gameplay scene draws the room's back tiles and
    /// its front tiles as layers of their own.
    /// </summary>
    public bool FrontTile => (fields & FrontTileFlag) != 0;

    /// <summary>
    /// Whether the pixel takes part in colour blends where it is drawn as an actor's (#1514): a glow or tint that blends the
    /// actors shows over it, and a translucent actor's pixel blends with what lies behind it.
    /// </summary>
    public bool Blends => (fields & BlendsFlag) != 0;
}
