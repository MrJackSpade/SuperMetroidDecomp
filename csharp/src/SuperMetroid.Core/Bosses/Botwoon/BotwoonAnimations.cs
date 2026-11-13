using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's typed animations (#641).</summary>
internal static class BotwoonAnimations
{
    /// <summary>Number of frames in a body link's cycle.</summary>
    internal const int BodyPhaseCount = 4;

    /// <summary>Number of frames in the spit's cycle.</summary>
    internal const int SpitPhaseCount = 5;

    /// <summary>Clockwise octant order used by head and segment selectors.</summary>
    private static readonly BotwoonFacing[] Octants =
    [
        BotwoonFacing.Up, BotwoonFacing.UpRight, BotwoonFacing.Right, BotwoonFacing.DownRight,
        BotwoonFacing.Down, BotwoonFacing.DownLeft, BotwoonFacing.Left, BotwoonFacing.UpLeft,
    ];

    /// <summary>The closed-mouth aiming hold before Botwoon publishes the spit cue.</summary>
    private const int MouthClosedBeforeSpit = 32;
    /// <summary>The usual open-mouth hold after a spit volley.</summary>
    private const int MouthOpenAfterSpit = 16;
    /// <summary>The longer left-facing open-mouth hold.</summary>
    private const int MouthOpenAfterSpitLeft = 25;
    /// <summary>Duration of each of a body link's four cycling frames.</summary>
    private const int BodyFrameDuration = 8;
    /// <summary>Duration of each of the spit's five cycling frames.</summary>
    private const int SpitFrameDuration = 3;

    /// <summary>Every head animation's segment: moving and spitting by facing, plus the hidden head.</summary>
    private static readonly BotwoonHeadProgram[] HeadProgramKeys =
    [
        .. Enum.GetValues<BotwoonFacing>().SelectMany(facing => new[]
        {
            new BotwoonHeadProgram(BotwoonHeadProgramKind.Moving, facing),
            new BotwoonHeadProgram(BotwoonHeadProgramKind.Spitting, facing),
        }),
        BotwoonHeadProgram.Hidden,
    ];

    /// <summary>Every projectile animation's segment: body cycles and stationary tail caps by facing, the hidden body/tail, and the spit.</summary>
    private static readonly BotwoonSegmentProgram[] SegmentProgramKeys =
    [
        .. Enum.GetValues<BotwoonFacing>().SelectMany(facing => new[]
        {
            new BotwoonSegmentProgram(BotwoonSegmentProgramKind.Body, facing),
            new BotwoonSegmentProgram(BotwoonSegmentProgramKind.Tail, facing),
        }),
        BotwoonSegmentProgram.Hidden,
        BotwoonSegmentProgram.Spit,
    ];

    /// <summary>The head's segments: each head program's beats.</summary>
    internal static AnimationSegments<BotwoonHeadProgram, BotwoonHeadFrame, BotwoonHeadCue> HeadSegments { get; } = new(
        HeadProgramKeys.ToDictionary(program => program, HeadBeats), HeadProgramKeys);

    /// <summary>The head animation that starts at each head program: it plays the program's beats once and holds its last frame.</summary>
    internal static IReadOnlyDictionary<BotwoonHeadProgram, Animation<BotwoonHeadProgram>> HeadStartingAt { get; } =
        HeadProgramKeys.ToDictionary(program => program, program => new Animation<BotwoonHeadProgram>(
            [new(program, 0, HeadSegments.Beats(program).Count, 1)], AnimationEnd.Holds));

    /// <summary>The segments a body segment or spit plays: each segment program's beats.</summary>
    internal static AnimationSegments<BotwoonSegmentProgram, BotwoonProjectileFrame, NoAnimationCue> ProjectileSegments { get; } = new(
        SegmentProgramKeys.ToDictionary(program => program, ProjectileBeats), SegmentProgramKeys);

    /// <summary>
    /// The projectile animation that starts at each segment program: a body link and the spit cycle their frames for good,
    /// and a tail cap or hidden segment shows its one frame and holds it.
    /// </summary>
    internal static IReadOnlyDictionary<BotwoonSegmentProgram, Animation<BotwoonSegmentProgram>> ProjectileStartingAt { get; } =
        SegmentProgramKeys.ToDictionary(program => program, program => new Animation<BotwoonSegmentProgram>(
            [new(program, 0, ProjectileSegments.Beats(program).Count, 1)], ProjectileEnd(program.Kind)));

    /// <summary>The head animation that starts at <paramref name="program"/>.</summary>
    internal static Animation<BotwoonHeadProgram> HeadStartingWith(BotwoonHeadProgram program) => HeadStartingAt[program];

    /// <summary>The projectile animation that starts at <paramref name="program"/>.</summary>
    internal static Animation<BotwoonSegmentProgram> ProjectileStartingWith(BotwoonSegmentProgram program) => ProjectileStartingAt[program];

    /// <summary>The facing of angle octant <paramref name="octant"/>, zero (up) through seven, clockwise.</summary>
    internal static BotwoonFacing FacingOfOctant(int octant) =>
        (uint)octant < (uint)Octants.Length
            ? Octants[octant]
            : throw new ArgumentOutOfRangeException(nameof(octant), octant, "An angle octant is zero through seven.");

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

