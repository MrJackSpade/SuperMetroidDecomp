using SuperMetroid.Core;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>$96:FF14 cave: one of the two selected two-cell stalactite drawings. Their lower rows are repacked rather than
/// following the atlas-row stride.</summary>
/// <param name="Origin">The stalactite's top-left cell.</param>
/// <param name="Width">Which of the two drawings it is.</param>
internal sealed record IntroCeilingPoint(Position16<TilemapCells> Origin, IntroCeilingPointWidth Width) : IntroBackgroundStroke
{
    /// <inheritdoc/>
    internal override void Draw(IntroBackgroundCanvas canvas)
    {
        IntroCeilingPointTiles tiles = Ensure.Listed(IntroBackgroundPageDefinitions.CeilingPoints, Width);
        canvas.Patch(Origin, new(2, 1), tiles.Top);
        canvas.Put(Origin + new Offset16<TilemapCells>(0, 1), tiles.LowerLeft);
        canvas.Put(Origin + new Offset16<TilemapCells>(1, 1), tiles.LowerRight);
    }
}
