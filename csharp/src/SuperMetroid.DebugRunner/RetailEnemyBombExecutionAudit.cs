using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Executes the normal-bomb collision path against every authored retail enemy variant
/// whose definition installs either shared `$xx:802D` normal-shot AI or one of the exact
/// private callbacks covered by this audit's behavior table.
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
        var privateDefinitions = new HashSet<ushort>();
        var literalNoOpDefinitions = new HashSet<ushort>();
        var focusedCallbacks = new HashSet<PrivateBombCallback>();
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
                    if (initialTarget.EnemyDefinitionPointer is 0 or 0xffff)
                    {
                        continue;
                    }
                    NormalBombAuditBehavior behavior = ClassifyNormalBombBehavior(
                        bus,
                        initialTarget);
                    if (behavior == NormalBombAuditBehavior.FocusedOnly)
                    {
                        focusedCallbacks.Add(new PrivateBombCallback(
                            initialTarget.EnemyDefinitionPointer,
                            initialTarget.Definition.Bank,
                            initialTarget.Definition.ShotAiPointer,
                            initialTarget.ExtraProperties.HasAny(
                                EnemyExtraProperties.UsesExtendedSpritemap)));
                        continue;
                    }
                    if (behavior == NormalBombAuditBehavior.Unsupported)
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
                    int expectedDamage = behavior is
                            NormalBombAuditBehavior.CommonDamage or
                            NormalBombAuditBehavior.PrivateCommonDamage
                        ? (AuditBombDamage >> 1) * (vulnerability & 0x7f)
                        : 0;
                    ushort expectedHealth = expectedDamage >= target.Health
                        ? (ushort)0
                        : unchecked((ushort)(target.Health - expectedDamage));
                    bool expectedDeleted = expectedHealth == 0;
                    bool expectedCollisionMark =
                        behavior != NormalBombAuditBehavior.PrivateDirectionClear;
                    int killsBefore = loaded.Enemies.EnemiesKilled;

                    int hits = loaded.Enemies.ResolveOrdinaryBombHits(
                        loaded.SharedProjectiles,
                        loaded.SamusProjectiles,
                        loaded.Samus);
                    if (hits != 1 ||
                        ((bomb.Direction & 0x0010) != 0) != expectedCollisionMark ||
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
                    switch (behavior)
                    {
                        case NormalBombAuditBehavior.CommonDamage:
                            commonDefinitions.Add(variant.Definition);
                            break;
                        case NormalBombAuditBehavior.LiteralNoOp:
                            literalNoOpDefinitions.Add(variant.Definition);
                            break;
                        default:
                            privateDefinitions.Add(variant.Definition);
                            break;
                    }
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
            $"{privateDefinitions.Count} private, {literalNoOpDefinitions.Count} literal RTL), " +
            $"and {unavailableDefinitions.Count} " +
            "definitions remained naturally deleted, empty, or intangible.");
        Console.WriteLine(
            $"Focused normal-bomb callbacks ({focusedCallbacks.Count}): " +
            string.Join(", ", focusedCallbacks
                .OrderBy(callback => callback.Definition)
                .Select(callback =>
                    $"${callback.Definition:X4}=$" +
                    $"{callback.Bank:X2}:{callback.Pointer:X4}" +
                    (callback.UsesExtendedHitboxes ? "[extended]" : string.Empty))));
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

    /// <summary>
    /// Maps only callback contracts whose bomb result can be asserted from public actor state.
    /// The tuple includes the definition and bank so unrelated same-bank pointer aliases cannot
    /// accidentally gain coverage. Extended actors stay in focused hitbox audits because a bomb
    /// placed at the actor origin does not prove which rectangle/callback the cartridge selected.
    /// </summary>
    private static NormalBombAuditBehavior ClassifyNormalBombBehavior(
        ISnesAddressSpace bus,
        RoomEnemySlot target)
    {
        ushort callback = target.Definition.ShotAiPointer;
        if (callback == CommonNormalShotAi)
            return NormalBombAuditBehavior.CommonDamage;

        bool usesExtendedHitboxes = target.ExtraProperties.HasAny(
            EnemyExtraProperties.UsesExtendedSpritemap);
        if (callback != 0 && !usesExtendedHitboxes &&
            bus.ReadByte((target.Definition.Bank << 16) | callback) == 0x6b)
        {
            return NormalBombAuditBehavior.LiteralNoOp;
        }

        return (target.EnemyDefinitionPointer, target.Definition.Bank, callback) switch
        {
            // Baby Turtle, GRipper/Ripper II, Dragon, Metaree, Fireflea, Tripper,
            // Mochtroid, Skree, Yard, and the destroyable shutter all reach common bomb
            // vulnerability damage before executing their already-translated private tail.
            (0xcf7f, 0xa2, 0x930f) => NormalBombAuditBehavior.PrivateCommonDamage,
            (0xd3ff, 0xa2, 0xe3a9) => NormalBombAuditBehavior.PrivateCommonDamage,
            (0xd43f, 0xa2, 0xe3a9) => NormalBombAuditBehavior.PrivateCommonDamage,
            (0xd4bf, 0xa2, 0xe7ce) => NormalBombAuditBehavior.PrivateCommonDamage,
            (0xd5bf, 0xa2, 0xf0aa) => NormalBombAuditBehavior.PrivateCommonDamage,
            (0xd67f, 0xa3, 0x8b0f) => NormalBombAuditBehavior.PrivateCommonDamage,
            (0xd6bf, 0xa3, 0x8e89) => NormalBombAuditBehavior.PrivateCommonDamage,
            (0xd7ff, 0xa3, 0x9f08) => NormalBombAuditBehavior.PrivateCommonDamage,
            (0xd8ff, 0xa3, 0xa9a8) => NormalBombAuditBehavior.PrivateCommonDamage,
            (0xdb7f, 0xa3, 0xc7f5) => NormalBombAuditBehavior.PrivateCommonDamage,
            (0xdbbf, 0xa3, 0xd469) => NormalBombAuditBehavior.PrivateCommonDamage,

            // Bank-$A8 families reuse common bomb damage and then synchronize children,
            // release Samus, start attacks/staged reactions, or update typed family state.
            (0xe63f, 0xa8, 0x8b12) => NormalBombAuditBehavior.PrivateCommonDamage,
            (0xe7bf, 0xa8, 0xa7bd) => NormalBombAuditBehavior.PrivateCommonDamage,
            (0xe7ff, 0xa8, 0xab83) => NormalBombAuditBehavior.PrivateCommonDamage,
            (0xe83f, 0xa8, 0xb40c) => NormalBombAuditBehavior.PrivateCommonDamage,
            (0xe87f, 0xa8, 0xbeac) => NormalBombAuditBehavior.PrivateCommonDamage,
            (0xe93f, 0xa8, 0xd18d) => NormalBombAuditBehavior.PrivateCommonDamage,
            (0xe97f, 0xa8, 0xdb14) => NormalBombAuditBehavior.PrivateCommonDamage,
            (0xeabf, 0xa8, 0xf701) => NormalBombAuditBehavior.PrivateCommonDamage,
            (0xeb3f, 0xa8, 0xf701) => NormalBombAuditBehavior.PrivateCommonDamage,
            (0xebbf, 0xa8, 0xf701) => NormalBombAuditBehavior.PrivateCommonDamage,
            (0xe0ff, 0xa6, 0x9c39) => NormalBombAuditBehavior.PrivateCommonDamage,

            // Every non-gold bank-$B2 Pirate component callback ultimately jumps to normal
            // Pirate shot handling for a family-$0500 bomb. Gold Ninja is the sole identity
            // check: both of its authored component callbacks return after the collision
            // mark because normal bombs sort after Power Bomb family $0300.
            (0xf353, 0xb2, 0x8779) => NormalBombAuditBehavior.PrivateCommonDamage,
            (0xf413, 0xb2, 0x8779) => NormalBombAuditBehavior.PrivateCommonDamage,
            (0xf453, 0xb2, 0x8779) => NormalBombAuditBehavior.PrivateCommonDamage,
            (0xf493, 0xb2, 0x8779) => NormalBombAuditBehavior.PrivateCommonDamage,
            (0xf613, 0xb2, 0x8779) => NormalBombAuditBehavior.PrivateCommonDamage,
            (0xf653, 0xb2, 0x8779) => NormalBombAuditBehavior.PrivateCommonDamage,
            (0xf693, 0xb2, 0x8779) => NormalBombAuditBehavior.PrivateCommonDamage,
            (0xf6d3, 0xb2, 0x8779) => NormalBombAuditBehavior.PrivateCommonDamage,
            (0xf713, 0xb2, 0x8779) => NormalBombAuditBehavior.PrivateCommonDamage,
            (0xf753, 0xb2, 0x8779) => NormalBombAuditBehavior.PrivateCommonDamage,
            (0xf793, 0xb2, 0x8779) => NormalBombAuditBehavior.PrivateCommonDamage,

            // These platform callbacks react without entering vulnerability damage.
            (0xd53f, 0xa2, 0xf0a2) => NormalBombAuditBehavior.PrivateReactionOnly,
            (0xd5ff, 0xa2, 0xf0a2) => NormalBombAuditBehavior.PrivateReactionOnly,

            // Metroid's callback consumes the normal bomb as detach/recoil input and never
            // applies ordinary health damage while unfrozen.
            (0xdd7f, 0xa3, 0xef07) => NormalBombAuditBehavior.PrivateReactionOnly,
            (0xed3f, 0xa9, 0xd433) => NormalBombAuditBehavior.PrivateReactionOnly,
            (0xed7f, 0xa9, 0xdd1d) => NormalBombAuditBehavior.PrivateReactionOnly,
            (0xedff, 0xa9, 0xdcf8) => NormalBombAuditBehavior.PrivateReactionOnly,
            (0xee3f, 0xa9, 0xdd08) => NormalBombAuditBehavior.PrivateReactionOnly,
            (0xee7f, 0xa9, 0xdd18) => NormalBombAuditBehavior.PrivateReactionOnly,
            (0xf593, 0xb2, 0x8779) => NormalBombAuditBehavior.PrivateReactionOnly,

            // Spark and the Blue Brinstar face block explicitly undo bank $A0's direction
            // bit-$10 collision mark and never enter vulnerability damage.
            (0xea3f, 0xa8, 0xe70e) => NormalBombAuditBehavior.PrivateDirectionClear,
            (0xea7f, 0xa8, 0xe91d) => NormalBombAuditBehavior.PrivateDirectionClear,

            // These callbacks are translated but need family-state or rectangle-specific
            // assertions that this public-state sweep intentionally cannot infer. Their
            // dedicated Rinka, Oum, Powamp, and Work Robot audits exercise the exact tails.
            (0xd23f, 0xa2, 0xb94d) => NormalBombAuditBehavior.FocusedOnly,
            (0xd37f, 0xa2, 0xd3b4) => NormalBombAuditBehavior.FocusedOnly,
            (0xe8bf, 0xa8, 0xc5ef) => NormalBombAuditBehavior.FocusedOnly,
            (0xe8ff, 0xa8, 0xd192) => NormalBombAuditBehavior.FocusedOnly,
            (0xe27f, 0xa6, 0xfdac) => NormalBombAuditBehavior.FocusedOnly,
            (0xde3f, 0xa5, 0x95f0) => NormalBombAuditBehavior.FocusedOnly,
            (0xdf3f, 0xa5, 0xed5a) => NormalBombAuditBehavior.FocusedOnly,
            (0xddbf, 0xa4, 0x0000) => NormalBombAuditBehavior.FocusedOnly,
            (0xeebf, 0xa9, 0xf842) => NormalBombAuditBehavior.FocusedOnly,
            (0xf293, 0xb3, 0xa016) => NormalBombAuditBehavior.FocusedOnly,
            _ => NormalBombAuditBehavior.Unsupported,
        };
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

    private enum NormalBombAuditBehavior : byte
    {
        Unsupported,
        CommonDamage,
        PrivateCommonDamage,
        PrivateReactionOnly,
        PrivateDirectionClear,
        LiteralNoOp,
        FocusedOnly,
    }
}
