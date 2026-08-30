using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// End-to-end ROM-backed audit for Botwoon and both of his hardcoded wall PLMs. Every enemy
/// header, path, instruction list, spritemap, projectile definition, and PLM draw list is
/// consumed from the user's cartridge; the audit supplies only deterministic controller,
/// Samus, boss-bit, and weapon stimuli.
/// </summary>
internal static partial class BotwoonAudit
{
    private const ushort RoomPointer = 0xd95e;
    private const ushort Definition = 0xf293;
    private const ushort PopulationPointer = 0xde5a;
    private const ushort ClearWallHeader = 0xb797;
    private const ushort CrumbleWallHeader = 0xb79b;
    private const ushort CameraX = 0;
    private const ushort CameraY = 0;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, RoomPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);

        VerifyRetailRoomAndHeader(bus, room);
        VerifyLiveMovementAttackAndDrawing(bus, room, assets);
        VerifySpitLifecycleAndInteractions(bus, room, assets);
        VerifyCombatDeathDropsWallAndMusic(bus, room, assets);
        VerifyAlreadyDefeatedWall(bus, room);
        VerifyRuntimeAlreadyDefeatedIntegration(bus);

        Console.WriteLine(
            "Botwoon audit passed: retail room/header, 13-link body, path movement, aimed " +
            "spit with exact fixed-point movement, five-map animation, off-screen disposal, " +
            "shot pass-through and contact damage, beam fatal and normal-bomb immune " +
            "callbacks, delayed " +
            "fatal traversal, staggered body fall, sixteen " +
            "drop requests, 192-frame death effects, boss bit, delayed music, live nine-row " +
            "crumble PLM, and already-defeated clear-wall PLM all used cartridge data.");
        return 0;
    }

    private static void VerifyRetailRoomAndHeader(
        ISnesAddressSpace bus,
        CartridgeRoomHeader room)
    {
        if (room.Pointer != RoomPointer || room.AreaIndex != 4 ||
            room.WidthInScreens != 2 || room.HeightInScreens != 1 ||
            room.State.Pointer != 0xd970 ||
            room.State.EnemyPopulationPointer != PopulationPointer ||
            room.State.EnemyTilesetPointer != 0x9028)
        {
            throw new InvalidDataException(
                $"Botwoon room mismatch: room/state=${room.Pointer:X4}/${room.State.Pointer:X4}, " +
                $"area={room.AreaIndex}, size={room.WidthInScreens}x{room.HeightInScreens}, " +
                $"population/tiles=${room.State.EnemyPopulationPointer:X4}/" +
                $"${room.State.EnemyTilesetPointer:X4}.");
        }

        ushort[] expectedPopulation =
            [Definition, 0x0080, 0x0080, 0, 0x2800, 0, 0, 0];
        for (int word = 0; word < expectedPopulation.Length; word++)
        {
            ushort actual = ReadWord(bus, 0xa10000 | (PopulationPointer + word * 2));
            if (actual != expectedPopulation[word])
            {
                throw new InvalidDataException(
                    $"Botwoon population word {word} is ${actual:X4}, expected " +
                    $"${expectedPopulation[word]:X4}.");
            }
        }
        if (ReadWord(bus, 0xa10000 | (PopulationPointer + 16)) != 0xffff)
            throw new InvalidDataException("Botwoon population did not end after one actor.");

        RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(bus, Definition);
        if (definition.Bank != 0xb3 || definition.Health != 3000 ||
            definition.Damage != 120 || definition.XRadius != 8 ||
            definition.YRadius != 8 || definition.InitializationAiPointer != 0x9583 ||
            definition.MainAiPointer != 0x9668 || definition.HurtAiPointer != 0x804c ||
            definition.TouchAiPointer != 0x9fff || definition.ShotAiPointer != 0xa016 ||
            definition.PowerBombReactionPointer != 0xa041 ||
            definition.ItemDropChancesPointer != 0xf344 ||
            definition.VulnerabilityPointer != 0xf118)
        {
            throw new InvalidDataException(
                $"Botwoon header mismatch: bank=${definition.Bank:X2}, health/damage=" +
                $"{definition.Health}/{definition.Damage}, radius={definition.XRadius}x" +
                $"{definition.YRadius}, init/main=${definition.InitializationAiPointer:X4}/" +
                $"${definition.MainAiPointer:X4}, touch/shot/pb=" +
                $"${definition.TouchAiPointer:X4}/${definition.ShotAiPointer:X4}/" +
                $"${definition.PowerBombReactionPointer:X4}, drop/vulnerability=" +
                $"${definition.ItemDropChancesPointer:X4}/${definition.VulnerabilityPointer:X4}.");
        }
    }

    private static void VerifyLiveMovementAttackAndDrawing(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedBotwoon loaded = Load(bus, room, assets, alreadyDefeated: false);
        RoomEnemySlot head = loaded.Head;
        BotwoonEnemyState state = loaded.State;
        int bodyCount = loaded.Enemies.EnemyProjectiles.Count(projectile =>
            projectile.IsActive && projectile.Kind == RoomEnemyProjectileKind.BotwoonBody);
        if (loaded.Enemies.EnemyCount != 1 || head.Health != 3000 ||
            head.XPosition != 0x0080 || head.YPosition != 0x0080 ||
            state.Function != BotwoonEnemyFunction.InitialDelay ||
            state.InitialDelayTimer != 256 || state.BodySegments.Any(segment => segment is null) ||
            bodyCount != 13)
        {
            throw new InvalidDataException(
                $"Botwoon initialization mismatch: count={loaded.Enemies.EnemyCount}, " +
                $"health={head.Health}, position=(${head.XPosition:X4},${head.YPosition:X4}), " +
                $"function/timer={state.Function}/{state.InitialDelayTimer}, body={bodyCount}.");
        }

        ushort startX = head.XPosition;
        ushort startY = head.YPosition;
        var headMaps = new HashSet<ushort>();
        var bodyMaps = new HashSet<ushort>();
        RoomEnemyProjectileSlot? spit = null;
        for (int frame = 0; frame < 4000 && spit is null; frame++)
        {
            StepEnemies(loaded, assets.LevelData);
            headMaps.Add(head.SpritemapPointer);
            foreach (RoomEnemyProjectileSlot projectile in loaded.Enemies.EnemyProjectiles)
            {
                if (projectile.IsActive && projectile.Kind == RoomEnemyProjectileKind.BotwoonBody)
                    bodyMaps.Add(projectile.SpritemapPointer);
            }
            spit = loaded.Enemies.EnemyProjectiles.FirstOrDefault(projectile =>
                projectile.IsActive && projectile.Kind == RoomEnemyProjectileKind.BotwoonSpit);
        }
        if (spit is null || head.XPosition == startX && head.YPosition == startY ||
            headMaps.Count < 2 || bodyMaps.Count < 2)
        {
            throw new InvalidDataException(
                $"Botwoon live cycle failed: moved={head.XPosition != startX || head.YPosition != startY}, " +
                $"head/body maps={headMaps.Count}/{bodyMaps.Count}, spit={spit is not null}.");
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        loaded.Enemies.DrawEnemyProjectiles(oam, CameraX, CameraY);
        loaded.Enemies.DrawLayers(oam, CameraX, CameraY, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("Botwoon head/body/spit emitted no OBJ pieces.");

    }

    private static void VerifyCombatDeathDropsWallAndMusic(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedBotwoon loaded = Load(bus, room, assets, alreadyDefeated: false);
        var shots = new SamusProjectileSystem();
        var bombs = new SamusBombProjectileSystem();

        AdvanceUntilShootable(loaded, assets.LevelData);

        // `$B3:A016` saves pre-hit health before invoking common no-death shot damage. A
        // normal bomb reaches the same callback through physical slots five through nine,
        // but Botwoon's cartridge vulnerability byte fourteen is zero. Prove both facts on
        // a separate load: the saved-health write occurs, while health/death state does not.
        LoadedBotwoon bombLoaded = Load(bus, room, assets, alreadyDefeated: false);
        AdvanceUntilShootable(bombLoaded, assets.LevelData);
        var bombShots = new SamusProjectileSystem();
        var normalBombs = new SamusBombProjectileSystem();
        bombLoaded.State.PreviousHealth = 0x1234;
        SamusBombProjectileSlot normalBomb =
            EnemyProjectileAuditAssertions.ArmExplodingNormalBomb(
                normalBombs,
                bombLoaded.Head.XPosition,
                bombLoaded.Head.YPosition,
                damage: 6000);
        int bombHits = bombLoaded.Enemies.ResolveOrdinaryBombHits(
            normalBombs,
            bombShots,
            bombLoaded.Samus);
        if (bombHits != 1 || (normalBomb.Direction & 0x0010) == 0 ||
            bombLoaded.State.PreviousHealth != 3000 || bombLoaded.Head.Health != 3000 ||
            bombLoaded.Head.FlashTimer != 0 || bombLoaded.State.PendingDeath ||
            bombLoaded.Head.Properties.HasAny(EnemyProperties.Deleted))
        {
            throw new InvalidDataException(
                $"Botwoon normal-bomb immunity mismatch: hits={bombHits}, direction=" +
                $"${normalBomb.Direction:X4}, previous/health=" +
                $"{bombLoaded.State.PreviousHealth}/{bombLoaded.Head.Health}, pending=" +
                $"{bombLoaded.State.PendingDeath}, deleted=" +
                $"{bombLoaded.Head.Properties.HasAny(EnemyProperties.Deleted)}.");
        }

        // A synthetic 3000-damage super preserves the ordinary projectile dispatcher while
        // reaching the fatal callback in one deterministic frame. Damage magnitude is only
        // a test stimulus; vulnerability, hit registration, callback, and delayed deletion
        // all remain the cartridge-authored Botwoon path.
        ArmProjectile(shots.Slots[0], loaded.Head, type: 0x0200, damage: 3000);
        int hits = loaded.Enemies.ResolveOrdinaryProjectileHits(
            bus,
            shots,
            bombs,
            loaded.Samus);
        if (hits != 1 || loaded.Head.Health != 0 || !loaded.State.PendingDeath ||
            loaded.Head.Properties.HasAny(EnemyProperties.Deleted))
        {
            throw new InvalidDataException(
                $"Botwoon fatal shot mismatch: hits={hits}, health={loaded.Head.Health}, " +
                $"pending={loaded.State.PendingDeath}, deleted=" +
                $"{loaded.Head.Properties.HasAny(EnemyProperties.Deleted)}, active=" +
                $"[{string.Join(',', loaded.Enemies.InteractiveEnemyIndexes.Select(index => $"${index:X4}"))}], " +
                $"head=(${loaded.Head.XPosition:X4},${loaded.Head.YPosition:X4}) " +
                $"r={loaded.Head.XRadius}x{loaded.Head.YRadius} map=${loaded.Head.SpritemapPointer:X4} " +
                $"props=${loaded.Head.Properties:X4}, shotActive={shots.Slots[0].IsActive} " +
                $"type/damage=${shots.Slots[0].Type:X4}/{shots.Slots[0].Damage}.");
        }

        var plms = new RoomPlmSystem();
        BackgroundTilemapStreamer streamer = assets.LevelData.CreateBackgroundStreamer();
        bool crumbleSpawned = false;
        bool bossBitSet = false;
        bool sawMusic = false;
        bool sawBodyDeath = false;
        int plmFrames = 0;

        for (int frame = 0; frame < 6000; frame++)
        {
            StepEnemies(loaded, assets.LevelData);
            sawBodyDeath |= loaded.State.BodyDeathStarted;

            if (loaded.Enemies.LastBotwoonWallPlm is ushort wallHeader)
            {
                if (wallHeader != CrumbleWallHeader || crumbleSpawned ||
                    !plms.TrySpawnBotwoonWall(assets.LevelData, wallHeader))
                {
                    throw new InvalidDataException(
                        $"Botwoon live wall publication was invalid: header=${wallHeader:X4}, " +
                        $"alreadySpawned={crumbleSpawned}, PLMs={plms.ActiveCount}.");
                }
                crumbleSpawned = true;

                if (loaded.Enemies.BotwoonDropRequests.Count != 16 ||
                    loaded.Enemies.BotwoonDropRequests.Any(drop =>
                        drop.X is < 64 or > 191 || drop.Y is < 128 or > 191 ||
                        drop.ItemDropChancesPointer != 0xf344))
                {
                    throw new InvalidDataException(
                        $"Botwoon emitted {loaded.Enemies.BotwoonDropRequests.Count} invalid drop requests.");
                }
            }

            if (crumbleSpawned && plms.ActiveCount != 0)
            {
                plmFrames++;
                plms.Step(
                    bus,
                    assets.LevelData,
                    streamer,
                    CameraX,
                    CameraY,
                    bg1XOffset: 0,
                    assets.Scrolls);

                // The setup-owned 64-frame wait includes the spawn frame's PLM pass. AB51
                // is the first executable command when that timer reaches zero.
                if (plmFrames == 63 && assets.Scrolls.ReadStorage(1) != 0)
                    throw new InvalidDataException("Botwoon wall opened scroll one before frame 64.");
                if (plmFrames == 64 && assets.Scrolls.ReadStorage(1) != 1)
                    throw new InvalidDataException("Botwoon wall did not open scroll one on frame 64.");
            }

            bossBitSet |= loaded.State.BossBitSet;
            sawMusic |= loaded.Enemies.LastBotwoonMusicRequest is { Track: 3, DelayFrames: 8 };
            if (loaded.Head.Properties.HasAny(EnemyProperties.Deleted) && plms.ActiveCount == 0)
                break;
        }

        if (!sawBodyDeath || !crumbleSpawned || !bossBitSet || !loaded.MiniBossBitWasSet ||
            !sawMusic || !loaded.Head.Properties.HasAny(EnemyProperties.Deleted) ||
            plms.ActiveCount != 0)
        {
            throw new InvalidDataException(
                $"Botwoon death did not complete: body={sawBodyDeath}, wall={crumbleSpawned}, " +
                $"state/callback boss={bossBitSet}/{loaded.MiniBossBitWasSet}, music={sawMusic}, " +
                $"deleted={loaded.Head.Properties.HasAny(EnemyProperties.Deleted)}, " +
                $"PLMs={plms.ActiveCount}.");
        }

        for (int row = 0; row < 9; row++)
        {
            int blockIndex = assets.LevelData.GetBlockIndex(15, 4 + row);
            if (assets.LevelData.GetCollisionBlockByIndex(blockIndex).LevelWord != 0x00ff)
            {
                throw new InvalidDataException(
                    $"Botwoon crumble PLM left wall row {row} non-air.");
            }
        }
    }

    private static void VerifyAlreadyDefeatedWall(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room)
    {
        // Reload assets because the live audit intentionally mutated its level and scroll
        // arrays. CartridgeRoomAssets.Load decompresses a fresh room-owned copy each time.
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        LoadedBotwoon loaded = Load(bus, room, assets, alreadyDefeated: true);
        if (!loaded.Head.Properties.HasAny(EnemyProperties.Deleted) ||
            loaded.State.BodySegments.Any(segment => segment is not null) ||
            loaded.Enemies.LastBotwoonWallPlm != ClearWallHeader)
        {
            throw new InvalidDataException(
                "Already-defeated Botwoon did not delete the head and publish $B797.");
        }

        var plms = new RoomPlmSystem();
        BackgroundTilemapStreamer streamer = assets.LevelData.CreateBackgroundStreamer();
        if (!plms.TrySpawnBotwoonWall(assets.LevelData, ClearWallHeader))
            throw new InvalidDataException("Already-defeated Botwoon could not allocate $B797.");

        // `$B3:959E` is outside the PLM list and performs this store immediately.
        assets.Scrolls.SetLogicalCell(0, 0, (byte)RoomScrollState.Blue);
        assets.Scrolls.SetLogicalCell(1, 0, (byte)RoomScrollState.Blue);
        plms.Step(bus, assets.LevelData, streamer, CameraX, CameraY, 0, assets.Scrolls);
        for (int row = 0; row < 9; row++)
        {
            int blockIndex = assets.LevelData.GetBlockIndex(15, 4 + row);
            if (assets.LevelData.GetCollisionBlockByIndex(blockIndex).LevelWord != 0x00ff)
                throw new InvalidDataException($"Clear-wall PLM left row {row} non-air.");
        }
        if (assets.Scrolls.ReadStorage(0) != 1 || assets.Scrolls.ReadStorage(1) != 1)
            throw new InvalidDataException("Already-defeated Botwoon did not open both scrolls.");
        plms.Step(bus, assets.LevelData, streamer, CameraX, CameraY, 0, assets.Scrolls);
        if (plms.ActiveCount != 0)
            throw new InvalidDataException("Clear-wall PLM did not delete after its one-frame draw.");
    }

    private static void VerifyRuntimeAlreadyDefeatedIntegration(SuperMetroidAddressSpace bus)
    {
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();

        // Select room state `$D98A` and drive Botwoon_Init's defeated branch through the
        // same Bank80SystemState callback used by normal door loading.
        runtime.System.SetBossBits(areaIndex: 4, BossBits.AreaMiniBoss);
        runtime.LoadCartridgeRoomForDebug(RoomPointer, CameraX, CameraY);
        if (runtime.Plms.ActiveCount != 1 || runtime.Camera is null ||
            runtime.Camera.Scrolls.ReadStorage(0) != 1 ||
            runtime.Camera.Scrolls.ReadStorage(1) != 1)
        {
            throw new InvalidDataException(
                "Runtime did not consume Botwoon's $B797 publication during room load.");
        }

        // The first gameplay PLM pass draws the complete nine-block vertical air list; the
        // second consumes its timer-one delete. This also proves the runtime now supplies
        // the active scroll grid to the generalized instruction interpreter.
        runtime.StepFrame(0);
        RoomLevelData level = runtime.LevelData ?? throw new InvalidDataException(
            "Runtime Botwoon room lost level data.");
        for (int row = 0; row < 9; row++)
        {
            int blockIndex = level.GetBlockIndex(15, 4 + row);
            if (level.GetCollisionBlockByIndex(blockIndex).LevelWord != 0x00ff)
                throw new InvalidDataException($"Runtime clear-wall row {row} remained solid.");
        }
        runtime.StepFrame(0);
        if (runtime.Plms.ActiveCount != 0)
            throw new InvalidDataException("Runtime did not retire Botwoon's clear-wall PLM.");
    }

    private static LoadedBotwoon Load(
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
            XPosition = 32,
            YPosition = 32,
            Pose = SamusState.FacingRightNormalPose,
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

        RoomEnemySlot head = enemies.Slots[0];
        BotwoonEnemyState state = enemies.Botwoon ?? throw new InvalidDataException(
            "Botwoon load produced no typed state.");
        return new LoadedBotwoon(
            enemies,
            samus,
            head,
            state,
            () => miniBossBitWasSet);
    }

    private static void StepEnemies(LoadedBotwoon loaded, RoomLevelData level)
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

    private static void AdvanceUntilShootable(LoadedBotwoon loaded, RoomLevelData level)
    {
        // Projectile and bomb collision consume the active-enemy index list selected by
        // EnemyMain, not the raw population array. Advance through the initial wait until
        // the head has both a concrete map and a scheduler-published interactive record.
        for (int frame = 0;
            frame < 4000 &&
                (loaded.Head.SpritemapPointer is 0 or 0x8000 or 0x804d ||
                 !loaded.Enemies.InteractiveEnemyIndexes.Contains(loaded.Head.NativeIndex));
            frame++)
        {
            StepEnemies(loaded, level);
        }
        if (loaded.Head.SpritemapPointer is 0 or 0x8000 or 0x804d ||
            !loaded.Enemies.InteractiveEnemyIndexes.Contains(loaded.Head.NativeIndex))
        {
            throw new InvalidDataException("Botwoon never exposed a shootable head spritemap.");
        }
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
        projectile.Direction = (ushort)SamusProjectileDirection.Right;
        projectile.XPosition = target.XPosition;
        projectile.YPosition = target.YPosition;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private readonly record struct LoadedBotwoon(
        RoomEnemySystem Enemies,
        SamusState Samus,
        RoomEnemySlot Head,
        BotwoonEnemyState State,
        Func<bool> ReadMiniBossBit)
    {
        public bool MiniBossBitWasSet => ReadMiniBossBit();
    }
}
