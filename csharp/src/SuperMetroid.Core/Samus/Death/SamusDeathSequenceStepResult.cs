namespace SuperMetroid.Core.Samus.Death;

/// <summary>One update of Samus's death: what it draws of her and whether it completed the sequence.</summary>
/// <param name="PhaseAfterStep">The death's phase once the update has run.</param>
/// <param name="Draws">What the update draws of Samus.</param>
/// <param name="Completed">Whether the update completed the sequence.</param>
public readonly record struct SamusDeathSequenceStepResult(
    SamusDeathSequencePhase PhaseAfterStep,
    SamusDeathDraw Draws,
    bool Completed);
