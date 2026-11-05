namespace SuperMetroid.Core.Assets;

/// <summary>Builds sprite images a check paints itself, through the image's decoding constructor.</summary>
internal static class SpriteImageFixtures
{
    /// <summary>
    /// A named image holding copies of <paramref name="pixels"/>, <paramref name="paletteIndices"/> and
    /// <paramref name="priorities"/>, checked against its size when its pixels are first read.
    /// </summary>
    /// <param name="name">The image's name, which names it in diagnostics.</param>
    /// <param name="size">The image's width and height in pixels.</param>
    /// <param name="pixels">Width times height pixels, row by row from the top.</param>
    /// <param name="paletteIndices">Each pixel's colour group's place times sixteen plus its colour.</param>
    /// <param name="palette">Which half of the colour table the indices name, and its colours.</param>
    /// <param name="priorities">Each pixel's priority.</param>
    internal static SpriteImage Copied(string name, Extent16<ImagePixels> size, ReadOnlySpan<RgbaPixel> pixels,
        ReadOnlySpan<byte> paletteIndices, SpriteImagePalette palette, ReadOnlySpan<byte> priorities)
    {
        var layers = new SpriteImageLayers(pixels.ToArray(), paletteIndices.ToArray(), priorities.ToArray());
        return new SpriteImage(name, size, palette, () => layers);
    }
}
