using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>What a walking pirate's laser keeps: the speed its pirate fires it at (#2278).</summary>
/// <param name="speed">The laser's speed.</param>
internal sealed class PirateLaserState(PirateLaserSpeed speed) : EnemyProjectileState
{
    /// <summary>The speed the laser flies at.</summary>
    internal PirateLaserSpeed Speed { get; } = speed;
}
