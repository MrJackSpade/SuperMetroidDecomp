namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's eight health bands (#2259), each 312 health wide from his last point of health upward, the highest holding
/// the rest (#1940); each lower band shows his body redder.
/// </summary>
public static class PhantoonHealthBandDefinitions
{
    /// <summary>How much health each band spans.</summary>
    private const int BandWidth = 312;

    /// <summary>His last point of health, where his lowest band starts.</summary>
    private const int LastHealth = 1;

    /// <summary>Phantoon at 2185 health or more: his own colours.</summary>
    public static PhantoonHealthBand Unhurt { get; } = new(LeastHealthOf(7), "unhurt");

    /// <summary>Phantoon from 1873 through 2184 health: below seven eighths of his health.</summary>
    public static PhantoonHealthBand BelowSevenEighths { get; } = new(LeastHealthOf(6), "below-seven-eighths");

    /// <summary>Phantoon from 1561 through 1872 health: below three quarters of his health.</summary>
    public static PhantoonHealthBand BelowThreeQuarters { get; } = new(LeastHealthOf(5), "below-three-quarters");

    /// <summary>Phantoon from 1249 through 1560 health: below five eighths of his health.</summary>
    public static PhantoonHealthBand BelowFiveEighths { get; } = new(LeastHealthOf(4), "below-five-eighths");

    /// <summary>Phantoon from 937 through 1248 health: below half his health.</summary>
    public static PhantoonHealthBand BelowHalf { get; } = new(LeastHealthOf(3), "below-half");

    /// <summary>Phantoon from 625 through 936 health: below three eighths of his health.</summary>
    private static PhantoonHealthBand BelowThreeEighths { get; } = new(LeastHealthOf(2), "below-three-eighths");

    /// <summary>Phantoon from 313 through 624 health: below a quarter of his health.</summary>
    public static PhantoonHealthBand BelowQuarter { get; } = new(LeastHealthOf(1), "below-quarter");

    /// <summary>Phantoon at 312 health or less: below an eighth of his health, his reddest.</summary>
    private static PhantoonHealthBand BelowEighth { get; } = new(null, "below-eighth");

    /// <summary>The bands, highest first, the order his health is tested against them.</summary>
    public static IReadOnlyList<PhantoonHealthBand> HighestFirst { get; } =
        [Unhurt, BelowSevenEighths, BelowThreeQuarters, BelowFiveEighths, BelowHalf, BelowThreeEighths, BelowQuarter, BelowEighth];

    /// <summary>The bands, lowest first: the order his installed health colours hold them in.</summary>
    public static IReadOnlyList<PhantoonHealthBand> LowestFirst { get; } =
        [BelowEighth, BelowQuarter, BelowThreeEighths, BelowHalf, BelowFiveEighths, BelowThreeQuarters, BelowSevenEighths, Unhurt];

    /// <summary>The least health of the band <paramref name="bandsUp"/> bands above his lowest.</summary>
    private static int LeastHealthOf(int bandsUp) => LastHealth + bandsUp * BandWidth;
}
