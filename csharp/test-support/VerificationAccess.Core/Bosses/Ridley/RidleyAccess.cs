using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Verification's reads of the Ridley encounter a room's body holds (#2231): each body holds its own from its
/// initialization, so a check finds it on the body, and a fixture gives a hand-made body its encounter as initialization
/// does.
/// </summary>
internal static class RidleyAccess
{
    /// <summary>Lower Norfair Ridley's encounter as his body holds it, or null when no body of the room holds one.</summary>
    internal static NorfairRidleyState? NorfairRidley(this RoomEnemySystem enemies) => Held<NorfairRidleyState>(enemies);

    /// <summary>Ceres Ridley's encounter as his body holds it, or null when no body of the room holds one.</summary>
    internal static CeresRidleyState? CeresRidley(this RoomEnemySystem enemies) => Held<CeresRidleyState>(enemies);

    /// <summary>The encounter either Ridley's body holds, or null when no body of the room holds one.</summary>
    internal static RidleyEncounterState? Ridley(this RoomEnemySystem enemies) =>
        (RidleyEncounterState?)enemies.NorfairRidley() ?? enemies.CeresRidley();



    /// <summary>The <typeparamref name="TState"/> the first body of the room holds, or null.</summary>
    private static TState? Held<TState>(RoomEnemySystem enemies) where TState : RidleyEncounterState =>
        enemies.Places.InUpdateOrder.Select(place => place.SpeciesState<TState>()).FirstOrDefault(state => state is not null);
}
