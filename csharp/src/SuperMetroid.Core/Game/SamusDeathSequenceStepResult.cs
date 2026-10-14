namespace SuperMetroid.Core.Game;

/// <summary>One native death game-state call, including draw and NMI-transfer requests.</summary>
public readonly record struct SamusDeathSequenceStepResult(
    SamusDeathSequencePhase PhaseAfterStep,
    bool DrawPose,
    bool DrawExplosion,
    bool Completed);
