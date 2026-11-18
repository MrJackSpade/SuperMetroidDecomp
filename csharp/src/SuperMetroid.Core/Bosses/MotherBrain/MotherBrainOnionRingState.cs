namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>What one of Mother Brain's onion rings keeps: how long it stays at her mouth before it flies.</summary>
/// <param name="motherBrain">The encounter whose mouth the ring leaves.</param>
/// <param name="mouthWaitUpdates">The updates from the ring's first to its flight, the ring at the mouth on every earlier one.</param>
internal sealed class MotherBrainOnionRingState(MotherBrainEnemyState motherBrain, int mouthWaitUpdates)
    : MotherBrainProjectileState(motherBrain)
{
    /// <summary>The ring's wait at the mouth (#2469): it stays pinned there while this runs and flies from the update it runs out on.</summary>
    internal BossCountdown MouthWait { get; } = BossCountdown.Lasting(mouthWaitUpdates);
}
