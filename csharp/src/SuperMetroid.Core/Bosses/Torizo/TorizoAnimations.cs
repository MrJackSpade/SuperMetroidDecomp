using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Bosses.Torizo.TorizoCueKind;
using P = SuperMetroid.Core.Bosses.Torizo.TorizoBodyProgram;
using SuperMetroid.Core.Bosses.Torizo.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Bomb and Golden Torizo's typed body programs (#641), in place of their bank-$AA lists. Generated from the verified
/// native lists: each frame shows a <see cref="TorizoFrame"/> pose, each callback becomes a <see cref="TorizoCue"/>
/// with its typed operands, and each tile transfer names its <see cref="TorizoInstructionVramTransferDefinitions"/>
/// entry. A callback that sleeps for a fixed time is its cue followed by a wait of that length.
/// </summary>
internal static class TorizoAnimations
{
    /// <summary>Every body program.</summary>
    internal static AnimationProgramSet<TorizoBodyProgram, TorizoFrame, TorizoCue> Programs { get; } = new(
        new Dictionary<TorizoBodyProgram, AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue>[]>
    {
        [P.SpecialCallableBlowUpBombTorizosGut] =
        [
            Cue(new(K.SetFunction, Function: TorizoFunction.Idle)),
            Cue(new(K.LockAnimation)),
            Cue(new(K.SpawnLowHealthExplosions, Value: 0)),
            Wait(40),
            Cue(new(K.BlowUpGut)),
            CopyTiles(0),
            CopyTiles(1),
            CopyTiles(2),
            CopyTiles(3),
            Cue(new(K.SetFunction, Function: TorizoFunction.Active)),
            Cue(new(K.UnlockAnimation)),
            Cue(new(K.ResumeInterrupted)),
            Unreachable,
        ],
        [P.CallableBlowUpBombTorizosFace] =
        [
            Cue(new(K.SetFunction, Function: TorizoFunction.Idle)),
            Cue(new(K.LockAnimation)),
            Cue(new(K.SpawnLowHealthExplosions, Value: 6)),
            Wait(40),
            Cue(new(K.MarkFaceBlownUp)),
            CopyTiles(4),
            CopyTiles(5),
            CopyTiles(6),
            CopyTiles(7),
            Wait(1),
            CopyTiles(8),
            CopyTiles(9),
            CopyTiles(10),
            CopyTiles(11),
            CopyTiles(12),
            Cue(new(K.SetFunction, Function: TorizoFunction.Active)),
            Cue(new(K.UnlockAnimation)),
            Cue(new(K.Return)),
            Unreachable,
        ],
        [P.DeathSequence0] =
        [
            Cue(new(K.SetFunction, Function: TorizoFunction.Idle)),
            Cue(new(K.LockAnimation)),
            SetCounter(8),
            FallInto(P.DeathSequence1),
        ],
        [P.DeathSequence1] =
        [
            Cue(new(K.SpawnDeathExplosion)),
            Wait(1),
            Wait(6),
            RepeatWhileCounting(P.DeathSequence1),
            Cue(new(K.FadeToBlack)),
            SetCounter(14),
            FallInto(P.DeathSequence2),
        ],
        [P.DeathSequence2] =
        [
            Cue(new(K.SpawnDeathExplosion)),
            Wait(1),
            Cue(new(K.MakeVisible)),
            Wait(2),
            Cue(new(K.MakeInvisible)),
            Wait(2),
            Cue(new(K.SpawnDeathExplosion)),
            Wait(1),
            Cue(new(K.MakeVisible)),
            Wait(2),
            Cue(new(K.MakeInvisible)),
            Wait(2),
            Cue(new(K.MakeVisible)),
            Wait(2),
            Cue(new(K.MakeInvisible)),
            Wait(2),
            Cue(new(K.MakeVisible)),
            Wait(2),
            Cue(new(K.MakeInvisible)),
            Wait(2),
            Cue(new(K.AdvancePaletteFade)),
            RepeatWhileCounting(P.DeathSequence2),
            Wait(64),
            Cue(new(K.FinishDeath)),
            Stop,
        ],
        [P.BombTorizoInitial0] =
        [
            Cue(new(K.SteppedLeftWithRightFoot)),
            Cue(new(K.LockAnimation)),
            Show(0, 1),
            Cue(new(K.SetFunction, Function: TorizoFunction.WaitForHandTrigger)),
            Hold,
        ],
        [P.BombTorizoInitial0Resumed] =
        [
            Cue(new(K.SetFunction, Function: TorizoFunction.Falling)),
            Show(44, 48),
            CopyTiles(13),
            Show(44, 32),
            CopyTiles(14),
            Show(44, 16),
            CopyTiles(15),
            Show(44, 8),
            CopyTiles(16),
            Show(44, 32),
            SetCounter(2),
            FallInto(P.BombTorizoInitial1),
        ],
        [P.BombTorizoInitial1] =
        [
            Show(44, 4),
            CopyTiles(17),
            Show(44, 4),
            CopyTiles(18),
            Show(44, 4),
            CopyTiles(19),
            Show(44, 4),
            CopyTiles(20),
            RepeatWhileCounting(P.BombTorizoInitial1),
            Show(44, 48),
            Cue(new(K.StandUp, Value: 0)),
            Show(45, 16),
            Cue(new(K.StandUp, Value: 2)),
            Show(46, 8),
            Cue(new(K.StandUp, Value: 4)),
            Show(47, 8),
            Cue(new(K.StandUp, Value: 6)),
            Show(48, 8),
            Cue(new(K.StandUp, Value: 8)),
            Show(49, 8),
            Cue(new(K.StandUp, Value: 10)),
            Cue(new(K.FadeToNormalColors)),
            SetCounter(16),
            FallInto(P.BombTorizoInitial2),
        ],
        [P.BombTorizoInitial2] =
        [
            Show(50, 4),
            Cue(new(K.AdvancePaletteFade)),
            RepeatWhileCounting(P.BombTorizoInitial2),
            Cue(new(K.NoOperation)),
            Cue(new(K.UnlockAnimation)),
            Cue(new(K.StartBombTorizoFight)),
            Show(50, 16),
            GoTo(P.FacingLeftWalkingLeftLegMoving),
        ],
        [P.FacingLeftTurningLeft] =
        [
            Cue(new(K.SetFunction, Function: TorizoFunction.Falling)),
            Cue(new(K.SetTurningAround)),
            Show(1, 24),
            FallInto(P.FacingLeftWalkingRightLegMoving),
        ],
        [P.FacingLeftWalkingRightLegMoving] =
        [
            Cue(new(K.SteppedLeftWithLeftFoot)),
            Cue(new(K.SetFunction, Function: TorizoFunction.Active)),
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.AirTransition)),
            Cue(new(K.SpawnLowHealthDrool)),
            Cue(new(K.PlayFootstepSound)),
            Show(2, 10),
            Cue(new(K.CallIfSamusCloseBehind, Target: P.FacingLeftExplosiveSwipeLeftFootForward)),
            Cue(new(K.JumpBackIfSamusCloseInFront, Target: P.FacingLeftJumpingBackwardLandLeftFootFwd0)),
            Cue(new(K.WalkBombTorizo, Value: 2)),
            Show(3, 6),
            Cue(new(K.JumpBackIfSamusCloseInFront, Target: P.FacingLeftJumpingBackwardLandLeftFootFwd0)),
            Cue(new(K.WalkBombTorizo, Value: 4)),
            Show(4, 6),
            Cue(new(K.JumpBackIfSamusCloseInFront, Target: P.FacingLeftJumpingBackwardLandLeftFootFwd0)),
            Cue(new(K.WalkBombTorizo, Value: 6)),
            Show(5, 6),
            Cue(new(K.JumpBackIfSamusCloseInFront, Target: P.FacingLeftJumpingBackwardLandLeftFootFwd0)),
            Cue(new(K.WalkBombTorizo, Value: 8)),
            Show(6, 6),
            Cue(new(K.WalkBombTorizo, Value: 10)),
            FallInto(P.FacingLeftWalkingLeftLegMoving),
        ],
        [P.FacingLeftWalkingLeftLegMoving] =
        [
            Cue(new(K.SteppedLeftWithRightFoot)),
            Cue(new(K.SetFunction, Function: TorizoFunction.Active)),
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.AirTransition)),
            Cue(new(K.SpawnLowHealthDrool)),
            Cue(new(K.PlayFootstepSound)),
            Show(7, 10),
            Cue(new(K.CallIfSamusCloseBehind, Target: P.FacingLeftExplosiveSwipeRightFootForward)),
            Cue(new(K.JumpBackIfSamusCloseInFront, Target: P.FacingLeftJumpingBackwardRightFootFwd0)),
            Cue(new(K.WalkBombTorizo, Value: 12)),
            Show(8, 6),
            Cue(new(K.JumpBackIfSamusCloseInFront, Target: P.FacingLeftJumpingBackwardRightFootFwd0)),
            Cue(new(K.WalkBombTorizo, Value: 14)),
            Show(9, 6),
            Cue(new(K.JumpBackIfSamusCloseInFront, Target: P.FacingLeftJumpingBackwardRightFootFwd0)),
            Cue(new(K.WalkBombTorizo, Value: 16)),
            Show(10, 6),
            Cue(new(K.JumpBackIfSamusCloseInFront, Target: P.FacingLeftJumpingBackwardRightFootFwd0)),
            Cue(new(K.WalkBombTorizo, Value: 18)),
            Show(11, 6),
            Cue(new(K.WalkBombTorizo, Value: 0)),
            GoTo(P.FacingLeftWalkingRightLegMoving),
        ],
        [P.FacingLeftSpewingChozoOrbsRightFootFwd0] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.Gravity)),
            Show(12, 16),
            Show(13, 8),
            Show(14, 8),
            Show(15, 8),
            Show(16, 8),
            Show(17, 16),
            Cue(new(K.PlayShotSound)),
            SetCounter(3),
            FallInto(P.FacingLeftSpewingChozoOrbsRightFootFwd1),
        ],
        [P.FacingLeftSpewingChozoOrbsRightFootFwd1] =
        [
            Cue(new(K.SpawnBombTorizoChozoOrbs)),
            Wait(16),
            RepeatWhileCounting(P.FacingLeftSpewingChozoOrbsRightFootFwd1),
            Show(16, 8),
            Show(15, 8),
            Show(14, 8),
            Show(13, 72),
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.AirTransition)),
            Cue(new(K.Return)),
            FallInto(P.FacingLeftSpewingChozoOrbsLeftFootFwd0),
        ],
        [P.FacingLeftSpewingChozoOrbsLeftFootFwd0] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.Gravity)),
            Show(18, 16),
            Show(19, 8),
            Show(20, 8),
            Show(21, 8),
            Show(22, 8),
            Show(23, 16),
            Cue(new(K.PlayShotSound)),
            SetCounter(3),
            FallInto(P.FacingLeftSpewingChozoOrbsLeftFootFwd1),
        ],
        [P.FacingLeftSpewingChozoOrbsLeftFootFwd1] =
        [
            Cue(new(K.SpawnBombTorizoChozoOrbs)),
            Wait(16),
            RepeatWhileCounting(P.FacingLeftSpewingChozoOrbsLeftFootFwd1),
            Show(22, 8),
            Show(21, 8),
            Show(20, 8),
            Show(19, 72),
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.AirTransition)),
            Cue(new(K.Return)),
            FallInto(P.FacingLeftSonicBoomsRightFootForward0),
        ],
        [P.FacingLeftSonicBoomsRightFootForward0] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.Gravity)),
            SetCounter(4),
            FallInto(P.FacingLeftSonicBoomsRightFootForward1),
        ],
        [P.FacingLeftSonicBoomsRightFootForward1] =
        [
            Show(12, 6),
            Show(24, 6),
            Show(25, 6),
            Show(26, 6),
            Show(27, 6),
            Show(28, 6),
            Show(27, 2),
            Show(26, 2),
            Cue(new(K.SpawnBombTorizoSonicBoom, Value: 0)),
            Show(25, 2),
            Show(24, 24),
            Show(12, 6),
            Show(29, 6),
            Show(30, 6),
            Show(31, 6),
            Show(32, 6),
            Show(33, 6),
            Show(32, 2),
            Show(31, 2),
            Cue(new(K.SpawnBombTorizoSonicBoom, Value: 1)),
            Show(30, 2),
            Show(29, 56),
            RepeatWhileCounting(P.FacingLeftSonicBoomsRightFootForward1),
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.AirTransition)),
            Cue(new(K.Return)),
            FallInto(P.FacingLeftSonicBoomsLeftFootForward0),
        ],
        [P.FacingLeftSonicBoomsLeftFootForward0] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.Gravity)),
            SetCounter(4),
            FallInto(P.FacingLeftSonicBoomsLeftFootForward1),
        ],
        [P.FacingLeftSonicBoomsLeftFootForward1] =
        [
            Show(18, 6),
            Show(34, 6),
            Show(35, 6),
            Show(36, 6),
            Show(37, 6),
            Show(38, 6),
            Show(37, 2),
            Show(36, 2),
            Cue(new(K.SpawnBombTorizoSonicBoom, Value: 0)),
            Show(35, 2),
            Show(34, 24),
            Show(18, 6),
            Show(39, 6),
            Show(40, 6),
            Show(41, 6),
            Show(42, 6),
            Show(43, 6),
            Show(42, 2),
            Show(41, 2),
            Cue(new(K.SpawnBombTorizoSonicBoom, Value: 1)),
            Show(40, 2),
            Show(39, 56),
            RepeatWhileCounting(P.FacingLeftSonicBoomsLeftFootForward1),
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.AirTransition)),
            Cue(new(K.Return)),
            FallInto(P.FacingLeftExplosiveSwipeRightFootForward),
        ],
        [P.FacingLeftExplosiveSwipeRightFootForward] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.Gravity)),
            Show(12, 3),
            Show(24, 3),
            Show(25, 3),
            Show(26, 3),
            Show(27, 3),
            Cue(new(K.SpawnExplosiveSwipe, Value: 0)),
            Show(28, 3),
            Cue(new(K.SpawnExplosiveSwipe, Value: 2)),
            Show(27, 1),
            Cue(new(K.SpawnExplosiveSwipe, Value: 4)),
            Show(26, 1),
            Cue(new(K.SpawnExplosiveSwipe, Value: 6)),
            Show(25, 1),
            Cue(new(K.SpawnExplosiveSwipe, Value: 8)),
            Show(24, 1),
            Cue(new(K.SpawnExplosiveSwipe, Value: 10)),
            Show(12, 3),
            Show(29, 3),
            Show(30, 3),
            Show(31, 3),
            Show(32, 3),
            Cue(new(K.SpawnExplosiveSwipe, Value: 12)),
            Show(33, 3),
            Cue(new(K.SpawnExplosiveSwipe, Value: 14)),
            Show(32, 1),
            Cue(new(K.SpawnExplosiveSwipe, Value: 16)),
            Show(31, 1),
            Cue(new(K.SpawnExplosiveSwipe, Value: 18)),
            Show(30, 1),
            Show(29, 16),
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.AirTransition)),
            Cue(new(K.Return)),
            FallInto(P.FacingLeftExplosiveSwipeLeftFootForward),
        ],
        [P.FacingLeftExplosiveSwipeLeftFootForward] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.Gravity)),
            Show(18, 3),
            Show(34, 3),
            Show(35, 3),
            Show(36, 3),
            Show(37, 3),
            Cue(new(K.SpawnExplosiveSwipe, Value: 0)),
            Show(38, 3),
            Cue(new(K.SpawnExplosiveSwipe, Value: 2)),
            Show(37, 1),
            Cue(new(K.SpawnExplosiveSwipe, Value: 4)),
            Show(36, 1),
            Cue(new(K.SpawnExplosiveSwipe, Value: 6)),
            Show(35, 1),
            Cue(new(K.SpawnExplosiveSwipe, Value: 8)),
            Show(34, 1),
            Cue(new(K.SpawnExplosiveSwipe, Value: 10)),
            Show(18, 3),
            Show(39, 3),
            Show(40, 3),
            Show(41, 3),
            Show(42, 3),
            Cue(new(K.SpawnExplosiveSwipe, Value: 12)),
            Show(43, 3),
            Cue(new(K.SpawnExplosiveSwipe, Value: 14)),
            Show(42, 1),
            Cue(new(K.SpawnExplosiveSwipe, Value: 16)),
            Show(41, 1),
            Cue(new(K.SpawnExplosiveSwipe, Value: 18)),
            Show(40, 1),
            Show(39, 16),
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.AirTransition)),
            Cue(new(K.Return)),
            FallInto(P.FacingLeftJumpingForwards0),
        ],
        [P.FacingLeftJumpingForwards0] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.Jump)),
            Cue(new(K.Link, Target: P.FacingLeftFalling0)),
            Show(100, 5),
            Show(101, 5),
            FallInto(P.FacingLeftJumpingForwards1),
        ],
        [P.FacingLeftJumpingForwards1] =
        [
            Show(102, 1),
            Cue(new(K.GoToIfRising, Target: P.FacingLeftJumpingForwards1)),
            FallInto(P.FacingLeftFalling0),
        ],
        [P.FacingLeftFalling0] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.Jump)),
            Cue(new(K.Link, Target: P.FacingLeftFalling2)),
            FallInto(P.FacingLeftFalling1),
        ],
        [P.FacingLeftFalling1] =
        [
            Show(101, 5),
            GoTo(P.FacingLeftFalling1),
        ],
        [P.FacingLeftFalling2] =
        [
            Cue(new(K.PlayFootstepSound)),
            Cue(new(K.SpawnLandingDust)),
            Cue(new(K.GoToIfFaceBlownUpElseIfGolden, Target: P.FacingLeftFacelessWalkingLeftLegMoving, Alternate: P.GoldenWalkingLeftLeftLegMoving)),
            GoTo(P.FacingLeftWalkingLeftLegMoving),
        ],
        [P.FacingLeftJumpingBackwardLandLeftFootFwd0] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.Jump)),
            Cue(new(K.Link, Target: P.FacingLeftJumpingBackwardLandLeftFootFwd2)),
            Show(100, 5),
            Show(101, 5),
            FallInto(P.FacingLeftJumpingBackwardLandLeftFootFwd1),
        ],
        [P.FacingLeftJumpingBackwardLandLeftFootFwd1] =
        [
            Show(102, 1),
            Cue(new(K.GoToIfRising, Target: P.FacingLeftJumpingBackwardLandLeftFootFwd1)),
            FallInto(P.FacingLeftJumpingBackwardLandLeftFootFwd2),
        ],
        [P.FacingLeftJumpingBackwardLandLeftFootFwd2] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.Jump)),
            Cue(new(K.Link, Target: P.FacingLeftJumpingBackwardLandLeftFootFwd4)),
            FallInto(P.FacingLeftJumpingBackwardLandLeftFootFwd3),
        ],
        [P.FacingLeftJumpingBackwardLandLeftFootFwd3] =
        [
            Show(101, 5),
            GoTo(P.FacingLeftJumpingBackwardLandLeftFootFwd3),
        ],
        [P.FacingLeftJumpingBackwardLandLeftFootFwd4] =
        [
            Cue(new(K.PlayFootstepSound)),
            Cue(new(K.SpawnLandingDust)),
            Cue(new(K.GoToIfFaceBlownUpElseIfGolden, Target: P.FacingLeftFacelessWalkingRightLegMoving, Alternate: P.GoldenLandedFromBackwardsJumpFacingLeftLeftFootFwd)),
            Cue(new(K.CallBombTorizoAttack, Target: P.FacingLeftSpewingChozoOrbsLeftFootFwd0, Alternate: P.FacingLeftSonicBoomsLeftFootForward0)),
            GoTo(P.FacingLeftWalkingRightLegMoving),
        ],
        [P.FacingLeftJumpingBackwardRightFootFwd0] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.Jump)),
            Cue(new(K.Link, Target: P.FacingLeftJumpingBackwardRightFootFwd2)),
            Show(100, 5),
            Show(101, 5),
            FallInto(P.FacingLeftJumpingBackwardRightFootFwd1),
        ],
        [P.FacingLeftJumpingBackwardRightFootFwd1] =
        [
            Show(102, 1),
            Cue(new(K.GoToIfRising, Target: P.FacingLeftJumpingBackwardRightFootFwd1)),
            FallInto(P.FacingLeftJumpingBackwardRightFootFwd2),
        ],
        [P.FacingLeftJumpingBackwardRightFootFwd2] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.Jump)),
            Cue(new(K.Link, Target: P.FacingLeftJumpingBackwardRightFootFwd4)),
            FallInto(P.FacingLeftJumpingBackwardRightFootFwd3),
        ],
        [P.FacingLeftJumpingBackwardRightFootFwd3] =
        [
            Show(101, 5),
            GoTo(P.FacingLeftJumpingBackwardRightFootFwd3),
        ],
        [P.FacingLeftJumpingBackwardRightFootFwd4] =
        [
            Cue(new(K.PlayFootstepSound)),
            Cue(new(K.SpawnLandingDust)),
            Cue(new(K.GoToIfFaceBlownUpElseIfGolden, Target: P.FacingLeftFacelessWalkingLeftLegMoving, Alternate: P.GoldenLandedFromBackwardsJumpFacingLeftRightFootFwd)),
            Cue(new(K.CallBombTorizoAttack, Target: P.FacingLeftSpewingChozoOrbsRightFootFwd0, Alternate: P.FacingLeftSonicBoomsRightFootForward0)),
            GoTo(P.FacingLeftWalkingLeftLegMoving),
        ],
        [P.FacingLeftFacelessTurningLeft] =
        [
            Cue(new(K.SetFunction, Function: TorizoFunction.Falling)),
            Cue(new(K.SetTurningAround)),
            Show(1, 8),
            FallInto(P.FacingLeftFacelessWalkingRightLegMoving),
        ],
        [P.FacingLeftFacelessWalkingRightLegMoving] =
        [
            Cue(new(K.SteppedLeftWithLeftFoot)),
            Cue(new(K.SetFunction, Function: TorizoFunction.Active)),
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.AirTransition)),
            Cue(new(K.SpawnLowHealthDrool)),
            Cue(new(K.PlayFootstepSound)),
            Show(2, 1),
            Cue(new(K.CallIfSamusCloseBehind, Target: P.FacingLeftExplosiveSwipeLeftFootForward)),
            Cue(new(K.WalkFacelessBombTorizo, Value: 2)),
            Show(3, 5),
            Cue(new(K.WalkFacelessBombTorizo, Value: 4)),
            Show(4, 5),
            Cue(new(K.WalkFacelessBombTorizo, Value: 6)),
            Show(5, 5),
            Cue(new(K.WalkFacelessBombTorizo, Value: 8)),
            Show(6, 5),
            Cue(new(K.WalkFacelessBombTorizo, Value: 10)),
            FallInto(P.FacingLeftFacelessWalkingLeftLegMoving),
        ],
        [P.FacingLeftFacelessWalkingLeftLegMoving] =
        [
            Cue(new(K.SteppedLeftWithRightFoot)),
            Cue(new(K.SetFunction, Function: TorizoFunction.Active)),
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.AirTransition)),
            Cue(new(K.SpawnLowHealthDrool)),
            Cue(new(K.PlayFootstepSound)),
            Show(7, 1),
            Cue(new(K.CallIfSamusCloseBehind, Target: P.FacingLeftExplosiveSwipeRightFootForward)),
            Cue(new(K.WalkFacelessBombTorizo, Value: 12)),
            Show(8, 5),
            Cue(new(K.WalkFacelessBombTorizo, Value: 14)),
            Show(9, 5),
            Cue(new(K.WalkFacelessBombTorizo, Value: 16)),
            Show(10, 5),
            Cue(new(K.WalkFacelessBombTorizo, Value: 18)),
            Show(11, 5),
            Cue(new(K.WalkFacelessBombTorizo, Value: 0)),
            GoTo(P.FacingLeftFacelessWalkingRightLegMoving),
        ],
        [P.FacingLeftFacelessWalkingLeftLegMovingTrailing] =
        [
            Cue(new(K.SteppedRightWithLeftFoot)),
            Show(0, 1),
            Cue(new(K.SetFunction, Function: TorizoFunction.WaitForHandTrigger)),
            Hold,
        ],
        [P.FacingLeftFacelessWalkingLeftLegMovingTrailingResumed] =
        [
            Cue(new(K.SetFunction, Function: TorizoFunction.Falling)),
            Show(93, 32),
            Cue(new(K.StandUp, Value: 16)),
            Show(94, 16),
            Cue(new(K.StandUp, Value: 18)),
            Show(95, 8),
            Cue(new(K.StandUp, Value: 20)),
            Show(96, 8),
            Cue(new(K.StandUp, Value: 22)),
            Show(97, 8),
            Cue(new(K.StandUp, Value: 24)),
            Show(98, 8),
            Cue(new(K.StandUp, Value: 26)),
            Show(99, 8),
            GoTo(P.FacingRightWalkingRightLegMoving),
        ],
        [P.FacingRightTurningRight] =
        [
            Cue(new(K.SetFunction, Function: TorizoFunction.Falling)),
            Cue(new(K.SetTurningAround)),
            Show(1, 24),
            FallInto(P.FacingRightWalkingLeftLegMoving),
        ],
        [P.FacingRightWalkingLeftLegMoving] =
        [
            Cue(new(K.SteppedRightWithRightFoot)),
            Cue(new(K.SetFunction, Function: TorizoFunction.Active)),
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.AirTransition)),
            Cue(new(K.SpawnLowHealthDrool)),
            Cue(new(K.PlayFootstepSound)),
            Show(51, 10),
            Cue(new(K.CallIfSamusCloseBehind, Target: P.FacingRightExplosiveSwipeRightFootForward)),
            Cue(new(K.JumpBackIfSamusCloseInFront, Target: P.FacingRightJumpBackwardLandRightFootFwd0)),
            Cue(new(K.WalkBombTorizo, Value: 22)),
            Show(52, 6),
            Cue(new(K.JumpBackIfSamusCloseInFront, Target: P.FacingRightJumpBackwardLandRightFootFwd0)),
            Cue(new(K.WalkBombTorizo, Value: 24)),
            Show(53, 6),
            Cue(new(K.JumpBackIfSamusCloseInFront, Target: P.FacingRightJumpBackwardLandRightFootFwd0)),
            Cue(new(K.WalkBombTorizo, Value: 26)),
            Show(54, 6),
            Cue(new(K.JumpBackIfSamusCloseInFront, Target: P.FacingRightJumpBackwardLandRightFootFwd0)),
            Cue(new(K.WalkBombTorizo, Value: 28)),
            Show(55, 6),
            Cue(new(K.JumpBackIfSamusCloseInFront, Target: P.FacingRightJumpBackwardLandRightFootFwd0)),
            Cue(new(K.WalkBombTorizo, Value: 30)),
            FallInto(P.FacingRightWalkingRightLegMoving),
        ],
        [P.FacingRightWalkingRightLegMoving] =
        [
            Cue(new(K.SteppedRightWithLeftFoot)),
            Cue(new(K.SetFunction, Function: TorizoFunction.Active)),
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.AirTransition)),
            Cue(new(K.SpawnLowHealthDrool)),
            Cue(new(K.PlayFootstepSound)),
            Show(56, 10),
            Cue(new(K.CallIfSamusCloseBehind, Target: P.FacingRightExplosiveSwipeLeftFootForward)),
            Cue(new(K.JumpBackIfSamusCloseInFront, Target: P.FacingRightJumpBackwardsLandLeftFootFwd0)),
            Cue(new(K.WalkBombTorizo, Value: 32)),
            Show(57, 6),
            Cue(new(K.JumpBackIfSamusCloseInFront, Target: P.FacingRightJumpBackwardsLandLeftFootFwd0)),
            Cue(new(K.WalkBombTorizo, Value: 34)),
            Show(58, 6),
            Cue(new(K.JumpBackIfSamusCloseInFront, Target: P.FacingRightJumpBackwardsLandLeftFootFwd0)),
            Cue(new(K.WalkBombTorizo, Value: 36)),
            Show(59, 6),
            Cue(new(K.JumpBackIfSamusCloseInFront, Target: P.FacingRightJumpBackwardsLandLeftFootFwd0)),
            Cue(new(K.WalkBombTorizo, Value: 38)),
            Show(60, 6),
            Cue(new(K.WalkBombTorizo, Value: 20)),
            GoTo(P.FacingRightWalkingLeftLegMoving),
        ],
        [P.FacingRightSpewingChozoOrbsLeftFootFwd0] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.Gravity)),
            Show(61, 16),
            Show(62, 8),
            Show(63, 8),
            Show(64, 8),
            Show(65, 8),
            Show(66, 16),
            Cue(new(K.PlayShotSound)),
            SetCounter(3),
            FallInto(P.FacingRightSpewingChozoOrbsLeftFootFwd1),
        ],
        [P.FacingRightSpewingChozoOrbsLeftFootFwd1] =
        [
            Cue(new(K.SpawnBombTorizoChozoOrbs)),
            Wait(16),
            RepeatWhileCounting(P.FacingRightSpewingChozoOrbsLeftFootFwd1),
            Show(65, 8),
            Show(64, 8),
            Show(63, 8),
            Show(62, 72),
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.AirTransition)),
            Cue(new(K.Return)),
            FallInto(P.FacingRightSpewingChozoOrbsRightFootFwd0),
        ],
        [P.FacingRightSpewingChozoOrbsRightFootFwd0] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.Gravity)),
            Show(67, 16),
            Show(68, 8),
            Show(69, 8),
            Show(70, 8),
            Show(71, 8),
            Show(72, 16),
            Cue(new(K.PlayShotSound)),
            SetCounter(3),
            FallInto(P.FacingRightSpewingChozoOrbsRightFootFwd1),
        ],
        [P.FacingRightSpewingChozoOrbsRightFootFwd1] =
        [
            Cue(new(K.SpawnBombTorizoChozoOrbs)),
            Wait(16),
            RepeatWhileCounting(P.FacingRightSpewingChozoOrbsRightFootFwd1),
            Show(71, 8),
            Show(70, 8),
            Show(69, 8),
            Show(68, 72),
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.AirTransition)),
            Cue(new(K.Return)),
            FallInto(P.FacingRightSonicBoomsLeftFootForward0),
        ],
        [P.FacingRightSonicBoomsLeftFootForward0] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.Gravity)),
            SetCounter(4),
            FallInto(P.FacingRightSonicBoomsLeftFootForward1),
        ],
        [P.FacingRightSonicBoomsLeftFootForward1] =
        [
            Show(61, 6),
            Show(73, 6),
            Show(74, 6),
            Show(75, 6),
            Show(76, 6),
            Show(77, 6),
            Show(76, 2),
            Show(75, 2),
            Cue(new(K.SpawnBombTorizoSonicBoom, Value: 0)),
            Show(74, 2),
            Show(73, 24),
            Show(61, 6),
            Show(78, 6),
            Show(79, 6),
            Show(80, 6),
            Show(81, 6),
            Show(82, 6),
            Show(81, 2),
            Show(80, 2),
            Cue(new(K.SpawnBombTorizoSonicBoom, Value: 1)),
            Show(79, 2),
            Show(78, 56),
            RepeatWhileCounting(P.FacingRightSonicBoomsLeftFootForward1),
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.AirTransition)),
            Cue(new(K.Return)),
            FallInto(P.FacingRightSonicBoomsRightFootForward0),
        ],
        [P.FacingRightSonicBoomsRightFootForward0] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.Gravity)),
            SetCounter(4),
            FallInto(P.FacingRightSonicBoomsRightFootForward1),
        ],
        [P.FacingRightSonicBoomsRightFootForward1] =
        [
            Show(67, 6),
            Show(83, 6),
            Show(84, 6),
            Show(85, 6),
            Show(86, 6),
            Show(87, 6),
            Show(86, 2),
            Show(85, 2),
            Cue(new(K.SpawnBombTorizoSonicBoom, Value: 0)),
            Show(84, 2),
            Show(83, 24),
            Show(67, 6),
            Show(88, 6),
            Show(89, 6),
            Show(90, 6),
            Show(91, 6),
            Show(92, 6),
            Show(91, 2),
            Show(90, 2),
            Cue(new(K.SpawnBombTorizoSonicBoom, Value: 1)),
            Show(89, 2),
            Show(88, 56),
            RepeatWhileCounting(P.FacingRightSonicBoomsRightFootForward1),
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.AirTransition)),
            Cue(new(K.Return)),
            FallInto(P.FacingRightExplosiveSwipeLeftFootForward),
        ],
        [P.FacingRightExplosiveSwipeLeftFootForward] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.Gravity)),
            Show(61, 3),
            Show(73, 3),
            Show(74, 3),
            Show(75, 3),
            Show(76, 3),
            Cue(new(K.SpawnExplosiveSwipe, Value: 0)),
            Show(77, 3),
            Cue(new(K.SpawnExplosiveSwipe, Value: 2)),
            Show(76, 1),
            Cue(new(K.SpawnExplosiveSwipe, Value: 4)),
            Show(75, 1),
            Cue(new(K.SpawnExplosiveSwipe, Value: 6)),
            Show(74, 1),
            Cue(new(K.SpawnExplosiveSwipe, Value: 8)),
            Show(73, 1),
            Cue(new(K.SpawnExplosiveSwipe, Value: 10)),
            Show(61, 3),
            Show(78, 3),
            Show(79, 3),
            Show(80, 3),
            Show(81, 3),
            Cue(new(K.SpawnExplosiveSwipe, Value: 12)),
            Show(82, 3),
            Cue(new(K.SpawnExplosiveSwipe, Value: 14)),
            Show(81, 1),
            Cue(new(K.SpawnExplosiveSwipe, Value: 16)),
            Show(80, 1),
            Cue(new(K.SpawnExplosiveSwipe, Value: 18)),
            Show(79, 1),
            Show(78, 16),
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.AirTransition)),
            Cue(new(K.Return)),
            FallInto(P.FacingRightExplosiveSwipeRightFootForward),
        ],
        [P.FacingRightExplosiveSwipeRightFootForward] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.Gravity)),
            Show(67, 3),
            Show(83, 3),
            Show(84, 3),
            Show(85, 3),
            Show(86, 3),
            Cue(new(K.SpawnExplosiveSwipe, Value: 0)),
            Show(87, 3),
            Cue(new(K.SpawnExplosiveSwipe, Value: 2)),
            Show(86, 1),
            Cue(new(K.SpawnExplosiveSwipe, Value: 4)),
            Show(85, 1),
            Cue(new(K.SpawnExplosiveSwipe, Value: 6)),
            Show(84, 1),
            Cue(new(K.SpawnExplosiveSwipe, Value: 8)),
            Show(83, 1),
            Cue(new(K.SpawnExplosiveSwipe, Value: 10)),
            Show(67, 3),
            Show(88, 3),
            Show(89, 3),
            Show(90, 3),
            Show(91, 3),
            Cue(new(K.SpawnExplosiveSwipe, Value: 12)),
            Show(92, 3),
            Cue(new(K.SpawnExplosiveSwipe, Value: 14)),
            Show(91, 1),
            Cue(new(K.SpawnExplosiveSwipe, Value: 16)),
            Show(90, 1),
            Cue(new(K.SpawnExplosiveSwipe, Value: 18)),
            Show(89, 1),
            Show(88, 16),
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.AirTransition)),
            Cue(new(K.Return)),
            FallInto(P.FacingRightJumpingForwards0),
        ],
        [P.FacingRightJumpingForwards0] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.Jump)),
            Cue(new(K.Link, Target: P.FacingRightFalling0)),
            Show(103, 5),
            Show(104, 5),
            FallInto(P.FacingRightJumpingForwards1),
        ],
        [P.FacingRightJumpingForwards1] =
        [
            Show(105, 1),
            Cue(new(K.GoToIfRising, Target: P.FacingRightJumpingForwards1)),
            FallInto(P.FacingRightFalling0),
        ],
        [P.FacingRightFalling0] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.Jump)),
            Cue(new(K.Link, Target: P.FacingRightFalling2)),
            FallInto(P.FacingRightFalling1),
        ],
        [P.FacingRightFalling1] =
        [
            Show(104, 5),
            GoTo(P.FacingRightFalling1),
        ],
        [P.FacingRightFalling2] =
        [
            Cue(new(K.PlayFootstepSound)),
            Cue(new(K.SpawnLandingDust)),
            Cue(new(K.GoToIfFaceBlownUpElseIfGolden, Target: P.FacingRightFacelessWalkingRightLegMoving, Alternate: P.GoldenWalkingRightRightLegMoving)),
            GoTo(P.FacingRightWalkingRightLegMoving),
        ],
        [P.FacingRightJumpBackwardLandRightFootFwd0] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.Jump)),
            Cue(new(K.Link, Target: P.FacingRightJumpBackwardLandRightFootFwd2)),
            Show(103, 5),
            Show(104, 5),
            FallInto(P.FacingRightJumpBackwardLandRightFootFwd1),
        ],
        [P.FacingRightJumpBackwardLandRightFootFwd1] =
        [
            Show(105, 1),
            Cue(new(K.GoToIfRising, Target: P.FacingRightJumpBackwardLandRightFootFwd1)),
            FallInto(P.FacingRightJumpBackwardLandRightFootFwd2),
        ],
        [P.FacingRightJumpBackwardLandRightFootFwd2] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.Jump)),
            Cue(new(K.Link, Target: P.FacingRightJumpBackwardLandRightFootFwd4)),
            FallInto(P.FacingRightJumpBackwardLandRightFootFwd3),
        ],
        [P.FacingRightJumpBackwardLandRightFootFwd3] =
        [
            Show(104, 5),
            GoTo(P.FacingRightJumpBackwardLandRightFootFwd3),
        ],
        [P.FacingRightJumpBackwardLandRightFootFwd4] =
        [
            Cue(new(K.PlayFootstepSound)),
            Cue(new(K.SpawnLandingDust)),
            Cue(new(K.GoToIfFaceBlownUpElseIfGolden, Target: P.FacingRightFacelessWalkingLeftLegMoving, Alternate: P.GoldenLandedFromBackwardsJumpFacingRightRightFootFwd)),
            Cue(new(K.CallBombTorizoAttack, Target: P.FacingRightSpewingChozoOrbsRightFootFwd0, Alternate: P.FacingRightSonicBoomsRightFootForward0)),
            GoTo(P.FacingRightWalkingLeftLegMoving),
        ],
        [P.FacingRightJumpBackwardsLandLeftFootFwd0] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.Jump)),
            Cue(new(K.Link, Target: P.FacingRightJumpBackwardsLandLeftFootFwd2)),
            Show(103, 5),
            Show(104, 5),
            FallInto(P.FacingRightJumpBackwardsLandLeftFootFwd1),
        ],
        [P.FacingRightJumpBackwardsLandLeftFootFwd1] =
        [
            Show(105, 1),
            Cue(new(K.GoToIfRising, Target: P.FacingRightJumpBackwardsLandLeftFootFwd1)),
            FallInto(P.FacingRightJumpBackwardsLandLeftFootFwd2),
        ],
        [P.FacingRightJumpBackwardsLandLeftFootFwd2] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.Jump)),
            Cue(new(K.Link, Target: P.FacingRightJumpBackwardsLandLeftFootFwd4)),
            FallInto(P.FacingRightJumpBackwardsLandLeftFootFwd3),
        ],
        [P.FacingRightJumpBackwardsLandLeftFootFwd3] =
        [
            Show(104, 5),
            GoTo(P.FacingRightJumpBackwardsLandLeftFootFwd3),
        ],
        [P.FacingRightJumpBackwardsLandLeftFootFwd4] =
        [
            Cue(new(K.PlayFootstepSound)),
            Cue(new(K.SpawnLandingDust)),
            Cue(new(K.GoToIfFaceBlownUpElseIfGolden, Target: P.FacingRightFacelessWalkingRightLegMoving, Alternate: P.GoldenLandedFromBackwardsJumpFacingRightLeftFootFwd)),
            Cue(new(K.CallBombTorizoAttack, Target: P.FacingRightSpewingChozoOrbsLeftFootFwd0, Alternate: P.FacingRightSonicBoomsLeftFootForward0)),
            GoTo(P.FacingRightWalkingRightLegMoving),
        ],
        [P.FacingRightFacelessTurningRight] =
        [
            Cue(new(K.SetFunction, Function: TorizoFunction.Falling)),
            Cue(new(K.SetTurningAround)),
            Show(1, 8),
            FallInto(P.FacingRightFacelessWalkingLeftLegMoving),
        ],
        [P.FacingRightFacelessWalkingLeftLegMoving] =
        [
            Cue(new(K.SteppedRightWithRightFoot)),
            Cue(new(K.SetFunction, Function: TorizoFunction.Active)),
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.AirTransition)),
            Cue(new(K.SpawnLowHealthDrool)),
            Cue(new(K.PlayFootstepSound)),
            Show(51, 1),
            Cue(new(K.CallIfSamusCloseBehind, Target: P.FacingRightExplosiveSwipeRightFootForward)),
            Cue(new(K.WalkFacelessBombTorizo, Value: 22)),
            Show(52, 5),
            Cue(new(K.WalkFacelessBombTorizo, Value: 24)),
            Show(53, 5),
            Cue(new(K.WalkFacelessBombTorizo, Value: 26)),
            Show(54, 5),
            Cue(new(K.WalkFacelessBombTorizo, Value: 28)),
            Show(55, 5),
            Cue(new(K.WalkFacelessBombTorizo, Value: 30)),
            FallInto(P.FacingRightFacelessWalkingRightLegMoving),
        ],
        [P.FacingRightFacelessWalkingRightLegMoving] =
        [
            Cue(new(K.SteppedRightWithLeftFoot)),
            Cue(new(K.SetFunction, Function: TorizoFunction.Active)),
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.AirTransition)),
            Cue(new(K.SpawnLowHealthDrool)),
            Cue(new(K.PlayFootstepSound)),
            Show(56, 1),
            Cue(new(K.CallIfSamusCloseBehind, Target: P.FacingRightExplosiveSwipeLeftFootForward)),
            Cue(new(K.WalkFacelessBombTorizo, Value: 32)),
            Show(57, 5),
            Cue(new(K.WalkFacelessBombTorizo, Value: 34)),
            Show(58, 5),
            Cue(new(K.WalkFacelessBombTorizo, Value: 36)),
            Show(59, 5),
            Cue(new(K.WalkFacelessBombTorizo, Value: 38)),
            Show(60, 5),
            Cue(new(K.WalkFacelessBombTorizo, Value: 20)),
            GoTo(P.FacingRightFacelessWalkingLeftLegMoving),
        ],
        [P.GoldenInitial0] =
        [
            CopyTiles(21),
            Cue(new(K.SteppedLeftWithRightFoot)),
            Cue(new(K.LockAnimation)),
            Cue(new(K.SetFunction, Function: TorizoFunction.GoldenWaitForSamus)),
            Show(47, 1),
            Hold,
        ],
        [P.GoldenInitial0Resumed] =
        [
            Cue(new(K.SetFunction, Function: TorizoFunction.Falling)),
            FallInto(P.GoldenInitial1),
        ],
        [P.GoldenInitial1] =
        [
            Show(47, 1),
            Cue(new(K.GoToIfAirborne, Target: P.GoldenInitial1)),
            Cue(new(K.PlayFootstepSound)),
            Show(47, 3),
            Cue(new(K.SitDown, Value: 4)),
            Show(46, 4),
            Cue(new(K.SitDown, Value: 2)),
            Show(45, 5),
            Cue(new(K.SitDown, Value: 0)),
            Cue(new(K.SetFunction, Function: TorizoFunction.Idle)),
            Show(44, 48),
            CopyTiles(22),
            Show(44, 32),
            CopyTiles(23),
            Show(44, 16),
            CopyTiles(24),
            Show(44, 8),
            CopyTiles(25),
            Show(44, 32),
            SetCounter(2),
            FallInto(P.GoldenInitial2),
        ],
        [P.GoldenInitial2] =
        [
            Show(44, 4),
            CopyTiles(26),
            Show(44, 4),
            CopyTiles(27),
            Show(44, 4),
            CopyTiles(28),
            Show(44, 4),
            CopyTiles(29),
            RepeatWhileCounting(P.GoldenInitial2),
            Show(44, 32),
            Cue(new(K.StandUp, Value: 0)),
            Show(45, 12),
            Cue(new(K.StandUp, Value: 2)),
            Show(46, 8),
            Cue(new(K.StandUp, Value: 4)),
            Show(47, 8),
            Cue(new(K.StandUp, Value: 6)),
            Show(48, 8),
            Cue(new(K.StandUp, Value: 8)),
            Show(49, 8),
            Cue(new(K.StandUp, Value: 10)),
            Cue(new(K.LoadGoldenTorizoPalettes)),
            SetCounter(16),
            FallInto(P.GoldenInitial3),
        ],
        [P.GoldenInitial3] =
        [
            Show(50, 4),
            Cue(new(K.AdvancePaletteFade)),
            RepeatWhileCounting(P.GoldenInitial3),
            Cue(new(K.NoOperation)),
            Cue(new(K.UnlockAnimation)),
            Cue(new(K.StartGoldenTorizoFight)),
            Show(50, 16),
            GoTo(P.GoldenWalkingLeftLeftLegMoving),
        ],
        [P.GoldenSpewChozoOrbsFaceLeftRightFootFwd0] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.GoldenGravity)),
            Show(12, 6),
            Show(13, 3),
            Show(14, 3),
            Show(15, 3),
            Show(16, 3),
            Show(17, 6),
            SetCounter(6),
            FallInto(P.GoldenSpewChozoOrbsFaceLeftRightFootFwd1),
        ],
        [P.GoldenSpewChozoOrbsFaceLeftRightFootFwd1] =
        [
            Cue(new(K.PlayShotSound)),
            Cue(new(K.SpawnGoldenTorizoChozoOrbs)),
            Wait(6),
            RepeatWhileCounting(P.GoldenSpewChozoOrbsFaceLeftRightFootFwd1),
            Show(16, 3),
            Show(15, 3),
            Show(14, 3),
            Show(13, 3),
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.GoldenAirTransition)),
            Cue(new(K.Return)),
            FallInto(P.GoldenSpewChozoOrbsFacingLeftLeftFootFwd0),
        ],
        [P.GoldenSpewChozoOrbsFacingLeftLeftFootFwd0] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.GoldenGravity)),
            Show(18, 6),
            Show(19, 3),
            Show(20, 3),
            Show(21, 3),
            Show(22, 3),
            Show(23, 6),
            SetCounter(6),
            FallInto(P.GoldenSpewChozoOrbsFacingLeftLeftFootFwd1),
        ],
        [P.GoldenSpewChozoOrbsFacingLeftLeftFootFwd1] =
        [
            Cue(new(K.PlayShotSound)),
            Cue(new(K.SpawnGoldenTorizoChozoOrbs)),
            Wait(6),
            RepeatWhileCounting(P.GoldenSpewChozoOrbsFacingLeftLeftFootFwd1),
            Show(22, 3),
            Show(21, 3),
            Show(20, 3),
            Show(19, 3),
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.GoldenAirTransition)),
            Cue(new(K.Return)),
            FallInto(P.GoldenSonicBoomsFacingLeftRightFootFwd0),
        ],
        [P.GoldenSonicBoomsFacingLeftRightFootFwd0] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.GoldenGravity)),
            SetCounter(4),
            FallInto(P.GoldenSonicBoomsFacingLeftRightFootFwd1),
        ],
        [P.GoldenSonicBoomsFacingLeftRightFootFwd1] =
        [
            Show(12, 3),
            Show(24, 3),
            Show(25, 3),
            Show(26, 3),
            Show(27, 3),
            Show(28, 3),
            Show(27, 1),
            Show(26, 1),
            Cue(new(K.SpawnGoldenTorizoSonicBoom, Value: 0)),
            Show(25, 1),
            Show(24, 4),
            Show(12, 3),
            Show(29, 3),
            Show(30, 3),
            Show(31, 3),
            Show(32, 3),
            Show(33, 3),
            Show(32, 1),
            Show(31, 1),
            Cue(new(K.SpawnGoldenTorizoSonicBoom, Value: 1)),
            Show(30, 1),
            Show(29, 4),
            RepeatWhileCounting(P.GoldenSonicBoomsFacingLeftRightFootFwd1),
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.GoldenAirTransition)),
            Cue(new(K.Return)),
            FallInto(P.GoldenSonicBoomsFacingLeftLeftFootFwd0),
        ],
        [P.GoldenSonicBoomsFacingLeftLeftFootFwd0] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.GoldenGravity)),
            SetCounter(4),
            FallInto(P.GoldenSonicBoomsFacingLeftLeftFootFwd1),
        ],
        [P.GoldenSonicBoomsFacingLeftLeftFootFwd1] =
        [
            Show(18, 3),
            Show(34, 3),
            Show(35, 3),
            Show(36, 3),
            Show(37, 3),
            Show(38, 3),
            Show(37, 1),
            Show(36, 1),
            Cue(new(K.SpawnGoldenTorizoSonicBoom, Value: 0)),
            Show(35, 1),
            Show(34, 4),
            Show(18, 3),
            Show(39, 3),
            Show(40, 3),
            Show(41, 3),
            Show(42, 3),
            Show(43, 3),
            Show(42, 1),
            Show(41, 1),
            Cue(new(K.SpawnGoldenTorizoSonicBoom, Value: 1)),
            Show(40, 1),
            Show(39, 4),
            RepeatWhileCounting(P.GoldenSonicBoomsFacingLeftLeftFootFwd1),
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.GoldenAirTransition)),
            Cue(new(K.Return)),
            FallInto(P.GoldenSpewChozoOrbFacingLeftLeftFootFwd0),
        ],
        [P.GoldenSpewChozoOrbFacingLeftLeftFootFwd0] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.GoldenGravity)),
            Show(61, 6),
            Show(62, 3),
            Show(63, 3),
            Show(64, 3),
            Show(65, 3),
            Show(66, 6),
            SetCounter(6),
            FallInto(P.GoldenSpewChozoOrbFacingLeftLeftFootFwd1),
        ],
        [P.GoldenSpewChozoOrbFacingLeftLeftFootFwd1] =
        [
            Cue(new(K.PlayShotSound)),
            Cue(new(K.SpawnGoldenTorizoChozoOrbs)),
            Wait(6),
            RepeatWhileCounting(P.GoldenSpewChozoOrbFacingLeftLeftFootFwd1),
            Show(65, 3),
            Show(64, 3),
            Show(63, 3),
            Show(62, 3),
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.GoldenAirTransition)),
            Cue(new(K.Return)),
            FallInto(P.GoldenSpewChozoOrbFacingLeftRightFootFwd0),
        ],
        [P.GoldenSpewChozoOrbFacingLeftRightFootFwd0] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.GoldenGravity)),
            Show(67, 6),
            Show(68, 3),
            Show(69, 3),
            Show(70, 3),
            Show(71, 3),
            Show(72, 6),
            SetCounter(6),
            FallInto(P.GoldenSpewChozoOrbFacingLeftRightFootFwd1),
        ],
        [P.GoldenSpewChozoOrbFacingLeftRightFootFwd1] =
        [
            Cue(new(K.PlayShotSound)),
            Cue(new(K.SpawnGoldenTorizoChozoOrbs)),
            Wait(6),
            RepeatWhileCounting(P.GoldenSpewChozoOrbFacingLeftRightFootFwd1),
            Show(71, 3),
            Show(70, 3),
            Show(69, 3),
            Show(68, 3),
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.GoldenAirTransition)),
            Cue(new(K.Return)),
            FallInto(P.GoldenSonicBoomsFacingRightLeftFootFwd0),
        ],
        [P.GoldenSonicBoomsFacingRightLeftFootFwd0] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.GoldenGravity)),
            SetCounter(4),
            FallInto(P.GoldenSonicBoomsFacingRightLeftFootFwd1),
        ],
        [P.GoldenSonicBoomsFacingRightLeftFootFwd1] =
        [
            Show(61, 3),
            Show(73, 3),
            Show(74, 3),
            Show(75, 3),
            Show(76, 3),
            Show(77, 3),
            Show(76, 1),
            Show(75, 1),
            Cue(new(K.SpawnGoldenTorizoSonicBoom, Value: 0)),
            Show(74, 1),
            Show(73, 4),
            Show(61, 3),
            Show(78, 3),
            Show(79, 3),
            Show(80, 3),
            Show(81, 3),
            Show(82, 3),
            Show(81, 1),
            Show(80, 1),
            Cue(new(K.SpawnGoldenTorizoSonicBoom, Value: 1)),
            Show(79, 1),
            Show(78, 4),
            RepeatWhileCounting(P.GoldenSonicBoomsFacingRightLeftFootFwd1),
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.GoldenAirTransition)),
            Cue(new(K.Return)),
            FallInto(P.GoldenSonicBoomsFacingRightRightFootFwd0),
        ],
        [P.GoldenSonicBoomsFacingRightRightFootFwd0] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.GoldenGravity)),
            SetCounter(4),
            FallInto(P.GoldenSonicBoomsFacingRightRightFootFwd1),
        ],
        [P.GoldenSonicBoomsFacingRightRightFootFwd1] =
        [
            Show(67, 3),
            Show(83, 3),
            Show(84, 3),
            Show(85, 3),
            Show(86, 3),
            Show(87, 3),
            Show(86, 1),
            Show(85, 1),
            Cue(new(K.SpawnGoldenTorizoSonicBoom, Value: 0)),
            Show(84, 1),
            Show(83, 4),
            Show(67, 3),
            Show(88, 3),
            Show(89, 3),
            Show(90, 3),
            Show(91, 3),
            Show(92, 3),
            Show(91, 1),
            Show(90, 1),
            Cue(new(K.SpawnGoldenTorizoSonicBoom, Value: 1)),
            Show(89, 1),
            Show(88, 4),
            RepeatWhileCounting(P.GoldenSonicBoomsFacingRightRightFootFwd1),
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.GoldenAirTransition)),
            Cue(new(K.Return)),
            FallInto(P.GoldenLandedFromBackwardsJumpFacingLeftLeftFootFwd),
        ],
        [P.GoldenLandedFromBackwardsJumpFacingLeftLeftFootFwd] =
        [
            Cue(new(K.CallGoldenTorizoAttack, Target: P.GoldenSpewChozoOrbsFacingLeftLeftFootFwd0, Alternate: P.GoldenSonicBoomsFacingLeftLeftFootFwd0)),
            GoTo(P.GoldenWalkingLeftRightLegMoving),
        ],
        [P.GoldenLandedFromBackwardsJumpFacingLeftRightFootFwd] =
        [
            Cue(new(K.CallGoldenTorizoAttack, Target: P.GoldenSpewChozoOrbsFaceLeftRightFootFwd0, Alternate: P.GoldenSonicBoomsFacingLeftRightFootFwd0)),
            GoTo(P.GoldenWalkingLeftLeftLegMoving),
        ],
        [P.GoldenLandedFromBackwardsJumpFacingRightRightFootFwd] =
        [
            Cue(new(K.CallGoldenTorizoAttack, Target: P.GoldenSpewChozoOrbFacingLeftRightFootFwd0, Alternate: P.GoldenSonicBoomsFacingRightRightFootFwd0)),
            GoTo(P.GoldenWalkingRightLeftLegMoving),
        ],
        [P.GoldenLandedFromBackwardsJumpFacingRightLeftFootFwd] =
        [
            Cue(new(K.CallGoldenTorizoAttack, Target: P.GoldenSpewChozoOrbFacingLeftLeftFootFwd0, Alternate: P.GoldenSonicBoomsFacingRightLeftFootFwd0)),
            GoTo(P.GoldenWalkingRightRightLegMoving),
        ],
        [P.GoldenCaughtSuperFacingLeftLeftLegFwd] =
        [
            Cue(new(K.PlaySonicBoomSound)),
            Cue(new(K.SetFunction, Function: TorizoFunction.GoldenGravity)),
            Show(18, 1),
            Show(34, 1),
            Show(35, 1),
            Show(36, 1),
            Show(37, 1),
            Show(38, 1),
            Show(37, 1),
            Show(36, 1),
            Show(35, 1),
            Show(34, 1),
            Show(34, 2),
            Show(35, 2),
            Show(36, 2),
            Show(37, 2),
            Cue(new(K.SpawnSuperMissile)),
            Show(38, 48),
            Show(37, 2),
            Show(36, 2),
            Show(35, 2),
            Show(34, 2),
            Show(18, 2),
            Show(39, 16),
            Cue(new(K.ClearCaughtSuperMissile)),
            GoTo(P.GoldenWalkingLeftRightLegMoving),
        ],
        [P.GoldenCaughtSuperFacingLeftRightLegFwd] =
        [
            Cue(new(K.PlaySonicBoomSound)),
            Cue(new(K.SetFunction, Function: TorizoFunction.GoldenGravity)),
            Show(12, 1),
            Show(24, 1),
            Show(25, 1),
            Show(26, 1),
            Show(27, 1),
            Show(28, 1),
            Show(27, 1),
            Show(26, 1),
            Show(25, 1),
            Show(24, 1),
            Show(24, 2),
            Show(25, 2),
            Show(26, 2),
            Show(27, 2),
            Cue(new(K.SpawnSuperMissile)),
            Show(28, 48),
            Show(27, 2),
            Show(26, 2),
            Show(25, 2),
            Show(24, 2),
            Show(12, 2),
            Show(29, 16),
            Cue(new(K.ClearCaughtSuperMissile)),
            GoTo(P.GoldenWalkingLeftLeftLegMoving),
        ],
        [P.GoldenCaughtSuperFacingRightRightLegFwd] =
        [
            Cue(new(K.PlaySonicBoomSound)),
            Cue(new(K.SetFunction, Function: TorizoFunction.GoldenGravity)),
            Show(67, 1),
            Show(83, 1),
            Show(84, 1),
            Show(85, 1),
            Show(86, 1),
            Show(87, 1),
            Show(86, 1),
            Show(85, 1),
            Show(84, 1),
            Show(84, 2),
            Show(85, 2),
            Show(86, 2),
            Cue(new(K.SpawnSuperMissile)),
            Show(87, 48),
            Show(86, 2),
            Show(85, 2),
            Show(84, 2),
            Show(83, 2),
            Show(67, 2),
            Show(88, 16),
            Cue(new(K.ClearCaughtSuperMissile)),
            GoTo(P.GoldenWalkingRightLeftLegMoving),
        ],
        [P.GoldenCaughtSuperFacingRightLeftLegFwd] =
        [
            Cue(new(K.PlaySonicBoomSound)),
            Cue(new(K.SetFunction, Function: TorizoFunction.GoldenGravity)),
            Show(61, 1),
            Show(73, 1),
            Show(74, 1),
            Show(75, 1),
            Show(76, 1),
            Show(77, 1),
            Show(76, 1),
            Show(75, 1),
            Show(74, 1),
            Show(74, 2),
            Show(75, 2),
            Show(76, 2),
            Cue(new(K.SpawnSuperMissile)),
            Show(77, 48),
            Show(76, 2),
            Show(75, 2),
            Show(74, 2),
            Show(73, 2),
            Show(61, 2),
            Show(78, 16),
            Cue(new(K.ClearCaughtSuperMissile)),
            GoTo(P.GoldenWalkingRightRightLegMoving),
        ],
        [P.GoldenSitDownAttackFacingLeft] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.GoldenGravity)),
            Show(50, 8),
            Cue(new(K.SitDown, Value: 10)),
            Show(49, 8),
            Cue(new(K.SitDown, Value: 8)),
            Show(48, 6),
            Cue(new(K.SitDown, Value: 6)),
            Show(47, 6),
            Cue(new(K.SitDown, Value: 4)),
            Show(46, 4),
            Cue(new(K.SitDown, Value: 2)),
            Show(45, 4),
            Cue(new(K.SitDown, Value: 0)),
            Show(44, 8),
            Cue(new(K.StandUp, Value: 0)),
            Show(45, 8),
            Cue(new(K.StandUp, Value: 2)),
            Show(46, 8),
            Cue(new(K.StandUp, Value: 4)),
            Show(47, 8),
            Cue(new(K.StandUp, Value: 6)),
            Show(48, 10),
            Cue(new(K.StandUp, Value: 8)),
            Show(49, 10),
            Cue(new(K.StandUp, Value: 10)),
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.GoldenAirTransition)),
            GoTo(P.GoldenWalkingLeftLeftLegMoving),
        ],
        [P.GoldenSitDownAttackFacingRight] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.GoldenGravity)),
            Show(99, 8),
            Cue(new(K.SitDown, Value: 26)),
            Show(98, 8),
            Cue(new(K.SitDown, Value: 24)),
            Show(97, 6),
            Cue(new(K.SitDown, Value: 22)),
            Show(96, 6),
            Cue(new(K.SitDown, Value: 20)),
            Show(95, 4),
            Cue(new(K.SitDown, Value: 18)),
            Show(94, 4),
            Cue(new(K.SitDown, Value: 16)),
            Show(93, 8),
            Cue(new(K.StandUp, Value: 16)),
            Show(94, 8),
            Cue(new(K.StandUp, Value: 18)),
            Show(95, 8),
            Cue(new(K.StandUp, Value: 20)),
            Show(96, 8),
            Cue(new(K.StandUp, Value: 22)),
            Show(97, 10),
            Cue(new(K.StandUp, Value: 24)),
            Show(98, 10),
            Cue(new(K.StandUp, Value: 26)),
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.GoldenAirTransition)),
            GoTo(P.GoldenWalkingRightRightLegMoving),
        ],
        [P.GoldenReleaseGoldenTorizoEggs0] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.GoldenGravity)),
            CopyTiles(30),
            CopyTiles(31),
            Wait(8),
            CopyTiles(32),
            CopyTiles(33),
            Wait(8),
            CopyTiles(34),
            CopyTiles(35),
            Wait(16),
            SetCounter(6),
            FallInto(P.GoldenReleaseGoldenTorizoEggs1),
        ],
        [P.GoldenReleaseGoldenTorizoEggs1] =
        [
            Cue(new(K.PlayEggReleasedSound)),
            Cue(new(K.SpawnEgg)),
            Wait(8),
            RepeatWhileCounting(P.GoldenReleaseGoldenTorizoEggs1),
            FallInto(P.GoldenReleaseGoldenTorizoEggs2),
        ],
        [P.GoldenReleaseGoldenTorizoEggs2] =
        [
            Wait(1),
            Cue(new(K.GoToIfEyeBeamActive, Target: P.GoldenReleaseGoldenTorizoEggs2)),
            CopyTiles(36),
            CopyTiles(37),
            Wait(8),
            CopyTiles(38),
            CopyTiles(39),
            Wait(8),
            CopyTiles(40),
            CopyTiles(41),
            Wait(8),
            CopyTiles(42),
            CopyTiles(43),
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.GoldenAirTransition)),
            Cue(new(K.Return)),
            Unreachable,
        ],
        [P.GoldenEyeBeamAttack0] =
        [
            Cue(new(K.SetFunction, Function: TorizoFunction.GoldenGravity)),
            Cue(new(K.DisableEyeBeamExplosions)),
            Cue(new(K.LockAnimation)),
            Wait(8),
            SetCounter(4),
            Cue(new(K.PlayLaserSound)),
            FallInto(P.GoldenEyeBeamAttack1),
        ],
        [P.GoldenEyeBeamAttack1] =
        [
            Cue(new(K.SpawnEyeBeam, Value: 0)),
            Wait(4),
            RepeatWhileCounting(P.GoldenEyeBeamAttack1),
            Wait(8),
            SetCounter(2),
            FallInto(P.GoldenEyeBeamAttack2),
        ],
        [P.GoldenEyeBeamAttack2] =
        [
            Wait(3),
            CopyTiles(44),
            Wait(3),
            CopyTiles(45),
            Wait(3),
            CopyTiles(46),
            Wait(3),
            CopyTiles(47),
            RepeatWhileCounting(P.GoldenEyeBeamAttack2),
            Cue(new(K.EnableEyeBeamExplosions)),
            Wait(8),
            Cue(new(K.DisableEyeBeamExplosions)),
            Cue(new(K.UnlockAnimation)),
            Cue(new(K.SetFunction, Function: TorizoFunction.GoldenPreInstruction)),
            Cue(new(K.Return)),
            Unreachable,
        ],
        [P.Stunned0] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.GoldenGravity)),
            Cue(new(K.LockAnimation)),
            Wait(24),
            SetCounter(2),
            FallInto(P.Stunned1),
        ],
        [P.Stunned1] =
        [
            Wait(3),
            CopyTiles(48),
            Wait(3),
            CopyTiles(49),
            Wait(3),
            CopyTiles(50),
            Wait(3),
            CopyTiles(51),
            RepeatWhileCounting(P.Stunned1),
            Wait(16),
            Cue(new(K.UnlockAnimation)),
            Cue(new(K.UnmarkStunned)),
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.GoldenAirTransition)),
            Cue(new(K.Return)),
            Unreachable,
        ],
        [P.GoldenDodgeTurningLeft] =
        [
            Cue(new(K.SetFunction, Function: TorizoFunction.Falling)),
            Cue(new(K.LockAnimation)),
            Cue(new(K.SetTurningAround)),
            Show(1, 24),
            Cue(new(K.UnlockAnimation)),
            GoTo(P.GoldenWalkingLeftRightLegMoving),
        ],
        [P.GoldenTurningLeft] =
        [
            Cue(new(K.SetFunction, Function: TorizoFunction.Falling)),
            Cue(new(K.SetTurningAround)),
            Show(1, 8),
            FallInto(P.GoldenWalkingLeftRightLegMoving),
        ],
        [P.GoldenWalkingLeftRightLegMoving] =
        [
            Cue(new(K.SteppedLeftWithLeftFoot)),
            Cue(new(K.SetFunction, Function: TorizoFunction.GoldenPreInstruction)),
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.GoldenAirTransition)),
            Cue(new(K.PlayFootstepSound)),
            Show(2, 8),
            Cue(new(K.JumpForwardIfSamusFar, Target: P.FacingLeftJumpingForwards0)),
            Cue(new(K.JumpBackIfSamusCloseInFrontGolden, Target: P.FacingLeftJumpingBackwardLandLeftFootFwd0)),
            Cue(new(K.CallIfStunHealthHigh, Target: P.Stunned0)),
            Cue(new(K.CallSometimesIfSamusMorphedInFront, Target: P.GoldenEyeBeamAttack0)),
            Cue(new(K.WalkGoldenTorizo, Value: 2)),
            Show(3, 4),
            Cue(new(K.WalkGoldenTorizo, Value: 4)),
            Show(4, 4),
            Cue(new(K.CallSometimesIfHealthLow, Target: P.GoldenReleaseGoldenTorizoEggs0)),
            Cue(new(K.JumpBackIfSamusCloseInFrontGolden, Target: P.FacingLeftJumpingBackwardLandLeftFootFwd0)),
            Cue(new(K.WalkGoldenTorizo, Value: 6)),
            Show(5, 4),
            Cue(new(K.WalkGoldenTorizo, Value: 8)),
            Show(6, 4),
            Cue(new(K.WalkGoldenTorizo, Value: 10)),
            FallInto(P.GoldenWalkingLeftLeftLegMoving),
        ],
        [P.GoldenWalkingLeftLeftLegMoving] =
        [
            Cue(new(K.SteppedLeftWithRightFoot)),
            Cue(new(K.SetFunction, Function: TorizoFunction.GoldenPreInstruction)),
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.GoldenAirTransition)),
            Cue(new(K.PlayFootstepSound)),
            Show(7, 8),
            Cue(new(K.GoToIfSamusMorphedBehind, Target: P.GoldenSitDownAttackFacingLeft)),
            Cue(new(K.JumpForwardIfSamusFar, Target: P.FacingLeftJumpingForwards0)),
            Cue(new(K.JumpBackIfSamusCloseInFrontGolden, Target: P.FacingLeftJumpingBackwardRightFootFwd0)),
            Cue(new(K.CallIfStunHealthHigh, Target: P.Stunned0)),
            Cue(new(K.CallSometimesIfSamusMorphedInFront, Target: P.GoldenEyeBeamAttack0)),
            Cue(new(K.WalkGoldenTorizo, Value: 12)),
            Show(8, 4),
            Cue(new(K.WalkGoldenTorizo, Value: 14)),
            Show(9, 4),
            Cue(new(K.CallSometimesIfHealthLow, Target: P.GoldenReleaseGoldenTorizoEggs0)),
            Cue(new(K.JumpBackIfSamusCloseInFrontGolden, Target: P.FacingLeftJumpingBackwardRightFootFwd0)),
            Cue(new(K.WalkGoldenTorizo, Value: 16)),
            Show(10, 4),
            Cue(new(K.WalkGoldenTorizo, Value: 18)),
            Show(11, 4),
            Cue(new(K.WalkGoldenTorizo, Value: 0)),
            GoTo(P.GoldenWalkingLeftRightLegMoving),
        ],
        [P.GoldenDodgeTurningRight] =
        [
            Cue(new(K.SetFunction, Function: TorizoFunction.Falling)),
            Cue(new(K.LockAnimation)),
            Cue(new(K.SetTurningAround)),
            Show(1, 24),
            Cue(new(K.UnlockAnimation)),
            GoTo(P.GoldenWalkingRightLeftLegMoving),
        ],
        [P.GoldenTurningRight] =
        [
            Cue(new(K.SetFunction, Function: TorizoFunction.Falling)),
            Cue(new(K.SetTurningAround)),
            Show(1, 8),
            FallInto(P.GoldenWalkingRightLeftLegMoving),
        ],
        [P.GoldenWalkingRightLeftLegMoving] =
        [
            Cue(new(K.SteppedRightWithRightFoot)),
            Cue(new(K.SetFunction, Function: TorizoFunction.GoldenPreInstruction)),
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.GoldenAirTransition)),
            Cue(new(K.PlayFootstepSound)),
            Show(51, 8),
            Cue(new(K.JumpForwardIfSamusFar, Target: P.FacingRightJumpingForwards0)),
            Cue(new(K.JumpBackIfSamusCloseInFrontGolden, Target: P.FacingRightJumpBackwardLandRightFootFwd0)),
            Cue(new(K.CallIfStunHealthHigh, Target: P.Stunned0)),
            Cue(new(K.CallSometimesIfSamusMorphedInFront, Target: P.GoldenEyeBeamAttack0)),
            Cue(new(K.WalkGoldenTorizo, Value: 22)),
            Show(52, 4),
            Cue(new(K.WalkGoldenTorizo, Value: 24)),
            Show(53, 4),
            Cue(new(K.CallSometimesIfHealthLow, Target: P.GoldenReleaseGoldenTorizoEggs0)),
            Cue(new(K.WalkGoldenTorizo, Value: 26)),
            Show(54, 4),
            Cue(new(K.JumpBackIfSamusCloseInFrontGolden, Target: P.FacingRightJumpBackwardLandRightFootFwd0)),
            Cue(new(K.WalkGoldenTorizo, Value: 28)),
            Show(55, 4),
            Cue(new(K.WalkGoldenTorizo, Value: 30)),
            FallInto(P.GoldenWalkingRightRightLegMoving),
        ],
        [P.GoldenWalkingRightRightLegMoving] =
        [
            Cue(new(K.SteppedRightWithLeftFoot)),
            Cue(new(K.SetFunction, Function: TorizoFunction.GoldenPreInstruction)),
            Cue(new(K.SetPreInstruction, PreInstruction: TorizoPreInstruction.GoldenAirTransition)),
            Cue(new(K.PlayFootstepSound)),
            Show(56, 8),
            Cue(new(K.GoToIfSamusMorphedBehind, Target: P.GoldenSitDownAttackFacingRight)),
            Cue(new(K.JumpForwardIfSamusFar, Target: P.FacingRightJumpingForwards0)),
            Cue(new(K.JumpBackIfSamusCloseInFrontGolden, Target: P.FacingRightJumpBackwardsLandLeftFootFwd0)),
            Cue(new(K.CallIfStunHealthHigh, Target: P.Stunned0)),
            Cue(new(K.CallSometimesIfSamusMorphedInFront, Target: P.GoldenEyeBeamAttack0)),
            Cue(new(K.WalkGoldenTorizo, Value: 32)),
            Show(57, 4),
            Cue(new(K.WalkGoldenTorizo, Value: 34)),
            Show(58, 4),
            Cue(new(K.CallSometimesIfHealthLow, Target: P.GoldenReleaseGoldenTorizoEggs0)),
            Cue(new(K.JumpBackIfSamusCloseInFrontGolden, Target: P.FacingRightJumpBackwardsLandLeftFootFwd0)),
            Cue(new(K.WalkGoldenTorizo, Value: 36)),
            Show(59, 4),
            Cue(new(K.WalkGoldenTorizo, Value: 38)),
            Show(60, 4),
            Cue(new(K.WalkGoldenTorizo, Value: 20)),
            GoTo(P.GoldenWalkingRightLeftLegMoving),
        ],
    });

    /// <summary>
    /// The program a sleeping list resumes into when the AI steps its pointer past the sleep, as Bomb Torizo's wake
    /// and Golden Torizo's trigger do.
    /// </summary>
    internal static TorizoBodyProgram AfterSleep(TorizoBodyProgram program) => program switch
    {
        P.BombTorizoInitial0
            or P.FacingLeftFacelessWalkingLeftLegMovingTrailing
            or P.GoldenInitial0 => EnumOrder.Next(program),
        P.SpecialCallableBlowUpBombTorizosGut or
            P.CallableBlowUpBombTorizosFace or
            P.DeathSequence0 or
            P.DeathSequence1 or
            P.DeathSequence2 or
            P.BombTorizoInitial0Resumed or
            P.BombTorizoInitial1 or
            P.BombTorizoInitial2 or
            P.FacingLeftTurningLeft or
            P.FacingLeftWalkingRightLegMoving or
            P.FacingLeftWalkingLeftLegMoving or
            P.FacingLeftSpewingChozoOrbsRightFootFwd0 or
            P.FacingLeftSpewingChozoOrbsRightFootFwd1 or
            P.FacingLeftSpewingChozoOrbsLeftFootFwd0 or
            P.FacingLeftSpewingChozoOrbsLeftFootFwd1 or
            P.FacingLeftSonicBoomsRightFootForward0 or
            P.FacingLeftSonicBoomsRightFootForward1 or
            P.FacingLeftSonicBoomsLeftFootForward0 or
            P.FacingLeftSonicBoomsLeftFootForward1 or
            P.FacingLeftExplosiveSwipeRightFootForward or
            P.FacingLeftExplosiveSwipeLeftFootForward or
            P.FacingLeftJumpingForwards0 or
            P.FacingLeftJumpingForwards1 or
            P.FacingLeftFalling0 or
            P.FacingLeftFalling1 or
            P.FacingLeftFalling2 or
            P.FacingLeftJumpingBackwardLandLeftFootFwd0 or
            P.FacingLeftJumpingBackwardLandLeftFootFwd1 or
            P.FacingLeftJumpingBackwardLandLeftFootFwd2 or
            P.FacingLeftJumpingBackwardLandLeftFootFwd3 or
            P.FacingLeftJumpingBackwardLandLeftFootFwd4 or
            P.FacingLeftJumpingBackwardRightFootFwd0 or
            P.FacingLeftJumpingBackwardRightFootFwd1 or
            P.FacingLeftJumpingBackwardRightFootFwd2 or
            P.FacingLeftJumpingBackwardRightFootFwd3 or
            P.FacingLeftJumpingBackwardRightFootFwd4 or
            P.FacingLeftFacelessTurningLeft or
            P.FacingLeftFacelessWalkingRightLegMoving or
            P.FacingLeftFacelessWalkingLeftLegMoving or
            P.FacingLeftFacelessWalkingLeftLegMovingTrailingResumed or
            P.FacingRightTurningRight or
            P.FacingRightWalkingLeftLegMoving or
            P.FacingRightWalkingRightLegMoving or
            P.FacingRightSpewingChozoOrbsLeftFootFwd0 or
            P.FacingRightSpewingChozoOrbsLeftFootFwd1 or
            P.FacingRightSpewingChozoOrbsRightFootFwd0 or
            P.FacingRightSpewingChozoOrbsRightFootFwd1 or
            P.FacingRightSonicBoomsLeftFootForward0 or
            P.FacingRightSonicBoomsLeftFootForward1 or
            P.FacingRightSonicBoomsRightFootForward0 or
            P.FacingRightSonicBoomsRightFootForward1 or
            P.FacingRightExplosiveSwipeLeftFootForward or
            P.FacingRightExplosiveSwipeRightFootForward or
            P.FacingRightJumpingForwards0 or
            P.FacingRightJumpingForwards1 or
            P.FacingRightFalling0 or
            P.FacingRightFalling1 or
            P.FacingRightFalling2 or
            P.FacingRightJumpBackwardLandRightFootFwd0 or
            P.FacingRightJumpBackwardLandRightFootFwd1 or
            P.FacingRightJumpBackwardLandRightFootFwd2 or
            P.FacingRightJumpBackwardLandRightFootFwd3 or
            P.FacingRightJumpBackwardLandRightFootFwd4 or
            P.FacingRightJumpBackwardsLandLeftFootFwd0 or
            P.FacingRightJumpBackwardsLandLeftFootFwd1 or
            P.FacingRightJumpBackwardsLandLeftFootFwd2 or
            P.FacingRightJumpBackwardsLandLeftFootFwd3 or
            P.FacingRightJumpBackwardsLandLeftFootFwd4 or
            P.FacingRightFacelessTurningRight or
            P.FacingRightFacelessWalkingLeftLegMoving or
            P.FacingRightFacelessWalkingRightLegMoving or
            P.GoldenInitial0Resumed or
            P.GoldenInitial1 or
            P.GoldenInitial2 or
            P.GoldenInitial3 or
            P.GoldenSpewChozoOrbsFaceLeftRightFootFwd0 or
            P.GoldenSpewChozoOrbsFaceLeftRightFootFwd1 or
            P.GoldenSpewChozoOrbsFacingLeftLeftFootFwd0 or
            P.GoldenSpewChozoOrbsFacingLeftLeftFootFwd1 or
            P.GoldenSonicBoomsFacingLeftRightFootFwd0 or
            P.GoldenSonicBoomsFacingLeftRightFootFwd1 or
            P.GoldenSonicBoomsFacingLeftLeftFootFwd0 or
            P.GoldenSonicBoomsFacingLeftLeftFootFwd1 or
            P.GoldenSpewChozoOrbFacingLeftLeftFootFwd0 or
            P.GoldenSpewChozoOrbFacingLeftLeftFootFwd1 or
            P.GoldenSpewChozoOrbFacingLeftRightFootFwd0 or
            P.GoldenSpewChozoOrbFacingLeftRightFootFwd1 or
            P.GoldenSonicBoomsFacingRightLeftFootFwd0 or
            P.GoldenSonicBoomsFacingRightLeftFootFwd1 or
            P.GoldenSonicBoomsFacingRightRightFootFwd0 or
            P.GoldenSonicBoomsFacingRightRightFootFwd1 or
            P.GoldenLandedFromBackwardsJumpFacingLeftLeftFootFwd or
            P.GoldenLandedFromBackwardsJumpFacingLeftRightFootFwd or
            P.GoldenLandedFromBackwardsJumpFacingRightRightFootFwd or
            P.GoldenLandedFromBackwardsJumpFacingRightLeftFootFwd or
            P.GoldenCaughtSuperFacingLeftLeftLegFwd or
            P.GoldenCaughtSuperFacingLeftRightLegFwd or
            P.GoldenCaughtSuperFacingRightRightLegFwd or
            P.GoldenCaughtSuperFacingRightLeftLegFwd or
            P.GoldenSitDownAttackFacingLeft or
            P.GoldenSitDownAttackFacingRight or
            P.GoldenReleaseGoldenTorizoEggs0 or
            P.GoldenReleaseGoldenTorizoEggs1 or
            P.GoldenReleaseGoldenTorizoEggs2 or
            P.GoldenEyeBeamAttack0 or
            P.GoldenEyeBeamAttack1 or
            P.GoldenEyeBeamAttack2 or
            P.Stunned0 or
            P.Stunned1 or
            P.GoldenDodgeTurningLeft or
            P.GoldenTurningLeft or
            P.GoldenWalkingLeftRightLegMoving or
            P.GoldenWalkingLeftLeftLegMoving or
            P.GoldenDodgeTurningRight or
            P.GoldenTurningRight or
            P.GoldenWalkingRightLeftLegMoving or
            P.GoldenWalkingRightRightLegMoving => throw new ArgumentOutOfRangeException(nameof(program), program, "The program does not sleep before authored code."),
        _ => throw new ArgumentOutOfRangeException(nameof(program), program, "The program does not sleep before authored code."),
    };

    /// <summary>
    /// The extended spritemaps <paramref name="programs"/> show, in authored order with repeats: the artwork schema's
    /// first-seen frame order.
    /// </summary>
    internal static IEnumerable<ushort> Spritemaps(params TorizoBodyProgram[] programs) =>
        programs.SelectMany(program => Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => TorizoFrameDefinitions.Spritemap(step.Frame));

    /// <summary>Every body program, in authored address order.</summary>
    internal static TorizoBodyProgram[] AllPrograms { get; } = Enum.GetValues<TorizoBodyProgram>();

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue> Show(int pose, int duration) =>
        AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue> Cue(TorizoCue cue) =>
        AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue>.Publish(cue);

    /// <summary>Copies tile transfer <paramref name="transfer"/> to VRAM.</summary>
    private static AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue> CopyTiles(int transfer) =>
        Cue(new(K.CopyTiles, Transfer: transfer));

    /// <summary>Loads the loop counter with <paramref name="count"/>.</summary>
    private static AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue> SetCounter(int count) =>
        AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue>.SetCounter(count);

    /// <summary>Repeats from <paramref name="program"/> while the loop counter counts down.</summary>
    private static AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue> RepeatWhileCounting(TorizoBodyProgram program) =>
        AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue>.RepeatWhileCounting(program);

    /// <summary>Waits <paramref name="updates"/> updates on the current frame.</summary>
    private static AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue> Wait(int updates) =>
        AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue>.Wait(updates);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue> GoTo(TorizoBodyProgram program) =>
        AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue> FallInto(TorizoBodyProgram program) =>
        AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue>.FallInto(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue> Hold =>
        AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue>.Hold;

    /// <summary>Ends the actor's program, as the native stop does.</summary>
    private static AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue> Stop =>
        AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue>.Stop;

    /// <summary>Closes a program after a cue that always redirects; the program never reaches it.</summary>
    private static AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue> Unreachable => Hold;
}
