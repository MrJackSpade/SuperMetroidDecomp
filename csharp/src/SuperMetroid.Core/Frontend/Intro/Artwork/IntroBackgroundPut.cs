using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Places one tile on one cell.</summary>
/// <param name="Cell">The page cell.</param>
/// <param name="Tile">The tile it shows.</param>
internal sealed record IntroBackgroundPut(Position16<TilemapCells> Cell, BackgroundTile Tile) : IntroBackgroundStroke
{
    /// <inheritdoc/>
    internal override void Draw(IntroBackgroundCanvas canvas) => canvas.Put(Cell, Tile);
}
