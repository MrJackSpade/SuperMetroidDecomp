using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>What a piece of Bomb Torizo's breaking statue keeps: the acceleration its spawn gives it.</summary>
/// <param name="acceleration">The piece's acceleration, in 1/256 pixel per update per update.</param>
/// <remarks>
/// A falling piece does not add its own acceleration; it adds the first projectile slot's piece's
/// (<see cref="FirstSlotFallAcceleration"/>), so this value matters only to a piece in that slot.
/// </remarks>
internal sealed class BombTorizoStatueFragmentState(int acceleration) : EnemyProjectileState
{
    /// <summary>The piece's acceleration, in 1/256 pixel per update per update.</summary>
    internal int Acceleration { get; } = acceleration;
}
