using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>What a Powamp spike keeps: which of the burst's eight directions it flies in.</summary>
/// <param name="direction">The spike's direction.</param>
internal sealed class PowampSpikeState(CompassDirection direction) : EnemyProjectileState
{
    /// <summary>The spike's direction among the burst's eight; it selects the spike's acceleration.</summary>
    internal CompassDirection Direction { get; } = direction;
}
