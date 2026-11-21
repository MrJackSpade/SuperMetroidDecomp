using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// A sheet's pixel planes as they are painted and encoded, each its width times its height long, row by row from the top
/// (#1490): held by a <see cref="ComposedSpriteSheet"/> or lent by planes one thread reuses from sheet to sheet.
/// </summary>
/// <param name="size">The sheet's width and height.</param>
/// <param name="pixels">Each pixel's colour; zero alpha is transparent.</param>
/// <param name="inks">Each pixel's ink, the colour group and colour it was painted in; <see cref="ComposedInk.None"/> where transparent.</param>
internal readonly ref struct SpriteSheetPlanes(Extent16<ImagePixels> size, Span<RgbaPixel> pixels, Span<ComposedInk> inks)
{
    /// <summary>The sheet's width and height.</summary>
    internal Extent16<ImagePixels> Size { get; } = size;

    /// <summary>Each pixel's colour; zero alpha is transparent.</summary>
    internal Span<RgbaPixel> Pixels { get; } = pixels;

    /// <summary>Each pixel's ink, the colour group and colour it was painted in; <see cref="ComposedInk.None"/> where transparent.</summary>
    internal Span<ComposedInk> Inks { get; } = inks;
}
