using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using F = SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileFrameId;
using K = SuperMetroid.Core.Bosses.Botwoon.BotwoonBodyAnimationSegmentKind;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's typed animations (#641).</summary>
internal static class BotwoonAnimations
{
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
    private static readonly BotwoonHeadAnimationSegment[] HeadAnimationKeys =
    [
        .. Enum.GetValues<CompassDirection>().SelectMany(facing => new[]
        {
            new BotwoonHeadAnimationSegment(BotwoonHeadAnimationSegmentKind.Moving, facing),
            new BotwoonHeadAnimationSegment(BotwoonHeadAnimationSegmentKind.Spitting, facing),
        }),
        BotwoonHeadAnimationSegment.Hidden,
    ];

    /// <summary>Every projectile animation's segment: body cycles and stationary tail caps by facing, the hidden body/tail, and the spit.</summary>
    private static readonly BotwoonBodyAnimationSegment[] SegmentAnimationKeys =
    [
        .. Enum.GetValues<CompassDirection>().SelectMany(facing => new[]
        {
            new BotwoonBodyAnimationSegment(BotwoonBodyAnimationSegmentKind.Body, facing),
            new BotwoonBodyAnimationSegment(BotwoonBodyAnimationSegmentKind.Tail, facing),
        }),
        BotwoonBodyAnimationSegment.Hidden,
        BotwoonBodyAnimationSegment.Spit,
    ];

    /// <summary>The head's segments: each head animation's beats.</summary>
    internal static AnimationSegments<BotwoonHeadAnimationSegment, BotwoonHeadFrame, BotwoonHeadCue> HeadSegments { get; } = new(
        HeadAnimationKeys.ToDictionary(segment => segment, HeadBeats), HeadAnimationKeys);

    /// <summary>The head animation that starts at each of the head's segments: it plays that segment's beats once and holds its last frame.</summary>
    internal static IReadOnlyDictionary<BotwoonHeadAnimationSegment, Animation<BotwoonHeadAnimationSegment>> HeadStartingAt { get; } =
        HeadAnimationKeys.ToDictionary(segment => segment, segment => new Animation<BotwoonHeadAnimationSegment>(
            [new(segment, 0, HeadSegments.Beats(segment).Count, 1)], AnimationEnd.Holds));

