namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The entity of an occupant whose definition no species owns: the respawn placeholder, Reflec, the second dead
/// Sidehopper, the Tourian statue ghost, or a header no species translates. It runs only the shared participation; an
/// AI the header names that is not a common no-op fails loudly at dispatch.
/// </summary>
/// <param name="slot">The slot the occupant holds.</param>
/// <param name="common">The shared participation.</param>
internal sealed class UnownedEnemyEntity(RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => EnemyCommonParticipation.FindInitialization(Slot);

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => EnemyCommonParticipation.FindMain(Slot);
}
