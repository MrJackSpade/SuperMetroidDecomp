namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's four health bands (#2259), each lower one showing her more damaged colours.</summary>
public static class MotherBrainHealthBandDefinitions
{
    /// <summary>Mother Brain at 9000 health or more, shown undamaged.</summary>
    public static MotherBrainHealthBand Unhurt { get; } = new(9000, "unhurt");

    /// <summary>Mother Brain from 5400 health, her first damaged colours.</summary>
    public static MotherBrainHealthBand Wounded { get; } = new(5400, "wounded");

    /// <summary>Mother Brain from 1800 health, her second damaged colours.</summary>
    public static MotherBrainHealthBand BadlyWounded { get; } = new(1800, "badly-wounded");

    /// <summary>Mother Brain below 1800 health, her most damaged colours.</summary>
    public static MotherBrainHealthBand NearlyDead { get; } = new(null, "nearly-dead");

    /// <summary>The bands, highest first: the order her health is tested against them and her installed health colours hold them.</summary>
    public static IReadOnlyList<MotherBrainHealthBand> HighestFirst { get; } = [Unhurt, Wounded, BadlyWounded, NearlyDead];
}
