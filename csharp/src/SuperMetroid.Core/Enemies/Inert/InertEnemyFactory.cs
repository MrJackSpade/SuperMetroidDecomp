namespace SuperMetroid.Core.Enemies.Inert;

/// <summary>
/// The occupants whose headers name only the shared $804C long returns: the respawn placeholder that holds a killed
/// enemy's slot, and the Tourian statue ghost. They run the shared participation and no species AI.
/// </summary>
internal sealed class InertEnemyFactory : IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Respawn, EnemyDefinitionId.TourianStatueGhost];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new InertEnemyEntity(slot, common);
}
