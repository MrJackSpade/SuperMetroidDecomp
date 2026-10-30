using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>What one of Crocomire's spit projectiles keeps: its place in the volley, which sets the slope it is fired along.</summary>
/// <param name="volleyParameter">The volley's spawn parameter for this shot, 2 through 18 in steps of two.</param>
internal sealed class CrocomireProjectileState(int volleyParameter) : EnemyProjectileState
{
    /// <summary>The volley's spawn parameter for this shot, 2 through 18 in steps of two; it selects the slope it is fired along.</summary>
    internal int VolleyParameter { get; } = Ensure.AtLeastZero(volleyParameter);
}
