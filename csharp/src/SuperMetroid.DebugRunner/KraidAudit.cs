using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// ROM-backed end-to-end checkpoint for Kraid's room lockout, rise, repeating first phase,
/// growth, second-phase attacks, cartridge collision reactions, and complete death sequence.
/// </summary>
internal static class KraidAudit
{
    private const ushort RoomPointer = 0xa59f;
    private const ushort PopulationPointer = 0x9eb5;
    private const ushort IncomingDoorPointer = 0x91b6;
    private const ushort CameraX = 0;
    private const ushort CameraY = 256;

    private static readonly ushort[] ExpectedDefinitions =
        [0xe2bf, 0xe2ff, 0xe33f, 0xe37f, 0xe3bf, 0xe3ff, 0xe43f, 0xe47f];

    /// <summary>
    /// Captures the complete retail-room rise through the production room loader and frame
    /// renderer. This is intentionally separate from the pass/fail audit so a visual defect
    /// can be localized to its first frame before its exact pixel assertion is finalized.
    /// </summary>
    public static int CaptureRise(string romPath, string outputDirectory)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeDoorHeader door = CartridgeDoorHeader.Load(bus, IncomingDoorPointer);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, RoomPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        Console.WriteLine(
            $"Kraid character bytes: room=${assets.RoomCharacters.Length:X}, " +
            $"CRE=${assets.CreCharacters.Length:X}.");
        var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomThroughDoorForVerification(door, CameraX, CameraY);

