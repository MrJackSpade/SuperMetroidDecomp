using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// A sheet's pixel planes as they are painted and encoded, each its width times its height long, row by row from the top
/// (#1490): held by a <see cref="ComposedSpriteSheet"/> or lent by planes one thread reuses from sheet to sheet.
/// </summary>
/// <param name="size">The sheet's width and height.</param>
/// <param name="pixels">Each pixel's colour; zero alpha is transparent.</param>
/// <param name="paletteIndices">Each pixel's colour group's place times sixteen plus its colour, zero where transparent.</param>
/// <param name="priorities">Each pixel's priority against the backgrounds.</param>
/// <param name="partGroups">Each pixel's <see cref="PartColourGroup.Code"/>.</param>
internal readonly ref struct SpriteSheetPlanes(Extent16<ImagePixels> size, Span<RgbaPixel> pixels, Span<byte> paletteIndices,
    Span<byte> priorities, Span<byte> partGroups)
{
    /// <summary>The sheet's width and height.</summary>
    internal Extent16<ImagePixels> Size { get; } = size;

    /// <summary>Each pixel's colour; zero alpha is transparent.</summary>
    internal Span<RgbaPixel> Pixels { get; } = pixels;

    /// <summary>Each pixel's colour group's place times sixteen plus its colour, zero where transparent.</summary>
    internal Span<byte> PaletteIndices { get; } = paletteIndices;

    /// <summary>Each pixel's priority against the backgrounds.</summary>
    internal Span<byte> Priorities { get; } = priorities;

    /// <summary>Each pixel's <see cref="PartColourGroup.Code"/>.</summary>
    internal Span<byte> PartGroups { get; } = partGroups;
}
