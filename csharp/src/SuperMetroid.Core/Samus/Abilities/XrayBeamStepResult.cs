namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Immutable lifecycle witness for one bank-$88 beam-state call; angle and width remain on the owning X-ray state for the window renderer.</summary>
/// <param name="PhaseAtStart">Beam phase before this call's setup-stage or dispatcher work; it is not necessarily the resulting phase.</param>
/// <param name="Completed">True when this call removed the active X-ray object; merely reaching the finish phase does not imply completion.</param>
public readonly record struct XrayBeamStepResult(
    XrayBeamPhase PhaseAtStart,
    bool Completed);
