namespace SuperMetroid.Core.Game;

/// <summary>The typed animations that replace Botwoon's native instruction lists (#641).</summary>
internal static class BotwoonAnimations
{
    /// <summary>Number of frames in a body link's cycle.</summary>
    internal const int BodyPhaseCount = 4;

    /// <summary>Number of frames in the spit's cycle.</summary>
    internal const int SpitPhaseCount = 5;

    /// <summary>Native clockwise octant order used by head and segment selectors.</summary>
    private static readonly BotwoonFacing[] Octants =
    [
        BotwoonFacing.Up, BotwoonFacing.UpRight, BotwoonFacing.Right, BotwoonFacing.DownRight,
        BotwoonFacing.Down, BotwoonFacing.DownLeft, BotwoonFacing.Left, BotwoonFacing.UpLeft,
    ];

    /// <summary>The closed-mouth aiming hold before Botwoon publishes the spit cue.</summary>
    private const int MouthClosedBeforeSpit = 32;
    /// <summary>The usual open-mouth hold after a spit volley.</summary>
    private const int MouthOpenAfterSpit = 16;
    /// <summary>The longer left-facing open-mouth hold at $B3:93B9.</summary>
    private const int MouthOpenAfterSpitLeft = 25;
    /// <summary>Duration of each of a body link's four cycling frames.</summary>
    private const int BodyFrameDuration = 8;
    /// <summary>Duration of each of the spit's five cycling frames.</summary>
    private const int SpitFrameDuration = 3;

    /// <summary>The invisible head frame followed by a permanent hold while Botwoon is inside a wall.</summary>
    private static readonly AnimationTimeline<BotwoonHeadFrame, BotwoonHeadCue> HiddenHead =
        new(AnimationStep<BotwoonHeadFrame, BotwoonHeadCue>.Show(BotwoonHeadFrame.Hidden, 1),
            AnimationStep<BotwoonHeadFrame, BotwoonHeadCue>.Hold);

    /// <summary>All moving and spitting head timelines by octant, plus the shared hidden timeline.</summary>
    private static readonly Dictionary<BotwoonHeadProgram, AnimationTimeline<BotwoonHeadFrame, BotwoonHeadCue>> HeadTimelines =
        Enum.GetValues<BotwoonFacing>().SelectMany(facing => new[]
        {
            (new BotwoonHeadProgram(BotwoonHeadProgramKind.Moving, facing), MovingHead(facing)),
            (new BotwoonHeadProgram(BotwoonHeadProgramKind.Spitting, facing), SpittingHead(facing)),
        }).Append((BotwoonHeadProgram.Hidden, HiddenHead)).ToDictionary(entry => entry.Item1, entry => entry.Item2);

