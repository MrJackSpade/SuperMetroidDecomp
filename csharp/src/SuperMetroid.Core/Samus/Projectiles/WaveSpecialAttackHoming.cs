namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>A Wave special attack particle's homing on Samus (#1923), until its lifetime ends or it strikes something.</summary>
internal sealed partial class WaveSpecialAttackHoming : SpecialAttackParticleBehavior
{
    /// <inheritdoc/>
    private protected override SoundEffectId? Step(SamusShotUpdate update) =>
        update.Shots.Combos.StepWaveCombo(update.Samus, update.Held, update.Shot.KindStateAs<WaveParticleState>(), update.SamusBombs);
}