    /// <summary>
    /// The segments a body segment or spit plays, each beat the installed <see cref="EnemyProjectileFrameId"/> it shows
    /// (#2036): a body link cycles its facing's four frames at eight updates apiece, a tail cap and the hidden segment show
    /// their one frame, and the spit cycles its five frames at three updates apiece.
    /// </summary>
    internal static AnimationSegments<BotwoonBodyAnimationSegment, EnemyProjectileFrameId, NoAnimationCue> ProjectileSegments { get; } = new(
        new Dictionary<BotwoonBodyAnimationSegment, AnimationBeat<EnemyProjectileFrameId, NoAnimationCue>[]>
        {
            [new(K.Body, CompassDirection.Up)] =
            [
                Show(F.BotwoonBodyUpFirst, BodyFrameDuration),
                Show(F.BotwoonBodyUpSecond, BodyFrameDuration),
                Show(F.BotwoonBodyUpThird, BodyFrameDuration),
                Show(F.BotwoonBodyUpFourth, BodyFrameDuration),
            ],
            [new(K.Tail, CompassDirection.Up)] = [Show(F.BotwoonTailUp, 1)],
            [new(K.Body, CompassDirection.UpRight)] =
            [
                Show(F.BotwoonBodyUpRightFirst, BodyFrameDuration),
                Show(F.BotwoonBodyUpRightSecond, BodyFrameDuration),
                Show(F.BotwoonBodyUpRightThird, BodyFrameDuration),
                Show(F.BotwoonBodyUpRightFourth, BodyFrameDuration),
            ],
            [new(K.Tail, CompassDirection.UpRight)] = [Show(F.BotwoonTailUpRight, 1)],
            [new(K.Body, CompassDirection.Right)] =
            [
                Show(F.BotwoonBodyRightFirst, BodyFrameDuration),
                Show(F.BotwoonBodyRightSecond, BodyFrameDuration),
                Show(F.BotwoonBodyRightThird, BodyFrameDuration),
                Show(F.BotwoonBodyRightFourth, BodyFrameDuration),
            ],
            [new(K.Tail, CompassDirection.Right)] = [Show(F.BotwoonTailRight, 1)],
            [new(K.Body, CompassDirection.DownRight)] =
            [
                Show(F.BotwoonBodyDownRightFirst, BodyFrameDuration),
                Show(F.BotwoonBodyDownRightSecond, BodyFrameDuration),
                Show(F.BotwoonBodyDownRightThird, BodyFrameDuration),
                Show(F.BotwoonBodyDownRightFourth, BodyFrameDuration),
            ],
            [new(K.Tail, CompassDirection.DownRight)] = [Show(F.BotwoonTailDownRight, 1)],
            [new(K.Body, CompassDirection.Down)] =
            [
                Show(F.BotwoonBodyDownFirst, BodyFrameDuration),
                Show(F.BotwoonBodyDownSecond, BodyFrameDuration),
                Show(F.BotwoonBodyDownThird, BodyFrameDuration),
                Show(F.BotwoonBodyDownFourth, BodyFrameDuration),
            ],
            [new(K.Tail, CompassDirection.Down)] = [Show(F.BotwoonTailDown, 1)],
            [new(K.Body, CompassDirection.DownLeft)] =
            [
                Show(F.BotwoonBodyDownLeftFirst, BodyFrameDuration),
                Show(F.BotwoonBodyDownLeftSecond, BodyFrameDuration),
                Show(F.BotwoonBodyDownLeftThird, BodyFrameDuration),
                Show(F.BotwoonBodyDownLeftFourth, BodyFrameDuration),
            ],
            [new(K.Tail, CompassDirection.DownLeft)] = [Show(F.BotwoonTailDownLeft, 1)],
            [new(K.Body, CompassDirection.Left)] =
            [
                Show(F.BotwoonBodyLeftFirst, BodyFrameDuration),
                Show(F.BotwoonBodyLeftSecond, BodyFrameDuration),
                Show(F.BotwoonBodyLeftThird, BodyFrameDuration),
                Show(F.BotwoonBodyLeftFourth, BodyFrameDuration),
            ],
            [new(K.Tail, CompassDirection.Left)] = [Show(F.BotwoonTailLeft, 1)],
            [new(K.Body, CompassDirection.UpLeft)] =
            [
                Show(F.BotwoonBodyUpLeftFirst, BodyFrameDuration),
                Show(F.BotwoonBodyUpLeftSecond, BodyFrameDuration),
                Show(F.BotwoonBodyUpLeftThird, BodyFrameDuration),
                Show(F.BotwoonBodyUpLeftFourth, BodyFrameDuration),
            ],
            [new(K.Tail, CompassDirection.UpLeft)] = [Show(F.BotwoonTailUpLeft, 1)],
            [BotwoonBodyAnimationSegment.Hidden] = [Show(F.BotwoonHiddenUp, 1)],
            [BotwoonBodyAnimationSegment.Spit] =
            [
                Show(F.BotwoonSpitUpFirst, SpitFrameDuration),
                Show(F.BotwoonSpitUpSecond, SpitFrameDuration),
                Show(F.BotwoonSpitUpThird, SpitFrameDuration),
                Show(F.BotwoonSpitUpFourth, SpitFrameDuration),
                Show(F.BotwoonSpitUpFifth, SpitFrameDuration),
            ],
        }, SegmentAnimationKeys);

    /// <summary>
    /// The projectile animation that starts at each of those segments: a body link and the spit cycle their frames for good,
    /// and a tail cap or hidden segment shows its one frame and holds it.
    /// </summary>
    internal static IReadOnlyDictionary<BotwoonBodyAnimationSegment, Animation<BotwoonBodyAnimationSegment>> ProjectileStartingAt { get; } =
        SegmentAnimationKeys.ToDictionary(segment => segment, segment => new Animation<BotwoonBodyAnimationSegment>(
            [new(segment, 0, ProjectileSegments.Beats(segment).Count, 1)], ProjectileEnd(segment.Kind)));

    /// <summary>The head animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<BotwoonHeadAnimationSegment> HeadStartingWith(BotwoonHeadAnimationSegment segment) => HeadStartingAt[segment];

    /// <summary>The projectile animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<BotwoonBodyAnimationSegment> ProjectileStartingWith(BotwoonBodyAnimationSegment segment) => ProjectileStartingAt[segment];

