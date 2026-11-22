namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>A Wave special attack particle's homing on Samus (#1923), until its lifetime ends or it strikes something.</summary>
internal sealed partial class WaveSpecialAttackHoming : SpecialAttackParticleBehavior<WaveParticleState>
{
    /// <inheritdoc/>
    private protected override SoundEffectId? Step(SamusShotUpdate update, WaveParticleState state) =>
        update.Shots.Combos.StepWaveCombo(update.Samus, update.Held, state, update.SamusBombs);
}
