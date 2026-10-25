namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>One-frame debugger witness from the translated Crystal Flash handler.</summary>
/// <param name="PhaseAtStart">Handler selected at call entry, before any transition made by that call.</param>
/// <param name="BubbleEffectRequested">Retained request flag, not a consumed one-shot event; pair with the raising entry phase to identify bubble activation.</param>
public readonly record struct CrystalFlashMovementResult(
    CrystalFlashPhase PhaseAtStart,
    bool BubbleEffectRequested);
