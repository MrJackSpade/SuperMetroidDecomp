namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>A body segment's animation: what it shows and which way it faces.</summary>
internal readonly record struct BotwoonSegmentProgram(BotwoonSegmentProgramKind Kind, BotwoonFacing Facing)
{
    /// <summary>The hidden segment. Facing is irrelevant and fixed so equal programs compare equal.</summary>
    internal static BotwoonSegmentProgram Hidden => new(BotwoonSegmentProgramKind.Hidden, BotwoonFacing.Up);

    /// <summary>The spit's program. Facing is irrelevant and fixed so equal programs compare equal.</summary>
    internal static BotwoonSegmentProgram Spit => new(BotwoonSegmentProgramKind.Spit, BotwoonFacing.Up);

    /// <summary>
    /// The segment program of <paramref name="kind"/> facing the octant of <paramref name="toLeader"/>, the bearing
    /// to the segment ahead; a hidden segment has no facing.
    /// </summary>
    internal static BotwoonSegmentProgram Toward(BotwoonSegmentProgramKind kind, Bearing toLeader) =>
        kind == BotwoonSegmentProgramKind.Hidden
            ? Hidden
            : new(kind, BotwoonAnimations.FacingOfOctant(toLeader.RawValue >> 5));
}
