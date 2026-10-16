namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The reactions, AI-state handlers and collision capabilities the enemy system shares across species, which entities
/// compose for every participation they do not own, keeping the shared lists' first-match precedence. Until #1312
/// stage 8 it answers from the system's AI table, which then holds only the shared and unconverted registrations.
/// </summary>
/// <param name="dispatch">The enemy system whose AI table answers.</param>
internal sealed class EnemyCommonParticipation(IEnemyDispatch dispatch)
{
    /// <summary>The handler table; resolved at use because the system builds it lazily.</summary>
    internal EnemyAiTable Ai => dispatch.Ai;

    /// <summary>
    /// The shared initialization handler for <paramref name="slot"/>'s header routine (for example the common RTL), or
    /// null. Species registrations are guarded by definition, so for a converted species only shared ones match.
    /// </summary>
    internal EnemyInitializationAiHandler? FindInitialization(RoomEnemySlot slot) =>
        EnemyAiTable.Routine(slot.Definition.Bank, slot.Definition.InitializationAiPointer) is { } routine &&
        Ai.TryGetInitialization(routine, slot, out EnemyInitializationAiHandler? handler) ? handler : null;

    /// <summary>The shared main-AI handler for <paramref name="slot"/>'s header routine, or null.</summary>
    internal EnemyMainAiHandler? FindMain(RoomEnemySlot slot) =>
        EnemyAiTable.Routine(slot.Definition.Bank, slot.Definition.MainAiPointer) is { } routine &&
        Ai.TryGetMain(routine, slot, out EnemyMainAiHandler? handler) ? handler : null;

    /// <summary>The shared typed animation matching <paramref name="slot"/>, or null.</summary>
    internal Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation(RoomEnemySlot slot) => Ai.FindTypedAnimation(slot);

    /// <summary>The first hurt handler matching <paramref name="slot"/>, or null.</summary>
    internal EnemyHurtAiHandler? FindHurt(RoomEnemySlot slot) => Ai.FindHurt(slot);

    /// <summary>The first frozen preemption matching <paramref name="slot"/>, or null.</summary>
    internal EnemyFrozenPreemption? FindFrozenPreemption(RoomEnemySlot slot) => Ai.FindFrozenPreemption(slot);

    /// <summary>Every frozen handler matching <paramref name="slot"/>, in order.</summary>
    internal IReadOnlyList<Action<RoomEnemySlot>> FrozenHandlers(RoomEnemySlot slot) => Ai.FrozenHandlers(slot);

    /// <summary>The first touch handler matching <paramref name="slot"/>, or null.</summary>
    internal EnemyTouchHandler? FindTouch(RoomEnemySlot slot) => Ai.FindTouch(slot);

    /// <summary>The first shot reaction matching <paramref name="slot"/>, or null.</summary>
    internal EnemyShotReaction? FindShot(RoomEnemySlot slot) => Ai.FindShot(slot);

    /// <summary>The first special bomb handler matching <paramref name="context"/>, or null.</summary>
    internal Action<EnemyBombContext>? FindBombSpecial(EnemyBombContext context) => Ai.FindBombSpecial(context);

    /// <summary>The first after-no-op bomb handler matching <paramref name="context"/>, or null.</summary>
    internal Action<EnemyBombContext>? FindBombSpecialAfterNoOp(EnemyBombContext context) => Ai.FindBombSpecialAfterNoOp(context);

    /// <summary>The first common bomb reaction matching <paramref name="context"/>, or null.</summary>
    internal EnemyBombReaction? FindBombCommon(EnemyBombContext context) => Ai.FindBombCommon(context);

    /// <summary>The first power-bomb reaction matching <paramref name="slot"/>, or null.</summary>
    internal EnemyPowerBombReaction? FindPowerBomb(RoomEnemySlot slot) => Ai.FindPowerBomb(slot);
}
