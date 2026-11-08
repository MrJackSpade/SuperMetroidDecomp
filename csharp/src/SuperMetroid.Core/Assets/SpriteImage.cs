using System.Collections.Immutable;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// A finished, pre-coloured image the installation provides: a sheet of an actor's frames or one part's image, or a
/// sheet of a room tileset's blocks, coloured with its own standard colours. Beside the pixels it keeps each pixel's ink:
/// the place among its own colours of the colour the pixel shows, so a shade that recolours some of those colours
/// recolours the image (#1512, #1544), and whether the pixel is clear and whether it blends, as the image itself says.
/// </summary>
public sealed class SpriteImage
{
    /// <summary>
    /// The pixels and how each was inked, decoded when first read: an installation's stock images are verified when it
    /// opens, so decoding every sheet up front only cost the time and memory of images nothing draws (#1430).
    /// </summary>
    private readonly Lazy<Content> content;

    /// <summary>The colours the image was coloured with.</summary>
    private readonly RgbColor[] standardColours;

    /// <summary>
    /// Names an image whose pixels <paramref name="decode"/> produces when they are first read. The layers are checked
    /// then.
    /// </summary>
    /// <param name="name">The image's installed name, which names it in diagnostics.</param>
    /// <param name="size">The image's width and height in pixels; both are positive.</param>
    /// <param name="colours">The image's own colours, the shades it is pre-coloured in and its scene's backdrop, if it names one.</param>
    /// <param name="decode">Produces the image's layers.</param>
    /// <exception cref="ArgumentException">
    /// The name is empty, a dimension is zero, the image has more colours than a pixel can name, or the backdrop or a
    /// shade names a colour the image does not have.
    /// </exception>
    public SpriteImage(string name, Extent16<ImagePixels> size, PictureColours colours, Func<SpriteImageLayers> decode)
    {
        Ensure.NotNull(colours);
        ArgumentException.ThrowIfNullOrEmpty(name);
        Ensure.NotNull(decode);
        if (size.X == 0 || size.Y == 0)
            throw new ArgumentException($"Sprite image {name} has no pixels.", nameof(size));
        ImmutableArray<RgbColor> standard = colours.Standard.IsDefault ? [] : colours.Standard;
        if (standard.Length > SpriteImageInk.ColourMaximum + 1)
            throw new ArgumentException($"Sprite image {name} has {standard.Length} colours, more than its pixels can name.", nameof(colours));
        if (colours.Backdrop is { } backdrop && (backdrop < 0 || backdrop >= standard.Length))
            throw new ArgumentException($"Sprite image {name} names backdrop colour {backdrop} of its {standard.Length}.", nameof(colours));
        Shades = Ensure.NotNull(colours.Shades);
        foreach ((PictureShade shade, ShadeInks inks) in Shades)
        {
            if (inks.Inks.Any(ink => ink < 0 || ink >= standard.Length))
                throw new ArgumentException($"Sprite image {name}'s shade {shade.Key} recolours a colour past its {standard.Length}.", nameof(colours));
        }
        Name = name;
        Size = size;
        standardColours = [.. standard];
        Backdrop = colours.Backdrop;
        content = new(() =>
        {
            SpriteImageLayers layers = decode();
            return Check(layers.Pixels, layers.Inks);
        });
    }

    /// <summary>Checks one image's layers against its size and colours and keeps its pixels and inks.</summary>
    /// <exception cref="ArgumentException">A length does not match, or a pixel names a colour the image does not have.</exception>
    private Content Check(ReadOnlySpan<RgbaPixel> pixels, ReadOnlySpan<SpriteImageInk> inks)
    {
        Ensure.LengthEqual(pixels, Size.X * Size.Y);
        Ensure.LengthEqual(inks, Size.X * Size.Y);
        foreach (SpriteImageInk ink in inks)
        {
            if (ink.Colour is { } colour && colour >= standardColours.Length)
                throw new ArgumentException($"Sprite image {Name} has a pixel of colour {colour}, past its {standardColours.Length}.", nameof(inks));
        }
        return new Content(pixels.ToArray(), inks.ToArray());
    }

    /// <summary>An image's pixels and how each was inked.</summary>
    /// <param name="Pixels">The pixels, row by row from the top.</param>
    /// <param name="Inks">How each pixel was inked.</param>
    private sealed record Content(RgbaPixel[] Pixels, SpriteImageInk[] Inks);

    /// <summary>The image's installed name.</summary>
    public string Name { get; }

    /// <summary>The image's width and height in pixels.</summary>
    public Extent16<ImagePixels> Size { get; }

    /// <summary>The pixels, row by row from the top; a pixel with zero alpha is transparent.</summary>
    public ReadOnlySpan<RgbaPixel> Pixels => content.Value.Pixels;

    /// <summary>How each pixel was inked, row by row from the top: its colour, whether it is clear and blends, and whether it lies in a front tile.</summary>
    public ReadOnlySpan<SpriteImageInk> Inks => content.Value.Inks;

    /// <summary>The image's own colours, which its pixels and shades name by place.</summary>
    public ReadOnlySpan<RgbColor> StandardColours => standardColours;

    /// <summary>
    /// The place among <see cref="StandardColours"/> of the colour the image's scene shows where nothing is drawn, for a room
    /// tileset's picture; null for every other image.
    /// </summary>
    public int? Backdrop { get; }

    /// <summary>The shades the import pre-coloured the image in besides its standard colours (#1512).</summary>
    public ImmutableDictionary<PictureShade, ShadeInks> Shades { get; }

    /// <summary>What the image shows under <paramref name="shade"/>.</summary>
    /// <exception cref="InvalidDataException">The import did not pre-colour the image in that shade.</exception>
    public ShadeInks Shade(PictureShade shade) => Shades.TryGetValue(shade, out ShadeInks? inks) ? inks
        : throw new InvalidDataException($"Image {Name} was not pre-coloured in shade {shade.Key}.");

    /// <summary>Names the image with its size.</summary>
    public override string ToString() => $"{Name} ({Size.X}x{Size.Y})";
}
