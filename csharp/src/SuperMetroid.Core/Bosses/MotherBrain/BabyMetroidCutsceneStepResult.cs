using SuperMetroid.Core.Escape;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One-call debugger witness for the Baby entrance and latch chain.</summary>
[IndependentFlag("independent requests one cutscene step makes")]
public readonly record struct BabyMetroidCutsceneStepResult(
    BabyMetroidCutscenePhase PhaseBefore,
    BabyMetroidCutscenePhase PhaseAfter,
    bool BodyStumbleRequested,
    bool LatchSoundQueued,
    IReadOnlyList<BabyMetroidReleaseDustRequest> ReleaseDustClouds,
    bool AmbientCrySoundQueued,
    DustExplosionRequest? DeathExplosion,
    BabyMetroidPaletteTransferRequest? BabyPaletteTransfer,
    MotherBrainSpriteTileTransferRequest? AttackTileTransfer,
    MotherBrainBackgroundPaletteTransferRequest? BackgroundPaletteTransfer)
{
    /// <summary>The one-call fatal-impact edge at $A9:CBF2-CC3D, before the ongoing shake phase.</summary>
    public bool FatalBlowStarted => PhaseBefore == BabyMetroidCutscenePhase.FinalCharge &&
        PhaseAfter == BabyMetroidCutscenePhase.TakeFinalBlow;

    /// <summary>The $A9:CC60 timer expiry queues the theme before entering Hyper Beam preparation.</summary>
    public bool SamusThemeStarted => PhaseBefore == BabyMetroidCutscenePhase.PlaySamusTheme &&
        PhaseAfter == BabyMetroidCutscenePhase.PrepareSamusForHyperBeam;
}