    /// <summary>
    /// The head's collision half-extents facing <paramref name="facing"/>: tall vertically, wide
    /// horizontally and square on the diagonals.
    /// </summary>
    internal static Extent16<RoomPixels> HeadRadius(CompassDirection facing) => facing switch
    {
        CompassDirection.Up or CompassDirection.Down => new(8, 16),
        CompassDirection.Left or CompassDirection.Right => new(16, 8),
        CompassDirection.UpRight or CompassDirection.DownRight or CompassDirection.DownLeft or CompassDirection.UpLeft => new(12, 12),
        _ => throw new ArgumentOutOfRangeException(nameof(facing), facing, "Undefined Botwoon facing."),
    };

    /// <summary>How a body segment's animation ends: a body link and the spit cycle, a tail cap or hidden segment holds.</summary>
    private static AnimationEnd ProjectileEnd(BotwoonBodyAnimationSegmentKind kind) => kind switch
    {
        BotwoonBodyAnimationSegmentKind.Body or BotwoonBodyAnimationSegmentKind.Spit => AnimationEnd.LoopsFrom(0),
        BotwoonBodyAnimationSegmentKind.Tail or BotwoonBodyAnimationSegmentKind.Hidden => AnimationEnd.Holds,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Undefined Botwoon segment program kind."),
    };

    /// <summary>The beats of head animation <paramref name="segment"/>.</summary>
    private static AnimationBeat<BotwoonHeadFrame, BotwoonHeadCue>[] HeadBeats(BotwoonHeadAnimationSegment segment) => segment.Kind switch
    {
        BotwoonHeadAnimationSegmentKind.Hidden => HiddenHead(),
        BotwoonHeadAnimationSegmentKind.Moving => MovingHead(segment.Facing),
        BotwoonHeadAnimationSegmentKind.Spitting => SpittingHead(segment.Facing),
        _ => throw new ArgumentOutOfRangeException(nameof(segment), segment, "Undefined Botwoon head program."),
    };

    /// <summary>The invisible head frame, held while Botwoon is inside a wall.</summary>
    private static AnimationBeat<BotwoonHeadFrame, BotwoonHeadCue>[] HiddenHead() => [HeadFrame(BotwoonHeadFrame.Hidden, 1)];

    /// <summary>Raises a facing-radius update before showing the closed mouth, which is held.</summary>
    private static AnimationBeat<BotwoonHeadFrame, BotwoonHeadCue>[] MovingHead(CompassDirection facing) =>
    [
        HeadEvent(BotwoonHeadCue.ApplyFacingRadius),
        HeadFrame(BotwoonHeadFrame.Closed(facing), 1),
    ];

    /// <summary>Holds aim, raises radius/sound/volley events, then opens the mouth with a longer leftward hold, which is held.</summary>
    private static AnimationBeat<BotwoonHeadFrame, BotwoonHeadCue>[] SpittingHead(CompassDirection facing) =>
    [
        HeadFrame(BotwoonHeadFrame.Closed(facing), MouthClosedBeforeSpit),
        HeadEvent(BotwoonHeadCue.ApplyFacingRadius),
        HeadEvent(BotwoonHeadCue.QueueSpitSound),
        HeadEvent(BotwoonHeadCue.ReachSpitFrame),
        // Botwoon holds its open mouth longer only when spitting left (25 updates, the rest 16).
        HeadFrame(BotwoonHeadFrame.Open(facing), facing == CompassDirection.Left ? MouthOpenAfterSpitLeft : MouthOpenAfterSpit),
    ];

    /// <summary>Shows head frame <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<BotwoonHeadFrame, BotwoonHeadCue> HeadFrame(BotwoonHeadFrame frame, int duration) =>
        new AnimationBeat<BotwoonHeadFrame, BotwoonHeadCue>.Frame(frame, duration);

    /// <summary>Raises head event <paramref name="cue"/>.</summary>
    private static AnimationBeat<BotwoonHeadFrame, BotwoonHeadCue> HeadEvent(BotwoonHeadCue cue) =>
        new AnimationBeat<BotwoonHeadFrame, BotwoonHeadCue>.Event(cue);

    /// <summary>Shows projectile frame <paramref name="frame"/> for <paramref name="duration"/> passes.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, NoAnimationCue> Show(EnemyProjectileFrameId frame, int duration) =>
        new AnimationBeat<EnemyProjectileFrameId, NoAnimationCue>.Frame(frame, duration);
}
