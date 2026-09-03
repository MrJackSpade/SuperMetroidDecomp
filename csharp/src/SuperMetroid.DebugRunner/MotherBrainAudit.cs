using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Untouched-ROM audit of Mother Brain's retail two-record load, first-phase animation, and
/// custom draw hook. The event callback remains clear, so every observed steady-state frame
/// follows the same pre-glass-destruction branch the cartridge executes on a fresh save.
/// </summary>
internal static class MotherBrainAudit
{
    private const ushort RoomPointer = 0xdd58;
    private const ushort PopulationPointer = 0xe321;
    private const ushort BodyDefinition = 0xec7f;
    private const ushort HeadDefinition = 0xec3f;
    private const ushort InitialHeadSpritemap = 0xa586;
    private const ushort TurretDefinition = 0xc17e;
    private const ushort TurretBulletDefinition = 0xc18c;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, RoomPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        if (room.WidthInScreens != 4 || room.HeightInScreens != 1 || room.AreaIndex != 5 ||
            room.State.EnemyPopulationPointer != PopulationPointer)
        {
            throw new InvalidDataException(
                $"Mother Brain room mismatch: {room.WidthInScreens}x{room.HeightInScreens}, " +
                $"area={room.AreaIndex}, population=$A1:{room.State.EnemyPopulationPointer:X4}.");
        }

        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState(0x1234);
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            XPosition = 0x0080,
            YPosition = 0x00a0,
            Pose = SamusPoseIds.FacingRightNormalPose,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);

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
            isAreaBossDefeated: () => false,
            hasEvent: _ => false);

        MotherBrainEnemyState state = enemies.MotherBrain ??
            throw new InvalidDataException("Mother Brain room did not allocate typed encounter state.");
        RoomEnemySlot body = state.Body;
        RoomEnemySlot head = state.Head ??
            throw new InvalidDataException("Mother Brain's head record was not linked to its body.");
        if (enemies.EnemyCount != 6 || body.EnemyDefinitionPointer != BodyDefinition ||
            head.EnemyDefinitionPointer != HeadDefinition || body.SlotIndex != 0 || head.SlotIndex != 1)
        {
            throw new InvalidDataException(
                $"Mother Brain population mismatch: count={enemies.EnemyCount}, " +
                $"slots=${body.EnemyDefinitionPointer:X4}/${head.EnemyDefinitionPointer:X4}.");
        }

        MotherBrainCorpseRotEntry firstRotEntry = MotherBrainCorpseRottingState.ReadEntry(bus, 0);
        MotherBrainCorpseRotEntry lastRotEntry =
            MotherBrainCorpseRottingState.ReadEntry(bus, MotherBrainCorpseRottingState.EntryCount - 1);
        bool turretParametersMatch = state.InitialTurretParameters.Count == 12;
        for (int index = 0; index < state.InitialTurretParameters.Count; index++)
            turretParametersMatch &= state.InitialTurretParameters[index] == index;
        AuditInitialTurretPool(bus, enemies);

        if (body.XPosition != 0x0081 || body.YPosition != 0x006f ||
            head.XPosition != 0x0081 || head.YPosition != 0x006f || head.Health != 0x0bb8 ||
            body.CurrentInstruction != 0x9c13 || head.CurrentInstruction != 0x9c21 ||
            body.Properties != 0x3d00 || head.Properties != 0x3900 ||
            body.PaletteIndex != 0 || head.PaletteIndex != 0x0200 ||
            body.VramTilesIndex != 0 || head.VramTilesIndex != 0 ||
            state.Form != 0 || state.HitboxesEnabled != 2 || state.EnableUnpauseHook ||
            state.Function != MotherBrainBodyFunction.FirstPhase ||
            state.BrainFunction != MotherBrainBrainFunction.SetupBrainToBeDrawn ||
            state.FxEntry != 1 || !state.BackgroundTilemapPrepared ||
            state.NeckPaletteIndex != 0x0200 || state.BrainPaletteIndex != 0x0200 ||
            state.BrainPaletteTimer != 10 || !state.CorpseRotting.IsInitialized ||
            firstRotEntry.YOffset != 47 || firstRotEntry.Timer != 0 ||
            lastRotEntry.YOffset != 0 || lastRotEntry.Timer != 94 || !turretParametersMatch ||
            vram.ReadWord(0x4800) != 0x0338 || vram.ReadWord(0x4fff) != 0x0338)
        {
            throw new InvalidDataException(
                $"Mother Brain initialization mismatch: body=({body.XPosition:X4},{body.YPosition:X4})/" +
                $"${body.Properties:X4}, head=({head.XPosition:X4},{head.YPosition:X4})/" +
                $"hp={head.Health}/${head.Properties:X4}, lists={body.CurrentInstruction:X4}/" +
                $"{head.CurrentInstruction:X4}, function=$A9:{(ushort)state.Function:X4}, " +
                $"corpse={state.CorpseRotting.IsInitialized}, turrets={state.InitialTurretParameters.Count}.");
        }

        // Compare every copied palette word with its ROM source, including both endpoints;
        // this catches the easy-to-miss +2 source offset and byte-index/color-index mismatch.
        for (int color = 0; color < 15; color++)
        {
            ushort expectedGlass = ReadWord(bus, 0xa99514 + color * 2);
            ushort expectedTube = ReadWord(bus, 0xa994f4 + color * 2);
            if (cgram.Colors[177 + color] != expectedGlass || cgram.Colors[241 + color] != expectedTube)
            {
                throw new InvalidDataException(
                    $"Mother Brain palette copy diverged at color {color}: " +
                    $"glass=${cgram.Colors[177 + color]:X4}/${expectedGlass:X4}, " +
                    $"tube=${cgram.Colors[241 + color]:X4}/${expectedTube:X4}.");
            }
        }

        var observedHeadMaps = new HashSet<ushort>();
        for (int frame = 0; frame < 20; frame++)
        {
            enemies.StepFrame(
                cameraX: 0,
                cameraY: 0,
                timeIsFrozen: false,
                samus,
                level: assets.LevelData,
                nmiFrameCounter8: unchecked((byte)frame));
            observedHeadMaps.Add(head.SpritemapPointer);

            var oam = new OamBuffer();
            oam.BeginFrame();
            enemies.DrawLayers(oam, 0, 0, firstLayer: 5, lastLayer: 5);
            ushort authoredEntryCount = ReadWord(bus, 0xa90000 | head.SpritemapPointer);
            if (head.SpritemapPointer != InitialHeadSpritemap ||
                oam.NextByteOffset / 4 != authoredEntryCount)
            {
                throw new InvalidDataException(
                    $"Mother Brain draw hook mismatch on frame {frame}: map=${head.SpritemapPointer:X4}, " +
                    $"OAM={oam.NextByteOffset / 4}, authored={authoredEntryCount}.");
            }
        }

        if (observedHeadMaps.Count != 1 || !observedHeadMaps.Contains(InitialHeadSpritemap) ||
            !state.DrawBrain || state.Form != 0 ||
            state.Function != MotherBrainBodyFunction.FirstPhase || state.DeleteTurretsAndRinkas)
        {
            throw new InvalidDataException(
                $"Mother Brain phase-one loop mutated unexpectedly: maps={observedHeadMaps.Count}, " +
                $"draw={state.DrawBrain}, form={state.Form}, function=$A9:{(ushort)state.Function:X4}.");
        }

        AuditHeadSpinTouch(bus, enemies, samus, head);
        AuditTurretRuntime(bus, assets.LevelData, enemies, samus);
        AuditGlassSequence(bus, room);
        AuditFakeDeathSequence(bus, room);

        Console.WriteLine(
            "Mother Brain audit passed: retail room $DD58 loaded six physical records; " +
            "body/head initialization, BG2 clear, two palette slices, corpse-rot seed, " +
            "twelve shared-pool turrets, rotating/firing turret bytecode, bullet movement, " +
            "terrain/contact behavior, looping head bytecode, the custom ordinary-" +
            "spritemap draw hook, asymmetric Samus collision, loaded glass PLM, missile " +
            "gating, threshold bytecode, shard actors, fake-death pauses/palettes, ordered " +
            "tube PLMs, four ceiling tubes, five physical tube actors, phase-two DMA, " +
            "uncrouch movement, neck geometry, stretching projectiles, phase-two attack " +
            "selection/cooldown, four aimed onion rings, custom ring damage, and the " +
            "body/head/shared-projectile laser, bomb, recursive hand-beam, and complete " +
            "live rainbow-beam/Baby cycles through a produced Hyper Beam recoil " +
            "matched the untouched cartridge.");
        return 0;
    }

    /// <summary>
    /// Exercises head touch `$A9:B5C6` through the public collision walker. The callback is
    /// intentionally harmless to standing Samus and uses the previous flash parity—not a
    /// host animation clock—to select 13 or 14 while spin jumping.
    /// </summary>
    private static void AuditHeadSpinTouch(
        SuperMetroidAddressSpace bus,
        RoomEnemySystem enemies,
        SamusState samus,
        RoomEnemySlot head)
    {
        var savedPositions = enemies.Slots
            .Select(slot => (slot.XPosition, slot.YPosition))
            .ToArray();
        for (int index = 0; index < enemies.EnemyCount; index++)
        {
            if (index == head.SlotIndex)
                continue;
            enemies.Slots[index].XPosition = unchecked((ushort)(head.XPosition + 0x4000));
            enemies.Slots[index].YPosition = unchecked((ushort)(head.YPosition + 0x4000));
        }

        samus.XPosition = head.XPosition;
        samus.YPosition = head.YPosition;
        samus.InvincibilityTimer = 0;
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        head.FlashTimer = 0;
        if (!enemies.ResolveOrdinarySamusContact(samus, controllerInput: 0) ||
            head.FlashTimer != 0)
        {
            throw new InvalidDataException(
                $"Mother Brain standing head touch changed flash to {head.FlashTimer}.");
        }

        samus.Pose = SamusPoseIds.SpinJumpRightPose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        if (!enemies.ResolveOrdinarySamusContact(samus, controllerInput: 0) ||
            head.FlashTimer != 13)
        {
            throw new InvalidDataException(
                $"Mother Brain spin head touch did not select 13 from zero: {head.FlashTimer}.");
        }
        if (!enemies.ResolveOrdinarySamusContact(samus, controllerInput: 0) ||
            head.FlashTimer != 14)
        {
            throw new InvalidDataException(
                $"Mother Brain spin head touch did not select 14 from odd: {head.FlashTimer}.");
        }
        if (!enemies.ResolveOrdinarySamusContact(samus, controllerInput: 0) ||
            head.FlashTimer != 13)
        {
            throw new InvalidDataException(
                $"Mother Brain spin head touch did not select 13 from even: {head.FlashTimer}.");
        }

        for (int index = 0; index < enemies.EnemyCount; index++)
        {
            enemies.Slots[index].XPosition = savedPositions[index].XPosition;
            enemies.Slots[index].YPosition = savedPositions[index].YPosition;
        }
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.XPosition = 0x0080;
        samus.YPosition = 0x00a0;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        head.FlashTimer = 0;
    }

    /// <summary>
    /// Drives the real first-phase exit through phase-two graphics setup. This is an
    /// encounter audit rather than a direct state mutation: event two and zero head health
    /// are the cartridge's entry conditions, after which every body/head/projectile/PLM
    /// frame runs through the same public schedulers used by gameplay.
    /// </summary>
    private static void AuditFakeDeathSequence(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room)
    {
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState(0x1234);
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            XPosition = 32,
            YPosition = 220,
            Pose = SamusPoseIds.FacingRightNormalPose,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);

        byte[] scrollBytes = new byte[50];
        scrollBytes[0] = 2;
        scrollBytes[1] = 1;
        ushort observedLayerBlendingConfig = 0;
        ushort observedBg2X = 0;
        ushort observedBg2Y = 0;
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
            isAreaBossDefeated: () => false,
            hasEvent: eventNumber => eventNumber == 2,
            setRoomScrollByte: (index, value) => scrollBytes[index] = value,
            readRoomScrollByte: index => scrollBytes[index],
            setMotherBrainLayerBlendingDefaultConfig:
                value => observedLayerBlendingConfig = value,
            setMotherBrainBg2Scroll: (x, y) =>
            {
                observedBg2X = x;
                observedBg2Y = y;
            });

        MotherBrainEnemyState state = enemies.MotherBrain ?? throw new InvalidDataException(
            "Mother Brain fake-death audit lost the encounter state.");
        state.Head!.Health = 0;

        var plms = new RoomPlmSystem();
        BackgroundTilemapStreamer streamer =
            assets.LevelData.CreateBackgroundStreamer(sizeOfBg2: 0x0800);
        var observedHeaders = new List<ushort>();
        var observedMusic = new List<ushort>();
        var observedCeilingTubes = new HashSet<RoomEnemyProjectileKind>();
        bool observedLockedInput = false;
        bool observedUnlockAfterLock = false;
        bool reachedPhaseTwoSetup = false;

        for (int frame = 0; frame < 2048; frame++)
        {
            enemies.StepFrame(
                cameraX: 0,
                cameraY: 0,
                timeIsFrozen: false,
                samus,
                level: assets.LevelData,
                nmiFrameCounter8: unchecked((byte)frame));

            observedLockedInput |= samus.InputLocked;
            observedUnlockAfterLock |= observedLockedInput && !samus.InputLocked;
            foreach (MotherBrainMusicRequest request in state.MusicRequests)
                observedMusic.Add(request.RawTrack);
            foreach (MotherBrainPlmRequest request in state.PlmRequests)
            {
                observedHeaders.Add(request.Header);
                if (!plms.TrySpawnMotherBrainMutation(
                        assets.LevelData,
                        request.BlockX,
                        request.BlockY,
                        request.Header))
                {
                    throw new InvalidDataException(
                        $"Mother Brain fake-death PLM pool filled on frame {frame}.");
                }
            }

            enemies.StepEnemyProjectiles(
                assets.LevelData,
                samus: null,
                cameraX: 0,
                cameraY: 0,
                nmiFrameCounter8: unchecked((byte)frame));
            plms.Step(bus, assets.LevelData, streamer, 0, 0, 0, assets.Scrolls);

            foreach (RoomEnemyProjectileSlot projectile in enemies.EnemyProjectiles)
            {
                if (projectile.Kind is
                    RoomEnemyProjectileKind.MotherBrainTopRightTube or
                    RoomEnemyProjectileKind.MotherBrainTopLeftTube or
                    RoomEnemyProjectileKind.MotherBrainTopMiddleLeftTube or
                    RoomEnemyProjectileKind.MotherBrainTopMiddleRightTube)
                {
                    observedCeilingTubes.Add(projectile.Kind);
                }
            }

            if (state.Function == MotherBrainBodyFunction.FakeDeathAscentSetupPhase2Brain)
            {
                reachedPhaseTwoSetup = true;
                break;
            }
        }

        ushort[] expectedHeaders =
        [
            0xb673, 0xb673, 0xb6b3,
            0xb6c3, 0xb6b3, 0xb6b3, 0xb6c7, 0xb6bb,
            0xb6b7, 0xb6b7, 0xb6bb, 0xb6bf,
            0xb67b, 0xb67f, 0xb683, 0xb687, 0xb68b, 0xb68f,
            0xb693, 0xb697, 0xb69b, 0xb69f, 0xb6a3, 0xb6a7,
        ];
        if (!reachedPhaseTwoSetup || !observedLockedInput || !observedUnlockAfterLock ||
            scrollBytes[1] != scrollBytes[0] || state.SpawnedFallingTubeCount != 5 ||
            observedCeilingTubes.Count != 4 || !observedHeaders.SequenceEqual(expectedHeaders) ||
            !observedMusic.SequenceEqual(new ushort[] { 6, 0, 0xff21 }) ||
            state.RoomPaletteInstructionPointer != 0 || state.RoomPaletteInstructionTimer != 0 ||
            !state.EnableUnpauseHook || state.Head.YPosition != 196 ||
            state.Body.XPosition != 59 || state.Body.YPosition != 279)
        {
            throw new InvalidDataException(
                $"Mother Brain fake-death sequence diverged: reached={reachedPhaseTwoSetup}, " +
                $"lock/unlock={observedLockedInput}/{observedUnlockAfterLock}, " +
                $"scroll={scrollBytes[0]}/{scrollBytes[1]}, tubes={state.SpawnedFallingTubeCount}/" +
                $"{observedCeilingTubes.Count}, PLMs={observedHeaders.Count}, " +
                $"music={string.Join(',', observedMusic.Select(track => track.ToString("X4")))}, " +
                $"function=$A9:{(ushort)state.Function:X4}.");
        }

        // `$A9:8D11` copies colors 1..15 from the two phase-two ROM palettes. Checking all
        // words catches both the +2 source offset and the byte-to-color destination divide.
        for (int color = 0; color < 15; color++)
        {
            ushort expectedAttack = ReadWord(bus, 0xa994b4 + color * 2);
            ushort expectedBackLeg = ReadWord(bus, 0xa99494 + color * 2);
            if (cgram.Colors[161 + color] != expectedAttack ||
                cgram.Colors[177 + color] != expectedBackLeg)
            {
                throw new InvalidDataException(
                    $"Mother Brain phase-two palette diverged at color {color}: " +
                    $"attack=${cgram.Colors[161 + color]:X4}/${expectedAttack:X4}, " +
                    $"leg=${cgram.Colors[177 + color]:X4}/${expectedBackLeg:X4}.");
            }
        }

        // Continue from the public scheduler boundary reached above. This covers every
        // resurrection state, all twelve one-transfer-per-frame leg/attack DMA entries,
        // the slow ROM-authored uncrouch list, the from-gray palette table, articulated
        // neck placement, and the stretching head list's drool/purple-breath opcodes.
        bool observedRisingHdma = false;
        bool observedLargePurpleBreath = false;
        bool observedDrool = false;
        bool observedNeckOam = false;
        int phaseTwoFrames = 0;
        for (; phaseTwoFrames < 2048; phaseTwoFrames++)
        {
            byte frameCounter = unchecked((byte)(phaseTwoFrames + 1));
            enemies.StepFrame(
                cameraX: 0,
                cameraY: 0,
                timeIsFrozen: false,
                samus,
                level: assets.LevelData,
                nmiFrameCounter8: frameCounter);

            foreach (MotherBrainMusicRequest request in state.MusicRequests)
                observedMusic.Add(request.RawTrack);
            observedRisingHdma |= state.RisingHdmaActive;

            enemies.StepEnemyProjectiles(
                assets.LevelData,
                samus: null,
                cameraX: 0,
                cameraY: 0,
                nmiFrameCounter8: frameCounter);
            observedLargePurpleBreath |= enemies.EnemyProjectiles.Any(projectile =>
                projectile.Kind == RoomEnemyProjectileKind.MotherBrainPurpleBreathBig);
            observedDrool |= enemies.EnemyProjectiles.Any(projectile =>
                projectile.Kind is RoomEnemyProjectileKind.MotherBrainDrool or
                    RoomEnemyProjectileKind.MotherBrainDyingDrool);

            if (state.DrawNeck)
            {
                var oam = new OamBuffer();
                oam.BeginFrame();
                enemies.DrawLayers(oam, 0, 0, firstLayer: 0, lastLayer: 7);
                observedNeckOam |= oam.NextByteOffset >= 5 * 4;
            }

            // `$8F33` writes B605 but returns; stopping at that boundary avoids consuming
            // a random attack/walk choice that belongs to the next focused combat audit.
            if (state.Function == MotherBrainBodyFunction.SecondPhaseThinking)
                break;
        }

        if (phaseTwoFrames == 2048)
        {
            throw new InvalidDataException(
                $"Mother Brain resurrection did not reach phase-two thinking; " +
                $"function=$A9:{(ushort)state.Function:X4}, pose={state.Pose}.");
        }

        // Read the transfer descriptors themselves so this test remains tied to the retail
        // cartridge's source banks and destinations instead of duplicating their constants.
        int transferEntry = 0xa98f8f;
        for (int transfer = 0; transfer < 12; transfer++, transferEntry += 7)
        {
            ushort byteCount = ReadWord(bus, transferEntry);
            int source = bus.ReadByte(transferEntry + 2) |
                (bus.ReadByte(transferEntry + 3) << 8) |
                (bus.ReadByte(transferEntry + 4) << 16);
            ushort destinationWord = ReadWord(bus, transferEntry + 5);
            if (byteCount != 0x0200)
            {
                throw new InvalidDataException(
                    $"Mother Brain transfer {transfer} has unexpected size ${byteCount:X4}.");
            }
            for (int byteIndex = 0; byteIndex < byteCount; byteIndex++)
            {
                byte expected = bus.ReadByte(source + byteIndex);
                byte actual = vram.ReadByte(destinationWord * 2 + byteIndex);
                if (actual != expected)
                {
                    throw new InvalidDataException(
                        $"Mother Brain transfer {transfer} diverged at byte ${byteIndex:X3}: " +
                        $"VRAM=${actual:X2}, ROM=${expected:X2}.");
                }
            }
        }

        MotherBrainNeckPoint finalHeadJoint = state.NeckSegment4;
        if (observedLayerBlendingConfig != 0x0034 ||
            !observedRisingHdma || state.RisingHdmaActive ||
            state.Body.XPosition != 0x003b || state.Body.YPosition != 0x0096 ||
            observedBg2X != 0xffe5 || observedBg2Y != 0xffa9 ||
            state.Bg2XScroll != observedBg2X || state.Bg2YScroll != observedBg2Y ||
            state.Pose != MotherBrainBodyPose.Standing || state.Form != 2 ||
            state.Head!.Health != 0x4650 || state.HitboxesEnabled != 7 ||
            state.Head.XPosition != finalHeadJoint.X ||
            state.Head.YPosition != unchecked((ushort)(finalHeadJoint.Y - 21)) ||
            !state.DrawBrain || !state.DrawNeck || !observedNeckOam ||
            !state.BrainPaletteHandlingEnabled || !state.DroolGenerationEnabled ||
            !state.SmallPurpleBreathGenerationEnabled ||
            !state.EnemyBg2TilemapTransferRequested ||
            state.EnemyBg2TilemapSize != 0x0140 ||
            !observedLargePurpleBreath || !observedDrool ||
            !observedMusic.SequenceEqual(new ushort[] { 6, 0, 0xff21, 5 }))
        {
            throw new InvalidDataException(
                $"Mother Brain resurrection diverged: frames={phaseTwoFrames}, " +
                $"blend=${observedLayerBlendingConfig:X4}, HDMA={observedRisingHdma}/" +
                $"{state.RisingHdmaActive}, body=({state.Body.XPosition:X4}," +
                $"{state.Body.YPosition:X4}), BG2=({observedBg2X:X4},{observedBg2Y:X4}), " +
                $"pose/form={state.Pose}/{state.Form}, head=({state.Head.XPosition:X4}," +
                $"{state.Head.YPosition:X4}) joint=({finalHeadJoint.X:X4}," +
                $"{finalHeadJoint.Y:X4}), projectiles={observedDrool}/" +
                $"{observedLargePurpleBreath}, function=$A9:{(ushort)state.Function:X4}.");
        }

        AuditPhaseTwoShotReactions(bus, assets.LevelData, enemies, state, samus);
        AuditPhaseTwoOnionRingAttack(
            bus,
            assets.LevelData,
            enemies,
            state,
            samus,
            random);
        AuditPhaseTwoLaserAttack(
            bus,
            assets.LevelData,
            enemies,
            state,
            samus,
            random);
        AuditPhaseTwoBombAttack(
            bus,
            assets.LevelData,
            enemies,
            state,
            samus,
            random);
        AuditPhaseTwoHandBeamAttack(
            bus,
            assets.LevelData,
            enemies,
            state,
            samus,
            random);
        AuditPhaseTwoRainbowBeamAttack(
            bus,
            assets.LevelData,
            enemies,
            state,
            samus,
            random,
            vram,
            cgram);
    }

    /// <summary>
    /// Exercises the public projectile collision walker against the resurrected head. This
    /// proves that `$B562`'s beam/missile walk-counter reaction occurs before the ordinary
    /// no-death damage tail and that zero health remains body-AI-owned.
    /// </summary>
    private static void AuditPhaseTwoShotReactions(
        SuperMetroidAddressSpace bus,
        RoomLevelData level,
        RoomEnemySystem enemies,
        MotherBrainEnemyState state,
        SamusState samus)
    {
        RoomEnemySlot head = state.Head ?? throw new InvalidDataException(
            "Mother Brain phase-two shot audit lost the linked head record.");
        var projectiles = new SamusProjectileSystem();
        var sharedProjectiles = new SamusBombProjectileSystem();

        AuditShot(SamusProjectileFamily.Beam, type: 0x8000, damage: 20, walkBefore: 0x0200,
            expectedWalkAfter: 0x0100);
        AuditShot(SamusProjectileFamily.Missile, type: 0x8100, damage: 100, walkBefore: 0x0200,
            expectedWalkAfter: 0);
        AuditNormalBombMultiboxBoundary();

        void AuditNormalBombMultiboxBoundary()
        {
            projectiles.Reset();
            sharedProjectiles.Reset();
            SamusBombProjectileSlot bomb =
                EnemyProjectileAuditAssertions.ArmExplodingNormalBomb(
                    sharedProjectiles,
                    head.XPosition,
                    head.YPosition,
                    damage: 100);
            // Mother Brain's live phase-two head publishes an ordinary drawing spritemap at
            // `$A9:A5F8` while its population still carries extra-property `$0004`. Native
            // bomb dispatch therefore takes the multibox walker, not the header-radius path.
            // A bomb at the nominal head origin must not be promoted into a `$B507` callback
            // merely because the host's ordinary projectile compatibility path uses radii.
            bomb.XRadius = 1;
            bomb.YRadius = 1;
            ushort healthBefore = head.Health;
            state.WalkCounter = 0x0200;

            int hits = enemies.ResolveOrdinaryBombHits(
                sharedProjectiles,
                projectiles,
                samus);
            if (hits != 0 || (bomb.Direction & 0x0010) != 0 ||
                head.Health != healthBefore || state.WalkCounter != 0x0200 ||
                head.Properties.HasAny(EnemyProperties.Deleted))
            {
                throw new InvalidDataException(
                    $"Mother Brain phase-two normal-bomb boundary diverged: hits={hits}, " +
                    $"direction=${bomb.Direction:X4}, health={head.Health}/{healthBefore}, " +
                    $"walk=${state.WalkCounter:X4}/0200, " +
                    $"map=$A9:{head.SpritemapPointer:X4}, " +
                    $"extra=${head.ExtraProperties:X4}, " +
                    $"deleted={head.Properties.HasAny(EnemyProperties.Deleted)}.");
            }
        }

        void AuditShot(
            SamusProjectileFamily expectedFamily,
            ushort type,
            ushort damage,
            ushort walkBefore,
            ushort expectedWalkAfter)
        {
            projectiles.Reset();
            sharedProjectiles.Reset();
            samus.SelectedHudItem = expectedFamily == SamusProjectileFamily.Missile
                ? (ushort)1
                : (ushort)0;
            samus.Missiles = 99;
            samus.EquippedBeams = 0;
            const ushort shoot = (ushort)SnesButton.X;
            SamusProjectileFrameResult produced = projectiles.StepFrame(
                bus,
                level,
                samus,
                controllerInput: shoot,
                controllerNewInput: shoot,
                layer1X: 0,
                layer1Y: 0,
                sharedProjectiles);
            if (produced.FiredSlot is not int slotIndex)
            {
                throw new InvalidDataException(
                    $"Could not produce {expectedFamily} for Mother Brain phase-two audit.");
            }

            SamusProjectileSlot shot = projectiles.Slots[slotIndex];
            shot.Type = type;
            shot.Damage = damage;
            shot.Direction = (ushort)SamusProjectileDirection.Right;
            if (shot.InstructionPointer == 0)
                shot.InstructionPointer = 1;
            shot.XPosition = head.XPosition;
            shot.YPosition = head.YPosition;
            shot.XRadius = 1;
            shot.YRadius = 1;

            ushort healthBefore = head.Health;
            state.WalkCounter = walkBefore;
            int hits = enemies.ResolveOrdinaryProjectileHits(
                bus,
                projectiles,
                sharedProjectiles,
                samus);
            ushort vulnerabilityPointer = head.Definition.VulnerabilityPointer != 0
                ? head.Definition.VulnerabilityPointer
                : (ushort)0xec1c;
            int vulnerabilityOffset = expectedFamily == SamusProjectileFamily.Beam ? 0 : 12;
            byte vulnerability = bus.ReadByte(
                0xb40000 | unchecked((ushort)(vulnerabilityPointer + vulnerabilityOffset)));
            int expectedDamage = (damage >> 1) * (vulnerability & 0x7f);
            ushort expectedHealth = expectedDamage >= healthBefore
                ? (ushort)0
                : unchecked((ushort)(healthBefore - expectedDamage));

            SamusProjectileFamily expectedImpactFamily =
                expectedFamily == SamusProjectileFamily.Beam
                    ? SamusProjectileFamily.BeamExplosion
                    : SamusProjectileFamily.MissileExplosion;
            if (hits != 1 || shot.PackedType.Family != expectedImpactFamily ||
                head.Health != expectedHealth || state.WalkCounter != expectedWalkAfter ||
                head.Properties.HasAny(EnemyProperties.Deleted))
            {
                throw new InvalidDataException(
                    $"Mother Brain phase-two {expectedFamily} reaction diverged: hits={hits}, " +
                    $"family=${shot.PackedType.FamilyValue:X3}, health={head.Health}/" +
                    $"{expectedHealth}, walk=${state.WalkCounter:X4}/${expectedWalkAfter:X4}, " +
                    $"deleted={head.Properties.HasAny(EnemyProperties.Deleted)}.");
            }
        }
    }

    /// <summary>
    /// Forces the cartridge's grounded/default `$50` random-byte route: thinking installs
    /// `$B64B`, phase zero selects head list `$9D3D`, and that ordinary list emits four
    /// independently aimed `$86:CB4B` actors before the 64-frame body cooldown expires.
    /// </summary>
    private static void AuditPhaseTwoOnionRingAttack(
        SuperMetroidAddressSpace bus,
        RoomLevelData level,
        RoomEnemySystem enemies,
        MotherBrainEnemyState state,
        SamusState samus,
        Bank80SystemState random)
    {
        RoomEnemySlot head = state.Head ?? throw new InvalidDataException(
            "Mother Brain onion-ring audit lost the linked head record.");

        // Current random is read without advancing by both `$B605` and `$B65A`. Low byte
        // $50 lies between the default $40/$80 thresholds and therefore selects rings.
        random.SetRandomNumber(0x0050);
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.RefreshCollisionRadii(bus);
        samus.XPosition = 0x0020;
        samus.YPosition = 0x00dc;
        samus.EquippedItems = 0;
        samus.Health = 999;
        samus.InvincibilityTimer = 0;
        samus.KnockbackTimer = 0;

        bool[] ringWasActive = new bool[enemies.EnemyProjectiles.Count];
        bool observedAttackDispatcher = false;
        bool observedCooldown = false;
        bool observedDisabledNeck = false;
        bool observedReenabledNeck = false;
        bool observedRingSound = false;
        bool observedCollisionSound = false;
        bool observedSamusHit = false;
        int spawnCount = 0;
        int frames = 0;

        for (; frames < 160; frames++)
        {
            enemies.StepFrame(
                cameraX: 0,
                cameraY: 0,
                timeIsFrozen: false,
                samus,
                level: level,
                nmiFrameCounter8: unchecked((byte)frames));

            observedAttackDispatcher |=
                state.Function == MotherBrainBodyFunction.SecondPhaseTryAttack;
            observedCooldown |= state.AttackPhase == MotherBrainAttackPhase.Cooldown &&
                state.AttackCooldown != 0;
            observedDisabledNeck |= !state.NeckMovementEnabled;
            observedReenabledNeck |= observedDisabledNeck && state.NeckMovementEnabled;
            observedRingSound |= state.LastSoundEffectLibrary3 == 0x0017;

            RoomEnemyProjectileSlot? collisionCandidate = null;
            ushort expectedKnockbackDirection = 0;
            foreach (RoomEnemyProjectileSlot ring in enemies.EnemyProjectiles.Where(
                         projectile =>
                             projectile.Kind == RoomEnemyProjectileKind.MotherBrainOnionRing))
            {
                if (!ringWasActive[ring.SlotIndex])
                {
                    spawnCount++;
                    byte expectedAngle = CalculateMotherBrainOnionRingAngleReference(
                        unchecked((short)(samus.XPosition - head.XPosition - 0x000a)),
                        unchecked((short)(samus.YPosition - head.YPosition - 0x0010)));
                    ushort expectedXVelocity = MultiplyMotherBrainSineReference(
                        bus,
                        0x0450,
                        expectedAngle);
                    ushort expectedYVelocity = MultiplyMotherBrainSineReference(
                        bus,
                        0x0450,
                        unchecked((byte)(expectedAngle + 0x40)));
                    if (ring.PreInstruction != 0xc335 ||
                        ring.InstructionPointer != 0xc432 || ring.InstructionTimer != 1 ||
                        ring.SpritemapPointer != 0x8000 || ring.GraphicsIndex != 0x0400 ||
                        ring.XRadius != 6 || ring.YRadius != 6 || ring.Damage != 0x0050 ||
                        ring.Variable0 != 8 || ring.Variable1 != 0 ||
                        ring.DirectionParameter != expectedAngle ||
                        ring.XVelocity != expectedXVelocity || ring.YVelocity != expectedYVelocity ||
                        ring.XPosition != unchecked((ushort)(head.XPosition + 0x000a)) ||
                        ring.YPosition != unchecked((ushort)(head.YPosition + 0x0010)) ||
                        ring.CanDamageSamus || ring.PersistsOnSamusContact ||
                        ring.BlocksSamusProjectiles)
                    {
                        throw new InvalidDataException(
                            $"Mother Brain onion ring {spawnCount} initialization diverged: " +
                            $"slot={ring.SlotIndex}, angle=${ring.DirectionParameter:X2}/" +
                            $"${expectedAngle:X2}, velocity=({ring.XVelocity:X4}," +
                            $"{ring.YVelocity:X4})/({expectedXVelocity:X4}," +
                            $"{expectedYVelocity:X4}), pos=({ring.XPosition:X4}," +
                            $"{ring.YPosition:X4}), list=${ring.InstructionPointer:X4}." );
                    }
                }

                // Let the real eight delayed pre-instruction calls expire. On the first
                // naturally active call, place Samus at the exact next 8.8 coordinate and
                // retain a nonzero invincibility timer to prove this private path ignores it.
                if (!observedSamusHit && collisionCandidate is null && ring.Variable0 == 0)
                {
                    collisionCandidate = ring;
                    (ushort targetX, _) = AddEightBitVelocityReference(
                        ring.XPosition,
                        ring.XSubposition,
                        ring.XVelocity);
                    (ushort targetY, _) = AddEightBitVelocityReference(
                        ring.YPosition,
                        ring.YSubposition,
                        ring.YVelocity);
                    samus.XPosition = targetX;
                    samus.YPosition = targetY;
                    samus.Health = 999;
                    samus.InvincibilityTimer = 7;
                    samus.KnockbackTimer = 0;
                    // Samus and the moved ring share X exactly. `$C373` treats equality as
                    // the nonnegative/right branch and therefore writes direction one.
                    expectedKnockbackDirection = 1;
                }
            }

            enemies.StepEnemyProjectiles(
                level,
                samus,
                cameraX: 0,
                cameraY: 0,
                nmiFrameCounter8: unchecked((byte)frames));

            if (collisionCandidate is not null && !observedSamusHit)
            {
                if (samus.Health != 919 || samus.InvincibilityTimer != 0x0060 ||
                    samus.KnockbackTimer != 5 ||
                    samus.KnockbackXDirection != expectedKnockbackDirection)
                {
                    throw new InvalidDataException(
                        $"Mother Brain onion-ring Samus collision diverged: health=" +
                        $"{samus.Health}/919, invincibility={samus.InvincibilityTimer:X4}/" +
                        $"0060, knockback={samus.KnockbackTimer}/" +
                        $"{samus.KnockbackXDirection}.");
                }
                observedSamusHit = true;
                observedCollisionSound |= state.LastSoundEffectLibrary3 == 0x0013;

                // Keep later rings alive long enough to prove four distinct spawn opcodes;
                // the custom ring path ignores invincibility, so distance is the real gate.
                samus.XPosition = 0x0300;
                samus.YPosition = 0x0300;
            }

            for (int slotIndex = 0; slotIndex < ringWasActive.Length; slotIndex++)
            {
                ringWasActive[slotIndex] = enemies.EnemyProjectiles[slotIndex].Kind ==
                    RoomEnemyProjectileKind.MotherBrainOnionRing;
            }

            if (spawnCount == 4 && observedSamusHit &&
                state.Function == MotherBrainBodyFunction.SecondPhaseThinking &&
                state.AttackPhase == MotherBrainAttackPhase.ChooseAttack)
            {
                break;
            }
        }

        if (frames == 160 || spawnCount != 4 || !observedAttackDispatcher ||
            !observedCooldown || !observedDisabledNeck || !observedReenabledNeck ||
            !observedRingSound || !observedCollisionSound || !observedSamusHit ||
            state.AttackCooldown != 0 || head.CurrentInstruction < 0x9c87 ||
            head.CurrentInstruction > 0x9cab)
        {
            throw new InvalidDataException(
                $"Mother Brain phase-two onion-ring cycle diverged: frames={frames}, " +
                $"spawns={spawnCount}, function=$A9:{(ushort)state.Function:X4}, " +
                $"phase/cooldown={state.AttackPhase}/{state.AttackCooldown}, neck=" +
                $"{observedDisabledNeck}/{observedReenabledNeck}, sounds=" +
                $"{observedRingSound}/{observedCollisionSound}, hit={observedSamusHit}, " +
                $"headList=${head.CurrentInstruction:X4}.");
        }
    }

    /// <summary>
    /// Forces the airborne low-random route through <c>$A9:B80E/$B839/$B863</c>, then lets
    /// the ordinary head interpreter reach opcode <c>$9F46</c> and the shared bank-$86
    /// projectile interpreter advance laser definition <c>$A17B</c>. This guards the seams
    /// between three independently scheduled actors rather than validating a host shortcut.
    /// </summary>
    private static void AuditPhaseTwoLaserAttack(
        SuperMetroidAddressSpace bus,
        RoomLevelData level,
        RoomEnemySystem enemies,
        MotherBrainEnemyState state,
        SamusState samus,
        Bank80SystemState random)
    {
        RoomEnemySlot head = state.Head ?? throw new InvalidDataException(
            "Mother Brain laser audit lost the linked head record.");

        // Movement type three enters the cartridge's airborne strategy. Low random byte
        // $50 chooses laser rather than rings, while the full word remains below $1000 so
        // the preceding thinking state elects to attack on its next ordinary frame.
        random.SetRandomNumber(0x0050);
        samus.Pose = SamusPoseIds.SpinJumpRightPose;
        samus.RefreshCollisionRadii(bus);
        samus.XPosition = 0x0020;
        samus.YPosition = 0x00dc;
        samus.Health = 999;
        samus.InvincibilityTimer = 0;
        samus.KnockbackTimer = 0;

        enemies.StepFrame(
            cameraX: 0,
            cameraY: 0,
            timeIsFrozen: false,
            samus,
            level: level,
            nmiFrameCounter8: 0);
        if (state.Function != MotherBrainBodyFunction.SecondPhaseTryAttack)
        {
            throw new InvalidDataException(
                $"Mother Brain airborne laser setup did not enter $B64B: " +
                $"function=$A9:{(ushort)state.Function:X4}.");
        }

        // Predict the head slot's same-frame neck update after body state $B80E writes its
        // two target indices. This prevents a looser test from accepting a correct body
        // function pointer paired with incorrect articulation or a one-frame scheduling lag.
        ushort expectedLowerAngle = state.LowerNeckAngle;
        ushort expectedUpperAngle = state.UpperNeckAngle;
        ushort headMinusSamus = unchecked((ushort)(head.YPosition - samus.YPosition));
        ushort expectedTargetIndex = (headMinusSamus & 0x8000) == 0
            ? (ushort)8
            : (ushort)6;
        ushort expectedLowerIndex = expectedTargetIndex;
        ushort expectedUpperIndex = expectedTargetIndex;
        MotherBrainNeckKinematics.StepAngles(
            ref expectedLowerAngle,
            ref expectedUpperAngle,
            ref expectedLowerIndex,
            ref expectedUpperIndex,
            angleDelta: 0x0200,
            brainY: head.YPosition,
            samusY: samus.YPosition);

        enemies.StepFrame(
            cameraX: 0,
            cameraY: 0,
            timeIsFrozen: false,
            samus,
            level: level,
            nmiFrameCounter8: 1);
        if (state.Function !=
                MotherBrainBodyFunction.SecondPhaseLaserPositionHeadSlowlyAndFire ||
            state.FunctionTimer != 4 || state.NeckAngleDelta != 0x0200 ||
            state.LowerNeckAngle != expectedLowerAngle ||
            state.UpperNeckAngle != expectedUpperAngle ||
            state.LowerNeckMovementIndex != expectedLowerIndex ||
            state.UpperNeckMovementIndex != expectedUpperIndex ||
            state.AttackPhase != MotherBrainAttackPhase.Cooldown ||
            state.AttackCooldown != 0x0040)
        {
            throw new InvalidDataException(
                $"Mother Brain quick laser positioning diverged: function/timer=" +
                $"$A9:{(ushort)state.Function:X4}/{state.FunctionTimer}, delta=" +
                $"${state.NeckAngleDelta:X4}, angles=({state.LowerNeckAngle:X4}," +
                $"{state.UpperNeckAngle:X4})/({expectedLowerAngle:X4}," +
                $"{expectedUpperAngle:X4}), indices=({state.LowerNeckMovementIndex}," +
                $"{state.UpperNeckMovementIndex})/({expectedLowerIndex}," +
                $"{expectedUpperIndex}), attack={state.AttackPhase}/{state.AttackCooldown}.");
        }

        // Once selection has committed, use a non-attack RNG word. `$B863` jumps into
        // thinking on its expiration frame, and this keeps that exact tail call observable
        // instead of immediately starting another attack from the retained cooldown phase.
        random.SetRandomNumber(0x5000);
        samus.XPosition = 0x0300;

        bool[] laserWasActive = new bool[enemies.EnemyProjectiles.Count];
        bool observedSlowFireState = false;
        bool observedBodyFinish = false;
        bool observedNeckDisabled = false;
        bool observedNeckReenabled = false;
        bool observedLaserSound = false;
        bool observedLaserMovement = false;
        int spawnCount = 0;
        int frames = 2;

        for (; frames < 128; frames++)
        {
            MotherBrainBodyFunction functionBefore = state.Function;
            ushort timerBefore = state.FunctionTimer;

            // `$B863` writes movement index four and jumps to thinking before the head slot
            // runs. Predict that following head update so the audit still checks the native
            // write even when the index immediately reaches/reverses at an angle boundary.
            bool finishingBodyThisFrame =
                functionBefore == MotherBrainBodyFunction.SecondPhaseLaserFinishAttack &&
                timerBefore == 0;
            ushort expectedFinishLowerAngle = state.LowerNeckAngle;
            ushort expectedFinishUpperAngle = state.UpperNeckAngle;
            ushort expectedFinishLowerIndex = 4;
            ushort expectedFinishUpperIndex = 4;
            ushort finishBrainY = head.YPosition;
            if (finishingBodyThisFrame)
            {
                MotherBrainNeckKinematics.StepAngles(
                    ref expectedFinishLowerAngle,
                    ref expectedFinishUpperAngle,
                    ref expectedFinishLowerIndex,
                    ref expectedFinishUpperIndex,
                    state.NeckAngleDelta,
                    finishBrainY,
                    samus.YPosition);
            }

            enemies.StepFrame(
                cameraX: 0,
                cameraY: 0,
                timeIsFrozen: false,
                samus,
                level: level,
                nmiFrameCounter8: unchecked((byte)frames));

            if (state.Function == MotherBrainBodyFunction.SecondPhaseLaserFinishAttack)
            {
                observedSlowFireState = true;
                if (state.NeckAngleDelta != 0x0100)
                {
                    throw new InvalidDataException(
                        $"Mother Brain laser slow-position delta was " +
                        $"${state.NeckAngleDelta:X4}, expected $0100.");
                }
            }

            if (finishingBodyThisFrame)
            {
                if (state.Function != MotherBrainBodyFunction.SecondPhaseThinking ||
                    state.LowerNeckAngle != expectedFinishLowerAngle ||
                    state.UpperNeckAngle != expectedFinishUpperAngle ||
                    state.LowerNeckMovementIndex != expectedFinishLowerIndex ||
                    state.UpperNeckMovementIndex != expectedFinishUpperIndex)
                {
                    throw new InvalidDataException(
                        $"Mother Brain laser finish/tail-call diverged: function=" +
                        $"$A9:{(ushort)state.Function:X4}, angles=({state.LowerNeckAngle:X4}," +
                        $"{state.UpperNeckAngle:X4})/({expectedFinishLowerAngle:X4}," +
                        $"{expectedFinishUpperAngle:X4}), indices=" +
                        $"({state.LowerNeckMovementIndex},{state.UpperNeckMovementIndex})/" +
                        $"({expectedFinishLowerIndex},{expectedFinishUpperIndex}).");
                }
                observedBodyFinish = true;
            }

            observedLaserSound |= state.LastSoundEffect == 0x0067;
            RoomEnemyProjectileSlot? movementCandidate = null;
            ushort movementCandidateX = 0;
            foreach (RoomEnemyProjectileSlot laser in enemies.EnemyProjectiles.Where(
                         projectile =>
                             projectile.Kind == RoomEnemyProjectileKind.PirateMotherBrainLaser))
            {
                if (!laserWasActive[laser.SlotIndex])
                {
                    spawnCount++;
                    if (laser.PreInstruction != 0xa05b ||
                        laser.InstructionPointer != 0x9f7d || laser.InstructionTimer != 1 ||
                        laser.SpritemapPointer != 0x8000 || laser.GraphicsIndex != 0 ||
                        laser.XRadius != 16 || laser.YRadius != 4 ||
                        laser.Damage != head.Definition.Damage ||
                        laser.Variable0 != head.Parameter1 || laser.DirectionParameter != 1 ||
                        laser.XPosition != unchecked((ushort)(head.XPosition + 0x0010)) ||
                        laser.YPosition != unchecked((ushort)(head.YPosition + 0x0004)) ||
                        !laser.CanDamageSamus || laser.PersistsOnSamusContact ||
                        laser.BlocksSamusProjectiles)
                    {
                        throw new InvalidDataException(
                            $"Mother Brain laser initialization diverged: slot=" +
                            $"{laser.SlotIndex}, pre/list/timer=${laser.PreInstruction:X4}/" +
                            $"${laser.InstructionPointer:X4}/{laser.InstructionTimer}, map/gfx=" +
                            $"${laser.SpritemapPointer:X4}/${laser.GraphicsIndex:X4}, radii=" +
                            $"{laser.XRadius}/{laser.YRadius}, damage={laser.Damage}/" +
                            $"{head.Definition.Damage}, var/dir=${laser.Variable0:X4}/" +
                            $"${laser.DirectionParameter:X4}, pos=({laser.XPosition:X4}," +
                            $"{laser.YPosition:X4}), flags={laser.CanDamageSamus}/" +
                            $"{laser.PersistsOnSamusContact}/{laser.BlocksSamusProjectiles}.");
                    }
                    observedNeckDisabled |= !state.NeckMovementEnabled;
                }

                movementCandidate ??= laser;
                movementCandidateX = laser.XPosition;
            }

            enemies.StepEnemyProjectiles(
                level,
                samus,
                cameraX: 0,
                cameraY: 0,
                nmiFrameCounter8: unchecked((byte)frames));

            if (movementCandidate?.IsActive == true &&
                movementCandidate.XPosition != movementCandidateX)
            {
                ushort expectedPixels = (movementCandidate.Variable0 & 0x8000) == 0
                    ? (ushort)4
                    : (ushort)2;
                ushort actualPixels = unchecked((ushort)(
                    movementCandidate.XPosition - movementCandidateX));
                if (actualPixels != expectedPixels || movementCandidate.PreInstruction != 0xa07a)
                {
                    throw new InvalidDataException(
                        $"Mother Brain laser movement diverged: delta={actualPixels}/" +
                        $"{expectedPixels}, pre=${movementCandidate.PreInstruction:X4}.");
                }
                observedLaserMovement = true;
            }

            observedNeckReenabled |= observedNeckDisabled && state.NeckMovementEnabled;
            for (int slotIndex = 0; slotIndex < laserWasActive.Length; slotIndex++)
            {
                laserWasActive[slotIndex] = enemies.EnemyProjectiles[slotIndex].Kind ==
                    RoomEnemyProjectileKind.PirateMotherBrainLaser;
            }

            if (observedSlowFireState && observedBodyFinish && spawnCount == 1 &&
                observedNeckDisabled && observedNeckReenabled && observedLaserSound &&
                observedLaserMovement && head.CurrentInstruction >= 0x9c87 &&
                head.CurrentInstruction <= 0x9cab)
            {
                break;
            }
        }

        if (frames == 128 || !observedSlowFireState || !observedBodyFinish ||
            spawnCount != 1 || !observedNeckDisabled || !observedNeckReenabled ||
            !observedLaserSound || !observedLaserMovement ||
            state.Function != MotherBrainBodyFunction.SecondPhaseThinking ||
            state.AttackPhase != MotherBrainAttackPhase.Cooldown ||
            state.AttackCooldown != 0x0040)
        {
            throw new InvalidDataException(
                $"Mother Brain phase-two laser cycle diverged: frames={frames}, " +
                $"function=$A9:{(ushort)state.Function:X4}, attack=" +
                $"{state.AttackPhase}/{state.AttackCooldown}, slow/finish=" +
                $"{observedSlowFireState}/{observedBodyFinish}, spawns={spawnCount}, neck=" +
                $"{observedNeckDisabled}/{observedNeckReenabled}, sound/movement=" +
                $"{observedLaserSound}/{observedLaserMovement}, headList=" +
                $"${head.CurrentInstruction:X4}.");
        }
    }

    /// <summary>
    /// Drives the retained attack-phase cooldown back to selection, forces the native bomb
    /// route, and follows body bytecode plus projectile physics through natural expiry. A
    /// second real head-opcode spawn then validates the separate Samus-bomb destruction path.
    /// </summary>
    private static void AuditPhaseTwoBombAttack(
        SuperMetroidAddressSpace bus,
        RoomLevelData level,
        RoomEnemySystem enemies,
        MotherBrainEnemyState state,
        SamusState samus,
        Bank80SystemState random)
    {
        RoomEnemySlot head = state.Head ?? throw new InvalidDataException(
            "Mother Brain bomb audit lost the linked head record.");
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.RefreshCollisionRadii(bus);
        samus.XPosition = 0x0300;
        samus.YPosition = 0x0300;

        // The preceding laser correctly leaves `$B64B`'s phase/cooldown at 1/$40. Let the
        // native dispatcher consume that state rather than resetting test internals. Once
        // thinking installs `$B64B` with phase zero again, the next frame is selection.
        random.SetRandomNumber(0x0050);
        int synchronizationFrames = 0;
        for (; synchronizationFrames < 96; synchronizationFrames++)
        {
            enemies.StepFrame(
                cameraX: 0,
                cameraY: 0,
                timeIsFrozen: false,
                samus,
                level: level,
                nmiFrameCounter8: unchecked((byte)synchronizationFrames));
            enemies.StepEnemyProjectiles(
                level,
                samus,
                cameraX: 0,
                cameraY: 0,
                nmiFrameCounter8: unchecked((byte)synchronizationFrames));
            if (state.Function == MotherBrainBodyFunction.SecondPhaseTryAttack &&
                state.AttackPhase == MotherBrainAttackPhase.ChooseAttack)
            {
                break;
            }
        }
        if (synchronizationFrames == 96)
        {
            throw new InvalidDataException(
                $"Mother Brain bomb audit could not naturally restore attack phase zero: " +
                $"function=$A9:{(ushort)state.Function:X4}, phase/cooldown=" +
                $"{state.AttackPhase}/{state.AttackCooldown}.");
        }

        // `$8080` has low byte >=$80, selecting the early grounded bomb strategy. The body
        // starts at X=$80 so full-word RNG chooses target $40 and necessarily exercises the
        // medium backwards-walk helper. Advancing this exact seed yields an upper-half word,
        // which then exercises slow crouch rather than the immediate-fire half.
        state.Body.XPosition = 0x0080;
        random.SetRandomNumber(0x8080);

        bool[] bombWasActive = new bool[enemies.EnemyProjectiles.Count];
        bool observedWalk = false;
        bool observedCrouch = false;
        bool observedFireWait = false;
        bool observedStand = false;
        bool observedBodyFinish = false;
        bool observedHeadCry = false;
        bool observedPurpleBreath = false;
        bool observedNeckDisabled = false;
        bool observedNeckReenabled = false;
        bool observedBounce = false;
        bool observedNaturalExpiry = false;
        bool observedAfterburn = false;
        bool observedDust = false;
        bool observedExpirySound = false;
        int spawnCount = 0;
        int frames = 0;

        for (; frames < 2048; frames++)
        {
            enemies.StepFrame(
                cameraX: 0,
                cameraY: 0,
                timeIsFrozen: false,
                samus,
                level: level,
                nmiFrameCounter8: unchecked((byte)frames));

            observedWalk |=
                state.Function == MotherBrainBodyFunction.SecondPhaseBombWalkingBackwards;
            observedCrouch |=
                state.Function == MotherBrainBodyFunction.SecondPhaseBombCrouch ||
                state.Pose == MotherBrainBodyPose.CrouchingTransition ||
                state.Pose == MotherBrainBodyPose.Crouched;
            observedFireWait |=
                state.Function == MotherBrainBodyFunction.SecondPhaseBombFired;
            observedStand |=
                state.Function == MotherBrainBodyFunction.SecondPhaseBombStandUp;
            observedBodyFinish |= observedFireWait &&
                state.Function == MotherBrainBodyFunction.SecondPhaseThinking &&
                state.Pose == MotherBrainBodyPose.Standing;
            observedHeadCry |= state.LastSoundEffect == 0x006f;
            observedPurpleBreath |= enemies.EnemyProjectiles.Any(
                projectile =>
                    projectile.Kind == RoomEnemyProjectileKind.MotherBrainPurpleBreathBig);
            observedNeckDisabled |= !state.NeckMovementEnabled;
            observedNeckReenabled |= observedNeckDisabled && state.NeckMovementEnabled;

            foreach (RoomEnemyProjectileSlot bomb in enemies.EnemyProjectiles.Where(
                         projectile => projectile.Kind == RoomEnemyProjectileKind.MotherBrainBomb))
            {
                if (!bombWasActive[bomb.SlotIndex])
                {
                    spawnCount++;
                    if (bomb.PreInstruction != 0xc4c8 ||
                        bomb.InstructionPointer != 0xc76e || bomb.InstructionTimer != 1 ||
                        bomb.SpritemapPointer != 0x8000 || bomb.GraphicsIndex != 0x0400 ||
                        bomb.XRadius != 6 || bomb.YRadius != 6 || bomb.Damage != 0x00a0 ||
                        bomb.XSubposition != 7 || bomb.YSubposition != 0 ||
                        bomb.XVelocity != 0x00e0 || bomb.YVelocity != 0x0100 ||
                        bomb.Variable0 != 0x0070 || bomb.Variable1 != 0 ||
                        bomb.DirectionParameter != 7 ||
                        bomb.XPosition != unchecked((ushort)(head.XPosition + 0x000c)) ||
                        bomb.YPosition != unchecked((ushort)(head.YPosition + 0x0010)) ||
                        !bomb.CanDamageSamus || !bomb.PersistsOnSamusContact ||
                        bomb.BlocksSamusProjectiles || state.BombCounter != 1)
                    {
                        throw new InvalidDataException(
                            $"Mother Brain bomb initialization diverged: slot={bomb.SlotIndex}, " +
                            $"pre/list/timer=${bomb.PreInstruction:X4}/" +
                            $"${bomb.InstructionPointer:X4}/{bomb.InstructionTimer}, map/gfx=" +
                            $"${bomb.SpritemapPointer:X4}/${bomb.GraphicsIndex:X4}, radii/dmg=" +
                            $"{bomb.XRadius}/{bomb.YRadius}/{bomb.Damage}, sub=" +
                            $"({bomb.XSubposition:X4},{bomb.YSubposition:X4}), velocity=" +
                            $"({bomb.XVelocity:X4},{bomb.YVelocity:X4}), vars=" +
                            $"({bomb.Variable0:X4},{bomb.Variable1:X4}), param=" +
                            $"{bomb.DirectionParameter}, pos=({bomb.XPosition:X4}," +
                            $"{bomb.YPosition:X4}), flags={bomb.CanDamageSamus}/" +
                            $"{bomb.PersistsOnSamusContact}/{bomb.BlocksSamusProjectiles}, " +
                            $"counter={state.BombCounter}.");
                    }
                }
            }

            ushort[] bounceOffsetsBefore = enemies.EnemyProjectiles
                .Select(projectile => projectile.Variable1)
                .ToArray();
            enemies.StepEnemyProjectiles(
                level,
                samus,
                cameraX: 0,
                cameraY: 0,
                nmiFrameCounter8: unchecked((byte)frames));

            foreach (RoomEnemyProjectileSlot bomb in enemies.EnemyProjectiles.Where(
                         projectile => projectile.Kind == RoomEnemyProjectileKind.MotherBrainBomb))
            {
                if (bomb.Variable1 != bounceOffsetsBefore[bomb.SlotIndex])
                {
                    if (bomb.Variable1 !=
                            unchecked((ushort)(bounceOffsetsBefore[bomb.SlotIndex] + 2)) ||
                        bomb.YPosition != 0x00d0 || bomb.YVelocity != 0xfe00 ||
                        WrappedMagnitudeReference(bomb.XVelocity) != 0x0070)
                    {
                        throw new InvalidDataException(
                            $"Mother Brain bomb bounce diverged: offset=" +
                            $"${bomb.Variable1:X4}/${bounceOffsetsBefore[bomb.SlotIndex] + 2:X4}, " +
                            $"positionY=${bomb.YPosition:X4}, velocity=" +
                            $"({bomb.XVelocity:X4},{bomb.YVelocity:X4}).");
                    }
                    observedBounce = true;
                }
            }

            bool bombIsActive = enemies.EnemyProjectiles.Any(
                projectile => projectile.Kind == RoomEnemyProjectileKind.MotherBrainBomb);
            bool bombWasActiveLastFrame = bombWasActive.Any(active => active);
            if (bombWasActiveLastFrame && !bombIsActive && state.BombCounter == 0)
            {
                observedNaturalExpiry = true;
                observedAfterburn |= enemies.EnemyProjectiles.Any(
                    projectile => projectile.Kind ==
                        RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnCenter);
                observedDust |= enemies.EnemyProjectiles.Any(
                    projectile => projectile.Kind == RoomEnemyProjectileKind.MiscDustExplosion);
                observedExpirySound |= state.LastSoundEffectLibrary3 == 0x0013;
            }

            for (int slotIndex = 0; slotIndex < bombWasActive.Length; slotIndex++)
            {
                bombWasActive[slotIndex] = enemies.EnemyProjectiles[slotIndex].Kind ==
                    RoomEnemyProjectileKind.MotherBrainBomb;
            }

            if (spawnCount == 1 && observedWalk && observedCrouch && observedFireWait &&
                observedStand && observedBodyFinish && observedHeadCry &&
                observedPurpleBreath && observedNeckDisabled && observedNeckReenabled &&
                observedBounce && observedNaturalExpiry && observedAfterburn &&
                observedDust && observedExpirySound)
            {
                break;
            }
        }

        if (frames == 2048 || spawnCount != 1 || state.BodyTargetXPosition != 0x0040 ||
            !observedWalk || !observedCrouch || !observedFireWait || !observedStand ||
            !observedBodyFinish || !observedHeadCry || !observedPurpleBreath ||
            !observedNeckDisabled || !observedNeckReenabled || !observedBounce ||
            !observedNaturalExpiry || !observedAfterburn || !observedDust ||
            !observedExpirySound || state.BombCounter != 0)
        {
            throw new InvalidDataException(
                $"Mother Brain natural bomb cycle diverged: frames={frames}, spawns=" +
                $"{spawnCount}, function/pose=$A9:{(ushort)state.Function:X4}/{state.Pose}, " +
                $"target=${state.BodyTargetXPosition:X4}, walk/crouch/fire/stand/finish=" +
                $"{observedWalk}/{observedCrouch}/{observedFireWait}/{observedStand}/" +
                $"{observedBodyFinish}, cry/breath={observedHeadCry}/{observedPurpleBreath}, " +
                $"neck={observedNeckDisabled}/{observedNeckReenabled}, bounce/expiry/fx=" +
                $"{observedBounce}/{observedNaturalExpiry}/{observedAfterburn}/" +
                $"{observedDust}/{observedExpirySound}, counter={state.BombCounter}.");
        }

        AuditMotherBrainBombDestroyedBySamusBomb(
            bus,
            level,
            enemies,
            state,
            samus,
            random);
    }

    private static void AuditMotherBrainBombDestroyedBySamusBomb(
        SuperMetroidAddressSpace bus,
        RoomLevelData level,
        RoomEnemySystem enemies,
        MotherBrainEnemyState state,
        SamusState samus,
        Bank80SystemState random)
    {
        RoomEnemySlot head = state.Head!;
        ushort spawnCommandPointer = 0;
        for (ushort pointer = 0x9ecc; pointer < 0x9f00; pointer += 2)
        {
            ushort word = unchecked((ushort)(
                bus.ReadByte(0xa90000 | pointer) |
                (bus.ReadByte(0xa90000 | unchecked((ushort)(pointer + 1))) << 8)));
            if (word == 0x9ebd)
            {
                spawnCommandPointer = pointer;
                break;
            }
        }
        if (spawnCommandPointer == 0)
            throw new InvalidDataException("Could not locate Mother Brain's $9EBD bomb opcode.");

        // Enter the real opcode at its phase-two list location; this isolates the collision
        // seam without spending another complete attack/crouch cycle on an identical spawn.
        random.SetRandomNumber(0x5000);
        head.CurrentInstruction = spawnCommandPointer;
        head.InstructionTimer = 1;
        head.Timer = 0;
        enemies.StepFrame(
            cameraX: 0,
            cameraY: 0,
            timeIsFrozen: false,
            samus,
            level: level,
            nmiFrameCounter8: 0);

        RoomEnemyProjectileSlot motherBrainBomb = enemies.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.MotherBrainBomb);
        ushort impactX = motherBrainBomb.XPosition;
        ushort impactY = motherBrainBomb.YPosition;

        // Place a normal bomb through Samus's public producer so aggregate count, type,
        // definition data, and radii are real. Only the fuse is advanced to its explosion
        // instant; `$86:C1BF` explicitly keys on timer zero and does not consume the bomb.
        var samusBombs = new SamusBombProjectileSystem();
        samus.Pose = SamusPoseIds.MorphBallGroundRightPose;
        samus.RefreshCollisionRadii(bus);
        samus.XPosition = impactX;
        samus.YPosition = impactY;
        samus.EquippedItems |= (ushort)SamusEquipmentFlags.Bombs;
        samus.SelectedHudItem = 0;
        const ushort shoot = (ushort)SnesButton.X;
        BombProjectileFrameResult placed = samusBombs.StepFrame(
            bus,
            level,
            samus,
            controllerInput: shoot,
            controllerNewInput: shoot);
        if (placed.PlacedSlot is not int samusBombIndex)
            throw new InvalidDataException("Could not place Samus bomb for Mother Brain collision audit.");
        SamusBombProjectileSlot samusBomb = samusBombs.Slots[samusBombIndex];
        samusBomb.XPosition = impactX;
        samusBomb.YPosition = impactY;
        samusBomb.BombTimer = 0;

        // The enemy-projectile collision pass follows all pre-instructions. Move Samus
        // away while leaving her independent bomb at the impact point so the newborn
        // zero-damage dust is not immediately consumed by an unrelated contact pass.
        samus.XPosition = 0x0300;
        samus.YPosition = 0x0300;

        bool[] dustWasActive = enemies.EnemyProjectiles
            .Select(projectile => projectile.Kind == RoomEnemyProjectileKind.MiscDustExplosion)
            .ToArray();
        enemies.StepEnemyProjectiles(
            level,
            samus,
            cameraX: 0,
            cameraY: 0,
            nmiFrameCounter8: 1,
            samusBombs: samusBombs);

        int newDustCount = enemies.EnemyProjectiles.Count(projectile =>
            projectile.Kind == RoomEnemyProjectileKind.MiscDustExplosion &&
            !dustWasActive[projectile.SlotIndex]);
        MotherBrainBombDropRequest? drop = state.LastBombDropRequest;
        if (state.BombCounter != 0 || enemies.EnemyProjectiles.Any(
                projectile => projectile.Kind == RoomEnemyProjectileKind.MotherBrainBomb) ||
            newDustCount != 1 || drop is null || drop.Value.X != impactX ||
            drop.Value.Y != impactY || drop.Value.EnemyDefinitionPointer !=
                head.EnemyDefinitionPointer || state.LastSoundEffectLibrary3 is not null)
        {
            throw new InvalidDataException(
                $"Mother Brain bomb/Samus-bomb collision diverged: counter=" +
                $"{state.BombCounter}, live={enemies.EnemyProjectiles.Count(projectile => projectile.Kind == RoomEnemyProjectileKind.MotherBrainBomb)}, " +
                $"newDust={newDustCount}, drop={drop}, expected=({impactX:X4}," +
                $"{impactY:X4},${head.EnemyDefinitionPointer:X4}), sound=" +
                $"{state.LastSoundEffectLibrary3?.ToString("X4") ?? "none"}.");
        }
    }

    private static ushort WrappedMagnitudeReference(ushort value) =>
        unchecked((short)value) < 0 ? unchecked((ushort)-value) : value;

    /// <summary>
    /// Forces low-health thinking through the real `$B87D` body dispatcher, then observes
    /// `$9A42` produce its charge actor and `$8171` recursively produce the first fired
    /// child. The remainder runs naturally through the 240-frame hold and bytecode-owned
    /// phase increment instead of editing the body timer or instruction cursor.
    /// </summary>
    private static void AuditPhaseTwoHandBeamAttack(
        SuperMetroidAddressSpace bus,
        RoomLevelData level,
        RoomEnemySystem enemies,
        MotherBrainEnemyState state,
        SamusState samus,
        Bank80SystemState random)
    {
        RoomEnemySlot head = state.Head ?? throw new InvalidDataException(
            "Mother Brain hand-beam audit lost the linked head record.");

        // `$4000` lies in the low-health hand-beam interval [$2000,$A000). The body is
        // deliberately left at the position reached by the preceding bomb audit so the
        // native slow walk and X=$30 safety floor are exercised rather than skipped.
        random.SetRandomNumber(0x4000);
        head.Health = 0x1000;
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.RefreshCollisionRadii(bus);
        samus.XPosition = 0x00d0;
        samus.YPosition = 0x0060;
        samus.Health = 999;
        samus.InvincibilityTimer = 0;
        samus.KnockbackTimer = 0;

        enemies.StepFrame(0, 0, timeIsFrozen: false, samus, level: level, nmiFrameCounter8: 0);
        if (state.Function != MotherBrainBodyFunction.SecondPhaseHandBeam ||
            state.HandBeamPhase != MotherBrainHandBeamPhase.BackUp)
        {
            throw new InvalidDataException(
                $"Mother Brain low-health thinking did not select $B87D: function=" +
                $"$A9:{(ushort)state.Function:X4}, phase={state.HandBeamPhase}.");
        }

        bool observedSlowWalk = false;
        bool observedWaitForBombs = false;
        bool observedDeathBeamPose = false;
        bool observedChargeSound = false;
        bool observedChargeDust = false;
        bool observedChargingInitializer = false;
        bool observedFirstFiredChild = false;
        bool observedImpact = false;
        bool observedFinishPhase = false;
        int frames = 1;

        bool[] dustWasActive = new bool[enemies.EnemyProjectiles.Count];
        for (; frames < 640; frames++)
        {
            for (int slotIndex = 0; slotIndex < dustWasActive.Length; slotIndex++)
            {
                dustWasActive[slotIndex] = enemies.EnemyProjectiles[slotIndex].Kind ==
                    RoomEnemyProjectileKind.MiscDustExplosion;
            }

            enemies.StepFrame(
                cameraX: 0,
                cameraY: 0,
                timeIsFrozen: false,
                samus,
                level: level,
                nmiFrameCounter8: unchecked((byte)frames));

            observedSlowWalk |= state.Body.CurrentInstruction >= 0x9852 &&
                state.Body.CurrentInstruction <= 0x988a;
            observedWaitForBombs |= state.HandBeamPhase ==
                MotherBrainHandBeamPhase.WaitForBombs;
            observedDeathBeamPose |= state.Pose == MotherBrainBodyPose.DeathBeam;
            observedChargeSound |= state.LastSoundEffect == 0x0063;

            foreach (RoomEnemyProjectileSlot projectile in enemies.EnemyProjectiles)
            {
                if (projectile.Kind == RoomEnemyProjectileKind.MiscDustExplosion &&
                    !dustWasActive[projectile.SlotIndex] &&
                    projectile.XPosition is >= 0x0040 and <= 0x0080 &&
                    projectile.YPosition < state.Body.YPosition)
                {
                    observedChargeDust = true;
                }
            }

            RoomEnemyProjectileSlot? charging = enemies.EnemyProjectiles.FirstOrDefault(
                projectile => projectile.Kind ==
                    RoomEnemyProjectileKind.MotherBrainHandBeamCharging);
            if (charging is not null && !observedChargingInitializer)
            {
                byte expectedAngle = unchecked((byte)(0x80 -
                    CalculateMotherBrainCartridgeAngleReference(
                        unchecked((short)(samus.XPosition - charging.XPosition)),
                        unchecked((short)(samus.YPosition - charging.YPosition)))));
                ushort expectedXVelocity = MultiplyMotherBrainSineReference(
                    bus,
                    0x0c00,
                    expectedAngle);
                ushort expectedYVelocity = MultiplyMotherBrainSineReference(
                    bus,
                    0x0c00,
                    unchecked((byte)(expectedAngle + 0x40)));
                if (charging.XPosition != unchecked((ushort)(state.Body.XPosition + 0x0040)) ||
                    charging.YPosition != unchecked((ushort)(state.Body.YPosition - 0x0030)) ||
                    charging.XSubposition != 0 || charging.YSubposition != 0 ||
                    charging.XVelocity != 0 || charging.YVelocity != 0 ||
                    charging.Variable0 != 0 || charging.Variable1 != 0 ||
                    charging.PreInstruction != 0xc76d ||
                    charging.InstructionPointer != 0xc796 ||
                    charging.InstructionTimer != 1 ||
                    charging.GraphicsIndex != 0x0400 ||
                    charging.XRadius != 6 || charging.YRadius != 6 ||
                    charging.Damage != 0x0190 || !charging.CanDamageSamus ||
                    state.HandBeamNextAngle != expectedAngle ||
                    state.HandBeamNextXVelocity != expectedXVelocity ||
                    state.HandBeamNextYVelocity != expectedYVelocity)
                {
                    throw new InvalidDataException(
                        $"Mother Brain hand-beam charge initializer diverged: pos=" +
                        $"({charging.XPosition:X4},{charging.YPosition:X4}), velocity=" +
                        $"({state.HandBeamNextXVelocity:X4},{state.HandBeamNextYVelocity:X4})/" +
                        $"({expectedXVelocity:X4},{expectedYVelocity:X4}), angle=" +
                        $"${state.HandBeamNextAngle:X2}/${expectedAngle:X2}, list=" +
                        $"${charging.InstructionPointer:X4}.");
                }
                observedChargingInitializer = true;

                // Aiming has already sampled Samus. Move her outside the arena so the
                // common contact pass cannot consume a red-beam actor during this lifecycle
                // audit and accidentally turn projectile saturation into an input variable.
                samus.XPosition = 0x0300;
                samus.YPosition = 0x0300;
            }

            ushort sharedXBefore = state.HandBeamNextXPosition;
            ushort sharedXSubBefore = state.HandBeamNextXSubposition;
            ushort sharedYBefore = state.HandBeamNextYPosition;
            ushort sharedYSubBefore = state.HandBeamNextYSubposition;
            ushort sharedXVelocity = state.HandBeamNextXVelocity;
            ushort sharedYVelocity = state.HandBeamNextYVelocity;
            var expectedBeamRandom = new Bank80SystemState(random.RandomNumber);
            int firedBefore = enemies.EnemyProjectiles.Count(projectile =>
                projectile.Kind == RoomEnemyProjectileKind.MotherBrainHandBeamFired);

            enemies.StepEnemyProjectiles(
                level,
                samus,
                cameraX: 0,
                cameraY: 0,
                nmiFrameCounter8: unchecked((byte)frames));

            int firedAfter = enemies.EnemyProjectiles.Count(projectile =>
                projectile.Kind == RoomEnemyProjectileKind.MotherBrainHandBeamFired);
            if (!observedFirstFiredChild && firedAfter > firedBefore)
            {
                (ushort expectedSharedX, ushort expectedSharedXSub) =
                    AddEightBitVelocityReference(
                        sharedXBefore,
                        sharedXSubBefore,
                        sharedXVelocity);
                (ushort expectedSharedY, ushort expectedSharedYSub) =
                    AddEightBitVelocityReference(
                        sharedYBefore,
                        sharedYSubBefore,
                        sharedYVelocity);
                byte expectedScatterAngle = unchecked((byte)(
                    state.HandBeamNextAngle +
                    unchecked((byte)expectedBeamRandom.NextRandom())));
                ushort expectedScatterSpeed = unchecked((ushort)(
                    expectedBeamRandom.NextRandom() & 0x0700));
                ushort expectedScatterXVelocity = MultiplyMotherBrainSineReference(
                    bus,
                    expectedScatterSpeed,
                    expectedScatterAngle);
                ushort expectedScatterYVelocity = MultiplyMotherBrainSineReference(
                    bus,
                    expectedScatterSpeed,
                    unchecked((byte)(expectedScatterAngle + 0x40)));
                (ushort expectedChildX, ushort expectedChildXSub) =
                    AddEightBitVelocityReference(
                        expectedSharedX,
                        expectedSharedXSub,
                        expectedScatterXVelocity);
                (ushort expectedChildY, ushort expectedChildYSub) =
                    AddEightBitVelocityReference(
                        expectedSharedY,
                        expectedSharedYSub,
                        expectedScatterYVelocity);
                RoomEnemyProjectileSlot child = enemies.EnemyProjectiles.First(projectile =>
                    projectile.Kind == RoomEnemyProjectileKind.MotherBrainHandBeamFired);

                // `$8171` allocates the fired child below its charging parent. The native
                // descending projectile pass therefore reaches the child later in this same
                // call: its pre-instruction applies the scatter displacement above and its
                // fresh timer consumes the first timed entry at $C796. The cursor visible at
                // the frame boundary is $C79A, not the initializer's untouched $C796.
                ushort expectedChildInstructionTimer = ReadWord(bus, 0x86c796);
                ushort expectedChildSpritemap = ReadWord(bus, 0x86c798);
                if (state.HandBeamNextXPosition != expectedSharedX ||
                    state.HandBeamNextXSubposition != expectedSharedXSub ||
                    state.HandBeamNextYPosition != expectedSharedY ||
                    state.HandBeamNextYSubposition != expectedSharedYSub ||
                    child.PreInstruction != 0xc76d ||
                    child.InstructionPointer != 0xc79a ||
                    child.InstructionTimer != expectedChildInstructionTimer ||
                    child.SpritemapPointer != expectedChildSpritemap ||
                    child.Variable0 != 1 || child.Variable1 != 0 ||
                    child.XVelocity != 0 || child.YVelocity != 0 ||
                    child.XPosition != expectedChildX ||
                    child.XSubposition != expectedChildXSub ||
                    child.YPosition != expectedChildY ||
                    child.YSubposition != expectedChildYSub ||
                    child.XRadius != 6 || child.YRadius != 6 ||
                    child.Damage != 0x0190 || !child.CanDamageSamus)
                {
                    throw new InvalidDataException(
                        $"Mother Brain fired hand-beam child diverged: shared=" +
                        $"({state.HandBeamNextXPosition:X4}." +
                        $"{state.HandBeamNextXSubposition:X4}," +
                        $"{state.HandBeamNextYPosition:X4}." +
                        $"{state.HandBeamNextYSubposition:X4})/" +
                        $"({expectedSharedX:X4}.{expectedSharedXSub:X4}," +
                        $"{expectedSharedY:X4}.{expectedSharedYSub:X4}), child=" +
                        $"({child.XPosition:X4}.{child.XSubposition:X4}," +
                        $"{child.YPosition:X4}.{child.YSubposition:X4})/" +
                        $"({expectedChildX:X4}.{expectedChildXSub:X4}," +
                        $"{expectedChildY:X4}.{expectedChildYSub:X4}), list=" +
                        $"${child.InstructionPointer:X4}/{child.Variable0}.");
                }
                observedFirstFiredChild = true;
            }

            observedImpact |= state.LastSoundEffectLibrary3 == 0x0013 &&
                enemies.EarthquakeType == 5 && enemies.EarthquakeTimer == 10;
            observedFinishPhase |= state.HandBeamPhase == MotherBrainHandBeamPhase.Finish;
            if (observedFirstFiredChild && observedFinishPhase &&
                state.Function == MotherBrainBodyFunction.SecondPhaseThinking &&
                state.HandBeamPhase == MotherBrainHandBeamPhase.BackUp)
            {
                break;
            }
        }

        if (frames == 640 || !observedSlowWalk || !observedWaitForBombs ||
            !observedDeathBeamPose || !observedChargeSound || !observedChargeDust ||
            !observedChargingInitializer || !observedFirstFiredChild || !observedImpact ||
            !observedFinishPhase || state.Function !=
                MotherBrainBodyFunction.SecondPhaseThinking ||
            state.HandBeamPhase != MotherBrainHandBeamPhase.BackUp ||
            state.Pose != MotherBrainBodyPose.Standing ||
            state.LowerNeckMovementIndex != 2 || state.UpperNeckMovementIndex != 4 ||
            head.CurrentInstruction < 0x9c87 || head.CurrentInstruction > 0x9cab)
        {
            throw new InvalidDataException(
                $"Mother Brain hand-beam cycle diverged: frames={frames}, walk=" +
                $"{observedSlowWalk}, wait={observedWaitForBombs}, pose=" +
                $"{observedDeathBeamPose}/{state.Pose}, charge=" +
                $"{observedChargeSound}/{observedChargeDust}/" +
                $"{observedChargingInitializer}, fired={observedFirstFiredChild}, " +
                $"impact={observedImpact}, finish={observedFinishPhase}, function=" +
                $"$A9:{(ushort)state.Function:X4}, phase={state.HandBeamPhase}.");
        }
    }

    /// <summary>
    /// Drives the loaded room's real body slot through the zero-health `$B605 → $B8EB`
    /// tail call, both 256-count charge waits, ordinary head bytecode, bank-$86 charge
    /// sprites, pre-fire power-bomb gate, HDMA-state handoff, forced Samus movement, and the
    /// first drain tick. The standalone sequence verifier already covers every arithmetic
    /// boundary; this audit proves the gameplay scheduler is attached to that implementation.
    /// </summary>
    private static void AuditPhaseTwoRainbowBeamAttack(
        SuperMetroidAddressSpace bus,
        RoomLevelData level,
        RoomEnemySystem enemies,
        MotherBrainEnemyState state,
        SamusState samus,
        Bank80SystemState random,
        SnesVram vram,
        SnesCgram cgram)
    {
        RoomEnemySlot head = state.Head ?? throw new InvalidDataException(
            "Mother Brain rainbow audit lost the linked head record.");
        if (state.Function != MotherBrainBodyFunction.SecondPhaseThinking ||
            state.Pose != MotherBrainBodyPose.Standing)
        {
            throw new InvalidDataException(
                $"Mother Brain rainbow audit requires standing thinking state, got " +
                $"$A9:{(ushort)state.Function:X4}/{state.Pose}.");
        }

        // `$B605` tests only the head's health word. A normal phase-two projectile collision
        // is what writes zero in gameplay; mutating that terminal word here avoids fabricating
        // hundreds of extra beam hits while retaining the exact public body scheduler entry.
        head.Health = 0;
        samus.Health = 999;
        samus.MaxHealth = 999;
        samus.Missiles = 99;
        samus.SuperMissiles = 20;
        samus.PowerBombs = 10;
        samus.EquippedItems = (ushort)SamusEquipmentFlags.VariaSuit;
        samus.XPosition = 0x00dc;
        samus.YPosition = 0x007c;
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.InputLocked = false;
        samus.InvincibilityTimer = 0;
        samus.KnockbackTimer = 0;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        random.SetRandomNumber(0x1234);

        var sharedProjectiles = new SamusBombProjectileSystem();
        enemies.StepFrame(
            cameraX: 0,
            cameraY: 0,
            timeIsFrozen: false,
            samus,
            level: level,
            nmiFrameCounter8: 0,
            sharedProjectiles: sharedProjectiles);

        MotherBrainRainbowBeamAttackSequence sequence = state.RainbowBeamSequence ??
            throw new InvalidDataException(
                "Mother Brain zero health did not allocate the live rainbow sequence.");
        if (state.Function != MotherBrainBodyFunction.SecondPhaseRainbowStartCharging ||
            sequence.Phase != MotherBrainRainbowBeamAttackPhase.StartCharging ||
            state.FunctionTimer != 0x0100 || sequence.FunctionTimer != 0x0100 ||
            state.RainbowAppliedHeadInstructionList !=
                MotherBrainRainbowBeamAttackSequence.HeadNeutralPhase2InstructionList ||
            state.NeckAngleDelta != 0x0040 || !state.NeckMovementEnabled ||
            state.LowerNeckMovementIndex != 2 || state.UpperNeckMovementIndex != 4)
        {
            throw new InvalidDataException(
                $"Mother Brain live rainbow setup diverged: function/phase=" +
                $"$A9:{(ushort)state.Function:X4}/{sequence.Phase}, timer=" +
                $"${state.FunctionTimer:X4}/${sequence.FunctionTimer:X4}, head=" +
                $"${state.RainbowAppliedHeadInstructionList:X4}, neck=" +
                $"${state.NeckAngleDelta:X4}/{state.LowerNeckMovementIndex}/" +
                $"{state.UpperNeckMovementIndex}.");
        }

        bool observedChargingHeadList = false;
        bool observedChargeEffects = false;
        bool observedChargingProjectile = false;
        bool observedChargingProjectilePin = false;
        bool observedSecondWait = false;
        bool observedChargeSound = false;
        bool observedPrefireCooldown = false;
        bool observedActiveBeam = false;
        bool observedBeamSound = false;
        bool observedRainbowExplosion = false;
        bool observedDrain = false;
        bool observedBeamShutdown = false;
        bool observedFalling = false;
        bool observedLanding = false;
        bool observedDecisionDelay = false;
        bool observedRepeatPointer = false;
        bool observedRepeatSetup = false;
        int firstChargeCalls = 0;
        int secondChargeCalls = 0;
        int drainCalls = 0;
        int frames = 1;

        for (; frames < 1400; frames++)
        {
            MotherBrainRainbowBeamAttackPhase phaseBefore = sequence.Phase;
            ushort healthBefore = samus.Health;
            enemies.StepFrame(
                cameraX: 0,
                cameraY: 0,
                timeIsFrozen: false,
                samus,
                level: level,
                nmiFrameCounter8: unchecked((byte)frames),
                sharedProjectiles: sharedProjectiles);

            if (phaseBefore == MotherBrainRainbowBeamAttackPhase.StartCharging)
                firstChargeCalls++;
            if (phaseBefore == MotherBrainRainbowBeamAttackPhase.WaitForCharge)
                secondChargeCalls++;
            if (phaseBefore is MotherBrainRainbowBeamAttackPhase.StartDrainingSamus or
                MotherBrainRainbowBeamAttackPhase.DrainingSamus)
            {
                drainCalls++;
            }

            observedChargingHeadList |= state.RainbowAppliedHeadInstructionList ==
                MotherBrainRainbowBeamAttackSequence.HeadChargingRainbowInstructionList;
            observedChargeEffects |= !state.SmallPurpleBreathGenerationEnabled &&
                state.BrainPaletteTimer == 0x0202 && state.LastSoundEffect == 0x007f;
            observedSecondWait |= sequence.Phase ==
                MotherBrainRainbowBeamAttackPhase.WaitForCharge;
            observedChargeSound |= state.LastRainbowBeamStep is
                { ChargeSoundQueued: true } && state.LastSoundEffect == 0x0071;
            observedPrefireCooldown |= sharedProjectiles.CooldownTimer == 8;
            observedActiveBeam |= state.RainbowBeamHdmaActive && samus.InputLocked &&
                sequence.Phase is MotherBrainRainbowBeamAttackPhase.MoveSamusTowardWall or
                    MotherBrainRainbowBeamAttackPhase.OneFrameDelay or
                    MotherBrainRainbowBeamAttackPhase.StartDrainingSamus or
                    MotherBrainRainbowBeamAttackPhase.DrainingSamus;
            observedBeamSound |= state.LastSoundEffectLibrary1 == 0x0040;

            bool matchedCurrentRainbowExplosion = false;
            foreach (RoomEnemyProjectileSlot projectile in enemies.EnemyProjectiles)
            {
                if (projectile.Kind ==
                    RoomEnemyProjectileKind.MotherBrainRainbowBeamCharging)
                {
                    observedChargingProjectile = true;
                    if (projectile.PreInstruction != 0xc814 ||
                        projectile.InstructionPointer < 0xc829 ||
                        projectile.InstructionPointer > 0xc843 ||
                        projectile.GraphicsIndex != 0 || projectile.XVelocity != 0 ||
                        projectile.YVelocity != 0 || projectile.XRadius != 0 ||
                        projectile.YRadius != 0 || projectile.CanDamageSamus)
                    {
                        throw new InvalidDataException(
                            $"Mother Brain rainbow charge projectile diverged: pre/list=" +
                            $"${projectile.PreInstruction:X4}/${projectile.InstructionPointer:X4}, " +
                            $"graphics=${projectile.GraphicsIndex:X4}, velocity=" +
                            $"({projectile.XVelocity:X4},{projectile.YVelocity:X4}), radius=" +
                            $"{projectile.XRadius}/{projectile.YRadius}.");
                    }
                    observedChargingProjectilePin |=
                        projectile.XPosition == head.XPosition &&
                        projectile.YPosition == head.YPosition;
                }
                else if (projectile.Kind ==
                    RoomEnemyProjectileKind.MotherBrainRainbowBeamExplosion)
                {
                    short offsetX = unchecked((short)projectile.XVelocity);
                    short offsetY = unchecked((short)projectile.YVelocity);
                    if (projectile.PreInstruction != 0xc94c ||
                        projectile.XRadius != 1 || projectile.YRadius != 1 ||
                        projectile.CanDamageSamus)
                    {
                        throw new InvalidDataException(
                            $"Mother Brain rainbow explosion diverged: pre/list=" +
                            $"${projectile.PreInstruction:X4}/${projectile.InstructionPointer:X4}, " +
                            $"position=({projectile.XPosition:X4},{projectile.YPosition:X4}), " +
                            $"Samus=({samus.XPosition:X4},{samus.YPosition:X4}), offset=" +
                            $"({offsetX},{offsetY}).");
                    }

                    // `$BC76` spawns before the same body function moves Samus. Match the
                    // new actor against the movement witness's pre-move coordinate; older
                    // explosions remain attached by their own later bank-$86 calls.
                    if (state.LastRainbowBeamStep is
                        { Explosion: { } request, Movement: { } movement } &&
                        projectile.InstructionPointer == ReadWord(bus, 0x86cbb1) &&
                        projectile.XVelocity == unchecked((ushort)request.XOffset) &&
                        projectile.YVelocity == unchecked((ushort)request.YOffset) &&
                        projectile.XPosition == unchecked((ushort)(
                            movement.Before.XPosition + request.XOffset)) &&
                        projectile.YPosition == unchecked((ushort)(
                            movement.Before.YPosition + request.YOffset)))
                    {
                        observedRainbowExplosion = true;
                        matchedCurrentRainbowExplosion = true;
                    }
                }
            }

            if (state.LastRainbowBeamStep is
                    { Explosion: { } expectedExplosion, Movement: { } expectedMovement } &&
                !matchedCurrentRainbowExplosion)
            {
                string candidates = string.Join(",", enemies.EnemyProjectiles
                    .Where(projectile => projectile.Kind ==
                        RoomEnemyProjectileKind.MotherBrainRainbowBeamExplosion)
                    .Select(projectile =>
                        $"{projectile.SlotIndex}:({projectile.XPosition:X4}," +
                        $"{projectile.YPosition:X4})/({projectile.XVelocity:X4}," +
                        $"{projectile.YVelocity:X4})/${projectile.InstructionPointer:X4}"));
                throw new InvalidDataException(
                    $"Mother Brain did not publish the current pre-move rainbow explosion: " +
                    $"before=({expectedMovement.Before.XPosition:X4}," +
                    $"{expectedMovement.Before.YPosition:X4}), offset=" +
                    $"({expectedExplosion.XOffset},{expectedExplosion.YOffset}), " +
                    $"candidates=[{candidates}].");
            }

            enemies.StepEnemyProjectiles(
                level,
                samus,
                cameraX: 0,
                cameraY: 0,
                nmiFrameCounter8: unchecked((byte)frames),
                samusBombs: sharedProjectiles);

            // Runtime order reaches Samus's bank-$90 alpha after enemy/projectile work.
            // With no input this call only advances the shared cooldown and dormant bombs.
            sharedProjectiles.StepFrame(
                bus,
                level,
                samus,
                controllerInput: 0,
                controllerNewInput: 0);

            observedDrain |= sequence.Phase ==
                    MotherBrainRainbowBeamAttackPhase.DrainingSamus &&
                samus.Health < healthBefore;
            observedBeamShutdown |= phaseBefore ==
                    MotherBrainRainbowBeamAttackPhase.FinishFiring &&
                sequence.Phase == MotherBrainRainbowBeamAttackPhase.LetSamusFall &&
                !state.RainbowBeamHdmaActive && !samus.InputLocked &&
                sharedProjectiles.CooldownTimer == 7;
            observedFalling |= phaseBefore is MotherBrainRainbowBeamAttackPhase.LetSamusFall or
                MotherBrainRainbowBeamAttackPhase.WaitForSamusToLand;
            observedLanding |= sequence.Phase == MotherBrainRainbowBeamAttackPhase.LowerHead &&
                samus.YPosition == 0x00c0 &&
                samus.Pose == SamusPoseIds.DrainedCrouchingLeftPose;
            observedDecisionDelay |= sequence.Phase ==
                MotherBrainRainbowBeamAttackPhase.DecideNextAction &&
                state.Function == MotherBrainBodyFunction.SecondPhaseRainbowDecideNextAction;
            observedRepeatPointer |= sequence.Phase ==
                MotherBrainRainbowBeamAttackPhase.RepeatAttack &&
                state.Function == MotherBrainBodyFunction.SecondPhaseRainbowExtendNeck;
            observedRepeatSetup |= phaseBefore == MotherBrainRainbowBeamAttackPhase.RepeatAttack &&
                sequence.Phase == MotherBrainRainbowBeamAttackPhase.StartCharging &&
                state.Function == MotherBrainBodyFunction.SecondPhaseRainbowStartCharging &&
                state.FunctionTimer == 0x0100;
            if (observedRepeatSetup)
                break;
        }

        if (frames == 1400 || firstChargeCalls != 257 || secondChargeCalls != 256 ||
            drainCalls != 300 ||
            !observedChargingHeadList || !observedChargeEffects ||
            !observedChargingProjectile || !observedChargingProjectilePin ||
            !observedSecondWait || !observedChargeSound || !observedPrefireCooldown ||
            !observedActiveBeam || !observedBeamSound || !observedRainbowExplosion ||
            !observedDrain || !observedBeamShutdown || !observedFalling ||
            !observedLanding || !observedDecisionDelay || !observedRepeatPointer ||
            !observedRepeatSetup || state.RainbowBeamHdmaActive || samus.InputLocked ||
            samus.Health != 699)
        {
            throw new InvalidDataException(
                $"Mother Brain live rainbow cycle diverged: frames={frames}, waits=" +
                $"{firstChargeCalls}/257,{secondChargeCalls}/256, drain={drainCalls}/300, " +
                $"head/effects/projectile=" +
                $"{observedChargingHeadList}/{observedChargeEffects}/" +
                $"{observedChargingProjectile}/{observedChargingProjectilePin}, " +
                $"wait/sound/cooldown={observedSecondWait}/{observedChargeSound}/" +
                $"{observedPrefireCooldown}, active/sfx/explosion/drain=" +
                $"{observedActiveBeam}/{observedBeamSound}/{observedRainbowExplosion}/" +
                $"{observedDrain}, shutdown/fall/land/decision/repeat=" +
                $"{observedBeamShutdown}/{observedFalling}/{observedLanding}/" +
                $"{observedDecisionDelay}/{observedRepeatPointer}/{observedRepeatSetup}, " +
                $"phase={sequence.Phase}, function=" +
                $"$A9:{(ushort)state.Function:X4}, health={samus.Health}.");
        }

        AuditLiveBabyMetroidSpawn(
            bus,
            level,
            enemies,
            state,
            samus,
            sequence,
            random,
            sharedProjectiles,
            vram,
            cgram,
            frames + 1);
    }

    /// <summary>
    /// Continues the already-live final rainbow sequence through the cartridge's low-health
    /// branch, four sprite-page DMAs, generic enemy allocation, bank-$A9 initializer, and
    /// the Baby's first independently scheduled AI/instruction turn. This is intentionally
    /// not a standalone state-machine call: the frozen active-index array must keep the new
    /// physical enemy dormant on its allocation frame, just as <c>$A0:8FD4</c> does.
    /// </summary>
    private static void AuditLiveBabyMetroidSpawn(
        SuperMetroidAddressSpace bus,
        RoomLevelData level,
        RoomEnemySystem enemies,
        MotherBrainEnemyState state,
        SamusState samus,
        MotherBrainRainbowBeamAttackSequence sequence,
        Bank80SystemState random,
        SamusBombProjectileSystem sharedProjectiles,
        SnesVram vram,
        SnesCgram cgram,
        int startingFrame)
    {
        RoomEnemySlot expectedFreeSlot = enemies.Slots.FirstOrDefault(
            slot => slot.EnemyDefinitionPointer == 0) ?? throw new InvalidDataException(
            "Mother Brain Baby audit found no free physical enemy slot.");

        // Earlier focused attack audits deliberately drive the same physical record to both
        // arena extremes; their composite endpoint is X=$26, below `$C647`'s backward-walk
        // floor. A real final-beam route does not inherit that synthetic history. Isolate
        // this cutscene at the cartridge's ordinary X=$60 phase-two attack waypoint, then
        // import the physical words exactly as the live adapter does before `$BB1A`.
        int frame = startingFrame;
        RoomEnemySlot head = state.Head ?? throw new InvalidDataException(
            "Baby audit lost Mother Brain's physical head record.");
        state.Body.XPosition = 0x0060;
        state.Pose = MotherBrainBodyPose.Standing;
        sequence.SynchronizeLiveActor(
            state.Body.XPosition,
            state.Body.YPosition,
            state.Pose,
            state.Form,
            state.Body.Properties,
            state.Body.ExtraProperties,
            head.XPosition,
            head.YPosition,
            head.Health,
            head.Properties,
            head.ExtraProperties,
            state.LowerNeckAngle,
            state.UpperNeckAngle,
            state.NeckMovementEnabled,
            state.LowerNeckMovementIndex,
            state.UpperNeckMovementIndex,
            state.BombCounter,
            state.HitboxesEnabled != 0);

        // Lower only the word consulted by `$BD45`; all subsequent transitions execute
        // through the public room scheduler. StartFinishOffSequence represents the `$BB1A`
        // decision call, whose BodyWalkRequested result would also copy this list into the
        // physical body. Because this audit deliberately enters at the public debugger seam,
        // reproduce that single adapter write before returning to ordinary scheduled frames.
        samus.Health = 100;
        sequence.StartFinishOffSequence();
        state.Function = MotherBrainBodyFunction.SecondPhaseFinishSamusOff;
        state.Body.CurrentInstruction =
            MotherBrainRainbowBeamAttackSequence.BodyWalkingForwardReallySlowInstructionList;
        state.Body.InstructionTimer = 1;
        state.Body.Timer = 0;

        bool observedStandUp = false;
        bool observedAdmiration = false;
        bool observedFinalCharge = false;
        int tileTransferCount = 0;
        RoomEnemySlot? babySlot = null;
        int spawnDeadline = frame + 600;
        for (; frame < spawnDeadline; frame++)
        {
            enemies.StepFrame(
                cameraX: 0,
                cameraY: 0,
                timeIsFrozen: false,
                samus,
                level: level,
                nmiFrameCounter8: unchecked((byte)frame),
                sharedProjectiles: sharedProjectiles);

            observedStandUp |= sequence.Phase ==
                MotherBrainRainbowBeamAttackPhase.FinishStandUp;
            observedAdmiration |= sequence.Phase ==
                MotherBrainRainbowBeamAttackPhase.AdmireJobWellDone;
            observedFinalCharge |= sequence.Phase ==
                MotherBrainRainbowBeamAttackPhase.ChargeFinalRainbowBeam;
            // The charge-expiration call falls straight into `$BDD2` and emits page zero
            // before the externally visible phase was ever LoadBabyMetroidTiles.
            if (state.LastRainbowBeamStep?.SpriteTileTransfer is not null)
            {
                tileTransferCount++;
            }

            if (state.BabyMetroidSlot is { } spawned)
            {
                babySlot = spawned;
                break;
            }
        }

        if (babySlot is null || frame == spawnDeadline)
        {
            throw new InvalidDataException(
                $"Mother Brain did not physically spawn the Baby within 600 frames: " +
                $"phase={sequence.Phase}, function=$A9:{(ushort)state.Function:X4}, " +
                $"transfers={tileTransferCount}.");
        }

        BabyMetroidCutsceneState baby = state.BabyMetroid ?? throw new InvalidDataException(
            "Mother Brain allocated a Baby slot without attaching its actor state.");
        RoomEnemyDefinition definition = babySlot.Definition;
        if (!ReferenceEquals(babySlot, expectedFreeSlot) ||
            babySlot.EnemyDefinitionPointer != 0xecbf || definition.Bank != 0xa9 ||
            definition.InitializationAiPointer != 0xc710 ||
            definition.MainAiPointer != 0xc779 || definition.Health != 3200 ||
            definition.Damage != 40 || definition.XRadius != 0x0024 ||
            definition.YRadius != 0x0024 || definition.Layer != 2 ||
            babySlot.XRadius != 0x0024 || babySlot.YRadius != 0x0024 ||
            babySlot.XPosition != 0x0140 || babySlot.YPosition != 0x0060 ||
            babySlot.XSubposition != 0 || babySlot.YSubposition != 0 ||
            babySlot.Properties != 0x3800 || babySlot.ExtraProperties != 0 ||
            babySlot.Health != 3200 || babySlot.PaletteIndex != 0x0e00 ||
            babySlot.VramTilesIndex != 0x00a0 || babySlot.Layer != 2 ||
            babySlot.CurrentInstruction != BabyMetroidCutsceneState.InitialInstructionList ||
            babySlot.InstructionTimer != 1 || babySlot.SpritemapPointer != 0x804d ||
            babySlot.FrameCounter != 0 || baby.Phase != BabyMetroidCutscenePhase.DashOntoScreen ||
            baby.FunctionTimer != 0x00f8 ||
            sequence.Phase != MotherBrainRainbowBeamAttackPhase.FireFinalRainbowBeam ||
            state.Function != MotherBrainBodyFunction.SecondPhaseFinishSamusOffFireFinalBeam ||
            sequence.FunctionTimer != 0x0100 || tileTransferCount != 4 ||
            !observedStandUp || !observedAdmiration || !observedFinalCharge)
        {
            throw new InvalidDataException(
                $"Live Baby allocation diverged: slot={babySlot.SlotIndex}/" +
                $"{expectedFreeSlot.SlotIndex}, id=${babySlot.EnemyDefinitionPointer:X4}, " +
                $"AI=${definition.Bank:X2}:{definition.InitializationAiPointer:X4}/" +
                $"{definition.MainAiPointer:X4}, header={definition.Health}/" +
                $"{definition.Damage}/{definition.XRadius:X4}/{definition.YRadius:X4}/" +
                $"{definition.Layer}, pos=({babySlot.XPosition:X4},{babySlot.YPosition:X4}), " +
                $"props=${babySlot.Properties:X4}/${babySlot.ExtraProperties:X4}, " +
                $"health/palette/tiles/layer={babySlot.Health:X4}/" +
                $"{babySlot.PaletteIndex:X4}/{babySlot.VramTilesIndex:X4}/" +
                $"{babySlot.Layer}, list/timer/map/frame=${babySlot.CurrentInstruction:X4}/" +
                $"{babySlot.InstructionTimer:X4}/${babySlot.SpritemapPointer:X4}/" +
                $"{babySlot.FrameCounter}, phase/timer={baby.Phase}/${baby.FunctionTimer:X4}, " +
                $"brain={sequence.Phase}/$A9:{(ushort)state.Function:X4}/" +
                $"{sequence.FunctionTimer:X4}, transfers={tileTransferCount}, route=" +
                $"{observedStandUp}/{observedAdmiration}/{observedFinalCharge}.");
        }

        // `$A9:8FE5` copies four complete $200-byte OBJ pages before spawning the actor.
        ReadOnlySpan<int> tileSources = [0xb18400, 0xb18600, 0xb18800, 0xb18a00];
        ReadOnlySpan<int> tileDestinationWords = [0x7c00, 0x7d00, 0x7e00, 0x7f00];
        for (int page = 0; page < tileSources.Length; page++)
        {
            for (int byteIndex = 0; byteIndex < 0x0200; byteIndex++)
            {
                byte expected = bus.ReadByte(tileSources[page] + byteIndex);
                byte actual = vram.ReadByte(tileDestinationWords[page] * 2 + byteIndex);
                if (actual != expected)
                {
                    throw new InvalidDataException(
                        $"Baby OBJ page {page} diverged at byte ${byteIndex:X3}: " +
                        $"${actual:X2}/${expected:X2}.");
                }
            }
        }

        // Initialization copies colors 1..15 from `$A9:94D2`; color zero is preserved.
        for (int color = 0; color < 15; color++)
        {
            ushort expected = ReadWord(bus, 0xa994d4 + color * 2);
            ushort actual = cgram.Colors[0x00f1 + color];
            if (actual != expected)
            {
                throw new InvalidDataException(
                    $"Baby initial palette color {color + 1} diverged: " +
                    $"${actual:X4}/${expected:X4}.");
            }
        }

        // The allocation happened after the frame's active enemy indexes were frozen. On
        // the next frame the Baby receives its first main call and then the ordinary list
        // interpreter consumes `$CFA2` into its first visible spritemap.
        frame++;
        enemies.StepFrame(
            cameraX: 0,
            cameraY: 0,
            timeIsFrozen: false,
            samus,
            level: level,
            nmiFrameCounter8: unchecked((byte)frame),
            sharedProjectiles: sharedProjectiles);
        ushort expectedDuration = ReadWord(bus, 0xa9cfa2);
        ushort expectedSpritemap = ReadWord(bus, 0xa9cfa4);
        if (babySlot.FrameCounter != 1 || baby.FunctionTimer != 0x00f7 ||
            babySlot.InstructionTimer != expectedDuration ||
            babySlot.SpritemapPointer != expectedSpritemap ||
            babySlot.CurrentInstruction != 0xcfa6 || state.LastBabyMetroidStep is null)
        {
            throw new InvalidDataException(
                $"Baby first scheduled turn diverged: frame={babySlot.FrameCounter}, " +
                $"functionTimer=${baby.FunctionTimer:X4}, instruction=" +
                $"${babySlot.InstructionTimer:X4}/${expectedDuration:X4}, map=" +
                $"${babySlot.SpritemapPointer:X4}/${expectedSpritemap:X4}, cursor=" +
                $"${babySlot.CurrentInstruction:X4}, witness=" +
                $"{state.LastBabyMetroidStep is not null}.");
        }

        bool observedBodyStumble = false;
        bool observedDrainAnimation = false;
        int babyCalls = 1;
        for (; babyCalls < 500; babyCalls++)
        {
            frame++;
            enemies.StepFrame(
                cameraX: 0,
                cameraY: 0,
                timeIsFrozen: false,
                samus,
                level: level,
                nmiFrameCounter8: unchecked((byte)frame),
                sharedProjectiles: sharedProjectiles);

            BabyMetroidCutsceneStepResult step = state.LastBabyMetroidStep ??
                throw new InvalidDataException(
                    $"Physical Baby missed scheduled AI call {babyCalls + 1}.");
            if (step.BodyStumbleRequested)
            {
                observedBodyStumble = true;
                if (state.Body.CurrentInstruction !=
                        MotherBrainRainbowBeamAttackSequence.
                            BodyWalkingBackwardReallyFastInstructionList ||
                    state.Body.InstructionTimer != 1)
                {
                    throw new InvalidDataException(
                        $"Baby's cross-slot body stumble did not install physical list " +
                        $"$988C: list=${state.Body.CurrentInstruction:X4}, timer=" +
                        $"${state.Body.InstructionTimer:X4}.");
                }
            }

            observedDrainAnimation |= state.BabyAppliedInstructionList ==
                BabyMetroidCutsceneState.DrainingMotherBrainInstructionList;
            if (step.MotherBrainInterrupted)
            {
                if (!step.LatchSoundQueued || state.LastSoundEffectLibrary1 != 0x0040 ||
                    baby.Phase != BabyMetroidCutscenePhase.WaitForMotherBrainToTurnToCorpse ||
                    sequence.Phase !=
                        MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidTakenAback ||
                    state.Function != MotherBrainBodyFunction.SecondPhaseDrainedByBabyTakenAback ||
                    baby.XPosition != sequence.BrainXPosition ||
                    baby.YPosition != unchecked((ushort)(sequence.BrainYPosition - 0x0018)) ||
                    baby.XVelocity != 0 || baby.YVelocity != 0 ||
                    state.BabyAppliedInstructionList !=
                        BabyMetroidCutsceneState.DrainingMotherBrainInstructionList ||
                    babySlot.CurrentInstruction != 0xcfbc ||
                    babySlot.InstructionTimer != ReadWord(bus, 0xa9cfb8) ||
                    babySlot.SpritemapPointer != ReadWord(bus, 0xa9cfba))
                {
                    throw new InvalidDataException(
                        $"Baby drain interrupt diverged on call {babyCalls + 1}: " +
                        $"sound={step.LatchSoundQueued}/${state.LastSoundEffectLibrary1:X4}, " +
                        $"phase={baby.Phase}/{sequence.Phase}/" +
                        $"$A9:{(ushort)state.Function:X4}, pos=({baby.XPosition:X4}," +
                        $"{baby.YPosition:X4})/brain({sequence.BrainXPosition:X4}," +
                        $"{sequence.BrainYPosition:X4}), velocity=({baby.XVelocity:X4}," +
                        $"{baby.YVelocity:X4}), list=${state.BabyAppliedInstructionList:X4}/" +
                        $"${babySlot.CurrentInstruction:X4}, instruction=" +
                        $"${babySlot.InstructionTimer:X4}, map=${babySlot.SpritemapPointer:X4}.");
                }
                break;
            }
        }

        if (babyCalls == 500 || !observedBodyStumble || !observedDrainAnimation)
        {
            throw new InvalidDataException(
                $"Baby did not complete its live entrance/drain handoff: calls={babyCalls}, " +
                $"stumble={observedBodyStumble}, drainList={observedDrainAnimation}, " +
                $"phase={baby.Phase}/{sequence.Phase}.");
        }

        // Mother Brain's body record is earlier than the Baby slot. It cannot run `$BE38`
        // until the following frame; that call initializes form/neck/timer and falls through
        // into `$BE5D`, which immediately decrements `$30` to `$2F`.
        frame++;
        enemies.StepFrame(
            cameraX: 0,
            cameraY: 0,
            timeIsFrozen: false,
            samus,
            level: level,
            nmiFrameCounter8: unchecked((byte)frame),
            sharedProjectiles: sharedProjectiles);
        if (sequence.Phase !=
                MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidRegainBalance ||
            state.Function != MotherBrainBodyFunction.SecondPhaseDrainedByBabyRegainBalance ||
            state.Form != 3 || state.FunctionTimer != 0x002f ||
            state.LowerNeckMovementIndex != 8 || state.UpperNeckMovementIndex != 8 ||
            state.NeckAngleDelta != 0x0700 ||
            baby.Phase != BabyMetroidCutscenePhase.WaitForMotherBrainToTurnToCorpse)
        {
            throw new InvalidDataException(
                $"Mother Brain did not consume the Baby's deferred `$BE38` write: " +
                $"phase={sequence.Phase}/$A9:{(ushort)state.Function:X4}, form=" +
                $"{state.Form}, timer=${state.FunctionTimer:X4}, neck=" +
                $"{state.LowerNeckMovementIndex}/{state.UpperNeckMovementIndex}/" +
                $"${state.NeckAngleDelta:X4}, Baby={baby.Phase}.");
        }

        AuditLiveBabyMetroidDrainAndHealing(
            bus,
            level,
            enemies,
            state,
            samus,
            sequence,
            random,
            sharedProjectiles,
            babySlot,
            baby,
            vram,
            cgram,
            frame);
    }

    /// <summary>
    /// Runs the two independently scheduled actors from `$BE5D` until the Baby has reached
    /// Samus and restored every energy point. The body must perform all eight painful-walk
    /// stages, retreat, crouch, and nine grey-table probes while the later Baby slot remains
    /// pinned to the moving head; only the shared corpse word releases it toward Samus.
    /// </summary>
    private static void AuditLiveBabyMetroidDrainAndHealing(
        SuperMetroidAddressSpace bus,
        RoomLevelData level,
        RoomEnemySystem enemies,
        MotherBrainEnemyState state,
        SamusState samus,
        MotherBrainRainbowBeamAttackSequence sequence,
        Bank80SystemState random,
        SamusBombProjectileSystem sharedProjectiles,
        RoomEnemySlot babySlot,
        BabyMetroidCutsceneState baby,
        SnesVram vram,
        SnesCgram cgram,
        int startingFrame)
    {
        bool observedPainfulWalk = false;
        bool observedBeamRunOut = false;
        bool observedRetreat = false;
        bool observedLowPower = false;
        bool observedGreyTransition = false;
        bool observedCorpseHandoff = false;
        bool observedInitialAnimationRestored = false;
        bool observedCeilingRoute = false;
        bool observedSamusTouch = false;
        bool observedHealing = false;
        int releaseDustRequests = 0;
        int healingCalls = 0;
        int frame = startingFrame;

        for (int calls = 0; calls < 6000; calls++)
        {
            frame++;
            ushort healthBefore = samus.Health;
            BabyMetroidCutscenePhase babyPhaseBefore = baby.Phase;
            enemies.StepFrame(
                cameraX: 0,
                cameraY: 0,
                timeIsFrozen: false,
                samus,
                level: level,
                nmiFrameCounter8: unchecked((byte)frame),
                sharedProjectiles: sharedProjectiles);

            BabyMetroidCutsceneStepResult step = state.LastBabyMetroidStep ??
                throw new InvalidDataException(
                    $"Baby lost its physical scheduler before healing on frame {frame}.");
            observedPainfulWalk |= sequence.Phase ==
                MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidFiringRainbowBeam;
            observedBeamRunOut |= sequence.Phase ==
                MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidRainbowBeamRunOut;
            observedRetreat |= sequence.Phase ==
                MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidMoveToBackOfRoom;
            observedRetreat |= state.LastRainbowBeamStep is
            {
                PhaseBefore: MotherBrainRainbowBeamAttackPhase.
                    DrainedByBabyMetroidRainbowBeamRunOut,
                PhaseAfter: MotherBrainRainbowBeamAttackPhase.
                    DrainedByBabyMetroidGoIntoLowPowerMode,
            };
            observedLowPower |= sequence.Phase ==
                MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidGoIntoLowPowerMode;
            observedGreyTransition |= sequence.Phase ==
                MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidTransitionToGrey;
            observedInitialAnimationRestored |= state.BabyAppliedInstructionList ==
                BabyMetroidCutsceneState.InitialInstructionList &&
                babyPhaseBefore == BabyMetroidCutscenePhase.StopDraining;
            observedCeilingRoute |= step.SamusCrouchingRequested &&
                baby.MovementTablePointer == BabyMetroidCutsceneState.CeilingToSamusMovementTable;
            observedSamusTouch |= step.SamusTouchCollision;
            releaseDustRequests += step.ReleaseDustClouds.Count;

            if (!observedCorpseHandoff && sequence.Phase2CorpseState != 0)
            {
                observedCorpseHandoff = true;
                RoomEnemySlot head = state.Head!;
                if (sequence.Phase2CorpseState != 1 || head.Health != 0x8ca0 ||
                    state.Form != 2 || state.SmallPurpleBreathGenerationEnabled ||
                    baby.Phase != BabyMetroidCutscenePhase.StopDraining ||
                    baby.FunctionTimer != 0x0040 ||
                    state.Function != MotherBrainBodyFunction.SecondPhaseReviveInanimateGrey)
                {
                    throw new InvalidDataException(
                        $"Mother Brain corpse/Baby release handoff diverged: corpse=" +
                        $"{sequence.Phase2CorpseState}, health=${head.Health:X4}, form=" +
                        $"{state.Form}, breath={state.SmallPurpleBreathGenerationEnabled}, " +
                        $"Baby={baby.Phase}/${baby.FunctionTimer:X4}, function=" +
                        $"$A9:{(ushort)state.Function:X4}.");
                }
            }

            if (babyPhaseBefore == BabyMetroidCutscenePhase.HealSamusToFullHealth)
            {
                healingCalls++;
                observedHealing = true;
                ushort expectedHealth = Math.Min(
                    samus.MaxHealth,
                    unchecked((ushort)(healthBefore + 1)));
                if (samus.Health != expectedHealth)
                {
                    throw new InvalidDataException(
                        $"Live Baby healing call {healingCalls} changed Samus energy " +
                        $"{healthBefore}->{samus.Health}, expected {expectedHealth}.");
                }
            }

            // Preserve runtime ordering and allow drool/dust actors to release their shared
            // pool slots. Their trajectories are spatially separate from the Baby/Samus
            // latch, so no cutscene collision is bypassed by this maintenance pass.
            enemies.StepEnemyProjectiles(
                level,
                samus,
                cameraX: 0,
                cameraY: 0,
                nmiFrameCounter8: unchecked((byte)frame),
                samusBombs: sharedProjectiles);
            sharedProjectiles.StepFrame(
                bus,
                level,
                samus,
                controllerInput: 0,
                controllerNewInput: 0);

            if (step.HealingCompleted)
            {
                if (baby.Phase != BabyMetroidCutscenePhase.IdleUntilNoHealth ||
                    samus.Health != samus.MaxHealth ||
                    samus.ReserveEnergy != samus.MaxReserveEnergy ||
                    babySlot.Health != baby.Health)
                {
                    throw new InvalidDataException(
                        $"Live Baby healing completion diverged: phase={baby.Phase}, " +
                        $"health={samus.Health}/{samus.MaxHealth}, reserve=" +
                        $"{samus.ReserveEnergy}/{samus.MaxReserveEnergy}, Baby health=" +
                        $"{babySlot.Health}/{baby.Health}.");
                }

                if (!observedPainfulWalk || !observedBeamRunOut || !observedRetreat ||
                    !observedLowPower || !observedGreyTransition || !observedCorpseHandoff ||
                    !observedInitialAnimationRestored || !observedCeilingRoute ||
                    !observedSamusTouch || !observedHealing || releaseDustRequests != 3 ||
                    healingCalls != 899)
                {
                    throw new InvalidDataException(
                        $"Live Baby drain/healing route missed a native stage: body=" +
                        $"{observedPainfulWalk}/{observedBeamRunOut}/{observedRetreat}/" +
                        $"{observedLowPower}/{observedGreyTransition}, corpse=" +
                        $"{observedCorpseHandoff}, animation={observedInitialAnimationRestored}, " +
                        $"ceiling/touch/heal={observedCeilingRoute}/{observedSamusTouch}/" +
                        $"{observedHealing}, dust={releaseDustRequests}/3, healing=" +
                        $"{healingCalls}/899.");
                }
                AuditLiveBabyMetroidMurderAndPhaseThree(
                    bus,
                    level,
                    enemies,
                    state,
                    samus,
                    sequence,
                    random,
                    sharedProjectiles,
                    babySlot,
                    baby,
                    vram,
                    cgram,
                    frame);
                return;
            }
        }

        throw new InvalidDataException(
            $"Live Baby did not finish healing within 6000 frames: Baby={baby.Phase}, " +
            $"Mother Brain={sequence.Phase}, energy={samus.Health}/{samus.MaxHealth}, " +
            $"body=({state.Body.XPosition:X4},{state.Body.YPosition:X4})/" +
            $"{state.Pose}/${state.Body.CurrentInstruction:X4}/" +
            $"$A9:{(ushort)state.Function:X4}, step=" +
            $"{state.LastRainbowBeamStep?.PhaseBefore}->" +
            $"{state.LastRainbowBeamStep?.PhaseAfter}, neck=" +
            $"{state.NeckMovementEnabled}/{state.LowerNeckMovementIndex}/" +
            $"{state.UpperNeckMovementIndex}/${state.NeckAngleDelta:X4}, mirror=" +
            $"{sequence.Body.Pose}/{sequence.LowerNeckMovementIndex}/" +
            $"{sequence.UpperNeckMovementIndex}.");
    }

    /// <summary>
    /// Lets the revived body and its ordinary `$9DB1` head bytecode attack the physical Baby
    /// through both health pools. Bank-$86 onion rings own collision/damage; the Baby owns
    /// release, final charge, death art, tile restoration, room-light restoration, Hyper
    /// Beam grant, deletion, and the later-slot `$C1CF` phase-three function write.
    /// </summary>
    private static void AuditLiveBabyMetroidMurderAndPhaseThree(
        SuperMetroidAddressSpace bus,
        RoomLevelData level,
        RoomEnemySystem enemies,
        MotherBrainEnemyState state,
        SamusState samus,
        MotherBrainRainbowBeamAttackSequence sequence,
        Bank80SystemState random,
        SamusBombProjectileSystem sharedProjectiles,
        RoomEnemySlot babySlot,
        BabyMetroidCutsceneState baby,
        SnesVram vram,
        SnesCgram cgram,
        int startingFrame)
    {
        int frame = startingFrame;
        int ringHits = 0;
        int initialDrainHits = 0;
        int finalChargeHits = 0;
        int babyPaletteTransfers = 0;
        int attackTileTransfers = 0;
        int roomPaletteTransfers = 0;
        int deathExplosions = 0;
        bool observedCry = false;
        bool observedRelease = false;
        bool observedPrepareFinalAttack = false;
        bool observedExecuteFinalAttack = false;
        bool observedFatalBlow = false;
        bool observedSamusRainbow = false;
        bool observedPhaseThreeHandoff = false;

        for (int calls = 0; calls < 12000; calls++)
        {
            frame++;

            // `$C15C` attacks only when random bit 15 is set. The encompassing audit has
            // already proven RNG arithmetic separately; fixing this input selects the live
            // combat branch deterministically without bypassing body/head/projectile work.
            random.SetRandomNumber(0x8000);
            BabyMetroidCutscenePhase phaseBefore = baby.Phase;
            enemies.StepFrame(
                cameraX: 0,
                cameraY: 0,
                timeIsFrozen: false,
                samus,
                level: level,
                nmiFrameCounter8: unchecked((byte)frame),
                sharedProjectiles: sharedProjectiles);

            BabyMetroidCutsceneStepResult? step = state.LastBabyMetroidStep;
            if (step is { } babyStep)
            {
                babyPaletteTransfers += babyStep.BabyPaletteTransfer is null ? 0 : 1;
                attackTileTransfers += babyStep.AttackTileTransfer is null ? 0 : 1;
                roomPaletteTransfers += babyStep.BackgroundPaletteTransfer is null ? 0 : 1;
                deathExplosions += babyStep.DeathExplosion is null ? 0 : 1;
                observedFatalBlow |= babyStep.SamusAnimationFrozen;
                observedSamusRainbow |= babyStep.SamusRainbowActivated;
                observedPhaseThreeHandoff |= babyStep.PhaseThreeHandoff &&
                    babyStep.HyperBeamEnabled && babyStep.SamusRainbowDisabled;
            }
            observedCry |= state.LastSoundEffect == 0x0072;
            observedRelease |= baby.Phase is BabyMetroidCutscenePhase.ReleaseSamus or
                BabyMetroidCutscenePhase.StareDownMotherBrain or
                BabyMetroidCutscenePhase.FlyOffScreen;
            observedPrepareFinalAttack |= sequence.Phase ==
                MotherBrainRainbowBeamAttackPhase.PrepareForFinalBabyMetroidAttack;
            observedExecuteFinalAttack |= sequence.Phase is
                MotherBrainRainbowBeamAttackPhase.ExecuteFinalBabyMetroidAttack or
                MotherBrainRainbowBeamAttackPhase.FinalBabyMetroidAttackHolding;

            ushort healthBeforeProjectiles = baby.Health;
            enemies.StepEnemyProjectiles(
                level,
                samus,
                cameraX: 0,
                cameraY: 0,
                nmiFrameCounter8: unchecked((byte)frame),
                samusBombs: sharedProjectiles);
            sharedProjectiles.StepFrame(
                bus,
                level,
                samus,
                controllerInput: 0,
                controllerNewInput: 0);

            if (baby.Health < healthBeforeProjectiles)
            {
                int damage = healthBeforeProjectiles - baby.Health;
                int hitsThisFrame;
                if (healthBeforeProjectiles == 0x004f && baby.Health == 0)
                {
                    hitsThisFrame = 1;
                    finalChargeHits++;
                }
                else if (damage % 0x0050 == 0)
                {
                    hitsThisFrame = damage / 0x0050;
                    initialDrainHits += hitsThisFrame;
                }
                else
                {
                    throw new InvalidDataException(
                        $"Onion-ring Baby damage diverged: {healthBeforeProjectiles}->" +
                        $"{baby.Health} during {baby.Phase}.");
                }
                ringHits += hitsThisFrame;

                if (state.LastSoundEffectLibrary3 != 0x0013 ||
                    baby.OnionRingHitFlashTimer != 0x0010)
                {
                    throw new InvalidDataException(
                        $"Onion-ring Baby impact omitted explosion/flash state: sound=" +
                        $"{state.LastSoundEffectLibrary3:X4}, flash=" +
                        $"${baby.OnionRingHitFlashTimer:X4}.");
                }
            }

            if (phaseBefore == BabyMetroidCutscenePhase.IdleUntilNoHealth &&
                baby.Phase == BabyMetroidCutscenePhase.ReleaseSamus)
            {
                if (initialDrainHits != 40 || baby.Health != 0x0140 ||
                    baby.LowHealthPaletteTimer != 0x000a)
                {
                    throw new InvalidDataException(
                        $"Baby first health-pool exhaustion diverged: hits=" +
                        $"{initialDrainHits}/40, health=${baby.Health:X4}, low palette=" +
                        $"${baby.LowHealthPaletteTimer:X4}.");
                }
            }

            if (observedPhaseThreeHandoff)
            {
                if (!baby.IsDeleted || babySlot.Properties != baby.Properties ||
                    sequence.Phase !=
                        MotherBrainRainbowBeamAttackPhase.Phase3RecoverFromCutsceneMakeSomeDistance ||
                    state.Function != MotherBrainBodyFunction.ThirdPhaseRecoverMakeSomeDistance ||
                    samus.HyperBeam == 0)
                {
                    throw new InvalidDataException(
                        $"Baby phase-three handoff diverged: deleted={baby.IsDeleted}/" +
                        $"${babySlot.Properties:X4}/${baby.Properties:X4}, phase=" +
                        $"{sequence.Phase}/$A9:{(ushort)state.Function:X4}, Hyper=" +
                        $"${samus.HyperBeam:X4}.");
                }

                // The following frame consumes the later-slot `$C1CF` write in the earlier
                // body record and clears the deleted physical Baby from the active pool.
                frame++;
                random.SetRandomNumber(0x8000);
                enemies.StepFrame(
                    cameraX: 0,
                    cameraY: 0,
                    timeIsFrozen: false,
                    samus,
                    level: level,
                    nmiFrameCounter8: unchecked((byte)frame),
                    sharedProjectiles: sharedProjectiles);
                if (sequence.Phase != MotherBrainRainbowBeamAttackPhase.
                        Phase3RecoverFromCutsceneSetupForFighting ||
                    state.Function != MotherBrainBodyFunction.ThirdPhaseRecoverSetupForFighting ||
                    state.Form != 4 || state.FunctionTimer != 0x0020 ||
                    babySlot.EnemyDefinitionPointer != 0)
                {
                    throw new InvalidDataException(
                        $"Mother Brain did not consume the deferred phase-three handoff: " +
                        $"phase={sequence.Phase}/$A9:{(ushort)state.Function:X4}, form=" +
                        $"{state.Form}, timer=${state.FunctionTimer:X4}, Baby ID=" +
                        $"${babySlot.EnemyDefinitionPointer:X4}.");
                }

                if (ringHits != 41 || initialDrainHits != 40 || finalChargeHits != 1 ||
                    !observedCry || !observedRelease || !observedPrepareFinalAttack ||
                    !observedExecuteFinalAttack || !observedFatalBlow ||
                    !observedSamusRainbow || babyPaletteTransfers != 6 ||
                    attackTileTransfers != 4 || roomPaletteTransfers != 7 ||
                    deathExplosions == 0)
                {
                    throw new InvalidDataException(
                        $"Live Baby murder/death route missed a native stage: hits=" +
                        $"{ringHits}/41 ({initialDrainHits}/40,{finalChargeHits}/1), " +
                        $"cry/release/prepare/execute={observedCry}/{observedRelease}/" +
                        $"{observedPrepareFinalAttack}/{observedExecuteFinalAttack}, fatal/" +
                        $"rainbow={observedFatalBlow}/{observedSamusRainbow}, transfers=" +
                        $"{babyPaletteTransfers}/6,{attackTileTransfers}/4," +
                        $"{roomPaletteTransfers}/7, explosions={deathExplosions}.");
                }

                AuditRestoredMotherBrainAttackTiles(bus, vram);
                AuditRestoredMotherBrainRoomLights(bus, cgram);
                AuditLivePhaseThreeHyperBeamRecoil(
                    bus,
                    level,
                    enemies,
                    state,
                    samus,
                    sequence,
                    random,
                    sharedProjectiles,
                    frame);
                return;
            }
        }

        throw new InvalidDataException(
            $"Live Baby murder did not reach phase three within 12000 frames: Baby=" +
            $"{baby.Phase}/${baby.Health}, Mother Brain={sequence.Phase}, hits={ringHits}.");
    }

    /// <summary>
    /// Continues the physical room encounter past the Baby handoff, fires the real bank-$90
    /// Hyper Beam producer into the live head record, and proves that shot callback
    /// <c>$A9:B507</c> reaches recoil routine <c>$A9:B5A9</c>. The reusable sequence already
    /// verifies every recoil/recovery timer in isolation; this audit guards the cross-owner
    /// seam that previously threw before any of those translated functions could run.
    /// </summary>
    private static void AuditLivePhaseThreeHyperBeamRecoil(
        SuperMetroidAddressSpace bus,
        RoomLevelData level,
        RoomEnemySystem enemies,
        MotherBrainEnemyState state,
        SamusState samus,
        MotherBrainRainbowBeamAttackSequence sequence,
        Bank80SystemState random,
        SamusBombProjectileSystem sharedProjectiles,
        int startingFrame)
    {
        int frame = startingFrame;

        // `$C1F0` accepts timer zero and changes state only after DEC produces `$FFFF`.
        // Keep RNG bit 15 clear so the same-call fallthrough into `$C209` cannot obscure the
        // recoil proof by installing the independent 64-frame attack-cooldown function.
        for (int calls = 0;
            calls < 40 &&
            sequence.Phase != MotherBrainRainbowBeamAttackPhase.Phase3FightingMain;
            calls++)
        {
            frame++;
            random.SetRandomNumber(0);
            enemies.StepFrame(
                cameraX: 0,
                cameraY: 0,
                timeIsFrozen: false,
                samus,
                level: level,
                nmiFrameCounter8: unchecked((byte)frame),
                sharedProjectiles: sharedProjectiles);
            enemies.StepEnemyProjectiles(
                level,
                samus,
                cameraX: 0,
                cameraY: 0,
                nmiFrameCounter8: unchecked((byte)frame),
                samusBombs: sharedProjectiles);
            sharedProjectiles.StepFrame(
                bus,
                level,
                samus,
                controllerInput: 0,
                controllerNewInput: 0);
        }

        RoomEnemySlot head = state.Head ?? throw new InvalidDataException(
            "Mother Brain phase-three Hyper Beam audit lost the physical head record.");
        if (sequence.Phase != MotherBrainRainbowBeamAttackPhase.Phase3FightingMain ||
            state.Function != MotherBrainBodyFunction.ThirdPhaseFightingMain ||
            state.Form != 4 || sequence.Body.Form != 4 ||
            sequence.Phase3WalkCounter >= 0x010a)
        {
            throw new InvalidDataException(
                $"Mother Brain did not enter the live Hyper Beam recoil boundary: phase=" +
                $"{sequence.Phase}/$A9:{(ushort)state.Function:X4}, forms=" +
                $"{state.Form}/{sequence.Body.Form}, walk=" +
                $"${sequence.Phase3WalkCounter:X4}.");
        }

        // Produce, rather than manufacture, the exact `$9018` Hyper Beam actor. Only its
        // world position is moved to the authored head origin so this callback audit is not
        // coupled to travel time or room terrain; type, 1000 damage, radii, pre-instruction,
        // slot accounting, and shared `$0CCC` cooldown all remain bank-$90 output.
        var projectiles = new SamusProjectileSystem();
        samus.SelectedHudItem = 0;
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        const ushort shoot = (ushort)SnesButton.X;
        SamusProjectileFrameResult fired = projectiles.StepFrame(
            bus,
            level,
            samus,
            controllerInput: shoot,
            controllerNewInput: shoot,
            layer1X: 0,
            layer1Y: 0,
            sharedProjectiles);
        if (fired.FiredSlot is not int slotIndex)
        {
            throw new InvalidDataException(
                $"Mother Brain phase-three audit could not produce Hyper Beam: Hyper=" +
                $"${samus.HyperBeam:X4}, cooldown=${sharedProjectiles.CooldownTimer:X4}, " +
                $"count={projectiles.ProjectileCounter}.");
        }

        SamusProjectileSlot shot = projectiles.Slots[slotIndex];
        if (shot.Type != 0x9018 || shot.Damage != 1000 ||
            shot.PreInstruction != SamusProjectilePreInstruction.HyperBeam)
        {
            throw new InvalidDataException(
                $"Bank-$90 Hyper Beam producer diverged before Mother Brain collision: " +
                $"type=${shot.Type:X4}, damage={shot.Damage}, pre={shot.PreInstruction}.");
        }
        shot.Direction = (ushort)SamusProjectileDirection.Right;
        shot.XPosition = head.XPosition;
        shot.YPosition = head.YPosition;

        ushort healthBefore = head.Health;
        ushort walkBefore = sequence.Phase3WalkCounter;
        int hits = enemies.ResolveOrdinaryProjectileHits(
            bus,
            projectiles,
            sharedProjectiles,
            samus);
        ushort vulnerabilityPointer = head.Definition.VulnerabilityPointer != 0
            ? head.Definition.VulnerabilityPointer
            : (ushort)0xec1c;
        byte vulnerability = bus.ReadByte(0xb40000 | vulnerabilityPointer);
        int damage = (1000 >> 1) * (vulnerability & 0x7f);
        ushort expectedHealth = damage >= healthBefore
            ? (ushort)0
            : unchecked((ushort)(healthBefore - damage));
        if (hits != 1 || shot.PackedType.Family != SamusProjectileFamily.BeamExplosion ||
            head.Health != expectedHealth || sequence.Phase3WalkCounter != 0 ||
            state.WalkCounter != 0 || sequence.Phase3NeckPhase !=
                MotherBrainPhase3NeckPhase.SetupHyperBeamRecoil ||
            state.FunctionTimer != 0 || head.Properties.HasAny(EnemyProperties.Deleted))
        {
            throw new InvalidDataException(
                $"Live Mother Brain Hyper Beam hit diverged: hits={hits}, family=" +
                $"${shot.PackedType.FamilyValue:X3}, health={head.Health}/{expectedHealth}, " +
                $"walk=${walkBefore:X4}->${sequence.Phase3WalkCounter:X4}/" +
                $"${state.WalkCounter:X4}, neck={sequence.Phase3NeckPhase}, timer=" +
                $"${state.FunctionTimer:X4}, deleted=" +
                $"{head.Properties.HasAny(EnemyProperties.Deleted)}.");
        }

        // Recoil setup is deliberately deferred until the following body turn. Its native
        // fallthrough immediately consumes one tick, installs `$9BE7`, disables attacks,
        // requests indices eight/eight, and seeds the draw-owned fifty-frame brain shake.
        frame++;
        random.SetRandomNumber(0xffff);
        enemies.StepFrame(
            cameraX: 0,
            cameraY: 0,
            timeIsFrozen: false,
            samus,
            level: level,
            nmiFrameCounter8: unchecked((byte)frame),
            sharedProjectiles: sharedProjectiles);
        if (sequence.Phase3NeckPhase != MotherBrainPhase3NeckPhase.HyperBeamRecoil ||
            sequence.Phase3NeckFunctionTimer != 0x000a ||
            sequence.Phase3DisableAttacks != 1 ||
            sequence.HeadInstructionList !=
                MotherBrainRainbowBeamAttackSequence.HeadHyperBeamRecoilInstructionList ||
            // The later physical head slot runs after the body in this same scheduler pass:
            // opcode `$9BE7` seeds shake, timed frame `$9BE9` is loaded, and the readable
            // next-instruction pointer is therefore `$9BED` when StepFrame returns.
            head.CurrentInstruction != unchecked((ushort)(
                MotherBrainRainbowBeamAttackSequence.HeadHyperBeamRecoilInstructionList + 6)) ||
            state.NeckAngleDelta != 0x0900 ||
            state.LowerNeckMovementIndex != 8 || state.UpperNeckMovementIndex != 8 ||
            state.BrainMainShakeTimer != 0x0032)
        {
            throw new InvalidDataException(
                $"Live Mother Brain Hyper Beam recoil setup diverged: neck=" +
                $"{sequence.Phase3NeckPhase}/${sequence.Phase3NeckFunctionTimer:X4}, " +
                $"disable={sequence.Phase3DisableAttacks}, lists=" +
                $"$A9:{sequence.HeadInstructionList:X4}/${head.CurrentInstruction:X4}, " +
                $"delta=${state.NeckAngleDelta:X4}, indices=" +
                $"{state.LowerNeckMovementIndex}/{state.UpperNeckMovementIndex}, shake=" +
                $"${state.BrainMainShakeTimer:X4}.");
        }
    }

    private static void AuditRestoredMotherBrainAttackTiles(
        SuperMetroidAddressSpace bus,
        SnesVram vram)
    {
        ReadOnlySpan<int> sources = [0xb7a000, 0xb7a200, 0xb7a400, 0xb7a600];
        ReadOnlySpan<int> destinations = [0x7c00, 0x7d00, 0x7e00, 0x7f00];
        for (int page = 0; page < sources.Length; page++)
        {
            for (int byteIndex = 0; byteIndex < 0x0200; byteIndex++)
            {
                byte expected = bus.ReadByte(sources[page] + byteIndex);
                byte actual = vram.ReadByte(destinations[page] * 2 + byteIndex);
                if (actual != expected)
                {
                    throw new InvalidDataException(
                        $"Restored Mother Brain attack page {page} diverged at " +
                        $"${byteIndex:X3}: ${actual:X2}/${expected:X2}.");
                }
            }
        }
    }

    private static void AuditRestoredMotherBrainRoomLights(
        ISnesAddressSpace bus,
        SnesCgram cgram)
    {
        const int source = 0xadf3d3 - 6 * 0x38;
        for (int color = 0; color < 14; color++)
        {
            ushort expectedFirst = ReadWord(bus, source + color * 2);
            ushort expectedSecond = ReadWord(bus, source + 0x1c + color * 2);
            ushort actualFirst = cgram.Colors[0x0031 + color];
            ushort actualSecond = cgram.Colors[0x0051 + color];
            if (actualFirst != expectedFirst || actualSecond != expectedSecond)
            {
                throw new InvalidDataException(
                    $"Restored Mother Brain room-light color {color} diverged: first=" +
                    $"${actualFirst:X4}/${expectedFirst:X4}, second=" +
                    $"${actualSecond:X4}/${expectedSecond:X4}.");
            }
        }
    }

    private static byte CalculateMotherBrainCartridgeAngleReference(short x, short y)
    {
        int quadrant = 0;
        ushort absoluteX = unchecked((ushort)x);
        ushort absoluteY = unchecked((ushort)y);
        if (x < 0)
        {
            quadrant += 2;
            absoluteX = unchecked((ushort)-absoluteX);
        }
        if (y < 0)
        {
            quadrant++;
            absoluteY = unchecked((ushort)-absoluteY);
        }

        if (absoluteY < absoluteX)
        {
            int divided = absoluteX == 0 ? 0 : (absoluteY << 8) / absoluteX;
            return quadrant switch
            {
                0 => unchecked((byte)((divided >> 3) + 64)),
                1 => unchecked((byte)(64 - (divided >> 3))),
                2 => unchecked((byte)(-64 - (divided >> 3))),
                _ => unchecked((byte)((divided >> 3) - 64)),
            };
        }

        int inverseDivided = absoluteY == 0 ? 0 : (absoluteX << 8) / absoluteY;
        return quadrant switch
        {
            0 => unchecked((byte)(128 - (inverseDivided >> 3))),
            1 => unchecked((byte)(inverseDivided >> 3)),
            2 => unchecked((byte)((inverseDivided >> 3) + 128)),
            _ => unchecked((byte)(-(inverseDivided >> 3))),
        };
    }

    private static byte CalculateMotherBrainOnionRingAngleReference(short x, short y)
    {
        int quadrant = 0;
        ushort absoluteX = unchecked((ushort)x);
        ushort absoluteY = unchecked((ushort)y);
        if (x < 0)
        {
            quadrant += 2;
            absoluteX = unchecked((ushort)-absoluteX);
        }
        if (y < 0)
        {
            quadrant++;
            absoluteY = unchecked((ushort)-absoluteY);
        }

        int divided;
        byte cartridgeAngle;
        if (absoluteY < absoluteX)
        {
            divided = absoluteX == 0 ? 0 : (absoluteY << 8) / absoluteX;
            cartridgeAngle = quadrant switch
            {
                0 => unchecked((byte)((divided >> 3) + 64)),
                1 => unchecked((byte)(64 - (divided >> 3))),
                2 => unchecked((byte)(-64 - (divided >> 3))),
                _ => unchecked((byte)((divided >> 3) - 64)),
            };
        }
        else
        {
            divided = absoluteY == 0 ? 0 : (absoluteX << 8) / absoluteY;
            cartridgeAngle = quadrant switch
            {
                0 => unchecked((byte)(128 - (divided >> 3))),
                1 => unchecked((byte)(divided >> 3)),
                2 => unchecked((byte)((divided >> 3) + 128)),
                _ => unchecked((byte)(-(divided >> 3))),
            };
        }

        byte angle = unchecked((byte)(0x80 - cartridgeAngle));
        return angle switch
        {
            >= 0x10 and < 0x48 => angle,
            >= 0x48 and < 0xc0 => 0x48,
            _ => 0x10,
        };
    }

    private static ushort MultiplyMotherBrainSineReference(
        ISnesAddressSpace bus,
        ushort speed,
        byte angle)
    {
        short sample = unchecked((short)ReadWord(bus, 0xa0b443 + angle * 2));
        int magnitude = speed * Math.Abs((int)sample) >> 8;
        return unchecked((ushort)(sample < 0 ? -magnitude : magnitude));
    }

    /// <summary>
    /// Runs the room-authored glass object to completion in a fresh encounter. Keeping this
    /// separate from the turret audit proves shared-pool contention without allowing forty
    /// shard requests to perturb that audit's deterministic cooldown and bullet sample.
    /// </summary>
    private static void AuditGlassSequence(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room)
    {
        const ushort glassHeader = 0xd6de;
        const ushort shardDefinition = 0xcefc;
        const int destroyedEvent = 2;

        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState(0x1234);
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            XPosition = 0x0080,
            YPosition = 0x00a0,
            Pose = SamusPoseIds.FacingRightNormalPose,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);

        int glassBlockIndex = assets.LevelData.GetBlockIndex(9, 5);
        ushort originalGlassWord = assets.LevelData.GetCollisionBlockByIndex(glassBlockIndex).LevelWord;
        var events = new HashSet<int>();
        var plms = new RoomPlmSystem();
        BackgroundTilemapStreamer streamer =
            assets.LevelData.CreateBackgroundStreamer(sizeOfBg2: 0x0800);
        if (plms.LoadRoomPopulation(
                bus,
                assets.LevelData,
                streamer,
                vram,
                room.State.PlmPointer,
                random,
                room.AreaIndex,
                getSamus: () => samus,
                isAreaTorizoDefeated: () => false,
                hasAreaBossBit: _ => false,
                hasEvent: events.Contains,
                setEvent: eventNumber => events.Add(eventNumber)) == 0)
        {
            throw new InvalidDataException(
                $"Mother Brain room did not load PLM ${glassHeader:X4}.");
        }

        RoomCollisionBlock glass = assets.LevelData.GetCollisionBlockByIndex(glassBlockIndex);
        if (plms.ActiveCount != 1 || !plms.MotherBrainGlassWasLoaded ||
            plms.MotherBrainGlassWasDeleted || plms.MotherBrainGlassRoomArgument != 0 ||
            glass.CollisionType != RoomCollisionType.SolidBlock || glass.Behavior != 0x44 ||
            (glass.LevelWord & 0x0fff) != (originalGlassWord & 0x0fff))
        {
            throw new InvalidDataException(
                $"Mother Brain glass setup diverged: PLMs={plms.ActiveCount}, " +
                $"arg={plms.MotherBrainGlassRoomArgument}, block=${glass.LevelWord:X4}/" +
                $"BTS=${glass.Behavior:X2}.");
        }

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
            isAreaBossDefeated: () => false,
            hasEvent: events.Contains,
            setEvent: eventNumber => events.Add((int)eventNumber),
            incrementMotherBrainGlassRoomArgument: plms.IncrementMotherBrainGlassRoomArgument);

        MotherBrainEnemyState state = enemies.MotherBrain ?? throw new InvalidDataException(
            "Mother Brain glass audit lost the typed encounter state.");
        RoomEnemySlot head = state.Head ?? throw new InvalidDataException(
            "Mother Brain glass audit did not link the head record.");
        samus.XPosition = unchecked((ushort)(state.Body.XPosition + 25));
        samus.YPosition = head.YPosition;
        samus.Health = 999;
        enemies.StepFrame(0, 0, timeIsFrozen: false, samus, level: assets.LevelData);
        ushort expectedContactHealth = unchecked((ushort)(999 - state.Body.Definition.Damage));
        if (samus.Health != expectedContactHealth || samus.InvincibilityTimer != 96 ||
            samus.KnockbackTimer != 5 || samus.KnockbackXDirection != 1 ||
            samus.Kinematics.ExtraXDisplacement < 4 ||
            samus.Kinematics.ExtraYDisplacement != 4 ||
            samus.Kinematics.ExtraXSubdisplacement != 0 ||
            samus.Kinematics.ExtraYSubdisplacement != 0 ||
            samus.Kinematics.YDirection != 2)
        {
            throw new InvalidDataException(
                $"Mother Brain custom Samus collision diverged: health={samus.Health}/" +
                $"{expectedContactHealth}, timers={samus.InvincibilityTimer}/" +
                $"{samus.KnockbackTimer}, knockback={samus.KnockbackXDirection}, " +
                $"extra=({samus.Kinematics.ExtraXDisplacement}." +
                $"{samus.Kinematics.ExtraXSubdisplacement}," +
                $"{samus.Kinematics.ExtraYDisplacement}." +
                $"{samus.Kinematics.ExtraYSubdisplacement}), ydir={samus.Kinematics.YDirection}.");
        }
        samus.XPosition = 32;
        samus.YPosition = 220;
        samus.Health = 999;
        samus.InvincibilityTimer = 0;
        samus.KnockbackTimer = 0;
        samus.Kinematics.ExtraXDisplacement = 0;
        samus.Kinematics.ExtraYDisplacement = 0;
        samus.Kinematics.YDirection = 0;

        // Exercise the real bank-$A0 collision walker, not just the PLM argument seam. The
        // hidden body has property `$0400`, so only the head enters the interactive list.
        // Repositioning a normally produced shot removes travel/terrain from this callback
        // audit while retaining the producer's counter and impact lifecycle state.
        var projectiles = new SamusProjectileSystem();
        var sharedProjectiles = new SamusBombProjectileSystem();
        AuditRejectedBeam();
        AuditAcceptedMissile(super: false, expectedDamage: 100);
        AuditAcceptedMissile(super: true, expectedDamage: 300);

        void AuditRejectedBeam()
        {
            projectiles.Reset();
            sharedProjectiles.Reset();
            samus.SelectedHudItem = 0;
            samus.EquippedBeams = 0;
            SamusProjectileSlot shot = ProduceAndPlaceShot(
                expectedFamily: SamusProjectileFamily.Beam);
            ushort healthBefore = head.Health;
            int hits = enemies.ResolveOrdinaryProjectileHits(
                bus,
                projectiles,
                sharedProjectiles,
                samus);
            if (hits != 1 || !shot.IsActive || shot.PackedDirection.HasLowByteLifecycleState ||
                head.Health != healthBefore || plms.MotherBrainGlassRoomArgument != 0)
            {
                throw new InvalidDataException(
                    $"Mother Brain rejected beam diverged: hits={hits}, active={shot.IsActive}, " +
                    $"direction=${shot.Direction:X4}, health={head.Health}/{healthBefore}, " +
                    $"glass={plms.MotherBrainGlassRoomArgument}.");
            }
        }

        void AuditAcceptedMissile(bool super, ushort expectedDamage)
        {
            projectiles.Reset();
            sharedProjectiles.Reset();
            samus.SelectedHudItem = super ? (ushort)2 : (ushort)1;
            if (super)
                samus.SuperMissiles = 1;
            else
                samus.Missiles = 1;
            SamusProjectileSlot shot = ProduceAndPlaceShot(
                super ? SamusProjectileFamily.SuperMissile : SamusProjectileFamily.Missile);
            ushort healthBefore = head.Health;
            ushort argumentBefore = plms.MotherBrainGlassRoomArgument;
            int hits = enemies.ResolveOrdinaryProjectileHits(
                bus,
                projectiles,
                sharedProjectiles,
                samus);
            ushort expectedFlash = unchecked((ushort)(
                (head.HurtAiTime == 0 ? 4 : head.HurtAiTime) + 8));
            if (hits != 1 || shot.PackedType.Family != SamusProjectileFamily.MissileExplosion ||
                head.Health != healthBefore - expectedDamage ||
                plms.MotherBrainGlassRoomArgument != argumentBefore + 1 ||
                head.FlashTimer != expectedFlash)
            {
                throw new InvalidDataException(
                    $"Mother Brain {(super ? "super" : "missile")} hit diverged: hits={hits}, " +
                    $"family=${shot.PackedType.FamilyValue:X3}, health={head.Health}/" +
                    $"{healthBefore - expectedDamage}, flash={head.FlashTimer}, " +
                    $"glass={plms.MotherBrainGlassRoomArgument}/{argumentBefore + 1}.");
            }
        }

        SamusProjectileSlot ProduceAndPlaceShot(SamusProjectileFamily expectedFamily)
        {
            const ushort shoot = (ushort)SnesButton.X;
            SamusProjectileFrameResult result = projectiles.StepFrame(
                bus,
                assets.LevelData,
                samus,
                controllerInput: shoot,
                controllerNewInput: shoot,
                layer1X: 0,
                layer1Y: 0,
                sharedProjectiles,
                roomPlms: plms);
            if (result.FiredSlot is not int slotIndex)
                throw new InvalidDataException($"Could not produce {expectedFamily} for Mother Brain audit.");
            SamusProjectileSlot shot = projectiles.Slots[slotIndex];
            // The gameplay alpha pass follows production immediately and can encounter the
            // room terrain at Samus's fixture position. Rebuild only the shot words that
            // existed at the producer return; allocation and the private projectile counter
            // remain those of the real producer rather than a hand-created test actor.
            (shot.Type, shot.Damage) = expectedFamily switch
            {
                SamusProjectileFamily.Beam => ((ushort)0x8000, (ushort)20),
                SamusProjectileFamily.Missile => ((ushort)0x8100, (ushort)100),
                SamusProjectileFamily.SuperMissile => ((ushort)0x8200, (ushort)300),
                _ => throw new ArgumentOutOfRangeException(nameof(expectedFamily)),
            };
            shot.Direction = (ushort)SamusProjectileDirection.Right;
            if (shot.InstructionPointer == 0)
                shot.InstructionPointer = 1;
            shot.XPosition = head.XPosition;
            shot.YPosition = head.YPosition;
            shot.XRadius = 1;
            shot.YRadius = 1;
            return shot;
        }

        // The first handler pass executes both conditional branches, installs `$D1E6`, and
        // reaches the timer-one `$9717` drawing record. Subsequent direct increments model
        // the exact write performed by accepted head-shot AI while isolating PLM behavior.
        plms.Step(bus, assets.LevelData, streamer, 0, 0, 0, assets.Scrolls);
        int shardRequests = 0;
        int shatterSounds = 0;
        bool sawLiveShard = false;
        for (int frame = 0; frame < 512 && !plms.MotherBrainGlassWasDeleted; frame++)
        {
            if (plms.MotherBrainGlassRoomArgument < 18)
                plms.IncrementMotherBrainGlassRoomArgument();

            enemies.StepEnemyProjectiles(
                assets.LevelData,
                samus: null,
                cameraX: 0,
                cameraY: 0,
                nmiFrameCounter8: unchecked((byte)frame));
            plms.Step(bus, assets.LevelData, streamer, 0, 0, 0, assets.Scrolls);
            if (plms.SoundRequests.Any(request =>
                    request is { Library: 3, SoundId: 0x2e, MaximumQueued: 15 }))
            {
                shatterSounds++;
            }

            foreach (MotherBrainGlassProjectileRequest request in
                     plms.MotherBrainGlassProjectileRequests)
            {
                if (request.DefinitionPointer != shardDefinition ||
                    request.Parameter is not (0 or 2 or 4) ||
                    request.PlmBlockX != 9 || request.PlmBlockY != 5)
                {
                    throw new InvalidDataException(
                        $"Mother Brain glass emitted invalid shard request {request}.");
                }
                enemies.SpawnMotherBrainGlassProjectile(request);
                shardRequests++;
            }

            sawLiveShard |= enemies.EnemyProjectiles.Any(projectile =>
                (ushort)projectile.Kind == shardDefinition &&
                projectile.GraphicsIndex == 0x0640 &&
                projectile.PreInstruction == 0xce9b);
        }

        if (!plms.MotherBrainGlassWasDeleted || plms.ActiveCount != 0 ||
            plms.MotherBrainGlassRoomArgument != 18 || !events.Contains(destroyedEvent) ||
            shardRequests != 40 || shatterSounds != 10 || !sawLiveShard)
        {
            throw new InvalidDataException(
                $"Mother Brain glass sequence diverged: deleted={plms.MotherBrainGlassWasDeleted}, " +
                $"PLMs={plms.ActiveCount}, arg={plms.MotherBrainGlassRoomArgument}, " +
                $"event={events.Contains(destroyedEvent)}, shards={shardRequests}, " +
                $"sounds={shatterSounds}, liveShard={sawLiveShard}.");
        }
    }

    private static void AuditInitialTurretPool(
        ISnesAddressSpace bus,
        RoomEnemySystem enemies)
    {
        if (enemies.ActiveEnemyProjectileCount != 12)
        {
            throw new InvalidDataException(
                $"Mother Brain allocated {enemies.ActiveEnemyProjectileCount} initial projectiles, not 12.");
        }

        // The encounter is loaded with seed $1234 above. Replaying only the two documented
        // RNG calls per turret proves both consumption order and the two native lower clamps.
        var expectedRandom = new Bank80SystemState(0x1234);
        for (ushort parameter = 0; parameter < 12; parameter++)
        {
            RoomEnemyProjectileSlot turret = enemies.EnemyProjectiles[17 - parameter];
            int offset = parameter * 2;
            ushort direction = ReadWord(bus, 0x86bee1 + offset);
            ushort expectedRotationTimer = Math.Max(
                unchecked((byte)expectedRandom.NextRandom()),
                (byte)0x20);
            ushort expectedCooldownTimer = Math.Max(
                unchecked((byte)expectedRandom.NextRandom()),
                (byte)0x80);
            ushort expectedList = ReadWord(bus, 0x86beb9 + direction * 2);

            if ((ushort)turret.Kind != TurretDefinition ||
                turret.SlotIndex != 17 - parameter ||
                turret.DirectionParameter != parameter ||
                turret.XPosition != ReadWord(bus, 0x86be89 + offset) ||
                turret.YPosition != ReadWord(bus, 0x86bea1 + offset) ||
                turret.XSubposition != ReadWord(bus, 0x86bec9 + offset) ||
                turret.YSubposition != (ushort)(0x0100 | direction) ||
                turret.XVelocity != expectedRotationTimer ||
                turret.YVelocity != expectedCooldownTimer ||
                turret.InstructionPointer != expectedList ||
                turret.InstructionTimer != 1 || turret.PreInstruction != 0xbfdf ||
                turret.GraphicsIndex != 0x0400 || turret.XRadius != 0 || turret.YRadius != 0 ||
                turret.Damage != 0 || turret.CanDamageSamus || !turret.PersistsOnSamusContact ||
                turret.BlocksSamusProjectiles)
            {
                throw new InvalidDataException(
                    $"Mother Brain turret parameter {parameter} diverged: slot={turret.SlotIndex}, " +
                    $"kind=$86:{(ushort)turret.Kind:X4}, pos=({turret.XPosition:X4}," +
                    $"{turret.YPosition:X4}), dir/delta=${turret.YSubposition:X4}, " +
                    $"timers={turret.XVelocity:X4}/{turret.YVelocity:X4}.");
            }
        }

        // Native indexes $00-$0A remain available for at most six simultaneous bullets.
        for (int slot = 0; slot < 6; slot++)
        {
            if (enemies.EnemyProjectiles[slot].IsActive)
                throw new InvalidDataException($"Mother Brain unexpectedly occupied spare projectile slot {slot}.");
        }
    }

    private static void AuditTurretRuntime(
        ISnesAddressSpace bus,
        RoomLevelData level,
        RoomEnemySystem enemies,
        SamusState samus)
    {
        const ushort cameraX = 0x00d0;
        RoomEnemyProjectileSlot? bullet = null;
        for (int frame = 0; frame < 0x0100 && bullet is null; frame++)
        {
            enemies.StepEnemyProjectiles(
                level,
                samus: null,
                cameraX: cameraX,
                cameraY: 0,
                nmiFrameCounter8: unchecked((byte)frame));
            bullet = enemies.EnemyProjectiles.FirstOrDefault(projectile =>
                (ushort)projectile.Kind == TurretBulletDefinition);
        }

        if (bullet is null)
            throw new InvalidDataException("No visible Mother Brain turret fired within one maximum cooldown.");

        ushort direction = bullet.DirectionParameter;
        ushort directionOffset = unchecked((ushort)(direction * 2));
        ushort expectedXVelocity = ReadWord(bus, 0x86bfbf + directionOffset);
        ushort expectedYVelocity = ReadWord(bus, 0x86bfcf + directionOffset);
        ushort selectedInstructionList = ReadWord(bus, 0x86c133 + directionOffset);
        ushort expectedInstructionTimer = ReadWord(bus, 0x860000 | selectedInstructionList);
        ushort expectedSpritemap = ReadWord(
            bus,
            0x860000 | unchecked((ushort)(selectedInstructionList + 2)));

        // The native bank-$86 scheduler visits physical slots $22,$20,...,$00. A turret
        // fires from one of the twelve occupied high slots and AllocateEnemyProjectile
        // chooses the highest free slot below it. Consequently the newborn bullet is still
        // ahead of the scheduler and executes once in the SAME outer pass. Reconstruct its
        // post-pass position from the turret muzzle and one exact 8.8 velocity addition;
        // looking for the untouched spawn position would encode the old ascending-order bug.
        RoomEnemyProjectileSlot? source = enemies.EnemyProjectiles.FirstOrDefault(projectile =>
        {
            if ((ushort)projectile.Kind != TurretDefinition)
                return false;

            ushort muzzleX = unchecked((ushort)(projectile.XPosition +
                ReadWord(bus, 0x86bf9f + directionOffset)));
            ushort muzzleY = unchecked((ushort)(projectile.YPosition +
                ReadWord(bus, 0x86bfaf + directionOffset)));
            (ushort postPassX, ushort postPassXSubposition) = AddEightBitVelocityReference(
                muzzleX,
                0,
                expectedXVelocity);
            (ushort postPassY, ushort postPassYSubposition) = AddEightBitVelocityReference(
                muzzleY,
                0,
                expectedYVelocity);
            return bullet.XPosition == postPassX &&
                bullet.XSubposition == postPassXSubposition &&
                bullet.YPosition == postPassY &&
                bullet.YSubposition == postPassYSubposition;
        });
        if (source is null || bullet.PreInstruction != 0xc0e0 ||
            bullet.InstructionPointer != unchecked((ushort)(selectedInstructionList + 4)) ||
            bullet.InstructionTimer != expectedInstructionTimer ||
            bullet.SpritemapPointer != expectedSpritemap || bullet.GraphicsIndex != 0x0400 ||
            bullet.XRadius != 3 || bullet.YRadius != 3 || bullet.Damage != 0x0014 ||
            bullet.XVelocity != expectedXVelocity || bullet.YVelocity != expectedYVelocity ||
            bullet.Variable0 != directionOffset || bullet.Variable1 != 0 ||
            !bullet.CanDamageSamus || !bullet.PersistsOnSamusContact ||
            !bullet.BlocksSamusProjectiles)
        {
            throw new InvalidDataException(
                $"Mother Brain turret bullet initialization diverged: slot={bullet.SlotIndex}, " +
                $"direction={direction}, pos=({bullet.XPosition:X4},{bullet.YPosition:X4}), " +
                $"velocity=({bullet.XVelocity:X4},{bullet.YVelocity:X4}), " +
                $"list=${bullet.InstructionPointer:X4}.");
        }

        int bulletSlot = bullet.SlotIndex;
        (ushort expectedX, ushort expectedXSubposition) = AddEightBitVelocityReference(
            bullet.XPosition,
            bullet.XSubposition,
            bullet.XVelocity);
        (ushort expectedY, ushort expectedYSubposition) = AddEightBitVelocityReference(
            bullet.YPosition,
            bullet.YSubposition,
            bullet.YVelocity);
        enemies.StepEnemyProjectiles(
            level,
            samus: null,
            cameraX: cameraX,
            cameraY: 0,
            nmiFrameCounter8: 0);
        bullet = enemies.EnemyProjectiles[bulletSlot];
        if (!bullet.IsActive || bullet.XPosition != expectedX ||
            bullet.XSubposition != expectedXSubposition || bullet.YPosition != expectedY ||
            bullet.YSubposition != expectedYSubposition || bullet.BlocksSamusProjectiles ||
            bullet.SpritemapPointer == 0x8000)
        {
            throw new InvalidDataException(
                $"Mother Brain turret bullet movement/flicker diverged in slot {bulletSlot}: " +
                $"active={bullet.IsActive}, pos=({bullet.XPosition:X4}.{bullet.XSubposition:X4}," +
                $"{bullet.YPosition:X4}.{bullet.YSubposition:X4}), block={bullet.BlocksSamusProjectiles}.");
        }

        // Put Samus at the bullet's next 8.8 position. The common collision pass runs after
        // projectile movement, so this checks the exact $4014 persistent-contact behavior:
        // twenty damage, 96 invincibility frames, and a switch to ROM list $C19A.
        (ushort contactX, _) = AddEightBitVelocityReference(
            bullet.XPosition,
            bullet.XSubposition,
            bullet.XVelocity);
        (ushort contactY, _) = AddEightBitVelocityReference(
            bullet.YPosition,
            bullet.YSubposition,
            bullet.YVelocity);
        samus.XPosition = contactX;
        samus.YPosition = contactY;
        samus.Health = 999;
        samus.InvincibilityTimer = 0;
        enemies.StepEnemyProjectiles(
            level,
            samus,
            cameraX: cameraX,
            cameraY: 0,
            nmiFrameCounter8: 1);
        bullet = enemies.EnemyProjectiles[bulletSlot];
        if (!bullet.IsActive || samus.Health != 979 || samus.InvincibilityTimer != 96 ||
            bullet.InstructionPointer != 0xc19a || bullet.InstructionTimer != 1)
        {
            throw new InvalidDataException(
                $"Mother Brain bullet contact diverged: active={bullet.IsActive}, " +
                $"health={samus.Health}, invincibility={samus.InvincibilityTimer}, " +
                $"list=${bullet.InstructionPointer:X4}/{bullet.InstructionTimer}.");
        }

        enemies.StepEnemyProjectiles(
            level,
            samus,
            cameraX: cameraX,
            cameraY: 0,
            nmiFrameCounter8: 2);
        bullet = enemies.EnemyProjectiles[bulletSlot];
        if (!bullet.IsActive || bullet.GraphicsIndex != 0 || bullet.PreInstruction != 0x8170 ||
            bullet.InstructionTimer != 8 ||
            bullet.SpritemapPointer != ReadWord(bus, 0x86c1a0))
        {
            throw new InvalidDataException(
                $"Mother Brain bullet contact animation diverged: active={bullet.IsActive}, " +
                $"gfx=${bullet.GraphicsIndex:X4}, pre=${bullet.PreInstruction:X4}, " +
                $"timer={bullet.InstructionTimer}, map=${bullet.SpritemapPointer:X4}.");
        }

        var projectileOam = new OamBuffer();
        projectileOam.BeginFrame();
        enemies.DrawEnemyProjectiles(projectileOam, cameraX, 0);
        if (projectileOam.NextByteOffset == 0)
            throw new InvalidDataException("Mother Brain's live turret/bullet pool emitted no OAM.");
    }

    private static (ushort Position, ushort Subposition) AddEightBitVelocityReference(
        ushort position,
        ushort subposition,
        ushort velocity)
    {
        int fixedPosition = (position << 16) | subposition;
        fixedPosition = unchecked(fixedPosition + (unchecked((short)velocity) << 8));
        return (unchecked((ushort)(fixedPosition >> 16)), unchecked((ushort)fixedPosition));
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) => unchecked((ushort)(
        bus.ReadByte(address) |
        (bus.ReadByte((address & 0xff0000) | ((address + 1) & 0xffff)) << 8)));
}
