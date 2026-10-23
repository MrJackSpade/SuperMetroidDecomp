namespace SuperMetroid.Core.Rendering;

/// <summary>
/// One cell of a frontend tile layer (#1403): which of the layer's tiles it shows, under which of the tile's palette rows,
/// mirrored and prioritised as the screen laid it out.
/// </summary>
/// <param name="Tile">The tile shown, by its place in the layer's tile set.</param>
/// <param name="ColourVariant">Which of the tile's pre-coloured variants the cell shows.</param>
/// <param name="FlipHorizontally">Whether the tile is mirrored left to right.</param>
/// <param name="FlipVertically">Whether the tile is mirrored top to bottom.</param>
/// <param name="HighPriority">Whether the cell is in the layer's upper priority tier.</param>
[IndependentFlag("a cell's two mirrorings and its priority tier are independent settings of the cell")]
public readonly record struct ScreenTileCell(int Tile, int ColourVariant, bool FlipHorizontally, bool FlipVertically, bool HighPriority)
{
    /// <summary>The tile shown, 0 through one less than <see cref="ScreenLayerPictures.CharacterIndices"/>.</summary>
    public int Tile
    {
        get;
        init => field = Ensure.BetweenInclusive(0, ScreenLayerPictures.CharacterIndices - 1, value);
    } = Ensure.BetweenInclusive(0, ScreenLayerPictures.CharacterIndices - 1, Tile);

    /// <summary>The colour variant, 0 through one less than <see cref="ScreenTileSheet.ColourVariants"/>.</summary>
    public int ColourVariant
    {
        get;
        init => field = Ensure.BetweenInclusive(0, ScreenTileSheet.ColourVariants - 1, value);
    } = Ensure.BetweenInclusive(0, ScreenTileSheet.ColourVariants - 1, ColourVariant);
}
