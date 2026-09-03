using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>
/// ROM-backed audit for the four persistent Zebetite generations in Mother Brain's room.
/// The population prefix retains the untouched $E27F record while excluding the two boss
/// actors that precede it and the Rinkas that follow it. Generation tables, embedded spawn
/// records, event bits, health animation, palette data, and projectile vulnerability remain
/// cartridge sourced.
/// </summary>
internal static class ZebetiteAudit
{
    private const ushort MotherBrainRoom = 0xdd58;
    private const ushort ZebetitePopulationRecord = 0xe341;
    private const ushort ZebetiteDefinition = 0xe27f;
    private const ushort GenerationFlagsTable = 0xfc03;
    private const ushort YRadiusTable = 0xfc0b;
    private const ushort InitialInstructionTable = 0xfc13;
    private const ushort XPositionTable = 0xfc1b;
    private const ushort UpperYPositionTable = 0xfc23;
    private const ushort UpperHealthInstructionTable = 0xfd4a;
    private const ushort LowerHealthInstructionTable = 0xfd54;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, MotherBrainRoom);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);

        VerifyDefinitionAndPopulation(bus, room);
        VerifyEveryGenerationInitialization(bus, room, assets);
        VerifyLinkedShotAndContact(bus, room, assets);
        VerifyFourGenerationProgression(bus, room, assets);

        Console.WriteLine(
            "Zebetite audit passed: the untouched Mother Brain record selected all four " +
            "event-backed generations; ROM geometry, linked halves, transition gate, health " +
            "regeneration/tier animation, palette cycle, OBJ drawing, touch, mirrored shot " +
            "and normal-bomb damage, death explosions, embedded respawns, and final event " +
            "state were verified.");
        return 0;
    }

    private static void VerifyDefinitionAndPopulation(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room)
    {
        RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(bus, ZebetiteDefinition);
        if (room.State.EnemyPopulationPointer != 0xe321 ||
            ReadWord(bus, 0xa10000 | ZebetitePopulationRecord) != ZebetiteDefinition ||
            definition.Bank != 0xa6 || definition.InitializationAiPointer != 0xfb72 ||
            definition.MainAiPointer != 0xfc33 || definition.TouchAiPointer != 0xfda7 ||
            definition.ShotAiPointer != 0xfdac || definition.Health != 1000 ||
            definition.Damage != 0)
        {
            throw new InvalidDataException(
                $"Zebetite retail header/population failed: state=${room.State.Pointer:X4}, " +
                $"population=${room.State.EnemyPopulationPointer:X4}, bank=${definition.Bank:X2}, " +
                $"init/main=${definition.InitializationAiPointer:X4}/" +
                $"${definition.MainAiPointer:X4}, touch/shot=${definition.TouchAiPointer:X4}/" +
                $"${definition.ShotAiPointer:X4}, health/damage=" +
                $"{definition.Health}/{definition.Damage}.");
        }

        ushort[] expectedRecord =
        [
            ZebetiteDefinition, 0, 0, 0, 0x2000, 0, 0, 0,
        ];
        for (int word = 0; word < expectedRecord.Length; word++)
        {
            ushort actual = ReadWord(
                bus,
                0xa10000 | unchecked((ushort)(ZebetitePopulationRecord + word * 2)));
            if (actual != expectedRecord[word])
            {
                throw new InvalidDataException(
                    $"Zebetite population word {word} was ${actual:X4}, " +
                    $"expected ${expectedRecord[word]:X4}.");
            }
        }
    }

    private static void VerifyEveryGenerationInitialization(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        for (ushort generation = 0; generation < 4; generation++)
        {
            LoadedZebetites loaded = Load(bus, room, assets, generation);
            RoomEnemySlot actor = loaded.Enemies.Slots[0];
            ZebetiteEnemyState state = RequireState(loaded.Enemies, actor);
            int offset = generation * 2;
            ushort flags = ReadWord(bus, 0xa60000 | (GenerationFlagsTable + offset));
            ushort expectedRadius = ReadWord(bus, 0xa60000 | (YRadiusTable + offset));
            ushort expectedList = ReadWord(bus, 0xa60000 | (InitialInstructionTable + offset));
            ushort expectedX = ReadWord(bus, 0xa60000 | (XPositionTable + offset));
            ushort expectedY = ReadWord(bus, 0xa60000 | (UpperYPositionTable + offset));
            if (loaded.Enemies.EnemyCount != 1 || actor.EnemyDefinitionPointer != ZebetiteDefinition ||
                actor.Properties != 0xa000 || actor.PaletteIndex != 0x0400 ||
                actor.VramTilesIndex != 0x0080 || actor.Health != 1000 ||
                actor.YRadius != expectedRadius || actor.CurrentInstruction != expectedList ||
                actor.XPosition != expectedX || actor.YPosition != expectedY ||
                state.Function != ZebetiteAiFunction.SpawnLinkedHalf ||
                state.Generation != generation || state.GenerationFlags != flags ||
                state.IsSecondaryHalf)
            {
                throw new InvalidDataException(
                    $"Zebetite generation {generation} initialization failed: count=" +
                    $"{loaded.Enemies.EnemyCount}, properties=${actor.Properties:X4}, " +
                    $"position=(${actor.XPosition:X4},${actor.YPosition:X4}), radius=" +
                    $"${actor.YRadius:X4}, list=${actor.CurrentInstruction:X4}, function=" +
                    $"{state.Function}, flags=${state.GenerationFlags:X4}.");
            }

            // A live door transition lets Function 1 allocate the optional half but must
            // hold both records in Function 2. Clearing it on the next frame enters active
            // AI, selects the full-health list, increments/caps health, and advances color.
            loaded.Enemies.ElevatorDoorTransitionActive = true;
            Step(loaded, assets);
            int expectedActorCount = (flags & 0x8000) != 0 ? 2 : 1;
            if (loaded.Enemies.EnemyCount != expectedActorCount ||
                state.Function != ZebetiteAiFunction.WaitForDoorTransition)
            {
                throw new InvalidDataException(
                    $"Generation {generation} transition gate produced " +
                    $"{loaded.Enemies.EnemyCount} actors and {state.Function}.");
            }

            RoomEnemySlot? secondary = null;
            if ((flags & 0x8000) != 0)
            {
                secondary = loaded.Enemies.Slots[1];
                ZebetiteEnemyState secondaryState = RequireState(loaded.Enemies, secondary);
                if (!secondaryState.IsSecondaryHalf || secondary.Parameter2 != actor.NativeIndex ||
                    actor.Parameter2 != secondary.NativeIndex ||
                    secondaryState.Function != ZebetiteAiFunction.WaitForDoorTransition ||
                    secondaryState.Generation != generation)
                {
                    throw new InvalidDataException(
                        $"Generation {generation} linked-half contract failed: mainLink=" +
                        $"${actor.Parameter2:X4}, secondaryLink=${secondary.Parameter2:X4}, " +
                        $"secondaryFunction={secondaryState.Function}.");
                }
            }

            actor.Health = 999;
            loaded.Enemies.ElevatorDoorTransitionActive = false;
            Step(loaded, assets);
            ushort healthTable = (flags & 0x8000) != 0
                ? LowerHealthInstructionTable
                : UpperHealthInstructionTable;
            ushort fullHealthList = ReadWord(bus, 0xa60000 | healthTable);
            ushort expectedMap = ReadWord(bus, 0xa60000 | unchecked((ushort)(fullHealthList + 2)));
            if (state.Function != ZebetiteAiFunction.Active || actor.Health != 1000 ||
                actor.SpritemapPointer != expectedMap ||
                secondary is not null &&
                    RequireState(loaded.Enemies, secondary).Function != ZebetiteAiFunction.Active)
            {
                throw new InvalidDataException(
                    $"Generation {generation} activation failed: function={state.Function}, " +
                    $"health={actor.Health}, map=${actor.SpritemapPointer:X4}, " +
                    $"expected=${expectedMap:X4}.");
            }

            ushort expectedColor0 = ReadWord(bus, 0xa6fd8b);
            ushort expectedColor1 = ReadWord(bus, 0xa6fd8d);
            if (loaded.Cgram.Colors[0x0158 / 2] != expectedColor0 ||
                loaded.Cgram.Colors[0x0158 / 2 + 1] != expectedColor1)
            {
                throw new InvalidDataException(
                    $"Generation {generation} palette cycle wrote " +
                    $"${loaded.Cgram.Colors[0x0158 / 2]:X4}/" +
                    $"${loaded.Cgram.Colors[0x0158 / 2 + 1]:X4} instead of " +
                    $"${expectedColor0:X4}/${expectedColor1:X4}.");
            }

            var oam = new OamBuffer();
            oam.BeginFrame();
            loaded.Enemies.DrawLayers(oam, 0, 0, 0, 7);
            oam.FinalizeFrame();
            if (oam.LastFinalizedSpriteCount == 0)
                throw new InvalidDataException($"Generation {generation} emitted no OBJ pieces.");
        }

        // Binary values four through seven mean all four barriers have already died. Init
        // preserves the decoded event word but marks the population record deleted.
        LoadedZebetites finished = Load(bus, room, assets, generation: 4);
        if (!finished.Enemies.Slots[0].Properties.HasAny(EnemyProperties.Deleted) ||
            RequireState(finished.Enemies, finished.Enemies.Slots[0]).Generation != 4)
        {
            throw new InvalidDataException("Completed Zebetite events did not delete the room record.");
        }
    }

    private static void VerifyLinkedShotAndContact(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        ushort linkedGeneration = Enumerable.Range(0, 4)
            .Select(index => (ushort)index)
            .First(index =>
                (ReadWord(bus, 0xa60000 | (GenerationFlagsTable + index * 2)) & 0x8000) != 0);
        LoadedZebetites loaded = Load(bus, room, assets, linkedGeneration);
        Activate(loaded, assets);
        RoomEnemySlot primary = loaded.Enemies.Slots[0];
        ZebetiteEnemyState state = RequireState(loaded.Enemies, primary);
        RoomEnemySlot linked = loaded.Enemies.Slots[state.LinkedNativeIndex / RoomEnemySystem.NativeSlotSize];

        // The private shot tail runs after common vulnerability damage even for an immune
        // hit. Make the halves disagree first so equality proves the copy rather than merely
        // observing two actors that began with the same definition health.
        primary.Health = 500;
        linked.Health = 777;
        var projectiles = new SamusProjectileSystem();
        var bombs = new SamusBombProjectileSystem();
        ArmProjectile(projectiles.Slots[0], primary, type: 0x0100, damage: 20);
        int hits = loaded.Enemies.ResolveOrdinaryProjectileHits(
            bus,
            projectiles,
            bombs,
            loaded.Samus);
        if (hits != 1 || linked.Health != primary.Health ||
            linked.FlashTimer != primary.FlashTimer ||
            loaded.Enemies.LastZebetiteSoundEffect != 9)
        {
            throw new InvalidDataException(
                $"Zebetite linked shot failed: hits={hits}, health=" +
                $"{primary.Health}/{linked.Health}, flash={primary.FlashTimer}/" +
                $"{linked.FlashTimer}, sound={loaded.Enemies.LastZebetiteSoundEffect}.");
        }

        // `$A6:FDAC` uses the exact same no-death common-damage prelude for physical bombs,
        // then mirrors health and flash into the linked half. Force disagreement again so a
        // successful assertion cannot be inherited from the beam callback above.
        primary.Health = 500;
        linked.Health = 733;
        linked.FlashTimer = 0;
        bombs = new SamusBombProjectileSystem();
        ArmNormalBomb(bombs.Slots[0], primary);
        hits = loaded.Enemies.ResolveOrdinaryBombHits(
            bombs,
            new SamusProjectileSystem(),
            loaded.Samus);
        if (hits != 1 || (bombs.Slots[0].Direction & 0x0010) == 0 ||
            linked.Health != primary.Health || linked.FlashTimer != primary.FlashTimer ||
            loaded.Enemies.LastZebetiteSoundEffect != 9)
        {
            throw new InvalidDataException(
                $"Zebetite linked normal bomb failed: hits={hits}, direction=" +
                $"${bombs.Slots[0].Direction:X4}, health={primary.Health}/{linked.Health}, " +
                $"flash={primary.FlashTimer}/{linked.FlashTimer}, sound=" +
                $"{loaded.Enemies.LastZebetiteSoundEffect}.");
        }

        loaded.Samus.XPosition = primary.XPosition;
        loaded.Samus.YPosition = primary.YPosition;
        ushort samusHealth = loaded.Samus.Health;
        if (!loaded.Enemies.ResolveOrdinarySamusContact(loaded.Samus, 0) ||
            loaded.Samus.Health != samusHealth)
        {
            throw new InvalidDataException(
                $"Zero-damage Zebetite touch changed Samus from {samusHealth} to " +
                $"{loaded.Samus.Health}.");
        }
    }

    private static void VerifyFourGenerationProgression(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedZebetites loaded = Load(bus, room, assets, generation: 0);
        int expectedDeaths = 0;
        int expectedAllocatedSlots = 1;

        for (ushort generation = 0; generation < 4; generation++)
        {
            RoomEnemySlot primary = loaded.Enemies.Slots
                .First(slot => slot.EnemyDefinitionPointer == ZebetiteDefinition &&
                    slot.Parameter1 == 0 &&
                    !slot.Properties.HasAny(EnemyProperties.Deleted));
            loaded.Enemies.ElevatorDoorTransitionActive = false;
            Step(loaded, assets);
            ZebetiteEnemyState state = RequireState(loaded.Enemies, primary);
            if (state.Function != ZebetiteAiFunction.Active || state.Generation != generation)
            {
                throw new InvalidDataException(
                    $"Progression generation {generation} entered {state.Function}/" +
                    $"{state.Generation}.");
            }

            bool hasLinkedHalf = (state.GenerationFlags & 0x8000) != 0;
            RoomEnemySlot? linked = hasLinkedHalf
                ? loaded.Enemies.Slots[state.LinkedNativeIndex / RoomEnemySystem.NativeSlotSize]
                : null;
            expectedAllocatedSlots += hasLinkedHalf ? 1 : 0;

            // A lethal private shot would set both linked health words to zero. Assign that
            // post-callback state directly here so every generation can be advanced even if
            // its retail vulnerability happens to reject the test projectile family.
            primary.Health = 0;
            if (linked is not null)
                linked.Health = 0;
            Step(loaded, assets);
            expectedDeaths += hasLinkedHalf ? 2 : 1;
            if (loaded.Enemies.EnemiesKilled != expectedDeaths ||
                loaded.Enemies.EnemyProjectiles.Count(projectile =>
                    projectile.Kind == RoomEnemyProjectileKind.EnemyDeathExplosion) <
                    Math.Min(expectedDeaths, 18))
            {
                throw new InvalidDataException(
                    $"Generation {generation} death produced kills=" +
                    $"{loaded.Enemies.EnemiesKilled}, expected {expectedDeaths}.");
            }

            ushort next = unchecked((ushort)(generation + 1));
            if (loaded.System.HasEvent(3) != ((next & 1) != 0) ||
                loaded.System.HasEvent(4) != ((next & 2) != 0) ||
                loaded.System.HasEvent(5) != ((next & 4) != 0))
            {
                throw new InvalidDataException(
                    $"Generation {generation} published events " +
                    $"{loaded.System.HasEvent(5)}/{loaded.System.HasEvent(4)}/" +
                    $"{loaded.System.HasEvent(3)} instead of binary {next}.");
            }

            if (next < 4)
            {
                expectedAllocatedSlots++;
                RoomEnemySlot spawned = loaded.Enemies.Slots[expectedAllocatedSlots - 1];
                ZebetiteEnemyState spawnedState = RequireState(loaded.Enemies, spawned);
                if (spawned.EnemyDefinitionPointer != ZebetiteDefinition ||
                    spawnedState.Generation != next || spawnedState.IsSecondaryHalf)
                {
                    throw new InvalidDataException(
                        $"Generation {generation} spawned definition " +
                        $"${spawned.EnemyDefinitionPointer:X4}/generation " +
                        $"{spawnedState.Generation} in slot {spawned.SlotIndex}.");
                }
            }
        }

        if (loaded.Enemies.Slots.Any(slot =>
                slot.EnemyDefinitionPointer == ZebetiteDefinition &&
                !slot.Properties.HasAny(EnemyProperties.Deleted)))
        {
            throw new InvalidDataException("A live Zebetite remained after generation four.");
        }
    }

    private static LoadedZebetites Load(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        ushort generation)
    {
        var prefixBus = new PopulationPrefixAddressSpace(
            bus,
            ZebetitePopulationRecord,
            retainedRecordCount: 1,
            deathQuota: 0);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var system = new Bank80SystemState();
        system.SetOrClearEvent(3, (generation & 1) != 0);
        system.SetOrClearEvent(4, (generation & 2) != 0);
        system.SetOrClearEvent(5, (generation & 4) != 0);
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusPoseIds.FacingRightNormalPose,
        };
        samus.RefreshCollisionRadii(prefixBus);
        samus.InitializeAnimation(prefixBus);
        var enemies = new RoomEnemySystem();
        enemies.Load(
            prefixBus,
            ZebetitePopulationRecord,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            system.NextRandom,
            system.SetRandomNumber,
            readRandomNumber: () => system.RandomNumber,
            level: assets.LevelData,
            samus: samus,
            hasEvent: system.HasEvent,
            setEvent: system.SetEvent,
            clearEvent: system.ClearEvent);
        return new LoadedZebetites(enemies, samus, system, cgram);
    }

    private static void Activate(LoadedZebetites loaded, CartridgeRoomAssets assets)
    {
        loaded.Enemies.ElevatorDoorTransitionActive = true;
        Step(loaded, assets);
        loaded.Enemies.ElevatorDoorTransitionActive = false;
        Step(loaded, assets);
    }

    private static void Step(LoadedZebetites loaded, CartridgeRoomAssets assets)
    {
        RoomEnemySlot focus = loaded.Enemies.Slots.First(slot =>
            slot.EnemyDefinitionPointer == ZebetiteDefinition);
        ushort cameraX = focus.XPosition > 0x0080
            ? unchecked((ushort)(focus.XPosition - 0x0080))
            : (ushort)0;
        ushort cameraY = focus.YPosition > 0x0080
            ? unchecked((ushort)(focus.YPosition - 0x0080))
            : (ushort)0;
        loaded.Enemies.StepFrame(
            cameraX,
            cameraY,
            false,
            loaded.Samus,
            level: assets.LevelData);
    }

    private static void ArmProjectile(
        SamusProjectileSlot projectile,
        RoomEnemySlot target,
        ushort type,
        ushort damage)
    {
        projectile.ClearFields();
        projectile.Type = type;
        projectile.Damage = damage;
        projectile.Direction = 2;
        projectile.XPosition = target.XPosition;
        projectile.YPosition = target.YPosition;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    private static void ArmNormalBomb(
        SamusBombProjectileSlot bomb,
        RoomEnemySlot target)
    {
        bomb.ClearFields();
        bomb.Type = SamusBombProjectileSystem.NormalBombType;
        bomb.Damage = 20;
        bomb.Direction = (ushort)SamusProjectileDirection.Right;
        bomb.XPosition = target.XPosition;
        bomb.YPosition = target.YPosition;
        bomb.XRadius = 16;
        bomb.YRadius = 16;
        bomb.BombTimer = 0;
        bomb.InstructionPointer = 0xa06b;
        bomb.InstructionTimer = 1;
    }

    private static ZebetiteEnemyState RequireState(RoomEnemySystem enemies, RoomEnemySlot actor) =>
        enemies.ZebetiteStates[actor.SlotIndex] ?? throw new InvalidDataException(
            $"Zebetite slot {actor.SlotIndex} has no typed state.");

    private static ushort ReadWord(SuperMetroidAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private readonly record struct LoadedZebetites(
        RoomEnemySystem Enemies,
        SamusState Samus,
        Bank80SystemState System,
        SnesCgram Cgram);
}

internal static class ZebetiteAuditEventExtensions
{
    public static void SetOrClearEvent(
        this Bank80SystemState system,
        int eventNumber,
        bool set)
    {
        if (set)
            system.SetEvent(eventNumber);
        else
            system.ClearEvent(eventNumber);
    }
}
