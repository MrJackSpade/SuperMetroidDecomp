using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Bosses.Torizo.TorizoCueKind;
using P = SuperMetroid.Core.Bosses.Torizo.TorizoBodySegment;
using static SuperMetroid.Core.Bosses.Torizo.TorizoAnimationSteps;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Bomb Torizo's programs while it faces right: turning, walking, attacks, jumps and falls, with its face and without.</summary>
internal static class TorizoFacingRightAnimations
{
    /// <summary>Each segment and its beats.</summary>
    internal static Dictionary<TorizoBodySegment, AnimationBeat<EnemyFrameId, TorizoCue>[]> Segments { get; } = new()
    {
        [P.FacingRightTurningRight] =
        [
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.Falling)),
            Cue(new TorizoCue.Command(K.SetTurningAround)),
            Show(EnemyFrameId.TorizoFacingLeftTurningLeft, 24),
        ],
        [P.FacingRightWalkingLeftLegMoving] =
        [
            Cue(new TorizoCue.Command(K.SteppedRightWithRightFoot)),
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.Active)),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.SpawnLowHealthDrool)),
            Cue(new TorizoCue.Command(K.PlayFootstepSound)),
            Show(EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingFirst, 10),
            Cue(new TorizoCue.Branch(TorizoBranchKind.CallIfSamusCloseBehind, Target: P.FacingRightExplosiveSwipeRightFootForward)),
            Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: P.FacingRightJumpBackwardLandRightFootFwd0)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 11)),
            Show(EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingSecond, 6),
            Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: P.FacingRightJumpBackwardLandRightFootFwd0)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 12)),
            Show(EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingThird, 6),
            Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: P.FacingRightJumpBackwardLandRightFootFwd0)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 13)),
            Show(EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingFourth, 6),
            Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: P.FacingRightJumpBackwardLandRightFootFwd0)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 14)),
            Show(EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingFifth, 6),
            Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: P.FacingRightJumpBackwardLandRightFootFwd0)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 15)),
        ],
        [P.FacingRightWalkingRightLegMoving] =
        [
            Cue(new TorizoCue.Command(K.SteppedRightWithLeftFoot)),
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.Active)),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.SpawnLowHealthDrool)),
            Cue(new TorizoCue.Command(K.PlayFootstepSound)),
            Show(EnemyFrameId.TorizoFacingRightWalkingRightLegMovingFirst, 10),
            Cue(new TorizoCue.Branch(TorizoBranchKind.CallIfSamusCloseBehind, Target: P.FacingRightExplosiveSwipeLeftFootForward)),
            Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: P.FacingRightJumpBackwardsLandLeftFootFwd0)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 16)),
            Show(EnemyFrameId.TorizoFacingRightWalkingRightLegMovingSecond, 6),
            Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: P.FacingRightJumpBackwardsLandLeftFootFwd0)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 17)),
            Show(EnemyFrameId.TorizoFacingRightWalkingRightLegMovingThird, 6),
            Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: P.FacingRightJumpBackwardsLandLeftFootFwd0)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 18)),
            Show(EnemyFrameId.TorizoFacingRightWalkingRightLegMovingFourth, 6),
            Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: P.FacingRightJumpBackwardsLandLeftFootFwd0)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 19)),
            Show(EnemyFrameId.TorizoFacingRightWalkingRightLegMovingFifth, 6),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 10)),
        ],
        [P.FacingRightSpewingChozoOrbsLeftFootFwd0] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Gravity)),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootFwdFirst, 16),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootFwdSecond, 8),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootFwdThird, 8),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootFwdFourth, 8),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootFwdFifth, 8),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootFwdSixth, 16),
            Cue(new TorizoCue.Command(K.PlayShotSound)),
        ],
        [P.FacingRightSpewingChozoOrbsLeftFootFwd1] =
        [
            Cue(new TorizoCue.Command(K.SpawnBombTorizoChozoOrbs)),
            Wait(16),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootFwdFifth, 8),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootFwdFourth, 8),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootFwdThird, 8),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootFwdSecond, 72),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.Return)),
        ],
        [P.FacingRightSpewingChozoOrbsRightFootFwd0] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Gravity)),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootFwdFirst, 16),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootFwdSecond, 8),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootFwdThird, 8),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootFwdFourth, 8),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootFwdFifth, 8),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootFwdSixth, 16),
            Cue(new TorizoCue.Command(K.PlayShotSound)),
        ],
        [P.FacingRightSpewingChozoOrbsRightFootFwd1] =
        [
            Cue(new TorizoCue.Command(K.SpawnBombTorizoChozoOrbs)),
            Wait(16),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootFwdFifth, 8),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootFwdFourth, 8),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootFwdThird, 8),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootFwdSecond, 72),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.Return)),
        ],
        [P.FacingRightSonicBoomsLeftFootForward0] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Gravity)),
        ],
        [P.FacingRightSonicBoomsLeftFootForward1] =
        [
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootFwdFirst, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneSecond, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneThird, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneFourth, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneFifth, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneSixth, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneFifth, 2),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneFourth, 2),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnBombTorizoSonicBoom, Parameter: 0)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneThird, 2),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneSecond, 24),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootFwdFirst, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneSeventh, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneEighth, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneNinth, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneTenth, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneEleventh, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneTenth, 2),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneNinth, 2),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnBombTorizoSonicBoom, Parameter: 1)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneEighth, 2),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneSeventh, 56),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.Return)),
        ],
        [P.FacingRightSonicBoomsRightFootForward0] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Gravity)),
        ],
        [P.FacingRightSonicBoomsRightFootForward1] =
        [
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootFwdFirst, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneSecond, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneThird, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneFourth, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneFifth, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneSixth, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneFifth, 2),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneFourth, 2),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnBombTorizoSonicBoom, Parameter: 0)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneThird, 2),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneSecond, 24),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootFwdFirst, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneSeventh, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneEighth, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneNinth, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneTenth, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneEleventh, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneTenth, 2),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneNinth, 2),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnBombTorizoSonicBoom, Parameter: 1)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneEighth, 2),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneSeventh, 56),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.Return)),
        ],
        [P.FacingRightExplosiveSwipeLeftFootForward] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Gravity)),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootFwdFirst, 3),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneSecond, 3),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneThird, 3),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneFourth, 3),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneFifth, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 0)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneSixth, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 1)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneFifth, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 2)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneFourth, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 3)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneThird, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 4)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneSecond, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 5)),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootFwdFirst, 3),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneSeventh, 3),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneEighth, 3),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneNinth, 3),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneTenth, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 6)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneEleventh, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 7)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneTenth, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 8)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneNinth, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 9)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneEighth, 1),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneSeventh, 16),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.Return)),
        ],
        [P.FacingRightExplosiveSwipeRightFootForward] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Gravity)),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootFwdFirst, 3),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneSecond, 3),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneThird, 3),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneFourth, 3),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneFifth, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 0)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneSixth, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 1)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneFifth, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 2)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneFourth, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 3)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneThird, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 4)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneSecond, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 5)),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootFwdFirst, 3),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneSeventh, 3),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneEighth, 3),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneNinth, 3),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneTenth, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 6)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneEleventh, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 7)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneTenth, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 8)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneNinth, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 9)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneEighth, 1),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneSeventh, 16),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.Return)),
        ],
        [P.FacingRightJumpingForwards0] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Jump)),
            Cue(new TorizoCue.Branch(TorizoBranchKind.Link, Target: P.FacingRightFalling0)),
            Show(EnemyFrameId.TorizoFacingRightJumpingForwardsFirst, 5),
            Show(EnemyFrameId.TorizoFacingRightJumpingForwardsSecond, 5),
        ],
        [P.FacingRightJumpingForwards1] =
        [
            Show(EnemyFrameId.TorizoFacingRightJumpingForwardsThird, 1),
            Cue(new TorizoCue.Branch(TorizoBranchKind.GoToIfRising, Target: P.FacingRightJumpingForwards1)),
        ],
        [P.FacingRightFalling0] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Jump)),
            Cue(new TorizoCue.Branch(TorizoBranchKind.Link, Target: P.FacingRightFalling2)),
        ],
        [P.FacingRightFalling1] =
        [
            Show(EnemyFrameId.TorizoFacingRightJumpingForwardsSecond, 5),
        ],
        [P.FacingRightFalling2] =
        [
            Cue(new TorizoCue.Command(K.PlayFootstepSound)),
            Cue(new TorizoCue.Command(K.SpawnLandingDust)),
            Cue(new TorizoCue.Choice(TorizoChoiceKind.GoToIfFaceBlownUpElseIfGolden, Target: P.FacingRightFacelessWalkingRightLegMoving, Alternate: P.GoldenWalkingRightRightLegMoving)),
        ],
        [P.FacingRightJumpBackwardLandRightFootFwd0] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Jump)),
            Cue(new TorizoCue.Branch(TorizoBranchKind.Link, Target: P.FacingRightJumpBackwardLandRightFootFwd2)),
            Show(EnemyFrameId.TorizoFacingRightJumpingForwardsFirst, 5),
            Show(EnemyFrameId.TorizoFacingRightJumpingForwardsSecond, 5),
        ],
        [P.FacingRightJumpBackwardLandRightFootFwd1] =
        [
            Show(EnemyFrameId.TorizoFacingRightJumpingForwardsThird, 1),
            Cue(new TorizoCue.Branch(TorizoBranchKind.GoToIfRising, Target: P.FacingRightJumpBackwardLandRightFootFwd1)),
        ],
        [P.FacingRightJumpBackwardLandRightFootFwd2] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Jump)),
            Cue(new TorizoCue.Branch(TorizoBranchKind.Link, Target: P.FacingRightJumpBackwardLandRightFootFwd4)),
        ],
        [P.FacingRightJumpBackwardLandRightFootFwd3] =
        [
            Show(EnemyFrameId.TorizoFacingRightJumpingForwardsSecond, 5),
        ],
        [P.FacingRightJumpBackwardLandRightFootFwd4] =
        [
            Cue(new TorizoCue.Command(K.PlayFootstepSound)),
            Cue(new TorizoCue.Command(K.SpawnLandingDust)),
            Cue(new TorizoCue.Choice(TorizoChoiceKind.GoToIfFaceBlownUpElseIfGolden, Target: P.FacingRightFacelessWalkingLeftLegMoving, Alternate: P.GoldenLandedFromBackwardsJumpFacingRightRightFootFwd)),
            Cue(new TorizoCue.Choice(TorizoChoiceKind.CallBombTorizoAttack, Target: P.FacingRightSpewingChozoOrbsRightFootFwd0, Alternate: P.FacingRightSonicBoomsRightFootForward0)),
        ],
        [P.FacingRightJumpBackwardsLandLeftFootFwd0] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Jump)),
            Cue(new TorizoCue.Branch(TorizoBranchKind.Link, Target: P.FacingRightJumpBackwardsLandLeftFootFwd2)),
            Show(EnemyFrameId.TorizoFacingRightJumpingForwardsFirst, 5),
            Show(EnemyFrameId.TorizoFacingRightJumpingForwardsSecond, 5),
        ],
        [P.FacingRightJumpBackwardsLandLeftFootFwd1] =
        [
            Show(EnemyFrameId.TorizoFacingRightJumpingForwardsThird, 1),
            Cue(new TorizoCue.Branch(TorizoBranchKind.GoToIfRising, Target: P.FacingRightJumpBackwardsLandLeftFootFwd1)),
        ],
        [P.FacingRightJumpBackwardsLandLeftFootFwd2] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Jump)),
            Cue(new TorizoCue.Branch(TorizoBranchKind.Link, Target: P.FacingRightJumpBackwardsLandLeftFootFwd4)),
        ],
        [P.FacingRightJumpBackwardsLandLeftFootFwd3] =
        [
            Show(EnemyFrameId.TorizoFacingRightJumpingForwardsSecond, 5),
        ],
        [P.FacingRightJumpBackwardsLandLeftFootFwd4] =
        [
            Cue(new TorizoCue.Command(K.PlayFootstepSound)),
            Cue(new TorizoCue.Command(K.SpawnLandingDust)),
            Cue(new TorizoCue.Choice(TorizoChoiceKind.GoToIfFaceBlownUpElseIfGolden, Target: P.FacingRightFacelessWalkingRightLegMoving, Alternate: P.GoldenLandedFromBackwardsJumpFacingRightLeftFootFwd)),
            Cue(new TorizoCue.Choice(TorizoChoiceKind.CallBombTorizoAttack, Target: P.FacingRightSpewingChozoOrbsLeftFootFwd0, Alternate: P.FacingRightSonicBoomsLeftFootForward0)),
        ],
        [P.FacingRightFacelessTurningRight] =
        [
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.Falling)),
            Cue(new TorizoCue.Command(K.SetTurningAround)),
            Show(EnemyFrameId.TorizoFacingLeftTurningLeft, 8),
        ],
        [P.FacingRightFacelessWalkingLeftLegMoving] =
        [
            Cue(new TorizoCue.Command(K.SteppedRightWithRightFoot)),
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.Active)),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.SpawnLowHealthDrool)),
            Cue(new TorizoCue.Command(K.PlayFootstepSound)),
            Show(EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingFirst, 1),
            Cue(new TorizoCue.Branch(TorizoBranchKind.CallIfSamusCloseBehind, Target: P.FacingRightExplosiveSwipeRightFootForward)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 11)),
            Show(EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingSecond, 5),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 12)),
            Show(EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingThird, 5),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 13)),
            Show(EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingFourth, 5),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 14)),
            Show(EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingFifth, 5),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 15)),
        ],
        [P.FacingRightFacelessWalkingRightLegMoving] =
        [
            Cue(new TorizoCue.Command(K.SteppedRightWithLeftFoot)),
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.Active)),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.SpawnLowHealthDrool)),
            Cue(new TorizoCue.Command(K.PlayFootstepSound)),
            Show(EnemyFrameId.TorizoFacingRightWalkingRightLegMovingFirst, 1),
            Cue(new TorizoCue.Branch(TorizoBranchKind.CallIfSamusCloseBehind, Target: P.FacingRightExplosiveSwipeLeftFootForward)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 16)),
            Show(EnemyFrameId.TorizoFacingRightWalkingRightLegMovingSecond, 5),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 17)),
            Show(EnemyFrameId.TorizoFacingRightWalkingRightLegMovingThird, 5),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 18)),
            Show(EnemyFrameId.TorizoFacingRightWalkingRightLegMovingFourth, 5),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 19)),
            Show(EnemyFrameId.TorizoFacingRightWalkingRightLegMovingFifth, 5),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 10)),
        ],
    };
}
