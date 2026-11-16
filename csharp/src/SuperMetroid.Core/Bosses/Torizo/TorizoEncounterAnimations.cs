using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Bosses.Torizo.TorizoCueKind;
using A = SuperMetroid.Core.Bosses.Torizo.TorizoAnimation;
using L = SuperMetroid.Core.Bosses.Torizo.TorizoArtworkLook;
using static SuperMetroid.Core.Bosses.Torizo.TorizoAnimationSteps;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The encounter segments both Torizos share: the gut and face blow-ups, the death, the stun, the Bomb Torizo's wake and the
/// seated eyes and standing colour fade both wakes play.
/// </summary>
internal static class TorizoEncounterAnimations
{
    /// <summary>Each segment's steps, in the order it plays them.</summary>
    internal static Dictionary<TorizoAnimation, AnimationBeat<EnemyFrameId, TorizoCue>[]> Segments { get; } = new()
    {
        [A.BombTorizoGutBlowingUp] =
        [
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Idle)),
            Cue(new TorizoCue.Signal(K.LockAnimation)),
            Cue(new TorizoCue.LowHealthExplosions(BombTorizoExplosionSite.Gut)),
            Wait(40),
            Cue(new TorizoCue.Signal(K.BlowUpGut)),
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
            ShowArtwork(L.FaceHalfBlownAway),
            Wait(1),
            ShowArtwork(L.FaceBlownUp),
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Active)),
            Cue(new TorizoCue.Signal(K.UnlockAnimation)),
            Cue(new TorizoCue.Turn(WhenFacingRight: A.FacelessTurningLeft, WhenFacingLeft: A.FacelessTurningRight)),
        ],
        [A.Dying] =
        [
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Idle)),
            Cue(new TorizoCue.Signal(K.LockAnimation)),
        ],
        [A.DyingExplosions] =
        [
            Cue(new TorizoCue.Signal(K.SpawnDeathExplosion)),
            Wait(7),
        ],
        [A.DyingFadeToBlack] =
        [
            Cue(new TorizoCue.Signal(K.FadeToBlack)),
        ],
        [A.DyingFlickering] =
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
        ],
        [A.DyingFinish] =
        [
            Wait(64),
            Cue(new TorizoCue.Signal(K.FinishDeath)),
        ],
        [A.BombTorizoDormant] =
        [
            Cue(new TorizoCue.Stepped(HorizontalDirection.Left, Foot: HorizontalDirection.Right)),
            Cue(new TorizoCue.Signal(K.LockAnimation)),
            Show(EnemyFrameId.BombTorizoDormant, 1),
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.WaitForHandTrigger)),
        ],
        [A.BombTorizoAwakening] =
        [
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Falling)),
            Show(EnemyFrameId.TorizoFacingLeftSeated, 48),
            ShowArtwork(L.EyesDark),
            Show(EnemyFrameId.TorizoFacingLeftSeated, 32),
            ShowArtwork(L.EyesGlowingFaintly),
            Show(EnemyFrameId.TorizoFacingLeftSeated, 16),
            ShowArtwork(L.EyesGlowingBrightly),
            Show(EnemyFrameId.TorizoFacingLeftSeated, 8),
            ShowArtwork(L.EyesGlowingFully),
            Show(EnemyFrameId.TorizoFacingLeftSeated, 32),
        ],
        [A.SeatedEyesLightingAgain] =
        [
            Show(EnemyFrameId.TorizoFacingLeftSeated, 4),
            ShowArtwork(L.EyesDarkAgain),
            Show(EnemyFrameId.TorizoFacingLeftSeated, 4),
            ShowArtwork(L.EyesGlowingFaintlyAgain),
            Show(EnemyFrameId.TorizoFacingLeftSeated, 4),
            ShowArtwork(L.EyesGlowingBrightlyAgain),
            Show(EnemyFrameId.TorizoFacingLeftSeated, 4),
            ShowArtwork(L.EyesGlowingFullyAgain),
        ],
        [A.BombTorizoStandingUp] =
        [
            Show(EnemyFrameId.TorizoFacingLeftSeated, 48),
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, Displacement: new(-9, 0))),
            Show(EnemyFrameId.TorizoFacingLeftRisingFromSeat, 16),
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, Displacement: new(-6, -6))),
            Show(EnemyFrameId.TorizoFacingLeftCrouched, 8),
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, Displacement: new(-7, -6))),
            Show(EnemyFrameId.TorizoFacingLeftHalfRisen, 8),
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, Displacement: new(5, -7))),
            Show(EnemyFrameId.TorizoFacingLeftNearlyUpright, 8),
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, Displacement: new(-16, 0))),
            Show(EnemyFrameId.TorizoFacingLeftUprightStepping, 8),
            Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, Displacement: new(-7, 0))),
            Cue(new TorizoCue.Signal(K.FadeToNormalColors)),
        ],
        [A.StandingColourFade] =
        [
            Show(EnemyFrameId.TorizoFacingLeftUprightWideStance, 4),
            Cue(new TorizoCue.Signal(K.AdvanceColourFade)),
        ],
        [A.BombTorizoFightStart] =
        [
            Cue(new TorizoCue.Signal(K.UnlockAnimation)),
            Cue(new TorizoCue.Signal(K.StartBombTorizoFight)),
            Show(EnemyFrameId.TorizoFacingLeftUprightWideStance, 16),
        ],
        [A.Stunned] =
        [
            Cue(new TorizoCue.Signal(K.StartStanding)),
            Cue(new TorizoCue.Signal(K.LockAnimation)),
            Wait(24),
        ],
        [A.StunnedEyesLightingAgain] =
        [
            Wait(3),
            ShowArtwork(L.StunnedEyesDark),
            Wait(3),
            ShowArtwork(L.StunnedEyesGlowingFaintly),
            Wait(3),
            ShowArtwork(L.StunnedEyesGlowingBrightly),
            Wait(3),
            ShowArtwork(L.StunnedEyesGlowingFully),
        ],
        [A.StunnedRecovery] =
        [
            Wait(16),
            Cue(new TorizoCue.Signal(K.UnlockAnimation)),
            Cue(new TorizoCue.Signal(K.UnmarkStunned)),
            Cue(new TorizoCue.Signal(K.StartWalking)),
            Cue(new TorizoCue.Signal(K.ResumeWalk)),
        ],
    };
}
