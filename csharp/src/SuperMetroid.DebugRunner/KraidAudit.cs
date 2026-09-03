using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
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
    private const ushort CameraX = 0;
    private const ushort CameraY = 256;

    private static readonly ushort[] ExpectedDefinitions =
        [0xe2bf, 0xe2ff, 0xe33f, 0xe37f, 0xe3bf, 0xe3ff, 0xe43f, 0xe47f];

    public static int Run(string romPath)
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
            isAreaBossDefeated: () => bossDefeated,
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
            state.MusicRequest != 5 || state.RiseRockSpawnRequestCount == 0 ||
            state.SpawnedRiseRockCount == 0 || !sawNailMovement || functions.Count < 6)
        {
            throw new InvalidDataException(
                $"Kraid rise mismatch after {frame} frames: function=$A7:{body.VariableA:X4}, " +
                $"Samus X={samus.XPosition}, body=({body.XPosition},{body.YPosition}), " +
                $"uploads={state.TopTilemapUploadCount}/{state.BottomTilemapUploadCount}, " +
                $"music={state.MusicRequest}, rocks=" +
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
            sawFootstep |= enemies.LastKraidSoundEffect is { Library: 2, SoundEffect: 0x0076 };
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
        ushort? secondPhaseStartY = null;
        for (int growthFrame = 0; growthFrame < 2600; growthFrame++)
        {
            growthFunctions.Add((KraidAiFunction)body.VariableA);
            if (body.VariableA == (ushort)KraidAiFunction.SecondPhaseThinking)
                secondPhaseStartY ??= body.YPosition;
            secondPhaseFootFunctions.Add((KraidAiFunction)enemies.Slots[5].VariableA);
            for (int lintSlot = 2; lintSlot <= 4; lintSlot++)
                lintFunctions.Add((KraidAiFunction)enemies.Slots[lintSlot].VariableA);
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
            enemies.StepEnemyProjectiles(assets.LevelData, samus, cameraX: CameraX, cameraY: CameraY);
        }
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
                $"palette={roomPaletteReachedTarget}, missing projectile contact=" +
                $"[{string.Join(',', missingContactedProjectiles)}], missing observation=" +
                $"[{string.Join(',', missingObservedProjectiles)}].");
        }

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
            enemies.StepEnemyProjectiles(assets.LevelData, samus, cameraX: CameraX, cameraY: CameraY);
        }
        if (!state.DeathSequenceComplete || !state.BossDefeatPersisted || !bossDefeated ||
            body.YPosition < 608 || state.SinkTableEventCount < 20 ||
            state.DeathDropRequestCount != 16 || state.DeathBg3TransferCount != 4 ||
            state.MusicRequest != 3 ||
            !deathFunctions.Contains(KraidAiFunction.DeathFadeOut) ||
            !deathFunctions.Contains(KraidAiFunction.DeathSink) ||
            !deathFunctions.Contains(KraidAiFunction.DeathFadeInBackground))
        {
            throw new InvalidDataException(
                $"Kraid death mismatch after {deathFrames} frames: complete/persisted/bit=" +
                $"{state.DeathSequenceComplete}/{state.BossDefeatPersisted}/{bossDefeated}, " +
                $"Y={body.YPosition}, sink events={state.SinkTableEventCount}, " +
                $"drops/BG3/music={state.DeathDropRequestCount}/" +
                $"{state.DeathBg3TransferCount}/{state.MusicRequest}, functions=" +
                $"{string.Join(',', deathFunctions)}.");
        }

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
            $"walking/lint attacks, and {deathFrames}-frame sink/death/persistence.");
        return 0;
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
        if (room.AreaIndex != 1 || room.WidthInScreens != 2 || room.HeightInScreens != 2 ||
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
            defeated.Slots.Take(ExpectedDefinitions.Length).Any(
                slot => !slot.Properties.HasAny(
                    EnemyProperties.Deleted | EnemyProperties.Invisible)))
        {
            throw new InvalidDataException(
                "Defeated Kraid room did not retain and delete all eight native part records.");
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
            Pose = SamusState.FacingRightNormalPose,
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
