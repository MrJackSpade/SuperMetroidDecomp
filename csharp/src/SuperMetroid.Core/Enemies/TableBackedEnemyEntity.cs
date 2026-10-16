namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The transitional entity for a species not yet converted to its own entity (#1312 stage 2). Every participation
/// answers with exactly the <see cref="EnemyAiTable"/> lookup the dispatcher made before, so binding changes no behavior.
/// Species replace it one at a time; it is deleted with the table.
/// </summary>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation, whose table also still holds the unconverted species' handlers.</param>
internal sealed class TableBackedEnemyEntity(RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() =>
        EnemyAiTable.Routine(Slot.Definition.Bank, Slot.Definition.InitializationAiPointer) is { } routine &&
        Common.Ai.TryGetInitialization(routine, Slot, out EnemyInitializationAiHandler? handler) ? handler : null;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() =>
        EnemyAiTable.Routine(Slot.Definition.Bank, Slot.Definition.MainAiPointer) is { } routine &&
        Common.Ai.TryGetMain(routine, Slot, out EnemyMainAiHandler? handler) ? handler : null;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => Common.Ai.FindTypedAnimation(Slot);
}
