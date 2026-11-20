using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// A defeated room's Lower Norfair Ridley (#2554): his setup finds him defeated and deletes him before any
/// encounter, workspace or colour side effects, so he has no encounter and no action of his own.
/// </summary>
/// <param name="place">The place he occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class DefeatedNorfairRidleyEntity(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi
{
    /// <summary>Takes no action: he is deleted as he is placed.</summary>
    /// <param name="frame">The enemy frame.</param>
    public void RunMainAi(IEnemyAiFrame frame)
    {
    }
}
