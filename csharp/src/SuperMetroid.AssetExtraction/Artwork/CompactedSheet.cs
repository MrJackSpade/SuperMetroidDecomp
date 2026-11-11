using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>A sheet repacked so each distinct picture is held once (#1635).</summary>
/// <param name="Size">The packed sheet's width and height.</param>
/// <param name="Pixels">Its pixels, row by row.</param>
/// <param name="PaletteIndices">Its colour places, row by row.</param>
/// <param name="FrontTiles">Its front-tile marks, row by row.</param>
/// <param name="Manifest">Its frames, each at its picture's packed corner.</param>
internal sealed record CompactedSheet(Extent16<ImagePixels> Size, RgbaPixel[] Pixels, byte[] PaletteIndices, byte[] FrontTiles,
    SpriteSheetDocument Manifest)
{
    /// <summary>The packed sheet's planes.</summary>
    internal SpriteSheetPlanes Planes() => new(Size, Pixels, PaletteIndices, FrontTiles);
}
