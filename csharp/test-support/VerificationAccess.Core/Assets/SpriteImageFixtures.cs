namespace SuperMetroid.Core.Assets;

/// <summary>Builds sprite images a check paints itself, through the image's decoding constructor.</summary>
internal static class SpriteImageFixtures
{
    /// <summary>
    /// A named image holding copies of <paramref name="pixels"/> and <paramref name="inks"/>, checked against its size and
    /// colours and decoded before it is returned.
    /// </summary>
    /// <param name="name">The image's name, which names it in diagnostics.</param>
    /// <param name="size">The image's width and height in pixels.</param>
    /// <param name="pixels">Width times height pixels, row by row from the top.</param>
    /// <param name="inks">Each pixel's ink, naming its colour by its place among <paramref name="colours"/>.</param>
    /// <param name="colours">The image's own colours, shades and backdrop.</param>
    internal static SpriteImage Copied(string name, Extent16<ImagePixels> size, ReadOnlySpan<RgbaPixel> pixels,
        ReadOnlySpan<SpriteImageInk> inks, PictureColours colours)
    {
        var layers = new SpriteImageLayers(pixels.ToArray(), inks.ToArray());
        var image = new SpriteImage(name, size, colours, () => layers);
        // The layers are in hand, so the image is checked and decoded now: reading it later never changes its state.
        _ = image.Pixels;
        return image;
    }
}
