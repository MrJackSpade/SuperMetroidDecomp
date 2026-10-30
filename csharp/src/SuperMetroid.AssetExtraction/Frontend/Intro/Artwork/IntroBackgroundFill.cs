using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>Paints a rectangle of cells with one tile.</summary>
/// <param name="Origin">The rectangle's top-left cell.</param>
/// <param name="Size">The rectangle's width and height in cells.</param>
/// <param name="Tile">The tile every cell shows.</param>
internal sealed record IntroBackgroundFill(Position16<TilemapCells> Origin, Extent16<TilemapCells> Size, BackgroundTile Tile) : IntroBackgroundStroke
{
    /// <inheritdoc/>
    internal override void Draw(IntroBackgroundCanvas canvas) => canvas.Fill(Origin, Size, Tile);
}
