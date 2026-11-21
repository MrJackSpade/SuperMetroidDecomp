namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A missile's flight (#1923): it ignites on its first update, then accelerates, leaves a trail every <see cref="ProjectileTimingDefinitions.TrailInterval"/> and
/// explodes at the first block its centre meets.
/// </summary>
internal sealed class MissileFlight : SamusShotBehavior
{
    /// <inheritdoc/>
    internal override void Run(SamusShotUpdate update) =>
        update.Shots.Motion.FlyMissile(update.Terrain, update.Samus, update.Held, update.Camera, update.SamusBombs);
}
