namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A Super Missile's flight (#1923): it ignites on its first update with an invisible link that closes the gap a fast
/// missile leaves between its positions, then accelerates and leaves a trail every two updates.
/// </summary>
internal sealed class SuperMissileFlight : SamusShotBehavior
{
    /// <inheritdoc/>
    internal override void Run(SamusShotUpdate update) =>
        update.Shots.Motion.FlySuperMissile(update.Terrain.Level, update.Samus, update.Shot, update.Camera,
            update.SharedProjectiles, update.Terrain.RoomObjects);
}
