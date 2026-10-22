namespace SuperMetroid.Core.Assets;

/// <summary>One frame of a sprite sheet: its rectangle on the sheet and the point of it that lands on the draw's origin.</summary>
public sealed record SpriteSheetFrame
{
    /// <summary>Checks that the rectangle lies on the sheet.</summary>
    /// <param name="sheet">The image the frame is cut from.</param>
    /// <param name="corner">The rectangle's top-left pixel on the sheet.</param>
    /// <param name="size">The rectangle's width and height; both are positive.</param>
    /// <param name="origin">The point of the rectangle, from its top-left pixel, that lands on the draw's origin; it may lie outside the rectangle.</param>
    /// <param name="blends">Whether the frame takes part in the screen's colour blending, as sprites coloured from the upper four palette rows do.</param>
    /// <exception cref="ArgumentException">The rectangle is empty or leaves the sheet.</exception>
    public SpriteSheetFrame(SpriteImage sheet, Position16<ImagePixels> corner, Extent16<ImagePixels> size, Offset16<ImagePixels> origin,
        bool blends)
    {
        Ensure.NotNull(sheet);
        if (size.X == 0 || size.Y == 0 || corner.X + size.X > sheet.Size.X || corner.Y + size.Y > sheet.Size.Y)
            throw new ArgumentException($"A {size.X}x{size.Y} frame at ({corner.X}, {corner.Y}) does not lie on {sheet}.", nameof(size));
        Sheet = sheet;
        Corner = corner;
        Size = size;
        Origin = origin;
        Blends = blends;
    }

    /// <summary>The image the frame is cut from.</summary>
    public SpriteImage Sheet { get; }

    /// <summary>The rectangle's top-left pixel on the sheet.</summary>
    public Position16<ImagePixels> Corner { get; }

    /// <summary>The rectangle's width and height.</summary>
    public Extent16<ImagePixels> Size { get; }

    /// <summary>The point of the rectangle, from its top-left pixel, that lands on the draw's origin.</summary>
    public Offset16<ImagePixels> Origin { get; }

    /// <summary>Whether the frame takes part in the screen's colour blending.</summary>
    public bool Blends { get; }
}
