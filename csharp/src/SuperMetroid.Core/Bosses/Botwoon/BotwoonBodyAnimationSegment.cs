namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>A body segment's animation: what it shows and which way it faces.</summary>
internal readonly record struct BotwoonBodyAnimationSegment(BotwoonBodyAnimationSegmentKind Kind, CompassDirection Facing)
{
    /// <summary>The hidden segment. Facing is irrelevant and fixed so equal animations compare equal.</summary>
    internal static BotwoonBodyAnimationSegment Hidden => new(BotwoonBodyAnimationSegmentKind.Hidden, CompassDirection.Up);

    /// <summary>The spit's animation. Facing is irrelevant and fixed so equal animations compare equal.</summary>
    internal static BotwoonBodyAnimationSegment Spit => new(BotwoonBodyAnimationSegmentKind.Spit, CompassDirection.Up);

    /// <summary>
    /// The segment animation of <paramref name="kind"/> facing the direction opening the eighth of the turn that holds
    /// <paramref name="toLeader"/>, the bearing to the segment ahead; a hidden segment has no facing.
    /// </summary>
    internal static BotwoonBodyAnimationSegment Toward(BotwoonBodyAnimationSegmentKind kind, Bearing toLeader) =>
        kind == BotwoonBodyAnimationSegmentKind.Hidden
            ? Hidden
            : new(kind, toLeader.DirectionOfEighth());
}
