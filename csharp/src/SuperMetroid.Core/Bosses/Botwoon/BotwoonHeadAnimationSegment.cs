namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's head animation: what it does and which way it faces.</summary>
internal readonly record struct BotwoonHeadAnimationSegment(BotwoonHeadAnimationSegmentKind Kind, CompassDirection Facing)
{
    /// <summary>The hidden head. Facing is irrelevant and fixed so equal animations compare equal.</summary>
    internal static BotwoonHeadAnimationSegment Hidden => new(BotwoonHeadAnimationSegmentKind.Hidden, CompassDirection.Up);

    /// <summary>
    /// The visible moving head for a bearing: the direction opening the eighth of the turn the bearing lies in selects
    /// one of the eight closed-mouth animations.
    /// </summary>
    internal static BotwoonHeadAnimationSegment MovingToward(Bearing bearing) =>
        new(BotwoonHeadAnimationSegmentKind.Moving, bearing.DirectionOfEighth());

    /// <summary>The spitting head for a bearing: it faces the compass direction nearest the bearing, so Samus is aimed at.</summary>
    internal static BotwoonHeadAnimationSegment SpittingToward(Bearing bearing) =>
        new(BotwoonHeadAnimationSegmentKind.Spitting, bearing.NearestDirection());
}
