using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>What one of Crocomire's spit projectiles keeps: its place in the volley, which sets the slope it is fired along.</summary>
/// <param name="shot">The shot's place in the volley, 1 through <see cref="CrocomireProjectileDefinitions.VolleyShotCount"/>.</param>
internal sealed class CrocomireProjectileState(int shot) : EnemyProjectileState
{
    /// <summary>
    /// The shot's place in the volley, 1 through <see cref="CrocomireProjectileDefinitions.VolleyShotCount"/>; it selects
    /// the slope it is fired along.
    /// </summary>
    internal int Shot { get; } = Ensure.AtLeastZero(shot);
}
