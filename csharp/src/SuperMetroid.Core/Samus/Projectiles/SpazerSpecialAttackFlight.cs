namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A Spazer special attack particle's flight (#1923): circling Samus, flying up towards a point and away from it, then
/// falling until it leaves the screen (<see cref="SpazerComboPhase"/>).
/// </summary>
internal sealed class SpazerSpecialAttackFlight : SpecialAttackParticleBehavior<SpazerParticleState>
{
    /// <inheritdoc/>
    private protected override SoundEffectId? Step(SamusShotUpdate update, SpazerParticleState state) =>
        update.Shots.Combos.StepSpazerCombo(update.Samus, update.Held, state, update.SamusBombs, update.Camera.Y);
}
