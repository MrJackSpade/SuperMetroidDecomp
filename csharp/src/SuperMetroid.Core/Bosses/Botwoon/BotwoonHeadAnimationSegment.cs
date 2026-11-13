namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's head animation: what it does and which way it faces.</summary>
internal readonly record struct BotwoonHeadAnimationSegment(BotwoonHeadAnimationSegmentKind Kind, BotwoonFacing Facing)
{
    /// <summary>The hidden head. Facing is irrelevant and fixed so equal programs compare equal.</summary>
    internal static BotwoonHeadAnimationSegment Hidden => new(BotwoonHeadAnimationSegmentKind.Hidden, BotwoonFacing.Up);

    /// <summary>
    /// The visible moving head for a byte angle: its octant selects one of the eight
    /// closed-mouth programs.
    /// </summary>
    internal static BotwoonHeadAnimationSegment MovingToward(Bearing bearing) =>
        new(BotwoonHeadAnimationSegmentKind.Moving, BotwoonAnimations.FacingOfOctant(bearing.Sector(8)));

    /// <summary>
    /// The spitting head for a byte angle, biased half an octant so Samus is aimed at the
    /// nearest octant.
    /// </summary>
    internal static BotwoonHeadAnimationSegment SpittingToward(Bearing bearing) =>
        new(BotwoonHeadAnimationSegmentKind.Spitting, BotwoonAnimations.FacingOfOctant(bearing.TurnedBySteps(16).Sector(8)));
}
