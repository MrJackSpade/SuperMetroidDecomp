using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies;

/// <summary>
/// The dispatch-equivalence fixture (#1312). For every enemy definition, the entity a slot binds must participate exactly
/// where the address-keyed <see cref="EnemyAiTable"/> did before any species converted, which the committed baseline
/// records; a table-backed entity must also resolve the table's very handlers. It catches a participation a species
/// conversion drops or adds, the defect class the #621 split hit (Shaktool touch). Removed with the table.
/// </summary>
internal static class EnemyDispatchEquivalenceVerification
{
    /// <summary>The participation each definition had through the full table, recorded before the first conversion.</summary>
    private const string BaselinePath = "csharp/test-fixtures/enemy-dispatch/participation.txt";

    /// <summary>Compares every definition's bound participation with the baseline and, while table-backed, the table.</summary>
    [VerificationCommand("--enemy-dispatch-equivalence")]
    internal static int EnemyDispatchEquivalenceCommand()
    {
        Dictionary<string, string> baseline = File.ReadAllLines(BaselinePath)
            .Select(line => line.Split('\t', 2))
            .ToDictionary(fields => fields[0], fields => fields[1], StringComparer.Ordinal);
        var fixture = new Fixture();
        int checks = 0;
        foreach ((EnemyDefinitionId id, RoomEnemyDefinition definition) in RoomEnemyDefinitionCatalog.Definitions)
        {
            IEnemyEntity entity = fixture.Bind(id, definition);
            AssertEqual(baseline[id.ToString()], fixture.Signature(entity), $"{id} participation matches the pre-conversion table");
            if (entity is TableBackedEnemyEntity)
                fixture.AssertSameHandlersAsTable(id, entity);
            checks++;
        }
        AssertEqual(baseline.Count, checks, "the baseline covers exactly the catalog's definitions");
        Console.WriteLine($"Enemy dispatch equivalence: {checks} definitions bind the participation the AI table resolved.");
        return 0;
    }

    /// <summary>
    /// Records the baseline. Valid only while every definition is table-backed: after a conversion it would record the
    /// conversion instead of the table, so it refuses.
    /// </summary>
    [VerificationCommand("--write-enemy-dispatch-participation")]
    internal static int WriteParticipationCommand()
    {
        var fixture = new Fixture();
        var lines = new List<string>();
        foreach ((EnemyDefinitionId id, RoomEnemyDefinition definition) in RoomEnemyDefinitionCatalog.Definitions)
        {
            IEnemyEntity entity = fixture.Bind(id, definition);
            if (entity is not TableBackedEnemyEntity)
                throw new InvalidOperationException($"{id} is converted; the baseline must be recorded from the full table.");
            lines.Add($"{id}\t{fixture.Signature(entity)}");
        }
        Directory.CreateDirectory(Path.GetDirectoryName(BaselinePath) ?? throw new InvalidOperationException(BaselinePath));
        File.WriteAllLines(BaselinePath, lines);
        Console.WriteLine($"Wrote {lines.Count} definitions to {BaselinePath}.");
        return 0;
    }

    /// <summary>One enemy system and slot that every definition is bound into in turn.</summary>
    private sealed class Fixture
    {
        /// <summary>Each callback a hitbox can select, and none: bomb predicates test the live hitbox callback.</summary>
        private static readonly EnemyInteractionCallback?[] Callbacks = [null, .. Enum.GetValues<EnemyInteractionCallback>()];

        /// <summary>The enemy system whose table and registry answer.</summary>
        private readonly RoomEnemySystem _enemies = new();

        /// <summary>A bomb for the bomb contexts.</summary>
        private readonly SamusBombProjectileSlot _bomb = new SamusBombProjectileSystem().Slots[0];

        /// <summary>The ordinary projectiles for the bomb contexts.</summary>
        private readonly SamusProjectileSystem _projectiles = new();

        /// <summary>The slot every definition is bound into.</summary>
        private RoomEnemySlot Slot => _enemies.SlotTable.Slots[0];

        /// <summary>Binds <paramref name="id"/> and returns its entity.</summary>
        internal IEnemyEntity Bind(EnemyDefinitionId id, RoomEnemyDefinition definition)
        {
            Slot.Bind(id, definition);
            return Slot.Entity ?? throw new InvalidDataException($"{id} bound no entity.");
        }

