namespace SuperMetroid.Core.Enemies.Inert;

/// <summary>An occupant of a definition no species owns (constructed test data): it does nothing when it is initialized or each frame.</summary>
/// <param name="slot">The slot the occupant holds.</param>
/// <param name="common">The shared participation.</param>
internal sealed class InertEnemyEntity(RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi
{
    /// <inheritdoc/>
    /// <remarks>The placeholder sets nothing up.</remarks>
    public void Initialize()
    {
    }

    /// <inheritdoc/>
    /// <remarks>The placeholder takes no action of its own.</remarks>
    public void RunMainAi(IEnemyAiFrame frame)
    {
    }
}
