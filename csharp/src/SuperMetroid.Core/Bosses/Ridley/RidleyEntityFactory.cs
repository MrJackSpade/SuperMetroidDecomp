using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The factory for Lower Norfair Ridley, Ceres Ridley and the breakup explosion actor. The two Ridleys share the body
/// program and drawing, while Ceres Ridley's initialization and main AI are the Ceres encounter's.
/// </summary>
/// <param name="ridley">The Lower Norfair Ridley owner.</param>
/// <param name="ceres">The Ceres Ridley owner.</param>
/// <param name="encounter">The shared Ridley encounter.</param>
internal sealed class RidleyEntityFactory(RidleyBehavior ridley, CeresRidleyBehavior ceres, RidleyEncounter encounter) : IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } =
        [.. RidleyPartDefinitions.Parts.Keys];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) =>
        new RidleyEntity(ridley, ceres, encounter, slot, common);
}
