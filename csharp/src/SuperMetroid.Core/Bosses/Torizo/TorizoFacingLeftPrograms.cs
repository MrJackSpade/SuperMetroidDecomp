using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Bosses.Torizo.TorizoCueKind;
using P = SuperMetroid.Core.Bosses.Torizo.TorizoBodyProgram;
using static SuperMetroid.Core.Bosses.Torizo.TorizoProgramSteps;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Bomb Torizo's programs while it faces left: turning, walking, attacks, jumps and falls, with its face and without.</summary>
internal static class TorizoFacingLeftPrograms
{
    /// <summary>Each segment and its beats.</summary>
    internal static Dictionary<TorizoBodyProgram, AnimationBeat<EnemyFrameId, TorizoCue>[]> Segments { get; } = new()
    {
        [P.FacingLeftTurningLeft] =
        [
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.Falling)),
            Cue(new TorizoCue.Command(K.SetTurningAround)),
            Show(EnemyFrameId.TorizoFacingLeftTurningLeft, 24),
        ],
        [P.FacingLeftWalkingRightLegMoving] =
        [
            Cue(new TorizoCue.Command(K.SteppedLeftWithLeftFoot)),
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.Active)),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.SpawnLowHealthDrool)),
            Cue(new TorizoCue.Command(K.PlayFootstepSound)),
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
            Cue(new TorizoCue.Command(K.SteppedLeftWithRightFoot)),
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.Active)),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.SpawnLowHealthDrool)),
            Cue(new TorizoCue.Command(K.PlayFootstepSound)),
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
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootFwdFirst, 16),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootFwdSecond, 8),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootFwdThird, 8),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootFwdFourth, 8),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootFwdFifth, 8),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootFwdSixth, 16),
            Cue(new TorizoCue.Command(K.PlayShotSound)),
        ],
        [P.FacingLeftSpewingChozoOrbsRightFootFwd1] =
        [
            Cue(new TorizoCue.Command(K.SpawnBombTorizoChozoOrbs)),
            Wait(16),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootFwdFifth, 8),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootFwdFourth, 8),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootFwdThird, 8),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootFwdSecond, 72),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.Return)),
        ],
        [P.FacingLeftSpewingChozoOrbsLeftFootFwd0] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Gravity)),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootFwdFirst, 16),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootFwdSecond, 8),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootFwdThird, 8),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootFwdFourth, 8),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootFwdFifth, 8),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootFwdSixth, 16),
            Cue(new TorizoCue.Command(K.PlayShotSound)),
        ],
        [P.FacingLeftSpewingChozoOrbsLeftFootFwd1] =
        [
            Cue(new TorizoCue.Command(K.SpawnBombTorizoChozoOrbs)),
            Wait(16),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootFwdFifth, 8),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootFwdFourth, 8),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootFwdThird, 8),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootFwdSecond, 72),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.Return)),
        ],
        [P.FacingLeftSonicBoomsRightFootForward0] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Gravity)),
        ],
        [P.FacingLeftSonicBoomsRightFootForward1] =
        [
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootFwdFirst, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneSecond, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneThird, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneFourth, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneFifth, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneSixth, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneFifth, 2),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneFourth, 2),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnBombTorizoSonicBoom, Parameter: 0)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneThird, 2),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneSecond, 24),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootFwdFirst, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneSeventh, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneEighth, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneNinth, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneTenth, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneEleventh, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneTenth, 2),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneNinth, 2),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnBombTorizoSonicBoom, Parameter: 1)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneEighth, 2),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneSeventh, 56),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.Return)),
        ],
        [P.FacingLeftSonicBoomsLeftFootForward0] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Gravity)),
        ],
        [P.FacingLeftSonicBoomsLeftFootForward1] =
        [
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootFwdFirst, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneSecond, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneThird, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneFourth, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneFifth, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneSixth, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneFifth, 2),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneFourth, 2),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnBombTorizoSonicBoom, Parameter: 0)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneThird, 2),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneSecond, 24),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootFwdFirst, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneSeventh, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneEighth, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneNinth, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneTenth, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneEleventh, 6),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneTenth, 2),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneNinth, 2),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnBombTorizoSonicBoom, Parameter: 1)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneEighth, 2),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneSeventh, 56),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.Return)),
        ],
        [P.FacingLeftExplosiveSwipeRightFootForward] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Gravity)),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootFwdFirst, 3),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneSecond, 3),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneThird, 3),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneFourth, 3),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneFifth, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 0)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneSixth, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 1)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneFifth, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 2)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneFourth, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 3)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneThird, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 4)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneSecond, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 5)),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootFwdFirst, 3),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneSeventh, 3),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneEighth, 3),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneNinth, 3),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneTenth, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 6)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneEleventh, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 7)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneTenth, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 8)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneNinth, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 9)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneEighth, 1),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneSeventh, 16),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.Return)),
        ],
        [P.FacingLeftExplosiveSwipeLeftFootForward] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Gravity)),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootFwdFirst, 3),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneSecond, 3),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneThird, 3),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneFourth, 3),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneFifth, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 0)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneSixth, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 1)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneFifth, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 2)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneFourth, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 3)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneThird, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 4)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneSecond, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 5)),
            Show(EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootFwdFirst, 3),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneSeventh, 3),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneEighth, 3),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneNinth, 3),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneTenth, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 6)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneEleventh, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 7)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneTenth, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 8)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneNinth, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 9)),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneEighth, 1),
            Show(EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneSeventh, 16),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.Return)),
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
            Cue(new TorizoCue.Command(K.PlayFootstepSound)),
            Cue(new TorizoCue.Command(K.SpawnLandingDust)),
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
            Cue(new TorizoCue.Command(K.PlayFootstepSound)),
            Cue(new TorizoCue.Command(K.SpawnLandingDust)),
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
            Cue(new TorizoCue.Command(K.PlayFootstepSound)),
            Cue(new TorizoCue.Command(K.SpawnLandingDust)),
            Cue(new TorizoCue.Choice(TorizoChoiceKind.GoToIfFaceBlownUpElseIfGolden, Target: P.FacingLeftFacelessWalkingLeftLegMoving, Alternate: P.GoldenLandedFromBackwardsJumpFacingLeftRightFootFwd)),
            Cue(new TorizoCue.Choice(TorizoChoiceKind.CallBombTorizoAttack, Target: P.FacingLeftSpewingChozoOrbsRightFootFwd0, Alternate: P.FacingLeftSonicBoomsRightFootForward0)),
        ],
        [P.FacingLeftFacelessTurningLeft] =
        [
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.Falling)),
            Cue(new TorizoCue.Command(K.SetTurningAround)),
            Show(EnemyFrameId.TorizoFacingLeftTurningLeft, 8),
        ],
        [P.FacingLeftFacelessWalkingRightLegMoving] =
        [
            Cue(new TorizoCue.Command(K.SteppedLeftWithLeftFoot)),
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.Active)),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.SpawnLowHealthDrool)),
            Cue(new TorizoCue.Command(K.PlayFootstepSound)),
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
            Cue(new TorizoCue.Command(K.SteppedLeftWithRightFoot)),
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.Active)),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.SpawnLowHealthDrool)),
            Cue(new TorizoCue.Command(K.PlayFootstepSound)),
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
            Cue(new TorizoCue.Command(K.SteppedRightWithLeftFoot)),
            Show(EnemyFrameId.BombTorizoDormant, 1),
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.WaitForHandTrigger)),
        ],
        [P.FacingLeftFacelessWalkingLeftLegMovingTrailingResumed] =
        [
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.Falling)),
            Show(EnemyFrameId.TorizoFacingLeftFacelessWalkingLeftLegMovingTrailingResumedFirst, 32),
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, Step: 8)),
            Show(EnemyFrameId.TorizoFacingLeftFacelessWalkingLeftLegMovingTrailingResumedSecond, 16),
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, Step: 9)),
            Show(EnemyFrameId.TorizoFacingLeftFacelessWalkingLeftLegMovingTrailingResumedThird, 8),
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, Step: 10)),
            Show(EnemyFrameId.TorizoFacingLeftFacelessWalkingLeftLegMovingTrailingResumedFourth, 8),
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, Step: 11)),
            Show(EnemyFrameId.TorizoFacingLeftFacelessWalkingLeftLegMovingTrailingResumedFifth, 8),
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, Step: 12)),
            Show(EnemyFrameId.TorizoFacingLeftFacelessWalkingLeftLegMovingTrailingResumedSixth, 8),
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, Step: 13)),
            Show(EnemyFrameId.TorizoFacingLeftFacelessWalkingLeftLegMovingTrailingResumedSeventh, 8),
        ],
    };
}
