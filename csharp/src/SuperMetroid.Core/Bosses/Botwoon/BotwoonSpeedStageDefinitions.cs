namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>
/// Botwoon's three speed stages (#2266); the European release moves him at other speeds. Each stage spaces his body
/// segments twelve history entries divided by its movement speed apart, so a faster head pulls its body along in the
/// same time.
/// </summary>
public static class BotwoonSpeedStageDefinitions
{
    /// <summary>Botwoon at half his starting health or more, or at none: two pixels or steps an update, spits at speed two.</summary>
    public static BotwoonSpeedStage Full { get; } = new(2, 6, 2, "full");

    /// <summary>Botwoon below half his starting health: three pixels or steps an update, spits at speed three.</summary>
    public static BotwoonSpeedStage BelowHalf { get; } = new(3, 4, 3, "below-half");

    /// <summary>Botwoon below a quarter of his starting health: four pixels or steps an update, spits at speed four.</summary>
    public static BotwoonSpeedStage BelowQuarter { get; } = new(4, 3, 4, "below-quarter");
}
