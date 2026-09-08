using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Audio;

/// <summary>Retail room, actor, and viewport identities used by the Spore Spawn audit.</summary>
internal static class SporeSpawnAuditDefinitions
{
    /// <summary>Spore Spawn room header at <c>$8F:9DC7</c>, logical room <c>$01/$0B</c>.</summary>
    public const ushort RoomPointer = 0x9dc7;
    /// <summary>$8F:9D9C, the room containing the upward entrance into Spore Spawn.</summary>
    public const ushort EntryRoomPointer = 0x9d9c;
    /// <summary>$A5:E6B9 selects byte offset $C0 in the three death-palette tables.</summary>
    public const int FinalDeathPaletteOffset = 0xc0;
    /// <summary>$A5:E91C writes sprite row 9, level row 4, and background row 7.</summary>
    public static readonly (int Source, int Destination)[] DeathPaletteRows =
        [(0xa5e3f9, 144), (0xa5e4f9, 64), (0xa5e5d9, 112)];
    /// <summary>Single-body population at <c>$A1:A0FD</c>.</summary>
    public const ushort PopulationPointer = 0xa0fd;
    /// <summary>
    /// Upward door <c>$83:8E3E</c> used by the normal route from the Kihunter hall into
    /// the bottom of Spore Spawn's room.
    /// </summary>
    public const ushort IncomingDoorPointer = 0x8e3e;
    /// <summary>
    /// Final layer-one Y produced by the upward door IRQ: destination screen two plus the
    /// cartridge's <c>$20</c>-pixel upward-transition adjustment.
    /// </summary>
    public const ushort IncomingDoorFinalCameraY = 0x0220;
    /// <summary>Bank-$84 PLM which crumbles the live encounter ceiling.</summary>
    public const ushort CrumbleCeilingHeader = 0xb78f;
    /// <summary>Bank-$84 PLM which clears the ceiling on an already-defeated load.</summary>
    public const ushort ClearCeilingHeader = 0xb793;
    /// <summary>Locked one-screen-wide room camera X.</summary>
    public const ushort CameraX = 0;
    /// <summary>Bottom-screen camera Y used when the boss first becomes visible.</summary>
    public const ushort CameraY = SporeSpawnScrollingHooks.FightMinimumLayerOneY;
    /// <summary>
    /// Last position immediately above the <c>$90:9589</c> encounter camera floor. This
    /// is the boundary reached while the player climbs from the incoming bottom door.
    /// </summary>
    public const ushort CameraYImmediatelyAboveEncounterFloor = 463;
    /// <summary>Stable bottom-arena Samus Y used while observing the camera hook.</summary>
    public const ushort EntryAuditSamusY = 0x02c0;
    /// <summary>Cartridge-authored body center X.</summary>
    public const ushort BodyCenterX = 128;
    /// <summary>Initial visible body Y after <c>$A5:EAE1</c>'s 128-pixel subtraction.</summary>
    public const ushort InitialVisibleBodyY = 496;
    /// <summary>Left edge of the first closed extended spritemap at center X 128.</summary>
    public const int FirstVisibleLeft = 80;
    /// <summary>Exclusive right edge of the first closed extended spritemap.</summary>
    public const int FirstVisibleRight = 176;
    /// <summary>
    /// Closed-map tiles which intersect the 224-line playfield when the encounter hook
    /// holds the body at screen Y 32. The two highest tiles remain correctly clipped.
    /// </summary>
    public const int InitialVisibleBodyTileCount = 24;
}

