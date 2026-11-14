using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Bosses.Torizo.TorizoCueKind;
using A = SuperMetroid.Core.Bosses.Torizo.TorizoAnimation;
using S = SuperMetroid.Core.Bosses.Torizo.TorizoSharedSteps;
using L = SuperMetroid.Core.Bosses.Torizo.TorizoArtworkLook;
using static SuperMetroid.Core.Bosses.Torizo.TorizoAnimationSteps;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The encounter animations both Torizos share: the gut and face blow-ups, the death, the stun and the Bomb Torizo's wake.</summary>
internal static class TorizoEncounterAnimations
{
    /// <summary>Each animation's steps, in the order it plays them.</summary>
    internal static Dictionary<TorizoAnimation, AnimationBeat<EnemyFrameId, TorizoCue>[]> Segments { get; } = new()
    {
        [A.BombTorizoGutBlowingUp] =
        [
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Idle)),
            Cue(new TorizoCue.Signal(K.LockAnimation)),
            Cue(new TorizoCue.LowHealthExplosions(BombTorizoExplosionSite.Gut)),
            Wait(40),
            Cue(new TorizoCue.Signal(K.BlowUpGut)),
            ShowArtwork(L.GutBlowingUp1),
            ShowArtwork(L.GutBlowingUp2),
            ShowArtwork(L.GutBlowingUp3),
            ShowArtwork(L.GutBlownUp),
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Active)),
            Cue(new TorizoCue.Signal(K.UnlockAnimation)),
            Cue(new TorizoCue.Signal(K.ResumeInterrupted)),
        ],
        [A.BombTorizoFaceBlowingUp] =
        [
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Idle)),
            Cue(new TorizoCue.Signal(K.LockAnimation)),
            Cue(new TorizoCue.LowHealthExplosions(BombTorizoExplosionSite.Face)),
            Wait(40),
            Cue(new TorizoCue.Signal(K.MarkFaceBlownUp)),
            ShowArtwork(L.FaceBlowingUp1),
            ShowArtwork(L.FaceBlowingUp2),
            ShowArtwork(L.FaceBlowingUp3),
            ShowArtwork(L.FaceBlowingUp4),
            Wait(1),
            ShowArtwork(L.FaceBlowingUp5),
            ShowArtwork(L.FaceBlowingUp6),
            ShowArtwork(L.FaceBlowingUp7),
            ShowArtwork(L.FaceBlowingUp8),
            ShowArtwork(L.FaceBlownUp),
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Active)),
            Cue(new TorizoCue.Signal(K.UnlockAnimation)),
            Cue(new TorizoCue.Signal(K.Return)),
        ],
        [A.Dying] =
        [
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Idle)),
            Cue(new TorizoCue.Signal(K.LockAnimation)),
            Cue(new TorizoCue.Signal(K.SpawnDeathExplosion)),
            Wait(1),
            Wait(6),
            Cue(new TorizoCue.Signal(K.FadeToBlack)),
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
        [A.BombTorizoDormant] =
        [
            Cue(new TorizoCue.Signal(K.SteppedLeftWithRightFoot)),
            Cue(new TorizoCue.Signal(K.LockAnimation)),
            Show(EnemyFrameId.BombTorizoDormant, 1),
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.WaitForHandTrigger)),
        ],
        [A.BombTorizoAwakening] =
        [
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Falling)),
            Show(EnemyFrameId.TorizoFacingLeftSeated, 48),
            ShowArtwork(L.EyesLightingUp1),
            Show(EnemyFrameId.TorizoFacingLeftSeated, 32),
            ShowArtwork(L.EyesLightingUp2),
            Show(EnemyFrameId.TorizoFacingLeftSeated, 16),
            ShowArtwork(L.EyesLightingUp3),
            Show(EnemyFrameId.TorizoFacingLeftSeated, 8),
            ShowArtwork(L.EyesLightingUp4),
            Show(EnemyFrameId.TorizoFacingLeftSeated, 32),
            Show(EnemyFrameId.TorizoFacingLeftSeated, 4),
            ShowArtwork(L.EyesLightingAgain1),
            Show(EnemyFrameId.TorizoFacingLeftSeated, 4),
            ShowArtwork(L.EyesLightingAgain2),
            Show(EnemyFrameId.TorizoFacingLeftSeated, 4),
            ShowArtwork(L.EyesLightingAgain3),
            Show(EnemyFrameId.TorizoFacingLeftSeated, 4),
            ShowArtwork(L.EyesLightingAgain4),
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
            Show(EnemyFrameId.TorizoFacingLeftStandingUpSixth, 4),
            Cue(new TorizoCue.Signal(K.AdvanceColourFade)),
            Cue(new TorizoCue.Signal(K.NoOperation)),
            Cue(new TorizoCue.Signal(K.UnlockAnimation)),
            Cue(new TorizoCue.Signal(K.StartBombTorizoFight)),
            Show(EnemyFrameId.TorizoFacingLeftStandingUpSixth, 16),
            .. S.FacingLeftLeftLegStep,
            .. S.FacingLeftRightLegStep,
        ],
        [A.Stunned] =
        [
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.GoldenGravity)),
            Cue(new TorizoCue.Signal(K.LockAnimation)),
            Wait(24),
            Wait(3),
            ShowArtwork(L.EyesLightingAfterStun1),
            Wait(3),
            ShowArtwork(L.EyesLightingAfterStun2),
            Wait(3),
            ShowArtwork(L.EyesLightingAfterStun3),
            Wait(3),
            ShowArtwork(L.EyesLightingAfterStun4),
            Wait(16),
            Cue(new TorizoCue.Signal(K.UnlockAnimation)),
            Cue(new TorizoCue.Signal(K.UnmarkStunned)),
            Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.GoldenAirTransition)),
            Cue(new TorizoCue.Signal(K.Return)),
        ],
    };
}
