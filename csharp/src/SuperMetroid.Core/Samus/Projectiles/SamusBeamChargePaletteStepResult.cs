namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Semantic branch and raw table/timer evidence from one `$91:D743` call.</summary>
/// <param name="Action">The palette action selected by the charge-state branch.</param>
public readonly record struct SamusBeamChargePaletteStepResult(
    SamusBeamChargePaletteAction Action);
