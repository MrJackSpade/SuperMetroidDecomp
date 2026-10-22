namespace SuperMetroid.Core.Assets;

/// <summary>One frame of a sprite sheet: its rectangle on the sheet and the point of it that lands on the draw's origin.</summary>
public sealed record SpriteSheetFrame
{
    /// <summary>Checks that the rectangle lies on the sheet; an empty frame has no width and no height and draws nothing.</summary>
    /// <param name="sheet">The image the frame is cut from.</param>
    /// <param name="corner">The rectangle's top-left pixel on the sheet.</param>
    /// <param name="size">The rectangle's width and height; both are positive, or both zero for an empty frame.</param>
    /// <param name="origin">The point of the rectangle, from its top-left pixel, that lands on the draw's origin; it may lie outside the rectangle.</param>
    /// <param name="blends">Whether the frame takes part in the screen's colour blending, as sprites coloured from the upper four palette rows do.</param>
    /// <param name="priority">The frame's authored priority against the backgrounds 0..3, which a draw may use as its own.</param>
    /// <exception cref="ArgumentException">The rectangle has one dimension but not the other, or leaves the sheet.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The priority is out of range.</exception>
    public SpriteSheetFrame(SpriteImage sheet, Position16<ImagePixels> corner, Extent16<ImagePixels> size, Offset16<ImagePixels> origin,
        bool blends, int priority)
    {
        Ensure.NotNull(sheet);
        if (priority is < 0 or > 3) throw new ArgumentOutOfRangeException(nameof(priority), priority, "Sprite priorities are 0..3.");
        if ((size.X == 0) != (size.Y == 0) || corner.X + size.X > sheet.Size.X || corner.Y + size.Y > sheet.Size.Y)
            throw new ArgumentException($"A {size.X}x{size.Y} frame at ({corner.X}, {corner.Y}) does not lie on {sheet}.", nameof(size));
        Sheet = sheet;
        Corner = corner;
        Size = size;
        Origin = origin;
        Blends = blends;
        Priority = priority;
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

    /// <summary>The frame's authored priority against the backgrounds 0..3.</summary>
    public int Priority { get; }
}
