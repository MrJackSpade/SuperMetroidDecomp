using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Bosses.Torizo.TorizoCueKind;
using A = SuperMetroid.Core.Bosses.Torizo.TorizoAnimation;
using S = SuperMetroid.Core.Bosses.Torizo.TorizoBodySegment;
using L = SuperMetroid.Core.Bosses.Torizo.TorizoArtworkLook;
using static SuperMetroid.Core.Bosses.Torizo.TorizoAnimationSteps;
using static SuperMetroid.Core.Bosses.Torizo.TorizoSide;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The encounter segments both Torizos share: the gut and face blow-ups, the death, the stun, the Bomb Torizo's wake and the
/// seated eyes and standing colour fade both wakes play. Both statues sit facing left.
/// </summary>
internal static class TorizoEncounterAnimations
{
    /// <summary>Each segment's steps, in the order it plays them.</summary>
    internal static Dictionary<TorizoBodySegment, AnimationBeat<EnemyFrameId, TorizoCue>[]> Segments { get; } = new()
    {
        [S.BombTorizoGutBlowingUp] =
        [
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Idle)),
            Cue(new TorizoCue.Signal(K.LockAnimation)),
            Cue(new TorizoCue.LowHealthExplosions(BombTorizoExplosionSite.Gut)),
            Wait(TorizoDeathDefinitions.LowHealthExplosionFlash),
            Cue(new TorizoCue.Signal(K.BlowUpGut)),
            ShowArtwork(L.GutBlownUp),
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Active)),
            Cue(new TorizoCue.Signal(K.UnlockAnimation)),
            Cue(new TorizoCue.Signal(K.ResumeInterrupted)),
        ],
        [S.BombTorizoFaceBlowingUp] =
        [
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Idle)),
            Cue(new TorizoCue.Signal(K.LockAnimation)),
            Cue(new TorizoCue.LowHealthExplosions(BombTorizoExplosionSite.Face)),
            Wait(TorizoDeathDefinitions.LowHealthExplosionFlash),
            Cue(new TorizoCue.Signal(K.MarkFaceBlownUp)),
            ShowArtwork(L.FaceHalfBlownAway),
            Wait(1),
            ShowArtwork(L.FaceBlownUp),
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Active)),
            Cue(new TorizoCue.Signal(K.UnlockAnimation)),
            Cue(new TorizoCue.Turn(WhenFacingRight: A.FacelessTurningLeft, WhenFacingLeft: A.FacelessTurningRight)),
        ],
        [S.Dying] =
        [
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Idle)),
            Cue(new TorizoCue.Signal(K.LockAnimation)),
        ],
        [S.DyingExplosions] =
        [
            Cue(new TorizoCue.Signal(K.SpawnDeathExplosion)),
            Wait(7),
        ],
        [S.DyingFadeToBlack] =
        [
            Cue(new TorizoCue.Signal(K.FadeToBlack)),
        ],
        [S.DyingFlickering] =
        [
            .. ExplodingAndFlickering,
            .. ExplodingAndFlickering,
            .. Flickering,
            .. Flickering,
            Cue(new TorizoCue.Signal(K.AdvanceColourFade)),
        ],
        [S.DyingFinish] =
        [
            Wait(64),
            Cue(new TorizoCue.Signal(K.FinishDeath)),
        ],
        [S.BombTorizoDormant] =
        [
            Cue(new TorizoCue.Stepped(HorizontalDirection.Left, Foot: HorizontalDirection.Right)),
            Cue(new TorizoCue.Signal(K.LockAnimation)),
            Show(EnemyFrameId.BombTorizoDormant, 1),
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.WaitForHandTrigger)),
        ],
        [S.BombTorizoAwakening] =
        [
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Falling)),
            .. SeatedEyesLightingUp,
        ],
        [S.SeatedEyesLightingAgain] =
        [
            Show(Left.Seated, 4),
            ShowArtwork(L.EyesDarkAgain),
            Show(Left.Seated, 4),
            ShowArtwork(L.EyesGlowingFaintlyAgain),
            Show(Left.Seated, 4),
            ShowArtwork(L.EyesGlowingBrightlyAgain),
            Show(Left.Seated, 4),
            ShowArtwork(L.EyesGlowingFullyAgain),
        ],
        [S.BombTorizoStandingUp] =
        [
            .. StandingUp(Left, 48, 16, 8, 8, 8, 8),
            Cue(new TorizoCue.Signal(K.FadeToNormalColors)),
        ],
        [S.StandingColourFade] =
        [
            Show(Left.UprightWideStance, 4),
            Cue(new TorizoCue.Signal(K.AdvanceColourFade)),
        ],
        [S.BombTorizoFightStart] =
        [
            Cue(new TorizoCue.Signal(K.UnlockAnimation)),
            Cue(new TorizoCue.Signal(K.StartBombTorizoFight)),
            Show(Left.UprightWideStance, 16),
        ],
        [S.Stunned] =
        [
            Cue(new TorizoCue.Signal(K.StartStanding)),
            Cue(new TorizoCue.Signal(K.LockAnimation)),
            Wait(24),
        ],
        [S.StunnedEyesLightingAgain] =
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
        [S.StunnedRecovery] =
        [
            Wait(16),
            Cue(new TorizoCue.Signal(K.UnlockAnimation)),
            Cue(new TorizoCue.Signal(K.UnmarkStunned)),
            Cue(new TorizoCue.Signal(K.StartWalking)),
            Cue(new TorizoCue.Signal(K.ResumeWalk)),
        ],
    };

    /// <summary>A dying Torizo explodes, then flickers once.</summary>
    private static AnimationBeat<EnemyFrameId, TorizoCue>[] ExplodingAndFlickering =>
    [
        Cue(new TorizoCue.Signal(K.SpawnDeathExplosion)),
        Wait(TorizoDeathDefinitions.DeathExplosionFlash),
        .. Flickering,
    ];

    /// <summary>A dying Torizo flickers once: shown for two updates, then hidden for two.</summary>
    private static AnimationBeat<EnemyFrameId, TorizoCue>[] Flickering =>
    [
        Cue(new TorizoCue.Signal(K.MakeVisible)),
        Wait(2),
        Cue(new TorizoCue.Signal(K.MakeInvisible)),
        Wait(2),
    ];
}
