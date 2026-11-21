namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// An Ice special attack particle's orbit round Samus at a fixed distance (#1923), until its lifetime ends and it spreads
/// outward (<see cref="IceSpecialAttackSpread"/>).
/// </summary>
internal sealed class IceSpecialAttackOrbit : SpecialAttackParticleBehavior<IceParticleState>
{
    /// <inheritdoc/>
    private protected override SoundEffectId? Step(SamusShotUpdate update, IceParticleState state) =>
        update.Shots.Combos.StepIceOrbit(update.Samus, update.Held, state, update.SamusBombs);
}
