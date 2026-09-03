using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>
/// ROM-backed audit for all four Pipe Bug headers. The population lists below are the exact
/// named retail populations discovered from the checked-in symbol boundaries, but every
/// record, header, instruction, speed, spritemap, palette, and collision reaction is read
/// again from the user's cartridge on each run.
/// </summary>
internal static class PipeBugAudit
{
    private const ushort BrinstarDefinition = 0xf193;
    private const ushort StrongBrinstarDefinition = 0xf1d3;
    private const ushort NorfairDefinition = 0xf213;
    private const ushort YellowDefinition = 0xf253;

    private static readonly ushort[] BrinstarPopulations =
        [0x8684, 0x902e, 0x953e, 0x9a2d, 0x9a40, 0x9fa4, 0xcf90];
    private static readonly ushort[] StrongBrinstarPopulations =
        [0x89f2, 0x9778, 0x9e2f, 0xaa8d, 0xb769, 0xd3aa, 0xd53f];
    private static readonly ushort[] NorfairPopulations =
        [0xa133, 0xa2df, 0xa7bb, 0xb32c, 0xb4d1, 0xb912, 0xbbd7];
    private static readonly ushort[] YellowPopulations = [0x9452, 0x9cb9];

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        VerifyHeadersAndEveryRetailRecord(bus);

        CartridgeRoomHeader brinstarRoom = CartridgeRoomHeader.Load(bus, 0x9b5b);
        CartridgeRoomAssets brinstarAssets = CartridgeRoomAssets.Load(bus, brinstarRoom);
        LoadedPipeBugs brinstar = LoadPrefix(
            bus,
            brinstarRoom,
            brinstarAssets,
            FindFirstPopulationRecord(bus, brinstarRoom.State.EnemyPopulationPointer, BrinstarDefinition),
            retainedRecordCount: 1);
        VerifyBrinstarCycle(brinstar, brinstarAssets, strong: false);

        CartridgeRoomHeader strongRoom = CartridgeRoomHeader.Load(bus, 0x965b);
        CartridgeRoomAssets strongAssets = CartridgeRoomAssets.Load(bus, strongRoom);
        LoadedPipeBugs strong = LoadPrefix(
            bus,
            strongRoom,
            strongAssets,
            FindFirstPopulationRecord(bus, strongRoom.State.EnemyPopulationPointer, StrongBrinstarDefinition),
            retainedRecordCount: 1);
        VerifyBrinstarCycle(strong, strongAssets, strong: true);

        CartridgeRoomHeader norfairRoom = CartridgeRoomHeader.Load(bus, 0xb051);
        CartridgeRoomAssets norfairAssets = CartridgeRoomAssets.Load(bus, norfairRoom);
        LoadedPipeBugs norfair = LoadPrefix(
            bus,
            norfairRoom,
            norfairAssets,
            FindFirstPopulationRecord(bus, norfairRoom.State.EnemyPopulationPointer, NorfairDefinition),
            retainedRecordCount: 5);
        VerifyNorfairFormation(norfair, norfairAssets);

        CartridgeRoomHeader yellowRoom = CartridgeRoomHeader.Load(bus, 0x9e52);
        CartridgeRoomAssets yellowAssets = CartridgeRoomAssets.Load(bus, yellowRoom);
        LoadedPipeBugs yellow = LoadPrefix(
            bus,
            yellowRoom,
            yellowAssets,
            FindFirstPopulationRecord(bus, yellowRoom.State.EnemyPopulationPointer, YellowDefinition),
            retainedRecordCount: 5);
        VerifyYellowCycles(yellow, yellowAssets);
        VerifyOrdinaryCombat(bus, brinstarRoom, brinstarAssets);

