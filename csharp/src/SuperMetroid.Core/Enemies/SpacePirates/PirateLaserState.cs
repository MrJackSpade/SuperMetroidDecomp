using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>What a walking pirate's laser keeps: whether its pirate fires the slow laser.</summary>
/// <param name="slow">Whether the laser flies at the slow speed.</param>
internal sealed class PirateLaserState(bool slow) : EnemyProjectileState
{
    /// <summary>Whether the laser flies at the slow speed rather than the fast one.</summary>
    internal bool Slow { get; } = slow;
}
