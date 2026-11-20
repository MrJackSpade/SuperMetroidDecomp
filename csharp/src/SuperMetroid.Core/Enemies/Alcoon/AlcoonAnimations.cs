using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Alcoon.AlcoonCueKind;
using P = SuperMetroid.Core.Enemies.Alcoon.AlcoonSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>
/// The Alcoon's walking, fireball and airborne animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="AlcoonCue"/> naming what it does.
/// </summary>
internal static class AlcoonAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<AlcoonSegment, EnemyFrameId, AlcoonCue> Segments { get; } = new(
        new Dictionary<AlcoonSegment, AnimationBeat<EnemyFrameId, AlcoonCue>[]>
    {
        [P.WalkingLeft] = Walk(EnemyFrameId.AlcoonWalkingLeftNearLegStepping, EnemyFrameId.AlcoonWalkingLeftNearLegPlanted, EnemyFrameId.AlcoonWalkingLeftFarLegStepping, EnemyFrameId.AlcoonWalkingLeftFarLegPlanted),
        [P.FireLeft] = Fire(AlcoonSpitPoses.Left),
        [P.AirborneLeftLookingUp] =
        [
            Hold(EnemyFrameId.AlcoonLeapingFacingLeft),
        ],
        [P.AirborneLeftLookingForward] =
        [
            Hold(EnemyFrameId.AlcoonWalkingLeftFarLegPlanted),
        ],
        [P.WalkingRight] = Walk(EnemyFrameId.AlcoonWalkingRightNearLegStepping, EnemyFrameId.AlcoonWalkingRightNearLegPlanted, EnemyFrameId.AlcoonWalkingRightFarLegStepping, EnemyFrameId.AlcoonWalkingRightFarLegPlanted),
        [P.FireRight] = Fire(AlcoonSpitPoses.Right),
        [P.AirborneRightLookingUp] =
        [
            Hold(EnemyFrameId.AlcoonLeapingFacingRight),
        ],
        [P.AirborneRightLookingForward] =
        [
            Hold(EnemyFrameId.AlcoonWalkingRightFarLegPlanted),
        ],
    });

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at. A walk is one cycle, its move cue and
    /// its four strides, played for good (#2275). A fire animation ends at its start-walking cue, which always continues at
    /// the walk for the Alcoon's facing (#2061).
    /// </summary>
    private static Dictionary<AlcoonSegment, Animation<AlcoonSegment>> StartingAt { get; } = new Dictionary<AlcoonSegment, Animation<AlcoonSegment>>
    {
        [P.WalkingLeft] = Animation<AlcoonSegment>.Looping([Segments.Whole(P.WalkingLeft)]),
        [P.FireLeft] = new([Segments.Whole(P.FireLeft)], AnimationEnd.Holds),
        [P.AirborneLeftLookingUp] = new([Segments.Whole(P.AirborneLeftLookingUp)], AnimationEnd.Holds),
        [P.AirborneLeftLookingForward] = new([Segments.Whole(P.AirborneLeftLookingForward)], AnimationEnd.Holds),
        [P.WalkingRight] = Animation<AlcoonSegment>.Looping([Segments.Whole(P.WalkingRight)]),
        [P.FireRight] = new([Segments.Whole(P.FireRight)], AnimationEnd.Holds),
        [P.AirborneRightLookingUp] = new([Segments.Whole(P.AirborneRightLookingUp)], AnimationEnd.Holds),
        [P.AirborneRightLookingForward] = new([Segments.Whole(P.AirborneRightLookingForward)], AnimationEnd.Holds),
    };

    /// <summary>The left walk entered at its first stride, past its move cue, as a wall turning the Alcoon left enters it.</summary>
    internal static AnimationContinuation<AlcoonSegment> WalkingLeftFromFirstStride { get; } = new(StartingAt[P.WalkingLeft],
        Segments.PlaceOfFrame(StartingAt[P.WalkingLeft], P.WalkingLeft, EnemyFrameId.AlcoonWalkingLeftNearLegStepping));

    /// <summary>The right walk entered at its first stride, past its move cue, as a wall turning the Alcoon right enters it.</summary>
    internal static AnimationContinuation<AlcoonSegment> WalkingRightFromFirstStride { get; } = new(StartingAt[P.WalkingRight],
        Segments.PlaceOfFrame(StartingAt[P.WalkingRight], P.WalkingRight, EnemyFrameId.AlcoonWalkingRightNearLegStepping));

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<AlcoonSegment> StartingWith(AlcoonSegment segment) => StartingAt[segment];

    /// <summary>
    /// The Alcoon's fire facing the way <paramref name="poses"/> show (#2477): three spits, level, upward and downward, then
    /// its start-walking cue. It waits longer with its mouth closed before its first spit and holds its mouth wide longer
    /// after its last.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, AlcoonCue>[] Fire(AlcoonSpitPoses poses) =>
    [
        .. Spit(poses, 20, AlcoonFireballLaunchDefinitions.Level, 10),
        .. Spit(poses, 10, AlcoonFireballLaunchDefinitions.Upward, 10),
        .. Spit(poses, 10, AlcoonFireballLaunchDefinitions.Downward, 40),
        Cue(new AlcoonCue.Signal(K.StartWalking)),
    ];

    /// <summary>
    /// One spit (#2477): its mouth closed for <paramref name="closedUpdates"/> updates, opening, its throat glowing, opening
    /// wider, the fireball leaving by <paramref name="launch"/>, and its mouth wide for <paramref name="wideUpdates"/> updates.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, AlcoonCue>[] Spit(AlcoonSpitPoses poses, int closedUpdates, AlcoonFireballLaunch launch, int wideUpdates) =>
    [
        Show(poses.MouthClosed, closedUpdates),
        Show(poses.MouthOpening, 9),
        Show(poses.ThroatGlowing, 16),
        Show(poses.MouthOpening, 3),
        Cue(new AlcoonCue.Fire(launch)),
        Show(poses.MouthWide, wideUpdates),
    ];

    /// <summary>A walk cycle over a facing's poses, <paramref name="nearLegStepping"/>, <paramref name="nearLegPlanted"/>, <paramref name="farLegStepping"/>, <paramref name="farLegPlanted"/>, each shown ten updates and each but the last moving the Alcoon on.</summary>
    private static AnimationBeat<EnemyFrameId, AlcoonCue>[] Walk(EnemyFrameId nearLegStepping, EnemyFrameId nearLegPlanted, EnemyFrameId farLegStepping, EnemyFrameId farLegPlanted) =>
    [
        Cue(new AlcoonCue.Signal(K.Move)),
        Show(nearLegStepping, 10),
        Cue(new AlcoonCue.Signal(K.Move)),
        Show(nearLegPlanted, 10),
        Cue(new AlcoonCue.Signal(K.Move)),
        Show(farLegStepping, 10),
        Cue(new AlcoonCue.Signal(K.CompleteWalkCycle)),
        Show(farLegPlanted, 10),
    ];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, AlcoonCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, AlcoonCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good: the animation rests on it until its owner plays another (#1607).</summary>
    private static AnimationBeat<EnemyFrameId, AlcoonCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, AlcoonCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, AlcoonCue> Cue(AlcoonCue cue) => new AnimationBeat<EnemyFrameId, AlcoonCue>.Event(cue);
}
