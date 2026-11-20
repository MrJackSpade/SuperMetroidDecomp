namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>
/// Botwoon's eight health bands (#1544, #2266), each shown once his health falls below its threshold, 375 health apart
/// from his full 3000 down; the import pre-colours his head and body in each band's colours.
/// </summary>
public static class BotwoonHealthBandDefinitions
{
    /// <summary>Botwoon below 3000 health: hurt at all.</summary>
    public static BotwoonHealthBand BelowFull { get; } = new(3000, "below-full");

    /// <summary>Botwoon below 2625 health: below seven eighths of his health.</summary>
    public static BotwoonHealthBand BelowSevenEighths { get; } = new(2625, "below-seven-eighths");

    /// <summary>Botwoon below 2250 health: below three quarters of his health.</summary>
    public static BotwoonHealthBand BelowThreeQuarters { get; } = new(2250, "below-three-quarters");

    /// <summary>Botwoon below 1875 health: below five eighths of his health.</summary>
    public static BotwoonHealthBand BelowFiveEighths { get; } = new(1875, "below-five-eighths");

    /// <summary>Botwoon below 1500 health: below half his health.</summary>
    public static BotwoonHealthBand BelowHalf { get; } = new(1500, "below-half");

    /// <summary>Botwoon below 1125 health: below three eighths of his health.</summary>
    public static BotwoonHealthBand BelowThreeEighths { get; } = new(1125, "below-three-eighths");

    /// <summary>Botwoon below 750 health: below a quarter of his health.</summary>
    public static BotwoonHealthBand BelowQuarter { get; } = new(750, "below-quarter");

    /// <summary>Botwoon below 375 health: below an eighth of his health, his last colours.</summary>
    private static BotwoonHealthBand BelowEighth { get; } = new(375, "below-eighth");

    /// <summary>The bands in the order he shows them, the order his installed health colours hold them in.</summary>
    public static IReadOnlyList<BotwoonHealthBand> InOrderShown { get; } =
        [BelowFull, BelowSevenEighths, BelowThreeQuarters, BelowFiveEighths, BelowHalf, BelowThreeEighths, BelowQuarter, BelowEighth];
}
