namespace SuperMetroid.Core.Assets;

/// <summary>
/// A finished, pre-coloured image the installation provides: a sheet of an actor's frames or one part's image, coloured
/// with its standard palette. Draws place rectangles of it; nothing composes or recolours it.
/// </summary>
public sealed class SpriteImage
{
    /// <summary>The pixels, row by row from the top.</summary>
    private readonly Rgba32[] pixels;

    /// <summary>Copies the pixels of a named image.</summary>
    /// <param name="name">The image's installed name, which names it in diagnostics.</param>
    /// <param name="size">The image's width and height in pixels; both are positive.</param>
    /// <param name="pixels">Width times height pixels, row by row from the top; a pixel with zero alpha is transparent.</param>
    /// <exception cref="ArgumentException">The name is empty, a dimension is zero or the pixel count does not match.</exception>
    public SpriteImage(string name, Extent16<ImagePixels> size, ReadOnlySpan<Rgba32> pixels)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        if (size.X == 0 || size.Y == 0)
            throw new ArgumentException($"Sprite image {name} has no pixels.", nameof(size));
        Ensure.LengthEqual(pixels, size.X * size.Y);
        Name = name;
        Size = size;
        this.pixels = pixels.ToArray();
    }

    /// <summary>The image's installed name.</summary>
    public string Name { get; }

    /// <summary>The image's width and height in pixels.</summary>
    public Extent16<ImagePixels> Size { get; }

    /// <summary>The pixels, row by row from the top; a pixel with zero alpha is transparent.</summary>
    public ReadOnlySpan<Rgba32> Pixels => pixels;

    /// <summary>Names the image for debugger watches.</summary>
    public override string ToString() => $"{Name} ({Size.X}x{Size.Y})";
}
