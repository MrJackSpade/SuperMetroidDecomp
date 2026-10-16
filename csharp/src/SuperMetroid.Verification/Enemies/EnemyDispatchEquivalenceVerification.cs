using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies;

/// <summary>
/// The dispatch-equivalence fixture (#1312): for every enemy definition, the entity a slot binds resolves exactly the
/// handlers the address-keyed <see cref="EnemyAiTable"/> resolves for the same slot. It catches a participation a
/// species conversion drops or adds, the defect class the #621 split hit (Shaktool touch). Removed with the table.
/// </summary>
internal static class EnemyDispatchEquivalenceVerification
{
    /// <summary>Compares every definition's bound participation with the table's resolution.</summary>
    [VerificationCommand("--enemy-dispatch-equivalence")]
    internal static int EnemyDispatchEquivalenceCommand()
    {
        var enemies = new RoomEnemySystem();
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        EnemyAiTable table = enemies.Ai;
        SamusBombProjectileSlot bomb = new SamusBombProjectileSystem().Slots[0];
        var projectiles = new SamusProjectileSystem();
        EnemyInteractionCallback?[] callbacks = [null, .. Enum.GetValues<EnemyInteractionCallback>()];
        int checks = 0;
        foreach ((EnemyDefinitionId id, RoomEnemyDefinition definition) in RoomEnemyDefinitionCatalog.Definitions)
        {
            slot.Bind(id, definition);
            IEnemyEntity entity = slot.Entity ?? throw new InvalidDataException($"{id} bound no entity.");

            Same(Initialization(table, slot), (entity as IEnemyInitialization)?.FindInitialization(), id, "initialization");
            Same(Main(table, slot), (entity as IEnemyMainAi)?.FindMain(), id, "main AI");
            Same(table.FindHurt(slot), (entity as IEnemyHurtAi)?.FindHurt(), id, "hurt AI");
            Same(table.FindFrozenPreemption(slot), (entity as IEnemyFrozenAi)?.FindFrozenPreemption(), id, "frozen preemption");
            IReadOnlyList<Action<RoomEnemySlot>> frozen = (entity as IEnemyFrozenAi)?.FrozenHandlers() ?? [];
            AssertTrue(table.FrozenHandlers(slot).SequenceEqual(frozen), $"{id} frozen AI handlers");
            Same(table.FindTouch(slot), (entity as IEnemyTouchReaction)?.FindTouch(), id, "touch reaction");
            Same(table.FindShot(slot), (entity as IEnemyShotReaction)?.FindShot(), id, "shot reaction");
            Same(table.FindPowerBomb(slot), (entity as IEnemyPowerBombReaction)?.FindPowerBomb(), id, "power-bomb reaction");
            Same(table.FindTypedAnimation(slot), (entity as IEnemyTypedAnimation)?.FindTypedAnimation(), id, "typed animation");

            // Bomb predicates test the live hitbox callback, so each callback a hitbox can select is compared.
            var bombs = entity as IEnemyBombReaction;
            foreach (EnemyInteractionCallback? callback in callbacks)
            foreach (bool extended in new[] { false, true })
            {
                var context = new EnemyBombContext(slot, bomb, projectiles, null, callback, 0, extended);
                string label = $"bomb ({callback?.ToString() ?? "no callback"}, extended {extended})";
                Same(table.FindBombSpecial(context), bombs?.FindBombSpecial(context), id, label + " special");
                Same(table.FindBombSpecialAfterNoOp(context), bombs?.FindBombSpecialAfterNoOp(context), id, label + " after no-op");
                Same(table.FindBombCommon(context), bombs?.FindBombCommon(context), id, label + " common");
            }
            checks++;
        }
        slot.Unbind();
        Console.WriteLine($"Enemy dispatch equivalence: {checks} definitions bind the participation the AI table resolves.");
        return 0;
    }

    /// <summary>The table's initialization handler for the slot's header, or null.</summary>
    private static EnemyInitializationAiHandler? Initialization(EnemyAiTable table, RoomEnemySlot slot) =>
        EnemyAiTable.Routine(slot.Definition.Bank, slot.Definition.InitializationAiPointer) is { } routine &&
        table.TryGetInitialization(routine, slot, out EnemyInitializationAiHandler? handler) ? handler : null;

    /// <summary>The table's main-AI handler for the slot's header, or null.</summary>
    private static EnemyMainAiHandler? Main(EnemyAiTable table, RoomEnemySlot slot) =>
        EnemyAiTable.Routine(slot.Definition.Bank, slot.Definition.MainAiPointer) is { } routine &&
        table.TryGetMain(routine, slot, out EnemyMainAiHandler? handler) ? handler : null;

    /// <summary>Asserts the entity resolves the table's handler: equal delegates, or the same reaction object.</summary>
    private static void Same(object? expected, object? actual, EnemyDefinitionId id, string participation) =>
        AssertTrue(expected is Delegate ? Equals(expected, actual) : ReferenceEquals(expected, actual),
            $"{id} {participation}: entity resolves {Describe(actual)}, table resolves {Describe(expected)}");

    /// <summary>A handler's method name, for a failure message.</summary>
    private static string Describe(object? handler) => handler switch
    {
        null => "nothing",
        Delegate d => $"{d.Method.DeclaringType?.Name}.{d.Method.Name}",
        _ => handler.GetType().Name,
    };
}
