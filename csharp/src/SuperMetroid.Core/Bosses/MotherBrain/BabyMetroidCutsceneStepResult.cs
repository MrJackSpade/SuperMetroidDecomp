namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>What one step of the Baby's entrance did: its phase before and after and the requests it made.</summary>
/// <param name="PhaseBefore">The phase the step ran.</param>
/// <param name="PhaseAfter">The phase the step left.</param>
/// <param name="LatchSoundQueued">Whether the Baby latched on and the rainbow drain's sound starts.</param>
/// <param name="ReleaseDustClouds">The dust clouds the Baby's release bursts, in spawn order.</param>
/// <param name="AmbientCrySoundQueued">Whether the Baby cries its remorse cry.</param>
/// <param name="DeathExplosion">The explosion around the dying Baby, if one bursts.</param>
/// <param name="BabyFadeImage">The image of the Baby's fade to black shown, if one is.</param>
/// <param name="RecoveryLightsFrame">The room lights' recovery step shown, if one is.</param>
/// <param name="StartedAnimation">The animation the Baby starts on this step, if it starts one (#2319).</param>
[IndependentFlag("independent requests one cutscene step makes")]
public readonly record struct BabyMetroidCutsceneStepResult(
    BabyMetroidCutscenePhase PhaseBefore,
    BabyMetroidCutscenePhase PhaseAfter,
    bool LatchSoundQueued,
    IReadOnlyList<BabyMetroidReleaseDustRequest> ReleaseDustClouds,
    bool AmbientCrySoundQueued,
    DustExplosionRequest? DeathExplosion,
    MotherBrainFadeImage? BabyFadeImage,
    int? RecoveryLightsFrame,
    MotherBrainBabySegment? StartedAnimation)
{
    /// <summary>The one-update fatal-impact edge, before the ongoing shake phase.</summary>
    public bool FatalBlowStarted => PhaseBefore == BabyMetroidCutscenePhase.FinalCharge &&
        PhaseAfter == BabyMetroidCutscenePhase.TakeFinalBlow;

    /// <summary>The theme timer's expiry queues the theme before entering Hyper Beam preparation.</summary>
    public bool SamusThemeStarted => PhaseBefore == BabyMetroidCutscenePhase.PlaySamusTheme &&
        PhaseAfter == BabyMetroidCutscenePhase.PrepareSamusForHyperBeam;
}
