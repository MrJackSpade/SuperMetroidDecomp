namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>
/// Botwoon's head animation (#2400): moving or spitting facing a way, or hidden inside the wall, which faces no way. Only
/// the animations its factories make exist.
/// </summary>
internal readonly record struct BotwoonHeadAnimationSegment
{
    /// <summary>The head doing <paramref name="kind"/> facing <paramref name="facing"/>, or no way for the hidden head.</summary>
    private BotwoonHeadAnimationSegment(BotwoonHeadAnimationSegmentKind kind, CompassDirection? facing)
    {
        Kind = kind;
        Facing = facing;
    }

    /// <summary>What the head is doing.</summary>
    internal BotwoonHeadAnimationSegmentKind Kind { get; }

    /// <summary>The way a moving or spitting head faces; the hidden head faces no way.</summary>
    internal CompassDirection? Facing { get; }

    /// <summary>The hidden head.</summary>
    internal static BotwoonHeadAnimationSegment Hidden { get; } = new(BotwoonHeadAnimationSegmentKind.Hidden, null);

    /// <summary>The visible moving head facing <paramref name="facing"/>, its closed mouth facing its movement.</summary>
    internal static BotwoonHeadAnimationSegment Moving(CompassDirection facing) => new(BotwoonHeadAnimationSegmentKind.Moving, facing);

    /// <summary>The spitting head facing <paramref name="facing"/>.</summary>
    internal static BotwoonHeadAnimationSegment Spitting(CompassDirection facing) => new(BotwoonHeadAnimationSegmentKind.Spitting, facing);

    /// <summary>
    /// The visible moving head for a bearing: the direction opening the eighth of the turn the bearing lies in selects
    /// one of the eight closed-mouth animations.
    /// </summary>
    internal static BotwoonHeadAnimationSegment MovingToward(Bearing bearing) => Moving(bearing.DirectionOfEighth());

    /// <summary>The spitting head for a bearing: it faces the compass direction nearest the bearing, so Samus is aimed at.</summary>
    internal static BotwoonHeadAnimationSegment SpittingToward(Bearing bearing) => Spitting(bearing.NearestDirection());
}
