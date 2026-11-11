using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>A composed sprite sheet: its image and its manifest.</summary>
/// <param name="Size">The sheet image's width and height.</param>
/// <param name="Pixels">The sheet image's pixels, row by row from the top; zero alpha is transparent.</param>
/// <param name="PaletteIndices">Each pixel's colour group's place times sixteen plus its colour, zero where transparent.</param>
/// <param name="Manifest">The frames' rectangles, origins, colour groups and depths, and the standard colours.</param>
internal sealed record ComposedSpriteSheet(Extent16<ImagePixels> Size, RgbaPixel[] Pixels, byte[] PaletteIndices,
    SpriteSheetDocument Manifest);
