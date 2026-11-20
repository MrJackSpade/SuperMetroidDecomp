namespace Engine.Shared;

/// <summary>
/// A rectangle of whole pixels with its sides along the axes: the only shape the game collides. Its right and bottom
/// edges lie just past its last column and row, so a rectangle at left 0 that is 16 wide covers columns 0 through 15
/// and has its right edge at 16.
/// </summary>
public readonly record struct PixelRect
{
    /// <summary>A rectangle from its top-left corner and its size.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The width or height is negative.</exception>
    public PixelRect(int left, int top, int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(width);
        ArgumentOutOfRangeException.ThrowIfNegative(height);
        Left = left;
        Top = top;
        Width = width;
        Height = height;
    }

    /// <summary>The leftmost column.</summary>
    public int Left { get; }

    /// <summary>The topmost row.</summary>
    public int Top { get; }

    /// <summary>How many columns it covers.</summary>
    public int Width { get; }

    /// <summary>How many rows it covers.</summary>
    public int Height { get; }

    /// <summary>The edge just past the rightmost column.</summary>
    public int Right => Left + Width;

    /// <summary>The edge just past the bottom row.</summary>
    public int Bottom => Top + Height;

    /// <summary>Whether it covers no pixels.</summary>
    public bool IsEmpty => Width == 0 || Height == 0;

    /// <summary>A rectangle reaching the given distances each way from a centre.</summary>
    public static PixelRect Around(int centreX, int centreY, int halfWidth, int halfHeight) =>
        new(centreX - halfWidth, centreY - halfHeight, halfWidth * 2, halfHeight * 2);

    /// <summary>
    /// This rectangle, given relative to something's origin, moved to where that thing stands. Only the whole pixels of
    /// the place are used: a collider lies on whole pixels wherever within a pixel its owner is.
    /// </summary>
    public PixelRect At(PixelPosition origin) => new(Left + origin.X.Whole, Top + origin.Y.Whole, Width, Height);

    /// <summary>This rectangle moved by whole pixels.</summary>
    public PixelRect MovedBy(int x, int y) => new(Left + x, Top + y, Width, Height);

    /// <summary>Whether the two rectangles share at least one pixel. Rectangles that only touch along an edge do not.</summary>
    public bool Overlaps(PixelRect other) =>
        !IsEmpty && !other.IsEmpty && Left < other.Right && other.Left < Right && Top < other.Bottom && other.Top < Bottom;

    /// <summary>Whether the two rectangles share a pixel or lie edge against edge or corner against corner.</summary>
    public bool OverlapsOrTouches(PixelRect other) =>
        !IsEmpty && !other.IsEmpty && Left <= other.Right && other.Left <= Right && Top <= other.Bottom && other.Top <= Bottom;

    /// <summary>Whether the pixel at the given column and row lies inside.</summary>
    public bool Contains(int x, int y) => x >= Left && x < Right && y >= Top && y < Bottom;
}
