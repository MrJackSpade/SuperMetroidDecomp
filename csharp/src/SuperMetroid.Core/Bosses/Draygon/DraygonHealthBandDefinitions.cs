namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Draygon's eight health bands (#2266), each 750 health wide from none upward, the highest holding the rest up to his
/// authored 6000; each lower band shows his body layer more hurt.
/// </summary>
public static class DraygonHealthBandDefinitions
{
    /// <summary>Draygon body's authored health.</summary>
    public const int MaximumAuthoredHealth = 6000;

    /// <summary>Draygon at 5250 health or more: his own colours.</summary>
    public static DraygonHealthBand Unhurt { get; } = new(5250, "unhurt");

    /// <summary>Draygon from 4500 through 5249 health: below seven eighths of his health.</summary>
    public static DraygonHealthBand BelowSevenEighths { get; } = new(4500, "below-seven-eighths");

    /// <summary>Draygon from 3750 through 4499 health: below three quarters of his health.</summary>
    public static DraygonHealthBand BelowThreeQuarters { get; } = new(3750, "below-three-quarters");

    /// <summary>Draygon from 3000 through 3749 health: below five eighths of his health.</summary>
    public static DraygonHealthBand BelowFiveEighths { get; } = new(3000, "below-five-eighths");

    /// <summary>Draygon from 2250 through 2999 health: below half his health.</summary>
    public static DraygonHealthBand BelowHalf { get; } = new(2250, "below-half");

    /// <summary>Draygon from 1500 through 2249 health: below three eighths of his health.</summary>
    public static DraygonHealthBand BelowThreeEighths { get; } = new(1500, "below-three-eighths");

    /// <summary>Draygon from 750 through 1499 health: below a quarter of his health.</summary>
    public static DraygonHealthBand BelowQuarter { get; } = new(750, "below-quarter");

    /// <summary>Draygon below 750 health: below an eighth of his health, his most hurt colours.</summary>
    public static DraygonHealthBand BelowEighth { get; } = new(null, "below-eighth");

    /// <summary>The bands, highest first: the order his health is tested against them and his installed health colours hold them in.</summary>
    public static IReadOnlyList<DraygonHealthBand> HighestFirst { get; } =
        [Unhurt, BelowSevenEighths, BelowThreeQuarters, BelowFiveEighths, BelowHalf, BelowThreeEighths, BelowQuarter, BelowEighth];
}
