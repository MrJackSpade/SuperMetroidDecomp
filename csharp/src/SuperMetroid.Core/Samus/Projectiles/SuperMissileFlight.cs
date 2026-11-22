namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A Super Missile's flight (#1923): it ignites on its first update with an invisible link that closes the gap a fast
/// missile leaves between its positions, then accelerates and leaves a trail more often than a missile does.
/// </summary>
internal sealed partial class SuperMissileFlight : SamusShotBehavior<SuperMissileShotState>
{
    /// <inheritdoc/>
    internal override void Run(SamusShotUpdate update, SuperMissileShotState state) =>
        update.Shots.Motion.FlySuperMissile(update.Terrain, update.Samus, update.Held, state, update.Camera,
            update.SamusBombs);
}
