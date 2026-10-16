using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy with contact work the shared pass runs at fixed points around its ordinary collision passes.</summary>
internal interface IEnemyContactHooks : IEnemyEntity
{
    /// <summary>Runs before the ordinary projectile, bomb and touch passes.</summary>
    /// <param name="bus">The room's address space.</param>
    /// <param name="frame">The enemy frame.</param>
    void ResolveBeforeOrdinaryHits(ISnesAddressSpace bus, RoomEnemyFrame frame);

    /// <summary>Runs after the ordinary Samus contact pass.</summary>
    /// <param name="samus">Samus.</param>
    void ResolveAfterSamusContact(SamusState samus);
}
