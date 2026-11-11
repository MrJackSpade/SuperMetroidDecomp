namespace SuperMetroid.Core.Assets;

/// <summary>Builds sprite images a check paints itself, through the image's decoding constructor.</summary>
internal static class SpriteImageFixtures
{
    /// <summary>
    /// A named image holding <paramref name="pixels"/>, checked against its size and decoded before it is returned.
    /// </summary>
    /// <param name="name">The image's name, which names it in diagnostics.</param>
    /// <param name="size">The image's width and height in pixels.</param>
    /// <param name="pixels">Width times height pixels with the image's maps, row by row from the top.</param>
    /// <param name="colours">The image's shades and backdrop.</param>
    internal static SpriteImage Copied(string name, Extent16<ImagePixels> size, SpriteImagePixels pixels, PictureColours colours)
    {
        Ensure.NotNull(pixels);
        var image = new SpriteImage(name, size, colours, () => pixels);
        // The pixels are in hand, so the image is checked and decoded now: reading it later never changes its state.
        _ = image.Pixels;
        return image;
    }

    /// <summary>
    /// A named image holding copies of <paramref name="pixels"/>, with no recolour-region or blend map, checked against its
    /// size and decoded before it is returned.
    /// </summary>
    /// <param name="name">The image's name, which names it in diagnostics.</param>
    /// <param name="size">The image's width and height in pixels.</param>
    /// <param name="pixels">Width times height pixels, row by row from the top.</param>
    /// <param name="colours">The image's shades and backdrop.</param>
    internal static SpriteImage Copied(string name, Extent16<ImagePixels> size, ReadOnlySpan<RgbaPixel> pixels, PictureColours colours) =>
        Copied(name, size, new SpriteImagePixels(pixels.ToArray(), RecolourRegions: null, Blending: null), colours);
}
