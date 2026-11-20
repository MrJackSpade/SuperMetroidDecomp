using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>What one of Phantoon's rain flames keeps: how long it hangs before it falls.</summary>
/// <param name="hangUpdates">The updates from the flame's first to its fall, the flame hanging on every earlier one.</param>
internal sealed class PhantoonRainFlameState(int hangUpdates) : EnemyProjectileState
{
    /// <summary>
    /// The flame's hang before it falls (#2469): it falls, with its falling sound, on the update the hang runs out on.
    /// </summary>
    internal BossCountdown HangWait { get; } = BossCountdown.Lasting(hangUpdates);

    /// <summary>Whether the flame has stopped hanging and falls.</summary>
    internal bool Falls { get; set; }
}
