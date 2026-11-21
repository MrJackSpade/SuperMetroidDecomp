using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// A tail action that steers Ridley's tail each update (#2553): the free whip in flight and the pogo's steps. Only the
/// resting tail is steered by nothing.
/// </summary>
public abstract class RidleySteeredTailAction : RidleyTailAction
{
    /// <summary>An action named <paramref name="name"/>.</summary>
    private protected RidleySteeredTailAction(string name)
        : base(name)
    {
    }

    /// <summary>Steers <paramref name="tail"/> for one update of Ridley's encounter.</summary>
    /// <param name="tail">Ridley's tail.</param>
    /// <param name="place">Ridley's body.</param>
    /// <param name="state">Ridley's state.</param>
    /// <param name="samus">Samus, whom the tail follows and whips at.</param>
    /// <param name="samusProjectiles">Samus's beams and missiles, a missile among which the Ceres whip may aim at instead.</param>
    internal abstract void Steer(RidleyTail tail, RoomEnemyPlace place, RidleyEncounterState state, SamusState samus, SamusProjectileSystem samusProjectiles);
}
