using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Bosses.Torizo.TorizoCueKind;
using P = SuperMetroid.Core.Bosses.Torizo.TorizoBodyProgram;
using static SuperMetroid.Core.Bosses.Torizo.TorizoProgramSteps;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Bomb Torizo's programs while it faces right: turning, walking, attacks, jumps and falls, with its face and without.</summary>
internal static class TorizoFacingRightPrograms
{
    /// <summary>Each segment and its beats.</summary>
    internal static Dictionary<TorizoBodyProgram, AnimationBeat<EnemyFrameId, TorizoCue>[]> Segments { get; } = new()
    {
        [P.FacingRightTurningRight] =
        [
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.Falling)),
            Cue(new TorizoCue.Command(K.SetTurningAround)),
            Show(EnemyFrameId.GoldenTorizoRightward0, 24),
        ],
        [P.FacingRightWalkingLeftLegMoving] =
        [
            Cue(new TorizoCue.Command(K.SteppedRightWithRightFoot)),
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.Active)),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.SpawnLowHealthDrool)),
            Cue(new TorizoCue.Command(K.PlayFootstepSound)),
            Show(EnemyFrameId.GoldenTorizoRightward1, 10),
            Cue(new TorizoCue.Branch(TorizoBranchKind.CallIfSamusCloseBehind, Target: P.FacingRightExplosiveSwipeRightFootForward)),
            Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: P.FacingRightJumpBackwardLandRightFootFwd0)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 11)),
            Show(EnemyFrameId.GoldenTorizoRightward2, 6),
            Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: P.FacingRightJumpBackwardLandRightFootFwd0)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 12)),
            Show(EnemyFrameId.GoldenTorizoRightward3, 6),
            Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: P.FacingRightJumpBackwardLandRightFootFwd0)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 13)),
            Show(EnemyFrameId.GoldenTorizoRightward4, 6),
            Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: P.FacingRightJumpBackwardLandRightFootFwd0)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 14)),
            Show(EnemyFrameId.GoldenTorizoRightward5, 6),
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
            Show(EnemyFrameId.GoldenTorizoRightward6, 10),
            Cue(new TorizoCue.Branch(TorizoBranchKind.CallIfSamusCloseBehind, Target: P.FacingRightExplosiveSwipeLeftFootForward)),
            Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: P.FacingRightJumpBackwardsLandLeftFootFwd0)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 16)),
            Show(EnemyFrameId.GoldenTorizoRightward7, 6),
            Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: P.FacingRightJumpBackwardsLandLeftFootFwd0)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 17)),
            Show(EnemyFrameId.GoldenTorizoRightward8, 6),
            Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: P.FacingRightJumpBackwardsLandLeftFootFwd0)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 18)),
            Show(EnemyFrameId.GoldenTorizoRightward9, 6),
            Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: P.FacingRightJumpBackwardsLandLeftFootFwd0)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 19)),
            Show(EnemyFrameId.GoldenTorizoRightward10, 6),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 10)),
        ],
        [P.FacingRightSpewingChozoOrbsLeftFootFwd0] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Gravity)),
            Show(EnemyFrameId.GoldenTorizoRightSonic0, 16),
            Show(EnemyFrameId.GoldenTorizoLeftFootOrb0, 8),
            Show(EnemyFrameId.GoldenTorizoLeftFootOrb1, 8),
            Show(EnemyFrameId.GoldenTorizoLeftFootOrb2, 8),
            Show(EnemyFrameId.GoldenTorizoLeftFootOrb3, 8),
            Show(EnemyFrameId.GoldenTorizoLeftFootOrb4, 16),
            Cue(new TorizoCue.Command(K.PlayShotSound)),
        ],
        [P.FacingRightSpewingChozoOrbsLeftFootFwd1] =
        [
            Cue(new TorizoCue.Command(K.SpawnBombTorizoChozoOrbs)),
            Wait(16),
            Show(EnemyFrameId.GoldenTorizoLeftFootOrb3, 8),
            Show(EnemyFrameId.GoldenTorizoLeftFootOrb2, 8),
            Show(EnemyFrameId.GoldenTorizoLeftFootOrb1, 8),
            Show(EnemyFrameId.GoldenTorizoLeftFootOrb0, 72),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.Return)),
        ],
        [P.FacingRightSpewingChozoOrbsRightFootFwd0] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Gravity)),
            Show(EnemyFrameId.GoldenTorizoRightOrb0, 16),
            Show(EnemyFrameId.GoldenTorizoRightOrb1, 8),
            Show(EnemyFrameId.GoldenTorizoRightOrb2, 8),
            Show(EnemyFrameId.GoldenTorizoRightOrb3, 8),
            Show(EnemyFrameId.GoldenTorizoRightOrb4, 8),
            Show(EnemyFrameId.GoldenTorizoRightOrb5, 16),
            Cue(new TorizoCue.Command(K.PlayShotSound)),
        ],
        [P.FacingRightSpewingChozoOrbsRightFootFwd1] =
        [
            Cue(new TorizoCue.Command(K.SpawnBombTorizoChozoOrbs)),
            Wait(16),
            Show(EnemyFrameId.GoldenTorizoRightOrb4, 8),
            Show(EnemyFrameId.GoldenTorizoRightOrb3, 8),
            Show(EnemyFrameId.GoldenTorizoRightOrb2, 8),
            Show(EnemyFrameId.GoldenTorizoRightOrb1, 72),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.Return)),
        ],
        [P.FacingRightSonicBoomsLeftFootForward0] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Gravity)),
        ],
        [P.FacingRightSonicBoomsLeftFootForward1] =
        [
            Show(EnemyFrameId.GoldenTorizoRightSonic0, 6),
            Show(EnemyFrameId.GoldenTorizoRightSonic1, 6),
            Show(EnemyFrameId.GoldenTorizoRightSonic2, 6),
            Show(EnemyFrameId.GoldenTorizoRightSonic3, 6),
            Show(EnemyFrameId.GoldenTorizoRightSonic4, 6),
            Show(EnemyFrameId.GoldenTorizoRightSonic5, 6),
            Show(EnemyFrameId.GoldenTorizoRightSonic4, 2),
            Show(EnemyFrameId.GoldenTorizoRightSonic3, 2),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnBombTorizoSonicBoom, Parameter: 0)),
            Show(EnemyFrameId.GoldenTorizoRightSonic2, 2),
            Show(EnemyFrameId.GoldenTorizoRightSonic1, 24),
            Show(EnemyFrameId.GoldenTorizoRightSonic0, 6),
            Show(EnemyFrameId.GoldenTorizoRightSonic6, 6),
            Show(EnemyFrameId.GoldenTorizoRightSonic7, 6),
            Show(EnemyFrameId.GoldenTorizoRightSonic8, 6),
            Show(EnemyFrameId.GoldenTorizoRightSonic9, 6),
            Show(EnemyFrameId.GoldenTorizoRightSonic10, 6),
            Show(EnemyFrameId.GoldenTorizoRightSonic9, 2),
            Show(EnemyFrameId.GoldenTorizoRightSonic8, 2),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnBombTorizoSonicBoom, Parameter: 1)),
            Show(EnemyFrameId.GoldenTorizoRightSonic7, 2),
            Show(EnemyFrameId.GoldenTorizoRightSonic6, 56),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.Return)),
        ],
        [P.FacingRightSonicBoomsRightFootForward0] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Gravity)),
        ],
        [P.FacingRightSonicBoomsRightFootForward1] =
        [
            Show(EnemyFrameId.GoldenTorizoRightOrb0, 6),
            Show(EnemyFrameId.GoldenTorizoRightSonic11, 6),
            Show(EnemyFrameId.GoldenTorizoRightSonic12, 6),
            Show(EnemyFrameId.GoldenTorizoRightSonic13, 6),
            Show(EnemyFrameId.GoldenTorizoRightSonic14, 6),
            Show(EnemyFrameId.GoldenTorizoRightSonic15, 6),
            Show(EnemyFrameId.GoldenTorizoRightSonic14, 2),
            Show(EnemyFrameId.GoldenTorizoRightSonic13, 2),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnBombTorizoSonicBoom, Parameter: 0)),
            Show(EnemyFrameId.GoldenTorizoRightSonic12, 2),
            Show(EnemyFrameId.GoldenTorizoRightSonic11, 24),
            Show(EnemyFrameId.GoldenTorizoRightOrb0, 6),
            Show(EnemyFrameId.GoldenTorizoRightSonic16, 6),
            Show(EnemyFrameId.GoldenTorizoRightSonic17, 6),
            Show(EnemyFrameId.GoldenTorizoRightSonic18, 6),
            Show(EnemyFrameId.GoldenTorizoRightSonic19, 6),
            Show(EnemyFrameId.GoldenTorizoRightSonic20, 6),
            Show(EnemyFrameId.GoldenTorizoRightSonic19, 2),
            Show(EnemyFrameId.GoldenTorizoRightSonic18, 2),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnBombTorizoSonicBoom, Parameter: 1)),
            Show(EnemyFrameId.GoldenTorizoRightSonic17, 2),
            Show(EnemyFrameId.GoldenTorizoRightSonic16, 56),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.Return)),
        ],
        [P.FacingRightExplosiveSwipeLeftFootForward] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Gravity)),
            Show(EnemyFrameId.GoldenTorizoRightSonic0, 3),
            Show(EnemyFrameId.GoldenTorizoRightSonic1, 3),
            Show(EnemyFrameId.GoldenTorizoRightSonic2, 3),
            Show(EnemyFrameId.GoldenTorizoRightSonic3, 3),
            Show(EnemyFrameId.GoldenTorizoRightSonic4, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 0)),
            Show(EnemyFrameId.GoldenTorizoRightSonic5, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 1)),
            Show(EnemyFrameId.GoldenTorizoRightSonic4, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 2)),
            Show(EnemyFrameId.GoldenTorizoRightSonic3, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 3)),
            Show(EnemyFrameId.GoldenTorizoRightSonic2, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 4)),
            Show(EnemyFrameId.GoldenTorizoRightSonic1, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 5)),
            Show(EnemyFrameId.GoldenTorizoRightSonic0, 3),
            Show(EnemyFrameId.GoldenTorizoRightSonic6, 3),
            Show(EnemyFrameId.GoldenTorizoRightSonic7, 3),
            Show(EnemyFrameId.GoldenTorizoRightSonic8, 3),
            Show(EnemyFrameId.GoldenTorizoRightSonic9, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 6)),
            Show(EnemyFrameId.GoldenTorizoRightSonic10, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 7)),
            Show(EnemyFrameId.GoldenTorizoRightSonic9, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 8)),
            Show(EnemyFrameId.GoldenTorizoRightSonic8, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 9)),
            Show(EnemyFrameId.GoldenTorizoRightSonic7, 1),
            Show(EnemyFrameId.GoldenTorizoRightSonic6, 16),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.Return)),
        ],
        [P.FacingRightExplosiveSwipeRightFootForward] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Gravity)),
            Show(EnemyFrameId.GoldenTorizoRightOrb0, 3),
            Show(EnemyFrameId.GoldenTorizoRightSonic11, 3),
            Show(EnemyFrameId.GoldenTorizoRightSonic12, 3),
            Show(EnemyFrameId.GoldenTorizoRightSonic13, 3),
            Show(EnemyFrameId.GoldenTorizoRightSonic14, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 0)),
            Show(EnemyFrameId.GoldenTorizoRightSonic15, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 1)),
            Show(EnemyFrameId.GoldenTorizoRightSonic14, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 2)),
            Show(EnemyFrameId.GoldenTorizoRightSonic13, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 3)),
            Show(EnemyFrameId.GoldenTorizoRightSonic12, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 4)),
            Show(EnemyFrameId.GoldenTorizoRightSonic11, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 5)),
            Show(EnemyFrameId.GoldenTorizoRightOrb0, 3),
            Show(EnemyFrameId.GoldenTorizoRightSonic16, 3),
            Show(EnemyFrameId.GoldenTorizoRightSonic17, 3),
            Show(EnemyFrameId.GoldenTorizoRightSonic18, 3),
            Show(EnemyFrameId.GoldenTorizoRightSonic19, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 6)),
            Show(EnemyFrameId.GoldenTorizoRightSonic20, 3),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 7)),
            Show(EnemyFrameId.GoldenTorizoRightSonic19, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 8)),
            Show(EnemyFrameId.GoldenTorizoRightSonic18, 1),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnExplosiveSwipe, Parameter: 9)),
            Show(EnemyFrameId.GoldenTorizoRightSonic17, 1),
            Show(EnemyFrameId.GoldenTorizoRightSonic16, 16),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.Return)),
        ],
        [P.FacingRightJumpingForwards0] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Jump)),
            Cue(new TorizoCue.Branch(TorizoBranchKind.Link, Target: P.FacingRightFalling0)),
            Show(EnemyFrameId.TorizoJumpBack0, 5),
            Show(EnemyFrameId.TorizoJumpBack1, 5),
        ],
        [P.FacingRightJumpingForwards1] =
        [
            Show(EnemyFrameId.TorizoJumpBack2, 1),
            Cue(new TorizoCue.Branch(TorizoBranchKind.GoToIfRising, Target: P.FacingRightJumpingForwards1)),
        ],
        [P.FacingRightFalling0] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Jump)),
            Cue(new TorizoCue.Branch(TorizoBranchKind.Link, Target: P.FacingRightFalling2)),
        ],
        [P.FacingRightFalling1] =
        [
            Show(EnemyFrameId.TorizoJumpBack1, 5),
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
            Show(EnemyFrameId.TorizoJumpBack0, 5),
            Show(EnemyFrameId.TorizoJumpBack1, 5),
        ],
        [P.FacingRightJumpBackwardLandRightFootFwd1] =
        [
            Show(EnemyFrameId.TorizoJumpBack2, 1),
            Cue(new TorizoCue.Branch(TorizoBranchKind.GoToIfRising, Target: P.FacingRightJumpBackwardLandRightFootFwd1)),
        ],
        [P.FacingRightJumpBackwardLandRightFootFwd2] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Jump)),
            Cue(new TorizoCue.Branch(TorizoBranchKind.Link, Target: P.FacingRightJumpBackwardLandRightFootFwd4)),
        ],
        [P.FacingRightJumpBackwardLandRightFootFwd3] =
        [
            Show(EnemyFrameId.TorizoJumpBack1, 5),
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
            Show(EnemyFrameId.TorizoJumpBack0, 5),
            Show(EnemyFrameId.TorizoJumpBack1, 5),
        ],
        [P.FacingRightJumpBackwardsLandLeftFootFwd1] =
        [
            Show(EnemyFrameId.TorizoJumpBack2, 1),
            Cue(new TorizoCue.Branch(TorizoBranchKind.GoToIfRising, Target: P.FacingRightJumpBackwardsLandLeftFootFwd1)),
        ],
        [P.FacingRightJumpBackwardsLandLeftFootFwd2] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Jump)),
            Cue(new TorizoCue.Branch(TorizoBranchKind.Link, Target: P.FacingRightJumpBackwardsLandLeftFootFwd4)),
        ],
        [P.FacingRightJumpBackwardsLandLeftFootFwd3] =
        [
            Show(EnemyFrameId.TorizoJumpBack1, 5),
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
            Show(EnemyFrameId.GoldenTorizoRightward0, 8),
        ],
        [P.FacingRightFacelessWalkingLeftLegMoving] =
        [
            Cue(new TorizoCue.Command(K.SteppedRightWithRightFoot)),
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.Active)),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.SpawnLowHealthDrool)),
            Cue(new TorizoCue.Command(K.PlayFootstepSound)),
            Show(EnemyFrameId.GoldenTorizoRightward1, 1),
            Cue(new TorizoCue.Branch(TorizoBranchKind.CallIfSamusCloseBehind, Target: P.FacingRightExplosiveSwipeRightFootForward)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 11)),
            Show(EnemyFrameId.GoldenTorizoRightward2, 5),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 12)),
            Show(EnemyFrameId.GoldenTorizoRightward3, 5),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 13)),
            Show(EnemyFrameId.GoldenTorizoRightward4, 5),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 14)),
            Show(EnemyFrameId.GoldenTorizoRightward5, 5),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 15)),
        ],
        [P.FacingRightFacelessWalkingRightLegMoving] =
        [
            Cue(new TorizoCue.Command(K.SteppedRightWithLeftFoot)),
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.Active)),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
            Cue(new TorizoCue.Command(K.SpawnLowHealthDrool)),
            Cue(new TorizoCue.Command(K.PlayFootstepSound)),
            Show(EnemyFrameId.GoldenTorizoRightward6, 1),
            Cue(new TorizoCue.Branch(TorizoBranchKind.CallIfSamusCloseBehind, Target: P.FacingRightExplosiveSwipeLeftFootForward)),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 16)),
            Show(EnemyFrameId.GoldenTorizoRightward7, 5),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 17)),
            Show(EnemyFrameId.GoldenTorizoRightward8, 5),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 18)),
            Show(EnemyFrameId.GoldenTorizoRightward9, 5),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 19)),
            Show(EnemyFrameId.GoldenTorizoRightward10, 5),
            Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 10)),
        ],
    };
}
