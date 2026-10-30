using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>What a Phantoon flame circling its body keeps: where on its circle it is.</summary>
/// <param name="angle">The flame's starting angle around the body.</param>
internal sealed class PhantoonOrbitingFlameState(ByteAngle angle) : EnemyProjectileState
{
    /// <summary>The flame's angle around Phantoon's body.</summary>
    internal ByteAngle Angle { get; set; } = angle;
}
