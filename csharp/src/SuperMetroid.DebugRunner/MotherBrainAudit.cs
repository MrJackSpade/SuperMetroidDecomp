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
            Pose = SamusState.FacingRightNormalPose,
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

        MotherBrainCorpseRotEntry firstRotEntry = state.CorpseRotting.ReadEntry(bus, 0);
        MotherBrainCorpseRotEntry lastRotEntry =
            state.CorpseRotting.ReadEntry(bus, MotherBrainCorpseRottingState.EntryCount - 1);
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
            "tube PLMs, four ceiling tubes, and five physical tube actors matched the " +
            "untouched cartridge.");
        return 0;
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
            Pose = SamusState.FacingRightNormalPose,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);

        byte[] scrollBytes = new byte[50];
        scrollBytes[0] = 2;
        scrollBytes[1] = 1;
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
            readRoomScrollByte: index => scrollBytes[index]);

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
            Pose = SamusState.FacingRightNormalPose,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);

        int glassBlockIndex = assets.LevelData.GetBlockIndex(9, 5);
        ushort originalGlassWord = assets.LevelData.GetCollisionBlockByIndex(glassBlockIndex).LevelWord;
        var events = new HashSet<int>();
        var plms = new RoomPlmSystem();
        BackgroundTilemapStreamer streamer =
            assets.LevelData.CreateBackgroundStreamer(sizeOfBg2: 0x0800);
        if (!plms.TryLoadMotherBrainGlassPopulation(
                bus,
                assets.LevelData,
                streamer,
                room.State.PlmPointer,
                hasAreaBossBit: _ => false,
                hasEvent: events.Contains,
                setEvent: eventNumber => events.Add(eventNumber)))
        {
            throw new InvalidDataException(
                $"Mother Brain room did not load PLM ${glassHeader:X4}.");
        }

        RoomCollisionBlock glass = assets.LevelData.GetCollisionBlockByIndex(glassBlockIndex);
        if (plms.ActiveCount != 1 || !plms.MotherBrainGlassWasLoaded ||
            plms.MotherBrainGlassWasDeleted || plms.MotherBrainGlassRoomArgument != 0 ||
            glass.CollisionType != 8 || glass.Behavior != 0x44 ||
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
        RoomEnemyProjectileSlot? source = enemies.EnemyProjectiles.FirstOrDefault(projectile =>
            (ushort)projectile.Kind == TurretDefinition &&
            bullet.XPosition == unchecked((ushort)(projectile.XPosition +
                ReadWord(bus, 0x86bf9f + directionOffset))) &&
            bullet.YPosition == unchecked((ushort)(projectile.YPosition +
                ReadWord(bus, 0x86bfaf + directionOffset))));
        if (source is null || bullet.PreInstruction != 0xc0e0 ||
            bullet.InstructionPointer != 0xc131 || bullet.InstructionTimer != 1 ||
            bullet.SpritemapPointer != 0x8000 || bullet.GraphicsIndex != 0x0400 ||
            bullet.XRadius != 3 || bullet.YRadius != 3 || bullet.Damage != 0x0014 ||
            bullet.XVelocity != ReadWord(bus, 0x86bfbf + directionOffset) ||
            bullet.YVelocity != ReadWord(bus, 0x86bfcf + directionOffset) ||
            bullet.Variable0 != directionOffset || bullet.Variable1 != 0 ||
            !bullet.CanDamageSamus || !bullet.PersistsOnSamusContact ||
            bullet.BlocksSamusProjectiles)
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
            bullet.YSubposition != expectedYSubposition || !bullet.BlocksSamusProjectiles ||
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