        SamusState samus = runtime.Samus ??
            throw new InvalidDataException("Kraid capture did not retain Samus.");
        samus.XPosition = 128;
        samus.YPosition = 456;
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);

        KraidEnemyState state = runtime.Enemies.Kraid ??
            throw new InvalidDataException("Kraid capture did not initialize encounter state.");
        RoomEnemySlot body = runtime.Enemies.Slots[0];
        Directory.CreateDirectory(outputDirectory);
        int frame;
        for (frame = 0; frame < 1200; frame++)
        {
            if (frame % 8 == 0 ||
                body.VariableA is (ushort)KraidAiFunction.GrowBreakCeilingPlatforms or
                    (ushort)KraidAiFunction.GrowSetBg2Priority or
                    (ushort)KraidAiFunction.GrowFinishBg2Update)
            {
                Rgba32[] pixels = SuperMetroidRuntimeFrameRenderer.Render(runtime);
                PngWriter.WriteRgba(
                    Path.Combine(
                        outputDirectory,
                        $"kraid-rise-{frame:D4}-{body.VariableA:X4}-{body.YPosition:D3}.png"),
                    FrontendFrame.Width,
                    FrontendFrame.Height,
                    pixels);
            }

            if (body.VariableA == (ushort)KraidAiFunction.MainloopThinking)
                break;
            runtime.StepFrame(controller1Input: 0);
        }

        if (body.VariableA != (ushort)KraidAiFunction.MainloopThinking)
            throw new InvalidDataException("Kraid capture did not reach the first-phase main loop.");

        for (int hit = 0; hit < 2; hit++)
        {
            AdvanceRuntimeUntilOpenMouth(runtime, body, state);
            if (StrikeKraidMouth(bus, runtime.Enemies, body, state, projectileDamage: 100) != 1)
                throw new InvalidDataException("Kraid capture could not trigger the growth phase.");
            runtime.StepFrame(controller1Input: 0);
        }

        for (int growthFrame = 0; growthFrame < 1800; growthFrame++)
        {
            Rgba32[] pixels = SuperMetroidRuntimeFrameRenderer.Render(runtime);
            PngWriter.WriteRgba(
                Path.Combine(
                    outputDirectory,
                    $"kraid-growth-{growthFrame:D4}-{body.VariableA:X4}-{body.YPosition:D3}.png"),
                FrontendFrame.Width,
                FrontendFrame.Height,
                pixels);
            if (growthFrame == 341)
            {
                var visibleWords = new HashSet<ushort>();
                for (int screenY = 80; screenY < 136; screenY += 8)
                {
                    for (int screenX = 48; screenX < 240; screenX += 8)
                    {
                        int tileX = ((state.Bg2HorizontalScroll + screenX) & 0x01ff) >> 3;
                        int tileY = ((state.Bg2VerticalScroll + screenY) & 0x01ff) >> 3;
                        int page = (tileY >> 5) * 2 + (tileX >> 5);
                        int mapWord = KraidBackgroundRomData.LiveBg2TilemapWord +
                            page * 0x0400 + (tileY & 31) * 32 + (tileX & 31);
                        visibleWords.Add(runtime.Vram.ReadWord(mapWord));
                    }
                }
                Console.WriteLine(
                    "Kraid issue #268 rectangle tilemap words: " +
                    string.Join(',', visibleWords.Order().Select(word => $"${word:X4}")) +
                    $"; radius={body.XRadius}, BG2=(${state.Bg2HorizontalScroll:X4}," +
                    $"${state.Bg2VerticalScroll:X4}), body=({body.XPosition},{body.YPosition}).");
                PngWriter.WriteRgba(
                    Path.Combine(outputDirectory, "kraid-growth-0341-bg1.png"),
                    FrontendFrame.Width,
                    FrontendFrame.Height,
                    SnesBgTilemapRenderer.Render4BppViewport(
                        runtime.Vram,
                        runtime.Cgram,
                        SnesPpuLayout.GameplayBg1TilemapWord,
                        characterBaseWord: 0,
                        runtime.DisplayedGameplayPpu.Bg1HorizontalScroll,
                        runtime.DisplayedGameplayPpu.Bg1VerticalScroll,
                        FrontendFrame.Width,
                        FrontendFrame.Height));
                PngWriter.WriteRgba(
                    Path.Combine(outputDirectory, "kraid-growth-0341-bg2.png"),
                    FrontendFrame.Width,
                    FrontendFrame.Height,
                    SnesBgTilemapRenderer.Render4BppViewport(
                        runtime.Vram,
                        runtime.Cgram,
                        KraidBackgroundRomData.LiveBg2TilemapWord,
                        characterBaseWord: 0,
                        state.Bg2HorizontalScroll,
                        state.Bg2VerticalScroll,
                        FrontendFrame.Width,
                        FrontendFrame.Height,
                        KraidBackgroundRomData.TilemapWidthInTiles,
                        KraidBackgroundRomData.TilemapHeightInTiles));
            }
            if (body.VariableA == (ushort)KraidAiFunction.SecondPhaseThinking)
                break;
            runtime.StepFrame(controller1Input: 0);
        }

        if (body.VariableA != (ushort)KraidAiFunction.SecondPhaseThinking)
            throw new InvalidDataException("Kraid capture did not reach the second-phase main loop.");
        Console.WriteLine(
            $"Captured Kraid's production rise and growth through frame " +
            $"{runtime.NmiFrameCounter} to {outputDirectory}.");
        return 0;
    }

    public static int Run(string romPath, string? deathCaptureDirectory = null, bool observeFloor = false)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, RoomPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        VerifyRetailRoom(room);

        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState(0x1234);
        bool bossDefeated = false;
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            XPosition = 300,
            YPosition = 456,
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
            isAreaBossDefeated: () => bossDefeated,
            setRoomScrollState: assets.Scrolls.SetStorage,
            setAreaBossDefeated: () => bossDefeated = true,
            cameraX: CameraX,
            cameraY: CameraY);

        KraidEnemyState state = enemies.Kraid ??
            throw new InvalidDataException("Live Kraid room did not allocate typed encounter state.");
        RoomEnemySlot body = enemies.Slots[0];
        if (enemies.EnemyCount != ExpectedDefinitions.Length ||
            body.XPosition != 176 || body.YPosition != 592 || body.Health != 1000 ||
            body.VariableA != (ushort)KraidAiFunction.RestrictSamusToFirstScreen ||
            body.VariableF != 300 || !state.BackgroundTilemapsPrepared)
        {
            throw new InvalidDataException(
                $"Kraid initialization mismatch: count={enemies.EnemyCount}, body=" +
                $"({body.XPosition},{body.YPosition}) hp={body.Health}, " +
                $"function=$A7:{body.VariableA:X4}, timer={body.VariableF}, " +
                $"BG prepared={state.BackgroundTilemapsPrepared}.");
        }
        for (int slot = 0; slot < ExpectedDefinitions.Length; slot++)
        {
            if (enemies.Slots[slot].EnemyDefinitionPointer != ExpectedDefinitions[slot])
            {
                throw new InvalidDataException(
                    $"Kraid slot {slot} loaded ${enemies.Slots[slot].EnemyDefinitionPointer:X4}, " +
                    $"expected ${ExpectedDefinitions[slot]:X4}.");
            }
        }
        ushort[] expectedEighths = [125, 250, 375, 500, 625, 750, 875, 1000];
        ushort[] expectedQuarters = [250, 500, 750, 1000];
        if (!state.HealthEighthThresholds.SequenceEqual(expectedEighths) ||
            !state.HealthQuarterThresholds.SequenceEqual(expectedQuarters))
        {
            throw new InvalidDataException("Kraid health phase thresholds do not match retail arithmetic.");
        }

        VerifyArmContact(enemies, samus, assets.LevelData);
        VerifyMultipartShotCallbacks(bus, enemies, samus);

        var functions = new HashSet<KraidAiFunction>();
        var observedProjectileKinds = new HashSet<RoomEnemyProjectileKind>();
        var contactedProjectileKinds = new HashSet<RoomEnemyProjectileKind>();
        bool sawBattleMusicRequest = false;
        ushort nailStartX = enemies.Slots[6].XPosition;
        bool sawNailMovement = false;
        int frame;
        for (frame = 0; frame < 1200; frame++)
        {
            functions.Add((KraidAiFunction)body.VariableA);
            if (body.VariableA == (ushort)KraidAiFunction.MainloopThinking)
                break;
            enemies.StepFrame(
                CameraX,
                CameraY,
                timeIsFrozen: false,
                samus,
                level: assets.LevelData);
            sawBattleMusicRequest |= enemies.MusicRequests.Any(
                request => request.Command.RawValue == 5);
            ProbeFirstUnauditedProjectileContact(
                bus,
                enemies,
                assets.LevelData,
                observedProjectileKinds,
                contactedProjectileKinds);
            enemies.StepEnemyProjectiles(
                assets.LevelData,
                samus,
                cameraX: CameraX,
                cameraY: CameraY);
            sawNailMovement |= enemies.Slots[6].XPosition != nailStartX;
        }

        if (body.VariableA != (ushort)KraidAiFunction.MainloopThinking ||
            samus.XPosition != 256 || body.XPosition != 176 || body.YPosition >= 457 ||
            state.TopTilemapUploadCount != 1 || state.BottomTilemapUploadCount != 1 ||
            !sawBattleMusicRequest || state.RiseRockSpawnRequestCount == 0 ||
            state.SpawnedRiseRockCount == 0 || !sawNailMovement || functions.Count < 6)
        {
            throw new InvalidDataException(
                $"Kraid rise mismatch after {frame} frames: function=$A7:{body.VariableA:X4}, " +
                $"Samus X={samus.XPosition}, body=({body.XPosition},{body.YPosition}), " +
                $"uploads={state.TopTilemapUploadCount}/{state.BottomTilemapUploadCount}, " +
                $"music observed={sawBattleMusicRequest}, rocks=" +
                $"{state.SpawnedRiseRockCount}/{state.RiseRockSpawnRequestCount}, " +
                $"nail moved={sawNailMovement}, functions={functions.Count}.");
        }

        // Continue well beyond the setup seam. This covers more than one independently
        // timed mouth cycle and gives the foot enough time to complete a full 176 -> 92 ->
        // 176 lunge/retreat path through its real bank-$A7 instruction lists.
        var combatFunctions = new HashSet<KraidAiFunction>();
        var footFunctions = new HashSet<KraidAiFunction>();
        var headTilemaps = new HashSet<ushort>();
        bool sawFootstep = false;
        bool sawLiveSpitRock = false;
        bool sawSpitRockMovement = false;
        var previousSpitPositions = new Dictionary<int, (ushort X, ushort Y)>();
        ushort minimumBodyX = body.XPosition;
        for (int combatFrame = 0; combatFrame < 2400; combatFrame++)
        {
            enemies.StepFrame(
                CameraX,
                CameraY,
                timeIsFrozen: false,
                samus,
                level: assets.LevelData);
            ProbeFirstUnauditedProjectileContact(
                bus,
                enemies,
                assets.LevelData,
                observedProjectileKinds,
                contactedProjectileKinds);
            foreach (RoomEnemyProjectileSlot projectile in enemies.EnemyProjectiles)
            {
                if (projectile.Kind != RoomEnemyProjectileKind.KraidSpitRock)
                    continue;
                sawLiveSpitRock = true;
                if (previousSpitPositions.TryGetValue(
                        projectile.SlotIndex,
                        out (ushort X, ushort Y) previous) &&
                    (previous.X != projectile.XPosition || previous.Y != projectile.YPosition))
                {
                    sawSpitRockMovement = true;
                }
                previousSpitPositions[projectile.SlotIndex] =
                    (projectile.XPosition, projectile.YPosition);
            }
            enemies.StepEnemyProjectiles(
                assets.LevelData,
                samus,
                cameraX: CameraX,
                cameraY: CameraY);
            combatFunctions.Add((KraidAiFunction)body.VariableA);
            footFunctions.Add((KraidAiFunction)enemies.Slots[5].VariableA);
            if (state.CurrentHeadTilemap != 0)
                headTilemaps.Add(state.CurrentHeadTilemap);
            minimumBodyX = Math.Min(minimumBodyX, body.XPosition);
            sawFootstep |= enemies.LastKraidSoundEffect is
            {
                SoundEffect: { Library: SoundEffectLibrary.Library2, Value: 0x76 },
            };
        }

        if (!combatFunctions.Contains(KraidAiFunction.MainloopThinking) ||
            !combatFunctions.Contains(KraidAiFunction.MainAttackWithMouthOpen) ||
            !footFunctions.Contains(KraidAiFunction.FootPrepareFirstPhaseLunge) ||
            !footFunctions.Contains(KraidAiFunction.FootFirstPhaseLunge) ||
            !footFunctions.Contains(KraidAiFunction.FootFirstPhaseRetreat) ||
            minimumBodyX != 92 || body.XPosition > 176 || headTilemaps.Count < 4 ||
            state.HeadTilemapUploadCount < 8 || state.SpitRockRequestCount < 3 ||
            state.SpawnedSpitRockCount == 0 || !sawLiveSpitRock || !sawSpitRockMovement ||
            state.RoarRequestCount == 0 || !sawFootstep)
        {
            throw new InvalidDataException(
                $"Kraid first phase did not cycle: body functions=" +
                $"{string.Join(',', combatFunctions)}, foot functions=" +
                $"{string.Join(',', footFunctions)}, X={minimumBodyX}..{body.XPosition}, " +
                $"head maps/uploads={headTilemaps.Count}/{state.HeadTilemapUploadCount}, " +
                $"spit spawned/requested/roars={state.SpawnedSpitRockCount}/" +
                $"{state.SpitRockRequestCount}/{state.RoarRequestCount}, moving=" +
                $"{sawSpitRockMovement}, " +
                $"footstep={sawFootstep}.");
        }

        VerifyKraidBg2VideoState(bus, vram, body, state);

        // Damage is accepted only by the inner hitbox pointer carried by a live open-mouth
        // head entry. Advance to that authored window instead of placing a shot against the
        // broad 56x144 header radius, which Kraid's BG2 body never uses for projectile hits.
        for (int mouthFrame = 0;
             mouthFrame < 1000 &&
             (body.VariableA != (ushort)KraidAiFunction.MainAttackWithMouthOpen ||
              state.InvulnerableMouthHitbox == ushort.MaxValue);
             mouthFrame++)
        {
            enemies.StepFrame(
                CameraX,
                CameraY,
                timeIsFrozen: false,
                samus,
                level: assets.LevelData);
            enemies.StepEnemyProjectiles(
                assets.LevelData,
                samus,
                cameraX: CameraX,
                cameraY: CameraY);
        }
        if (state.InvulnerableMouthHitbox == ushort.MaxValue)
            throw new InvalidDataException("Kraid never exposed a live inner-mouth hitbox.");

        int mouthAddress = 0xa70000 | state.InvulnerableMouthHitbox;
        short mouthLeft = unchecked((short)ReadWord(bus, mouthAddress));
        short mouthTop = unchecked((short)ReadWord(bus, mouthAddress + 2));
        short mouthBottom = unchecked((short)ReadWord(bus, mouthAddress + 6));
        var kraidShots = new SamusProjectileSystem();
        SamusProjectileSlot missile = kraidShots.Slots[0];
        missile.ClearFields();
        missile.Type = 0x8100;
        missile.Damage = 100;
        missile.Direction = (ushort)SamusProjectileDirection.Right;
        missile.XPosition = unchecked((ushort)(body.XPosition + mouthLeft + 2));
        missile.YPosition = unchecked((ushort)(
            body.YPosition + (mouthTop + mouthBottom) / 2));
        missile.XRadius = 2;
        missile.YRadius = 2;
        missile.InstructionPointer = 0x9000;
        missile.InstructionTimer = 1;
        ushort healthBeforeMouthHit = body.Health;
        int mouthHits = enemies.ResolveKraidProjectileHits(
            bus,
            kraidShots,
            new SamusBombProjectileSystem());
        if (mouthHits != 1 || healthBeforeMouthHit - body.Health != 100 ||
            state.HurtFrame != 6 || state.HurtFrameTimer != 2 || body.FlashTimer == 0)
        {
            throw new InvalidDataException(
                $"Kraid mouth damage mismatch: hits={mouthHits}, health=" +
                $"{healthBeforeMouthHit}->{body.Health}, hurt=" +
                $"{state.HurtFrame}/{state.HurtFrameTimer}, flash={body.FlashTimer}, " +
                $"hitbox=$A7:{state.InvulnerableMouthHitbox:X4}.");
        }

        // A charged beam absorbed by the outer BG2 body does no HP damage. It sets the
        // three-reopen eye reaction flags, then the private palette code raises and lowers
        // the red/green channels of CGRAM colors 113-115 while advancing `$974A` head art.
        for (int closeFrame = 0;
             closeFrame < 1000 &&
             (body.VariableA != (ushort)KraidAiFunction.MainloopThinking ||
              state.InvulnerableMouthHitbox != ushort.MaxValue);
             closeFrame++)
        {
            enemies.StepFrame(
                CameraX,
                CameraY,
                timeIsFrozen: false,
                samus,
                level: assets.LevelData);
            enemies.StepEnemyProjectiles(assets.LevelData, samus, cameraX: CameraX, cameraY: CameraY);
        }
        var chargedShots = new SamusProjectileSystem();
        SamusProjectileSlot chargedBeam = chargedShots.Slots[0];
        chargedBeam.ClearFields();
        chargedBeam.Type = 0x8010;
        chargedBeam.Damage = 100;
        chargedBeam.Direction = (ushort)SamusProjectileDirection.Right;
        chargedBeam.XPosition = unchecked((ushort)(body.XPosition + 20));
        chargedBeam.YPosition = unchecked((ushort)(body.YPosition + 40));
        chargedBeam.XRadius = 4;
        chargedBeam.YRadius = 4;
        chargedBeam.InstructionPointer = 0x9000;
        chargedBeam.InstructionTimer = 1;
        ushort healthBeforeArmor = body.Health;
        ushort[] eyePaletteBefore = cgram.Colors.Slice(113, 3).ToArray();
        int armorHits = enemies.ResolveKraidProjectileHits(
            bus,
            chargedShots,
            new SamusBombProjectileSystem());
        if (armorHits != 1 || body.Health != healthBeforeArmor ||
            body.VariableA != (ushort)KraidAiFunction.InitializeEyeGlow ||
            (state.MouthFlags & 0x0303) != 0x0303)
        {
            throw new InvalidDataException(
                $"Kraid outer-body charged reaction mismatch: hits={armorHits}, " +
                $"health={healthBeforeArmor}->{body.Health}, function=$A7:{body.VariableA:X4}, " +
                $"flags=${state.MouthFlags:X4}.");
        }

        var eyeFunctions = new HashSet<KraidAiFunction>();
        bool sawEyePaletteChange = false;
        for (int eyeFrame = 0; eyeFrame < 700; eyeFrame++)
        {
            enemies.StepFrame(
                CameraX,
                CameraY,
                timeIsFrozen: false,
                samus,
                level: assets.LevelData);
            enemies.StepEnemyProjectiles(assets.LevelData, samus, cameraX: CameraX, cameraY: CameraY);
            eyeFunctions.Add((KraidAiFunction)body.VariableA);
            sawEyePaletteChange |= !cgram.Colors.Slice(113, 3).SequenceEqual(eyePaletteBefore);
        }
        if (!eyeFunctions.Contains(KraidAiFunction.GlowEye) ||
            !eyeFunctions.Contains(KraidAiFunction.UnglowEye) ||
            !eyeFunctions.Contains(KraidAiFunction.MouthOpenReaction) ||
            !sawEyePaletteChange)
        {
            throw new InvalidDataException(
                $"Kraid eye reaction did not cycle: functions=" +
                $"{string.Join(',', eyeFunctions)}, palette changed={sawEyePaletteChange}.");
        }

        // A second genuine mouth hit crosses the 875-HP threshold. The foot's next native
        // first-phase callback owns `$C005`, so do not invoke growth directly from the audit.
        for (int mouthFrame = 0;
             mouthFrame < 1200 &&
             (body.VariableA != (ushort)KraidAiFunction.MainAttackWithMouthOpen ||
              state.InvulnerableMouthHitbox == ushort.MaxValue);
             mouthFrame++)
        {
            enemies.StepFrame(
                CameraX,
                CameraY,
                timeIsFrozen: false,
                samus,
                level: assets.LevelData);
            enemies.StepEnemyProjectiles(assets.LevelData, samus, cameraX: CameraX, cameraY: CameraY);
        }
        ushort healthBeforeGrowthHit = body.Health;
        int growthHit = StrikeKraidMouth(
            bus,
            enemies,
            body,
            state,
            projectileDamage: 100);
        if (growthHit != 1 || body.Health != healthBeforeGrowthHit - 100 || body.Health >= 875)
        {
            throw new InvalidDataException(
                $"Kraid phase-change mouth hit mismatch: hits={growthHit}, " +
                $"health={healthBeforeGrowthHit}->{body.Health}.");
        }

        var growthFunctions = new HashSet<KraidAiFunction>();
        var secondPhaseFootFunctions = new HashSet<KraidAiFunction>();
        var lintFunctions = new HashSet<KraidAiFunction>();
        var lintFlight = new KraidLintFlightAudit();
        var growthCeilingPlms = new List<KraidPlmRequest>();
        ushort? secondPhaseStartY = null;
        for (int growthFrame = 0; growthFrame < 2600; growthFrame++)
        {
            growthFunctions.Add((KraidAiFunction)body.VariableA);
            if (body.VariableA == (ushort)KraidAiFunction.SecondPhaseThinking)
                secondPhaseStartY ??= body.YPosition;
            secondPhaseFootFunctions.Add((KraidAiFunction)enemies.Slots[5].VariableA);
            for (int lintSlot = 2; lintSlot <= 4; lintSlot++)
                lintFunctions.Add((KraidAiFunction)enemies.Slots[lintSlot].VariableA);
            lintFlight.BeforeFrame(enemies);
            enemies.StepFrame(
                CameraX,
                CameraY,
                timeIsFrozen: false,
                samus,
                level: assets.LevelData);
            lintFlight.AfterFrame(enemies);
            growthCeilingPlms.AddRange(enemies.KraidPlmRequests);
            ProbeFirstUnauditedProjectileContact(
                bus,
                enemies,
                assets.LevelData,
                observedProjectileKinds,
                contactedProjectileKinds);
            enemies.StepEnemyProjectiles(assets.LevelData, samus, cameraX: CameraX, cameraY: CameraY);
        }
        lintFlight.VerifyCoverage();
        RoomEnemyProjectileKind[] requiredKraidProjectiles =
        [
            RoomEnemyProjectileKind.KraidSpitRock,
            RoomEnemyProjectileKind.KraidCeilingRock,
            RoomEnemyProjectileKind.KraidRisingRockLeft,
            RoomEnemyProjectileKind.KraidRisingRockRight,
        ];
        // The main fixture deliberately uses random word $1234, selecting the right-hand
        // rise definition. A second complete retail load with bit $10 clear proves the
        // otherwise symmetric left initializer/list/movement path without mutating a live
        // projectile kind after allocation.
        observedProjectileKinds.Add(VerifyOppositeRisingRockVariant(bus, room, assets));
        RoomEnemyProjectileKind[] missingObservedProjectiles = requiredKraidProjectiles
            .Where(kind => !observedProjectileKinds.Contains(kind))
            .ToArray();
        RoomEnemyProjectileKind[] requiredContactedProjectiles = requiredKraidProjectiles
            .Where(kind => (ReadWord(
                bus,
                0x860000 | unchecked((ushort)((ushort)kind + 8))) & 0x2000) == 0)
            .ToArray();
        RoomEnemyProjectileKind[] missingContactedProjectiles = requiredContactedProjectiles
            .Where(kind => !contactedProjectileKinds.Contains(kind))
            .ToArray();
        bool roomPaletteReachedTarget = Enumerable.Range(0, 16).All(color =>
            cgram.Colors[96 + color] == ReadWord(bus, 0xa786c7 + color * 2));
        if (!growthFunctions.Contains(KraidAiFunction.ProcessHeadInstructionAndTimer) ||
            !growthFunctions.Contains(KraidAiFunction.GrowReleaseCamera) ||
            !growthFunctions.Contains(KraidAiFunction.GrowBreakCeilingPlatforms) ||
            !growthFunctions.Contains(KraidAiFunction.GrowSetBg2Priority) ||
            !growthFunctions.Contains(KraidAiFunction.GrowFinishBg2Update) ||
            !growthFunctions.Contains(KraidAiFunction.GrowDrawRoomBackground) ||
            !growthFunctions.Contains(KraidAiFunction.GrowFadeInRoomBackground) ||
            !growthFunctions.Contains(KraidAiFunction.SecondPhaseThinking) ||
            !state.CameraReleasedForSecondPhase || !state.Bg2PriorityBitsSet ||
            state.CeilingRockSpawnCount == 0 || secondPhaseStartY != 295 ||
            !growthCeilingPlms.SequenceEqual(KraidPlmDefinitions.GrowthCeiling) ||
            !roomPaletteReachedTarget ||
            !secondPhaseFootFunctions.Contains(KraidAiFunction.FootSecondPhaseWalkToStart) ||
            !secondPhaseFootFunctions.Contains(KraidAiFunction.FootSecondPhaseThinking) ||
            !lintFunctions.Contains(KraidAiFunction.LintProduce) ||
            !lintFunctions.Contains(KraidAiFunction.LintCharge) ||
            !lintFunctions.Contains(KraidAiFunction.LintFire) ||
            missingObservedProjectiles.Length != 0 ||
            missingContactedProjectiles.Length != 0)
        {
            throw new InvalidDataException(
                $"Kraid growth/second phase mismatch: body=" +
                $"{string.Join(',', growthFunctions)}, foot=" +
                $"{string.Join(',', secondPhaseFootFunctions)}, lints=" +
                $"{string.Join(',', lintFunctions)}, phase2 Y={secondPhaseStartY}, " +
                $"camera/priority={state.CameraReleasedForSecondPhase}/" +
                $"{state.Bg2PriorityBitsSet}, ceiling={state.CeilingRockSpawnCount}, " +
                $"ceiling PLMs=[{string.Join(',', growthCeilingPlms)}], " +
                $"palette={roomPaletteReachedTarget}, missing projectile contact=" +
                $"[{string.Join(',', missingContactedProjectiles)}], missing observation=" +
                $"[{string.Join(',', missingObservedProjectiles)}].");
        }

        VerifyKraidCeilingPlms(bus, assets.LevelData, assets.Scrolls, growthCeilingPlms);

        VerifyKraidFootCollisionSuppression(bus, enemies, samus);

        // Finish through the same vulnerable inner-mouth path. Kraid's no-death-check
        // header means zero HP must retain the body and enter `$C360`; deletion here would
        // skip the dying head art, 296-pixel sink, drops, room restoration, and boss bit.
        if (body.VariableA == (ushort)KraidAiFunction.MainloopThinking &&
            state.InvulnerableMouthHitbox == ushort.MaxValue)
        {
            int reactionHit = StrikeKraidOuterBody(
                bus,
                enemies,
                body,
                projectileDamage: 100);
            if (reactionHit != 1 ||
                body.VariableA != (ushort)KraidAiFunction.InitializeEyeGlow)
            {
                throw new InvalidDataException(
                    $"Kraid second-phase eye trigger mismatch: hits={reactionHit}, " +
                    $"function=$A7:{body.VariableA:X4}.");
            }
        }
        for (int mouthFrame = 0;
             mouthFrame < 1600 &&
             (body.VariableA is not (
                  (ushort)KraidAiFunction.MouthOpenReaction or
                  (ushort)KraidAiFunction.MainAttackWithMouthOpen) ||
              state.InvulnerableMouthHitbox == ushort.MaxValue);
             mouthFrame++)
        {
            enemies.StepFrame(
                CameraX,
                CameraY,
                timeIsFrozen: false,
                samus,
                level: assets.LevelData);
            enemies.StepEnemyProjectiles(assets.LevelData, samus, cameraX: CameraX, cameraY: CameraY);
        }
        int lethalHit = StrikeKraidMouth(
            bus,
            enemies,
            body,
            state,
            projectileDamage: ushort.MaxValue);
        if (lethalHit != 1 || body.Health != 0 ||
            body.VariableA != (ushort)KraidAiFunction.DeathInitialize ||
            body.Properties.HasAny(EnemyProperties.Deleted))
        {
            throw new InvalidDataException(
                $"Kraid lethal mouth hit mismatch: hits={lethalHit}, health={body.Health}, " +
                $"function=$A7:{body.VariableA:X4}, properties=${body.Properties:X4}.");
        }

        var deathFunctions = new HashSet<KraidAiFunction>();
        bool sawRoomMusicRequest = false;
        int deathFrames;
        for (deathFrames = 0;
             deathFrames < 1200 && !state.DeathSequenceComplete;
             deathFrames++)
        {
            deathFunctions.Add((KraidAiFunction)body.VariableA);
            enemies.StepFrame(
                CameraX,
                CameraY,
                timeIsFrozen: false,
                samus,
                level: assets.LevelData);
            sawRoomMusicRequest |= enemies.MusicRequests.Any(
                request => request.Command.RawValue == 3);
            enemies.StepEnemyProjectiles(assets.LevelData, samus, cameraX: CameraX, cameraY: CameraY);
        }
        if (!state.DeathSequenceComplete || !state.BossDefeatPersisted || !bossDefeated ||
            body.YPosition < 608 || state.SinkTableEventCount < 20 ||
            state.DeathDropRequestCount != 16 || state.DeathBg3TransferCount != 4 ||
            !sawRoomMusicRequest ||
            !deathFunctions.Contains(KraidAiFunction.DeathFadeOut) ||
            !deathFunctions.Contains(KraidAiFunction.DeathSink) ||
            !deathFunctions.Contains(KraidAiFunction.DeathFadeInBackground))
        {
            throw new InvalidDataException(
                $"Kraid death mismatch after {deathFrames} frames: complete/persisted/bit=" +
                $"{state.DeathSequenceComplete}/{state.BossDefeatPersisted}/{bossDefeated}, " +
                $"Y={body.YPosition}, sink events={state.SinkTableEventCount}, " +
                $"drops/BG3/music={state.DeathDropRequestCount}/" +
                $"{state.DeathBg3TransferCount}/{sawRoomMusicRequest}, functions=" +
                $"{string.Join(',', deathFunctions)}.");
        }

        VerifyRuntimeDefeatHandoff(bus, deathCaptureDirectory, observeFloor);
        if (observeFloor)
            Console.WriteLine("Floor diagnostic only: upper-body #268 pixel assertion excluded; capture completion is not proof of #269 visual parity.");
        VerifyDefeatedRoom(bus, room);
        Console.WriteLine(
            $"Kraid audit passed through repeating first-phase combat after {frame} rise frames: " +
            "retail 2x2 room, " +
            "eight-part population, phase thresholds, Samus lockout, BG2 upload cadence, " +
            "rise rocks/music, private head bytecode, roar/spit cadence, independently timed " +
            "foot lunge/retreat movement, arm-launch/lint-fire contact, fingernail motion, " +
            "canonical multibox gating, arm beam/bomb dust, foot property suppression, " +
            "cartridge mouth damage, all four rock variants plus every damage-enabled " +
            "projectile contact lifecycle, and " +
            "charged-body eye glow/unglow, ceiling growth, palette fade, second-phase " +
            "walking/lint attacks, cartridge-accurate selected/unselected HUD fading on " +
            $"both exits, and {deathFrames}-frame sink/death/persistence.");
        return 0;
    }

    /// <summary>
    /// Reproduces the missing-body report at the actual first-phase checkpoint. Kraid's
    /// arm is ordinary OAM, but his body is the two-page enemy BG2 surface constructed by
    /// <c>$A7:AAC6</c> and uploaded by <c>$A7:C874/$C8B6</c>. Counters alone cannot make
    /// that body visible, so compare stable words outside the animated 352-word head span.
    /// </summary>
    private static void VerifyKraidBg2VideoState(
        ISnesAddressSpace bus,
        SnesVram vram,
        RoomEnemySlot body,
        KraidEnemyState state)
    {
        byte[] upper = RomDataReader.Decompress(
            bus,
            KraidBackgroundRomData.UpperTilemap,
            KraidBackgroundRomData.DecompressedTilemapBytes);
        byte[] lower = RomDataReader.Decompress(
            bus,
            KraidBackgroundRomData.LowerTilemap,
            KraidBackgroundRomData.DecompressedTilemapBytes);
        if (upper.Length != KraidBackgroundRomData.DecompressedTilemapBytes ||
            lower.Length != KraidBackgroundRomData.DecompressedTilemapBytes)
        {
            throw new InvalidDataException(
                $"Kraid BG2 decompression size changed: upper=${upper.Length:X}, " +
                $"lower=${lower.Length:X}.");
        }

        const int stableTopWord = 500;
        const int stableBottomWord = 100;
        ushort expectedTop = unchecked((ushort)(
            ReadByteWord(upper, stableTopWord) & ~KraidBackgroundRomData.PriorityBit));
        ushort expectedBottom = unchecked((ushort)(
            ReadByteWord(lower, stableBottomWord) & ~KraidBackgroundRomData.PriorityBit));
        ushort actualTop = vram.ReadWord(
            KraidBackgroundRomData.LiveBg2TilemapWord + stableTopWord);
        ushort actualBottom = vram.ReadWord(
            KraidBackgroundRomData.LiveLowerBg2TilemapWord + stableBottomWord);
        ushort expectedHorizontalScroll = unchecked((ushort)(
            CameraX - body.XPosition + body.XRadius));
        ushort expectedVerticalScroll = unchecked((ushort)(CameraY - body.YPosition + 152));
        if (actualTop != expectedTop || actualBottom != expectedBottom ||
            !state.OwnsBg2Tilemap ||
            state.Bg2HorizontalScroll != expectedHorizontalScroll ||
            state.Bg2VerticalScroll != expectedVerticalScroll)
        {
            throw new InvalidDataException(
                $"Kraid body BG2 is not renderer-visible: top=${actualTop:X4}/${expectedTop:X4}, " +
                $"bottom=${actualBottom:X4}/${expectedBottom:X4}, " +
                $"owns={state.OwnsBg2Tilemap}, scroll=" +
                $"(${state.Bg2HorizontalScroll:X4},{state.Bg2VerticalScroll:X4})/" +
                $"({expectedHorizontalScroll:X4},{expectedVerticalScroll:X4}).");
        }
    }

    private static ushort ReadByteWord(ReadOnlySpan<byte> bytes, int wordIndex)
    {
        int byteIndex = checked(wordIndex * 2);
        return unchecked((ushort)(bytes[byteIndex] | (bytes[byteIndex + 1] << 8)));
    }

    /// <summary>
    /// Reproduces the reported defeat softlock through the complete gameplay runtime, not
    /// merely the isolated enemy dispatcher. It enters the retail room through its real
    /// south door, reaches both combat phases, applies a lethal hit through the production
    /// multibox callback, and then proves NMI, PLMs, boss state, music, and the grey-door
    /// handoff continue advancing beyond the formerly failing sinking-table frame.
    /// </summary>
    private static void VerifyRuntimeDefeatHandoff(SuperMetroidAddressSpace bus, string? deathCaptureDirectory, bool observeFloor)
    {
        CartridgeDoorHeader door = CartridgeDoorHeader.Load(bus, IncomingDoorPointer);
        if (door.DestinationRoomPointer != RoomPointer)
        {
            throw new InvalidDataException(
                $"Kraid audit door $83:{IncomingDoorPointer:X4} now targets " +
                $"$8F:{door.DestinationRoomPointer:X4}, not $8F:{RoomPointer:X4}.");
        }

        var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomThroughDoorForVerification(door, CameraX, CameraY);
        VerifyKraidHudCharacterBase(runtime);
        RoomLevelData liveLevel = runtime.LevelData ??
            throw new InvalidDataException("Kraid runtime load did not install level data.");
        KraidPlmRequest defeatedRoomSpikeClear = KraidPlmDefinitions.DefeatedRoom.Single(
            request => request.Header == RoomPlmHeaders.ClearKraidSpikes);
        ushort authoredLiveSpikes = liveLevel.GetCollisionBlock(
            defeatedRoomSpikeClear.BlockX,
            defeatedRoomSpikeClear.BlockY).LevelWord;

        SamusState samus = runtime.Samus ??
            throw new InvalidDataException("Kraid runtime load did not retain Samus.");
        samus.Health = 999;
        samus.MaxHealth = 999;
        samus.XPosition = 128;
        samus.YPosition = 456;
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);

        KraidEnemyState state = runtime.Enemies.Kraid ??
            throw new InvalidDataException("Kraid runtime room did not initialize its enemy state.");
        RoomEnemySlot body = runtime.Enemies.Slots[0];
        AdvanceRuntimeUntil(
            runtime,
            () => body.VariableA == (ushort)KraidAiFunction.MainloopThinking,
            maximumFrames: 1200,
            "first-phase main loop");

        for (int hit = 0; hit < 2; hit++)
        {
            AdvanceRuntimeUntilOpenMouth(runtime, body, state);
            if (StrikeKraidMouth(bus, runtime.Enemies, body, state, projectileDamage: 100) != 1)
                throw new InvalidDataException("Runtime Kraid did not accept a first-phase mouth hit.");
            runtime.StepFrame(controller1Input: 0);
        }
        AdvanceRuntimeUntil(
            runtime,
            () => body.VariableA == (ushort)KraidAiFunction.SecondPhaseThinking,
            maximumFrames: 1600,
            "second-phase main loop");
        if (!runtime.Camera!.Scrolls.Storage[..4].SequenceEqual(new byte[] { 2, 2, 1, 1 }))
            throw new InvalidDataException("Kraid growth did not publish the cartridge scroll release.");
        // #268's pixel rectangle is above the head in the upper viewport, not fixed
        // to whatever camera the preceding fight happens to leave behind. After the
        // real growth release, settle an explicit observer there through normal scrolling.
        samus.XPosition = 48;
        samus.YPosition = 100;
        samus.InputLocked = true;
        for (int settle = 0; settle < 256; settle++)
        {
            samus.YPosition = (ushort)(300 - settle);
            runtime.StepFrame(0);
        }
        // The floor diagnostic must reach the reported death frames even when the
        // separate upper-body composition assertion fails. Keep that assertion in
        // the ordinary audit; this scoped capture makes no claim about issue #268.
        bool backgroundVerified = observeFloor || VerifyKraidGrowthArtifactRegion(runtime, deathCaptureDirectory);

        if (deathCaptureDirectory is not null)
        {
            // The old handoff-only fixture can finish with its observer above the boss.
            // Place an input-locked observer beside the upper body. This is a diagnostic
            // viewpoint, not a controller-route claim; the enemy/death/render loop is live.
            samus.InputLocked = true;
            // Moving the observer in one large jump can itself make the camera cross
            // several ring-buffer rows in one frame. Waiting afterward does not repair
            // skipped rows. Advance the observer one pixel per frame instead.
            int observerX = samus.XPosition, observerY = samus.YPosition;
            int targetX = observeFloor ? 48 : 256, targetY = observeFloor ? 480 : 256;
            while (observerX != targetX || observerY != targetY)
            {
                observerX += Math.Sign(targetX - observerX);
                observerY += Math.Sign(targetY - observerY);
                samus.XPosition = (ushort)observerX;
                samus.YPosition = (ushort)observerY;
                runtime.StepFrame(0);
            }
            for (int settle = 0; settle < 120; settle++)
                runtime.StepFrame(0);
        }
        AdvanceRuntimeUntilOpenMouth(runtime, body, state,
            triggerIdleReaction: deathCaptureDirectory is not null);
        using var deathCapture = deathCaptureDirectory is null ? null :
            new KraidDeathCapture(deathCaptureDirectory);
        deathCapture?.Capture(runtime, -1);
        body.Health = 100;
        if (StrikeKraidMouth(bus, runtime.Enemies, body, state, projectileDamage: 100) != 1 ||
            body.Health != 0 ||
            body.VariableA != (ushort)KraidAiFunction.DeathInitialize)
        {
            throw new InvalidDataException(
                $"Runtime Kraid lethal transition failed: HP={body.Health}, " +
                $"function=$A7:{body.VariableA:X4}.");
        }

        bool sawRoomMusic = false;
        int deathFrames = 0;
        while (!state.DeathSequenceComplete && deathFrames < 1200)
        {
            runtime.StepFrame(controller1Input: 0);
            deathCapture?.Capture(runtime, deathFrames);
            sawRoomMusic |= runtime.Enemies.MusicRequests.Any(
                request => request.Command.RawValue == 3);
            deathFrames++;
        }
        if (!state.DeathSequenceComplete ||
            !runtime.System.HasAnyBossBits(AreaId.Brinstar, BossBits.AreaBoss) ||
            !sawRoomMusic ||
            state.SinkTableEventCount < 20 ||
            state.DeathDropRequestCount != 16)
        {
            throw new InvalidDataException(
                $"Runtime Kraid defeat stalled after {deathFrames} frames: " +
                $"complete={state.DeathSequenceComplete}, " +
                $"boss={runtime.System.HasAnyBossBits(AreaId.Brinstar, BossBits.AreaBoss)}, " +
                $"music={sawRoomMusic}, sink={state.SinkTableEventCount}, " +
                $"drops={state.DeathDropRequestCount}.");
        }

        VerifyStandardBg3Restored(bus, runtime);
        ReportLiveDefeatSpikeState(
            runtime,
            liveLevel,
            defeatedRoomSpikeClear,
            authoredLiveSpikes);

        ushort completedFrame = runtime.NmiFrameCounter;
        for (int postDefeatFrame = 0; postDefeatFrame < 60; postDefeatFrame++)
            runtime.StepFrame(controller1Input: 0);
        if (runtime.NmiFrameCounter != unchecked((ushort)(completedFrame + 60)) ||
            !runtime.Plms.GreyDoors.Any(doorState =>
                doorState.Condition == GreyDoorCondition.AreaBossDefeated &&
                doorState.Phase == GreyDoorPhase.Flashing))
        {
            throw new InvalidDataException(
                $"Kraid post-defeat runtime did not advance/unlock: frame " +
                $"${completedFrame:X4}->${runtime.NmiFrameCounter:X4}, grey doors=" +
                $"[{string.Join(',', runtime.Plms.GreyDoors.Select(value => value.Phase))}].");
        }

        VerifyBothDefeatedKraidExits(bus, runtime);
        VerifyDefeatedRoomReload(runtime);
        if (!backgroundVerified)
            throw new InvalidDataException("Kraid hand capture completed, but the separate #268 background assertion failed; see preserved growth comparison images. This run is not an audit pass.");
    }

    /// <summary>
    /// Records the current level/PLM outcome without treating an unchanged collision word
    /// as proof of rendered cartridge behavior. The player has contradicted the prior
    /// reload-only visual conclusion; that conclusion must not be enforced as a regression.
    /// </summary>
    private static void ReportLiveDefeatSpikeState(
        SuperMetroidRuntime runtime,
        RoomLevelData level,
        KraidPlmRequest spikeClear,
        ushort authoredLiveSpikes)
    {
        ushort liveSpikesAfterDeath = level.GetCollisionBlock(
            spikeClear.BlockX,
            spikeClear.BlockY).LevelWord;
        Console.WriteLine(
                $"Kraid live floor diagnostic (not a visual parity assertion): " +
                $"block (${spikeClear.BlockX:X2},${spikeClear.BlockY:X2}) " +
                $"${authoredLiveSpikes:X4}->${liveSpikesAfterDeath:X4}, " +
                $"active={runtime.Plms.HasActiveHeader(RoomPlmHeaders.ClearKraidSpikes)}, " +
                $"requested={runtime.Enemies.KraidPlmRequests.Contains(spikeClear)}.");
    }

    /// <summary>
    /// Reproduces issue #268 at the exact post-growth frame where Kraid's priority BG2 tail
    /// used to expose eight zero-filled rows over the now-visible room background.
    /// </summary>
    private static bool VerifyKraidGrowthArtifactRegion(SuperMetroidRuntime runtime, string? diagnosticDirectory = null)
    {
        KraidEnemyState state = runtime.Enemies.Kraid ??
            throw new InvalidDataException("Kraid artifact audit lost encounter state.");
        ReadOnlySpan<ushort> tail = state.BackgroundTilemapWords.AsSpan(
            KraidBackgroundRomData.PreservedLowerTailFirstWord,
            KraidBackgroundRomData.PreservedLowerTailWordCount);
        if (tail.Contains((ushort)0))
        {
            throw new InvalidDataException(
                "Reproduced issue #268: Kraid's preserved lower-stream BG2 tail still " +
                "contains zero tile words after growth.");
        }

        GameplayPpuRenderSnapshot ppu = runtime.DisplayedGameplayPpu;
        if (runtime.Camera!.XPosition != 0 || runtime.Camera.YPosition != 0)
            throw new InvalidDataException("The #268 visual fixture requires its upper-left reference viewport.");
        ushort bg1X = unchecked((ushort)(
            ppu.Bg1HorizontalScroll + ppu.RoomShake.Bg1X));
        // The raw viewport helper samples its supplied source row literally. Gameplay
        // starts BG sampling on physical scanline one, not output row zero; account
        // for that here or ordinary tile edges falsely look like BG2 corruption.
        ushort bg1Y = unchecked((ushort)(
            ppu.Bg1VerticalScroll + ppu.RoomShake.Bg1Y + SnesPpuLayout.FirstVisibleBackgroundScanline));
        Rgba32[] expectedBg1 = SnesBgTilemapRenderer.Render4BppViewport(
            runtime.Vram,
            runtime.Cgram,
            SnesPpuLayout.GameplayBg1TilemapWord,
            characterBaseWord: 0,
            bg1X,
            bg1Y,
            FrontendFrame.Width,
            FrontendFrame.Height);
        Rgba32[] actual = SuperMetroidRuntimeFrameRenderer.Render(runtime);

        int comparedOpaquePixels = 0;
        int differingOpaquePixels = 0;
        for (int y = KraidAuditDefinitions.ArtifactRegionTop;
             y < KraidAuditDefinitions.ArtifactRegionBottom;
             y++)
        {
            for (int x = KraidAuditDefinitions.ArtifactRegionLeft;
                 x < KraidAuditDefinitions.ArtifactRegionRight;
                 x++)
            {
                int pixel = y * FrontendFrame.Width + x;
                if (expectedBg1[pixel].A == 0)
                    continue;
                comparedOpaquePixels++;
                if (actual[pixel] != expectedBg1[pixel])
                    differingOpaquePixels++;
            }
        }

        if (comparedOpaquePixels < KraidAuditDefinitions.MinimumArtifactRegionBg1Pixels ||
            differingOpaquePixels != 0)
        {
            string failure =
                $"Reproduced issue #268: Kraid's post-growth region retained " +
                $"{differingOpaquePixels}/{comparedOpaquePixels} pixels over the " +
                "cartridge-authored opaque BG1 background.";
            if (diagnosticDirectory is null)
                throw new InvalidDataException(failure);
            // A hand-only diagnostic must not lose its later evidence to a different
            // pixel assertion. Preserve the failing comparison and announce it loudly;
            // ordinary --kraid-audit still throws, and this is not a #268 pass.
            Directory.CreateDirectory(diagnosticDirectory);
            PngWriter.WriteRgba(Path.Combine(diagnosticDirectory, "growth-actual.png"),
                FrontendFrame.Width, FrontendFrame.Height, actual);
            PngWriter.WriteRgba(Path.Combine(diagnosticDirectory, "growth-bg1-reference.png"),
                FrontendFrame.Width, FrontendFrame.Height, expectedBg1);
            Console.Error.WriteLine($"SEPARATE BACKGROUND ASSERTION FAILED: {failure} Continuing the scoped hand capture only.");
            return false;
        }
        return true;
    }

    /// <summary>
    /// Drives both cartridge exits after the encounter rather than assuming that fixing
    /// the shared source bytes is enough. The first transition leaves the completed fight;
    /// the second reloads the defeated room and leaves through its opposite door. Both
    /// destinations must retain the restored standard BG3 sheet after their complete
    /// production door coroutine.
    /// </summary>
    private static void VerifyBothDefeatedKraidExits(
        SuperMetroidAddressSpace bus,
        SuperMetroidRuntime runtime)
    {
        VerifyDefeatedKraidExit(
            bus,
            runtime,
            KraidAuditDefinitions.LeftExitDoor,
            KraidAuditDefinitions.LeftExitDestination);

        runtime.LoadCartridgeRoomForDebug(RoomPointer, CameraX, CameraY);
        VerifyDefeatedKraidExit(
            bus,
            runtime,
            KraidAuditDefinitions.RightExitDoor,
            KraidAuditDefinitions.RightExitDestination);
    }

    private static void VerifyDefeatedKraidExit(
        SuperMetroidAddressSpace bus,
        SuperMetroidRuntime runtime,
        ushort doorPointer,
        ushort destinationRoomPointer)
    {
        KraidEnemyState state = runtime.Enemies.Kraid ??
            throw new InvalidDataException("Defeated Kraid exit lost encounter state.");
        for (int frame = 0; !state.DeathSequenceComplete && frame < 120; frame++)
            runtime.StepFrame(controller1Input: 0);
        if (!state.DeathSequenceComplete)
        {
            throw new InvalidDataException(
                "Defeated Kraid room initialization did not complete its background restoration.");
        }

        PrepareKraidExitHud(runtime);
        PublishRetailDoor(runtime, bus, doorPointer);
        var transition = new DoorTransitionState();
        transition.Begin(runtime);
        var audio = new CartridgeAudioState();
        Rgba32[] hudBeforeFade = RenderHud(runtime);
        ushort[] paletteBeforeFade = runtime.Cgram.Colors.ToArray();
        bool verifiedSourceFade = false;
        for (int frame = 0; transition.IsActive && frame < 320; frame++)
        {
            transition.Step(runtime, audio, controllerInput: 0);
            if (!verifiedSourceFade && transition.Phase == DoorTransitionPhase.LoadDoorHeader)
            {
                VerifyKraidExitHudFade(
                    runtime,
                    doorPointer,
                    paletteBeforeFade,
                    hudBeforeFade);
                verifiedSourceFade = true;
            }
        }

        if (!verifiedSourceFade ||
            transition.IsActive ||
            runtime.ActiveRoom?.Pointer != destinationRoomPointer)
        {
            throw new InvalidDataException(
                $"Defeated Kraid exit $83:{doorPointer:X4} did not complete into " +
                $"$8F:{destinationRoomPointer:X4}; sourceFade={verifiedSourceFade}.");
        }
        VerifyStandardBg3Restored(bus, runtime);
    }

    /// <summary>
    /// Installs two simultaneously visible item families before each real Kraid-room exit.
    /// Missile is selected and therefore uses HUD palette four; super missile is unselected
    /// and uses palette five. This makes issue #271's reported asymmetry observable without
    /// relying on whichever inventory happened to survive the preceding boss audit.
    /// </summary>
    private static void PrepareKraidExitHud(SuperMetroidRuntime runtime)
    {
        SamusState samus = runtime.Samus ??
            throw new InvalidDataException("Kraid HUD fade audit lost Samus.");
        samus.Missiles = 5;
        samus.MaxMissiles = 5;
        samus.SuperMissiles = 5;
        samus.MaxSuperMissiles = 5;
        samus.SelectedHudItem = 1;
        runtime.InitializeHud(new HudSnapshot(
            Health: samus.Health,
            MaxHealth: samus.MaxHealth,
            Missiles: samus.Missiles,
            MaxMissiles: samus.MaxMissiles,
            SuperMissiles: samus.SuperMissiles,
            MaxSuperMissiles: samus.MaxSuperMissiles,
            PowerBombs: samus.PowerBombs,
            MaxPowerBombs: samus.MaxPowerBombs,
            EquippedItems: samus.EquippedItems,
            SelectedItem: samus.SelectedHudItem,
            ReserveHealth: samus.ReserveEnergy,
            ReserveMode: samus.ReserveTankMode));
        runtime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);
    }

    /// <summary>
    /// Validates issue #271 against the exact source-palette endpoint of both retail exits.
    /// State $10 preserves CGRAM colors 17-19 unconditionally, but preserves colors 21-23
    /// only when neither room has CRE bit zero. Kraid has that bit, so its selected missile
    /// remains visible while the unselected super missile intentionally fades to black.
    /// </summary>
    private static void VerifyKraidExitHudFade(
        SuperMetroidRuntime runtime,
        ushort doorPointer,
        ReadOnlySpan<ushort> paletteBeforeFade,
        ReadOnlySpan<Rgba32> hudBeforeFade)
    {
        CartridgeRoomHeader sourceRoom = runtime.ActiveRoom ??
            throw new InvalidDataException("Kraid HUD fade audit lost its source room.");
        if ((sourceRoom.CreBitset & RoomCreBitsets.SuppressDoorTransitionBg1) == 0)
        {
            throw new InvalidDataException(
                $"Kraid exit $83:{doorPointer:X4} no longer selects the CRE transition path.");
        }

        for (int color = 0; color < KraidAuditDefinitions.HudItemVisibleColorCount; color++)
        {
            int selectedColor = KraidAuditDefinitions.SelectedHudPaletteFirstColor + color;
            int unselectedColor = KraidAuditDefinitions.UnselectedHudPaletteFirstColor + color;
            if (runtime.Cgram.Colors[selectedColor] != paletteBeforeFade[selectedColor] ||
                runtime.Cgram.Colors[unselectedColor] != 0)
            {
                throw new InvalidDataException(
                    $"Kraid exit $83:{doorPointer:X4} violated the cartridge HUD fade rule " +
                    $"at palette colors {selectedColor}/{unselectedColor}: " +
                    $"selected=${runtime.Cgram.Colors[selectedColor]:X4}/" +
                    $"${paletteBeforeFade[selectedColor]:X4}, " +
                    $"unselected=${runtime.Cgram.Colors[unselectedColor]:X4}.");
            }
        }

        Rgba32[] hudAfterFade = RenderHud(runtime);
        AssertRegionUnchanged(
            hudBeforeFade,
            hudAfterFade,
            KraidAuditDefinitions.MissileIconLeft,
            KraidAuditDefinitions.HudItemIconTop,
            KraidAuditDefinitions.MissileIconWidth,
            KraidAuditDefinitions.HudItemIconHeight,
            $"selected missile during Kraid exit $83:{doorPointer:X4}");
        AssertRegionChanged(
            hudBeforeFade,
            hudAfterFade,
            KraidAuditDefinitions.SuperMissileIconLeft,
            KraidAuditDefinitions.HudItemIconTop,
            KraidAuditDefinitions.StandardItemIconWidth,
            KraidAuditDefinitions.HudItemIconHeight,
            $"unselected super missile during Kraid exit $83:{doorPointer:X4}");
    }

    private static Rgba32[] RenderHud(SuperMetroidRuntime runtime)
    {
        var pixels = new Rgba32[
            SnesGameplayFrameRenderer.Width * SnesGameplayFrameRenderer.HudHeight];
        SnesBgTilemapRenderer.Render2Bpp(
            pixels,
            runtime.Vram,
            runtime.Cgram,
            SnesPpuLayout.GameplayHudTilemapWord,
            runtime.GameplayHudCharacterBaseWord,
            rowCount: 4);
        return pixels;
    }

    private static void AssertRegionUnchanged(
        ReadOnlySpan<Rgba32> before,
        ReadOnlySpan<Rgba32> after,
        int left,
        int top,
        int width,
        int height,
        string context)
    {
        int compared = 0;
        for (int y = top; y < top + height; y++)
        {
            int row = y * SnesGameplayFrameRenderer.Width;
            for (int x = left; x < left + width; x++)
            {
                compared++;
                if (before[row + x] != after[row + x])
                {
                    throw new InvalidDataException(
                        $"Issue #271 cartridge check changed {context} at ({x},{y}).");
                }
            }
        }
        if (compared == 0)
            throw new InvalidDataException($"Issue #271 did not sample {context}.");
    }

    private static void AssertRegionChanged(
        ReadOnlySpan<Rgba32> before,
        ReadOnlySpan<Rgba32> after,
        int left,
        int top,
        int width,
        int height,
        string context)
    {
        int changed = 0;
        for (int y = top; y < top + height; y++)
        {
            int row = y * SnesGameplayFrameRenderer.Width;
            for (int x = left; x < left + width; x++)
            {
                if (before[row + x] != after[row + x])
                    changed++;
            }
        }
        if (changed == 0)
        {
            throw new InvalidDataException(
                $"Issue #271 cartridge check did not fade {context}.");
        }
    }

    private static void PublishRetailDoor(
        SuperMetroidRuntime runtime,
        ISnesAddressSpace bus,
        ushort expectedDoorPointer)
    {
        RoomLevelData level = runtime.LevelData ??
            throw new InvalidDataException("Kraid exit audit has no active room level.");
        byte pose = runtime.Samus?.Pose ?? 0;
        for (int blockY = 0; blockY < level.HeightInBlocks; blockY++)
        {
            for (int blockX = 0; blockX < level.WidthInBlocks; blockX++)
            {
                RoomCollisionBlock block = level.GetCollisionBlock(blockX, blockY);
                if (block.CollisionType != RoomCollisionType.DoorBlock)
                    continue;
                CartridgeDoorHeader candidate = level.ResolveDoorCollision(
                    bus,
                    block.Behavior,
                    pose,
                    publishDoorSideEffects: false);
                if (candidate.Pointer != expectedDoorPointer)
                    continue;
                _ = level.ResolveDoorCollision(
                    bus,
                    block.Behavior,
                    pose,
                    publishDoorSideEffects: true);
                return;
            }
        }

        throw new InvalidDataException(
            $"Kraid room has no collision block for exit $83:{expectedDoorPointer:X4}.");
    }

    /// <summary>
    /// Asserts the visual resource that both Kraid-room exits inherit. Kraid's body map
    /// occupies the ordinary BG3 character range at VRAM word $4000; the four native death
    /// transfers must restore every byte before either destination selects that character
    /// base again. A function-counter assertion cannot detect the resulting corrupt tiles.
    /// </summary>
    private static void VerifyStandardBg3Restored(
        SuperMetroidAddressSpace bus,
        SuperMetroidRuntime runtime)
    {
        int byteCount =
            KraidBackgroundRomData.StandardBg3TransferBytes *
            KraidBackgroundRomData.StandardBg3TransferCount;
        int vramByteOffset = KraidBackgroundRomData.StandardBg3VramWord * 2;
        for (int byteIndex = 0; byteIndex < byteCount; byteIndex++)
        {
            byte expected = bus.ReadByte(
                KraidBackgroundRomData.StandardBg3TilesAddress + byteIndex);
            byte actual = runtime.Vram.ReadByte(vramByteOffset + byteIndex);
            if (actual != expected)
            {
                throw new InvalidDataException(
                    $"Reproduced issue #267: Kraid death left corrupt BG3 byte " +
                    $"${byteIndex:X4} (${actual:X2}, expected {expected:X2}) before either exit; " +
                    $"room=$8F:{runtime.ActiveRoom?.Pointer ?? 0:X4}, background=" +
                    $"$8F:{runtime.ActiveRoom?.State.BackgroundDataPointer ?? 0:X4}.");
            }
        }
    }

    /// <summary>
    /// Reproduces the reported defeat, exit, and re-entry failure through the actual room
    /// loader and runtime PLM handoff. Reloading is the important part of this assertion:
    /// the live death sequence can leave the arena correct while a later defeated-room
    /// initialization silently restores the authored ceiling and spikes.
    /// </summary>
    private static void VerifyDefeatedRoomReload(SuperMetroidRuntime runtime)
    {
        runtime.LoadCartridgeRoomForDebug(RoomPointer, CameraX, CameraY);
        RoomLevelData level = runtime.LevelData ??
            throw new InvalidDataException("Defeated Kraid reload did not install room level data.");
        KraidEnemyState state = runtime.Enemies.Kraid ??
            throw new InvalidDataException("Defeated Kraid reload did not initialize encounter state.");

        if (!runtime.Enemies.KraidPlmRequests.SequenceEqual(KraidPlmDefinitions.DefeatedRoom))
        {
            throw new InvalidDataException(
                "Defeated Kraid reload did not publish ceiling/spike clears in native order.");
        }

        ushort authoredCeiling = level.GetCollisionBlock(0x02, 0x12).LevelWord;
        ushort authoredSpikes = level.GetCollisionBlock(0x05, 0x1b).LevelWord;
        if (!runtime.Plms.HasActiveHeader(RoomPlmHeaders.ClearKraidCeiling) ||
            !runtime.Plms.HasActiveHeader(RoomPlmHeaders.ClearKraidSpikes))
        {
            throw new InvalidDataException(
                "Defeated Kraid room-load handoff did not retain both native clear PLMs.");
        }

        // The spike clear is an authored multi-frame sweep across the floor; it is much
        // longer than the one-frame setup mutation at its origin. Let the real instruction
        // streams reach their delete opcodes instead of asserting an arbitrary animation
        // prefix.
        for (int frame = 0;
             frame < 180 &&
                 (runtime.Plms.HasActiveHeader(RoomPlmHeaders.ClearKraidCeiling) ||
                  runtime.Plms.HasActiveHeader(RoomPlmHeaders.ClearKraidSpikes));
             frame++)
            runtime.StepFrame(controller1Input: 0);

        ushort clearedCeiling = level.GetCollisionBlock(0x02, 0x12).LevelWord;
        ushort clearedSpikes = level.GetCollisionBlock(0x05, 0x1b).LevelWord;
        if (runtime.Plms.HasActiveHeader(RoomPlmHeaders.ClearKraidCeiling) ||
            runtime.Plms.HasActiveHeader(RoomPlmHeaders.ClearKraidSpikes) ||
            clearedCeiling == authoredCeiling ||
            clearedSpikes == authoredSpikes)
        {
            throw new InvalidDataException(
                $"Defeated Kraid reload did not restore the arena: PLMs={runtime.Plms.ActiveCount}, " +
                $"ceiling=${authoredCeiling:X4}->${clearedCeiling:X4}, " +
                $"spikes=${authoredSpikes:X4}->${clearedSpikes:X4}.");
        }

        if (state.OwnsBg2Tilemap)
            throw new InvalidDataException("Defeated Kraid incorrectly retained private BG2 ownership.");
    }

    private static void AdvanceRuntimeUntilOpenMouth(
        SuperMetroidRuntime runtime,
        RoomEnemySlot body,
        KraidEnemyState state,
        bool triggerIdleReaction = false) =>
        AdvanceRuntimeUntil(
            runtime,
            () =>
            {
                // The death observer can consume the initial roar while settling.
                // Native idle then requires a projectile hit. Exercise that contact
                // path, rather than inventing another timer-driven mouth opening.
                if (triggerIdleReaction && body.VariableA == (ushort)KraidAiFunction.MainloopThinking &&
                    state.ThinkingTimer == 0)
                {
                    if (StrikeKraidOuterBody(runtime.AddressSpace, runtime.Enemies, body, 100) != 1 ||
                        body.VariableA != (ushort)KraidAiFunction.InitializeEyeGlow)
                        throw new InvalidDataException("Kraid death observer could not trigger the native eye reaction.");
                }
                return body.VariableA is (
                    (ushort)KraidAiFunction.MainAttackWithMouthOpen or
                    (ushort)KraidAiFunction.MouthOpenReaction) &&
                    state.InvulnerableMouthHitbox != ushort.MaxValue;
            },
            maximumFrames: 1400,
            "open-mouth damage window");

    /// <summary>
    /// Reproduces the Kraid-entry HUD corruption as rendered pixels. Library-background
    /// command eight selects BG34NBA=$02 after copying standard HUD characters to VRAM
    /// $2000; using the normal $4000 character base decodes Kraid's body map as glyphs.
    /// </summary>
    private static void VerifyKraidHudCharacterBase(SuperMetroidRuntime runtime)
    {
        Rgba32[] actual = SuperMetroidRuntimeFrameRenderer.Render(runtime);
        var expectedHud = new Rgba32[
            SnesGameplayFrameRenderer.Width * SnesGameplayFrameRenderer.HudHeight];
        SnesBgTilemapRenderer.Render2Bpp(
            expectedHud,
            runtime.Vram,
            runtime.Cgram,
            SnesPpuLayout.GameplayHudTilemapWord,
            characterBaseWord: RoomAssetRomData.LibraryBackground.KraidHudCharacterBaseWord,
            rowCount: 4);

        ReadOnlySpan<Rgba32> actualHud = actual.AsSpan(0, expectedHud.Length);
        if (!actualHud.SequenceEqual(expectedHud))
        {
            int differingPixels = 0;
            for (int pixel = 0; pixel < expectedHud.Length; pixel++)
            {
                if (actualHud[pixel] != expectedHud[pixel])
                    differingPixels++;
            }
            throw new InvalidDataException(
                $"Kraid entry rendered {differingPixels} corrupt HUD pixels by ignoring " +
                "library-background command eight's BG34NBA=$02 side effect.");
        }
    }

    private static void AdvanceRuntimeUntil(
        SuperMetroidRuntime runtime,
        Func<bool> condition,
        int maximumFrames,
        string checkpoint)
    {
        for (int frame = 0; frame < maximumFrames; frame++)
        {
            if (condition())
                return;
            runtime.StepFrame(controller1Input: 0);
        }
        throw new InvalidDataException(
            $"Kraid runtime did not reach {checkpoint} within {maximumFrames} frames: " +
            $"phase={runtime.Enemies.Slots[0].VariableA:X4}, " +
            $"body=({runtime.Enemies.Slots[0].XPosition},{runtime.Enemies.Slots[0].YPosition}), " +
            $"Samus=({runtime.Samus!.XPosition},{runtime.Samus.YPosition}), " +
            $"camera=({runtime.Camera!.XPosition},{runtime.Camera.YPosition}), " +
            $"thinking={runtime.Enemies.Kraid!.ThinkingTimer}, " +
            $"headTimer={runtime.Enemies.Slots[0].VariableC}.");
    }

    /// <summary>
    /// Drives the nine published growth requests through the real shared bank-$84 PLM
    /// interpreter. This asserts the reported property itself: each cartridge ceiling
    /// origin loses its collision type immediately and receives its authored crumble draw,
    /// rather than merely proving that Kraid incremented a counter or spawned debris.
    /// </summary>
    private static void VerifyKraidCeilingPlms(
        ISnesAddressSpace bus,
        RoomLevelData level,
        RoomScrollGrid scrolls,
        List<KraidPlmRequest> requests)
    {
        if (!requests.SequenceEqual(KraidPlmDefinitions.GrowthCeiling))
            throw new InvalidDataException("Kraid did not publish all nine ceiling PLMs in ROM order.");

        ushort[] authoredOrigins = requests
            .Select(request => level.GetCollisionBlock(
                request.BlockX,
                request.BlockY).LevelWord)
            .ToArray();
        var streamer = new BackgroundTilemapStreamer(
            level.WidthInBlocks,
            level.ForegroundEntries.Span,
            level.BackgroundEntries.Span,
            level.BlockDefinitions.Span);
        var plms = new RoomPlmSystem();
        foreach (KraidPlmRequest request in requests)
        {
            if (!plms.TrySpawnKraidRoomMutation(
                    level,
                    request.BlockX,
                    request.BlockY,
                    request.Header))
            {
                throw new InvalidDataException("Kraid ceiling PLM exhausted an empty 40-slot pool.");
            }
        }

        if (plms.ActiveCount != requests.Count)
            throw new InvalidDataException("Kraid ceiling PLMs did not retain one native slot per request.");
        for (int index = 0; index < requests.Count; index++)
        {
            KraidPlmRequest request = requests[index];
            ushort deactivated = level.GetCollisionBlock(request.BlockX, request.BlockY).LevelWord;
            ushort expected = unchecked((ushort)(authoredOrigins[index] & 0x8fff));
            if (deactivated != expected)
            {
                throw new InvalidDataException(
                    $"Kraid ceiling setup at ({request.BlockX},{request.BlockY}) wrote " +
                    $"${deactivated:X4}, expected ${expected:X4}.");
            }
        }

        for (int frame = 0; frame < 16; frame++)
        {
            _ = plms.Step(
                bus,
                level,
                streamer,
                CameraX,
                CameraY,
                bg1XOffset: 0,
                scrolls);
        }
        if (plms.ActiveCount != 0)
            throw new InvalidDataException("Kraid ceiling crumble PLMs did not finish and delete.");
        for (int index = 0; index < requests.Count; index++)
        {
            KraidPlmRequest request = requests[index];
            ushort final = level.GetCollisionBlock(request.BlockX, request.BlockY).LevelWord;
            if (final == authoredOrigins[index])
            {
                throw new InvalidDataException(
                    $"Kraid ceiling origin ({request.BlockX},{request.BlockY}) never drew a crumble tile.");
            }
        }
    }

    /// <summary>
    /// Exercises the real common collision walker against Kraid's physical arm record.
    /// `$A7:9490` has an encounter-visible side effect beyond ordinary damage: it launches
    /// Samus by (+4,-8) and writes the lint-fire function directly into physical slot four.
    /// Every mutated field is restored so this focused contact probe cannot alter the later
    /// rise, first phase, growth, or death lifecycle checks in this same retail room.
    /// </summary>
    private static void VerifyArmContact(
        RoomEnemySystem enemies,
        SamusState samus,
        RoomLevelData level)
    {
        // Consume one real arm instruction frame so collision uses the authored spritemap,
        // rather than manufacturing geometry for the audit. The body remains in its normal
        // rise state and the enclosing lifecycle loop simply continues from this frame.
        enemies.StepFrame(CameraX, CameraY, timeIsFrozen: false, samus, level: level);
        RoomEnemySlot arm = enemies.Slots[1];
        if (arm.SpritemapPointer == 0)
        {
            throw new InvalidDataException(
                "Kraid arm did not expose an authored spritemap on its first live frame.");
        }

        ushort[] savedX = enemies.Slots.Take(enemies.EnemyCount)
            .Select(slot => slot.XPosition)
            .ToArray();
        ushort[] savedY = enemies.Slots.Take(enemies.EnemyCount)
            .Select(slot => slot.YPosition)
            .ToArray();
        for (int slotIndex = 0; slotIndex < enemies.EnemyCount; slotIndex++)
        {
            if (slotIndex == arm.SlotIndex)
                continue;
            enemies.Slots[slotIndex].XPosition = unchecked((ushort)(
                enemies.Slots[slotIndex].XPosition + 0x4000));
            enemies.Slots[slotIndex].YPosition = unchecked((ushort)(
                enemies.Slots[slotIndex].YPosition + 0x4000));
        }

        ushort savedSamusX = samus.XPosition;
        ushort savedSamusY = samus.YPosition;
        ushort savedHealth = samus.Health;
        ushort savedExtraX = samus.Kinematics.ExtraXDisplacement;
        ushort savedExtraXSub = samus.Kinematics.ExtraXSubdisplacement;
        ushort savedExtraY = samus.Kinematics.ExtraYDisplacement;
        ushort savedExtraYSub = samus.Kinematics.ExtraYSubdisplacement;
        ushort savedLintFunction = enemies.Slots[4].VariableA;

        samus.XPosition = arm.XPosition;
        samus.YPosition = arm.YPosition;
        samus.Health = 999;
        samus.InvincibilityTimer = 0;
        samus.KnockbackTimer = 0;
        samus.KnockbackActive = false;
        samus.Kinematics.ExtraXDisplacement = 0x1111;
        samus.Kinematics.ExtraXSubdisplacement = 0x2222;
        samus.Kinematics.ExtraYDisplacement = 0x3333;
        samus.Kinematics.ExtraYSubdisplacement = 0x4444;

        bool touched = enemies.ResolveOrdinarySamusContact(samus, controllerInput: 0, level);
        ushort expectedHealth = unchecked((ushort)(999 - arm.Definition.Damage));
        if (!touched || samus.Health != expectedHealth ||
            samus.Kinematics.ExtraXDisplacement != 4 ||
            samus.Kinematics.ExtraYDisplacement != unchecked((ushort)-8) ||
            samus.Kinematics.ExtraXSubdisplacement != 0x2222 ||
            samus.Kinematics.ExtraYSubdisplacement != 0x4444 ||
            enemies.Slots[4].VariableA != (ushort)KraidAiFunction.LintFire)
        {
            throw new InvalidDataException(
                $"Kraid arm contact mismatch: touched={touched}, health=" +
                $"999->{samus.Health}/{expectedHealth}, displacement=" +
                $"({samus.Kinematics.ExtraXDisplacement:X4}." +
                $"{samus.Kinematics.ExtraXSubdisplacement:X4}," +
                $"{samus.Kinematics.ExtraYDisplacement:X4}." +
                $"{samus.Kinematics.ExtraYSubdisplacement:X4}), lint4=" +
                $"$A7:{enemies.Slots[4].VariableA:X4}.");
        }

        for (int slotIndex = 0; slotIndex < enemies.EnemyCount; slotIndex++)
        {
            enemies.Slots[slotIndex].XPosition = savedX[slotIndex];
            enemies.Slots[slotIndex].YPosition = savedY[slotIndex];
        }
        enemies.Slots[4].VariableA = savedLintFunction;
        samus.XPosition = savedSamusX;
        samus.YPosition = savedSamusY;
        samus.Health = savedHealth;
        samus.InvincibilityTimer = 0;
        samus.KnockbackTimer = 0;
        samus.KnockbackActive = false;
        samus.Kinematics.ExtraXDisplacement = savedExtraX;
        samus.Kinematics.ExtraXSubdisplacement = savedExtraXSub;
        samus.Kinematics.ExtraYDisplacement = savedExtraY;
        samus.Kinematics.ExtraYSubdisplacement = savedExtraYSub;
    }

    /// <summary>
    /// Proves the shared bank-$A0 multibox dispatcher against Kraid's first live retail arm
    /// and foot maps. Their header callback `$A7:94B5` is a species-local RTL, so it must not
    /// be confused with the engine's canonical `$804B/$804C` pre-scan gates. An active arm
    /// rectangle selects `$94B6`, creates `$86:E509` dust, queues sound `$3D`, and marks the
    /// physical projectile; the foot's `$94B5` rectangle performs only that collision mark.
    /// </summary>
    private static void VerifyMultipartShotCallbacks(
        ISnesAddressSpace bus,
        RoomEnemySystem enemies,
        SamusState samus)
    {
        const ushort noOpShotAi = 0x94b5;
        const ushort armShotAi = 0x94b6;
        RoomEnemySlot body = enemies.Slots[0];
        RoomEnemySlot arm = enemies.Slots[1];
        RoomEnemySlot foot = enemies.Slots[5];

        if (body.Definition.ShotAiPointer != 0x804c ||
            arm.Definition.ShotAiPointer != noOpShotAi ||
            foot.Definition.ShotAiPointer != noOpShotAi ||
            !RetailExtendedHitboxProbe.TryFindShotPoint(
                bus,
                arm,
                armShotAi,
                out ushort armX,
                out ushort armY))
        {
            throw new InvalidDataException(
                $"Kraid multipart shot maps were unavailable: header body/arm/foot=" +
                $"$A7:{body.Definition.ShotAiPointer:X4}/" +
                $"{arm.Definition.ShotAiPointer:X4}/{foot.Definition.ShotAiPointer:X4}, " +
                $"maps=$A7:{arm.SpritemapPointer:X4}/{foot.SpritemapPointer:X4}.");
        }

        ushort[] savedProperties = enemies.Slots.Take(enemies.EnemyCount)
            .Select(slot => slot.Properties)
            .ToArray();
        try
        {
            // Kraid's BG2 body carries canonical `$804C`. Even though it also advertises
            // extended-spritemap scheduling, both native multibox handlers return before
            // examining its current map or touching a projectile.
            IsolateEnemy(enemies, body.SlotIndex, savedProperties);
            var bodyShots = new SamusProjectileSystem();
            SamusProjectileSlot bodyShot = ArmKraidAuditShot(
                bodyShots,
                body.XPosition,
                body.YPosition);
            int bodyHits = enemies.ResolveOrdinaryProjectileHits(
                bus,
                bodyShots,
                new SamusBombProjectileSystem(),
                samus);
            if (bodyHits != 0 || (bodyShot.Direction & 0x0010) != 0 ||
                bodyShot.InstructionPointer != 0x9000)
            {
                throw new InvalidDataException(
                    $"Kraid canonical-header gate produced hits/direction/list " +
                    $"{bodyHits}/${bodyShot.Direction:X4}/${bodyShot.InstructionPointer:X4}.");
            }

            // `$94B6` does not enter common shot AI and therefore leaves both the arm and
            // Kraid body HP alone. Its only actor-side effect is a room-graphics dust spawn
            // at the selected projectile coordinates plus library-one sound `$3D`.
            IsolateEnemy(enemies, arm.SlotIndex, savedProperties);
            HashSet<int> activeBeforeBeam = ActiveEnemyProjectileSlots(enemies);
            var armShots = new SamusProjectileSystem();
            SamusProjectileSlot armShot = ArmKraidAuditShot(armShots, armX, armY);
            ushort armHealthBefore = arm.Health;
            ushort bodyHealthBefore = body.Health;
            int armHits = enemies.ResolveOrdinaryProjectileHits(
                bus,
                armShots,
                new SamusBombProjectileSystem(),
                samus);
            VerifyKraidArmDust(
                bus,
                enemies,
                activeBeforeBeam,
                armX,
                armY,
                animationIndex: 6,
                owner: "beam");
            if (armHits != 1 || (armShot.Direction & 0x0010) == 0 ||
                armShot.Type != 0x0001 || armShot.InstructionPointer != 0x9000 ||
                arm.Health != armHealthBefore || body.Health != bodyHealthBefore)
            {
                throw new InvalidDataException(
                    $"Kraid arm beam callback mismatch: hits={armHits}, " +
                    $"direction=${armShot.Direction:X4}, type/list=" +
                    $"${armShot.Type:X4}/${armShot.InstructionPointer:X4}, health " +
                    $"arm/body={armHealthBefore}->{arm.Health}/" +
                    $"{bodyHealthBefore}->{body.Health}.");
            }

            IsolateEnemy(enemies, arm.SlotIndex, savedProperties);
            HashSet<int> activeBeforeBomb = ActiveEnemyProjectileSlots(enemies);
            var armBombs = new SamusBombProjectileSystem();
            SamusBombProjectileSlot armBomb =
                EnemyProjectileAuditAssertions.ArmExplodingNormalBomb(
                    armBombs,
                    armX,
                    armY);
            int armBombHits = enemies.ResolveOrdinaryBombHits(
                armBombs,
                new SamusProjectileSystem(),
                samus);
            VerifyKraidArmDust(
                bus,
                enemies,
                activeBeforeBomb,
                armX,
                armY,
                animationIndex: 6,
                owner: "normal bomb");
            if (armBombHits != 1 || (armBomb.Direction & 0x0010) == 0 ||
                arm.Health != armHealthBefore || body.Health != bodyHealthBefore)
            {
                throw new InvalidDataException(
                    $"Kraid arm normal-bomb callback mismatch: hits={armBombHits}, " +
                    $"direction=${armBomb.Direction:X4}, health arm/body=" +
                    $"{armHealthBefore}->{arm.Health}/{bodyHealthBefore}->{body.Health}.");
            }

        }
        finally
        {
            for (int slotIndex = 0; slotIndex < savedProperties.Length; slotIndex++)
                enemies.Slots[slotIndex].Properties = savedProperties[slotIndex];
        }
    }

    /// <summary>
    /// Proves that the foot's authored `$A7:94B5` rectangles remain collision-inert even in
    /// the naturally visible second-phase walk. Its retail property word retains `$0400`,
    /// keeping the actor out of the interactive list; the extended bomb handler independently
    /// retests the same bit before scanning. The map is real, but dispatching its callback
    /// would require manufacturing a property state the cartridge never enters.
    /// </summary>
    private static void VerifyKraidFootCollisionSuppression(
        ISnesAddressSpace bus,
        RoomEnemySystem enemies,
        SamusState samus)
    {
        const ushort noOpShotAi = 0x94b5;
        RoomEnemySlot body = enemies.Slots[0];
        RoomEnemySlot foot = enemies.Slots[5];
        if (enemies.InteractiveEnemyIndexes.Contains(foot.NativeIndex) ||
            !foot.Properties.HasAny(EnemyProperties.IgnoreSamusCollision) ||
            !RetailExtendedHitboxProbe.TryFindShotPoint(
                bus,
                foot,
                noOpShotAi,
                out ushort footX,
                out ushort footY))
        {
            throw new InvalidDataException(
                $"Kraid foot property gate diverged from retail: interactive=" +
                $"{enemies.InteractiveEnemyIndexes.Contains(foot.NativeIndex)}, " +
                $"properties=${foot.Properties:X4}, map=$A7:{foot.SpritemapPointer:X4}.");
        }

        ushort[] savedProperties = enemies.Slots.Take(enemies.EnemyCount)
            .Select(slot => slot.Properties)
            .ToArray();
        try
        {
            IsolateEnemy(enemies, foot.SlotIndex, savedProperties);
            HashSet<int> activeBefore = ActiveEnemyProjectileSlots(enemies);
            var bombs = new SamusBombProjectileSystem();
            SamusBombProjectileSlot bomb =
                EnemyProjectileAuditAssertions.ArmExplodingNormalBomb(
                    bombs,
                    footX,
                    footY);
            ushort footHealthBefore = foot.Health;
            ushort bodyHealthBefore = body.Health;
            int hits = enemies.ResolveOrdinaryBombHits(
                bombs,
                new SamusProjectileSystem(),
                samus);
            bool spawnedDust = enemies.EnemyProjectiles.Any(projectile =>
                projectile.IsActive &&
                !activeBefore.Contains(projectile.SlotIndex) &&
                projectile.Kind == RoomEnemyProjectileKind.MiscDustExplosion);
            if (hits != 0 || (bomb.Direction & 0x0010) != 0 ||
                foot.Health != footHealthBefore || body.Health != bodyHealthBefore ||
                spawnedDust)
            {
                throw new InvalidDataException(
                    $"Kraid foot property suppression mismatch: hits={hits}, " +
                    $"direction=${bomb.Direction:X4}, health=" +
                    $"{footHealthBefore}->{foot.Health}, body=" +
                    $"{bodyHealthBefore}->{body.Health}, dust={spawnedDust}.");
            }
        }
        finally
        {
            for (int slotIndex = 0; slotIndex < savedProperties.Length; slotIndex++)
                enemies.Slots[slotIndex].Properties = savedProperties[slotIndex];
        }
    }

    private static SamusProjectileSlot ArmKraidAuditShot(
        SamusProjectileSystem projectiles,
        ushort x,
        ushort y)
    {
        SamusProjectileSlot shot = projectiles.Slots[0];
        shot.ClearFields();
        shot.Type = 0x0001;
        shot.Damage = 20;
        shot.Direction = (ushort)SamusProjectileDirection.Right;
        shot.XPosition = x;
        shot.YPosition = y;
        shot.XRadius = 1;
        shot.YRadius = 1;
        shot.InstructionPointer = 0x9000;
        shot.InstructionTimer = 1;
        return shot;
    }

    private static void IsolateEnemy(
        RoomEnemySystem enemies,
        int retainedSlot,
        ushort[] savedProperties)
    {
        for (int slotIndex = 0; slotIndex < savedProperties.Length; slotIndex++)
        {
            enemies.Slots[slotIndex].Properties = slotIndex == retainedSlot
                ? savedProperties[slotIndex]
                : savedProperties[slotIndex].With(EnemyProperties.Deleted);
        }
    }

    private static HashSet<int> ActiveEnemyProjectileSlots(RoomEnemySystem enemies) =>
        enemies.EnemyProjectiles
            .Where(projectile => projectile.IsActive)
            .Select(projectile => projectile.SlotIndex)
            .ToHashSet();

    private static void VerifyKraidArmDust(
        ISnesAddressSpace bus,
        RoomEnemySystem enemies,
        HashSet<int> activeBefore,
        ushort expectedX,
        ushort expectedY,
        ushort animationIndex,
        string owner)
    {
        RoomEnemyProjectileSlot[] newDust = enemies.EnemyProjectiles
            .Where(projectile =>
                projectile.IsActive &&
                !activeBefore.Contains(projectile.SlotIndex) &&
                projectile.Kind == RoomEnemyProjectileKind.MiscDustExplosion)
            .ToArray();
        ushort expectedInstruction = ReadWord(
            bus,
            0x86e42c + animationIndex * 2);
        if (newDust.Length != 1 || newDust[0].XPosition != expectedX ||
            newDust[0].YPosition != expectedY ||
            newDust[0].InstructionPointer != expectedInstruction ||
            newDust[0].InstructionTimer != 1 ||
            enemies.LastEnemyProjectileDudSoundEffect != 0x003d)
        {
            string actual = newDust.Length == 1
                ? $"({newDust[0].XPosition},{newDust[0].YPosition})/" +
                  $"$86:{newDust[0].InstructionPointer:X4}/" +
                  $"{newDust[0].InstructionTimer}"
                : $"count {newDust.Length}";
            throw new InvalidDataException(
                $"Kraid arm {owner} dust mismatch: {actual}, expected " +
                $"({expectedX},{expectedY})/$86:{expectedInstruction:X4}/1, sound=" +
                $"{enemies.LastEnemyProjectileDudSoundEffect?.ToString("X4") ?? "none"}.");
        }
    }

    private static void VerifyRetailRoom(CartridgeRoomHeader room)
    {
        if (room.AreaIndex != AreaId.Brinstar || room.WidthInScreens != 2 || room.HeightInScreens != 2 ||
            room.State.Pointer != 0xa5b1 || room.State.EnemyPopulationPointer != PopulationPointer)
        {
            throw new InvalidDataException(
                $"Kraid room mismatch: area={room.AreaIndex}, " +
                $"size={room.WidthInScreens}x{room.HeightInScreens}, " +
                $"state=$8F:{room.State.Pointer:X4}, population=$A1:" +
                $"{room.State.EnemyPopulationPointer:X4}.");
        }
    }

    private static void VerifyDefeatedRoom(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room)
    {
        var defeated = new RoomEnemySystem();
        defeated.Load(
            bus,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            new SnesVram(),
            new SnesCgram(),
            () => 0,
            isAreaBossDefeated: () => true);
        if (defeated.EnemyCount != ExpectedDefinitions.Length || defeated.Kraid is null ||
            !defeated.KraidPlmRequests.SequenceEqual(KraidPlmDefinitions.DefeatedRoom) ||
            defeated.Slots[0].VariableA != (ushort)KraidAiFunction.DeathClearTopTilemap ||
            defeated.Slots.Skip(1).Take(ExpectedDefinitions.Length - 1).Any(
                slot => !slot.Properties.HasAny(
                    EnemyProperties.Deleted | EnemyProperties.Invisible)))
        {
            throw new InvalidDataException(
                "Defeated Kraid room did not retain its body restoration owner and delete the seven parts.");
        }
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    /// <summary>
    /// Sends one naturally spawned, previously unseen Kraid projectile through the shared
    /// contact assertion. A separate initialized Samus record prevents the forced overlap
    /// from changing the boss fixture's position, pose, health, or knockback state.
    /// </summary>
    private static void ProbeFirstUnauditedProjectileContact(
        ISnesAddressSpace bus,
        RoomEnemySystem enemies,
        RoomLevelData level,
        HashSet<RoomEnemyProjectileKind> observedKinds,
        HashSet<RoomEnemyProjectileKind> contactedKinds)
    {
        foreach (RoomEnemyProjectileSlot projectile in enemies.EnemyProjectiles)
        {
            if (projectile.Kind is RoomEnemyProjectileKind.KraidSpitRock or
                RoomEnemyProjectileKind.KraidCeilingRock or
                RoomEnemyProjectileKind.KraidRisingRockLeft or
                RoomEnemyProjectileKind.KraidRisingRockRight)
            {
                observedKinds.Add(projectile.Kind);
            }
        }

        RoomEnemyProjectileSlot? target = enemies.EnemyProjectiles.FirstOrDefault(
            projectile => projectile.IsActive && projectile.CanDamageSamus &&
                projectile.Kind is (RoomEnemyProjectileKind.KraidSpitRock or
                    RoomEnemyProjectileKind.KraidCeilingRock or
                    RoomEnemyProjectileKind.KraidRisingRockLeft or
                    RoomEnemyProjectileKind.KraidRisingRockRight) &&
                !contactedKinds.Contains(projectile.Kind));
        if (target is null)
            return;

        RoomEnemyProjectileKind kind = target.Kind;
        var probeSamus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusPoseIds.FacingRightNormalPose,
        };
        probeSamus.RefreshCollisionRadii(bus);
        probeSamus.InitializeAnimation(bus);
        EnemyProjectileAuditAssertions.VerifyNaturalSamusContact(
            bus,
            enemies,
            probeSamus,
            new SamusBombProjectileSystem(),
            level,
            target,
            CameraX,
            CameraY);
        contactedKinds.Add(kind);
    }

    /// <summary>
    /// Executes Kraid's actual rise AI with random bit $10 clear so bank $86 initializes
    /// and moves the left-hand debris definition. This is a second authored scenario, not
    /// a post-spawn kind substitution; every field still comes from $86:9C61.
    /// </summary>
    private static RoomEnemyProjectileKind VerifyOppositeRisingRockVariant(
        ISnesAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState(0x1224);
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            XPosition = 300,
            YPosition = 456,
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
            setRoomScrollState: assets.Scrolls.SetStorage,
            cameraX: CameraX,
            cameraY: CameraY);

        for (int frame = 0; frame < 1200; frame++)
        {
            enemies.StepFrame(
                CameraX,
                CameraY,
                timeIsFrozen: false,
                samus,
                level: assets.LevelData);
            RoomEnemyProjectileSlot? left = enemies.EnemyProjectiles.FirstOrDefault(
                projectile => projectile.Kind == RoomEnemyProjectileKind.KraidRisingRockLeft);
            if (left is null)
            {
                enemies.StepEnemyProjectiles(
                    assets.LevelData,
                    samus,
                    cameraX: CameraX,
                    cameraY: CameraY);
                continue;
            }

            int definition = 0x860000 | (ushort)left.Kind;
            ushort radii = ReadWord(bus, definition + 6);
            ushort properties = ReadWord(bus, definition + 8);
            ushort beforeX = left.XPosition;
            ushort beforeY = left.YPosition;
            enemies.StepEnemyProjectiles(
                assets.LevelData,
                samus,
                cameraX: CameraX,
                cameraY: CameraY);

            var oam = new OamBuffer();
            oam.BeginFrame();
            enemies.DrawEnemyProjectiles(oam, CameraX, CameraY);
            oam.FinalizeFrame();
            if (!left.IsActive ||
                left.XRadius != unchecked((byte)radii) ||
                left.YRadius != unchecked((byte)(radii >> 8)) ||
                left.Damage != (properties & 0x0fff) ||
                left.CanDamageSamus != ((properties & 0x2000) == 0) ||
                beforeX == left.XPosition && beforeY == left.YPosition ||
                left.SpritemapPointer is 0 or 0x8000 ||
                oam.LastFinalizedSpriteCount == 0)
            {
                throw new InvalidDataException(
                    $"Kraid left rise rock diverged on frame {frame}: active={left.IsActive}, " +
                    $"position=({beforeX:X4},{beforeY:X4})->" +
                    $"({left.XPosition:X4},{left.YPosition:X4}), map=" +
                    $"${left.SpritemapPointer:X4}, radius={left.XRadius}x{left.YRadius}/" +
                    $"${radii:X4}, damage/collision={left.Damage}/{left.CanDamageSamus}, " +
                    $"OAM={oam.LastFinalizedSpriteCount}.");
            }
            return left.Kind;
        }

        throw new InvalidDataException(
            "Kraid bit-$10-clear rise fixture never spawned left-hand debris.");
    }

    private static int StrikeKraidMouth(
        ISnesAddressSpace bus,
        RoomEnemySystem enemies,
        RoomEnemySlot body,
        KraidEnemyState state,
        ushort projectileDamage)
    {
        if (state.InvulnerableMouthHitbox == ushort.MaxValue)
            throw new InvalidDataException("Kraid audit attempted a mouth hit while it was closed.");
        int mouthAddress = 0xa70000 | state.InvulnerableMouthHitbox;
        short left = unchecked((short)ReadWord(bus, mouthAddress));
        short top = unchecked((short)ReadWord(bus, mouthAddress + 2));
        short bottom = unchecked((short)ReadWord(bus, mouthAddress + 6));
        var shots = new SamusProjectileSystem();
        SamusProjectileSlot missile = shots.Slots[0];
        missile.ClearFields();
        missile.Type = 0x8100;
        missile.Damage = projectileDamage;
        missile.Direction = (ushort)SamusProjectileDirection.Right;
        missile.XPosition = unchecked((ushort)(body.XPosition + left + 2));
        missile.YPosition = unchecked((ushort)(body.YPosition + (top + bottom) / 2));
        missile.XRadius = 2;
        missile.YRadius = 2;
        missile.InstructionPointer = 0x9000;
        missile.InstructionTimer = 1;
        return enemies.ResolveKraidProjectileHits(
            bus,
            shots,
            new SamusBombProjectileSystem());
    }

    private static int StrikeKraidOuterBody(
        ISnesAddressSpace bus,
        RoomEnemySystem enemies,
        RoomEnemySlot body,
        ushort projectileDamage)
    {
        var shots = new SamusProjectileSystem();
        SamusProjectileSlot chargedBeam = shots.Slots[0];
        chargedBeam.ClearFields();
        chargedBeam.Type = 0x8010;
        chargedBeam.Damage = projectileDamage;
        chargedBeam.Direction = (ushort)SamusProjectileDirection.Right;
        chargedBeam.XPosition = unchecked((ushort)(body.XPosition + 20));
        chargedBeam.YPosition = unchecked((ushort)(body.YPosition + 40));
        chargedBeam.XRadius = 4;
        chargedBeam.YRadius = 4;
        chargedBeam.InstructionPointer = 0x9000;
        chargedBeam.InstructionTimer = 1;
        return enemies.ResolveKraidProjectileHits(
            bus,
            shots,
            new SamusBombProjectileSystem());
    }
}
