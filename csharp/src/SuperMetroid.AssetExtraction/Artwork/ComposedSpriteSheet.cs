using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>A composed sprite sheet: its image and its manifest.</summary>
/// <param name="Size">The sheet image's width and height.</param>
/// <param name="Pixels">The sheet image's pixels, row by row from the top; zero alpha is transparent.</param>
/// <param name="Manifest">The frames' rectangles, origins and blending.</param>
internal sealed record ComposedSpriteSheet(Extent16<ImagePixels> Size, Rgba32[] Pixels, SpriteSheetDocument Manifest);
