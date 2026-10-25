namespace SuperMetroid.Core.Assets;

/// <summary>
/// A finished, pre-coloured image the installation provides: a sheet of an actor's frames or one part's image, or a
/// sheet of a room tileset's blocks, coloured with its standard palette. Beside the pixels it keeps each pixel's palette
/// row and colour, and the standard colours of those rows, so a palette effect that changes a row's colours recolours the
/// image as it did the characters.
/// </summary>
public sealed class SpriteImage
{
    /// <summary>The colours of one half of CGRAM's eight palette rows, sixteen each.</summary>
    public const int SpriteColourCount = 128;

    /// <summary>The pixels, row by row from the top.</summary>
    private readonly RgbaPixel[] pixels;

    /// <summary>How each pixel was inked: its palette entry, priority and part row.</summary>
    private readonly SpriteImageInk[] inks;

    /// <summary>The colours the image was coloured with.</summary>
    private readonly RgbColor[] standardColours;

    /// <summary>Copies the pixels, palette indices and standard colours of a named image.</summary>
    /// <param name="name">The image's installed name, which names it in diagnostics.</param>
    /// <param name="size">The image's width and height in pixels; both are positive.</param>
    /// <param name="pixels">Width times height pixels, row by row from the top; a pixel with zero alpha is transparent.</param>
    /// <param name="paletteIndices">Each pixel's palette row times sixteen plus its colour, zero where transparent.</param>
    /// <param name="palette">Which half of CGRAM the palette indices name, and the 128 colours of it the pixels were coloured with.</param>
    /// <param name="priorities">Each pixel's priority against the backgrounds 0..3, or a background pixel's tile priority 0..1.</param>
    /// <param name="partRows">
    /// Each pixel's part palette row 0..7, before a draw's palette bits were ORed into it, with <see cref="FixedPartRow"/>
    /// where the part keeps its own row under a draw's override.
    /// </param>
    /// <exception cref="ArgumentException">The name is empty, a dimension is zero, a length does not match or a priority exceeds 3.</exception>
    public SpriteImage(string name, Extent16<ImagePixels> size, ReadOnlySpan<RgbaPixel> pixels, ReadOnlySpan<byte> paletteIndices,
        SpriteImagePalette palette, ReadOnlySpan<byte> priorities, ReadOnlySpan<byte> partRows)
    {
        Ensure.NotNull(palette);
        if (!Enum.IsDefined(palette.Half))
            throw new ArgumentException($"Sprite image {name} names no half of CGRAM.", nameof(palette));
        ReadOnlySpan<RgbColor> standardColours = palette.Standard.AsSpan();
        ArgumentException.ThrowIfNullOrEmpty(name);
        if (size.X == 0 || size.Y == 0)
            throw new ArgumentException($"Sprite image {name} has no pixels.", nameof(size));
        Ensure.LengthEqual(pixels, size.X * size.Y);
        Ensure.LengthEqual(paletteIndices, size.X * size.Y);
        Ensure.LengthEqual(standardColours, ColourCountOf(palette.Half));
        Ensure.LengthEqual(priorities, size.X * size.Y);
        foreach (int priority in priorities)
        {
            if (priority > 3)
                throw new ArgumentException($"Sprite image {name} has a pixel of priority {priority}; priorities are 0..3.", nameof(priorities));
        }
        Ensure.LengthEqual(partRows, size.X * size.Y);
        foreach (int row in partRows)
        {
            if ((row & ~FixedPartRow) > MaximumPaletteRow)
                throw new ArgumentException($"Sprite image {name} has a pixel of part row {row}; sprite rows are 0..{MaximumPaletteRow}.", nameof(partRows));
        }
        Name = name;
        Size = size;
        Colours = palette.Half;
        this.pixels = pixels.ToArray();
        this.standardColours = standardColours.ToArray();
        inks = new SpriteImageInk[pixels.Length];
        for (int pixel = 0; pixel < inks.Length; pixel++)
            inks[pixel] = new(paletteIndices[pixel], priorities[pixel], partRows[pixel]);
    }

    /// <summary>The standard colours an image coloured as <paramref name="colours"/> keeps: all 256 for a full-palette image.</summary>
    public static int ColourCountOf(SpriteImageColours colours) =>
        colours == SpriteImageColours.FullPalette ? 2 * SpriteColourCount : SpriteColourCount;

    /// <summary>The highest sprite palette row.</summary>
    public const int MaximumPaletteRow = 7;

    /// <summary>The part-row flag of a pixel whose part names its own row, which a draw's override leaves alone.</summary>
    public const byte FixedPartRow = 8;

    /// <summary>
    /// The row a pixel of part row <paramref name="partRow"/> shows under a draw's override: its own row when the part
    /// names it, otherwise its part row with the override ORed in, as the cartridge ORs a draw's palette bits.
    /// </summary>
    public static int OverriddenRow(int partRow, int overriding) =>
        (partRow & FixedPartRow) != 0 ? partRow & MaximumPaletteRow : partRow | overriding;

    /// <summary>The image's installed name.</summary>
    public string Name { get; }

    /// <summary>The image's width and height in pixels.</summary>
    public Extent16<ImagePixels> Size { get; }

    /// <summary>Which half of CGRAM the palette indices name.</summary>
    public SpriteImageColours Colours { get; }

    /// <summary>The CGRAM colour palette index zero names: 128 for a sprite image, 0 for a background one.</summary>
    public int FirstColour => Colours == SpriteImageColours.Sprite ? SpriteColourCount : 0;

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
    public ReadOnlySpan<RgbaPixel> Pixels => pixels;

    /// <summary>How each pixel was inked, row by row from the top: its palette entry, priority and part row.</summary>
    public ReadOnlySpan<SpriteImageInk> Inks => inks;

    /// <summary>The 128 colours of the image's half of CGRAM the pixels were coloured with.</summary>
    public ReadOnlySpan<RgbColor> StandardColours => standardColours;

    /// <summary>Names the image for debugger watches.</summary>
    public override string ToString() => $"{Name} ({Size.X}x{Size.Y})";
}
