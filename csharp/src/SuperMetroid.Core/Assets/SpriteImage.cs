using System.Collections.Immutable;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// A finished, pre-coloured image the installation provides: a sheet of an actor's frames or one part's image, or a
/// sheet of a room tileset's blocks, coloured with its standard palette. Beside the pixels it keeps each pixel's colour
/// group and colour, and the standard colours of those groups, so an effect that changes the colours a group shows
/// recolours the image as it did the characters.
/// </summary>
public sealed class SpriteImage
{
    /// <summary>The standard colours of an image's colour groups: sixteen for each group.</summary>
    public const int SpriteColourCount = ImageColourGroup.Count * ImageColourGroup.Colours;

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
    /// <param name="palette">Which half of the colour table the palette indices name, and the colours of it the pixels were coloured with.</param>
    /// <param name="decode">Produces the image's layers.</param>
    /// <exception cref="ArgumentException">The name is empty or a dimension is zero.</exception>
    public SpriteImage(string name, Extent16<ImagePixels> size, SpriteImagePalette palette, Func<SpriteImageLayers> decode)
        : this(name, size, palette, Deferred(name, size, decode))
    {
    }

    /// <summary>The content <paramref name="decode"/> produces, checked when it is first read.</summary>
    private static Lazy<Content> Deferred(string name, Extent16<ImagePixels> size, Func<SpriteImageLayers> decode)
    {
        Ensure.NotNull(decode);
        return new(() =>
        {
            SpriteImageLayers layers = decode();
            return Check(name, size, layers.Pixels, layers.PaletteIndices, layers.Priorities);
        });
    }

    /// <summary>Checks and keeps what every image names before its pixels are read.</summary>
    private SpriteImage(string name, Extent16<ImagePixels> size, SpriteImagePalette palette, Lazy<Content> content)
    {
        Ensure.NotNull(palette);
        ArgumentException.ThrowIfNullOrEmpty(name);
        if (!Enum.IsDefined(palette.Half))
            throw new ArgumentException($"Sprite image {name} names no half of the colour table.", nameof(palette));
        if (size.X == 0 || size.Y == 0)
            throw new ArgumentException($"Sprite image {name} has no pixels.", nameof(size));
        ReadOnlySpan<RgbColor> colours = palette.Standard.AsSpan();
        Ensure.LengthEqual(colours, ColourCountOf(palette.Half));
        Name = name;
        Size = size;
        Colours = palette.Half;
        standardColours = colours.ToArray();
        Shades = Ensure.NotNull(palette.Shades);
        this.content = content;
    }

    /// <summary>Checks one image's layers against its size and keeps its pixels and inks.</summary>
    /// <exception cref="ArgumentException">A length does not match or a priority exceeds 3.</exception>
    private static Content Check(string name, Extent16<ImagePixels> size, ReadOnlySpan<RgbaPixel> pixels,
        ReadOnlySpan<byte> paletteIndices, ReadOnlySpan<byte> priorities)
    {
        Ensure.LengthEqual(pixels, size.X * size.Y);
        Ensure.LengthEqual(paletteIndices, size.X * size.Y);
        Ensure.LengthEqual(priorities, size.X * size.Y);
        foreach (int priority in priorities)
        {
            if (priority > 3)
                throw new ArgumentException($"Sprite image {name} has a pixel of priority {priority}; priorities are 0..3.", nameof(priorities));
        }
        var inks = new SpriteImageInk[pixels.Length];
        for (int pixel = 0; pixel < inks.Length; pixel++)
            inks[pixel] = new(paletteIndices[pixel], priorities[pixel]);
        return new Content(pixels.ToArray(), inks);
    }

    /// <summary>An image's pixels and how each was inked.</summary>
    /// <param name="Pixels">The pixels, row by row from the top.</param>
    /// <param name="Inks">How each pixel was inked: its palette entry and priority.</param>
    private sealed record Content(RgbaPixel[] Pixels, SpriteImageInk[] Inks);

    /// <summary>The standard colours an image coloured as <paramref name="colours"/> keeps: all 256 for a full-palette image.</summary>
    public static int ColourCountOf(SpriteImageColours colours) =>
        colours == SpriteImageColours.FullPalette ? 2 * SpriteColourCount : SpriteColourCount;

    /// <summary>The image's installed name.</summary>
    public string Name { get; }

    /// <summary>The image's width and height in pixels.</summary>
    public Extent16<ImagePixels> Size { get; }

    /// <summary>Which half of the colour table the palette indices name.</summary>
    public SpriteImageColours Colours { get; }

    /// <summary>
    /// Whether a pixel inked <paramref name="ink"/> shows colour zero, which a layer either leaves transparent or, where
    /// <see cref="ColourZeroKeepsGroup"/>, shows in its group's colour zero (#1408).
    /// </summary>
    public bool IsColourZero(SpriteImageInk ink) =>
        Colours == SpriteImageColours.TwoBppBackground ? ink.PaletteIndex % TwoBppGroupColours == 0 : ink.PaletteIndex == 0;

    /// <summary>
    /// Whether the image's colour-zero pixels keep the ink of their group's colour zero, so a layer that shows colour zero
    /// shows that ink's colour (#1408); otherwise a colour-zero pixel's ink is zero.
    /// </summary>
    public bool ColourZeroKeepsGroup => Colours == SpriteImageColours.TwoBppBackground;

    /// <summary>The colours of one of the four-colour groups a HUD or room effect image is inked in.</summary>
    private const int TwoBppGroupColours = 4;

    /// <summary>The pixels, row by row from the top; a pixel with zero alpha is transparent.</summary>
    public ReadOnlySpan<RgbaPixel> Pixels => content.Value.Pixels;

    /// <summary>How each pixel was inked, row by row from the top: its palette entry and priority.</summary>
    public ReadOnlySpan<SpriteImageInk> Inks => content.Value.Inks;

    /// <summary>The 128 colours of the image's half of the colour table the pixels were coloured with.</summary>
    public ReadOnlySpan<RgbColor> StandardColours => standardColours;

    /// <summary>The shades the import pre-coloured the image in besides its standard colours (#1512).</summary>
    public ImmutableDictionary<PictureShade, ShadeInks> Shades { get; }

    /// <summary>What the image shows under <paramref name="shade"/>.</summary>
    /// <exception cref="InvalidDataException">The import did not pre-colour the image in that shade.</exception>
    public ShadeInks Shade(PictureShade shade) => Shades.TryGetValue(shade, out ShadeInks? inks) ? inks
        : throw new InvalidDataException($"Image {Name} was not pre-coloured in shade {shade.Key}.");

    /// <summary>Names the image with its size.</summary>
    public override string ToString() => $"{Name} ({Size.X}x{Size.Y})";
}
