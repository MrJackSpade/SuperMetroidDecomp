using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>What a pirate's or Mother Brain's laser keeps: the way it flies and the speed it is fired at (#2278).</summary>
/// <param name="speed">The laser's speed.</param>
/// <param name="direction">The way the laser flies once fired.</param>
internal sealed partial class PirateLaserState(PirateLaserSpeed speed, HorizontalDirection direction) : EnemyProjectileState
{
    /// <summary>The speed the laser flies at.</summary>
    internal PirateLaserSpeed Speed { get; } = speed;

    /// <summary>The way the laser flies once its animation fires it, which its firer chose.</summary>
    internal HorizontalDirection Direction { get; } = direction;
}
