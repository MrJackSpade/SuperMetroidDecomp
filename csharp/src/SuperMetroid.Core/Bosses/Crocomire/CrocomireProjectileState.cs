using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>What one of Crocomire's spit projectiles keeps: the gradient its volley fires it along (#2303).</summary>
/// <param name="gradient">The gradient the shot is fired along (<see cref="CrocomireVolley.TryNextGradient"/>).</param>
internal sealed partial class CrocomireProjectileState(int gradient) : EnemyProjectileState
{
    /// <summary>The gradient the shot is fired along, which its aim reads as its flight starts.</summary>
    internal int Gradient { get; } = gradient;
}
