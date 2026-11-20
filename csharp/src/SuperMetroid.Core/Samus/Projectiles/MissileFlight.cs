namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A missile's flight (#1923): it ignites on its first update, then accelerates, leaves a trail every four updates and
/// explodes at the first block its centre meets.
/// </summary>
/// <param name="state">The state the shot's kind keeps.</param>
internal sealed class MissileFlight(MissileShotState state) : SamusShotBehavior
{
    /// <inheritdoc/>
    internal override SamusShotKindState KindState => state;

    /// <inheritdoc/>
    internal override void Run(SamusShotUpdate update) =>
        update.Shots.Motion.FlyMissile(update.Terrain.Level, update.Samus, update.Shot, state, update.Camera, update.SamusBombs,
            update.Terrain.RoomObjects);
}
