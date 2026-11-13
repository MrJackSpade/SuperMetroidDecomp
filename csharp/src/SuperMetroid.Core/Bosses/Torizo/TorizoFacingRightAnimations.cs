using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Bosses.Torizo.TorizoCueKind;
using P = SuperMetroid.Core.Bosses.Torizo.TorizoBodySegment;
using static SuperMetroid.Core.Bosses.Torizo.TorizoAnimationSteps;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Bomb Torizo's animations while it faces right: turning, walking, attacks, jumps and falls, with its face and without.</summary>
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
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootForwardFirst, 16),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootForwardSecond, 8),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootForwardThird, 8),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootForwardFourth, 8),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootForwardFifth, 8),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootForwardSixth, 16),
            Cue(new TorizoCue.Command(K.PlayShotSound)),
        ],
        [P.FacingRightSpewingChozoOrbsLeftFootFwd1] =
        [
            Cue(new TorizoCue.Command(K.SpawnBombTorizoChozoOrbs)),
            Wait(16),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootForwardFifth, 8),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootForwardFourth, 8),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootForwardThird, 8),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootForwardSecond, 72),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.Return)),
        ],
        [P.FacingRightSpewingChozoOrbsRightFootFwd0] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Gravity)),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootForwardFirst, 16),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootForwardSecond, 8),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootForwardThird, 8),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootForwardFourth, 8),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootForwardFifth, 8),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootForwardSixth, 16),
            Cue(new TorizoCue.Command(K.PlayShotSound)),
        ],
        [P.FacingRightSpewingChozoOrbsRightFootFwd1] =
        [
            Cue(new TorizoCue.Command(K.SpawnBombTorizoChozoOrbs)),
            Wait(16),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootForwardFifth, 8),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootForwardFourth, 8),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootForwardThird, 8),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootForwardSecond, 72),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.Return)),
        ],
        [P.FacingRightSonicBoomsLeftFootForward0] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Gravity)),
        ],
        [P.FacingRightSonicBoomsLeftFootForward1] =
        [
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootForwardFirst, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardSecond, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardThird, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardFourth, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardFifth, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardSixth, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardFifth, 2),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardFourth, 2),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnBombTorizoSonicBoom, Parameter: 0)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardThird, 2),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardSecond, 24),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootForwardFirst, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardSeventh, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardEighth, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardNinth, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardTenth, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardEleventh, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardTenth, 2),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardNinth, 2),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnBombTorizoSonicBoom, Parameter: 1)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardEighth, 2),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardSeventh, 56),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.Return)),
        ],
        [P.FacingRightSonicBoomsRightFootForward0] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Gravity)),
        ],
        [P.FacingRightSonicBoomsRightFootForward1] =
        [
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootForwardFirst, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardSecond, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardThird, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardFourth, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardFifth, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardSixth, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardFifth, 2),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardFourth, 2),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnBombTorizoSonicBoom, Parameter: 0)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardThird, 2),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardSecond, 24),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootForwardFirst, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardSeventh, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardEighth, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardNinth, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardTenth, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardEleventh, 6),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardTenth, 2),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardNinth, 2),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnBombTorizoSonicBoom, Parameter: 1)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardEighth, 2),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardSeventh, 56),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.Return)),
        ],
        [P.FacingRightExplosiveSwipeLeftFootForward] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Gravity)),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootForwardFirst, 3),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardSecond, 3),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardThird, 3),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardFourth, 3),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardFifth, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 0)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardSixth, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 1)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardFifth, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 2)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardFourth, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 3)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardThird, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 4)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardSecond, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 5)),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootForwardFirst, 3),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardSeventh, 3),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardEighth, 3),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardNinth, 3),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardTenth, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 6)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardEleventh, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 7)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardTenth, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 8)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardNinth, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 9)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardEighth, 1),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardSeventh, 16),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.Return)),
        ],
        [P.FacingRightExplosiveSwipeRightFootForward] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Gravity)),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootForwardFirst, 3),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardSecond, 3),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardThird, 3),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardFourth, 3),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardFifth, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 0)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardSixth, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 1)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardFifth, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 2)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardFourth, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 3)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardThird, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 4)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardSecond, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 5)),
            Show(EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootForwardFirst, 3),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardSeventh, 3),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardEighth, 3),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardNinth, 3),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardTenth, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 6)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardEleventh, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 7)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardTenth, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 8)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardNinth, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 9)),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardEighth, 1),
            Show(EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardSeventh, 16),
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
