using System.Text.Json.Serialization;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// One placed background tile (#1403, #1422): which of a layer's tiles it shows, under which of the tile's colour
/// variants, mirrored and prioritised as the screen, room or HUD lays it out. Rooms, the HUD, room effects, bosses drawn
/// as backgrounds and the frontend screens all lay their backgrounds out in these.
/// </summary>
/// <param name="Tile">The tile shown, by its place in the layer's tile set.</param>
/// <param name="ColourVariant">Which of the tile's pre-coloured variants the tile shows.</param>
/// <param name="FlipHorizontally">Whether the tile is mirrored left to right.</param>
/// <param name="FlipVertically">Whether the tile is mirrored top to bottom.</param>
/// <param name="HighPriority">Whether the tile is in the layer's upper priority tier.</param>
[IndependentFlag("a tile's two mirrorings and its priority tier are independent settings of the placement")]
public readonly record struct BackgroundTile(int Tile, int ColourVariant, bool FlipHorizontally, bool FlipVertically, bool HighPriority)
{
    /// <summary>The tiles a layer's tile set holds: a placement shows one of them.</summary>
    public const int TileCount = 1024;

    /// <summary>The pre-coloured variants of each tile a placement can select.</summary>
    public const int ColourVariants = 8;

    /// <summary>Places <paramref name="tile"/> under <paramref name="colourVariant"/>, mirrored by <paramref name="flip"/>.</summary>
    /// <param name="tile">The tile shown, 0 through one less than <see cref="TileCount"/>.</param>
    /// <param name="colourVariant">The colour variant, 0 through one less than <see cref="ColourVariants"/>.</param>
    /// <param name="highPriority">Whether the tile is in the layer's upper priority tier.</param>
    /// <param name="flip">The mirroring the tile is drawn with.</param>
    public BackgroundTile(int tile, int colourVariant, bool highPriority, TileFlip flip = TileFlip.None)
        : this(tile, colourVariant, (flip & TileFlip.Horizontal) != TileFlip.None, (flip & TileFlip.Vertical) != TileFlip.None, highPriority)
    {
    }

    /// <summary>The tile shown, 0 through one less than <see cref="TileCount"/>.</summary>
    public int Tile
    {
        get;
        init => field = Ensure.BetweenInclusive(0, TileCount - 1, value);
    } = Ensure.BetweenInclusive(0, TileCount - 1, Tile);

    /// <summary>The colour variant, 0 through one less than <see cref="ColourVariants"/>.</summary>
    public int ColourVariant
    {
        get;
        init => field = Ensure.BetweenInclusive(0, ColourVariants - 1, value);
    } = Ensure.BetweenInclusive(0, ColourVariants - 1, ColourVariant);

    /// <summary>The mirroring the tile is drawn with; documents store its two axes.</summary>
    [JsonIgnore]
    public TileFlip Flip => (FlipHorizontally ? TileFlip.Horizontal : TileFlip.None) | (FlipVertically ? TileFlip.Vertical : TileFlip.None);

    /// <summary>
    /// The same placement with the mirrorings <paramref name="flip"/> names reversed: a 16x16 block mirrored on an axis
    /// mirrors each of its four tiles on that axis as well as swapping their places.
    /// </summary>
    public BackgroundTile WithFlipsToggled(TileFlip flip) => this with
    {
        FlipHorizontally = FlipHorizontally ^ ((flip & TileFlip.Horizontal) != TileFlip.None),
        FlipVertically = FlipVertically ^ ((flip & TileFlip.Vertical) != TileFlip.None),
    };

    /// <summary>The same placement mirrored by <paramref name="flip"/> instead of its own mirroring.</summary>
    public BackgroundTile WithFlip(TileFlip flip) => this with
    {
        FlipHorizontally = (flip & TileFlip.Horizontal) != TileFlip.None,
        FlipVertically = (flip & TileFlip.Vertical) != TileFlip.None,
    };
}
