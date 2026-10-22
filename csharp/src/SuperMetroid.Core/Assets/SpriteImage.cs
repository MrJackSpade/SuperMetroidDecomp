namespace SuperMetroid.Core.Assets;

/// <summary>
/// A finished, pre-coloured image the installation provides: a sheet of an actor's frames or one part's image, coloured
/// with its standard palette. Beside the pixels it keeps each pixel's sprite palette row and colour, and the standard
/// colours of those rows, so a palette effect that changes a row's colours recolours the image as it did the characters.
/// </summary>
public sealed class SpriteImage
{
    /// <summary>The sprite colours of the eight OBJ palette rows, sixteen each.</summary>
    public const int SpriteColourCount = 128;

    /// <summary>The pixels, row by row from the top.</summary>
    private readonly Rgba32[] pixels;

    /// <summary>Each pixel's palette row times sixteen plus its colour; zero where the pixel is transparent.</summary>
    private readonly byte[] paletteIndices;

    /// <summary>The colours the image was coloured with.</summary>
    private readonly Bgr555[] standardColours;

    /// <summary>Each pixel's priority against the backgrounds 0..3, as the part that shows it had it.</summary>
    private readonly byte[] priorities;

    /// <summary>Each pixel's part palette row 0..7, before a draw's palette bits were ORed into it.</summary>
    private readonly byte[] partRows;

    /// <summary>Copies the pixels, palette indices and standard colours of a named image.</summary>
    /// <param name="name">The image's installed name, which names it in diagnostics.</param>
    /// <param name="size">The image's width and height in pixels; both are positive.</param>
    /// <param name="pixels">Width times height pixels, row by row from the top; a pixel with zero alpha is transparent.</param>
    /// <param name="paletteIndices">Each pixel's palette row times sixteen plus its colour, zero where transparent.</param>
    /// <param name="standardColours">The 128 sprite colours the pixels were coloured with.</param>
    /// <param name="priorities">Each pixel's priority against the backgrounds 0..3.</param>
    /// <param name="partRows">Each pixel's part palette row 0..7, before a draw's palette bits were ORed into it.</param>
    /// <exception cref="ArgumentException">The name is empty, a dimension is zero, a length does not match or a priority exceeds 3.</exception>
    public SpriteImage(string name, Extent16<ImagePixels> size, ReadOnlySpan<Rgba32> pixels, ReadOnlySpan<byte> paletteIndices,
        ReadOnlySpan<Bgr555> standardColours, ReadOnlySpan<byte> priorities, ReadOnlySpan<byte> partRows)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        if (size.X == 0 || size.Y == 0)
            throw new ArgumentException($"Sprite image {name} has no pixels.", nameof(size));
        Ensure.LengthEqual(pixels, size.X * size.Y);
        Ensure.LengthEqual(paletteIndices, size.X * size.Y);
        Ensure.LengthEqual(standardColours, SpriteColourCount);
        Ensure.LengthEqual(priorities, size.X * size.Y);
        foreach (byte priority in priorities)
        {
            if (priority > 3)
                throw new ArgumentException($"Sprite image {name} has a pixel of priority {priority}; priorities are 0..3.", nameof(priorities));
        }
        Ensure.LengthEqual(partRows, size.X * size.Y);
        foreach (byte row in partRows)
        {
            if (row > MaximumPaletteRow)
                throw new ArgumentException($"Sprite image {name} has a pixel of part row {row}; sprite rows are 0..{MaximumPaletteRow}.", nameof(partRows));
        }
        Name = name;
        Size = size;
        this.pixels = pixels.ToArray();
        this.paletteIndices = paletteIndices.ToArray();
        this.standardColours = standardColours.ToArray();
        this.priorities = priorities.ToArray();
        this.partRows = partRows.ToArray();
    }

    /// <summary>The highest sprite palette row.</summary>
    public const int MaximumPaletteRow = 7;

    /// <summary>The image's installed name.</summary>
    public string Name { get; }

    /// <summary>The image's width and height in pixels.</summary>
    public Extent16<ImagePixels> Size { get; }

    /// <summary>The pixels, row by row from the top; a pixel with zero alpha is transparent.</summary>
    public ReadOnlySpan<Rgba32> Pixels => pixels;

    /// <summary>Each pixel's palette row times sixteen plus its colour, zero where transparent.</summary>
    public ReadOnlySpan<byte> PaletteIndices => paletteIndices;

    /// <summary>The 128 sprite colours the pixels were coloured with.</summary>
    public ReadOnlySpan<Bgr555> StandardColours => standardColours;

    /// <summary>Each pixel's priority against the backgrounds 0..3, as the part that shows it had it.</summary>
    public ReadOnlySpan<byte> Priorities => priorities;

    /// <summary>Each pixel's part palette row 0..7, before a draw's palette bits were ORed into it.</summary>
    public ReadOnlySpan<byte> PartRows => partRows;

    /// <summary>Names the image for debugger watches.</summary>
    public override string ToString() => $"{Name} ({Size.X}x{Size.Y})";
}
