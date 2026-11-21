using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using F = SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileFrameId;
using B = SuperMetroid.Core.Bosses.Botwoon.BotwoonBodyAnimationSegment;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's typed animations (#641): his head's, his body segments' and his spit's (#2400).</summary>
internal static class BotwoonAnimations
{
    /// <summary>Botwoon's facings in the order his segments are listed (#2567).</summary>
    private static readonly IReadOnlyList<CompassDirection> FacingOrder = Ensure.EveryMemberOnce<CompassDirection>(
    [
        CompassDirection.Up,
        CompassDirection.UpRight,
        CompassDirection.Right,
        CompassDirection.DownRight,
        CompassDirection.Down,
        CompassDirection.DownLeft,
        CompassDirection.Left,
        CompassDirection.UpLeft,
    ]);

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
        .. FacingOrder.SelectMany(facing => new[]
        {
            BotwoonHeadAnimationSegment.Moving(facing),
            BotwoonHeadAnimationSegment.Spitting(facing),
        }),
        BotwoonHeadAnimationSegment.Hidden,
    ];

    /// <summary>
    /// What a body link and a tail cap show in each facing (#2557): the link's four ripples in the order it cycles them, and
    /// the tail cap's one frame. Every facing's body segments are built from this.
    /// </summary>
    private static readonly Dictionary<CompassDirection, BotwoonBodyLook> BodyLooks = new()
    {
        [CompassDirection.Up] = new([F.BotwoonBodyUpRipple0, F.BotwoonBodyUpRipple90, F.BotwoonBodyUpRipple180, F.BotwoonBodyUpRipple270], F.BotwoonTailUp),
        [CompassDirection.UpRight] = new([F.BotwoonBodyUpRightRipple0, F.BotwoonBodyUpRightRipple90, F.BotwoonBodyUpRightRipple180, F.BotwoonBodyUpRightRipple270], F.BotwoonTailUpRight),
        [CompassDirection.Right] = new([F.BotwoonBodyRightRipple0, F.BotwoonBodyRightRipple90, F.BotwoonBodyRightRipple180, F.BotwoonBodyRightRipple270], F.BotwoonTailRight),
        [CompassDirection.DownRight] = new([F.BotwoonBodyDownRightRipple0, F.BotwoonBodyDownRightRipple90, F.BotwoonBodyDownRightRipple180, F.BotwoonBodyDownRightRipple270], F.BotwoonTailDownRight),
        [CompassDirection.Down] = new([F.BotwoonBodyDownRipple0, F.BotwoonBodyDownRipple90, F.BotwoonBodyDownRipple180, F.BotwoonBodyDownRipple270], F.BotwoonTailDown),
        [CompassDirection.DownLeft] = new([F.BotwoonBodyDownLeftRipple0, F.BotwoonBodyDownLeftRipple90, F.BotwoonBodyDownLeftRipple180, F.BotwoonBodyDownLeftRipple270], F.BotwoonTailDownLeft),
        [CompassDirection.Left] = new([F.BotwoonBodyLeftRipple0, F.BotwoonBodyLeftRipple90, F.BotwoonBodyLeftRipple180, F.BotwoonBodyLeftRipple270], F.BotwoonTailLeft),
        [CompassDirection.UpLeft] = new([F.BotwoonBodyUpLeftRipple0, F.BotwoonBodyUpLeftRipple90, F.BotwoonBodyUpLeftRipple180, F.BotwoonBodyUpLeftRipple270], F.BotwoonTailUpLeft),
    };

    /// <summary>Every body segment animation: body cycles and stationary tail caps by facing, and the hidden link or tail.</summary>
    private static readonly BotwoonBodyAnimationSegment[] BodyAnimationKeys =
    [
        .. FacingOrder.SelectMany(facing => new[] { B.Link(facing), B.TailCap(facing) }),
        B.Hidden,
    ];

    /// <summary>The head's segments: each head animation's beats.</summary>
    internal static AnimationSegments<BotwoonHeadAnimationSegment, BotwoonHeadFrame, BotwoonHeadCue> HeadSegments { get; } = new(
        HeadBeatsBySegment(), HeadAnimationKeys);

    /// <summary>The head animation that starts at each of the head's segments: it plays that segment's beats once and holds its last frame.</summary>
    internal static IReadOnlyDictionary<BotwoonHeadAnimationSegment, Animation<BotwoonHeadAnimationSegment>> HeadStartingAt { get; } =
        HeadAnimationKeys.ToDictionary(segment => segment, segment => new Animation<BotwoonHeadAnimationSegment>(
            [HeadSegments.Whole(segment)], AnimationEnd.Holds));

    /// <summary>
    /// The segments a body segment plays, each beat the installed <see cref="EnemyProjectileFrameId"/> it shows (#2036): a
    /// body link cycles its facing's frames at <see cref="BodyFrameDuration"/> apiece, and a tail cap and the hidden segment show their
    /// one frame.
    /// </summary>
    internal static HidingUnpausedAnimationSegments<BotwoonBodyAnimationSegment, EnemyProjectileFrameId, NoAnimationCue> BodySegments { get; } = new(
        FacingOrder.SelectMany(facing => new KeyValuePair<BotwoonBodyAnimationSegment, IUnpausedHidingAnimationBeat<EnemyProjectileFrameId, NoAnimationCue>[]>[]
        {
            new(B.Link(facing), [.. BodyLooks[facing].Ripples.Select(frame => Show(frame, BodyFrameDuration))]),
            new(B.TailCap(facing), [Show(BodyLooks[facing].Tail, 1)]),
        }).Append(new(B.Hidden, [new AnimationHiddenBeat<EnemyProjectileFrameId, NoAnimationCue>(1)])).ToDictionary(), BodyAnimationKeys);

    /// <summary>
    /// The body segment animation that starts at each of those segments: a body link cycles its frames for good, and a tail
    /// cap or hidden segment shows its one frame and holds it.
    /// </summary>
    internal static IReadOnlyDictionary<BotwoonBodyAnimationSegment, Animation<BotwoonBodyAnimationSegment>> BodyStartingAt { get; } =
        BodyAnimationKeys.ToDictionary(segment => segment, BodyAnimation);

    /// <summary>The spit's run, each beat the installed <see cref="EnemyProjectileFrameId"/> it shows: its frames at <see cref="SpitFrameDuration"/> apiece.</summary>
    internal static UnpausedAnimationSegments<BotwoonSpitSegment, EnemyProjectileFrameId, NoAnimationCue> SpitSegments { get; } = new(
        new Dictionary<BotwoonSpitSegment, AnimationBeat<EnemyProjectileFrameId, NoAnimationCue>.Unpaused[]>
        {
            [BotwoonSpitSegment.Flight] =
            [
                Show(F.BotwoonSpitDim, SpitFrameDuration),
                Show(F.BotwoonSpitBright, SpitFrameDuration),
                Show(F.BotwoonSpitRinged, SpitFrameDuration),
                Show(F.BotwoonSpitRingedBright, SpitFrameDuration),
                Show(F.BotwoonSpitDissolving, SpitFrameDuration),
            ],
        });

    /// <summary>The spit's animation: its frames, cycled for good.</summary>
    internal static Animation<BotwoonSpitSegment> SpitFlight { get; } =
        Animation<BotwoonSpitSegment>.Looping([SpitSegments.Whole(BotwoonSpitSegment.Flight)]);

    /// <summary>The spit's one animation, by the run it starts at.</summary>
    internal static IReadOnlyDictionary<BotwoonSpitSegment, Animation<BotwoonSpitSegment>> SpitStartingAt { get; } =
        new Dictionary<BotwoonSpitSegment, Animation<BotwoonSpitSegment>> { [BotwoonSpitSegment.Flight] = SpitFlight };

    /// <summary>Every projectile frame his body segments and his spit show.</summary>
    internal static IEnumerable<EnemyProjectileFrameId> ProjectileFrames => BodySegments.Frames.Concat(SpitSegments.Frames);

    /// <summary>The head animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<BotwoonHeadAnimationSegment> HeadStartingWith(BotwoonHeadAnimationSegment segment) => HeadStartingAt[segment];

    /// <summary>The body segment animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<BotwoonBodyAnimationSegment> BodyStartingWith(BotwoonBodyAnimationSegment segment) => BodyStartingAt[segment];

    /// <summary>The head radius of each facing.</summary>
    private static IdentityTable<CompassDirection, Extent16<RoomPixels>> HeadRadiusByFacing => field ??= new(new Dictionary<CompassDirection, Extent16<RoomPixels>>
    {
        [CompassDirection.Up] = BotwoonFightDefinitions.VerticalHeadRadius,
        [CompassDirection.Down] = BotwoonFightDefinitions.VerticalHeadRadius,
        [CompassDirection.Left] = BotwoonFightDefinitions.HorizontalHeadRadius,
        [CompassDirection.Right] = BotwoonFightDefinitions.HorizontalHeadRadius,
        [CompassDirection.UpRight] = BotwoonFightDefinitions.DiagonalHeadRadius,
        [CompassDirection.DownRight] = BotwoonFightDefinitions.DiagonalHeadRadius,
        [CompassDirection.DownLeft] = BotwoonFightDefinitions.DiagonalHeadRadius,
        [CompassDirection.UpLeft] = BotwoonFightDefinitions.DiagonalHeadRadius,
    });

    /// <summary>
    /// The head's collision half-extents facing <paramref name="facing"/>: tall vertically, wide
    /// horizontally and square on the diagonals.
    /// </summary>
    internal static Extent16<RoomPixels> HeadRadius(CompassDirection facing) => HeadRadiusByFacing[facing];

    /// <summary>The animation that starts at body segment <paramref name="segment"/>: a body link cycles, a tail cap or hidden segment holds.</summary>
    private static Animation<B> BodyAnimation(B segment) => segment.Kind == BotwoonBodyAnimationSegmentKind.Body
        ? Animation<B>.Looping([BodySegments.Whole(segment)])
        // The remaining members, BotwoonBodyAnimationSegmentKind.Tail and BotwoonBodyAnimationSegmentKind.Hidden.
        : new([BodySegments.Whole(segment)], AnimationEnd.Holds);

    /// <summary>Each head animation's beats, built from the facing each moving and spitting head is made with.</summary>
    private static Dictionary<BotwoonHeadAnimationSegment, AnimationBeat<BotwoonHeadFrame, BotwoonHeadCue>[]> HeadBeatsBySegment()
    {
        Dictionary<BotwoonHeadAnimationSegment, AnimationBeat<BotwoonHeadFrame, BotwoonHeadCue>[]> beats = new()
        {
            [BotwoonHeadAnimationSegment.Hidden] = HiddenHead(),
        };
        foreach (CompassDirection facing in FacingOrder)
        {
            beats[BotwoonHeadAnimationSegment.Moving(facing)] = MovingHead(facing);
            beats[BotwoonHeadAnimationSegment.Spitting(facing)] = SpittingHead(facing);
        }
        return beats;
    }

    /// <summary>The invisible head frame, held while Botwoon is inside a wall.</summary>
    private static AnimationBeat<BotwoonHeadFrame, BotwoonHeadCue>[] HiddenHead() => [HeadFrame(BotwoonHeadFrame.Hidden, 1)];

    /// <summary>Raises a facing-radius update before showing the closed mouth, which is held.</summary>
    private static AnimationBeat<BotwoonHeadFrame, BotwoonHeadCue>[] MovingHead(CompassDirection facing) =>
    [
        HeadEvent(new BotwoonHeadCue.ApplyFacingRadius(facing)),
        HeadFrame(BotwoonHeadFrame.Closed(facing), 1),
    ];

    /// <summary>Holds aim, raises radius/sound/volley events, then opens the mouth with a longer leftward hold, which is held.</summary>
    private static AnimationBeat<BotwoonHeadFrame, BotwoonHeadCue>[] SpittingHead(CompassDirection facing) =>
    [
        HeadFrame(BotwoonHeadFrame.Closed(facing), MouthClosedBeforeSpit),
        HeadEvent(new BotwoonHeadCue.ApplyFacingRadius(facing)),
        HeadEvent(new BotwoonHeadCue.Signal(BotwoonHeadCueKind.QueueSpitSound)),
        HeadEvent(new BotwoonHeadCue.Signal(BotwoonHeadCueKind.ReachSpitFrame)),
        // Botwoon holds its open mouth longer only when spitting left.
        HeadFrame(BotwoonHeadFrame.Open(facing), facing == CompassDirection.Left ? MouthOpenAfterSpitLeft : MouthOpenAfterSpit),
    ];

    /// <summary>Shows head frame <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<BotwoonHeadFrame, BotwoonHeadCue> HeadFrame(BotwoonHeadFrame frame, int duration) =>
        new AnimationBeat<BotwoonHeadFrame, BotwoonHeadCue>.Frame(frame, duration);

    /// <summary>Raises head event <paramref name="cue"/>.</summary>
    private static AnimationBeat<BotwoonHeadFrame, BotwoonHeadCue> HeadEvent(BotwoonHeadCue cue) =>
        new AnimationBeat<BotwoonHeadFrame, BotwoonHeadCue>.Event(cue);

    /// <summary>Shows projectile frame <paramref name="frame"/> for <paramref name="duration"/> passes.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, NoAnimationCue>.Unpaused Show(EnemyProjectileFrameId frame, int duration) =>
        new AnimationBeat<EnemyProjectileFrameId, NoAnimationCue>.Frame(frame, duration);
}
