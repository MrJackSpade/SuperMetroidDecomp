using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>What a fragment of the collapsing door in Mother Brain's room keeps: how long it falls before it turns to dust.</summary>
internal sealed class MotherBrainDoorFragmentState : EnemyProjectileState
{
    /// <summary>The fragment's remaining life; it turns to dust on the update it runs out.</summary>
    internal MotherBrainCountdown Lifetime { get; } = new() { Count = MotherBrainDeathDefinitions.DoorFragmentLifetime };
}
