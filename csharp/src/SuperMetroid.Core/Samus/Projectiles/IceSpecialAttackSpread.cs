namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// An Ice special attack particle that has turned outward (#1923): it keeps circling Samus, its circle growing, until it
/// leaves the screen.
/// </summary>
internal sealed class IceSpecialAttackSpread : SpecialAttackParticleBehavior
{
    /// <inheritdoc/>
    private protected override SoundEffectId? Step(SamusShotUpdate update)
    {
        update.Shots.Combos.StepIceSpread(update.Samus, update.Shot, update.SharedProjectiles, update.Camera);
        return null;
    }
}
