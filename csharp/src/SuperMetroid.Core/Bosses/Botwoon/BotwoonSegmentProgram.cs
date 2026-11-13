namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>A body segment's animation: what it shows and which way it faces.</summary>
internal readonly record struct BotwoonSegmentProgram(BotwoonSegmentProgramKind Kind, CompassDirection Facing)
{
    /// <summary>The hidden segment. Facing is irrelevant and fixed so equal programs compare equal.</summary>
    internal static BotwoonSegmentProgram Hidden => new(BotwoonSegmentProgramKind.Hidden, CompassDirection.Up);

    /// <summary>The spit's program. Facing is irrelevant and fixed so equal programs compare equal.</summary>
    internal static BotwoonSegmentProgram Spit => new(BotwoonSegmentProgramKind.Spit, CompassDirection.Up);

    /// <summary>
    /// The segment program of <paramref name="kind"/> facing the direction opening the eighth of the turn that holds
    /// <paramref name="toLeader"/>, the bearing to the segment ahead; a hidden segment has no facing.
    /// </summary>
    internal static BotwoonSegmentProgram Toward(BotwoonSegmentProgramKind kind, Bearing toLeader) =>
        kind == BotwoonSegmentProgramKind.Hidden
            ? Hidden
            : new(kind, toLeader.DirectionOfEighth());
}
