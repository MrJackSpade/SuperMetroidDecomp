namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The transitional entity for a species not yet converted to its own entity (#1312 stage 2). Every participation
/// answers with exactly the <see cref="EnemyAiTable"/> lookup the dispatcher made before, so binding changes no behavior.
/// Species replace it one at a time; it is deleted with the table.
/// </summary>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="dispatch">The enemy system whose handler table answers.</param>
internal sealed class TableBackedEnemyEntity(RoomEnemySlot slot, IEnemyDispatch dispatch)
    : IEnemyInitialization, IEnemyMainAi, IEnemyHurtAi, IEnemyFrozenAi, IEnemyTouchReaction, IEnemyShotReaction,
        IEnemyBombReaction, IEnemyPowerBombReaction, IEnemyTypedAnimation
{
    /// <summary>The slot the enemy occupies; the table's predicates read its live state.</summary>
    private RoomEnemySlot Slot { get; } = slot;

    /// <summary>The handler table; resolved at use because the system builds it lazily.</summary>
    private EnemyAiTable Ai => dispatch.Ai;

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() =>
        EnemyAiTable.Routine(Slot.Definition.Bank, Slot.Definition.InitializationAiPointer) is { } routine &&
        Ai.TryGetInitialization(routine, Slot, out EnemyInitializationAiHandler? handler) ? handler : null;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() =>
        EnemyAiTable.Routine(Slot.Definition.Bank, Slot.Definition.MainAiPointer) is { } routine &&
        Ai.TryGetMain(routine, Slot, out EnemyMainAiHandler? handler) ? handler : null;

    /// <inheritdoc/>
    public EnemyHurtAiHandler? FindHurt() => Ai.FindHurt(Slot);

    /// <inheritdoc/>
    public EnemyFrozenPreemption? FindFrozenPreemption() => Ai.FindFrozenPreemption(Slot);

    /// <inheritdoc/>
    public IReadOnlyList<Action<RoomEnemySlot>> FrozenHandlers() => Ai.FrozenHandlers(Slot);

    /// <inheritdoc/>
    public EnemyTouchHandler? FindTouch() => Ai.FindTouch(Slot);

    /// <inheritdoc/>
    public EnemyShotReaction? FindShot() => Ai.FindShot(Slot);

    /// <inheritdoc/>
    public Action<EnemyBombContext>? FindBombSpecial(EnemyBombContext context) => Ai.FindBombSpecial(context);

    /// <inheritdoc/>
    public Action<EnemyBombContext>? FindBombSpecialAfterNoOp(EnemyBombContext context) => Ai.FindBombSpecialAfterNoOp(context);

    /// <inheritdoc/>
    public EnemyBombReaction? FindBombCommon(EnemyBombContext context) => Ai.FindBombCommon(context);

    /// <inheritdoc/>
    public EnemyPowerBombReaction? FindPowerBomb() => Ai.FindPowerBomb(Slot);

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => Ai.FindTypedAnimation(Slot);
}
