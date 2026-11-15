namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>What one update of the charged-shot glow did.</summary>
/// <param name="Action">The shade action selected by the charge-state branch.</param>
public readonly record struct SamusBeamChargeGlowStep(
    SamusBeamChargeGlowAction Action);
