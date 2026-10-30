using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>What a Powamp spike keeps: which of the burst's eight directions it flies in.</summary>
/// <param name="direction">The spike's direction, 0 through 7.</param>
internal sealed class PowampSpikeState(int direction) : EnemyProjectileState
{
    /// <summary>The spike's direction among the burst's eight, 0 through 7; it selects the spike's acceleration.</summary>
    internal int Direction { get; } = Ensure.BetweenInclusive(0, PowampMotionDefinitions.SpikeDirectionCount - 1, direction);
}
