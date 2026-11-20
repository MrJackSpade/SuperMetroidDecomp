namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A Super Missile's flight (#1923): it ignites on its first update with an invisible link that closes the gap a fast
/// missile leaves between its positions, then accelerates and leaves a trail every two updates.
/// </summary>
/// <param name="state">The state the shot's kind keeps.</param>
internal sealed class SuperMissileFlight(SuperMissileShotState state) : SamusShotBehavior
{
    /// <inheritdoc/>
    internal override SamusShotKindState KindState => state;

    /// <inheritdoc/>
    internal override void Run(SamusShotUpdate update) =>
        update.Shots.Motion.FlySuperMissile(update.Terrain.Level, update.Samus, update.Shot, state, update.Camera,
            update.SamusBombs, update.Terrain.RoomObjects);
}