        Console.WriteLine(
            "Pipe Bug audit passed: all 77 records in 23 retail populations and all four " +
            "headers matched; normal/strong emergence and animation, five-member Norfair " +
            "stagger/flight and post-member-death physical aliasing, both yellow directions " +
            "and quadratic arcs, OBJ drawing, contact damage, beam damage/death, freeze, " +
            "and power-bomb damage used cartridge data.");
        return 0;
    }

    private static void VerifyHeadersAndEveryRetailRecord(ISnesAddressSpace bus)
    {
        VerifyHeader(bus, BrinstarDefinition, 9, 6, 0x883b, 0x887a);
        VerifyHeader(bus, StrongBrinstarDefinition, 30, 20, 0x883b, 0x887a);
        VerifyHeader(bus, NorfairDefinition, 20, 16, 0x8b61, 0x8b9e);
        VerifyHeader(bus, YellowDefinition, 10, 10, 0x8f4c, 0x8fae);

        VerifyPopulationOccurrences(bus, BrinstarPopulations, BrinstarDefinition, 12);
        VerifyPopulationOccurrences(bus, StrongBrinstarPopulations, StrongBrinstarDefinition, 23);
        VerifyPopulationOccurrences(bus, NorfairPopulations, NorfairDefinition, 35);
        VerifyPopulationOccurrences(bus, YellowPopulations, YellowDefinition, 7);
    }

    private static void VerifyHeader(
        ISnesAddressSpace bus,
        ushort pointer,
        ushort health,
        ushort damage,
        ushort initialization,
        ushort main)
    {
        RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(bus, pointer);
        if (definition.Bank != 0xb3 || definition.Health != health ||
            definition.Damage != damage || definition.InitializationAiPointer != initialization ||
            definition.MainAiPointer != main || definition.TouchAiPointer != 0x8023 ||
            definition.ShotAiPointer != 0x802d || definition.GrappleAiPointer != 0x800a ||
            definition.FrozenAiPointer != 0x8041 || definition.VulnerabilityPointer == 0)
        {
            throw new InvalidDataException(
                $"Pipe Bug header ${pointer:X4} mismatch: bank=${definition.Bank:X2}, " +
                $"health/damage={definition.Health}/{definition.Damage}, init/main=" +
                $"${definition.InitializationAiPointer:X4}/${definition.MainAiPointer:X4}, " +
                $"touch/shot=${definition.TouchAiPointer:X4}/${definition.ShotAiPointer:X4}, " +
                $"grapple/frozen=${definition.GrappleAiPointer:X4}/" +
                $"${definition.FrozenAiPointer:X4}, vulnerability/map=" +
                $"${definition.VulnerabilityPointer:X4}/${definition.InitialSpritemapPointer:X4}.");
        }
    }

    private static void VerifyPopulationOccurrences(
        ISnesAddressSpace bus,
        IEnumerable<ushort> populations,
        ushort definition,
        int expectedRecords)
    {
        int records = 0;
        foreach (ushort population in populations)
        {
            bool found = false;
            for (ushort cursor = population, index = 0;
                index < RoomEnemySystem.MaximumEnemyCount;
                cursor = unchecked((ushort)(cursor + 16)), index++)
            {
                ushort candidate = ReadWord(bus, 0xa10000 | cursor);
                if (candidate == 0xffff)
                    break;
                if (candidate == definition)
                {
                    found = true;
                    records++;
                }
            }
            if (!found)
                throw new InvalidDataException(
                    $"Named population $A1:{population:X4} has no Pipe Bug ${definition:X4}.");
        }
        if (records != expectedRecords)
        {
            throw new InvalidDataException(
                $"Pipe Bug ${definition:X4} has {records} records, expected {expectedRecords}.");
        }
    }

    private static void VerifyBrinstarCycle(
        LoadedPipeBugs loaded,
        CartridgeRoomAssets assets,
        bool strong)
    {
        RoomEnemySlot actor = loaded.Enemies.Slots[0];
        PipeBugEnemyState state = RequireState(loaded.Enemies, actor);
        ushort expectedDefinition = strong ? StrongBrinstarDefinition : BrinstarDefinition;
        ushort expectedHealth = strong ? (ushort)30 : (ushort)9;
        ushort expectedInstruction = strong ? (ushort)0x8a1d : (ushort)0x87ab;
        if (actor.EnemyDefinitionPointer != expectedDefinition || actor.Health != expectedHealth ||
            actor.CurrentInstruction != expectedInstruction ||
            state.Function != PipeBugEnemyFunction.BrinstarWaitUntilOnScreen ||
            state.SpawnX != actor.XPosition || state.SpawnY != actor.YPosition ||
            state.EmergenceTopY != unchecked((ushort)(actor.YPosition - 16)))
        {
            throw new InvalidDataException(
                $"Brinstar Pipe Bug initialization mismatch for ${expectedDefinition:X4}.");
        }

        ushort cameraX = CenterCameraX(actor);
        ushort cameraY = CenterCameraY(actor);
        // Despite its historical name, CheckIfEnemyIsOnScreen returns zero on-screen. One
        // centered frame must therefore arm the waiting state; the next consumes Samus's
        // exact 64x96 activation box above the pipe.
        Step(loaded, assets, cameraX, cameraY);
        loaded.Samus.XPosition = unchecked((ushort)(actor.XPosition + 32));
        loaded.Samus.YPosition = unchecked((ushort)(actor.YPosition - 48));
        Step(loaded, assets, cameraX, cameraY);
        if (state.Function != PipeBugEnemyFunction.BrinstarEmerge ||
            actor.Properties.HasAny(EnemyProperties.Invisible))
        {
            throw new InvalidDataException("Brinstar Pipe Bug did not emerge for Samus.");
        }
        VerifyDrawing(loaded.Enemies, actor, cameraX, cameraY);

        ushort spawnX = actor.XPosition;
        ushort spawnY = actor.YPosition;
        var maps = new HashSet<ushort>();
        bool flew = false;
        bool reset = false;
        for (int frame = 0; frame < 220; frame++)
        {
            Step(loaded, assets, cameraX, cameraY);
            maps.Add(actor.SpritemapPointer);
            flew |= state.Function == PipeBugEnemyFunction.BrinstarFlyHorizontally &&
                actor.XPosition != spawnX;
            reset |= state.Function == PipeBugEnemyFunction.BrinstarRespawnDelay &&
                actor.XPosition == state.SpawnX && actor.YPosition == state.SpawnY;
        }
        if (!flew || !reset || maps.Count < 2 || spawnY == actor.YPosition && !reset)
        {
            throw new InvalidDataException(
                $"Brinstar Pipe Bug cycle failed: flew={flew}, reset={reset}, maps={maps.Count}.");
        }
    }

    private static void VerifyNorfairFormation(
        LoadedPipeBugs loaded,
        CartridgeRoomAssets assets)
    {
        RoomEnemySlot leader = loaded.Enemies.Slots[0];
        if (loaded.Enemies.EnemyCount != 5 ||
            loaded.Enemies.Slots.Take(5).Any(slot => slot.EnemyDefinitionPointer != NorfairDefinition) ||
            loaded.Enemies.Slots.Take(5).Any(slot =>
                RequireState(loaded.Enemies, slot).Function !=
                    PipeBugEnemyFunction.NorfairWaitForFormation))
        {
            throw new InvalidDataException("Norfair Pipe Bug formation did not load as five dormant members.");
        }

        loaded.Samus.XPosition = leader.XPosition;
        loaded.Samus.YPosition = unchecked((ushort)(leader.YPosition - 48));
        ushort cameraX = CenterCameraX(leader);
        ushort cameraY = CenterCameraY(leader);
        Step(loaded, assets, cameraX, cameraY); // leader verifies all five and arms itself
        Step(loaded, assets, cameraX, cameraY); // leader launches all five
        VerifyDrawing(loaded.Enemies, leader, cameraX, cameraY);

        bool sawRise = false;
        bool sawDistinctStaggers = false;
        bool sawHorizontalFlight = false;
        var maps = new HashSet<ushort>();
        ushort startX = leader.XPosition;
        for (int frame = 0; frame < 220; frame++)
        {
            Step(loaded, assets, cameraX, cameraY);
            PipeBugEnemyFunction[] functions = loaded.Enemies.Slots.Take(5)
                .Select(slot => RequireState(loaded.Enemies, slot).Function)
                .ToArray();
            sawRise |= functions.Contains(PipeBugEnemyFunction.NorfairRise);
            sawDistinctStaggers |= functions.Distinct().Count() >= 3;
            sawHorizontalFlight |= functions.Any(function => function is
                    PipeBugEnemyFunction.NorfairFlyLeft or PipeBugEnemyFunction.NorfairFlyRight) &&
                loaded.Enemies.Slots.Take(5).Any(slot => slot.XPosition != startX);
            foreach (RoomEnemySlot member in loaded.Enemies.Slots.Take(5))
                maps.Add(member.SpritemapPointer);
        }
        if (!sawRise || !sawDistinctStaggers || !sawHorizontalFlight || maps.Count < 2)
        {
            throw new InvalidDataException(
                $"Norfair formation failed: rise={sawRise}, stagger={sawDistinctStaggers}, " +
                $"flight={sawHorizontalFlight}, maps={maps.Count}.");
        }

        VerifyNorfairFormationAfterMemberDeath(loaded, assets);
    }

    /// <summary>
    /// Generic death clears an enemy definition on the next processing scan, but the retail
    /// leader still writes rise instructions/functions through all five physical formation
    /// records. This is an observable raw-WRAM alias, not a malformed-room case.
    /// </summary>
    private static void VerifyNorfairFormationAfterMemberDeath(
        LoadedPipeBugs source,
        CartridgeRoomAssets assets)
    {
        LoadedPipeBugs loaded = LoadPrefix(
            source.Bus,
            source.Room,
            assets,
            source.PopulationPointer,
            retainedRecordCount: 5);
        RoomEnemySlot leader = loaded.Enemies.Slots[0];
        RoomEnemySlot victim = loaded.Enemies.Slots[2];
        ushort cameraX = CenterCameraX(leader);
        ushort cameraY = CenterCameraY(leader);
        // DetermineWhichEnemiesToProcess publishes the live collision-index array at the
        // start of an enemy frame. Keep Samus at the fixture's harmless default while the
        // leader performs its ordinary all-five dormant check.
        Step(loaded, assets, cameraX, cameraY);
        ushort[] savedProperties = loaded.Enemies.Slots.Take(5)
            .Select(slot => slot.Properties)
            .ToArray();
        for (int slotIndex = 0; slotIndex < 5; slotIndex++)
        {
            if (slotIndex != victim.SlotIndex)
            {
                loaded.Enemies.Slots[slotIndex].Properties =
                    loaded.Enemies.Slots[slotIndex].Properties.With(
                        EnemyProperties.IgnoreSamusCollision);
            }
        }

        var projectiles = new SamusProjectileSystem();
        ArmProjectile(projectiles.Slots[0], victim, type: 0, damage: victim.Health);
        int hits = loaded.Enemies.ResolveOrdinaryProjectileHits(
            source.Bus,
            projectiles,
            new SamusBombProjectileSystem(),
            loaded.Samus);
        for (int slotIndex = 0; slotIndex < 5; slotIndex++)
            loaded.Enemies.Slots[slotIndex].Properties = savedProperties[slotIndex];
        // Restore only the victim's native deletion bit after restoring collision isolation.
        victim.Properties = victim.Properties.With(EnemyProperties.Deleted);
        if (hits != 1 || victim.Health != 0 || loaded.Enemies.EnemiesKilled != 1)
        {
            throw new InvalidDataException(
                $"Norfair formation member death setup failed: hits={hits}, " +
                $"health={victim.Health}, killed={loaded.Enemies.EnemiesKilled}.");
        }

        loaded.Samus.XPosition = leader.XPosition;
        loaded.Samus.YPosition = unchecked((ushort)(leader.YPosition - 48));
        Step(loaded, assets, cameraX, cameraY); // clear victim definition; raw-write all five

        PipeBugEnemyState victimState = RequireState(loaded.Enemies, victim);
        if (victim.EnemyDefinitionPointer != 0 ||
            victimState.DefinitionAtInitialization != NorfairDefinition ||
            victimState.Function != PipeBugEnemyFunction.NorfairRise ||
            victim.CurrentInstruction != 0x8b21 ||
            RequireState(loaded.Enemies, leader).Function != PipeBugEnemyFunction.NorfairRise)
        {
            throw new InvalidDataException(
                $"Norfair leader lost its physical alias after member death: victim=" +
                $"${victim.EnemyDefinitionPointer:X4}/owner " +
                $"${victimState.DefinitionAtInitialization:X4}, function=" +
                $"$B3:{(ushort)victimState.Function:X4}, instruction=" +
                $"$B3:{victim.CurrentInstruction:X4}, leader=" +
                $"$B3:{(ushort)RequireState(loaded.Enemies, leader).Function:X4}.");
        }
    }

    private static void VerifyYellowCycles(
        LoadedPipeBugs loaded,
        CartridgeRoomAssets assets)
    {
        if (loaded.Enemies.EnemyCount != 5)
            throw new InvalidDataException("Yellow Pipe Bug prefix did not retain five actors.");

        RoomEnemySlot left = loaded.Enemies.Slots.First(slot => slot.Parameter1 != 0);
        RoomEnemySlot right = loaded.Enemies.Slots.First(slot => slot.Parameter1 == 0);
        VerifyYellowDirection(loaded, assets, left, movingLeft: true);

        // Reset the whole authored prefix before testing the opposite direction so the first
        // flight cannot make unrelated off-screen members influence scheduler timing.
        LoadedPipeBugs reloaded = LoadPrefix(
            loaded.Bus,
            loaded.Room,
            assets,
            loaded.PopulationPointer,
            retainedRecordCount: 5);
        right = reloaded.Enemies.Slots.First(slot => slot.Parameter1 == 0);
        VerifyYellowDirection(reloaded, assets, right, movingLeft: false);
    }

    private static void VerifyYellowDirection(
        LoadedPipeBugs loaded,
        CartridgeRoomAssets assets,
        RoomEnemySlot actor,
        bool movingLeft)
    {
        PipeBugEnemyState state = RequireState(loaded.Enemies, actor);
        loaded.Samus.XPosition = unchecked((ushort)(actor.XPosition + (movingLeft ? -64 : 64)));
        loaded.Samus.YPosition = actor.YPosition;
        ushort cameraX = CenterCameraX(actor);
        ushort cameraY = CenterCameraY(actor);
        ushort startX = actor.XPosition;
        ushort startY = actor.YPosition;
        bool sawDelay = false;
        bool sawStraight = false;
        bool sawArc = false;
        bool sawBothArcPhases = false;
        bool sawCompletedArc = false;
        var maps = new HashSet<ushort>();
        for (int frame = 0; frame < 260; frame++)
        {
            // Follow the actor as a real scrolling room would. A fixed audit camera would
            // deliberately invoke the ROM's off-screen despawn before the long quadratic
            // arc could finish, testing reset behavior instead of the complete trajectory.
            cameraX = CenterCameraX(actor);
            cameraY = CenterCameraY(actor);
            Step(loaded, assets, cameraX, cameraY);
            maps.Add(actor.SpritemapPointer);
            sawDelay |= state.Function == PipeBugEnemyFunction.YellowEmergenceDelay;
            sawStraight |= state.Function == (movingLeft
                ? PipeBugEnemyFunction.YellowFlyLeft
                : PipeBugEnemyFunction.YellowFlyRight);
            sawArc |= state.Function == (movingLeft
                ? PipeBugEnemyFunction.YellowArcLeft
                : PipeBugEnemyFunction.YellowArcRight);
            sawBothArcPhases |= sawArc && state.ArcPhase == 0;
            sawCompletedArc |= state.ArcCompleted;
            if (state.Function == PipeBugEnemyFunction.YellowWaitForSamus && frame > 32)
                break;
        }
        bool movedCorrectly = movingLeft
            ? unchecked((short)(actor.XPosition - startX)) < 0 || state.SpawnX == actor.XPosition
            : unchecked((short)(actor.XPosition - startX)) > 0 || state.SpawnX == actor.XPosition;
        if (!sawDelay || !sawStraight || !sawArc || !sawBothArcPhases || !sawCompletedArc ||
            !movedCorrectly || maps.Count < 2 || state.SpawnY != startY)
        {
            throw new InvalidDataException(
                $"Yellow Pipe Bug {(movingLeft ? "left" : "right")} cycle failed: " +
                $"delay={sawDelay}, straight={sawStraight}, arc={sawArc}, phases=" +
                $"{sawBothArcPhases}, complete={sawCompletedArc}, maps={maps.Count}.");
        }
    }

    private static void VerifyOrdinaryCombat(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        ushort population = FindFirstPopulationRecord(
            bus, room.State.EnemyPopulationPointer, BrinstarDefinition);
        LoadedPipeBugs contact = LoadPrefix(bus, room, assets, population, 1);
        RoomEnemySlot actor = contact.Enemies.Slots[0];
        ActivateBrinstarPipeBug(contact, assets, actor);
        contact.Samus.XPosition = actor.XPosition;
        contact.Samus.YPosition = actor.YPosition;
        ushort health = contact.Samus.Health;
        if (!contact.Enemies.ResolveOrdinarySamusContact(contact.Samus, 0) ||
            contact.Samus.Health != health - actor.Definition.Damage)
        {
            throw new InvalidDataException("Pipe Bug contact did not use its header damage.");
        }

        LoadedPipeBugs shot = LoadPrefix(bus, room, assets, population, 1);
        actor = shot.Enemies.Slots[0];
        ActivateBrinstarPipeBug(shot, assets, actor);
        var projectiles = new SamusProjectileSystem();
        var bombs = new SamusBombProjectileSystem();
        ArmProjectile(projectiles.Slots[0], actor, type: 0, damage: 20);
        int hits = shot.Enemies.ResolveOrdinaryProjectileHits(bus, projectiles, bombs, shot.Samus);
        if (hits != 1 || actor.Health != 0 ||
            !actor.Properties.HasAny(EnemyProperties.Deleted) || shot.Enemies.EnemiesKilled != 1)
        {
            throw new InvalidDataException("Pipe Bug common beam damage/death failed.");
        }

        LoadedPipeBugs frozen = LoadPrefix(bus, room, assets, population, 1);
        actor = frozen.Enemies.Slots[0];
        ActivateBrinstarPipeBug(frozen, assets, actor);
        int vulnerability = 0xb40000 | actor.Definition.VulnerabilityPointer;
        int iceType = Enumerable.Range(0, 12)
            .FirstOrDefault(index => bus.ReadByte(vulnerability + index) == 0xff, -1);
        if (iceType >= 0)
        {
            ArmProjectile(projectiles.Slots[0], actor, unchecked((ushort)iceType), damage: 1);
            frozen.Enemies.ResolveOrdinaryProjectileHits(bus, projectiles, bombs, frozen.Samus);
            if (actor.FrozenTimer == 0 || (actor.AiHandlerBits & 4) == 0)
                throw new InvalidDataException("Pipe Bug freeze reaction did not select common frozen AI.");
        }

        LoadedPipeBugs powerBomb = LoadPrefix(bus, room, assets, population, 1);
        actor = powerBomb.Enemies.Slots[0];
        ActivateBrinstarPipeBug(powerBomb, assets, actor);
        if (powerBomb.Enemies.ResolveOrdinaryPowerBombHits(
                bus, actor.XPosition, actor.YPosition, explosionRadius: 32) != 1 ||
            actor.Health != 0 || !actor.Properties.HasAny(EnemyProperties.Deleted))
        {
            throw new InvalidDataException("Pipe Bug common power-bomb damage/death failed.");
        }
    }

    private static void ActivateBrinstarPipeBug(
        LoadedPipeBugs loaded,
        CartridgeRoomAssets assets,
        RoomEnemySlot actor)
    {
        Step(loaded, assets, CenterCameraX(actor), CenterCameraY(actor));
        loaded.Samus.XPosition = unchecked((ushort)(actor.XPosition + 32));
        loaded.Samus.YPosition = unchecked((ushort)(actor.YPosition - 48));
        Step(loaded, assets, CenterCameraX(actor), CenterCameraY(actor));
        if (actor.Properties.HasAny(EnemyProperties.Invisible))
            throw new InvalidDataException("Combat fixture could not activate its Brinstar Pipe Bug.");
    }

    private static LoadedPipeBugs LoadPrefix(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        ushort populationPointer,
        int retainedRecordCount)
    {
        var prefixBus = new PopulationPrefixAddressSpace(
            bus, populationPointer, retainedRecordCount, deathQuota: 0);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState(0x4567);
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
            populationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            random.NextRandom,
            random.SetRandomNumber,
            readRandomNumber: () => random.RandomNumber,
            level: assets.LevelData,
            samus: samus);
        return new LoadedPipeBugs(enemies, samus, bus, room, populationPointer);
    }

    private static void Step(
        LoadedPipeBugs loaded,
        CartridgeRoomAssets assets,
        ushort cameraX,
        ushort cameraY) =>
        loaded.Enemies.StepFrame(
            cameraX,
            cameraY,
            false,
            loaded.Samus,
            level: assets.LevelData);

    private static void VerifyDrawing(
        RoomEnemySystem enemies,
        RoomEnemySlot actor,
        ushort cameraX,
        ushort cameraY)
    {
        var oam = new OamBuffer();
        oam.BeginFrame();
        enemies.DrawLayers(oam, cameraX, cameraY, 0, 7);
        oam.FinalizeFrame();
        if (actor.SpritemapPointer is 0 or 0x8000 || oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("Pipe Bug emitted no ROM-authored OBJ pieces.");
    }

    private static ushort CenterCameraX(RoomEnemySlot actor) =>
        actor.XPosition >= 128 ? unchecked((ushort)(actor.XPosition - 128)) : (ushort)0;

    private static ushort CenterCameraY(RoomEnemySlot actor) =>
        actor.YPosition >= 112 ? unchecked((ushort)(actor.YPosition - 112)) : (ushort)0;

    private static ushort FindFirstPopulationRecord(
        ISnesAddressSpace bus,
        ushort populationPointer,
        ushort definition)
    {
        for (ushort cursor = populationPointer, index = 0;
            index < RoomEnemySystem.MaximumEnemyCount;
            cursor = unchecked((ushort)(cursor + 16)), index++)
        {
            ushort candidate = ReadWord(bus, 0xa10000 | cursor);
            if (candidate == definition)
                return cursor;
            if (candidate == 0xffff)
                break;
        }
        throw new InvalidDataException(
            $"Population $A1:{populationPointer:X4} contains no Pipe Bug ${definition:X4}.");
    }

    private static PipeBugEnemyState RequireState(
        RoomEnemySystem enemies,
        RoomEnemySlot actor) =>
        enemies.PipeBugStates[actor.SlotIndex] ?? throw new InvalidDataException(
            $"Pipe Bug slot {actor.SlotIndex} has no typed state.");

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private static void ArmProjectile(
        SamusProjectileSlot projectile,
        RoomEnemySlot target,
        ushort type,
        ushort damage)
    {
        projectile.ClearFields();
        projectile.Type = type;
        projectile.Damage = damage;
        projectile.Direction = (ushort)SamusProjectileDirection.Right;
        projectile.XPosition = target.XPosition;
        projectile.YPosition = target.YPosition;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    private readonly record struct LoadedPipeBugs(
        RoomEnemySystem Enemies,
        SamusState Samus,
        SuperMetroidAddressSpace Bus,
        CartridgeRoomHeader Room,
        ushort PopulationPointer);
}