    /// <summary>Body cycles, stationary tail caps, and the hidden body/tail program by facing.</summary>
    private static readonly Dictionary<BotwoonSegmentProgram, AnimationTimeline<BotwoonProjectileFrame, NoAnimationCue>> SegmentTimelines =
        Enum.GetValues<BotwoonFacing>().SelectMany(facing => new[]
        {
            (new BotwoonSegmentProgram(BotwoonSegmentProgramKind.Body, facing), BodyLink(facing)),
            (new BotwoonSegmentProgram(BotwoonSegmentProgramKind.Tail, facing), StillSegment(new(BotwoonProjectileFrameKind.Tail, facing, 0))),
        }).Append((BotwoonSegmentProgram.Hidden, StillSegment(new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.Up, 0))))
            .ToDictionary(entry => entry.Item1, entry => entry.Item2);

    /// <summary>The spit's cycling animation (<c>InstList_EnemyProjectile_BotwoonsSpit</c>).</summary>
    internal static AnimationTimeline<BotwoonProjectileFrame, NoAnimationCue> Spit { get; } = new(
        [.. Enumerable.Range(0, SpitPhaseCount).Select(phase => AnimationStep<BotwoonProjectileFrame, NoAnimationCue>.Show(
            new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.Up, phase), SpitFrameDuration)),
         AnimationStep<BotwoonProjectileFrame, NoAnimationCue>.Loop]);

    /// <summary>The facing of cartridge angle octant <paramref name="octant"/>, zero (up) through seven, clockwise.</summary>
    internal static BotwoonFacing FacingOfOctant(int octant) =>
        (uint)octant < (uint)Octants.Length
            ? Octants[octant]
            : throw new ArgumentOutOfRangeException(nameof(octant), octant, "A cartridge angle octant is zero through seven.");

    /// <summary>The head timeline for <paramref name="program"/>.</summary>
    internal static AnimationTimeline<BotwoonHeadFrame, BotwoonHeadCue> Head(BotwoonHeadProgram program) =>
        HeadTimelines.TryGetValue(program, out var timeline)
            ? timeline
            : throw new ArgumentOutOfRangeException(nameof(program), program, "Botwoon has no such head program.");

    /// <summary>The body-segment timeline for <paramref name="program"/>.</summary>
    internal static AnimationTimeline<BotwoonProjectileFrame, NoAnimationCue> Segment(BotwoonSegmentProgram program) =>
        SegmentTimelines.TryGetValue(program, out var timeline)
            ? timeline
            : throw new ArgumentOutOfRangeException(nameof(program), program, "Botwoon has no such segment program.");

    /// <summary>
    /// The head's collision half-extents facing <paramref name="facing"/>: tall vertically, wide
    /// horizontally and square on the diagonals.
    /// </summary>
    internal static Extent16<RoomPixels> HeadRadius(BotwoonFacing facing) => facing switch
    {
        BotwoonFacing.Up or BotwoonFacing.Down => new(8, 16),
        BotwoonFacing.Left or BotwoonFacing.Right => new(16, 8),
        BotwoonFacing.UpRight or BotwoonFacing.DownRight or BotwoonFacing.DownLeft or BotwoonFacing.UpLeft => new(12, 12),
        _ => throw new ArgumentOutOfRangeException(nameof(facing), facing, "Undefined Botwoon facing."),
    };

    /// <summary>Publishes a facing-radius update before showing and holding the closed mouth.</summary>
    private static AnimationTimeline<BotwoonHeadFrame, BotwoonHeadCue> MovingHead(BotwoonFacing facing) => new(
        AnimationStep<BotwoonHeadFrame, BotwoonHeadCue>.Publish(BotwoonHeadCue.ApplyFacingRadius),
        AnimationStep<BotwoonHeadFrame, BotwoonHeadCue>.Show(BotwoonHeadFrame.Closed(facing), 1),
        AnimationStep<BotwoonHeadFrame, BotwoonHeadCue>.Hold);

    /// <summary>Holds aim, publishes radius/sound/volley cues, then opens the mouth with a longer leftward hold.</summary>
    private static AnimationTimeline<BotwoonHeadFrame, BotwoonHeadCue> SpittingHead(BotwoonFacing facing) => new(
        AnimationStep<BotwoonHeadFrame, BotwoonHeadCue>.Show(BotwoonHeadFrame.Closed(facing), MouthClosedBeforeSpit),
        AnimationStep<BotwoonHeadFrame, BotwoonHeadCue>.Publish(BotwoonHeadCue.ApplyFacingRadius),
        AnimationStep<BotwoonHeadFrame, BotwoonHeadCue>.Publish(BotwoonHeadCue.QueueSpitSound),
        AnimationStep<BotwoonHeadFrame, BotwoonHeadCue>.Publish(BotwoonHeadCue.ReachSpitFrame),
        // The cartridge holds the open mouth longer only when spitting left ($B3:93B9 is $19, the rest $10).
        AnimationStep<BotwoonHeadFrame, BotwoonHeadCue>.Show(BotwoonHeadFrame.Open(facing),
            facing == BotwoonFacing.Left ? MouthOpenAfterSpitLeft : MouthOpenAfterSpit),
        AnimationStep<BotwoonHeadFrame, BotwoonHeadCue>.Hold);

    /// <summary>Cycles four facing-specific body-link pictures at eight updates apiece.</summary>
    private static AnimationTimeline<BotwoonProjectileFrame, NoAnimationCue> BodyLink(BotwoonFacing facing) => new(
        [.. Enumerable.Range(0, BodyPhaseCount).Select(phase => AnimationStep<BotwoonProjectileFrame, NoAnimationCue>.Show(
            new(BotwoonProjectileFrameKind.Body, facing, phase), BodyFrameDuration)),
         AnimationStep<BotwoonProjectileFrame, NoAnimationCue>.Loop]);

    /// <summary>Shows one tail or hidden frame and holds it without advancing.</summary>
    private static AnimationTimeline<BotwoonProjectileFrame, NoAnimationCue> StillSegment(BotwoonProjectileFrame frame) => new(
        AnimationStep<BotwoonProjectileFrame, NoAnimationCue>.Show(frame, 1),
        AnimationStep<BotwoonProjectileFrame, NoAnimationCue>.Hold);
}
