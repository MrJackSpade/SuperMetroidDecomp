namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// An Ice special attack particle that has turned outward (#1923): it keeps circling Samus, its circle growing, until it
/// leaves the screen.
/// </summary>
/// <param name="state">The state the shot's kind keeps.</param>
internal sealed class IceSpecialAttackSpread(IceParticleState state) : SpecialAttackParticleBehavior
{
    /// <inheritdoc/>
    internal override SamusShotKindState KindState => state;

    /// <inheritdoc/>
    private protected override SoundEffectId? Step(SamusShotUpdate update)
    {
        update.Shots.Combos.StepIceSpread(update.Samus, update.Held, state, update.SamusBombs, update.Camera);
        return null;
    }
}
