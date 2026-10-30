namespace SuperMetroid.Core.Rendering;

/// <summary>A rectangle of screen pixels (#1468), both corners included; its corners may lie off the screen.</summary>
public sealed record WindowRectangle : WindowShape
{
    /// <summary>The rectangle's top-left pixel.</summary>
    public Offset16<ScreenPixels> TopLeft { get; }

    /// <summary>The rectangle's bottom-right pixel, at or right of and below <see cref="TopLeft"/>.</summary>
    public Offset16<ScreenPixels> BottomRight { get; }

    /// <summary>Creates a rectangle from its two inclusive corners.</summary>
    /// <param name="topLeft">The top-left pixel.</param>
    /// <param name="bottomRight">The bottom-right pixel, at or right of and below <paramref name="topLeft"/>.</param>
    /// <exception cref="ArgumentException">The bottom-right corner lies left of or above the top-left one.</exception>
    public WindowRectangle(Offset16<ScreenPixels> topLeft, Offset16<ScreenPixels> bottomRight)
    {
        if (bottomRight.X < topLeft.X || bottomRight.Y < topLeft.Y)
            throw new ArgumentException("A window rectangle's bottom-right corner lies left of or above its top-left corner.", nameof(bottomRight));
        TopLeft = topLeft;
        BottomRight = bottomRight;
    }

    /// <summary>The columns <paramref name="left"/> through <paramref name="right"/> on the lines <paramref name="top"/> through <paramref name="bottom"/>.</summary>
    public static WindowRectangle Spanning(int left, int top, int right, int bottom) =>
        new(Offset16<ScreenPixels>.Of(left, top), Offset16<ScreenPixels>.Of(right, bottom));
}
