namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Immutable lifecycle witness for one beam-state update; angle and width remain on the owning X-ray state, which the beam's cone is drawn from.</summary>
/// <param name="PhaseAtStart">Beam phase before this update's setup-stage or phase work; it is not necessarily the resulting phase.</param>
/// <param name="Completed">True when this update removed the active X-ray object; merely reaching the finish phase does not imply completion.</param>
public readonly partial record struct XrayBeamStepResult(
    XrayBeamPhase PhaseAtStart,
    bool Completed);
