using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Which room states Ceres Ridley's getaway can start in (#1649): those whose enemies include him.</summary>
public static class CeresGetawayRooms
{
    /// <summary>Whether <paramref name="state"/> places Ceres Ridley, whose retreat alone starts the getaway's turning floor.</summary>
    public static bool HoldsRidley(RoomStateSettings state)
    {
        Ensure.NotNull(state);
        foreach (RoomEnemyPopulationRecord record in state.EnemyPopulation.Records.Span)
        {
            if (record.DefinitionId == EnemyDefinitionId.RidleyCeres)
                return true;
        }
        return false;
    }
}
