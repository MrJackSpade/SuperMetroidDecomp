namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>What one update of the Crystal Flash movement did.</summary>
/// <param name="PhaseAtStart">Phase at call entry, before any transition made by that call.</param>
/// <param name="BubbleEffectRequested">Retained request flag, not a consumed one-shot event; pair with the raising entry phase to identify bubble activation.</param>
public readonly record struct CrystalFlashMovementResult(
    CrystalFlashPhase PhaseAtStart,
    bool BubbleEffectRequested);
