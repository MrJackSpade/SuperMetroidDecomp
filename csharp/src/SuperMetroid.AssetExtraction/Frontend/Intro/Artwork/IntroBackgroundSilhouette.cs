using SuperMetroid.Core;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>Copies only the occupied cells of an atlas block, row by row, leaving the rest of the page as drawn.</summary>
/// <param name="Origin">The page cell the block's top-left lands on.</param>
/// <param name="First">The block's top-left atlas tile, with its colour variant.</param>
/// <param name="Rows">Each block row's occupied column runs, left to right.</param>
internal sealed record IntroBackgroundSilhouette(Position16<TilemapCells> Origin, BackgroundTile First, InclusiveRange[][] Rows)
    : IntroBackgroundStroke
{
    /// <inheritdoc/>
    internal override void Draw(IntroBackgroundCanvas canvas)
    {
        for (int row = 0; row < Rows.Length; row++)
        foreach (InclusiveRange run in Rows[row])
        for (int column = run.First; column <= run.Last; column++)
            canvas.Put(Origin + new Offset16<TilemapCells>(checked((short)column), checked((short)row)),
                First with { Tile = First.Tile + row * IntroBackgroundPageDefinitions.AtlasColumns + column });
    }
}
