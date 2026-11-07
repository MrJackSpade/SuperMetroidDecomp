namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>What one step of the Baby's entrance did: its phase before and after and the requests it made.</summary>
[IndependentFlag("independent requests one cutscene step makes")]
public readonly record struct BabyMetroidCutsceneStepResult(
    BabyMetroidCutscenePhase PhaseBefore,
    BabyMetroidCutscenePhase PhaseAfter,
    bool BodyStumbleRequested,
    bool LatchSoundQueued,
    IReadOnlyList<BabyMetroidReleaseDustRequest> ReleaseDustClouds,
    bool AmbientCrySoundQueued,
    DustExplosionRequest? DeathExplosion,
    int? BabyFadeShade,
    int? RecoveryLightsFrame)
{
    /// <summary>The one-call fatal-impact edge, before the ongoing shake phase.</summary>
    public bool FatalBlowStarted => PhaseBefore == BabyMetroidCutscenePhase.FinalCharge &&
        PhaseAfter == BabyMetroidCutscenePhase.TakeFinalBlow;

    /// <summary>The theme timer's expiry queues the theme before entering Hyper Beam preparation.</summary>
    public bool SamusThemeStarted => PhaseBefore == BabyMetroidCutscenePhase.PlaySamusTheme &&
        PhaseAfter == BabyMetroidCutscenePhase.PrepareSamusForHyperBeam;
}
