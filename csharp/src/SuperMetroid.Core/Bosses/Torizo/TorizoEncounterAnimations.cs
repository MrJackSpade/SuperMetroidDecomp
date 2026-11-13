using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Bosses.Torizo.TorizoCueKind;
using P = SuperMetroid.Core.Bosses.Torizo.TorizoBodySegment;
using static SuperMetroid.Core.Bosses.Torizo.TorizoAnimationSteps;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The encounter programs both Torizos share: the gut and face blow-ups, the death sequence and the opening.</summary>
internal static class TorizoEncounterAnimations
{
    /// <summary>Each segment and its beats.</summary>
    internal static Dictionary<TorizoBodySegment, AnimationBeat<EnemyFrameId, TorizoCue>[]> Segments { get; } = new()
    {
        [P.SpecialCallableBlowUpBombTorizosGut] =
        [
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.Idle)),
            Cue(new TorizoCue.Command(K.LockAnimation)),
            Cue(new TorizoCue.LowHealthExplosions(BombTorizoExplosionSite.Gut)),
            Wait(40),
            Cue(new TorizoCue.Command(K.BlowUpGut)),
            ShowArtwork(0),
            ShowArtwork(1),
            ShowArtwork(2),
            ShowArtwork(3),
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.Active)),
            Cue(new TorizoCue.Command(K.UnlockAnimation)),
            Cue(new TorizoCue.Command(K.ResumeInterrupted)),
        ],
        [P.CallableBlowUpBombTorizosFace] =
        [
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.Idle)),
            Cue(new TorizoCue.Command(K.LockAnimation)),
            Cue(new TorizoCue.LowHealthExplosions(BombTorizoExplosionSite.Face)),
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
        ],
        [P.DeathSequence0] =
        [
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.Idle)),
            Cue(new TorizoCue.Command(K.LockAnimation)),
        ],
        [P.DeathSequence1] =
        [
            Cue(new TorizoCue.Command(K.SpawnDeathExplosion)),
            Wait(1),
            Wait(6),
            Cue(new TorizoCue.Command(K.FadeToBlack)),
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
            Cue(new TorizoCue.Command(K.AdvanceColourFade)),
            Wait(64),
            Cue(new TorizoCue.Command(K.FinishDeath)),
        ],
        [P.BombTorizoInitial0] =
        [
            Cue(new TorizoCue.Command(K.SteppedLeftWithRightFoot)),
            Cue(new TorizoCue.Command(K.LockAnimation)),
            Show(EnemyFrameId.BombTorizoDormant, 1),
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.WaitForHandTrigger)),
        ],
        [P.BombTorizoInitial0Resumed] =
        [
            Cue(new TorizoCue.SetFunction(Function: TorizoFunction.Falling)),
            Show(EnemyFrameId.BombTorizoInitialResumed, 48),
            ShowArtwork(13),
            Show(EnemyFrameId.BombTorizoInitialResumed, 32),
            ShowArtwork(14),
            Show(EnemyFrameId.BombTorizoInitialResumed, 16),
            ShowArtwork(15),
            Show(EnemyFrameId.BombTorizoInitialResumed, 8),
            ShowArtwork(16),
            Show(EnemyFrameId.BombTorizoInitialResumed, 32),
        ],
        [P.BombTorizoInitial1] =
        [
            Show(EnemyFrameId.BombTorizoInitialResumed, 4),
            ShowArtwork(17),
            Show(EnemyFrameId.BombTorizoInitialResumed, 4),
            ShowArtwork(18),
            Show(EnemyFrameId.BombTorizoInitialResumed, 4),
            ShowArtwork(19),
            Show(EnemyFrameId.BombTorizoInitialResumed, 4),
            ShowArtwork(20),
            Show(EnemyFrameId.BombTorizoInitialResumed, 48),
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, Step: 0)),
            Show(EnemyFrameId.BombTorizoInitialThird, 16),
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, Step: 1)),
            Show(EnemyFrameId.BombTorizoInitialFourth, 8),
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, Step: 2)),
            Show(EnemyFrameId.GoldenTorizoInitial, 8),
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, Step: 3)),
            Show(EnemyFrameId.BombTorizoInitialSixth, 8),
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, Step: 4)),
            Show(EnemyFrameId.BombTorizoInitialSeventh, 8),
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, Step: 5)),
            Cue(new TorizoCue.Command(K.FadeToNormalColors)),
        ],
        [P.BombTorizoInitial2] =
        [
            Show(EnemyFrameId.BombTorizoInitialEighth, 4),
            Cue(new TorizoCue.Command(K.AdvanceColourFade)),
            Cue(new TorizoCue.Command(K.NoOperation)),
            Cue(new TorizoCue.Command(K.UnlockAnimation)),
            Cue(new TorizoCue.Command(K.StartBombTorizoFight)),
            Show(EnemyFrameId.BombTorizoInitialEighth, 16),
        ],
        [P.Stunned0] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.GoldenGravity)),
            Cue(new TorizoCue.Command(K.LockAnimation)),
            Wait(24),
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
            Wait(16),
            Cue(new TorizoCue.Command(K.UnlockAnimation)),
            Cue(new TorizoCue.Command(K.UnmarkStunned)),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.GoldenAirTransition)),
            Cue(new TorizoCue.Command(K.Return)),
        ],
    };
}