        /// <summary>The bomb context for one selected callback and walker.</summary>
        private EnemyBombContext BombContext(EnemyInteractionCallback? callback, bool extended) =>
            new(Slot, _bomb, _projectiles, null, callback, 0, extended);

        /// <summary>Where the entity participates, as one baseline line.</summary>
        internal string Signature(IEnemyEntity entity)
        {
            var parts = new List<string>();
            void Add(string name, bool present)
            {
                if (present)
                    parts.Add(name);
            }

            Add("init", (entity as IEnemyInitialization)?.FindInitialization() is not null);
            Add("main", (entity as IEnemyMainAi)?.FindMain() is not null);
            Add("hurt", (entity as IEnemyHurtAi)?.FindHurt() is not null);
            Add("frozen-preemption", (entity as IEnemyFrozenAi)?.FindFrozenPreemption() is not null);
            int frozen = (entity as IEnemyFrozenAi)?.FrozenHandlers().Count ?? 0;
            Add($"frozen*{frozen}", frozen > 0);
            Add("touch", (entity as IEnemyTouchReaction)?.FindTouch() is not null);
            Add("shot", (entity as IEnemyShotReaction)?.FindShot() is not null);
            Add("power-bomb", (entity as IEnemyPowerBombReaction)?.FindPowerBomb() is not null);
            Add("typed-animation", (entity as IEnemyTypedAnimation)?.FindTypedAnimation() is not null);
            Add($"capabilities={entity.Capabilities}", entity.Capabilities != EnemyCapabilities.None);
            var bombs = entity as IEnemyBombReaction;
            foreach (EnemyInteractionCallback? callback in Callbacks)
            foreach (bool extended in new[] { false, true })
            {
                EnemyBombContext context = BombContext(callback, extended);
                string at = $"{callback?.ToString() ?? "none"}{(extended ? "/extended" : "")}";
                Add($"bomb-special@{at}", bombs?.FindBombSpecial(context) is not null);
                Add($"bomb-after-no-op@{at}", bombs?.FindBombSpecialAfterNoOp(context) is not null);
                Add($"bomb-common@{at}", bombs?.FindBombCommon(context) is not null);
            }
            return parts.Count == 0 ? "-" : string.Join(' ', parts);
        }

        /// <summary>Asserts a table-backed entity resolves the table's very handlers, not just handlers in the same places.</summary>
        internal void AssertSameHandlersAsTable(EnemyDefinitionId id, IEnemyEntity entity)
        {
            EnemyAiTable table = _enemies.Ai;
            Same(Initialization(table, Slot), (entity as IEnemyInitialization)?.FindInitialization(), id, "initialization");
            Same(Main(table, Slot), (entity as IEnemyMainAi)?.FindMain(), id, "main AI");
            Same(table.FindHurt(Slot), (entity as IEnemyHurtAi)?.FindHurt(), id, "hurt AI");
            Same(table.FindFrozenPreemption(Slot), (entity as IEnemyFrozenAi)?.FindFrozenPreemption(), id, "frozen preemption");
            IReadOnlyList<Action<RoomEnemySlot>> frozen = (entity as IEnemyFrozenAi)?.FrozenHandlers() ?? [];
            AssertTrue(table.FrozenHandlers(Slot).SequenceEqual(frozen), $"{id} frozen AI handlers");
            Same(table.FindTouch(Slot), (entity as IEnemyTouchReaction)?.FindTouch(), id, "touch reaction");
            Same(table.FindShot(Slot), (entity as IEnemyShotReaction)?.FindShot(), id, "shot reaction");
            Same(table.FindPowerBomb(Slot), (entity as IEnemyPowerBombReaction)?.FindPowerBomb(), id, "power-bomb reaction");
            Same(table.FindTypedAnimation(Slot), (entity as IEnemyTypedAnimation)?.FindTypedAnimation(), id, "typed animation");
            var bombs = entity as IEnemyBombReaction;
            foreach (EnemyInteractionCallback? callback in Callbacks)
            foreach (bool extended in new[] { false, true })
            {
                EnemyBombContext context = BombContext(callback, extended);
                string label = $"bomb ({callback?.ToString() ?? "no callback"}, extended {extended})";
                Same(table.FindBombSpecial(context), bombs?.FindBombSpecial(context), id, label + " special");
                Same(table.FindBombSpecialAfterNoOp(context), bombs?.FindBombSpecialAfterNoOp(context), id, label + " after no-op");
                Same(table.FindBombCommon(context), bombs?.FindBombCommon(context), id, label + " common");
            }
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
}
