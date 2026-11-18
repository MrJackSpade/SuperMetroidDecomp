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
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootEasingUp, 10),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootUpright, 1),
            Cue(new WorkRobotCue.TryLaser(WorkRobotLaser.Horizontal, WorkRobotSegment.FacingLeftShotLaserLeft)),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootUpright, 9),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootEasingForward, 1),
            Cue(new WorkRobotCue.TryLaser(WorkRobotLaser.Upward, WorkRobotSegment.FacingLeftShotLaserUpLeft)),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootEasingForward, 9),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootLeanForward, 10),
            Cue(new WorkRobotCue.Signal(K.Footstep)),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.LeftForwardWatchingLedges)),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootSettling, 10),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.LeftForwardWatchingLedges)),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootLeanBack, 10),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootEasingUp, 10),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootUpright, 10),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootEasingForward, 10),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootLeanForward, 10),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.LeftForwardWatchingLedges)),
            Cue(new WorkRobotCue.Signal(K.Footstep)),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootSettling, 10),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.LeftForwardWatchingLedges)),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootLeanBack, 1),
            Cue(new WorkRobotCue.TryLaser(WorkRobotLaser.Downward, WorkRobotSegment.FacingLeftShotLaserDownLeft)),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootLeanBack, 9),
        ],
        [P.FacingLeftHitWallMovingForwards] =
        [
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootSettling, 11),
            Cue(new WorkRobotCue.Signal(K.Footstep)),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.LeftBackwardWatchingLedges)),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootLeanForward, 10),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootEasingForward, 10),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootUpright, 10),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootEasingUp, 10),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootLeanBack, 10),
            Cue(new WorkRobotCue.Signal(K.Footstep)),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.LeftBackwardWatchingLedges)),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootSettling, 10),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.LeftBackwardWatchingLedges)),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootLeanForward, 10),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootEasingForward, 10),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootUpright, 10),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootEasingUp, 10),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootLeanBack, 10),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.LeftBackwardWatchingLedges)),
            Cue(new WorkRobotCue.Signal(K.Footstep)),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootSettling, 10),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.LeftBackwardWatchingLedges)),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootLeanForward, 10),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootEasingForward, 10),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootUpright, 10),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootEasingUp, 10),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootLeanBack, 10),
            Cue(new WorkRobotCue.Signal(K.Footstep)),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.LeftBackwardWatchingLedges)),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootSettling, 10),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.LeftBackwardWatchingLedges)),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootLeanForward, 10),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootEasingForward, 10),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootUpright, 10),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootEasingUp, 10),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootLeanBack, 10),
        ],
        [P.FacingLeftShotSamusAhead] =
        [
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootSettling, 5),
            Cue(new WorkRobotCue.Signal(K.Footstep)),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.LeftBackward)),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootLeanForward, 5),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootEasingForward, 5),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootUpright, 5),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootEasingUp, 5),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootLeanBack, 5),
            Cue(new WorkRobotCue.Signal(K.Footstep)),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.LeftBackward)),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootSettling, 5),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.LeftBackward)),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootLeanForward, 5),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootEasingForward, 5),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootUpright, 5),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootEasingUp, 5),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootLeanBack, 5),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.LeftBackward)),
            Cue(new WorkRobotCue.Signal(K.Footstep)),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootSettling, 5),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.LeftBackward)),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootLeanForward, 5),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootEasingForward, 5),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootUpright, 5),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootEasingUp, 5),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootLeanBack, 5),
            Cue(new WorkRobotCue.Signal(K.Footstep)),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.LeftBackward)),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootSettling, 5),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.LeftBackward)),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootLeanForward, 5),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootEasingForward, 5),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootUpright, 5),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootEasingUp, 5),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootLeanBack, 5),
        ],
        [P.FacingLeftShotSamusBehind] =
        [
            Cue(new WorkRobotCue.Signal(K.Footstep)),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.LeftForward)),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootEasingUp, 5),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootUpright, 5),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootEasingForward, 5),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootLeanForward, 5),
            Cue(new WorkRobotCue.Signal(K.Footstep)),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.LeftForward)),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootSettling, 5),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.LeftForward)),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootLeanBack, 5),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootEasingUp, 5),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootUpright, 5),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootEasingForward, 5),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootLeanForward, 5),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.LeftForward)),
            Cue(new WorkRobotCue.Signal(K.Footstep)),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootSettling, 5),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.LeftForward)),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootLeanBack, 5),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootEasingUp, 5),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootUpright, 5),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootEasingForward, 5),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootLeanForward, 5),
            Cue(new WorkRobotCue.Signal(K.Footstep)),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.LeftForward)),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootSettling, 5),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.LeftForward)),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootLeanBack, 5),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootEasingUp, 5),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootUpright, 5),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootEasingForward, 5),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootLeanForward, 5),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.LeftForward)),
            Cue(new WorkRobotCue.Signal(K.Footstep)),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootSettling, 5),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.LeftForward)),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootLeanBack, 5),
        ],
        [P.FacingLeftShotLaserDownLeft] =
        [
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootLeanBack, 5),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootSettling, 2),
        ],
        [P.FacingLeftShotLaserLeft] =
        [
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootEasingUp, 5),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootLeanBack, 2),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.LeftBackwardWatchingLedges)),
            Cue(new WorkRobotCue.Signal(K.Footstep)),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootSettling, 10),
        ],
        [P.FacingLeftShotLaserUpLeft] =
        [
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootEasingForward, 5),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootUpright, 2),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootEasingUp, 2),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootLeanBack, 4),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.LeftBackwardWatchingLedges)),
            Cue(new WorkRobotCue.Signal(K.Footstep)),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootSettling, 4),
        ],
        [P.FacingLeftLaserShotRecoil] =
        [
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.LeftBackwardWatchingLedges)),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootLeanForward, 16),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootEasingForward, 5),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootUpright, 5),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootEasingUp, 5),
            Show(EnemyFrameId.WorkRobotWalkingLeftFarFootLeanBack, 5),
            Cue(new WorkRobotCue.Signal(K.Footstep)),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.LeftBackwardWatchingLedges)),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootSettling, 10),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.LeftBackwardWatchingLedges)),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootLeanForward, 10),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootEasingForward, 10),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootUpright, 96),
            Cue(new WorkRobotCue.Signal(K.CountDownLaserCooldown)),
        ],
        [P.ApproachingFallRight] =
        [
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootLeanBack, 128),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootEasingUp, 10),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootUpright, 10),
            Cue(new WorkRobotCue.Signal(K.CountDownLaserCooldown)),
        ],
        [P.FacingRightWalkingForwards] =
        [
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootLeanBack, 1),
        ],
        [P.FacingRightWalkingStride] =
        [
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootLeanBack, 1),
            Cue(new WorkRobotCue.TryLaser(WorkRobotLaser.Downward, WorkRobotSegment.FacingRightShotLaserDownRight)),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootLeanBack, 9),
            Cue(new WorkRobotCue.Signal(K.CountDownLaserCooldown)),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootEasingUp, 10),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootUpright, 1),
            Cue(new WorkRobotCue.TryLaser(WorkRobotLaser.Horizontal, WorkRobotSegment.FacingRightShotLaserRight)),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootUpright, 9),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootEasingForward, 1),
            Cue(new WorkRobotCue.TryLaser(WorkRobotLaser.Upward, WorkRobotSegment.FacingRightShotLaserUpRight)),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootEasingForward, 9),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootLeanForward, 10),
            Cue(new WorkRobotCue.Signal(K.Footstep)),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.RightForwardWatchingLedges)),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootSettling, 10),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.RightForwardWatchingLedges)),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootLeanBack, 10),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootEasingUp, 10),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootUpright, 10),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootEasingForward, 10),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootLeanForward, 10),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.RightForwardWatchingLedges)),
            Cue(new WorkRobotCue.Signal(K.Footstep)),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootSettling, 10),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.RightForwardWatchingLedges)),
        ],
        [P.FacingRightHitWallMovingForwards] =
        [
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootSettling, 11),
            Cue(new WorkRobotCue.Signal(K.Footstep)),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.RightBackwardWatchingLedges)),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootLeanForward, 10),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootEasingForward, 10),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootUpright, 10),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootEasingUp, 10),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootLeanBack, 10),
            Cue(new WorkRobotCue.Signal(K.Footstep)),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.RightBackwardWatchingLedges)),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootSettling, 10),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.RightBackwardWatchingLedges)),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootLeanForward, 10),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootEasingForward, 10),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootUpright, 10),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootEasingUp, 10),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootLeanBack, 10),
            Cue(new WorkRobotCue.Signal(K.Footstep)),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.RightBackwardWatchingLedges)),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootSettling, 10),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.RightBackwardWatchingLedges)),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootLeanForward, 10),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootEasingForward, 10),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootUpright, 10),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootEasingUp, 10),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootLeanBack, 10),
            Cue(new WorkRobotCue.Signal(K.Footstep)),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.RightBackwardWatchingLedges)),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootSettling, 10),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.RightBackwardWatchingLedges)),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootLeanForward, 10),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootEasingForward, 10),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootUpright, 10),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootEasingUp, 10),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootLeanBack, 10),
        ],
        [P.FacingRightShotSamusAhead] =
        [
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootSettling, 6),
            Cue(new WorkRobotCue.Signal(K.Footstep)),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.RightBackward)),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootLeanForward, 5),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootEasingForward, 5),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootUpright, 5),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootEasingUp, 5),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootLeanBack, 5),
            Cue(new WorkRobotCue.Signal(K.Footstep)),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.RightBackward)),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootSettling, 5),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.RightBackward)),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootLeanForward, 5),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootEasingForward, 5),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootUpright, 5),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootEasingUp, 5),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootLeanBack, 5),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.RightBackward)),
            Cue(new WorkRobotCue.Signal(K.Footstep)),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootSettling, 5),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.RightBackward)),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootLeanForward, 5),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootEasingForward, 5),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootUpright, 5),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootEasingUp, 5),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootLeanBack, 5),
            Cue(new WorkRobotCue.Signal(K.Footstep)),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.RightBackward)),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootSettling, 10),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.RightBackward)),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootLeanForward, 5),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootEasingForward, 5),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootUpright, 5),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootEasingUp, 5),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootLeanBack, 5),
        ],
        [P.FacingRightShotSamusBehind] =
        [
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.RightForward)),
            Cue(new WorkRobotCue.Signal(K.Footstep)),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootLeanBack, 5),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootEasingUp, 5),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootUpright, 5),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootEasingForward, 5),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootLeanForward, 5),
            Cue(new WorkRobotCue.Signal(K.Footstep)),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.RightForward)),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootSettling, 5),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.RightForward)),
            Cue(new WorkRobotCue.Signal(K.Footstep)),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootLeanBack, 5),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootEasingUp, 5),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootUpright, 5),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootEasingForward, 5),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootLeanForward, 5),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.RightForward)),
            Cue(new WorkRobotCue.Signal(K.Footstep)),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootSettling, 5),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.RightForward)),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootLeanBack, 5),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootEasingUp, 5),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootUpright, 5),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootEasingForward, 5),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootLeanForward, 5),
            Cue(new WorkRobotCue.Signal(K.Footstep)),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.RightForward)),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootSettling, 5),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.RightForward)),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootLeanBack, 5),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootEasingUp, 5),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootUpright, 5),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootEasingForward, 5),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootLeanForward, 5),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.RightForward)),
            Cue(new WorkRobotCue.Signal(K.Footstep)),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootSettling, 5),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.RightForward)),
        ],
        [P.FacingRightShotLaserDownRight] =
        [
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootLeanBack, 5),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootSettling, 2),
        ],
        [P.FacingRightShotLaserRight] =
        [
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootEasingUp, 5),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootLeanBack, 2),
            Cue(new WorkRobotCue.Signal(K.Footstep)),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.RightBackwardWatchingLedges)),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootSettling, 10),
        ],
        [P.FacingRightShotLaserUpRight] =
        [
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootEasingForward, 5),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootUpright, 2),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootEasingUp, 2),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootLeanBack, 4),
            Cue(new WorkRobotCue.Signal(K.Footstep)),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.RightBackwardWatchingLedges)),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootSettling, 4),
        ],
        [P.FacingRightLaserShotRecoil] =
        [
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.RightBackwardWatchingLedges)),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootLeanForward, 16),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootEasingForward, 5),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootUpright, 5),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootEasingUp, 5),
            Show(EnemyFrameId.WorkRobotWalkingRightFarFootLeanBack, 5),
            Cue(new WorkRobotCue.Signal(K.Footstep)),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.RightBackwardWatchingLedges)),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootSettling, 10),
            Cue(new WorkRobotCue.Step(WorkRobotMoveStepDefinitions.RightBackwardWatchingLedges)),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootLeanForward, 10),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootEasingForward, 10),
            Show(EnemyFrameId.WorkRobotWalkingRightNearFootUpright, 96),
        ],
        [P.ApproachingFallLeft] =
        [
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootLeanBack, 128),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootEasingUp, 10),
            Show(EnemyFrameId.WorkRobotWalkingLeftNearFootUpright, 10),
            Cue(new WorkRobotCue.Signal(K.CountDownLaserCooldown)),
        ],
    });

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at. The wall, shot-ahead and shot-behind
    /// animations walk on, as the original's lists go on to a walk when they end (#2167).
    /// </summary>
    internal static IReadOnlyDictionary<WorkRobotSegment, Animation<WorkRobotSegment>> StartingAt { get; } = new Dictionary<WorkRobotSegment, Animation<WorkRobotSegment>>
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

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, WorkRobotCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, WorkRobotCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good: the animation rests on it until its owner plays another (#1607).</summary>
    private static AnimationBeat<EnemyFrameId, WorkRobotCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, WorkRobotCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, WorkRobotCue> Cue(WorkRobotCue cue) => new AnimationBeat<EnemyFrameId, WorkRobotCue>.Event(cue);
}
