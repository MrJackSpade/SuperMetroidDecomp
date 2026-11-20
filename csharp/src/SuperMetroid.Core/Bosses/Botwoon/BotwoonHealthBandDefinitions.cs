namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>
/// Botwoon's health bands (#1544, #2266), each shown once his health falls below its threshold, evenly apart from his
/// full health down; the import pre-colours his head and body in each band's colours.
/// </summary>
public static class BotwoonHealthBandDefinitions
{
    // Declared from his last band up: each band is built with the band he shows after it.
    /// <summary>Botwoon below an eighth of his health, his last colours.</summary>
    private static BotwoonHealthBand BelowEighth { get; } = new(375, "below-eighth", null);

    /// <summary>Botwoon below a quarter of his health.</summary>
    public static BotwoonHealthBand BelowQuarter { get; } = new(750, "below-quarter", BelowEighth);

    /// <summary>Botwoon below three eighths of his health.</summary>
    public static BotwoonHealthBand BelowThreeEighths { get; } = new(1125, "below-three-eighths", BelowQuarter);

    /// <summary>Botwoon below half his health.</summary>
    public static BotwoonHealthBand BelowHalf { get; } = new(1500, "below-half", BelowThreeEighths);

    /// <summary>Botwoon below five eighths of his health.</summary>
    public static BotwoonHealthBand BelowFiveEighths { get; } = new(1875, "below-five-eighths", BelowHalf);

    /// <summary>Botwoon below three quarters of his health.</summary>
    public static BotwoonHealthBand BelowThreeQuarters { get; } = new(2250, "below-three-quarters", BelowFiveEighths);

    /// <summary>Botwoon below seven eighths of his health.</summary>
    public static BotwoonHealthBand BelowSevenEighths { get; } = new(2625, "below-seven-eighths", BelowThreeQuarters);

    /// <summary>Botwoon below his full health: hurt at all.</summary>
    public static BotwoonHealthBand BelowFull { get; } = new(3000, "below-full", BelowSevenEighths);

    /// <summary>The bands in the order he shows them, the order his installed health colours hold them in.</summary>
    public static IReadOnlyList<BotwoonHealthBand> InOrderShown { get; } =
        [BelowFull, BelowSevenEighths, BelowThreeQuarters, BelowFiveEighths, BelowHalf, BelowThreeEighths, BelowQuarter, BelowEighth];
}