/// <summary>
/// End-to-end ROM-backed audit for Spore Spawn. The room, enemy/projectile definitions,
/// instruction streams, extended hitboxes, palettes, and PLM draw lists all come from the
/// user's cartridge; the audit supplies only deterministic RNG, Samus, and weapon stimuli.
/// </summary>
internal static class SporeSpawnAudit
{
    private const ushort DefinitionPointer = RoomEnemySystem.SporeSpawnDefinition;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, SporeSpawnAuditDefinitions.RoomPointer);
        VerifyRetailRoomAndHeader(bus, room);
        VerifyLiveEntryCameraAndFirstVisibleBody(bus);
        VerifyLiveEncounterCombatAndDeath(bus, room);
        VerifyAlreadyDefeatedCeiling(bus, room);
        VerifyRuntimeAlreadyDefeatedIntegration(bus);

        Console.WriteLine(
            "Spore Spawn audit passed: real entry camera hook/first visible body, retail " +
            "room/header, stalk interpolation, ceiling emitters, spores, ROM instruction " +
            "cadence, extended hitboxes, movement and damage reaction, death effects/" +
            "palettes/drops, boss bit, and both ceiling PLMs.");
        return 0;
    }

    private static void VerifyLiveEntryCameraAndFirstVisibleBody(
        SuperMetroidAddressSpace bus)
    {
        CartridgeDoorHeader incomingDoor = CartridgeDoorHeader.Load(
            bus,
            SporeSpawnAuditDefinitions.IncomingDoorPointer);
        if (incomingDoor.DestinationRoomPointer != SporeSpawnAuditDefinitions.RoomPointer ||
            incomingDoor.Orientation != 7 ||
            incomingDoor.DestinationScreenY != 2)
        {
            throw new InvalidDataException(
                $"Spore Spawn incoming door $83:{incomingDoor.Pointer:X4} no longer names " +
                $"the upward bottom-screen entry: destination=$8F:{incomingDoor.DestinationRoomPointer:X4}, " +
                $"orientation=${incomingDoor.Orientation:X2}, screenY={incomingDoor.DestinationScreenY}.");
        }

        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomThroughDoorForVerification(
            incomingDoor,
            cameraX: SporeSpawnAuditDefinitions.CameraX,
            cameraY: SporeSpawnAuditDefinitions.IncomingDoorFinalCameraY);

        // Reproduce the exact encounter boundary reached after the bottom-door climb.
        // Bank $90 calls its installed scrolling-finished hook after ordinary tracking and
        // before the enemy draw pass. Starting one pixel above the authored floor makes a
        // missing hook observable without substituting a guessed boss destination.
        runtime.Camera!.SetPosition(
            SporeSpawnAuditDefinitions.CameraX,
            SporeSpawnAuditDefinitions.CameraYImmediatelyAboveEncounterFloor);
        runtime.Samus!.XPosition = SporeSpawnAuditDefinitions.BodyCenterX;
        runtime.Samus.YPosition = SporeSpawnAuditDefinitions.EntryAuditSamusY;
        runtime.Samus.Pose = SamusPoseIds.FacingRightNormalPose;
        runtime.Samus.RefreshCollisionRadii(bus);
        runtime.Samus.InitializeAnimation(bus);
        runtime.StepFrame(controller1Input: 0);

        SporeSpawnEnemyState state = runtime.Enemies.SporeSpawn ??
            throw new InvalidDataException("Live room entry did not initialize Spore Spawn.");
        int bodyScreenY = unchecked((short)(state.Body.YPosition - runtime.Camera.YPosition));
        var bodyOam = new OamBuffer();
        bodyOam.BeginFrame();
        runtime.Enemies.DrawLayers(
            bodyOam,
            runtime.Camera.XPosition,
            runtime.Camera.YPosition,
            firstLayer: 0,
            lastLayer: 7);
        bodyOam.FinalizeFrame();
        int visibleBodyTiles = Enumerable.Range(0, bodyOam.LastFinalizedSpriteCount)
            .Select(bodyOam.GetEntry)
            .Count(entry => entry.Y < 224);
        if (!state.ScrollClampHookActive ||
            runtime.Camera.YPosition != SporeSpawnAuditDefinitions.CameraY ||
            bodyScreenY != 32 ||
            visibleBodyTiles != SporeSpawnAuditDefinitions.InitialVisibleBodyTileCount)
        {
            throw new InvalidDataException(
                $"Spore Spawn entry camera hook did not expose the body at its cartridge " +
                $"position: hook={state.ScrollClampHookActive}, cameraY={runtime.Camera.YPosition}, " +
                $"bodyY={state.Body.YPosition}, screenY={bodyScreenY}, " +
                $"visibleTiles={visibleBodyTiles}.");
        }
    }

    private static void VerifyRetailRoomAndHeader(
        ISnesAddressSpace bus,
        CartridgeRoomHeader room)
    {
        if (room.Pointer != SporeSpawnAuditDefinitions.RoomPointer ||
            room.Identity != new RoomIdentity(AreaId.Brinstar, 0x0b) ||
            room.WidthInScreens != 1 || room.HeightInScreens != 3 ||
            room.State.EnemyPopulationPointer != SporeSpawnAuditDefinitions.PopulationPointer)
        {
            throw new InvalidDataException(
                $"Spore Spawn room mismatch: room/state=${room.Pointer:X4}/" +
                $"${room.State.Pointer:X4}, area={room.AreaIndex}, size=" +
                $"{room.WidthInScreens}x{room.HeightInScreens}, population=" +
                $"${room.State.EnemyPopulationPointer:X4}.");
        }

        ushort[] expectedPopulation =
            [DefinitionPointer, SporeSpawnAuditDefinitions.BodyCenterX, 0x0270, 0, 0x2800, 0x0004, 0, 0];
        for (int word = 0; word < expectedPopulation.Length; word++)
        {
            ushort actual = ReadWord(
                bus,
                0xa10000 | (SporeSpawnAuditDefinitions.PopulationPointer + word * 2));
            if (actual != expectedPopulation[word])
            {
                throw new InvalidDataException(
                    $"Spore Spawn population word {word} is ${actual:X4}, expected " +
                    $"${expectedPopulation[word]:X4}.");
            }
        }
        if (ReadWord(
                bus,
                0xa10000 | (SporeSpawnAuditDefinitions.PopulationPointer + 16)) != 0xffff)
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

        // The load initializer deliberately installs the generic empty extended map. The
        // first ordinary enemy pass advances the cartridge list to the first visible map;
        // this is the exact entry frame named by issue #249, not a later orbit position.
        Step(loaded, assets.LevelData);
        var firstFrameOam = new OamBuffer();
        firstFrameOam.BeginFrame();
        loaded.Enemies.DrawLayers(
            firstFrameOam,
            SporeSpawnAuditDefinitions.CameraX,
            SporeSpawnAuditDefinitions.CameraY,
            0,
            7);
        firstFrameOam.FinalizeFrame();
        OamEntry[] firstFrameEntries = Enumerable.Range(
                0,
                firstFrameOam.LastFinalizedSpriteCount)
            .Select(firstFrameOam.GetEntry)
            .ToArray();
        int firstVisibleLeft = firstFrameEntries.Min(entry => SignedOamX(entry.X));
        int firstVisibleRight = firstFrameEntries.Max(entry =>
            SignedOamX(entry.X) + (entry.IsLarge ? 16 : 8));
        if (body.XPosition != SporeSpawnAuditDefinitions.BodyCenterX ||
            body.YPosition != SporeSpawnAuditDefinitions.InitialVisibleBodyY ||
            firstVisibleLeft != SporeSpawnAuditDefinitions.FirstVisibleLeft ||
            firstVisibleRight != SporeSpawnAuditDefinitions.FirstVisibleRight ||
            (firstVisibleLeft + firstVisibleRight) / 2 !=
                SporeSpawnAuditDefinitions.BodyCenterX)
        {
            throw new InvalidDataException(
                $"Spore Spawn first visible frame was not centered: body=" +
                $"({body.XPosition},{body.YPosition}), map=${body.SpritemapPointer:X4}, " +
                $"bounds=[{firstVisibleLeft},{firstVisibleRight}).");
        }

        int stalkCount = CountProjectiles(loaded.Enemies, RoomEnemyProjectileKind.SporeSpawnStalk);
        int spawnerCount = CountProjectiles(loaded.Enemies, RoomEnemyProjectileKind.SporeSpawnSpawner);
        if (loaded.Enemies.EnemyCount != 1 || body.Health != 960 ||
            body.XPosition != SporeSpawnAuditDefinitions.BodyCenterX ||
            body.YPosition != SporeSpawnAuditDefinitions.InitialVisibleBodyY ||
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
                Header: SporeSpawnAuditDefinitions.CrumbleCeilingHeader,
            } ||
            !loaded.MiniBossBitWasSet)
        {
            throw new InvalidDataException(
                $"Spore Spawn fatal shot mismatch: hits={hits}, health={body.Health}, " +
                $"death={state.DeathStarted}, PLM={loaded.Enemies.LastSporeSpawnPlm}, " +
                $"bossBit={loaded.MiniBossBitWasSet}, map=${body.SpritemapPointer:X4}.");
        }

        var plms = new RoomPlmSystem();
        if (!plms.TrySpawnSporeSpawnCeiling(
                assets.LevelData,
                SporeSpawnAuditDefinitions.CrumbleCeilingHeader))
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
                SporeSpawnAuditDefinitions.CameraX,
                SporeSpawnAuditDefinitions.CameraY,
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
            body.XPosition != SporeSpawnAuditDefinitions.BodyCenterX || body.YPosition != 624)
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
                Header: SporeSpawnAuditDefinitions.ClearCeilingHeader,
            })
        {
            throw new InvalidDataException(
                "Already-defeated Spore Spawn did not publish the clear-ceiling PLM.");
        }

        var plms = new RoomPlmSystem();
        BackgroundTilemapStreamer streamer = assets.LevelData.CreateBackgroundStreamer();
        if (!plms.TrySpawnSporeSpawnCeiling(
                assets.LevelData,
                SporeSpawnAuditDefinitions.ClearCeilingHeader))
            throw new InvalidDataException("Spore Spawn clear ceiling PLM could not allocate.");
        for (int frame = 0; frame < 8 && plms.ActiveCount != 0; frame++)
        {
            plms.Step(
                bus,
                assets.LevelData,
                streamer,
                SporeSpawnAuditDefinitions.CameraX,
                SporeSpawnAuditDefinitions.CameraY,
                0,
                assets.Scrolls);
        }
        if (plms.ActiveCount != 0)
            throw new InvalidDataException("Spore Spawn clear-ceiling PLM did not retire.");
        VerifyCeilingIsAir(assets.LevelData);
        VerifyDefeatedStalkDrawsBehindSamus(loaded, assets.LevelData);
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
        runtime.LoadCartridgeRoomForDebug(
            SporeSpawnAuditDefinitions.RoomPointer,
            SporeSpawnAuditDefinitions.CameraX,
            SporeSpawnAuditDefinitions.CameraY);
        if (runtime.LevelData is null)
        {
            throw new InvalidDataException(
                "Runtime did not retain level data after consuming Spore Spawn's $B793 publication.");
        }
        // The room's unrelated native population remains live after the one-shot ceiling
        // actor retires, so aggregate ActiveCount cannot identify $B793. The observable
        // contract is the exact four cartridge blocks that the instruction stream clears.
        for (int frame = 0; frame < 8 && !CeilingIsAir(runtime.LevelData); frame++)
            runtime.StepFrame(controller1Input: 0);
        VerifyCeilingIsAir(runtime.LevelData);
        // Exercise the production door coroutine, including its early palette capture
        // and later enemy-instruction pass, not just a manually merged target buffer.
        runtime.LoadCartridgeRoomForDebug(SporeSpawnAuditDefinitions.EntryRoomPointer);
        runtime.Samus!.XPosition = 128;
        runtime.Samus.YPosition = 64;
        var level = runtime.LevelData!;
        for (int index = 0; index < level.ForegroundEntries.Length; index++)
        {
            var block = level.GetCollisionBlockByIndex(index);
            if (block.CollisionType != RoomCollisionType.DoorBlock) continue;
            var door = level.ResolveDoorCollision(bus, block.Behavior, runtime.Samus.Pose, false);
            if (door.Pointer != SporeSpawnAuditDefinitions.IncomingDoorPointer) continue;
            level.ResolveDoorCollision(bus, block.Behavior, runtime.Samus.Pose, true);
            break;
        }
        var transition = new DoorTransitionState();
        transition.Begin(runtime);
        var audio = new CartridgeAudioState();
        for (int frame = 0; frame < 400 && transition.IsActive; frame++)
            transition.Step(runtime, audio, 0);
        if (transition.IsActive) throw new InvalidDataException("Spore Spawn entry fade did not finish.");
        foreach (var row in SporeSpawnAuditDefinitions.DeathPaletteRows)
        {
            for (int color = 0; color < 16; color++)
            {
                int address = row.Source + SporeSpawnAuditDefinitions.FinalDeathPaletteOffset + color * 2;
                ushort expected = (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
                ushort actual = runtime.Cgram.Colors[row.Destination + color];
                if (actual != expected)
                    throw new InvalidDataException($"Defeated Spore Spawn re-entry color {row.Destination + color}: {actual:X4}, expected {expected:X4}.");
            }
        }
    }

    private static void VerifyDefeatedStalkDrawsBehindSamus(
        LoadedSporeSpawn loaded,
        RoomLevelData level)
    {
        // Advance both the enemy body and bank-$86 streams once so their cartridge-authored
        // first maps exist in OAM. Every stalk definition has property $2000, so the high
        // pass must remain empty and the low pass must be appended after both Samus and the
        // layer-five defeated body. Lower OAM numbers win OBJ overlap on SNES hardware.
        Step(loaded, level);
        var oam = new OamBuffer();
        oam.BeginFrame();
        loaded.Enemies.DrawHighPriorityEnemyProjectiles(
            oam,
            SporeSpawnAuditDefinitions.CameraX,
            SporeSpawnAuditDefinitions.CameraY);
        int samusSpriteIndex = oam.NextByteOffset / 4;
        oam.AddRawSmallSprite(
            SporeSpawnAuditDefinitions.BodyCenterX,
            96,
            attributes: 0);
        loaded.Enemies.DrawLayers(
            oam,
            SporeSpawnAuditDefinitions.CameraX,
            SporeSpawnAuditDefinitions.CameraY,
            3,
            5);
        int firstLowProjectileSpriteIndex = oam.NextByteOffset / 4;
        loaded.Enemies.DrawLowPriorityEnemyProjectiles(
            oam,
            SporeSpawnAuditDefinitions.CameraX,
            SporeSpawnAuditDefinitions.CameraY);
        oam.FinalizeFrame();

        RoomEnemyProjectileSlot[] stalks = loaded.Enemies.EnemyProjectiles
            .Where(projectile =>
                projectile.IsActive &&
                projectile.Kind == RoomEnemyProjectileKind.SporeSpawnStalk)
            .ToArray();
        if (samusSpriteIndex != 0 ||
            firstLowProjectileSpriteIndex <= samusSpriteIndex ||
            oam.LastFinalizedSpriteCount <= firstLowProjectileSpriteIndex ||
            stalks.Length != 4 ||
            stalks.Any(stalk =>
                stalk.DrawPriority != EnemyProjectileDrawPriority.Low))
        {
            throw new InvalidDataException(
                $"Defeated Spore Spawn OAM order disagreed with $86:8390/$83B2: " +
                $"Samus={samusSpriteIndex}, lowStart={firstLowProjectileSpriteIndex}, " +
                $"final={oam.LastFinalizedSpriteCount}, stalks={stalks.Length}.");
        }
    }

    private static void Step(LoadedSporeSpawn loaded, RoomLevelData level)
    {
        loaded.Enemies.StepFrame(
            SporeSpawnAuditDefinitions.CameraX,
            SporeSpawnAuditDefinitions.CameraY,
            timeIsFrozen: false,
            loaded.Samus,
            level: level);
        loaded.Enemies.StepEnemyProjectiles(
            level,
            loaded.Samus,
            cameraX: SporeSpawnAuditDefinitions.CameraX,
            cameraY: SporeSpawnAuditDefinitions.CameraY);
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

    private static bool CeilingIsAir(RoomLevelData level)
    {
        for (int y = 30; y <= 31; y++)
        {
            for (int x = 7; x <= 8; x++)
            {
                if (level.GetCollisionBlockByIndex(level.GetBlockIndex(x, y)).LevelWord != 0x00ff)
                    return false;
            }
        }
        return true;
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private static int SignedOamX(int x) => x >= 0x100 ? x - 0x200 : x;

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
