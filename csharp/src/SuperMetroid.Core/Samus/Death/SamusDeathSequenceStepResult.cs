namespace SuperMetroid.Core.Samus.Death;

/// <summary>One native death game-state call, including draw and NMI-transfer requests.</summary>
[IndependentFlag("independent facts of one death step: what to draw and whether it completed")]
public readonly record struct SamusDeathSequenceStepResult(
    SamusDeathSequencePhase PhaseAfterStep,
    bool DrawPose,
    bool DrawExplosion,
    bool Completed);
