using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Bosses.Torizo.TorizoCueKind;
using P = SuperMetroid.Core.Bosses.Torizo.TorizoBodySegment;
using static SuperMetroid.Core.Bosses.Torizo.TorizoAnimationSteps;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The encounter animations both Torizos share: the gut and face blow-ups, the death sequence and the opening.</summary>
internal static class TorizoEncounterAnimations
{
    /// <summary>Each segment and its beats.</summary>
    internal static Dictionary<TorizoBodySegment, AnimationBeat<EnemyFrameId, TorizoCue>[]> Segments { get; } = new()
    {
        [P.SpecialCallableBlowUpBombTorizosGut] =
        [
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Idle)),
            Cue(new TorizoCue.Signal(K.LockAnimation)),
            Cue(new TorizoCue.LowHealthExplosions(BombTorizoExplosionSite.Gut)),
            Wait(40),
            Cue(new TorizoCue.Signal(K.BlowUpGut)),
            ShowArtwork(0),
            ShowArtwork(1),
            ShowArtwork(2),
            ShowArtwork(3),
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Active)),
            Cue(new TorizoCue.Signal(K.UnlockAnimation)),
            Cue(new TorizoCue.Signal(K.ResumeInterrupted)),
        ],
        [P.CallableBlowUpBombTorizosFace] =
        [
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Idle)),
            Cue(new TorizoCue.Signal(K.LockAnimation)),
            Cue(new TorizoCue.LowHealthExplosions(BombTorizoExplosionSite.Face)),
            Wait(40),
            Cue(new TorizoCue.Signal(K.MarkFaceBlownUp)),
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
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Active)),
            Cue(new TorizoCue.Signal(K.UnlockAnimation)),
            Cue(new TorizoCue.Signal(K.Return)),
        ],
        [P.DeathSequence0] =
        [
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Idle)),
            Cue(new TorizoCue.Signal(K.LockAnimation)),
        ],
        [P.DeathSequence1] =
        [
            Cue(new TorizoCue.Signal(K.SpawnDeathExplosion)),
            Wait(1),
            Wait(6),
            Cue(new TorizoCue.Signal(K.FadeToBlack)),
        ],
        [P.DeathSequence2] =
        [
            Cue(new TorizoCue.Signal(K.SpawnDeathExplosion)),
            Wait(1),
            Cue(new TorizoCue.Signal(K.MakeVisible)),
            Wait(2),
            Cue(new TorizoCue.Signal(K.MakeInvisible)),
            Wait(2),
            Cue(new TorizoCue.Signal(K.SpawnDeathExplosion)),
            Wait(1),
            Cue(new TorizoCue.Signal(K.MakeVisible)),
            Wait(2),
            Cue(new TorizoCue.Signal(K.MakeInvisible)),
            Wait(2),
            Cue(new TorizoCue.Signal(K.MakeVisible)),
            Wait(2),
            Cue(new TorizoCue.Signal(K.MakeInvisible)),
            Wait(2),
            Cue(new TorizoCue.Signal(K.MakeVisible)),
            Wait(2),
            Cue(new TorizoCue.Signal(K.MakeInvisible)),
            Wait(2),
            Cue(new TorizoCue.Signal(K.AdvanceColourFade)),
            Wait(64),
            Cue(new TorizoCue.Signal(K.FinishDeath)),
        ],
        [P.BombTorizoInitial0] =
        [
            Cue(new TorizoCue.Signal(K.SteppedLeftWithRightFoot)),
            Cue(new TorizoCue.Signal(K.LockAnimation)),
            Show(EnemyFrameId.BombTorizoDormant, 1),
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.WaitForHandTrigger)),
        ],
        [P.BombTorizoInitial0Resumed] =
        [
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Falling)),
            Show(EnemyFrameId.TorizoFacingLeftSeated, 48),
            ShowArtwork(13),
            Show(EnemyFrameId.TorizoFacingLeftSeated, 32),
            ShowArtwork(14),
            Show(EnemyFrameId.TorizoFacingLeftSeated, 16),
            ShowArtwork(15),
            Show(EnemyFrameId.TorizoFacingLeftSeated, 8),
            ShowArtwork(16),
            Show(EnemyFrameId.TorizoFacingLeftSeated, 32),
        ],
        [P.BombTorizoInitial1] =
        [
            Show(EnemyFrameId.TorizoFacingLeftSeated, 4),
            ShowArtwork(17),
            Show(EnemyFrameId.TorizoFacingLeftSeated, 4),
            ShowArtwork(18),
            Show(EnemyFrameId.TorizoFacingLeftSeated, 4),
            ShowArtwork(19),
            Show(EnemyFrameId.TorizoFacingLeftSeated, 4),
            ShowArtwork(20),
            Show(EnemyFrameId.TorizoFacingLeftSeated, 48),
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, Step: 0)),
            Show(EnemyFrameId.TorizoFacingLeftStandingUpFirst, 16),
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, Step: 1)),
            Show(EnemyFrameId.TorizoFacingLeftStandingUpSecond, 8),
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, Step: 2)),
            Show(EnemyFrameId.GoldenTorizoInitial, 8),
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, Step: 3)),
            Show(EnemyFrameId.TorizoFacingLeftStandingUpFourth, 8),
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, Step: 4)),
            Show(EnemyFrameId.TorizoFacingLeftStandingUpFifth, 8),
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, Step: 5)),
            Cue(new TorizoCue.Signal(K.FadeToNormalColors)),
        ],
        [P.BombTorizoInitial2] =
        [
            Show(EnemyFrameId.TorizoFacingLeftStandingUpSixth, 4),
            Cue(new TorizoCue.Signal(K.AdvanceColourFade)),
            Cue(new TorizoCue.Signal(K.NoOperation)),
            Cue(new TorizoCue.Signal(K.UnlockAnimation)),
            Cue(new TorizoCue.Signal(K.StartBombTorizoFight)),
            Show(EnemyFrameId.TorizoFacingLeftStandingUpSixth, 16),
        ],
        [P.Stunned0] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.GoldenGravity)),
            Cue(new TorizoCue.Signal(K.LockAnimation)),
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
            Cue(new TorizoCue.Signal(K.UnlockAnimation)),
            Cue(new TorizoCue.Signal(K.UnmarkStunned)),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.GoldenAirTransition)),
            Cue(new TorizoCue.Signal(K.Return)),
        ],
    };
}
