namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A Spazer special attack particle's flight before it falls (#1923): circling Samus, then flying up towards a point and
/// away from it (<see cref="SpazerComboPhase"/>), until it falls (<see cref="SpazerSpecialAttackFall"/>).
/// </summary>
internal sealed class SpazerSpecialAttackFlight : SpecialAttackParticleBehavior
{
    /// <inheritdoc/>
    private protected override SoundEffectId? Step(SamusShotUpdate update) =>
        update.Shots.Combos.StepSpazerCombo(update.Samus, update.Shot, update.SharedProjectiles, update.Camera.Y);
}
