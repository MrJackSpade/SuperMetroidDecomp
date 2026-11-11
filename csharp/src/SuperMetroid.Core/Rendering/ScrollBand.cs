namespace SuperMetroid.Core.Rendering;

/// <summary>One band of a <see cref="ScrollBands"/>: the screen line it begins on and the scroll its lines show (#1468).</summary>
/// <param name="FirstLine">The screen line the band begins on, 0 through the screen's height.</param>
/// <param name="Scroll">
/// The scroll the band's lines show, any whole number of pixels: the plane repeats along the axis, so a scroll and the
/// same scroll a whole number of plane lengths on show the same lines.
/// </param>
public readonly record struct ScrollBand(int FirstLine, int Scroll)
{
    /// <summary>The screen line the band begins on.</summary>
    public int FirstLine { get; init; } = Ensure.BetweenInclusive(0, ScreenGeometry.ScreenHeightPixels, FirstLine);
}
