using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>A composed sprite sheet: its image and its manifest.</summary>
/// <param name="Size">The sheet image's width and height.</param>
/// <param name="Pixels">The sheet image's pixels, row by row from the top; zero alpha is transparent.</param>
/// <param name="PaletteIndices">Each pixel's palette row times sixteen plus its colour, zero where transparent.</param>
/// <param name="Priorities">Each pixel's priority against the backgrounds 0..3.</param>
/// <param name="PartRows">Each pixel's part palette row 0..7, before the family's palette bits were ORed into it.</param>
/// <param name="Manifest">The frames' rectangles, origins, blending and rows, and the standard colours.</param>
internal sealed record ComposedSpriteSheet(Extent16<ImagePixels> Size, Rgba32[] Pixels, byte[] PaletteIndices, byte[] Priorities,
    byte[] PartRows, SpriteSheetDocument Manifest);
