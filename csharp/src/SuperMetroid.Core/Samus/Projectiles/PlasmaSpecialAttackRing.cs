namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>A Plasma special attack particle's place in its ring round Samus (#1923), which grows, shrinks and grows again.</summary>
internal sealed partial class PlasmaSpecialAttackRing : SpecialAttackParticleBehavior<PlasmaParticleState>
{
    /// <inheritdoc/>
    private protected override SoundEffectId? Step(SamusShotUpdate update, PlasmaParticleState state)
    {
        update.Shots.Combos.StepPlasmaCombo(update.Samus, update.Held, state, update.SamusBombs, update.Camera);
        return null;
    }
}
