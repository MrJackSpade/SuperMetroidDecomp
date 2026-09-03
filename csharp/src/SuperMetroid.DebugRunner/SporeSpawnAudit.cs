using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// End-to-end ROM-backed audit for Spore Spawn. The room, enemy/projectile definitions,
/// instruction streams, extended hitboxes, palettes, and PLM draw lists all come from the
/// user's cartridge; the audit supplies only deterministic RNG, Samus, and weapon stimuli.
/// </summary>
internal static class SporeSpawnAudit
{
    private const ushort RoomPointer = 0x9dc7;
    private const ushort PopulationPointer = 0xa0fd;
    private const ushort DefinitionPointer = RoomEnemySystem.SporeSpawnDefinition;
    private const ushort CrumbleCeilingHeader = 0xb78f;
    private const ushort ClearCeilingHeader = 0xb793;
    private const ushort CameraX = 0;
    private const ushort CameraY = 464;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, RoomPointer);
        VerifyRetailRoomAndHeader(bus, room);
        VerifyLiveEncounterCombatAndDeath(bus, room);
        VerifyAlreadyDefeatedCeiling(bus, room);
        VerifyRuntimeAlreadyDefeatedIntegration(bus);

        Console.WriteLine(
            "Spore Spawn audit passed: retail room/header, stalk interpolation, ceiling " +
            "emitters, spores, ROM instruction cadence, extended hitboxes, movement and " +
            "damage reaction, death effects/palettes/drops, boss bit, and both ceiling PLMs.");
        return 0;
    }

    private static void VerifyRetailRoomAndHeader(
        ISnesAddressSpace bus,
        CartridgeRoomHeader room)
    {
        if (room.Pointer != RoomPointer || room.AreaIndex != AreaId.Brinstar ||
            room.WidthInScreens != 1 || room.HeightInScreens != 3 ||
            room.State.EnemyPopulationPointer != PopulationPointer)
        {
            throw new InvalidDataException(
                $"Spore Spawn room mismatch: room/state=${room.Pointer:X4}/" +
                $"${room.State.Pointer:X4}, area={room.AreaIndex}, size=" +
                $"{room.WidthInScreens}x{room.HeightInScreens}, population=" +
                $"${room.State.EnemyPopulationPointer:X4}.");
        }

        ushort[] expectedPopulation =
            [DefinitionPointer, 0x0080, 0x0270, 0, 0x2800, 0x0004, 0, 0];
        for (int word = 0; word < expectedPopulation.Length; word++)
        {
            ushort actual = ReadWord(bus, 0xa10000 | (PopulationPointer + word * 2));
            if (actual != expectedPopulation[word])
            {
                throw new InvalidDataException(
                    $"Spore Spawn population word {word} is ${actual:X4}, expected " +
                    $"${expectedPopulation[word]:X4}.");
            }
        }
        if (ReadWord(bus, 0xa10000 | (PopulationPointer + 16)) != 0xffff)
            throw new InvalidDataException("Spore Spawn population did not end after one body.");

        RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(bus, DefinitionPointer);
        if (definition.Bank != 0xa5 || definition.Health != 960 || definition.Damage != 12 ||
            definition.InitializationAiPointer != 0xea2a ||
            definition.MainAiPointer != 0xeb13 ||
            definition.TouchAiPointer != 0xedec ||
            definition.ShotAiPointer != 0xed5a ||
            definition.PowerBombReactionPointer != 0xedf2)
        {
            throw new InvalidDataException(
                $"Spore Spawn header mismatch: bank=${definition.Bank:X2}, hp/damage=" +
                $"{definition.Health}/{definition.Damage}, init/main=" +
                $"${definition.InitializationAiPointer:X4}/${definition.MainAiPointer:X4}, " +
                $"touch/shot/pb=${definition.TouchAiPointer:X4}/" +
                $"${definition.ShotAiPointer:X4}/" +
                $"${definition.PowerBombReactionPointer:X4}.");
        }

        VerifyProjectileDefinition(bus, 0xde6c, 0xdca3, 0xdd44, 0xdc2e, 8, 0x2000);
        VerifyProjectileDefinition(bus, 0xde7a, 0xdc8d, 0xdcee, 0xdc1e, 2, 0x8004);
        VerifyProjectileDefinition(bus, 0xde88, 0xdcd4, 0xdd46, 0xdc00, 2, 0x2000);
    }

    private static void VerifyProjectileDefinition(
        ISnesAddressSpace bus,
        ushort pointer,
        ushort initializer,
        ushort preInstruction,
        ushort instructionList,
        ushort radius,
        ushort properties)
    {
        ushort packedRadii = unchecked((ushort)(radius | (radius << 8)));
        ushort[] expected =
            [initializer, preInstruction, instructionList, packedRadii, properties];
        for (int word = 0; word < expected.Length; word++)
        {
            ushort actual = ReadWord(bus, 0x860000 | (pointer + word * 2));
            if (actual != expected[word])
            {
                throw new InvalidDataException(
                    $"Spore Spawn projectile ${pointer:X4} word {word} is ${actual:X4}, " +
                    $"expected ${expected[word]:X4}.");
            }
        }
    }

    private static void VerifyLiveEncounterCombatAndDeath(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room)
    {
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        LoadedSporeSpawn loaded = Load(bus, room, assets, alreadyDefeated: false);
        RoomEnemySlot body = loaded.Body;
        SporeSpawnEnemyState state = loaded.State;

        int stalkCount = CountProjectiles(loaded.Enemies, RoomEnemyProjectileKind.SporeSpawnStalk);
        int spawnerCount = CountProjectiles(loaded.Enemies, RoomEnemyProjectileKind.SporeSpawnSpawner);
        if (loaded.Enemies.EnemyCount != 1 || body.Health != 960 ||
            body.XPosition != 128 || body.YPosition != 496 ||
            state.Function != SporeSpawnFunction.Idle || stalkCount != 4 || spawnerCount != 4)
        {
            throw new InvalidDataException(
                $"Spore Spawn initialization mismatch: count={loaded.Enemies.EnemyCount}, " +
                $"health={body.Health}, position=({body.XPosition},{body.YPosition}), " +
                $"function={state.Function}, stalks/spawners={stalkCount}/{spawnerCount}.");
        }

        bool sawSpore = false;
        bool sawMovement = false;
        bool sawOpenCore = false;
        ushort previousX = body.XPosition;
        ushort previousY = body.YPosition;
        for (int frame = 0; frame < 1800; frame++)
        {
            Step(loaded, assets.LevelData);
            sawSpore |= CountProjectiles(
                loaded.Enemies,
                RoomEnemyProjectileKind.SporeSpawnSpore) != 0;
            sawMovement |= body.XPosition != previousX || body.YPosition != previousY;
            previousX = body.XPosition;
            previousY = body.YPosition;
            sawOpenCore |= state.Function == SporeSpawnFunction.Idle &&
                body.SpritemapPointer is >= 0xef37 and <= 0xef61;
            if (sawSpore && sawMovement && sawOpenCore)
                break;
        }
        if (!sawSpore || !sawMovement || !sawOpenCore)
        {
            throw new InvalidDataException(
                $"Spore Spawn live cycle failed: spore={sawSpore}, movement={sawMovement}, " +
                $"open={sawOpenCore}, function={state.Function}, map=${body.SpritemapPointer:X4}.");
        }

        var shots = new SamusProjectileSystem();
        var bombs = new SamusBombProjectileSystem();
        ArmProjectile(shots.Slots[0], body, type: 0x0200, damage: 2000);
        int hits = loaded.Enemies.ResolveOrdinaryProjectileHits(
            bus,
            shots,
            bombs,
            loaded.Samus);
        if (hits != 1 || body.Health != 0 || !state.DeathStarted ||
            loaded.Enemies.LastSporeSpawnPlm is not SporeSpawnPlmRequest
            {
                BlockX: 7,
                BlockY: 30,
                Header: CrumbleCeilingHeader,
            } ||
            !loaded.MiniBossBitWasSet)
        {
            throw new InvalidDataException(
                $"Spore Spawn fatal shot mismatch: hits={hits}, health={body.Health}, " +
                $"death={state.DeathStarted}, PLM={loaded.Enemies.LastSporeSpawnPlm}, " +
                $"bossBit={loaded.MiniBossBitWasSet}, map=${body.SpritemapPointer:X4}.");
        }

        var plms = new RoomPlmSystem();
        if (!plms.TrySpawnSporeSpawnCeiling(assets.LevelData, CrumbleCeilingHeader))
            throw new InvalidDataException("Spore Spawn crumble ceiling PLM could not allocate.");
        BackgroundTilemapStreamer streamer = assets.LevelData.CreateBackgroundStreamer();
        bool sawDyingExplosion = false;
        bool sawHardeningDust = false;
        bool sawSixteenDrops = false;
        for (int frame = 0; frame < 1600; frame++)
        {
            Step(loaded, assets.LevelData);
            plms.Step(
                bus,
                assets.LevelData,
                streamer,
                CameraX,
                CameraY,
                bg1XOffset: 0,
                assets.Scrolls);
            sawDyingExplosion |= loaded.Enemies.RoomSpriteObjects.Any(sprite =>
                sprite.IsActive && sprite.Kind == RoomSpriteObjectKind.SporeSpawnDyingExplosion);
            sawHardeningDust |= loaded.Enemies.EnemyProjectiles.Any(projectile =>
                projectile.IsActive && projectile.Kind == RoomEnemyProjectileKind.MiscDustExplosion);
            sawSixteenDrops |= loaded.Enemies.SporeSpawnDropRequests.Count == 16;
            if (state.DeathDropRequested && plms.ActiveCount == 0)
                break;
        }

        if (!sawDyingExplosion || !sawHardeningDust || !sawSixteenDrops ||
            !state.DeathDropRequested || plms.ActiveCount != 0 ||
            body.XPosition != 128 || body.YPosition != 624)
        {
            throw new InvalidDataException(
                $"Spore Spawn death incomplete: explosion/dust/drops=" +
                $"{sawDyingExplosion}/{sawHardeningDust}/{sawSixteenDrops}, " +
                $"dropFlag={state.DeathDropRequested}, PLMs={plms.ActiveCount}, " +
                $"position=({body.XPosition},{body.YPosition}).");
        }
        VerifyCeilingIsAir(assets.LevelData);
    }

    private static void VerifyAlreadyDefeatedCeiling(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room)
    {
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        LoadedSporeSpawn loaded = Load(bus, room, assets, alreadyDefeated: true);
        if (!loaded.State.LoadedAsDefeated ||
            loaded.Enemies.LastSporeSpawnPlm is not SporeSpawnPlmRequest
            {
                BlockX: 7,
                BlockY: 30,
                Header: ClearCeilingHeader,
            })
        {
            throw new InvalidDataException(
                "Already-defeated Spore Spawn did not publish the clear-ceiling PLM.");
        }

        var plms = new RoomPlmSystem();
        BackgroundTilemapStreamer streamer = assets.LevelData.CreateBackgroundStreamer();
        if (!plms.TrySpawnSporeSpawnCeiling(assets.LevelData, ClearCeilingHeader))
            throw new InvalidDataException("Spore Spawn clear ceiling PLM could not allocate.");
        for (int frame = 0; frame < 8 && plms.ActiveCount != 0; frame++)
        {
            plms.Step(bus, assets.LevelData, streamer, CameraX, CameraY, 0, assets.Scrolls);
        }
        if (plms.ActiveCount != 0)
            throw new InvalidDataException("Spore Spawn clear-ceiling PLM did not retire.");
        VerifyCeilingIsAir(assets.LevelData);
    }

    private static LoadedSporeSpawn Load(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        bool alreadyDefeated)
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState(0x1234);
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            XPosition = 128,
            YPosition = 704,
            Pose = SamusPoseIds.FacingRightNormalPose,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);

        bool miniBossBitWasSet = false;
        var enemies = new RoomEnemySystem();
        enemies.Load(
            bus,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            random.NextRandom,
            random.SetRandomNumber,
            readRandomNumber: () => random.RandomNumber,
            level: assets.LevelData,
            samus: samus,
            isAreaMiniBossDefeated: () => alreadyDefeated,
            setAreaMiniBossDefeated: () => miniBossBitWasSet = true);
        return new LoadedSporeSpawn(
            enemies,
            samus,
            enemies.Slots[0],
            enemies.SporeSpawn ?? throw new InvalidDataException(
                "Spore Spawn load produced no typed state."),
            () => miniBossBitWasSet);
    }

    private static void VerifyRuntimeAlreadyDefeatedIntegration(
        SuperMetroidAddressSpace bus)
    {
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();

        runtime.System.SetBossBits(areaIndex: AreaId.Brinstar, BossBits.AreaMiniBoss);
        runtime.LoadCartridgeRoomForDebug(RoomPointer, CameraX, CameraY);
        if (runtime.Plms.ActiveCount != 1)
        {
            throw new InvalidDataException(
                "Runtime did not consume Spore Spawn's $B793 publication during room load.");
        }
    }

    private static void Step(LoadedSporeSpawn loaded, RoomLevelData level)
    {
        loaded.Enemies.StepFrame(
            CameraX,
            CameraY,
            timeIsFrozen: false,
            loaded.Samus,
            level: level);
        loaded.Enemies.StepEnemyProjectiles(
            level,
            loaded.Samus,
            cameraX: CameraX,
            cameraY: CameraY);
    }

    private static int CountProjectiles(
        RoomEnemySystem enemies,
        RoomEnemyProjectileKind kind) =>
        enemies.EnemyProjectiles.Count(projectile =>
            projectile.IsActive && projectile.Kind == kind);

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

    private static void VerifyCeilingIsAir(RoomLevelData level)
    {
        for (int y = 30; y <= 31; y++)
        {
            for (int x = 7; x <= 8; x++)
            {
                ushort word = level.GetCollisionBlockByIndex(level.GetBlockIndex(x, y)).LevelWord;
                if (word != 0x00ff)
                {
                    throw new InvalidDataException(
                        $"Spore Spawn ceiling block ({x},{y}) remained ${word:X4}.");
                }
            }
        }
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private readonly record struct LoadedSporeSpawn(
        RoomEnemySystem Enemies,
        SamusState Samus,
        RoomEnemySlot Body,
        SporeSpawnEnemyState State,
        Func<bool> ReadMiniBossBit)
    {
        public bool MiniBossBitWasSet => ReadMiniBossBit();
    }
}
