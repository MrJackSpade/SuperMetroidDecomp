using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Bosses.Torizo.TorizoCueKind;
using P = SuperMetroid.Core.Bosses.Torizo.TorizoBodyProgram;
using static SuperMetroid.Core.Bosses.Torizo.TorizoProgramSteps;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The encounter programs both Torizos share: the gut and face blow-ups, the death sequence and the opening.</summary>
internal static class TorizoEncounterPrograms
{
    /// <summary>Each program and its steps.</summary>
    internal static Dictionary<TorizoBodyProgram, AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue>[]> Programs { get; } = new()
    {
        [P.SpecialCallableBlowUpBombTorizosGut] =
        [
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.Idle)),
            Cue(new TorizoCue.Command(K.LockAnimation)),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnLowHealthExplosions, Parameter: 0)),
            Wait(40),
            Cue(new TorizoCue.Command(K.BlowUpGut)),
            CopyTiles(0),
            CopyTiles(1),
            CopyTiles(2),
            CopyTiles(3),
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.Active)),
            Cue(new TorizoCue.Command(K.UnlockAnimation)),
            Cue(new TorizoCue.Command(K.ResumeInterrupted)),
            Unreachable,
        ],
        [P.CallableBlowUpBombTorizosFace] =
        [
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.Idle)),
            Cue(new TorizoCue.Command(K.LockAnimation)),
            Cue(new TorizoCue.ParameterizedSpawn(TorizoParameterizedSpawnKind.SpawnLowHealthExplosions, Parameter: 6)),
            Wait(40),
            Cue(new TorizoCue.Command(K.MarkFaceBlownUp)),
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
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.Active)),
            Cue(new TorizoCue.Command(K.UnlockAnimation)),
            Cue(new TorizoCue.Command(K.Return)),
            Unreachable,
        ],
        [P.DeathSequence0] =
        [
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.Idle)),
            Cue(new TorizoCue.Command(K.LockAnimation)),
            SetCounter(8),
            FallInto(P.DeathSequence1),
        ],
        [P.DeathSequence1] =
        [
            Cue(new TorizoCue.Command(K.SpawnDeathExplosion)),
            Wait(1),
            Wait(6),
            RepeatWhileCounting(P.DeathSequence1),
            Cue(new TorizoCue.Command(K.FadeToBlack)),
            SetCounter(14),
            FallInto(P.DeathSequence2),
        ],
        [P.DeathSequence2] =
        [
            Cue(new TorizoCue.Command(K.SpawnDeathExplosion)),
            Wait(1),
            Cue(new TorizoCue.Command(K.MakeVisible)),
            Wait(2),
            Cue(new TorizoCue.Command(K.MakeInvisible)),
            Wait(2),
            Cue(new TorizoCue.Command(K.SpawnDeathExplosion)),
            Wait(1),
            Cue(new TorizoCue.Command(K.MakeVisible)),
            Wait(2),
            Cue(new TorizoCue.Command(K.MakeInvisible)),
            Wait(2),
            Cue(new TorizoCue.Command(K.MakeVisible)),
            Wait(2),
            Cue(new TorizoCue.Command(K.MakeInvisible)),
            Wait(2),
            Cue(new TorizoCue.Command(K.MakeVisible)),
            Wait(2),
            Cue(new TorizoCue.Command(K.MakeInvisible)),
            Wait(2),
            Cue(new TorizoCue.Command(K.AdvancePaletteFade)),
            RepeatWhileCounting(P.DeathSequence2),
            Wait(64),
            Cue(new TorizoCue.Command(K.FinishDeath)),
            Stop,
        ],
        [P.BombTorizoInitial0] =
        [
            Cue(new TorizoCue.Command(K.SteppedLeftWithRightFoot)),
            Cue(new TorizoCue.Command(K.LockAnimation)),
            Show(0, 1),
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.WaitForHandTrigger)),
            Hold,
        ],
        [P.BombTorizoInitial0Resumed] =
        [
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.Falling)),
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
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, MapOffset: 0)),
            Show(45, 16),
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, MapOffset: 2)),
            Show(46, 8),
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, MapOffset: 4)),
            Show(47, 8),
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, MapOffset: 6)),
            Show(48, 8),
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, MapOffset: 8)),
            Show(49, 8),
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, MapOffset: 10)),
            Cue(new TorizoCue.Command(K.FadeToNormalColors)),
            SetCounter(16),
            FallInto(P.BombTorizoInitial2),
        ],
        [P.BombTorizoInitial2] =
        [
            Show(50, 4),
            Cue(new TorizoCue.Command(K.AdvancePaletteFade)),
            RepeatWhileCounting(P.BombTorizoInitial2),
            Cue(new TorizoCue.Command(K.NoOperation)),
            Cue(new TorizoCue.Command(K.UnlockAnimation)),
            Cue(new TorizoCue.Command(K.StartBombTorizoFight)),
            Show(50, 16),
            GoTo(P.FacingLeftWalkingLeftLegMoving),
        ],
        [P.Stunned0] =
        [
            Cue(new TorizoCue.SetPreInstruction(PreInstruction: TorizoPreInstruction.GoldenGravity)),
            Cue(new TorizoCue.Command(K.LockAnimation)),
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
            Cue(new TorizoCue.Command(K.UnlockAnimation)),
            Cue(new TorizoCue.Command(K.UnmarkStunned)),
            Cue(new TorizoCue.SetPreInstruction(PreInstruction: TorizoPreInstruction.GoldenAirTransition)),
            Cue(new TorizoCue.Command(K.Return)),
            Unreachable,
        ],
    };
}
