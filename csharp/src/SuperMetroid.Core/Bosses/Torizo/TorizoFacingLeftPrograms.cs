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
            Show(EnemyFrameId.GoldenTorizoRightward0, 24),
        ],
        [P.FacingLeftWalkingRightLegMoving] =
        [
            Cue(new TorizoCue.Command(K.SteppedLeftWithLeftFoot)),
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.Active)),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.SpawnLowHealthDrool)),
            Cue(new TorizoCue.Command(K.PlayFootstepSound)),
            Show(EnemyFrameId.GoldenTorizoWalkLeft0, 10),
            Cue(new TorizoCue.Branch(TorizoBranchKind.CallIfSamusCloseBehind, Target: P.FacingLeftExplosiveSwipeLeftFootForward)),
            Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: P.FacingLeftJumpingBackwardLandLeftFootFwd0)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 1)),
            Show(EnemyFrameId.GoldenTorizoWalkLeft1, 6),
            Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: P.FacingLeftJumpingBackwardLandLeftFootFwd0)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 2)),
            Show(EnemyFrameId.GoldenTorizoWalkLeft2, 6),
            Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: P.FacingLeftJumpingBackwardLandLeftFootFwd0)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 3)),
            Show(EnemyFrameId.GoldenTorizoWalkLeft3, 6),
            Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: P.FacingLeftJumpingBackwardLandLeftFootFwd0)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 4)),
            Show(EnemyFrameId.GoldenTorizoWalkLeft4, 6),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 5)),
        ],
        [P.FacingLeftWalkingLeftLegMoving] =
        [
            Cue(new TorizoCue.Command(K.SteppedLeftWithRightFoot)),
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.Active)),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.SpawnLowHealthDrool)),
            Cue(new TorizoCue.Command(K.PlayFootstepSound)),
            Show(EnemyFrameId.GoldenTorizoWalkLeft5, 10),
            Cue(new TorizoCue.Branch(TorizoBranchKind.CallIfSamusCloseBehind, Target: P.FacingLeftExplosiveSwipeRightFootForward)),
            Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: P.FacingLeftJumpingBackwardRightFootFwd0)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 6)),
            Show(EnemyFrameId.GoldenTorizoWalkLeft6, 6),
            Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: P.FacingLeftJumpingBackwardRightFootFwd0)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 7)),
            Show(EnemyFrameId.GoldenTorizoWalkLeft7, 6),
            Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: P.FacingLeftJumpingBackwardRightFootFwd0)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 8)),
            Show(EnemyFrameId.GoldenTorizoWalkLeft8, 6),
            Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: P.FacingLeftJumpingBackwardRightFootFwd0)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 9)),
            Show(EnemyFrameId.GoldenTorizoWalkLeft9, 6),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 0)),
        ],
        [P.FacingLeftSpewingChozoOrbsRightFootFwd0] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Gravity)),
            Show(EnemyFrameId.GoldenTorizoLeftOrb0, 16),
            Show(EnemyFrameId.GoldenTorizoLeftOrb1, 8),
            Show(EnemyFrameId.GoldenTorizoLeftOrb2, 8),
            Show(EnemyFrameId.GoldenTorizoLeftOrb3, 8),
            Show(EnemyFrameId.GoldenTorizoLeftOrb4, 8),
            Show(EnemyFrameId.GoldenTorizoLeftOrb5, 16),
            Cue(new TorizoCue.Command(K.PlayShotSound)),
        ],
        [P.FacingLeftSpewingChozoOrbsRightFootFwd1] =
        [
            Cue(new TorizoCue.Command(K.SpawnBombTorizoChozoOrbs)),
            Wait(16),
            Show(EnemyFrameId.GoldenTorizoLeftOrb4, 8),
            Show(EnemyFrameId.GoldenTorizoLeftOrb3, 8),
            Show(EnemyFrameId.GoldenTorizoLeftOrb2, 8),
            Show(EnemyFrameId.GoldenTorizoLeftOrb1, 72),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.Return)),
        ],
        [P.FacingLeftSpewingChozoOrbsLeftFootFwd0] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Gravity)),
            Show(EnemyFrameId.GoldenTorizoLeftOrb6, 16),
            Show(EnemyFrameId.GoldenTorizoLeftOrb7, 8),
            Show(EnemyFrameId.GoldenTorizoLeftOrb8, 8),
            Show(EnemyFrameId.GoldenTorizoLeftOrb9, 8),
            Show(EnemyFrameId.GoldenTorizoLeftOrb10, 8),
            Show(EnemyFrameId.GoldenTorizoLeftOrb11, 16),
            Cue(new TorizoCue.Command(K.PlayShotSound)),
        ],
        [P.FacingLeftSpewingChozoOrbsLeftFootFwd1] =
        [
            Cue(new TorizoCue.Command(K.SpawnBombTorizoChozoOrbs)),
            Wait(16),
            Show(EnemyFrameId.GoldenTorizoLeftOrb10, 8),
            Show(EnemyFrameId.GoldenTorizoLeftOrb9, 8),
            Show(EnemyFrameId.GoldenTorizoLeftOrb8, 8),
            Show(EnemyFrameId.GoldenTorizoLeftOrb7, 72),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.Return)),
        ],
        [P.FacingLeftSonicBoomsRightFootForward0] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Gravity)),
        ],
        [P.FacingLeftSonicBoomsRightFootForward1] =
        [
            Show(EnemyFrameId.GoldenTorizoLeftOrb0, 6),
            Show(EnemyFrameId.TorizoCombat0, 6),
            Show(EnemyFrameId.TorizoCombat1, 6),
            Show(EnemyFrameId.TorizoCombat2, 6),
            Show(EnemyFrameId.TorizoCombat3, 6),
            Show(EnemyFrameId.TorizoCombat4, 6),
            Show(EnemyFrameId.TorizoCombat3, 2),
            Show(EnemyFrameId.TorizoCombat2, 2),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnBombTorizoSonicBoom, Parameter: 0)),
            Show(EnemyFrameId.TorizoCombat1, 2),
            Show(EnemyFrameId.TorizoCombat0, 24),
            Show(EnemyFrameId.GoldenTorizoLeftOrb0, 6),
            Show(EnemyFrameId.TorizoCombat5, 6),
            Show(EnemyFrameId.TorizoCombat6, 6),
            Show(EnemyFrameId.TorizoCombat7, 6),
            Show(EnemyFrameId.TorizoCombat8, 6),
            Show(EnemyFrameId.TorizoCombat9, 6),
            Show(EnemyFrameId.TorizoCombat8, 2),
            Show(EnemyFrameId.TorizoCombat7, 2),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnBombTorizoSonicBoom, Parameter: 1)),
            Show(EnemyFrameId.TorizoCombat6, 2),
            Show(EnemyFrameId.TorizoCombat5, 56),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.Return)),
        ],
        [P.FacingLeftSonicBoomsLeftFootForward0] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Gravity)),
        ],
        [P.FacingLeftSonicBoomsLeftFootForward1] =
        [
            Show(EnemyFrameId.GoldenTorizoLeftOrb6, 6),
            Show(EnemyFrameId.TorizoCombat10, 6),
            Show(EnemyFrameId.TorizoCombat11, 6),
            Show(EnemyFrameId.TorizoCombat12, 6),
            Show(EnemyFrameId.TorizoCombat13, 6),
            Show(EnemyFrameId.TorizoCombat14, 6),
            Show(EnemyFrameId.TorizoCombat13, 2),
            Show(EnemyFrameId.TorizoCombat12, 2),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnBombTorizoSonicBoom, Parameter: 0)),
            Show(EnemyFrameId.TorizoCombat11, 2),
            Show(EnemyFrameId.TorizoCombat10, 24),
            Show(EnemyFrameId.GoldenTorizoLeftOrb6, 6),
            Show(EnemyFrameId.TorizoCombat15, 6),
            Show(EnemyFrameId.TorizoCombat16, 6),
            Show(EnemyFrameId.TorizoCombat17, 6),
            Show(EnemyFrameId.TorizoCombat18, 6),
            Show(EnemyFrameId.TorizoCombat19, 6),
            Show(EnemyFrameId.TorizoCombat18, 2),
            Show(EnemyFrameId.TorizoCombat17, 2),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnBombTorizoSonicBoom, Parameter: 1)),
            Show(EnemyFrameId.TorizoCombat16, 2),
            Show(EnemyFrameId.TorizoCombat15, 56),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.Return)),
        ],
        [P.FacingLeftExplosiveSwipeRightFootForward] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Gravity)),
            Show(EnemyFrameId.GoldenTorizoLeftOrb0, 3),
            Show(EnemyFrameId.TorizoCombat0, 3),
            Show(EnemyFrameId.TorizoCombat1, 3),
            Show(EnemyFrameId.TorizoCombat2, 3),
            Show(EnemyFrameId.TorizoCombat3, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 0)),
            Show(EnemyFrameId.TorizoCombat4, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 1)),
            Show(EnemyFrameId.TorizoCombat3, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 2)),
            Show(EnemyFrameId.TorizoCombat2, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 3)),
            Show(EnemyFrameId.TorizoCombat1, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 4)),
            Show(EnemyFrameId.TorizoCombat0, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 5)),
            Show(EnemyFrameId.GoldenTorizoLeftOrb0, 3),
            Show(EnemyFrameId.TorizoCombat5, 3),
            Show(EnemyFrameId.TorizoCombat6, 3),
            Show(EnemyFrameId.TorizoCombat7, 3),
            Show(EnemyFrameId.TorizoCombat8, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 6)),
            Show(EnemyFrameId.TorizoCombat9, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 7)),
            Show(EnemyFrameId.TorizoCombat8, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 8)),
            Show(EnemyFrameId.TorizoCombat7, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 9)),
            Show(EnemyFrameId.TorizoCombat6, 1),
            Show(EnemyFrameId.TorizoCombat5, 16),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.Return)),
        ],
        [P.FacingLeftExplosiveSwipeLeftFootForward] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Gravity)),
            Show(EnemyFrameId.GoldenTorizoLeftOrb6, 3),
            Show(EnemyFrameId.TorizoCombat10, 3),
            Show(EnemyFrameId.TorizoCombat11, 3),
            Show(EnemyFrameId.TorizoCombat12, 3),
            Show(EnemyFrameId.TorizoCombat13, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 0)),
            Show(EnemyFrameId.TorizoCombat14, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 1)),
            Show(EnemyFrameId.TorizoCombat13, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 2)),
            Show(EnemyFrameId.TorizoCombat12, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 3)),
            Show(EnemyFrameId.TorizoCombat11, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 4)),
            Show(EnemyFrameId.TorizoCombat10, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 5)),
            Show(EnemyFrameId.GoldenTorizoLeftOrb6, 3),
            Show(EnemyFrameId.TorizoCombat15, 3),
            Show(EnemyFrameId.TorizoCombat16, 3),
            Show(EnemyFrameId.TorizoCombat17, 3),
            Show(EnemyFrameId.TorizoCombat18, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 6)),
            Show(EnemyFrameId.TorizoCombat19, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 7)),
            Show(EnemyFrameId.TorizoCombat18, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 8)),
            Show(EnemyFrameId.TorizoCombat17, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 9)),
            Show(EnemyFrameId.TorizoCombat16, 1),
            Show(EnemyFrameId.TorizoCombat15, 16),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.Return)),
        ],
        [P.FacingLeftJumpingForwards0] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Jump)),
            Cue(new TorizoCue.Branch(TorizoBranchKind.Link, Target: P.FacingLeftFalling0)),
            Show(EnemyFrameId.TorizoJumpBackLeft0, 5),
            Show(EnemyFrameId.TorizoFallingLeft, 5),
        ],
        [P.FacingLeftJumpingForwards1] =
        [
            Show(EnemyFrameId.TorizoJumpBackLeft1, 1),
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
            Show(EnemyFrameId.TorizoJumpBackLeft0, 5),
            Show(EnemyFrameId.TorizoFallingLeft, 5),
        ],
        [P.FacingLeftJumpingBackwardLandLeftFootFwd1] =
        [
            Show(EnemyFrameId.TorizoJumpBackLeft1, 1),
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
            Show(EnemyFrameId.TorizoJumpBackLeft0, 5),
            Show(EnemyFrameId.TorizoFallingLeft, 5),
        ],
        [P.FacingLeftJumpingBackwardRightFootFwd1] =
        [
            Show(EnemyFrameId.TorizoJumpBackLeft1, 1),
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
            Show(EnemyFrameId.GoldenTorizoRightward0, 8),
        ],
        [P.FacingLeftFacelessWalkingRightLegMoving] =
        [
            Cue(new TorizoCue.Command(K.SteppedLeftWithLeftFoot)),
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.Active)),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.SpawnLowHealthDrool)),
            Cue(new TorizoCue.Command(K.PlayFootstepSound)),
            Show(EnemyFrameId.GoldenTorizoWalkLeft0, 1),
            Cue(new TorizoCue.Branch(TorizoBranchKind.CallIfSamusCloseBehind, Target: P.FacingLeftExplosiveSwipeLeftFootForward)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 1)),
            Show(EnemyFrameId.GoldenTorizoWalkLeft1, 5),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 2)),
            Show(EnemyFrameId.GoldenTorizoWalkLeft2, 5),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 3)),
            Show(EnemyFrameId.GoldenTorizoWalkLeft3, 5),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 4)),
            Show(EnemyFrameId.GoldenTorizoWalkLeft4, 5),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 5)),
        ],
        [P.FacingLeftFacelessWalkingLeftLegMoving] =
        [
            Cue(new TorizoCue.Command(K.SteppedLeftWithRightFoot)),
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.Active)),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.SpawnLowHealthDrool)),
            Cue(new TorizoCue.Command(K.PlayFootstepSound)),
            Show(EnemyFrameId.GoldenTorizoWalkLeft5, 1),
            Cue(new TorizoCue.Branch(TorizoBranchKind.CallIfSamusCloseBehind, Target: P.FacingLeftExplosiveSwipeRightFootForward)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 6)),
            Show(EnemyFrameId.GoldenTorizoWalkLeft6, 5),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 7)),
            Show(EnemyFrameId.GoldenTorizoWalkLeft7, 5),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 8)),
            Show(EnemyFrameId.GoldenTorizoWalkLeft8, 5),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 9)),
            Show(EnemyFrameId.GoldenTorizoWalkLeft9, 5),
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
            Show(EnemyFrameId.TorizoCombat20, 32),
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, Step: 8)),
            Show(EnemyFrameId.TorizoCombat21, 16),
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, Step: 9)),
            Show(EnemyFrameId.TorizoCombat22, 8),
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, Step: 10)),
            Show(EnemyFrameId.TorizoCombat23, 8),
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, Step: 11)),
            Show(EnemyFrameId.TorizoCombat24, 8),
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, Step: 12)),
            Show(EnemyFrameId.TorizoCombat25, 8),
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, Step: 13)),
            Show(EnemyFrameId.TorizoCombat26, 8),
        ],
    };
}
