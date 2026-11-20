namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>A Plasma special attack particle's place in its ring round Samus (#1923), which grows, shrinks and grows again.</summary>
/// <param name="state">The state the shot's kind keeps.</param>
internal sealed class PlasmaSpecialAttackRing(PlasmaParticleState state) : SpecialAttackParticleBehavior
{
    /// <inheritdoc/>
    internal override SamusShotKindState KindState => state;

    /// <inheritdoc/>
    private protected override SoundEffectId? Step(SamusShotUpdate update)
    {
        update.Shots.Combos.StepPlasmaCombo(update.Samus, update.Shot, state, update.SamusBombs, update.Camera);
        return null;
    }
}
