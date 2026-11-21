using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.WorkRobots.WorkRobotCueKind;
using P = SuperMetroid.Core.Enemies.WorkRobots.WorkRobotSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// The Work Robot's unpowered poses and its walk, wall, recoil, laser and ledge animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="WorkRobotCue"/> naming what it does.
/// </summary>
internal static class WorkRobotAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<WorkRobotSegment, EnemyFrameId, WorkRobotCue> Segments { get; } = new(
        new Dictionary<WorkRobotSegment, AnimationBeat<EnemyFrameId, WorkRobotCue>[]>
    {
        [P.NoPowerNeutral] =
        [
            Hold(EnemyFrameId.WorkRobotUnpoweredUpright),
        ],
        [P.NoPowerLeaningLeft] =
        [
            Hold(EnemyFrameId.WorkRobotUnpoweredTiltedLeft),
        ],
        [P.NoPowerLeaningRight] =
        [
            Hold(EnemyFrameId.WorkRobotUnpoweredTiltedRight),
        ],
        [P.StandingFacingLeft] =
        [
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootEasingUp, 32),
        ],
        [P.FacingLeftWalkingForwards] =
        [
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootEasingUp, 1),
        ],
        [P.FacingLeftWalkingStride] =
        [
            .. WalkingStride(WorkRobotFoot.LeftNear, WorkRobotFoot.LeftFar, WorkRobotMoveStepDefinitions.LeftForwardWatchingLedges,
                P.FacingLeftShotLaserLeft, P.FacingLeftShotLaserUpLeft),
            .. PoseTryingLaser(WorkRobotFoot.LeftNear.LeanBack, WorkRobotLaser.Downward, P.FacingLeftShotLaserDownLeft),
        ],
        [P.FacingLeftHitWallMovingForwards] =
        [
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootSettling, 11),
            Footstep,
            Step(WorkRobotMoveStepDefinitions.LeftBackwardWatchingLedges),
            .. StrideBack(WorkRobotFoot.LeftNear, WorkRobotFoot.LeftFar, 10, 10, WorkRobotMoveStepDefinitions.LeftBackwardWatchingLedges),
            .. SteppingOntoFoot(WorkRobotFoot.LeftFar, 10, WorkRobotMoveStepDefinitions.LeftBackwardWatchingLedges),
            .. StrideBack(WorkRobotFoot.LeftNear, WorkRobotFoot.LeftFar, 10, 10, WorkRobotMoveStepDefinitions.LeftBackwardWatchingLedges),
        ],
        [P.FacingLeftShotSamusAhead] =
        [
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootSettling, 5),
            Footstep,
            Step(WorkRobotMoveStepDefinitions.LeftBackward),
            .. StrideBack(WorkRobotFoot.LeftNear, WorkRobotFoot.LeftFar, 5, 5, WorkRobotMoveStepDefinitions.LeftBackward),
            .. SteppingOntoFoot(WorkRobotFoot.LeftFar, 5, WorkRobotMoveStepDefinitions.LeftBackward),
            .. StrideBack(WorkRobotFoot.LeftNear, WorkRobotFoot.LeftFar, 5, 5, WorkRobotMoveStepDefinitions.LeftBackward),
        ],
        [P.FacingLeftShotSamusBehind] =
        [
            Footstep,
            Step(WorkRobotMoveStepDefinitions.LeftForward),
            .. StrideForward(WorkRobotFoot.LeftNear, WorkRobotFoot.LeftFar, 5, WorkRobotMoveStepDefinitions.LeftForward),
            .. StrideForward(WorkRobotFoot.LeftNear, WorkRobotFoot.LeftFar, 5, WorkRobotMoveStepDefinitions.LeftForward),
        ],
        [P.FacingLeftShotLaserDownLeft] = DownwardShot(WorkRobotFoot.LeftNear, WorkRobotFoot.LeftFar),
        [P.FacingLeftShotLaserLeft] =
        [
            .. StraightShotPoses(WorkRobotFoot.LeftNear),
            .. SteppedOnto(WorkRobotFoot.LeftFar, 10, WorkRobotMoveStepDefinitions.LeftBackwardWatchingLedges),
        ],
        [P.FacingLeftShotLaserUpLeft] =
        [
            .. UpwardShotPoses(WorkRobotFoot.LeftNear),
            .. SteppedOnto(WorkRobotFoot.LeftFar, 4, WorkRobotMoveStepDefinitions.LeftBackwardWatchingLedges),
        ],
        [P.FacingLeftLaserShotRecoil] =
        [
            .. Recoil(WorkRobotFoot.LeftNear, WorkRobotFoot.LeftFar, WorkRobotMoveStepDefinitions.LeftBackwardWatchingLedges),
            CountDownLaserCooldown,
        ],
        [P.ApproachingFallRight] = ApproachingFall(WorkRobotFoot.RightNear),
        [P.FacingRightWalkingForwards] =
        [
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootLeanBack, 1),
        ],
        [P.FacingRightWalkingStride] =
        [
            .. PoseTryingLaser(WorkRobotFoot.RightNear.LeanBack, WorkRobotLaser.Downward, P.FacingRightShotLaserDownRight),
            CountDownLaserCooldown,
            .. WalkingStride(WorkRobotFoot.RightNear, WorkRobotFoot.RightFar, WorkRobotMoveStepDefinitions.RightForwardWatchingLedges,
                P.FacingRightShotLaserRight, P.FacingRightShotLaserUpRight),
        ],
        [P.FacingRightHitWallMovingForwards] =
        [
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootSettling, 11),
            Footstep,
            Step(WorkRobotMoveStepDefinitions.RightBackwardWatchingLedges),
            .. StrideBack(WorkRobotFoot.RightNear, WorkRobotFoot.RightFar, 10, 10, WorkRobotMoveStepDefinitions.RightBackwardWatchingLedges),
            .. FootLanding(WorkRobotFoot.RightFar, 10, WorkRobotMoveStepDefinitions.RightBackwardWatchingLedges),
            .. StrideBack(WorkRobotFoot.RightNear, WorkRobotFoot.RightFar, 10, 10, WorkRobotMoveStepDefinitions.RightBackwardWatchingLedges),
        ],
        [P.FacingRightShotSamusAhead] =
        [
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootSettling, 6),
            Footstep,
            Step(WorkRobotMoveStepDefinitions.RightBackward),
            .. StrideBack(WorkRobotFoot.RightNear, WorkRobotFoot.RightFar, 5, 5, WorkRobotMoveStepDefinitions.RightBackward),
            .. SteppingOntoFoot(WorkRobotFoot.RightFar, 5, WorkRobotMoveStepDefinitions.RightBackward),
            .. StrideBack(WorkRobotFoot.RightNear, WorkRobotFoot.RightFar, 5, 10, WorkRobotMoveStepDefinitions.RightBackward),
        ],
        [P.FacingRightShotSamusBehind] =
        [
            Step(WorkRobotMoveStepDefinitions.RightForward),
            Footstep,
            .. Swing(WorkRobotFoot.RightNear, 5),
            .. FootLanding(WorkRobotFoot.RightNear, 5, WorkRobotMoveStepDefinitions.RightForward),
            Footstep,
            .. Swing(WorkRobotFoot.RightFar, 5),
            .. SteppingOntoFoot(WorkRobotFoot.RightFar, 5, WorkRobotMoveStepDefinitions.RightForward),
            .. Swing(WorkRobotFoot.RightNear, 5),
            .. FootLanding(WorkRobotFoot.RightNear, 5, WorkRobotMoveStepDefinitions.RightForward),
            .. Swing(WorkRobotFoot.RightFar, 5),
            .. SteppingOntoFoot(WorkRobotFoot.RightFar, 5, WorkRobotMoveStepDefinitions.RightForward),
        ],
        [P.FacingRightShotLaserDownRight] = DownwardShot(WorkRobotFoot.RightNear, WorkRobotFoot.RightFar),
        [P.FacingRightShotLaserRight] =
        [
            .. StraightShotPoses(WorkRobotFoot.RightNear),
            .. Landed(WorkRobotFoot.RightFar, 10, WorkRobotMoveStepDefinitions.RightBackwardWatchingLedges),
        ],
        [P.FacingRightShotLaserUpRight] =
        [
            .. UpwardShotPoses(WorkRobotFoot.RightNear),
            .. Landed(WorkRobotFoot.RightFar, 4, WorkRobotMoveStepDefinitions.RightBackwardWatchingLedges),
        ],
        [P.FacingRightLaserShotRecoil] = Recoil(WorkRobotFoot.RightNear, WorkRobotFoot.RightFar, WorkRobotMoveStepDefinitions.RightBackwardWatchingLedges),
        [P.ApproachingFallLeft] = ApproachingFall(WorkRobotFoot.LeftNear),
    });

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at. The wall, shot-ahead and shot-behind
    /// animations walk on when they end (#2167).
    /// </summary>
    internal static Dictionary<WorkRobotSegment, Animation<WorkRobotSegment>> StartingAt { get; } = new Dictionary<WorkRobotSegment, Animation<WorkRobotSegment>>
    {
        [P.NoPowerNeutral] = new([Segments.Whole(P.NoPowerNeutral)], AnimationEnd.Holds),
        [P.NoPowerLeaningLeft] = new([Segments.Whole(P.NoPowerLeaningLeft)], AnimationEnd.Holds),
        [P.NoPowerLeaningRight] = new([Segments.Whole(P.NoPowerLeaningRight)], AnimationEnd.Holds),
        [P.StandingFacingLeft] = Walk([P.StandingFacingLeft], P.FacingLeftWalkingForwards, P.FacingLeftWalkingStride),
        [P.FacingLeftWalkingForwards] = Walk([], P.FacingLeftWalkingForwards, P.FacingLeftWalkingStride),
        [P.FacingLeftHitWallMovingForwards] = Walk([P.FacingLeftHitWallMovingForwards], P.FacingRightWalkingForwards, P.FacingRightWalkingStride),
        [P.FacingLeftShotSamusAhead] = Walk([P.FacingLeftShotSamusAhead], P.FacingRightWalkingForwards, P.FacingRightWalkingStride),
        [P.FacingLeftShotSamusBehind] = Walk([P.FacingLeftShotSamusBehind], P.FacingLeftWalkingForwards, P.FacingLeftWalkingStride),
        [P.FacingLeftShotLaserDownLeft] = Walk([P.FacingLeftShotLaserDownLeft, P.FacingLeftLaserShotRecoil], P.FacingLeftWalkingForwards, P.FacingLeftWalkingStride),
        [P.FacingLeftShotLaserLeft] = Walk([P.FacingLeftShotLaserLeft, P.FacingLeftLaserShotRecoil], P.FacingLeftWalkingForwards, P.FacingLeftWalkingStride),
        [P.FacingLeftShotLaserUpLeft] = Walk([P.FacingLeftShotLaserUpLeft, P.FacingLeftLaserShotRecoil], P.FacingLeftWalkingForwards, P.FacingLeftWalkingStride),
        [P.ApproachingFallRight] = Walk([P.ApproachingFallRight], P.FacingLeftWalkingForwards, P.FacingLeftWalkingStride),
        [P.FacingRightWalkingForwards] = Walk([], P.FacingRightWalkingForwards, P.FacingRightWalkingStride),
        [P.FacingRightHitWallMovingForwards] = Walk([P.FacingRightHitWallMovingForwards], P.FacingLeftWalkingForwards, P.FacingLeftWalkingStride),
        [P.FacingRightShotSamusAhead] = Walk([P.FacingRightShotSamusAhead], P.FacingLeftWalkingForwards, P.FacingLeftWalkingStride),
        [P.FacingRightShotSamusBehind] = Walk([P.FacingRightShotSamusBehind], P.FacingRightWalkingForwards, P.FacingRightWalkingStride),
        [P.FacingRightShotLaserDownRight] = Walk([P.FacingRightShotLaserDownRight, P.FacingRightLaserShotRecoil], P.FacingRightWalkingForwards, P.FacingRightWalkingStride),
        [P.FacingRightShotLaserRight] = Walk([P.FacingRightShotLaserRight, P.FacingRightLaserShotRecoil], P.FacingRightWalkingForwards, P.FacingRightWalkingStride),
        [P.FacingRightShotLaserUpRight] = Walk([P.FacingRightShotLaserUpRight, P.FacingRightLaserShotRecoil], P.FacingRightWalkingForwards, P.FacingRightWalkingStride),
        [P.ApproachingFallLeft] = Walk([P.ApproachingFallLeft], P.FacingRightWalkingForwards, P.FacingRightWalkingStride),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<WorkRobotSegment> StartingWith(WorkRobotSegment segment) => StartingAt[segment];

    /// <summary>
    /// Plays each of <paramref name="before"/> whole, then a walk (#2276, #2414): its first step <paramref name="first"/> once
    /// and its stride <paramref name="stride"/> for good.
    /// </summary>
    private static Animation<P> Walk(IReadOnlyList<P> before, P first, P stride) =>
        Animation<P>.LeadingInto([.. before.Select(Segments.Whole), Segments.Whole(first)], [Segments.Whole(stride)]);

    /// <summary>A footstep's sound.</summary>
    private static AnimationBeat<EnemyFrameId, WorkRobotCue> Footstep => Cue(new WorkRobotCue.Signal(K.Footstep));

    /// <summary>A walk step of <paramref name="move"/>.</summary>
    private static AnimationBeat<EnemyFrameId, WorkRobotCue> Step(WorkRobotMoveStep move) => Cue(new WorkRobotCue.Step(move));

    /// <summary>The count down of the laser's cooldown.</summary>
    private static AnimationBeat<EnemyFrameId, WorkRobotCue> CountDownLaserCooldown => Cue(new WorkRobotCue.Signal(K.CountDownLaserCooldown));

    /// <summary>The updates a walking pose that tries a laser shows before it tries it (#2512).</summary>
    private const int LaserTryAfterUpdates = 1;

    /// <summary>The updates a walking pose that tries a laser shows after it tries it, the rest of the stride's pose (#2512).</summary>
    private const int AfterLaserTryUpdates = 9;

    /// <summary>
    /// <paramref name="pose"/>, a stride's pose, trying to fire <paramref name="laser"/> after its first update (#2512): a
    /// laser fired continues at <paramref name="firing"/>.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, WorkRobotCue>[] PoseTryingLaser(EnemyFrameId pose, WorkRobotLaser laser, P firing) =>
    [
        Show(pose, LaserTryAfterUpdates),
        Cue(new WorkRobotCue.TryLaser(laser, firing)),
        Show(pose, AfterLaserTryUpdates),
    ];

    /// <summary>
    /// A walking stride (#2512), each pose for ten updates and each step <paramref name="move"/>: <paramref name="near"/>
    /// easing up and swinging forward, trying its straight laser upright and its upward one easing forward, which continue at
    /// <paramref name="straightShot"/> and <paramref name="upwardShot"/>; the near foot landing; <paramref name="far"/>
    /// swinging forward; and the robot stepping onto the far foot.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, WorkRobotCue>[] WalkingStride(
        WorkRobotFoot near, WorkRobotFoot far, WorkRobotMoveStep move, P straightShot, P upwardShot) =>
    [
        Show(near.EasingUp, 10),
        .. PoseTryingLaser(near.Upright, WorkRobotLaser.Horizontal, straightShot),
        .. PoseTryingLaser(near.EasingForward, WorkRobotLaser.Upward, upwardShot),
        Show(near.LeanForward, 10),
        .. FootLanding(near, 10, move),
        .. Swing(far, 10),
        .. SteppingOntoFoot(far, 10, move),
    ];

    /// <summary>The downward laser's firing (#2512): <paramref name="near"/> leaning back, then <paramref name="far"/> settling.</summary>
    private static AnimationBeat<EnemyFrameId, WorkRobotCue>[] DownwardShot(WorkRobotFoot near, WorkRobotFoot far) =>
        [Show(near.LeanBack, 5), Show(far.Settling, 2)];

    /// <summary>The straight laser's firing poses (#2512): <paramref name="near"/> easing up, then leaning back.</summary>
    private static AnimationBeat<EnemyFrameId, WorkRobotCue>[] StraightShotPoses(WorkRobotFoot near) =>
        [Show(near.EasingUp, 5), Show(near.LeanBack, 2)];

    /// <summary>The upward laser's firing poses (#2512): <paramref name="near"/> swinging back from easing forward to leaning back.</summary>
    private static AnimationBeat<EnemyFrameId, WorkRobotCue>[] UpwardShotPoses(WorkRobotFoot near) =>
        [Show(near.EasingForward, 5), Show(near.Upright, 2), Show(near.EasingUp, 2), Show(near.LeanBack, 4)];

    /// <summary>
    /// The recoil after a laser shot (#2512): a step of <paramref name="move"/>, <paramref name="far"/> swinging back from a
    /// long lean forward, <paramref name="near"/> landing and swinging up to stand upright a while.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, WorkRobotCue>[] Recoil(WorkRobotFoot near, WorkRobotFoot far, WorkRobotMoveStep move) =>
    [
        Step(move),
        Show(far.LeanForward, 16),
        Show(far.EasingForward, 5),
        Show(far.Upright, 5),
        Show(far.EasingUp, 5),
        Show(far.LeanBack, 5),
        .. FootLanding(near, 10, move),
        Show(near.LeanForward, 10),
        Show(near.EasingForward, 10),
        Show(near.Upright, 96),
    ];

    /// <summary>
    /// The wait before a ledge the robot would fall from (#2512): <paramref name="near"/> leaning back a long while, easing up
    /// and standing upright, then the laser's cooldown counting down.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, WorkRobotCue>[] ApproachingFall(WorkRobotFoot near) =>
        [Show(near.LeanBack, 128), Show(near.EasingUp, 10), Show(near.Upright, 10), CountDownLaserCooldown];

    /// <summary><paramref name="foot"/> swinging forward, from leaning back to leaning forward, each pose for <paramref name="updates"/> updates (#2451).</summary>
    private static AnimationBeat<EnemyFrameId, WorkRobotCue>[] Swing(WorkRobotFoot foot, int updates) =>
    [
        Show(foot.LeanBack, updates),
        Show(foot.EasingUp, updates),
        Show(foot.Upright, updates),
        Show(foot.EasingForward, updates),
        Show(foot.LeanForward, updates),
    ];

    /// <summary><paramref name="foot"/> swinging back, from leaning forward to leaning back, each pose for <paramref name="updates"/> updates (#2451).</summary>
    private static AnimationBeat<EnemyFrameId, WorkRobotCue>[] SwingBack(WorkRobotFoot foot, int updates) =>
    [
        Show(foot.LeanForward, updates),
        Show(foot.EasingForward, updates),
        Show(foot.Upright, updates),
        Show(foot.EasingUp, updates),
        Show(foot.LeanBack, updates),
    ];

    /// <summary>
    /// <paramref name="foot"/> landing (#2451): its footstep, a step of <paramref name="move"/>, the foot settling for
    /// <paramref name="updates"/> updates, then another step.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, WorkRobotCue>[] FootLanding(WorkRobotFoot foot, int updates, WorkRobotMoveStep move) =>
        [.. Landed(foot, updates, move), Step(move)];

    /// <summary>
    /// <paramref name="foot"/> come down (#2512): its footstep, a step of <paramref name="move"/>, and the foot settling for
    /// <paramref name="updates"/> updates.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, WorkRobotCue>[] Landed(WorkRobotFoot foot, int updates, WorkRobotMoveStep move) =>
        [Footstep, Step(move), Show(foot.Settling, updates)];

    /// <summary>
    /// The robot stepping onto <paramref name="foot"/> (#2451): a step of <paramref name="move"/>, the foot's footstep, the
    /// foot settling for <paramref name="updates"/> updates, then another step.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, WorkRobotCue>[] SteppingOntoFoot(WorkRobotFoot foot, int updates, WorkRobotMoveStep move) =>
        [.. SteppedOnto(foot, updates, move), Step(move)];

    /// <summary>
    /// The robot stepped onto <paramref name="foot"/> (#2512): a step of <paramref name="move"/>, the foot's footstep, and
    /// the foot settling for <paramref name="updates"/> updates.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, WorkRobotCue>[] SteppedOnto(WorkRobotFoot foot, int updates, WorkRobotMoveStep move) =>
        [Step(move), Footstep, Show(foot.Settling, updates)];

    /// <summary>
    /// A stride back (#2451): <paramref name="far"/> swinging back, <paramref name="near"/> landing, settling for
    /// <paramref name="nearSettlingUpdates"/> updates, and swinging back; each swinging pose shows for
    /// <paramref name="updates"/> updates and each step is <paramref name="move"/>.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, WorkRobotCue>[] StrideBack(
        WorkRobotFoot near, WorkRobotFoot far, int updates, int nearSettlingUpdates, WorkRobotMoveStep move) =>
        [.. SwingBack(far, updates), .. FootLanding(near, nearSettlingUpdates, move), .. SwingBack(near, updates)];

    /// <summary>
    /// A stride forward (#2451), from <paramref name="near"/> easing up to it leaning back again: the near foot swinging
    /// forward and landing, then the robot stepping onto <paramref name="far"/> once it has swung; every pose shows for
    /// <paramref name="updates"/> updates and each step is <paramref name="move"/>.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, WorkRobotCue>[] StrideForward(WorkRobotFoot near, WorkRobotFoot far, int updates, WorkRobotMoveStep move) =>
    [
        Show(near.EasingUp, updates),
        Show(near.Upright, updates),
        Show(near.EasingForward, updates),
        Show(near.LeanForward, updates),
        .. FootLanding(near, updates, move),
        .. Swing(far, updates),
        .. SteppingOntoFoot(far, updates, move),
        Show(near.LeanBack, updates),
    ];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, WorkRobotCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, WorkRobotCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good: the animation rests on it until its owner plays another (#1607).</summary>
    private static AnimationBeat<EnemyFrameId, WorkRobotCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, WorkRobotCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, WorkRobotCue> Cue(WorkRobotCue cue) => new AnimationBeat<EnemyFrameId, WorkRobotCue>.Event(cue);
}
