namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>
/// A body segment's animation (#2400): a body link or the tail cap facing the segment ahead of it, or a segment hidden
/// inside the wall, which faces no way. Only the animations its factories make exist.
/// </summary>
internal readonly record struct BotwoonBodyAnimationSegment
{
    /// <summary>The animation of <paramref name="kind"/> facing <paramref name="facing"/>, or no way for a hidden segment.</summary>
    private BotwoonBodyAnimationSegment(BotwoonBodyAnimationSegmentKind kind, CompassDirection? facing)
    {
        Kind = kind;
        Facing = facing;
    }

    /// <summary>What the segment shows.</summary>
    internal BotwoonBodyAnimationSegmentKind Kind { get; }

    /// <summary>The way a body link or the tail cap faces; a hidden segment faces no way.</summary>
    internal CompassDirection? Facing { get; }

    /// <summary>A body link facing <paramref name="facing"/>.</summary>
    internal static BotwoonBodyAnimationSegment Link(CompassDirection facing) => new(BotwoonBodyAnimationSegmentKind.Body, facing);

    /// <summary>The tail cap facing <paramref name="facing"/>.</summary>
    internal static BotwoonBodyAnimationSegment TailCap(CompassDirection facing) => new(BotwoonBodyAnimationSegmentKind.Tail, facing);

    /// <summary>A segment hidden inside the wall.</summary>
    internal static BotwoonBodyAnimationSegment Hidden { get; } = new(BotwoonBodyAnimationSegmentKind.Hidden, null);

    /// <summary>
    /// The segment animation of <paramref name="kind"/> facing the direction opening the eighth of the turn that holds
    /// <paramref name="toLeader"/>, the bearing to the segment ahead; a hidden segment faces no way.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="kind"/> is undefined.</exception>
    internal static BotwoonBodyAnimationSegment Toward(BotwoonBodyAnimationSegmentKind kind, Bearing toLeader) => kind switch
    {
        BotwoonBodyAnimationSegmentKind.Body => Link(toLeader.DirectionOfEighth()),
        BotwoonBodyAnimationSegmentKind.Tail => TailCap(toLeader.DirectionOfEighth()),
        BotwoonBodyAnimationSegmentKind.Hidden => Hidden,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Undefined Botwoon segment animation kind."),
    };
}
