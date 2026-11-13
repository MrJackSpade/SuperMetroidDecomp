namespace SuperMetroid.Core.Assets;

/// <summary>One frame of a sprite sheet: its rectangle on the sheet and the point of it that lands on the draw's origin.</summary>
public sealed record SpriteSheetFrame
{
    /// <summary>Checks that the rectangle lies on the sheet; an empty frame has no width and no height and draws nothing.</summary>
    /// <param name="sheet">The image the frame is cut from.</param>
    /// <param name="corner">The rectangle's top-left pixel on the sheet.</param>
    /// <param name="size">The rectangle's width and height; both are positive, or both zero for an empty frame.</param>
    /// <param name="origin">The point of the rectangle, from its top-left pixel, that lands on the draw's origin; it may lie outside the rectangle.</param>
    /// <exception cref="ArgumentException">The rectangle has one dimension but not the other, or leaves the sheet.</exception>
    public SpriteSheetFrame(SpriteImage sheet, Position16<ImagePixels> corner, Extent16<ImagePixels> size, Offset16<ImagePixels> origin)
    {
        Ensure.NotNull(sheet);
        if ((size.X == 0) != (size.Y == 0) || corner.X + size.X > sheet.Size.X || corner.Y + size.Y > sheet.Size.Y)
            throw new ArgumentException($"A {size.X}x{size.Y} frame at ({corner.X}, {corner.Y}) does not lie on {sheet}.", nameof(size));
        Sheet = sheet;
        Corner = corner;
        Size = size;
        Origin = origin;
    }

    /// <summary>The image the frame is cut from.</summary>
    public SpriteImage Sheet { get; }

    /// <summary>The rectangle's top-left pixel on the sheet.</summary>
    public Position16<ImagePixels> Corner { get; }

    /// <summary>The rectangle's width and height.</summary>
    public Extent16<ImagePixels> Size { get; }

    /// <summary>The point of the rectangle, from its top-left pixel, that lands on the draw's origin.</summary>
    public Offset16<ImagePixels> Origin { get; }

    /// <summary>
    /// Where a gameplay draw of the frame sits among the room's layers (#1514), as the import decided from where the game
    /// draws the frame as a whole; null for a frame that is not an actor's, such as a picture of a layer's blocks, and for
    /// an empty frame.
    /// </summary>
    public ActorDepth? Depth { get; init; }

    /// <summary>
    /// Whether every pixel of the frame blends with what lies behind it when drawn as a sprite (#1633): a property of the
    /// picture the draw shows. A frame whose pixels blend in part reads its sheet's blend map (<see cref="SpriteImage.Blending"/>)
    /// instead, and is false here.
    /// </summary>
    public bool Blends { get; init; }

    /// <summary>Whether the frame is empty: a frame of no parts, which draws nothing.</summary>
    public bool IsEmpty => Size.X == 0;
}
