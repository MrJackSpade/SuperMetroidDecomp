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
            ShowArtwork(0),
            ShowArtwork(1),
            ShowArtwork(2),
            ShowArtwork(3),
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
            ShowArtwork(4),
            ShowArtwork(5),
            ShowArtwork(6),
            ShowArtwork(7),
            Wait(1),
            ShowArtwork(8),
            ShowArtwork(9),
            ShowArtwork(10),
            ShowArtwork(11),
            ShowArtwork(12),
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
            ShowArtwork(13),
            Show(44, 32),
            ShowArtwork(14),
            Show(44, 16),
            ShowArtwork(15),
            Show(44, 8),
            ShowArtwork(16),
            Show(44, 32),
            SetCounter(2),
            FallInto(P.BombTorizoInitial1),
        ],
        [P.BombTorizoInitial1] =
        [
            Show(44, 4),
            ShowArtwork(17),
            Show(44, 4),
            ShowArtwork(18),
            Show(44, 4),
            ShowArtwork(19),
            Show(44, 4),
            ShowArtwork(20),
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
            ShowArtwork(48),
            Wait(3),
            ShowArtwork(49),
            Wait(3),
            ShowArtwork(50),
            Wait(3),
            ShowArtwork(51),
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
