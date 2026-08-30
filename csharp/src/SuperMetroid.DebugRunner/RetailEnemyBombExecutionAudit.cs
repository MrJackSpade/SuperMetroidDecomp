using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Executes the normal-bomb collision path against every authored retail enemy variant
/// whose definition installs the shared `$xx:802D` normal-shot callback.
/// </summary>
internal static partial class RetailEnemyExecutionAudit
{
    private const ushort CommonNormalShotAi = 0x802d;
    private const ushort AuditBombDamage = 20;

    public static int RunNormalBombCombat(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        RetailRoomState[] states = LoadNamedRetailStates();
        var testedVariants = new HashSet<NormalBombVariant>();
        var reachedDefinitions = new HashSet<ushort>();
        var commonDefinitions = new HashSet<ushort>();
        var literalNoOpDefinitions = new HashSet<ushort>();
        var privateCallbacks = new HashSet<PrivateBombCallback>();
        var unavailableDefinitions = new HashSet<ushort>();
        var failures = new List<NormalBombFailure>();
        int bombCallbacks = 0;

        foreach (RetailRoomState state in states)
        {
            try
            {
                CartridgeRoomHeader defaultRoom = CartridgeRoomHeader.Load(bus, state.RoomPointer);
                CartridgeRoomState exactState = CartridgeRoomState.Load(bus, state.StatePointer);
                CartridgeRoomHeader room = defaultRoom with { State = exactState };
                CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
                LoadedRetailState initial = LoadState(bus, room, assets);

                for (int slotIndex = 0; slotIndex < initial.Enemies.EnemyCount; slotIndex++)
                {
                    RoomEnemySlot initialTarget = initial.Enemies.Slots[slotIndex];
                    bool usesCommonShotAi =
                        initialTarget.Definition.ShotAiPointer == CommonNormalShotAi;
                    bool usesLiteralNoOpShotAi =
                        initialTarget.Definition.ShotAiPointer != 0 &&
                        bus.ReadByte(
                            (initialTarget.Definition.Bank << 16) |
                            initialTarget.Definition.ShotAiPointer) == 0x6b;
                    bool usesAuditableLiteralNoOpShotAi = usesLiteralNoOpShotAi &&
                        !initialTarget.ExtraProperties.HasAny(
                            EnemyExtraProperties.UsesExtendedSpritemap);
                    if (initialTarget.EnemyDefinitionPointer is 0 or 0xffff)
                    {
                        continue;
                    }
                    if (!usesCommonShotAi && !usesAuditableLiteralNoOpShotAi)
                    {
                        privateCallbacks.Add(new PrivateBombCallback(
                            initialTarget.EnemyDefinitionPointer,
                            initialTarget.Definition.Bank,
                            initialTarget.Definition.ShotAiPointer,
                            initialTarget.ExtraProperties.HasAny(
                                EnemyExtraProperties.UsesExtendedSpritemap)));
                        continue;
                    }

                    var variant = new NormalBombVariant(
                        initialTarget.EnemyDefinitionPointer,
                        initialTarget.Parameter1,
                        initialTarget.Parameter2,
                        initialTarget.Properties,
                        initialTarget.ExtraProperties);
                    if (!testedVariants.Add(variant))
                        continue;

                    (ushort cameraX, ushort cameraY) = CameraFor(room, initialTarget);
                    int activationFrame = FindProjectileActivationFrame(
                        bus,
                        room,
                        assets,
                        slotIndex,
                        variant.Definition,
                        cameraX,
                        cameraY);
                    if (activationFrame < 0)
                    {
                        unavailableDefinitions.Add(variant.Definition);
                        continue;
                    }

                    LoadedRetailState loaded = LoadState(bus, room, assets);
                    RoomEnemySlot target = loaded.Enemies.Slots[slotIndex];
                    AdvanceToProjectileFrame(
                        bus,
                        loaded,
                        assets.LevelData,
                        slotIndex,
                        cameraX,
                        cameraY,
                        activationFrame);
                    if (target.EnemyDefinitionPointer != variant.Definition ||
                        target.Properties.HasAny(EnemyProperties.Deleted) ||
                        target.InvincibilityTimer != 0)
                    {
                        throw new InvalidDataException(
                            $"Definition ${variant.Definition:X4} was live in the normal-bomb " +
                            "activation probe but not in its fresh collision load.");
                    }

                    // The activity/interaction indexes were frozen during the activation
                    // frame. Setting `$0400` now cannot remove an already-listed coincident
                    // actor, and normal-radius `$A0:A236` does not retest that property.
                    // Common shot AI has no parent/child tail, so delete non-target records
                    // explicitly and let the gameplay resolver's existing deleted guard
                    // isolate the one callback being measured.
                    for (int otherIndex = 0; otherIndex < loaded.Enemies.EnemyCount; otherIndex++)
                    {
                        if (otherIndex == slotIndex)
                            continue;
                        RoomEnemySlot companion = loaded.Enemies.Slots[otherIndex];
                        companion.Properties = companion.Properties.With(
                            EnemyProperties.Deleted);
                    }

                    SamusBombProjectileSlot bomb = loaded.SharedProjectiles.Slots[0];
                    ArmNormalBomb(bomb, target);
                    ushort vulnerabilityPointer = target.Definition.VulnerabilityPointer == 0
                        ? (ushort)0xec1c
                        : target.Definition.VulnerabilityPointer;
                    byte vulnerability = bus.ReadByte(
                        0xb40000 | unchecked((ushort)(vulnerabilityPointer + 14)));
                    // A literal RTL receives the native collision mark but never enters
                    // common vulnerability/damage AI.
                    int expectedDamage = usesCommonShotAi
                        ? (AuditBombDamage >> 1) * (vulnerability & 0x7f)
                        : 0;
                    ushort expectedHealth = expectedDamage >= target.Health
                        ? (ushort)0
                        : unchecked((ushort)(target.Health - expectedDamage));
                    bool expectedDeleted = expectedHealth == 0;
                    int killsBefore = loaded.Enemies.EnemiesKilled;

                    int hits = loaded.Enemies.ResolveOrdinaryBombHits(
                        loaded.SharedProjectiles,
                        loaded.SamusProjectiles,
                        loaded.Samus);
                    if (hits != 1 ||
                        (bomb.Direction & 0x0010) == 0 ||
                        target.Health != expectedHealth ||
                        target.Properties.HasAny(EnemyProperties.Deleted) != expectedDeleted ||
                        loaded.Enemies.EnemiesKilled !=
                            killsBefore + (expectedDeleted ? 1 : 0))
                    {
                        throw new InvalidDataException(
                            $"Definition ${variant.Definition:X4} normal bomb diverged: " +
                            $"hits={hits}, direction=${bomb.Direction:X4}, health=" +
                            $"{target.Health}/{expectedHealth}, deleted=" +
                            $"{target.Properties.HasAny(EnemyProperties.Deleted)}/" +
                            $"{expectedDeleted}, kills={loaded.Enemies.EnemiesKilled}/" +
                            $"{killsBefore + (expectedDeleted ? 1 : 0)}, vulnerability=" +
                            $"${vulnerability:X2}.");
                    }

                    bombCallbacks++;
                    reachedDefinitions.Add(variant.Definition);
                    if (usesCommonShotAi)
                        commonDefinitions.Add(variant.Definition);
                    else
                        literalNoOpDefinitions.Add(variant.Definition);
                }
            }
            catch (Exception exception)
            {
                failures.Add(new NormalBombFailure(state, exception));
            }
        }

        if (failures.Count != 0)
        {
            Console.Error.WriteLine(
                $"Retail normal-bomb audit found {failures.Count} failing room states:");
            foreach (IGrouping<string, NormalBombFailure> group in failures
                .GroupBy(failure =>
                    $"{failure.Exception.GetType().Name}: {failure.Exception.Message}")
                .OrderBy(group => group.Key, StringComparer.Ordinal))
            {
                Console.Error.WriteLine($"  {group.Key}");
                foreach (NormalBombFailure failure in group.Take(12))
                {
                    Console.Error.WriteLine(
                        $"    room/state $8F:{failure.State.RoomPointer:X4}/" +
                        $"${failure.State.StatePointer:X4} {failure.State.Symbol}");
                }
                if (group.Count() > 12)
                    Console.Error.WriteLine($"    ... and {group.Count() - 12} more states");
            }
            return 1;
        }

        Console.WriteLine(
            $"Retail normal-bomb audit passed: {testedVariants.Count} authored supported " +
            $"variants inspected, {bombCallbacks} physical bomb callbacks executed across " +
            $"{reachedDefinitions.Count} definitions ({commonDefinitions.Count} common, " +
            $"{literalNoOpDefinitions.Count} literal RTL), and {unavailableDefinitions.Count} " +
            "definitions remained naturally deleted, empty, or intangible.");
        Console.WriteLine(
            $"Private normal-bomb callback residual inventory ({privateCallbacks.Count}): " +
            string.Join(", ", privateCallbacks
                .OrderBy(callback => callback.Definition)
                .Select(callback =>
                    $"${callback.Definition:X4}=$" +
                    $"{callback.Bank:X2}:{callback.Pointer:X4}" +
                    (callback.UsesExtendedHitboxes ? "[extended]" : string.Empty))));
        return 0;
    }

    private static void ArmNormalBomb(
        SamusBombProjectileSlot bomb,
        RoomEnemySlot target)
    {
        bomb.ClearFields();
        bomb.Type = SamusBombProjectileSystem.NormalBombType;
        bomb.Damage = AuditBombDamage;
        bomb.Direction = (ushort)SamusProjectileDirection.Right;
        bomb.XPosition = target.XPosition;
        bomb.YPosition = target.YPosition;
        bomb.XRadius = 16;
        bomb.YRadius = 16;
        bomb.BombTimer = 0;
        bomb.InstructionPointer = 0xa06b;
        bomb.InstructionTimer = 1;
    }

    private readonly record struct NormalBombVariant(
        ushort Definition,
        ushort Parameter1,
        ushort Parameter2,
        ushort Properties,
        ushort ExtraProperties);

    private sealed record NormalBombFailure(
        RetailRoomState State,
        Exception Exception);

    private readonly record struct PrivateBombCallback(
        ushort Definition,
        byte Bank,
        ushort Pointer,
        bool UsesExtendedHitboxes);
}
