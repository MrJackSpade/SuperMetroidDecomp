namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Draygon's health bands (#2266), each an eighth of his health wide from none upward, the highest holding the rest up
/// to <see cref="MaximumAuthoredHealth"/>; each lower band shows his body layer more hurt.
/// </summary>
public static class DraygonHealthBandDefinitions
{
    /// <summary>Draygon body's authored health.</summary>
    public const int MaximumAuthoredHealth = 6000;

    /// <summary>Draygon at seven eighths of his health or more: his own colours.</summary>
    public static DraygonHealthBand Unhurt { get; } = new(5250, "unhurt");

    /// <summary>Draygon below seven eighths of his health.</summary>
    public static DraygonHealthBand BelowSevenEighths { get; } = new(4500, "below-seven-eighths");

    /// <summary>Draygon below three quarters of his health.</summary>
    public static DraygonHealthBand BelowThreeQuarters { get; } = new(3750, "below-three-quarters");

    /// <summary>Draygon below five eighths of his health.</summary>
    public static DraygonHealthBand BelowFiveEighths { get; } = new(3000, "below-five-eighths");

    /// <summary>Draygon below half his health.</summary>
    public static DraygonHealthBand BelowHalf { get; } = new(2250, "below-half");

    /// <summary>Draygon below three eighths of his health.</summary>
    public static DraygonHealthBand BelowThreeEighths { get; } = new(1500, "below-three-eighths");

    /// <summary>Draygon below a quarter of his health.</summary>
    public static DraygonHealthBand BelowQuarter { get; } = new(750, "below-quarter");

    /// <summary>Draygon below an eighth of his health, his most hurt colours.</summary>
    public static DraygonHealthBand BelowEighth { get; } = new(0, "below-eighth");

    /// <summary>The bands, highest first: the order his health is tested against them and his installed health colours hold them in.</summary>
    public static IReadOnlyList<DraygonHealthBand> HighestFirst { get; } =
        [Unhurt, BelowSevenEighths, BelowThreeQuarters, BelowFiveEighths, BelowHalf, BelowThreeEighths, BelowQuarter, BelowEighth];
}
