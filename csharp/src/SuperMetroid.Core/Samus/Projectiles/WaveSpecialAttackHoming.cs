namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>A Wave special attack particle's homing on Samus (#1923), until its lifetime ends or it strikes something.</summary>
/// <param name="state">The state the shot's kind keeps.</param>
internal sealed class WaveSpecialAttackHoming(WaveParticleState state) : SpecialAttackParticleBehavior
{
    /// <inheritdoc/>
    internal override SamusShotKindState KindState => state;

    /// <inheritdoc/>
    private protected override SoundEffectId? Step(SamusShotUpdate update) =>
        update.Shots.Combos.StepWaveCombo(update.Samus, update.Shot, state, update.SamusBombs);
}
