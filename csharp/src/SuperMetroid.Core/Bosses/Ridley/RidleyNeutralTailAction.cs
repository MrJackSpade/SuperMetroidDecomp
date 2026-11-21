using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Ridley's tail curling and whipping freely behind him in flight (<see cref="RidleyTailAction.Neutral"/>).</summary>
internal sealed class RidleyNeutralTailAction() : RidleySteeredTailAction(nameof(Neutral))
{
    /// <inheritdoc/>
    internal override void Steer(RidleyTail tail, RoomEnemyPlace place, RidleyEncounterState state, SamusState samus, SamusProjectileSystem samusProjectiles) =>
        Ensure.NotNull(tail).SteerNeutralTail(place, state, samus, samusProjectiles);
}
