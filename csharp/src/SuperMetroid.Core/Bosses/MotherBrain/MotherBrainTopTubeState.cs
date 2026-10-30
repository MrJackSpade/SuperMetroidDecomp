using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>What one of the falling ceiling tubes in Mother Brain's room keeps: whether it has yet to puff its dust.</summary>
internal sealed class MotherBrainTopTubeState : EnemyProjectileState
{
    /// <summary>Whether the tube has yet to puff dust where it broke loose; it puffs on its first fall update.</summary>
    internal bool DustPending { get; set; } = true;
}
