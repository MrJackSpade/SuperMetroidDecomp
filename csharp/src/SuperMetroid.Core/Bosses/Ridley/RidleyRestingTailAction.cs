using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Ridley's tail with nothing steering it (<see cref="RidleyTailAction.None"/>): its segments stay where they are.</summary>
internal sealed class RidleyRestingTailAction() : RidleyTailAction(nameof(None))
{
    /// <inheritdoc/>
    internal override void Steer(RidleyTail tail, RoomEnemyPlace place, RidleyEncounterState state, SamusState samus, SamusProjectileSystem samusProjectiles)
    {
        // Nothing steers the tail; its offsets stay as they are.
    }
}
