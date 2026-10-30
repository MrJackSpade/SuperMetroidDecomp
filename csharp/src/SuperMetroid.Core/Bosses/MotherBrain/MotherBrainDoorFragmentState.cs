using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>What a fragment of the collapsing door in Mother Brain's room keeps: how long it falls before it turns to dust.</summary>
internal sealed class MotherBrainDoorFragmentState : EnemyProjectileState
{
    /// <summary>The fragment's remaining life; it turns to dust on the update this passes zero.</summary>
    internal WrappingCountdown Lifetime { get; set; } = WrappingCountdown.Start(MotherBrainDeathDefinitions.DoorFragmentLifetime);
}
