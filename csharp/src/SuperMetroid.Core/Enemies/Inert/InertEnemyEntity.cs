namespace SuperMetroid.Core.Enemies.Inert;

/// <summary>An occupant whose initialization and main AI are the shared long returns its header names.</summary>
/// <param name="slot">The slot the occupant holds.</param>
/// <param name="common">The shared participation.</param>
internal sealed class InertEnemyEntity(RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => EnemyCommonParticipation.FindInitialization(Slot);

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => EnemyCommonParticipation.FindMain(Slot);
}
