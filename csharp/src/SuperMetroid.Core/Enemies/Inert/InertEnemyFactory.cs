namespace SuperMetroid.Core.Enemies.Inert;

/// <summary>
/// The occupant whose definition names no AI and ignores Samus's touch and shots: the respawn placeholder that holds
/// a killed enemy's slot. It runs the shared participation and no species AI.
/// </summary>
internal sealed class InertEnemyFactory : IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Respawn];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new InertEnemyEntity(slot, common);
}
