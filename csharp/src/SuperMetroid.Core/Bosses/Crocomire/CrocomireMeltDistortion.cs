namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// How Crocomire's melt sags his body layer (#1468): from <see cref="FirstLine"/> up to <see cref="EndLine"/> each screen
/// line falls a further <see cref="Slope"/> of a row behind <see cref="BaseScroll"/>; the lines below show the base, and
/// the lines above keep <see cref="ScrollAbove"/>, the scroll the melt was last levelled at. The display draws it as a
/// scroll stretch; the boss keeps no table of lines.
/// </summary>
/// <param name="ScrollAbove">The vertical scroll the lines above the band show, 0 through 65535.</param>
/// <param name="BaseScroll">His body layer's vertical placement the band stretches from, 0 through 65535.</param>
/// <param name="FirstLine">The band's first screen line.</param>
/// <param name="EndLine">The screen line after the band's last.</param>
/// <param name="Slope">The rows the band falls behind per line, in 1/65536ths of a row.</param>
public sealed record CrocomireMeltDistortion(int ScrollAbove, int BaseScroll, int FirstLine, int EndLine, int Slope)
{
    /// <summary>The melt levelled at <paramref name="scroll"/>: every line shows it, and no band stretches.</summary>
    /// <param name="scroll">The vertical scroll every line shows.</param>
    public static CrocomireMeltDistortion Level(int scroll) => new(scroll, scroll, 0, 0, 0);
}
