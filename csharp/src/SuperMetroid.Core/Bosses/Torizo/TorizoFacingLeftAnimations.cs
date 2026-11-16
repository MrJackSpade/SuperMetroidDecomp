using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Bosses.Torizo.TorizoCueKind;
using A = SuperMetroid.Core.Bosses.Torizo.TorizoAnimation;
using static SuperMetroid.Core.Bosses.Torizo.TorizoAnimationSteps;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Torizo's animations facing left: turning, attacks, jumps, falls and landings, with its face and without.</summary>
internal static class TorizoFacingLeftAnimations
{
    /// <summary>Each segment's steps, in the order it plays them.</summary>
    internal static Dictionary<TorizoAnimation, AnimationBeat<EnemyFrameId, TorizoCue>[]> Segments { get; } = new()
    {
        [A.TurningLeft] =
        [
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Falling)),
            Cue(new TorizoCue.Signal(K.SetTurningAround)),
            Show(EnemyFrameId.TorizoFacingViewer, 24),
        ],
        [A.FacingLeftSpewingChozoOrbsRightFootForward] =
        [
            Cue(new TorizoCue.Signal(K.StartStanding)),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardSpewingHeadLowered, 16),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardSpewingHeadRaised, 8),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardSpewingMouthOpening, 8),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardSpewingArmThrust, 8),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardSpewingArmRaised, 8),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardSpewingArmDrawnBack, 16),
            Cue(new TorizoCue.Signal(K.PlayShotSound)),
        ],
        [A.FacingLeftSpewingChozoOrbsRightFootForwardEnd] =
        [
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardSpewingArmRaised, 8),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardSpewingArmThrust, 8),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardSpewingMouthOpening, 8),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardSpewingHeadRaised, 72),
            Cue(new TorizoCue.Signal(K.StartWalking)),
            Cue(new TorizoCue.WalkOn(Walking: A.FacingLeftWalkingLeftLegMoving)),
        ],
        [A.FacingLeftSpewingChozoOrbsLeftFootForward] =
        [
            Cue(new TorizoCue.Signal(K.StartStanding)),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardSpewingHeadLowered, 16),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardSpewingHeadRaised, 8),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardSpewingMouthOpening, 8),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardSpewingArmThrust, 8),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardSpewingArmRaised, 8),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardSpewingArmDrawnBack, 16),
            Cue(new TorizoCue.Signal(K.PlayShotSound)),
        ],
        [A.FacingLeftSpewingChozoOrbsLeftFootForwardEnd] =
        [
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardSpewingArmRaised, 8),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardSpewingArmThrust, 8),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardSpewingMouthOpening, 8),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardSpewingHeadRaised, 72),
            Cue(new TorizoCue.Signal(K.StartWalking)),
            Cue(new TorizoCue.WalkOn(Walking: A.FacingLeftWalkingRightLegMoving)),
        ],
        [A.FacingLeftSonicBoomsRightFootForward] =
        [
            Cue(new TorizoCue.Signal(K.StartStanding)),
        ],
        [A.FacingLeftSonicBoomsRightFootForwardThrowing] =
        [
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardSpewingHeadLowered, 6),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardNearArmDown, 6),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardNearArmLifting, 6),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardNearArmLevel, 6),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardNearArmRaised, 6),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardNearArmOverhead, 6),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardNearArmRaised, 2),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardNearArmLevel, 2),
            Cue(new TorizoCue.Signal(K.SpawnBombTorizoSonicBoom)),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardNearArmLifting, 2),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardNearArmDown, 24),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardSpewingHeadLowered, 6),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardFarArmDown, 6),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardFarArmLifting, 6),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardFarArmLevel, 6),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardFarArmRaised, 6),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardFarArmOverhead, 6),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardFarArmRaised, 2),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardFarArmLevel, 2),
            Cue(new TorizoCue.Signal(K.SpawnBombTorizoSonicBoom)),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardFarArmLifting, 2),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardFarArmDown, 56),
        ],
        [A.FacingLeftSonicBoomsRightFootForwardEnd] =
        [
            Cue(new TorizoCue.Signal(K.StartWalking)),
            Cue(new TorizoCue.WalkOn(Walking: A.FacingLeftWalkingLeftLegMoving)),
        ],
        [A.FacingLeftSonicBoomsLeftFootForward] =
        [
            Cue(new TorizoCue.Signal(K.StartStanding)),
        ],
        [A.FacingLeftSonicBoomsLeftFootForwardThrowing] =
        [
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardSpewingHeadLowered, 6),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardNearArmDown, 6),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardNearArmLifting, 6),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardNearArmLevel, 6),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardNearArmRaised, 6),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardNearArmOverhead, 6),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardNearArmRaised, 2),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardNearArmLevel, 2),
            Cue(new TorizoCue.Signal(K.SpawnBombTorizoSonicBoom)),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardNearArmLifting, 2),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardNearArmDown, 24),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardSpewingHeadLowered, 6),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardFarArmDown, 6),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardFarArmLifting, 6),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardFarArmLevel, 6),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardFarArmRaised, 6),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardFarArmOverhead, 6),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardFarArmRaised, 2),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardFarArmLevel, 2),
            Cue(new TorizoCue.Signal(K.SpawnBombTorizoSonicBoom)),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardFarArmLifting, 2),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardFarArmDown, 56),
        ],
        [A.FacingLeftSonicBoomsLeftFootForwardEnd] =
        [
            Cue(new TorizoCue.Signal(K.StartWalking)),
            Cue(new TorizoCue.WalkOn(Walking: A.FacingLeftWalkingRightLegMoving)),
        ],
        [A.FacingLeftExplosiveSwipeRightFootForward] =
        [
            Cue(new TorizoCue.Signal(K.StartStanding)),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardSpewingHeadLowered, 3),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardNearArmDown, 3),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardNearArmLifting, 3),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardNearArmLevel, 3),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardNearArmRaised, 3),
            Cue(new TorizoCue.Swipe(Hand: new(-30, -52))),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardNearArmOverhead, 3),
            Cue(new TorizoCue.Swipe(Hand: new(-40, -28))),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardNearArmRaised, 1),
            Cue(new TorizoCue.Swipe(Hand: new(-47, -11))),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardNearArmLevel, 1),
            Cue(new TorizoCue.Swipe(Hand: new(-31, 9))),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardNearArmLifting, 1),
            Cue(new TorizoCue.Swipe(Hand: new(-21, 21))),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardNearArmDown, 1),
            Cue(new TorizoCue.Swipe(Hand: new(-1, 20))),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardSpewingHeadLowered, 3),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardFarArmDown, 3),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardFarArmLifting, 3),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardFarArmLevel, 3),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardFarArmRaised, 3),
            Cue(new TorizoCue.Swipe(Hand: new(-28, -52))),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardFarArmOverhead, 3),
            Cue(new TorizoCue.Swipe(Hand: new(-43, -27))),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardFarArmRaised, 1),
            Cue(new TorizoCue.Swipe(Hand: new(-48, -10))),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardFarArmLevel, 1),
            Cue(new TorizoCue.Swipe(Hand: new(-31, 9))),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardFarArmLifting, 1),
            Show(EnemyFrameId.TorizoFacingLeftRightFootForwardFarArmDown, 16),
            Cue(new TorizoCue.Signal(K.StartWalking)),
            Cue(new TorizoCue.Signal(K.ResumeWalk)),
        ],
        [A.FacingLeftExplosiveSwipeLeftFootForward] =
        [
            Cue(new TorizoCue.Signal(K.StartStanding)),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardSpewingHeadLowered, 3),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardNearArmDown, 3),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardNearArmLifting, 3),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardNearArmLevel, 3),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardNearArmRaised, 3),
            Cue(new TorizoCue.Swipe(Hand: new(-30, -52))),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardNearArmOverhead, 3),
            Cue(new TorizoCue.Swipe(Hand: new(-40, -28))),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardNearArmRaised, 1),
            Cue(new TorizoCue.Swipe(Hand: new(-47, -11))),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardNearArmLevel, 1),
            Cue(new TorizoCue.Swipe(Hand: new(-31, 9))),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardNearArmLifting, 1),
            Cue(new TorizoCue.Swipe(Hand: new(-21, 21))),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardNearArmDown, 1),
            Cue(new TorizoCue.Swipe(Hand: new(-1, 20))),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardSpewingHeadLowered, 3),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardFarArmDown, 3),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardFarArmLifting, 3),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardFarArmLevel, 3),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardFarArmRaised, 3),
            Cue(new TorizoCue.Swipe(Hand: new(-28, -52))),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardFarArmOverhead, 3),
            Cue(new TorizoCue.Swipe(Hand: new(-43, -27))),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardFarArmRaised, 1),
            Cue(new TorizoCue.Swipe(Hand: new(-48, -10))),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardFarArmLevel, 1),
            Cue(new TorizoCue.Swipe(Hand: new(-31, 9))),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardFarArmLifting, 1),
            Show(EnemyFrameId.TorizoFacingLeftLeftFootForwardFarArmDown, 16),
            Cue(new TorizoCue.Signal(K.StartWalking)),
            Cue(new TorizoCue.Signal(K.ResumeWalk)),
        ],
        [A.FacingLeftJumpingForward] =
        [
            Cue(new TorizoCue.Signal(K.StartLeaping)),
            Cue(new TorizoCue.LandIn(Landing: A.FacingLeftFalling)),
            Show(EnemyFrameId.TorizoFacingLeftJumpCrouch, 5),
            Show(EnemyFrameId.TorizoFallingLeft, 5),
        ],
        [A.FacingLeftFalling] =
        [
            Cue(new TorizoCue.Signal(K.StartLeaping)),
            Cue(new TorizoCue.LandIn(Landing: A.FacingLeftLanding)),
        ],
        [A.FacingLeftLanding] =
        [
            Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
            Cue(new TorizoCue.Signal(K.SpawnLandingDust)),
            Cue(new TorizoCue.Choice(TorizoChoiceKind.LandFacelessOrGolden, Target: A.FacingLeftFacelessWalkingLeftLegMoving, Alternate: A.GoldenWalkingLeftLeftLegMoving)),
        ],
        [A.FacingLeftJumpingBackwardToLeftFoot] =
        [
            Cue(new TorizoCue.Signal(K.StartLeaping)),
            Cue(new TorizoCue.LandIn(Landing: A.FacingLeftFallingBackwardToLeftFoot)),
            Show(EnemyFrameId.TorizoFacingLeftJumpCrouch, 5),
            Show(EnemyFrameId.TorizoFallingLeft, 5),
        ],
        [A.FacingLeftFallingBackwardToLeftFoot] =
        [
            Cue(new TorizoCue.Signal(K.StartLeaping)),
            Cue(new TorizoCue.LandIn(Landing: A.FacingLeftLandingBackwardOnLeftFoot)),
        ],
        [A.FacingLeftLandingBackwardOnLeftFoot] =
        [
            Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
            Cue(new TorizoCue.Signal(K.SpawnLandingDust)),
            Cue(new TorizoCue.Choice(TorizoChoiceKind.LandFacelessOrGolden, Target: A.FacingLeftFacelessWalkingRightLegMoving, Alternate: A.GoldenFacingLeftLandedBackwardOnLeftFoot)),
            Cue(new TorizoCue.Choice(TorizoChoiceKind.BombAttack, Target: A.FacingLeftSpewingChozoOrbsLeftFootForward, Alternate: A.FacingLeftSonicBoomsLeftFootForward)),
        ],
        [A.FacingLeftJumpingBackwardToRightFoot] =
        [
            Cue(new TorizoCue.Signal(K.StartLeaping)),
            Cue(new TorizoCue.LandIn(Landing: A.FacingLeftFallingBackwardToRightFoot)),
            Show(EnemyFrameId.TorizoFacingLeftJumpCrouch, 5),
            Show(EnemyFrameId.TorizoFallingLeft, 5),
        ],
        [A.FacingLeftFallingBackwardToRightFoot] =
        [
            Cue(new TorizoCue.Signal(K.StartLeaping)),
            Cue(new TorizoCue.LandIn(Landing: A.FacingLeftLandingBackwardOnRightFoot)),
        ],
        [A.FacingLeftLandingBackwardOnRightFoot] =
        [
            Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
            Cue(new TorizoCue.Signal(K.SpawnLandingDust)),
            Cue(new TorizoCue.Choice(TorizoChoiceKind.LandFacelessOrGolden, Target: A.FacingLeftFacelessWalkingLeftLegMoving, Alternate: A.GoldenFacingLeftLandedBackwardOnRightFoot)),
            Cue(new TorizoCue.Choice(TorizoChoiceKind.BombAttack, Target: A.FacingLeftSpewingChozoOrbsRightFootForward, Alternate: A.FacingLeftSonicBoomsRightFootForward)),
        ],
        [A.FacelessTurningLeft] =
        [
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Falling)),
            Cue(new TorizoCue.Signal(K.SetTurningAround)),
            Show(EnemyFrameId.TorizoFacingViewer, 8),
        ],
        [A.FacingLeftFacelessWalkingRightLegMoving] =
        [
            Cue(new TorizoCue.Stepped(HorizontalDirection.Left, Foot: HorizontalDirection.Left)),
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Active)),
            Cue(new TorizoCue.Signal(K.StartWalking)),
            Cue(new TorizoCue.Signal(K.SpawnLowHealthDrool)),
            Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
            Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegLifting, 1),
            Cue(new TorizoCue.Interrupt(TorizoInterruptKind.BombSwipeAtSamusBehind, Animation: A.FacingLeftExplosiveSwipeLeftFootForward)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Velocity: 0)),
            Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegSwinging, 5),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Velocity: -5)),
            Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegReaching, 5),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Velocity: -19)),
            Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegPlanting, 5),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Velocity: -16)),
            Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegPlanted, 5),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Velocity: -7)),
        ],
        [A.FacingLeftFacelessWalkingLeftLegMoving] =
        [
            Cue(new TorizoCue.Stepped(HorizontalDirection.Left, Foot: HorizontalDirection.Right)),
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Active)),
            Cue(new TorizoCue.Signal(K.StartWalking)),
            Cue(new TorizoCue.Signal(K.SpawnLowHealthDrool)),
            Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
            Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegLifting, 1),
            Cue(new TorizoCue.Interrupt(TorizoInterruptKind.BombSwipeAtSamusBehind, Animation: A.FacingLeftExplosiveSwipeRightFootForward)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Velocity: 0)),
            Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegSwinging, 5),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Velocity: -7)),
            Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegReaching, 5),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Velocity: -17)),
            Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegPlanting, 5),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Velocity: -18)),
            Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegPlanted, 5),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Velocity: -5)),
        ],
        [A.FacingLeftWalkingRightLegMoving] =
        [
            Cue(new TorizoCue.Stepped(HorizontalDirection.Left, Foot: HorizontalDirection.Left)),
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Active)),
            Cue(new TorizoCue.Signal(K.StartWalking)),
            Cue(new TorizoCue.Signal(K.SpawnLowHealthDrool)),
            Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
            Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegLifting, 10),
            Cue(new TorizoCue.Interrupt(TorizoInterruptKind.BombSwipeAtSamusBehind, Animation: A.FacingLeftExplosiveSwipeLeftFootForward)),
            Cue(new TorizoCue.Branch(TorizoBranchKind.BombJumpBackFromSamusInFront)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Velocity: 0)),
            Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegSwinging, 6),
            Cue(new TorizoCue.Branch(TorizoBranchKind.BombJumpBackFromSamusInFront)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Velocity: -5)),
            Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegReaching, 6),
            Cue(new TorizoCue.Branch(TorizoBranchKind.BombJumpBackFromSamusInFront)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Velocity: -19)),
            Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegPlanting, 6),
            Cue(new TorizoCue.Branch(TorizoBranchKind.BombJumpBackFromSamusInFront)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Velocity: -16)),
            Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegPlanted, 6),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Velocity: -7)),
        ],
        [A.FacingLeftWalkingLeftLegMoving] =
        [
            Cue(new TorizoCue.Stepped(HorizontalDirection.Left, Foot: HorizontalDirection.Right)),
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Active)),
            Cue(new TorizoCue.Signal(K.StartWalking)),
            Cue(new TorizoCue.Signal(K.SpawnLowHealthDrool)),
            Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
            Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegLifting, 10),
            Cue(new TorizoCue.Interrupt(TorizoInterruptKind.BombSwipeAtSamusBehind, Animation: A.FacingLeftExplosiveSwipeRightFootForward)),
            Cue(new TorizoCue.Branch(TorizoBranchKind.BombJumpBackFromSamusInFront)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Velocity: 0)),
            Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegSwinging, 6),
            Cue(new TorizoCue.Branch(TorizoBranchKind.BombJumpBackFromSamusInFront)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Velocity: -7)),
            Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegReaching, 6),
            Cue(new TorizoCue.Branch(TorizoBranchKind.BombJumpBackFromSamusInFront)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Velocity: -17)),
            Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegPlanting, 6),
            Cue(new TorizoCue.Branch(TorizoBranchKind.BombJumpBackFromSamusInFront)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Velocity: -18)),
            Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegPlanted, 6),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Velocity: -5)),
        ],
        [A.FacingLeftRising] =
        [
            Show(EnemyFrameId.TorizoFacingLeftLeaping, 1),
            Cue(new TorizoCue.Replay(TorizoReplayKind.WhileRising)),
        ],
        [A.FacingLeftFallingPose] =
        [
            Show(EnemyFrameId.TorizoFallingLeft, 5),
        ],
        [A.BombChozoOrbBurst] =
        [
            Cue(new TorizoCue.Signal(K.SpawnBombTorizoChozoOrbs)),
            Wait(16),
        ],
    };
}
