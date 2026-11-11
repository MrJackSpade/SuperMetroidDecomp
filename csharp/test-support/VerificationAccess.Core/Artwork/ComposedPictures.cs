using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Installs pictures a check composes against the cartridge's colours as the import installs a composed sheet (#1544): the
/// image gets its own colours and inks through <see cref="InstalledSheetColouring"/>, and where each colour came from is
/// recorded as the loader records it.
/// </summary>
internal static class ComposedPictures
{
    /// <summary>The picture a sheet composed as <paramref name="composed"/> installs, with its source recorded.</summary>
    /// <param name="name">The picture's name.</param>
    /// <param name="size">The picture's width and height.</param>
    /// <param name="pixels">The composed pixels.</param>
    /// <param name="paletteIndices">Each pixel's composed palette index.</param>
    /// <param name="composed">The composed manifest, whose colours, shades and backdrop the picture installs.</param>
    /// <param name="installed">The installed manifest, whose frames a sheet cuts.</param>
    internal static SpriteImage Install(string name, Extent16<ImagePixels> size, ReadOnlySpan<RgbaPixel> pixels,
        ReadOnlySpan<byte> paletteIndices, SpriteSheetDocument composed, out InstalledSheetDocument installed)
    {
        InstalledSheetLayers layers = InstalledSheetColouring.Install(composed, paletteIndices);
        installed = layers.Manifest;
        SpriteImage image = SpriteImageFixtures.Copied(name, size, pixels, layers.Inks, SpriteSheetFile.ColoursOf(layers.Manifest));
        InstalledPictureColours.Record(image, layers.Manifest.Source);
        return image;
    }

    /// <summary>
    /// The picture of <paramref name="pixels"/>, composed as <paramref name="paletteIndices"/> against
    /// <paramref name="table"/>, the colours of <paramref name="palette"/>'s half of the colour table, as the import installs
    /// it.
    /// </summary>
    internal static SpriteImage Install(string name, Extent16<ImagePixels> size, ReadOnlySpan<RgbaPixel> pixels,
        ReadOnlySpan<byte> paletteIndices, SpriteImageColours palette, ReadOnlySpan<RgbColor> table) =>
        Install(name, size, pixels, paletteIndices, Composed(palette, table), out _);

    /// <summary>A frameless composed manifest of <paramref name="table"/>, the colours of <paramref name="palette"/>'s half.</summary>
    internal static SpriteSheetDocument Composed(SpriteImageColours palette, ReadOnlySpan<RgbColor> table)
    {
        Ensure.LengthEqual(table, ImageColourGroup.ColourCountOf(palette));
        return new SpriteSheetDocument
        {
            Palette = palette,
            Colours = [.. table.ToArray().Select(colour => colour.ToWord())],
            Frames = [],
        };
    }
}
