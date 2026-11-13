namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's head animation: what it does and which way it faces.</summary>
internal readonly record struct BotwoonHeadProgram(BotwoonHeadProgramKind Kind, BotwoonFacing Facing)
{
    /// <summary>The hidden head. Facing is irrelevant and fixed so equal programs compare equal.</summary>
    internal static BotwoonHeadProgram Hidden => new(BotwoonHeadProgramKind.Hidden, BotwoonFacing.Up);

    /// <summary>
    /// The visible moving head for a byte angle: its octant selects one of the eight
    /// closed-mouth programs.
    /// </summary>
    internal static BotwoonHeadProgram MovingToward(Bearing bearing) =>
        new(BotwoonHeadProgramKind.Moving, BotwoonAnimations.FacingOfOctant(bearing.Sector(8)));

    /// <summary>
    /// The spitting head for a byte angle, biased half an octant so Samus is aimed at the
    /// nearest octant.
    /// </summary>
    internal static BotwoonHeadProgram SpittingToward(Bearing bearing) =>
        new(BotwoonHeadProgramKind.Spitting, BotwoonAnimations.FacingOfOctant(bearing.TurnedBySteps(16).Sector(8)));
}
