namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// An Ice special attack particle's orbit round Samus at a fixed distance (#1923), until its lifetime ends and it spreads
/// outward (<see cref="IceSpecialAttackSpread"/>).
/// </summary>
internal sealed partial class IceSpecialAttackOrbit : SpecialAttackParticleBehavior
{
    /// <inheritdoc/>
    private protected override SoundEffectId? Step(SamusShotUpdate update) =>
        update.Shots.Combos.StepIceOrbit(update.Samus, update.Held, update.SamusBombs);
}
