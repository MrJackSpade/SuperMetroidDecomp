using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// A defeated room's Crocomire tongue (#2554): the cartridge's tongue setup finds Crocomire defeated and deletes the tongue
/// before setting anything up, so it has no state and no action of its own.
/// </summary>
/// <param name="place">The place the tongue occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class DefeatedCrocomireTongueEntity(RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi
{
    /// <summary>Takes no action: the tongue is deleted as it is placed.</summary>
    /// <param name="frame">The enemy frame.</param>
    public void RunMainAi(IEnemyAiFrame frame)
    {
    }
}
