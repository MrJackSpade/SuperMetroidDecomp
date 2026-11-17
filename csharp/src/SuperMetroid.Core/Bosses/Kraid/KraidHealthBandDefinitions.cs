namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's eight health bands (#2297), each an eighth of the health he initialized with wide, the highest holding his
/// whole health and the lowest everything below a quarter of it; each lower band shows him more hurt.
/// </summary>
public static class KraidHealthBandDefinitions
{
    /// <summary>Kraid at his whole health: his own colours.</summary>
    public static KraidHealthBand Unhurt { get; } = new(8, "unhurt");

    /// <summary>Kraid from seven eighths of his health up to his whole health.</summary>
    public static KraidHealthBand BelowWhole { get; } = new(7, "below-whole");

    /// <summary>Kraid from three quarters up to seven eighths of his health.</summary>
    public static KraidHealthBand BelowSevenEighths { get; } = new(6, "below-seven-eighths");

    /// <summary>Kraid from five eighths up to three quarters of his health.</summary>
    public static KraidHealthBand BelowThreeQuarters { get; } = new(5, "below-three-quarters");

    /// <summary>Kraid from half up to five eighths of his health.</summary>
    public static KraidHealthBand BelowFiveEighths { get; } = new(4, "below-five-eighths");

    /// <summary>Kraid from three eighths up to half his health.</summary>
    public static KraidHealthBand BelowHalf { get; } = new(3, "below-half");

    /// <summary>Kraid from a quarter up to three eighths of his health.</summary>
    public static KraidHealthBand BelowThreeEighths { get; } = new(2, "below-three-eighths");

    /// <summary>Kraid below a quarter of his health: his most hurt colours.</summary>
    public static KraidHealthBand BelowQuarter { get; } = new(null, "below-quarter");

    /// <summary>The bands, highest first: the order his health is tested against them.</summary>
    public static IReadOnlyList<KraidHealthBand> HighestFirst { get; } =
        [Unhurt, BelowWhole, BelowSevenEighths, BelowThreeQuarters, BelowFiveEighths, BelowHalf, BelowThreeEighths, BelowQuarter];

    /// <summary>The bands, most hurt first: the order his health colours hold them in after the hurt flash's.</summary>
    public static IReadOnlyList<KraidHealthBand> MostHurtFirst { get; } =
        [BelowQuarter, BelowThreeEighths, BelowHalf, BelowFiveEighths, BelowThreeQuarters, BelowSevenEighths, BelowWhole, Unhurt];
}
