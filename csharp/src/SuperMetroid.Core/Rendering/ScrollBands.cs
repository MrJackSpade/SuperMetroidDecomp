using System.Collections.Immutable;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A plane scrolled band by band along one axis (#1468), as the scrolling sky's bands drift at their own speeds: the lines
/// above the first band show <see cref="BaseScroll"/>, and each band's lines, from its first line to the next band's,
/// show the band's scroll. Without bands every line shows the base scroll. The backend's shader evaluates the bands line
/// by line; nothing keeps a table of lines.
/// </summary>
/// <param name="BaseScroll">The scroll the lines above the first band show, 0 through 65535.</param>
/// <param name="Bands">The bands from the top of the screen down, each beginning below the one before.</param>
public sealed record ScrollBands(int BaseScroll, ImmutableArray<ScrollBand> Bands) : LineScroll
{
    /// <summary>The scroll the lines above the first band show, 0 through 65535.</summary>
    public override int BaseScroll
    {
        get;
        init => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    } = Ensure.BetweenInclusive(0, ushort.MaxValue, BaseScroll);

    /// <summary>The bands from the top of the screen down; empty when every line shows the base scroll.</summary>
    /// <exception cref="ArgumentException">A band does not begin below the one before it.</exception>
    public ImmutableArray<ScrollBand> Bands
    {
        get;
        init => field = RequireDescending(value);
    } = RequireDescending(Bands);

    /// <summary>Every line at <paramref name="scroll"/>: no bands.</summary>
    /// <param name="scroll">The scroll every line shows, 0 through 65535.</param>
    public static ScrollBands Uniform(int scroll) => new(scroll, []);

    /// <summary>The lines above <paramref name="line"/> at <paramref name="scrollAbove"/>, and the lines from it down at <paramref name="scroll"/>.</summary>
    /// <param name="scrollAbove">The scroll the lines above show, 0 through 65535.</param>
    /// <param name="line">The screen line the second scroll begins on; a line outside the screen leaves one scroll on every line.</param>
    /// <param name="scroll">The scroll the lines from <paramref name="line"/> down show, 0 through 65535.</param>
    public static ScrollBands Split(int scrollAbove, int line, int scroll) =>
        new(scrollAbove, [new(Math.Clamp(line, 0, ScreenGeometry.ScreenHeightPixels), scroll)]);

    /// <summary>Requires bands each beginning below the one before.</summary>
    /// <exception cref="ArgumentException">The bands are out of order.</exception>
    private static ImmutableArray<ScrollBand> RequireDescending(ImmutableArray<ScrollBand> bands)
    {
        if (bands.IsDefault) return [];
        for (int index = 1; index < bands.Length; index++)
            if (bands[index].FirstLine <= bands[index - 1].FirstLine)
                throw new ArgumentException("Each band begins below the one before it.", nameof(bands));
        return bands;
    }
}
