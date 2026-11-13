using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Bosses.Torizo.TorizoCueKind;
using P = SuperMetroid.Core.Bosses.Torizo.TorizoBodySegment;
using static SuperMetroid.Core.Bosses.Torizo.TorizoAnimationSteps;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Bomb Torizo's animations while it faces left: turning, walking, attacks, jumps and falls, with its face and without.</summary>
internal static class TorizoFacingLeftAnimations
{
    /// <summary>Each segment and its beats.</summary>
    internal static Dictionary<TorizoBodySegment, AnimationBeat<EnemyFrameId, TorizoCue>[]> Segments { get; } = new()
    {
        [P.FacingLeftTurningLeft] =
        [
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.Falling)),
            Cue(new TorizoCue.Signal(K.SetTurningAround)),
            Show(EnemyFrameId.TorizoFacingLeftTurningLeft, 24),
        ],
        [P.FacingLeftWalkingRightLegMoving] =
        [
            Cue(new TorizoCue.Signal(K.SteppedLeftWithLeftFoot)),
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.Active)),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Signal(K.SpawnLowHealthDrool)),
            Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
            Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingFirst, 10),
            Cue(new TorizoCue.Branch(TorizoBranchKind.CallIfSamusCloseBehind, Target: P.FacingLeftExplosiveSwipeLeftFootForward)),
            Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: P.FacingLeftJumpingBackwardLandLeftFootFwd0)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 1)),
            Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingSecond, 6),
            Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: P.FacingLeftJumpingBackwardLandLeftFootFwd0)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 2)),
            Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingThird, 6),
            Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: P.FacingLeftJumpingBackwardLandLeftFootFwd0)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 3)),
            Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingFourth, 6),
            Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: P.FacingLeftJumpingBackwardLandLeftFootFwd0)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 4)),
            Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingFifth, 6),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 5)),
        ],
        [P.FacingLeftWalkingLeftLegMoving] =
        [
            Cue(new TorizoCue.Signal(K.SteppedLeftWithRightFoot)),
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.Active)),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Signal(K.SpawnLowHealthDrool)),
            Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
            Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingFirst, 10),
            Cue(new TorizoCue.Branch(TorizoBranchKind.CallIfSamusCloseBehind, Target: P.FacingLeftExplosiveSwipeRightFootForward)),
            Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: P.FacingLeftJumpingBackwardRightFootFwd0)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 6)),
            Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingSecond, 6),
            Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: P.FacingLeftJumpingBackwardRightFootFwd0)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 7)),
            Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingThird, 6),
            Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: P.FacingLeftJumpingBackwardRightFootFwd0)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 8)),
            Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingFourth, 6),
            Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: P.FacingLeftJumpingBackwardRightFootFwd0)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 9)),
            Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingFifth, 6),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 0)),
        ],
        [P.FacingLeftSpewingChozoOrbsRightFootFwd0] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Gravity)),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootForwardFirst, 16),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootForwardSecond, 8),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootForwardThird, 8),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootForwardFourth, 8),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootForwardFifth, 8),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootForwardSixth, 16),
            Cue(new TorizoCue.Signal(K.PlayShotSound)),
        ],
        [P.FacingLeftSpewingChozoOrbsRightFootFwd1] =
        [
            Cue(new TorizoCue.Signal(K.SpawnBombTorizoChozoOrbs)),
            Wait(16),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootForwardFifth, 8),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootForwardFourth, 8),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootForwardThird, 8),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootForwardSecond, 72),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Signal(K.Return)),
        ],
        [P.FacingLeftSpewingChozoOrbsLeftFootFwd0] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Gravity)),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootForwardFirst, 16),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootForwardSecond, 8),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootForwardThird, 8),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootForwardFourth, 8),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootForwardFifth, 8),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootForwardSixth, 16),
            Cue(new TorizoCue.Signal(K.PlayShotSound)),
        ],
        [P.FacingLeftSpewingChozoOrbsLeftFootFwd1] =
        [
            Cue(new TorizoCue.Signal(K.SpawnBombTorizoChozoOrbs)),
            Wait(16),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootForwardFifth, 8),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootForwardFourth, 8),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootForwardThird, 8),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootForwardSecond, 72),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Signal(K.Return)),
        ],
        [P.FacingLeftSonicBoomsRightFootForward0] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Gravity)),
        ],
        [P.FacingLeftSonicBoomsRightFootForward1] =
        [
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootForwardFirst, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardSecond, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardThird, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardFourth, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardFifth, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardSixth, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardFifth, 2),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardFourth, 2),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnBombTorizoSonicBoom, Parameter: 0)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardThird, 2),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardSecond, 24),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootForwardFirst, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardSeventh, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardEighth, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardNinth, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardTenth, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardEleventh, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardTenth, 2),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardNinth, 2),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnBombTorizoSonicBoom, Parameter: 1)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardEighth, 2),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardSeventh, 56),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Signal(K.Return)),
        ],
        [P.FacingLeftSonicBoomsLeftFootForward0] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Gravity)),
        ],
        [P.FacingLeftSonicBoomsLeftFootForward1] =
        [
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootForwardFirst, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardSecond, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardThird, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardFourth, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardFifth, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardSixth, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardFifth, 2),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardFourth, 2),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnBombTorizoSonicBoom, Parameter: 0)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardThird, 2),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardSecond, 24),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootForwardFirst, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardSeventh, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardEighth, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardNinth, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardTenth, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardEleventh, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardTenth, 2),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardNinth, 2),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnBombTorizoSonicBoom, Parameter: 1)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardEighth, 2),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardSeventh, 56),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Signal(K.Return)),
        ],
        [P.FacingLeftExplosiveSwipeRightFootForward] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Gravity)),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootForwardFirst, 3),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardSecond, 3),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardThird, 3),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardFourth, 3),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardFifth, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 0)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardSixth, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 1)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardFifth, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 2)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardFourth, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 3)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardThird, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 4)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardSecond, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 5)),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootForwardFirst, 3),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardSeventh, 3),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardEighth, 3),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardNinth, 3),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardTenth, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 6)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardEleventh, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 7)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardTenth, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 8)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardNinth, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 9)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardEighth, 1),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardSeventh, 16),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Signal(K.Return)),
        ],
        [P.FacingLeftExplosiveSwipeLeftFootForward] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Gravity)),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootForwardFirst, 3),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardSecond, 3),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardThird, 3),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardFourth, 3),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardFifth, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 0)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardSixth, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 1)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardFifth, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 2)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardFourth, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 3)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardThird, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 4)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardSecond, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 5)),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootForwardFirst, 3),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardSeventh, 3),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardEighth, 3),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardNinth, 3),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardTenth, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 6)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardEleventh, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 7)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardTenth, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 8)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardNinth, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 9)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardEighth, 1),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardSeventh, 16),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Signal(K.Return)),
        ],
        [P.FacingLeftJumpingForwards0] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Jump)),
            Cue(new TorizoCue.Branch(TorizoBranchKind.Link, Target: P.FacingLeftFalling0)),
            Show(EnemyFrameId.TorizoFacingLeftJumpingForwardsFirst, 5),
            Show(EnemyFrameId.TorizoFallingLeft, 5),
        ],
        [P.FacingLeftJumpingForwards1] =
        [
            Show(EnemyFrameId.TorizoFacingLeftJumpingForwardsThird, 1),
            Cue(new TorizoCue.Branch(TorizoBranchKind.GoToIfRising, Target: P.FacingLeftJumpingForwards1)),
        ],
        [P.FacingLeftFalling0] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Jump)),
            Cue(new TorizoCue.Branch(TorizoBranchKind.Link, Target: P.FacingLeftFalling2)),
        ],
        [P.FacingLeftFalling1] =
        [
            Show(EnemyFrameId.TorizoFallingLeft, 5),
        ],
        [P.FacingLeftFalling2] =
        [
            Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
            Cue(new TorizoCue.Signal(K.SpawnLandingDust)),
            Cue(new TorizoCue.Choice(TorizoChoiceKind.GoToIfFaceBlownUpElseIfGolden, Target: P.FacingLeftFacelessWalkingLeftLegMoving, Alternate: P.GoldenWalkingLeftLeftLegMoving)),
        ],
        [P.FacingLeftJumpingBackwardLandLeftFootFwd0] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Jump)),
            Cue(new TorizoCue.Branch(TorizoBranchKind.Link, Target: P.FacingLeftJumpingBackwardLandLeftFootFwd2)),
            Show(EnemyFrameId.TorizoFacingLeftJumpingForwardsFirst, 5),
            Show(EnemyFrameId.TorizoFallingLeft, 5),
        ],
        [P.FacingLeftJumpingBackwardLandLeftFootFwd1] =
        [
            Show(EnemyFrameId.TorizoFacingLeftJumpingForwardsThird, 1),
            Cue(new TorizoCue.Branch(TorizoBranchKind.GoToIfRising, Target: P.FacingLeftJumpingBackwardLandLeftFootFwd1)),
        ],
        [P.FacingLeftJumpingBackwardLandLeftFootFwd2] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Jump)),
            Cue(new TorizoCue.Branch(TorizoBranchKind.Link, Target: P.FacingLeftJumpingBackwardLandLeftFootFwd4)),
        ],
        [P.FacingLeftJumpingBackwardLandLeftFootFwd3] =
        [
            Show(EnemyFrameId.TorizoFallingLeft, 5),
        ],
        [P.FacingLeftJumpingBackwardLandLeftFootFwd4] =
        [
            Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
            Cue(new TorizoCue.Signal(K.SpawnLandingDust)),
            Cue(new TorizoCue.Choice(TorizoChoiceKind.GoToIfFaceBlownUpElseIfGolden, Target: P.FacingLeftFacelessWalkingRightLegMoving, Alternate: P.GoldenLandedFromBackwardsJumpFacingLeftLeftFootFwd)),
            Cue(new TorizoCue.Choice(TorizoChoiceKind.CallBombTorizoAttack, Target: P.FacingLeftSpewingChozoOrbsLeftFootFwd0, Alternate: P.FacingLeftSonicBoomsLeftFootForward0)),
        ],
        [P.FacingLeftJumpingBackwardRightFootFwd0] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Jump)),
            Cue(new TorizoCue.Branch(TorizoBranchKind.Link, Target: P.FacingLeftJumpingBackwardRightFootFwd2)),
            Show(EnemyFrameId.TorizoFacingLeftJumpingForwardsFirst, 5),
            Show(EnemyFrameId.TorizoFallingLeft, 5),
        ],
        [P.FacingLeftJumpingBackwardRightFootFwd1] =
        [
            Show(EnemyFrameId.TorizoFacingLeftJumpingForwardsThird, 1),
            Cue(new TorizoCue.Branch(TorizoBranchKind.GoToIfRising, Target: P.FacingLeftJumpingBackwardRightFootFwd1)),
        ],
        [P.FacingLeftJumpingBackwardRightFootFwd2] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Jump)),
            Cue(new TorizoCue.Branch(TorizoBranchKind.Link, Target: P.FacingLeftJumpingBackwardRightFootFwd4)),
        ],
        [P.FacingLeftJumpingBackwardRightFootFwd3] =
        [
            Show(EnemyFrameId.TorizoFallingLeft, 5),
        ],
        [P.FacingLeftJumpingBackwardRightFootFwd4] =
        [
            Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
            Cue(new TorizoCue.Signal(K.SpawnLandingDust)),
            Cue(new TorizoCue.Choice(TorizoChoiceKind.GoToIfFaceBlownUpElseIfGolden, Target: P.FacingLeftFacelessWalkingLeftLegMoving, Alternate: P.GoldenLandedFromBackwardsJumpFacingLeftRightFootFwd)),
            Cue(new TorizoCue.Choice(TorizoChoiceKind.CallBombTorizoAttack, Target: P.FacingLeftSpewingChozoOrbsRightFootFwd0, Alternate: P.FacingLeftSonicBoomsRightFootForward0)),
        ],
        [P.FacingLeftFacelessTurningLeft] =
        [
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.Falling)),
            Cue(new TorizoCue.Signal(K.SetTurningAround)),
            Show(EnemyFrameId.TorizoFacingLeftTurningLeft, 8),
        ],
        [P.FacingLeftFacelessWalkingRightLegMoving] =
        [
            Cue(new TorizoCue.Signal(K.SteppedLeftWithLeftFoot)),
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.Active)),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Signal(K.SpawnLowHealthDrool)),
            Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
            Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingFirst, 1),
            Cue(new TorizoCue.Branch(TorizoBranchKind.CallIfSamusCloseBehind, Target: P.FacingLeftExplosiveSwipeLeftFootForward)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 1)),
            Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingSecond, 5),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 2)),
            Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingThird, 5),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 3)),
            Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingFourth, 5),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 4)),
            Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingFifth, 5),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 5)),
        ],
        [P.FacingLeftFacelessWalkingLeftLegMoving] =
        [
            Cue(new TorizoCue.Signal(K.SteppedLeftWithRightFoot)),
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.Active)),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Signal(K.SpawnLowHealthDrool)),
            Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
            Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingFirst, 1),
            Cue(new TorizoCue.Branch(TorizoBranchKind.CallIfSamusCloseBehind, Target: P.FacingLeftExplosiveSwipeRightFootForward)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 6)),
            Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingSecond, 5),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 7)),
            Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingThird, 5),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 8)),
            Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingFourth, 5),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 9)),
            Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingFifth, 5),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 0)),
        ],
        [P.FacingLeftFacelessWalkingLeftLegMovingTrailing] =
        [
            Cue(new TorizoCue.Signal(K.SteppedRightWithLeftFoot)),
            Show(EnemyFrameId.BombTorizoDormant, 1),
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.WaitForHandTrigger)),
        ],
        [P.FacingLeftFacelessWalkingLeftLegMovingTrailingResumed] =
        [
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.Falling)),
            Show(EnemyFrameId.TorizoFacingRightSeated, 32),
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, Step: 8)),
            Show(EnemyFrameId.TorizoFacingRightStandingUpFirst, 16),
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, Step: 9)),
            Show(EnemyFrameId.TorizoFacingRightStandingUpSecond, 8),
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, Step: 10)),
            Show(EnemyFrameId.TorizoFacingRightStandingUpThird, 8),
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, Step: 11)),
            Show(EnemyFrameId.TorizoFacingRightStandingUpFourth, 8),
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, Step: 12)),
            Show(EnemyFrameId.TorizoFacingRightStandingUpFifth, 8),
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, Step: 13)),
            Show(EnemyFrameId.TorizoFacingRightStandingUpSixth, 8),
        ],
    };
}
