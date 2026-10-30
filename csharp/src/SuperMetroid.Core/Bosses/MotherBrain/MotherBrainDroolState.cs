using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>What a drop of Mother Brain's drool keeps while it hangs from her mouth: which of the six mouth points it hangs from.</summary>
/// <param name="mouthPoint">The mouth point, 0 through 5.</param>
internal sealed class MotherBrainDroolState(int mouthPoint) : EnemyProjectileState
{
    /// <summary>The mouth point the drool hangs from, 0 through 5.</summary>
    internal int MouthPoint { get; } = Ensure.BetweenInclusive(0, MotherBrainBehavior.MotherBrainDroolOffsets.Length - 1, mouthPoint);
}
