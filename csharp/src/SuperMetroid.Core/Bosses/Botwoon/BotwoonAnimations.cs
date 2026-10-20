using SuperMetroid.Core.Programs;
using HeadStep = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Bosses.Botwoon.BotwoonHeadProgram, SuperMetroid.Core.Bosses.Botwoon.BotwoonHeadFrame, SuperMetroid.Core.Bosses.Botwoon.BotwoonHeadCue>;
using SegmentStep = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Bosses.Botwoon.BotwoonSegmentProgram, SuperMetroid.Core.Bosses.Botwoon.BotwoonProjectileFrame, SuperMetroid.Core.Programs.NoAnimationCue>;

namespace SuperMetroid.Core.Bosses.Botwoon;

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

    /// <summary>Every head program: moving and spitting by facing, plus the hidden head.</summary>
    internal static AnimationProgramSet<BotwoonHeadProgram, BotwoonHeadFrame, BotwoonHeadCue> HeadPrograms { get; } = new(
        Enum.GetValues<BotwoonFacing>().SelectMany(facing => new[]
        {
            KeyValuePair.Create(new BotwoonHeadProgram(BotwoonHeadProgramKind.Moving, facing), MovingHead(facing)),
            KeyValuePair.Create(new BotwoonHeadProgram(BotwoonHeadProgramKind.Spitting, facing), SpittingHead(facing)),
        }).Append(KeyValuePair.Create(BotwoonHeadProgram.Hidden, HiddenHead())).ToDictionary());

    /// <summary>Every projectile program: body cycles and stationary tail caps by facing, the hidden body/tail, and the spit.</summary>
    internal static AnimationProgramSet<BotwoonSegmentProgram, BotwoonProjectileFrame, NoAnimationCue> SegmentPrograms { get; } = new(
        Enum.GetValues<BotwoonFacing>().SelectMany(facing => new[]
        {
            KeyValuePair.Create(new BotwoonSegmentProgram(BotwoonSegmentProgramKind.Body, facing), BodyLink(facing)),
            KeyValuePair.Create(new BotwoonSegmentProgram(BotwoonSegmentProgramKind.Tail, facing),
                StillSegment(new(BotwoonProjectileFrameKind.Tail, facing, 0))),
        }).Append(KeyValuePair.Create(BotwoonSegmentProgram.Hidden, StillSegment(new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.Up, 0))))
            .Append(KeyValuePair.Create(BotwoonSegmentProgram.Spit, SpitCycle())).ToDictionary());

    /// <summary>The facing of cartridge angle octant <paramref name="octant"/>, zero (up) through seven, clockwise.</summary>
    internal static BotwoonFacing FacingOfOctant(int octant) =>
        (uint)octant < (uint)Octants.Length
            ? Octants[octant]
            : throw new ArgumentOutOfRangeException(nameof(octant), octant, "A cartridge angle octant is zero through seven.");

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

    /// <summary>The invisible head frame followed by a permanent hold while Botwoon is inside a wall.</summary>
    private static HeadStep[] HiddenHead() => [HeadStep.Show(BotwoonHeadFrame.Hidden, 1), HeadStep.Hold];

    /// <summary>Publishes a facing-radius update before showing and holding the closed mouth.</summary>
    private static HeadStep[] MovingHead(BotwoonFacing facing) =>
    [
        HeadStep.Publish(BotwoonHeadCue.ApplyFacingRadius),
        HeadStep.Show(BotwoonHeadFrame.Closed(facing), 1),
        HeadStep.Hold,
    ];

    /// <summary>Holds aim, publishes radius/sound/volley cues, then opens the mouth with a longer leftward hold.</summary>
    private static HeadStep[] SpittingHead(BotwoonFacing facing) =>
    [
        HeadStep.Show(BotwoonHeadFrame.Closed(facing), MouthClosedBeforeSpit),
        HeadStep.Publish(BotwoonHeadCue.ApplyFacingRadius),
        HeadStep.Publish(BotwoonHeadCue.QueueSpitSound),
        HeadStep.Publish(BotwoonHeadCue.ReachSpitFrame),
        // The cartridge holds the open mouth longer only when spitting left ($B3:93B9 is $19, the rest $10).
        HeadStep.Show(BotwoonHeadFrame.Open(facing), facing == BotwoonFacing.Left ? MouthOpenAfterSpitLeft : MouthOpenAfterSpit),
        HeadStep.Hold,
    ];

    /// <summary>Cycles four facing-specific body-link pictures at eight updates apiece.</summary>
    private static SegmentStep[] BodyLink(BotwoonFacing facing) =>
    [
        .. Enumerable.Range(0, BodyPhaseCount).Select(phase =>
            SegmentStep.Show(new(BotwoonProjectileFrameKind.Body, facing, phase), BodyFrameDuration)),
        SegmentStep.GoTo(new(BotwoonSegmentProgramKind.Body, facing)),
    ];

    /// <summary>Shows one tail or hidden frame and holds it without advancing.</summary>
    private static SegmentStep[] StillSegment(BotwoonProjectileFrame frame) => [SegmentStep.Show(frame, 1), SegmentStep.Hold];

    /// <summary>The spit's five cycling frames.</summary>
    private static SegmentStep[] SpitCycle() =>
    [
        .. Enumerable.Range(0, SpitPhaseCount).Select(phase =>
            SegmentStep.Show(new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.Up, phase), SpitFrameDuration)),
        SegmentStep.GoTo(BotwoonSegmentProgram.Spit),
    ];
}
