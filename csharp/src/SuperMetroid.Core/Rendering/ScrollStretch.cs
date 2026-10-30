namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A plane stretched downward through a band of lines (#1468), as Crocomire's body sags while it melts: each line from
/// <see cref="FirstLine"/> up to <see cref="EndLine"/> shows the plane's row that falls behind the line by
/// <see cref="Slope"/> of a row for every line below the band's first, so the band repeats rows and the picture stretches.
/// Lines from the band's end down show the plane at <see cref="BaseScroll"/>; lines above the band keep
/// <see cref="ScrollAbove"/>, the scroll the plane was last levelled at. The backend's shader evaluates the stretch line
/// by line; nothing keeps a table of lines.
/// </summary>
/// <param name="ScrollAbove">The vertical scroll the lines above the band show, 0 through 65535.</param>
/// <param name="BaseScroll">The vertical scroll the band stretches from and the lines below it show, 0 through 65535.</param>
/// <param name="FirstLine">The band's first screen line, 0 through the screen's height.</param>
/// <param name="EndLine">The screen line after the band's last, <paramref name="FirstLine"/> through the screen's height.</param>
/// <param name="Slope">The rows the band falls behind per line, in 1/65536ths of a row, 0 through 65535.</param>
public sealed record ScrollStretch(int ScrollAbove, int BaseScroll, int FirstLine, int EndLine, int Slope) : LineScroll
{
    /// <summary>The vertical scroll the lines above the band show, 0 through 65535.</summary>
    public int ScrollAbove
    {
        get;
        init => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    } = Ensure.BetweenInclusive(0, ushort.MaxValue, ScrollAbove);

    /// <summary>The vertical scroll the band stretches from and the lines below it show, 0 through 65535.</summary>
    public override int BaseScroll
    {
        get;
        init => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    } = Ensure.BetweenInclusive(0, ushort.MaxValue, BaseScroll);

    /// <summary>The band's first screen line.</summary>
    public int FirstLine
    {
        get;
        init => field = Ensure.BetweenInclusive(0, ScreenGeometry.ScreenHeightPixels, value);
    } = Ensure.BetweenInclusive(0, ScreenGeometry.ScreenHeightPixels, FirstLine);

    /// <summary>The screen line after the band's last.</summary>
    public int EndLine
    {
        get;
        init => field = Ensure.BetweenInclusive(FirstLine, ScreenGeometry.ScreenHeightPixels, value);
    } = Ensure.BetweenInclusive(FirstLine, ScreenGeometry.ScreenHeightPixels, EndLine);

    /// <summary>The rows the band falls behind per line, in 1/65536ths of a row.</summary>
    public int Slope
    {
        get;
        init => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    } = Ensure.BetweenInclusive(0, ushort.MaxValue, Slope);

    /// <summary>A plane levelled at <paramref name="scroll"/>: every line shows it, and no band stretches.</summary>
    /// <param name="scroll">The vertical scroll every line shows, 0 through 65535.</param>
    public static ScrollStretch Level(int scroll) => new(scroll, scroll, 0, 0, 0);
}
