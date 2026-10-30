using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>Copies a rectangle of the intro atlas onto the page, reflected as a whole when <paramref name="Flips"/> names an axis.</summary>
/// <param name="Origin">The page cell the patch's top-left lands on.</param>
/// <param name="Size">The patch's width and height in cells.</param>
/// <param name="First">The atlas patch's top-left tile, with its colour variant, priority and own mirroring.</param>
/// <param name="Flips">The axes the whole patch is reflected on.</param>
internal sealed record IntroBackgroundPatch(Position16<TilemapCells> Origin, Extent16<TilemapCells> Size, BackgroundTile First,
    TileFlip Flips = TileFlip.None) : IntroBackgroundStroke
{
    /// <inheritdoc/>
    internal override void Draw(IntroBackgroundCanvas canvas) => canvas.Patch(Origin, Size, First, Flips);
}
