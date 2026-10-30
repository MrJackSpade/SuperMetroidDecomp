namespace SuperMetroid.Core.Rendering;

/// <summary>One band of a <see cref="ScrollBands"/>: the screen line it begins on and the scroll its lines show (#1468).</summary>
/// <param name="FirstLine">The screen line the band begins on, 0 through the screen's height.</param>
/// <param name="Scroll">The scroll the band's lines show, 0 through 65535.</param>
public readonly record struct ScrollBand(int FirstLine, int Scroll)
{
    /// <summary>The screen line the band begins on.</summary>
    public int FirstLine { get; init; } = Ensure.BetweenInclusive(0, ScreenGeometry.ScreenHeightPixels, FirstLine);

    /// <summary>The scroll the band's lines show.</summary>
    public int Scroll { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, Scroll);
}
