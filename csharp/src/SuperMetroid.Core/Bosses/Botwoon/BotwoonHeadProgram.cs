namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's head animation: what it does and which way it faces.</summary>
internal readonly record struct BotwoonHeadProgram(BotwoonHeadProgramKind Kind, CompassDirection Facing)
{
    /// <summary>The hidden head. Facing is irrelevant and fixed so equal programs compare equal.</summary>
    internal static BotwoonHeadProgram Hidden => new(BotwoonHeadProgramKind.Hidden, CompassDirection.Up);

    /// <summary>
    /// The visible moving head for a bearing: the direction opening the eighth of the turn the bearing lies in selects
    /// one of the eight closed-mouth programs.
    /// </summary>
    internal static BotwoonHeadProgram MovingToward(Bearing bearing) =>
        new(BotwoonHeadProgramKind.Moving, bearing.DirectionOfEighth());

    /// <summary>The spitting head for a bearing: it faces the compass direction nearest the bearing, so Samus is aimed at.</summary>
    internal static BotwoonHeadProgram SpittingToward(Bearing bearing) =>
        new(BotwoonHeadProgramKind.Spitting, bearing.NearestDirection());
}
