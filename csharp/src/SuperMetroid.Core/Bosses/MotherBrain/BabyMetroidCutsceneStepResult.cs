namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>What one step of the Baby's entrance did: its phase before and after and the requests it made.</summary>
/// <param name="PhaseBefore">The phase the step ran.</param>
/// <param name="PhaseAfter">The phase the step left.</param>
/// <param name="BabyDeleted">Whether this is the step on which the Baby deletes itself, the last of the cutscene.</param>
/// <param name="LatchSoundQueued">Whether the Baby latched on and the rainbow drain's sound starts.</param>
/// <param name="ReleaseDustClouds">The dust clouds the Baby's release bursts, in spawn order.</param>
/// <param name="AmbientCrySoundQueued">Whether the Baby cries its remorse cry.</param>
/// <param name="DeathExplosion">The explosion around the dying Baby, if one bursts.</param>
/// <param name="BabyFadeImage">The image of the Baby's fade to black shown, if one is.</param>
/// <param name="RecoveryLightsImage">The image of the room lights' recovery shown, if one is (#2316).</param>
/// <param name="StartedAnimation">The animation the Baby starts on this step, if it starts one (#2319).</param>
/// <param name="Visibility">Whether the step shows or hides the Baby on its place, or leaves it as it is (#2310).</param>
[IndependentFlag("independent requests one cutscene step makes")]
public readonly partial record struct BabyMetroidCutsceneStepResult(
    BabyMetroidCutscenePhase.Flying PhaseBefore,
    BabyMetroidCutscenePhase.Flying PhaseAfter,
    bool BabyDeleted,
    bool LatchSoundQueued,
    IReadOnlyList<BabyMetroidReleaseDustRequest> ReleaseDustClouds,
    bool AmbientCrySoundQueued,
    DustExplosionRequest? DeathExplosion,
    MotherBrainFadeImage? BabyFadeImage,
    MotherBrainFadeImage? RecoveryLightsImage,
    MotherBrainBabySegment? StartedAnimation,
    BabyMetroidVisibilityRequest Visibility)
{
    /// <summary>The one-update fatal-impact edge, before the ongoing shake phase.</summary>
    public bool FatalBlowStarted => PhaseBefore is BabyMetroidCutscenePhase.Flying.FinalCharge &&
        PhaseAfter is BabyMetroidCutscenePhase.Flying.TakeFinalBlow;

    /// <summary>The theme timer's expiry queues the theme before entering Hyper Beam preparation.</summary>
    public bool SamusThemeStarted => PhaseBefore is BabyMetroidCutscenePhase.Flying.PlaySamusTheme &&
        PhaseAfter is BabyMetroidCutscenePhase.Flying.PrepareSamusForHyperBeam;
}
