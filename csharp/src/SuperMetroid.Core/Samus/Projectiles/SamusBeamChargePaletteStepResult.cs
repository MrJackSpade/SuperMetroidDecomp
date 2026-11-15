namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Semantic branch and raw table/timer evidence from one charged-shot colour call.</summary>
/// <param name="Action">The colouring action selected by the charge-state branch.</param>
public readonly record struct SamusBeamChargePaletteStepResult(
    SamusBeamChargePaletteAction Action);
