namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>A body segment's animation: what it shows and which way it faces.</summary>
internal readonly record struct BotwoonBodyAnimationSegment(BotwoonBodyAnimationSegmentKind Kind, BotwoonFacing Facing)
{
    /// <summary>The hidden segment. Facing is irrelevant and fixed so equal programs compare equal.</summary>
    internal static BotwoonBodyAnimationSegment Hidden => new(BotwoonBodyAnimationSegmentKind.Hidden, BotwoonFacing.Up);

    /// <summary>The spit's program. Facing is irrelevant and fixed so equal programs compare equal.</summary>
    internal static BotwoonBodyAnimationSegment Spit => new(BotwoonBodyAnimationSegmentKind.Spit, BotwoonFacing.Up);

    /// <summary>
    /// The segment program of <paramref name="kind"/> facing the octant of <paramref name="toLeader"/>, the bearing
    /// to the segment ahead; a hidden segment has no facing.
    /// </summary>
    internal static BotwoonBodyAnimationSegment Toward(BotwoonBodyAnimationSegmentKind kind, Bearing toLeader) =>
        kind == BotwoonBodyAnimationSegmentKind.Hidden
            ? Hidden
            : new(kind, BotwoonAnimations.FacingOfOctant(toLeader.Sector(8)));
}
