using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Bosses.Torizo.TorizoCueKind;
using A = SuperMetroid.Core.Bosses.Torizo.TorizoAnimation;
using S = SuperMetroid.Core.Bosses.Torizo.TorizoSharedSteps;
using static SuperMetroid.Core.Bosses.Torizo.TorizoAnimationSteps;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Torizo's animations facing right: turning, attacks, jumps, falls and landings, with its face and without.</summary>
internal static class TorizoFacingRightAnimations
{
    /// <summary>Each animation's steps, in the order it plays them.</summary>
    internal static Dictionary<TorizoAnimation, AnimationBeat<EnemyFrameId, TorizoCue>[]> Segments { get; } = new()
    {
        [A.TurningRight] =
        [
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Falling)),
            Cue(new TorizoCue.Signal(K.SetTurningAround)),
            Show(EnemyFrameId.TorizoFacingViewer, 24),
            .. S.FacingRightLeftLegStep,
            .. S.FacingRightRightLegStep,
        ],
        [A.FacingRightSpewingChozoOrbsLeftFootForward] =
        [
            Cue(new TorizoCue.Signal(K.StartStanding)),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardSpewingHeadLowered, 16),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardSpewingHeadRaised, 8),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardSpewingMouthOpening, 8),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardSpewingArmThrust, 8),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardSpewingArmRaised, 8),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardSpewingArmDrawnBack, 16),
            Cue(new TorizoCue.Signal(K.PlayShotSound)),
            Cue(new TorizoCue.Signal(K.SpawnBombTorizoChozoOrbs)),
            Wait(16),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardSpewingArmRaised, 8),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardSpewingArmThrust, 8),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardSpewingMouthOpening, 8),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardSpewingHeadRaised, 72),
            Cue(new TorizoCue.Signal(K.StartWalking)),
            Cue(new TorizoCue.WalkOn(Walking: A.FacingRightWalkingRightLegMoving)),
        ],
        [A.FacingRightSpewingChozoOrbsRightFootForward] =
        [
            Cue(new TorizoCue.Signal(K.StartStanding)),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardSpewingHeadLowered, 16),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardSpewingHeadRaised, 8),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardSpewingMouthOpening, 8),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardSpewingArmThrust, 8),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardSpewingArmRaised, 8),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardSpewingArmDrawnBack, 16),
            Cue(new TorizoCue.Signal(K.PlayShotSound)),
            Cue(new TorizoCue.Signal(K.SpawnBombTorizoChozoOrbs)),
            Wait(16),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardSpewingArmRaised, 8),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardSpewingArmThrust, 8),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardSpewingMouthOpening, 8),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardSpewingHeadRaised, 72),
            Cue(new TorizoCue.Signal(K.StartWalking)),
            Cue(new TorizoCue.WalkOn(Walking: A.FacingRightWalkingLeftLegMoving)),
        ],
        [A.FacingRightSonicBoomsLeftFootForward] =
        [
            Cue(new TorizoCue.Signal(K.StartStanding)),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardSpewingHeadLowered, 6),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardNearArmDown, 6),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardNearArmLifting, 6),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardNearArmLevel, 6),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardNearArmRaised, 6),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardNearArmOverhead, 6),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardNearArmRaised, 2),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardNearArmLevel, 2),
            Cue(new TorizoCue.Signal(K.SpawnBombTorizoSonicBoom)),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardNearArmLifting, 2),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardNearArmDown, 24),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardSpewingHeadLowered, 6),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardFarArmDown, 6),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardFarArmLifting, 6),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardFarArmLevel, 6),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardFarArmRaised, 6),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardFarArmOverhead, 6),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardFarArmRaised, 2),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardFarArmLevel, 2),
            Cue(new TorizoCue.Signal(K.SpawnBombTorizoSonicBoom)),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardFarArmLifting, 2),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardFarArmDown, 56),
            Cue(new TorizoCue.Signal(K.StartWalking)),
            Cue(new TorizoCue.WalkOn(Walking: A.FacingRightWalkingRightLegMoving)),
        ],
        [A.FacingRightSonicBoomsRightFootForward] =
        [
            Cue(new TorizoCue.Signal(K.StartStanding)),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardSpewingHeadLowered, 6),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardNearArmDown, 6),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardNearArmLifting, 6),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardNearArmLevel, 6),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardNearArmRaised, 6),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardNearArmOverhead, 6),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardNearArmRaised, 2),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardNearArmLevel, 2),
            Cue(new TorizoCue.Signal(K.SpawnBombTorizoSonicBoom)),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardNearArmLifting, 2),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardNearArmDown, 24),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardSpewingHeadLowered, 6),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardFarArmDown, 6),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardFarArmLifting, 6),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardFarArmLevel, 6),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardFarArmRaised, 6),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardFarArmOverhead, 6),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardFarArmRaised, 2),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardFarArmLevel, 2),
            Cue(new TorizoCue.Signal(K.SpawnBombTorizoSonicBoom)),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardFarArmLifting, 2),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardFarArmDown, 56),
            Cue(new TorizoCue.Signal(K.StartWalking)),
            Cue(new TorizoCue.WalkOn(Walking: A.FacingRightWalkingLeftLegMoving)),
        ],
        [A.FacingRightExplosiveSwipeLeftFootForward] =
        [
            Cue(new TorizoCue.Signal(K.StartStanding)),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardSpewingHeadLowered, 3),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardNearArmDown, 3),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardNearArmLifting, 3),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardNearArmLevel, 3),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardNearArmRaised, 3),
            Cue(new TorizoCue.Swipe(Hand: new(-30, -52))),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardNearArmOverhead, 3),
            Cue(new TorizoCue.Swipe(Hand: new(-40, -28))),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardNearArmRaised, 1),
            Cue(new TorizoCue.Swipe(Hand: new(-47, -11))),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardNearArmLevel, 1),
            Cue(new TorizoCue.Swipe(Hand: new(-31, 9))),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardNearArmLifting, 1),
            Cue(new TorizoCue.Swipe(Hand: new(-21, 21))),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardNearArmDown, 1),
            Cue(new TorizoCue.Swipe(Hand: new(-1, 20))),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardSpewingHeadLowered, 3),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardFarArmDown, 3),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardFarArmLifting, 3),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardFarArmLevel, 3),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardFarArmRaised, 3),
            Cue(new TorizoCue.Swipe(Hand: new(-28, -52))),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardFarArmOverhead, 3),
            Cue(new TorizoCue.Swipe(Hand: new(-43, -27))),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardFarArmRaised, 1),
            Cue(new TorizoCue.Swipe(Hand: new(-48, -10))),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardFarArmLevel, 1),
            Cue(new TorizoCue.Swipe(Hand: new(-31, 9))),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardFarArmLifting, 1),
            Show(EnemyFrameId.TorizoFacingRightLeftFootForwardFarArmDown, 16),
            Cue(new TorizoCue.Signal(K.StartWalking)),
            Cue(new TorizoCue.Signal(K.ResumeWalk)),
        ],
        [A.FacingRightExplosiveSwipeRightFootForward] =
        [
            Cue(new TorizoCue.Signal(K.StartStanding)),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardSpewingHeadLowered, 3),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardNearArmDown, 3),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardNearArmLifting, 3),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardNearArmLevel, 3),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardNearArmRaised, 3),
            Cue(new TorizoCue.Swipe(Hand: new(-30, -52))),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardNearArmOverhead, 3),
            Cue(new TorizoCue.Swipe(Hand: new(-40, -28))),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardNearArmRaised, 1),
            Cue(new TorizoCue.Swipe(Hand: new(-47, -11))),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardNearArmLevel, 1),
            Cue(new TorizoCue.Swipe(Hand: new(-31, 9))),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardNearArmLifting, 1),
            Cue(new TorizoCue.Swipe(Hand: new(-21, 21))),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardNearArmDown, 1),
            Cue(new TorizoCue.Swipe(Hand: new(-1, 20))),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardSpewingHeadLowered, 3),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardFarArmDown, 3),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardFarArmLifting, 3),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardFarArmLevel, 3),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardFarArmRaised, 3),
            Cue(new TorizoCue.Swipe(Hand: new(-28, -52))),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardFarArmOverhead, 3),
            Cue(new TorizoCue.Swipe(Hand: new(-43, -27))),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardFarArmRaised, 1),
            Cue(new TorizoCue.Swipe(Hand: new(-48, -10))),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardFarArmLevel, 1),
            Cue(new TorizoCue.Swipe(Hand: new(-31, 9))),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardFarArmLifting, 1),
            Show(EnemyFrameId.TorizoFacingRightRightFootForwardFarArmDown, 16),
            Cue(new TorizoCue.Signal(K.StartWalking)),
            Cue(new TorizoCue.Signal(K.ResumeWalk)),
        ],
        [A.FacingRightJumpingForward] =
        [
            Cue(new TorizoCue.Signal(K.StartLeaping)),
            Cue(new TorizoCue.LandIn(Landing: A.FacingRightFalling)),
            Show(EnemyFrameId.TorizoFacingRightJumpCrouch, 5),
            Show(EnemyFrameId.TorizoFacingRightSpringing, 5),
            Show(EnemyFrameId.TorizoFacingRightLeaping, 1),
            Cue(new TorizoCue.Replay(TorizoReplayKind.WhileRising)),
            .. S.FacingRightFallStart,
            .. S.FacingRightFallingPose,
        ],
        [A.FacingRightFalling] =
        [
            .. S.FacingRightFallStart,
            .. S.FacingRightFallingPose,
        ],
        [A.FacingRightLanding] =
        [
            Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
            Cue(new TorizoCue.Signal(K.SpawnLandingDust)),
            Cue(new TorizoCue.Choice(TorizoChoiceKind.LandFacelessOrGolden, Target: A.FacingRightFacelessWalkingRightLegMoving, Alternate: A.GoldenWalkingRightRightLegMoving)),
            .. S.FacingRightRightLegStep,
            .. S.FacingRightLeftLegStep,
        ],
        [A.FacingRightJumpingBackwardToRightFoot] =
        [
            Cue(new TorizoCue.Signal(K.StartLeaping)),
            Cue(new TorizoCue.LandIn(Landing: A.FacingRightFallingBackwardToRightFoot)),
            Show(EnemyFrameId.TorizoFacingRightJumpCrouch, 5),
            Show(EnemyFrameId.TorizoFacingRightSpringing, 5),
            Show(EnemyFrameId.TorizoFacingRightLeaping, 1),
            Cue(new TorizoCue.Replay(TorizoReplayKind.WhileRising)),
            .. S.FacingRightBackwardFallStartToRightFoot,
            .. S.FacingRightBackwardFallingToRightFoot,
        ],
        [A.FacingRightFallingBackwardToRightFoot] =
        [
            .. S.FacingRightBackwardFallStartToRightFoot,
            .. S.FacingRightBackwardFallingToRightFoot,
        ],
        [A.FacingRightLandingBackwardOnRightFoot] =
        [
            Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
            Cue(new TorizoCue.Signal(K.SpawnLandingDust)),
            Cue(new TorizoCue.Choice(TorizoChoiceKind.LandFacelessOrGolden, Target: A.FacingRightFacelessWalkingLeftLegMoving, Alternate: A.GoldenFacingRightLandedBackwardOnRightFoot)),
            Cue(new TorizoCue.Choice(TorizoChoiceKind.BombAttack, Target: A.FacingRightSpewingChozoOrbsRightFootForward, Alternate: A.FacingRightSonicBoomsRightFootForward)),
            .. S.FacingRightLeftLegStep,
            .. S.FacingRightRightLegStep,
        ],
        [A.FacingRightJumpingBackwardToLeftFoot] =
        [
            Cue(new TorizoCue.Signal(K.StartLeaping)),
            Cue(new TorizoCue.LandIn(Landing: A.FacingRightFallingBackwardToLeftFoot)),
            Show(EnemyFrameId.TorizoFacingRightJumpCrouch, 5),
            Show(EnemyFrameId.TorizoFacingRightSpringing, 5),
            Show(EnemyFrameId.TorizoFacingRightLeaping, 1),
            Cue(new TorizoCue.Replay(TorizoReplayKind.WhileRising)),
            .. S.FacingRightBackwardFallStartToLeftFoot,
            .. S.FacingRightBackwardFallingToLeftFoot,
        ],
        [A.FacingRightFallingBackwardToLeftFoot] =
        [
            .. S.FacingRightBackwardFallStartToLeftFoot,
            .. S.FacingRightBackwardFallingToLeftFoot,
        ],
        [A.FacingRightLandingBackwardOnLeftFoot] =
        [
            Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
            Cue(new TorizoCue.Signal(K.SpawnLandingDust)),
            Cue(new TorizoCue.Choice(TorizoChoiceKind.LandFacelessOrGolden, Target: A.FacingRightFacelessWalkingRightLegMoving, Alternate: A.GoldenFacingRightLandedBackwardOnLeftFoot)),
            Cue(new TorizoCue.Choice(TorizoChoiceKind.BombAttack, Target: A.FacingRightSpewingChozoOrbsLeftFootForward, Alternate: A.FacingRightSonicBoomsLeftFootForward)),
            .. S.FacingRightRightLegStep,
            .. S.FacingRightLeftLegStep,
        ],
        [A.FacelessTurningRight] =
        [
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Falling)),
            Cue(new TorizoCue.Signal(K.SetTurningAround)),
            Show(EnemyFrameId.TorizoFacingViewer, 8),
            .. S.FacingRightFacelessLeftLegStep,
            .. S.FacingRightFacelessRightLegStep,
        ],
        [A.FacingRightFacelessWalkingLeftLegMoving] =
        [
            .. S.FacingRightFacelessLeftLegStep,
            .. S.FacingRightFacelessRightLegStep,
        ],
        [A.FacingRightFacelessWalkingRightLegMoving] =
        [
            .. S.FacingRightFacelessRightLegStep,
            .. S.FacingRightFacelessLeftLegStep,
        ],
        [A.FacingRightWalkingLeftLegMoving] =
        [
            .. S.FacingRightLeftLegStep,
            .. S.FacingRightRightLegStep,
        ],
        [A.FacingRightWalkingRightLegMoving] =
        [
            .. S.FacingRightRightLegStep,
            .. S.FacingRightLeftLegStep,
        ],
    };
}