    /// <summary>How a segment program's animation ends: a body link and the spit cycle, a tail cap or hidden segment holds.</summary>
    private static AnimationEnd ProjectileEnd(BotwoonSegmentProgramKind kind) => kind switch
    {
        BotwoonSegmentProgramKind.Body or BotwoonSegmentProgramKind.Spit => AnimationEnd.LoopsFrom(0),
        BotwoonSegmentProgramKind.Tail or BotwoonSegmentProgramKind.Hidden => AnimationEnd.Holds,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Undefined Botwoon segment program kind."),
    };

    /// <summary>The beats of head program <paramref name="program"/>.</summary>
    private static AnimationBeat<BotwoonHeadFrame, BotwoonHeadCue>[] HeadBeats(BotwoonHeadProgram program) => program.Kind switch
    {
        BotwoonHeadProgramKind.Hidden => HiddenHead(),
        BotwoonHeadProgramKind.Moving => MovingHead(program.Facing),
        BotwoonHeadProgramKind.Spitting => SpittingHead(program.Facing),
        _ => throw new ArgumentOutOfRangeException(nameof(program), program, "Undefined Botwoon head program."),
    };

    /// <summary>The beats of segment program <paramref name="program"/>.</summary>
    private static AnimationBeat<BotwoonProjectileFrame, NoAnimationCue>[] ProjectileBeats(BotwoonSegmentProgram program) => program.Kind switch
    {
        BotwoonSegmentProgramKind.Body => BodyLink(program.Facing),
        BotwoonSegmentProgramKind.Tail => [SegmentFrame(new(BotwoonProjectileFrameKind.Tail, program.Facing, 0), 1)],
        BotwoonSegmentProgramKind.Hidden => [SegmentFrame(new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.Up, 0), 1)],
        BotwoonSegmentProgramKind.Spit => SpitCycle(),
        _ => throw new ArgumentOutOfRangeException(nameof(program), program, "Undefined Botwoon segment program."),
    };

    /// <summary>The invisible head frame, held while Botwoon is inside a wall.</summary>
    private static AnimationBeat<BotwoonHeadFrame, BotwoonHeadCue>[] HiddenHead() => [HeadFrame(BotwoonHeadFrame.Hidden, 1)];

    /// <summary>Raises a facing-radius update before showing the closed mouth, which is held.</summary>
    private static AnimationBeat<BotwoonHeadFrame, BotwoonHeadCue>[] MovingHead(BotwoonFacing facing) =>
    [
        HeadEvent(BotwoonHeadCue.ApplyFacingRadius),
        HeadFrame(BotwoonHeadFrame.Closed(facing), 1),
    ];

    /// <summary>Holds aim, raises radius/sound/volley events, then opens the mouth with a longer leftward hold, which is held.</summary>
    private static AnimationBeat<BotwoonHeadFrame, BotwoonHeadCue>[] SpittingHead(BotwoonFacing facing) =>
    [
        HeadFrame(BotwoonHeadFrame.Closed(facing), MouthClosedBeforeSpit),
        HeadEvent(BotwoonHeadCue.ApplyFacingRadius),
        HeadEvent(BotwoonHeadCue.QueueSpitSound),
        HeadEvent(BotwoonHeadCue.ReachSpitFrame),
        // Botwoon holds its open mouth longer only when spitting left (25 updates, the rest 16).
        HeadFrame(BotwoonHeadFrame.Open(facing), facing == BotwoonFacing.Left ? MouthOpenAfterSpitLeft : MouthOpenAfterSpit),
    ];

    /// <summary>Four facing-specific body-link pictures at eight updates apiece, which cycle.</summary>
    private static AnimationBeat<BotwoonProjectileFrame, NoAnimationCue>[] BodyLink(BotwoonFacing facing) =>
    [
        .. Enumerable.Range(0, BodyPhaseCount).Select(phase =>
            SegmentFrame(new(BotwoonProjectileFrameKind.Body, facing, phase), BodyFrameDuration)),
    ];

    /// <summary>The spit's five cycling frames.</summary>
    private static AnimationBeat<BotwoonProjectileFrame, NoAnimationCue>[] SpitCycle() =>
    [
        .. Enumerable.Range(0, SpitPhaseCount).Select(phase =>
            SegmentFrame(new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.Up, phase), SpitFrameDuration)),
    ];

    /// <summary>Shows head frame <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<BotwoonHeadFrame, BotwoonHeadCue> HeadFrame(BotwoonHeadFrame frame, int duration) =>
        new AnimationBeat<BotwoonHeadFrame, BotwoonHeadCue>.Frame(frame, duration);

    /// <summary>Raises head event <paramref name="cue"/>.</summary>
    private static AnimationBeat<BotwoonHeadFrame, BotwoonHeadCue> HeadEvent(BotwoonHeadCue cue) =>
        new AnimationBeat<BotwoonHeadFrame, BotwoonHeadCue>.Event(cue);

    /// <summary>Shows projectile frame <paramref name="frame"/> for <paramref name="duration"/> passes.</summary>
    private static AnimationBeat<BotwoonProjectileFrame, NoAnimationCue> SegmentFrame(BotwoonProjectileFrame frame, int duration) =>
        new AnimationBeat<BotwoonProjectileFrame, NoAnimationCue>.Frame(frame, duration);
}
