namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A Spazer special attack particle's fall (#1923), until it leaves the screen; a spent falling particle ends without its
/// partner.
/// </summary>
internal sealed class SpazerSpecialAttackFall : SpecialAttackParticleBehavior
{
    /// <inheritdoc/>
    private protected override SoundEffectId? Step(SamusShotUpdate update) =>
        update.Shots.Combos.StepSpazerCombo(update.Samus, update.Shot, update.SharedProjectiles, update.Camera.Y);
}
