namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// An Ice special attack particle's orbit round Samus at a fixed distance (#1923), until its lifetime ends and it spreads
/// outward (<see cref="IceSpecialAttackSpread"/>).
/// </summary>
/// <param name="state">The state the shot's kind keeps.</param>
internal sealed class IceSpecialAttackOrbit(IceParticleState state) : SpecialAttackParticleBehavior
{
    /// <inheritdoc/>
    internal override SamusShotKindState KindState => state;

    /// <inheritdoc/>
    private protected override SoundEffectId? Step(SamusShotUpdate update) =>
        update.Shots.Combos.StepIceOrbit(update.Samus, update.Held, state, update.SamusBombs);
}
