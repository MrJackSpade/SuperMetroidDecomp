using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>
/// ROM-backed regression for Wrecked Ship Spark. Electric Death's defeated-Phantoon state
/// is an untouched seven-Spark population containing both intermittent and falling-projectile
/// variants. Main Shaft supplies the shipped always-active variant through a one-record view
/// beginning at its unchanged sixth Spark record; the view changes only the later terminator.
/// </summary>
internal static class SparkAudit
{
    private const ushort ElectricDeathRoom = 0xcbd5;
    private const ushort MainShaftRoom = 0xcaf6;
    private const ushort SparkDefinition = 0xea3f;

    private static readonly HashSet<ushort> ConstantMaps =
    [
        0xe71f,
        0xe72b,
        0xe737,
        0xe743,
    ];

    private static readonly HashSet<ushort> FlickeringMaps =
    [
        0x804d,
        0xe74f,
        0xe76a,
        0xe785,
        0xe79b,
    ];

    private static readonly HashSet<ushort> FallingMaps =
    [
        0x8000,
        0xc4ad,
        0xc4b4,
        0xc4bb,
    ];

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader electricDeath = CartridgeRoomHeader.Load(
            bus,
            ElectricDeathRoom,
            new RoomStateSelectionContext(default, BossBits: 1, false, false));
        CartridgeRoomAssets electricAssets = CartridgeRoomAssets.Load(bus, electricDeath);
        LoadedSparks loaded = Load(
            bus,
            electricDeath,
            electricAssets,
            bossDefeated: true,
            randomSeed: 0x1234);

        VerifyFullPopulationAndHeaders(electricDeath, loaded);
        AnimationResult animation = VerifyIntermittentAndEmitterAnimations(
            electricDeath,
            electricAssets,
            loaded);
        ProjectileResult projectile = VerifyNaturalFallingProjectile(
            bus,
            electricDeath,
            electricAssets);
        VerifyProjectileDamage(bus, electricDeath, electricAssets);
        VerifyShotPowerBombContactAndGrapple(bus, electricDeath, electricAssets);
        VerifyRandomTimerPath(bus, electricDeath, electricAssets);
        VerifyRandomTableOverread(bus, electricDeath, electricAssets);
        VerifyAlwaysActiveRetailVariant(bus);
        VerifyPreBossAbsoluteWramOr(bus, electricDeath, electricAssets);

        Console.WriteLine(
            "Spark audit passed: untouched seven-actor Electric Death population and " +
            $"retail always-active variant loaded; {animation.MapCount} body maps covered " +
            $"tangible/intangible cycles, falling attack covered {projectile.MapCount} maps " +
            $"and {projectile.TrailMaps} trail maps, floor rebound/deletion, 5 projectile " +
            "damage, 30 contact damage, indestructible beam/power-bomb behavior, grapple " +
            "hurt, randomized timers, the table overread, and the literal $7E:0100 OR bug.");
        return 0;
    }

    private static void VerifyFullPopulationAndHeaders(
        CartridgeRoomHeader room,
        LoadedSparks loaded)
    {
        RoomEnemySlot[] population = loaded.Enemies.Slots
            .Take(loaded.Enemies.EnemyCount)
            .ToArray();
        if (room.State.Pointer != 0xcc01 || population.Length != 7 ||
            population.Any(slot => slot.EnemyDefinitionPointer != SparkDefinition) ||
            population.Any(slot => loaded.Enemies.SparkStates[slot.SlotIndex] is null))
        {
            throw new InvalidDataException(
                $"Electric Death Spark population failed: state=${room.State.Pointer:X4}, " +
                $"count={population.Length}, definitions=" +
                $"[{string.Join(',', population.Select(slot => $"${slot.EnemyDefinitionPointer:X4}"))}].");
        }

        ushort[] expectedX = [0x0068, 0x002a, 0x00c8, 0x0086, 0x004f, 0x0035, 0x005c];
        ushort[] expectedY = [0x0258, 0x0227, 0x01dd, 0x018a, 0x0033, 0x00da, 0x0098];
        ushort[] expectedKind = [2, 1, 1, 2, 2, 1, 2];
        ushort[] expectedTime = [0x80, 0x80, 0x10, 0x90, 0x90, 0x10, 0xa0];

        for (int index = 0; index < population.Length; index++)
        {
            RoomEnemySlot actor = population[index];
            SparkEnemyState state = RequireState(loaded.Enemies, actor);
            SparkEnemyFunction expectedFunction = expectedKind[index] == 1
                ? SparkEnemyFunction.IntermittentActive
                : SparkEnemyFunction.EmitFallingSparks;
            ushort expectedList = expectedKind[index] == 1 ? (ushort)0xe5d1 : (ushort)0xe609;
            RoomEnemyDefinition definition = actor.Definition;

            if (actor.XPosition != expectedX[index] || actor.YPosition != expectedY[index] ||
                actor.Parameter1 != expectedKind[index] || actor.Parameter2 != expectedTime[index] ||
                actor.Properties != 0x2000 || state.Function != expectedFunction ||
                state.BaseFunctionTime != expectedTime[index] ||
                state.FunctionTimer != expectedTime[index] ||
                actor.CurrentInstruction != expectedList || actor.InstructionTimer != 1 ||
                definition.TileDataSize != 0x0200 || definition.PalettePointer != 0xe587 ||
                actor.Health != 80 || definition.Damage != 30 || actor.XRadius != 8 ||
                actor.YRadius != 8 || definition.Bank != 0xa8 ||
                definition.InitializationAiPointer != 0xe637 ||
                definition.MainAiPointer != 0xe68e ||
                definition.GrappleAiPointer != 0x801e ||
                definition.DeathAnimation != 0 ||
                definition.PowerBombReactionPointer != 0 ||
                definition.TouchAiPointer != 0x8023 ||
                definition.ShotAiPointer != 0xe70e || actor.Layer != 5 ||
                definition.ItemDropChancesPointer != 0xf458 ||
                definition.VulnerabilityPointer != 0xeec6)
            {
                throw new InvalidDataException(
                    $"Electric Death Spark {index} initialization failed: position=" +
                    $"(${actor.XPosition:X4},${actor.YPosition:X4}), params=" +
                    $"${actor.Parameter1:X4}/${actor.Parameter2:X4}, properties=" +
                    $"${actor.Properties:X4}, function=$A8:{(ushort)state.Function:X4}, " +
                    $"timer={state.FunctionTimer}, list=$A8:{actor.CurrentInstruction:X4}, " +
                    $"health/damage={actor.Health}/{definition.Damage}.");
            }
        }
    }

    private static AnimationResult VerifyIntermittentAndEmitterAnimations(
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        LoadedSparks loaded)
    {
        RoomEnemySlot intermittent = loaded.Enemies.Slots[2];
        RoomEnemySlot emitter = loaded.Enemies.Slots[4];
        SparkEnemyState intermittentState = RequireState(loaded.Enemies, intermittent);
        var intermittentMaps = new HashSet<ushort>();
        var emitterMaps = new HashSet<ushort>();
        bool sawInactiveFunction = false;
        bool sawIntangible = false;
        bool returnedActive = false;

        for (int frame = 0; frame < 220; frame++)
        {
            StepCentered(loaded, room, assets, intermittent);
            intermittentMaps.Add(intermittent.SpritemapPointer);
            sawInactiveFunction |=
                intermittentState.Function == SparkEnemyFunction.IntermittentInactive;
            sawIntangible |= intermittent.Properties.HasAny(EnemyProperties.IgnoreSamusCollision);
            returnedActive |= sawIntangible &&
                intermittentState.Function == SparkEnemyFunction.IntermittentActive &&
                !intermittent.Properties.HasAny(EnemyProperties.IgnoreSamusCollision);
        }

        // The emitter sits in a different screen. Give its instruction list a complete cycle
        // without relying on ProcessOffScreen, which the retail population does not request.
        for (int frame = 0; frame < 24; frame++)
        {
            StepCentered(loaded, room, assets, emitter);
            emitterMaps.Add(emitter.SpritemapPointer);
        }

        HashSet<ushort> expectedIntermittent = [.. ConstantMaps, .. FlickeringMaps];
        HashSet<ushort> expectedEmitter = FlickeringMaps.Where(map => map != 0x804d).ToHashSet();
        if (!intermittentMaps.IsSupersetOf(expectedIntermittent) ||
            !emitterMaps.SetEquals(expectedEmitter) || !sawInactiveFunction ||
            !sawIntangible || !returnedActive)
        {
            throw new InvalidDataException(
                $"Spark animation cycle failed: intermittent=" +
                $"[{string.Join(',', intermittentMaps.Select(map => $"${map:X4}"))}], " +
                $"emitter=[{string.Join(',', emitterMaps.Select(map => $"${map:X4}"))}], " +
                $"inactive/intangible/reactivated=" +
                $"{sawInactiveFunction}/{sawIntangible}/{returnedActive}.");
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        (ushort cameraX, ushort cameraY) = CenterCamera(room, emitter);
        loaded.Enemies.DrawLayers(oam, cameraX, cameraY, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("Spark body animation emitted no ROM-authored OBJ.");

        return new AnimationResult(intermittentMaps.Union(emitterMaps).Distinct().Count());
    }

    private static ProjectileResult VerifyNaturalFallingProjectile(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedSparks loaded = Load(bus, room, assets, bossDefeated: true, randomSeed: 0x1234);
        RoomEnemySlot emitter = loaded.Enemies.Slots[4];
        var fallingMaps = new HashSet<ushort>();
        var trailMaps = new HashSet<ushort>();
        bool sawProjectile = false;
        bool sawDownwardAcceleration = false;
        bool sawFloorImpact = false;
        bool sawProjectileDeletion = false;
        bool sawTrailDeletion = false;
        int peakTrailCount = 0;

        for (int frame = 0; frame < 1024; frame++)
        {
            StepCentered(loaded, room, assets, emitter);
            loaded.Enemies.StepEnemyProjectiles(
                assets.LevelData,
                samus: null,
                nmiFrameCounter8: unchecked((byte)frame));

            RoomEnemyProjectileSlot? projectile = loaded.Enemies.EnemyProjectiles
                .FirstOrDefault(candidate =>
                    candidate.Kind == RoomEnemyProjectileKind.FallingSpark);
            if (projectile is not null)
            {
                sawProjectile = true;
                fallingMaps.Add(projectile.SpritemapPointer);
                sawDownwardAcceleration |= projectile.YVelocity > 0 ||
                    projectile.XVelocity != 0;
                sawFloorImpact |= projectile.YVelocity == 0xffff &&
                    projectile.XVelocity == 0x8000;
            }
            else if (sawFloorImpact)
            {
                sawProjectileDeletion = true;
            }

            foreach (FallingSparkTrailSlot trail in loaded.Enemies.FallingSparkTrails
                         .Where(trail => trail.IsActive))
            {
                trailMaps.Add(trail.SpritemapPointer);
            }
            peakTrailCount = Math.Max(
                peakTrailCount,
                loaded.Enemies.ActiveFallingSparkTrailCount);
            sawTrailDeletion |= peakTrailCount != 0 &&
                loaded.Enemies.ActiveFallingSparkTrailCount < peakTrailCount;

            if (sawProjectileDeletion && sawTrailDeletion &&
                trailMaps.Count == 4 && fallingMaps.IsSupersetOf(FallingMaps))
            {
                break;
            }
        }

        if (!sawProjectile || !sawDownwardAcceleration || !sawFloorImpact ||
            !sawProjectileDeletion || !sawTrailDeletion || peakTrailCount == 0 ||
            trailMaps.Count != 4 || !fallingMaps.IsSupersetOf(FallingMaps))
        {
            throw new InvalidDataException(
                $"Natural falling Spark failed: spawned/accelerated/floor/deleted=" +
                $"{sawProjectile}/{sawDownwardAcceleration}/{sawFloorImpact}/" +
                $"{sawProjectileDeletion}, maps=" +
                $"[{string.Join(',', fallingMaps.Select(map => $"${map:X4}"))}], " +
                $"trails={peakTrailCount}/" +
                $"[{string.Join(',', trailMaps.Select(map => $"${map:X4}"))}]/" +
                $"deleted={sawTrailDeletion}.");
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        (ushort cameraX, ushort cameraY) = CenterCamera(room, emitter);
        loaded.Enemies.DrawEnemyProjectiles(oam, cameraX, cameraY);
        oam.FinalizeFrame();
        // The actor may already have completed by the time all deletion predicates settle.
        // Drawing validity is covered more directly in the damage run while it is live.

        return new ProjectileResult(fallingMaps.Count, trailMaps.Count);
    }

    private static void VerifyProjectileDamage(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedSparks loaded = Load(bus, room, assets, bossDefeated: true, randomSeed: 0x2468);
        RoomEnemySlot emitter = loaded.Enemies.Slots[0];
        SparkEnemyState state = RequireState(loaded.Enemies, emitter);
        state.FunctionTimer = 1;
        StepCentered(loaded, room, assets, emitter);
        RoomEnemyProjectileSlot projectile = loaded.Enemies.EnemyProjectiles.Single(candidate =>
            candidate.Kind == RoomEnemyProjectileKind.FallingSpark);

        loaded.Samus.XPosition = projectile.XPosition;
        loaded.Samus.YPosition = projectile.YPosition;
        ushort health = loaded.Samus.Health;
        loaded.Enemies.StepEnemyProjectiles(
            assets.LevelData,
            loaded.Samus,
            nmiFrameCounter8: 0);
        if (loaded.Samus.Health != health - 5 || !loaded.Samus.KnockbackActive ||
            loaded.Samus.InvincibilityTimer != 96 || projectile.IsActive)
        {
            throw new InvalidDataException(
                $"Falling Spark contact failed: health={health}->{loaded.Samus.Health}, " +
                $"knockback={loaded.Samus.KnockbackActive}, invincibility=" +
                $"{loaded.Samus.InvincibilityTimer}, live={projectile.IsActive}.");
        }

        var drawing = Load(bus, room, assets, bossDefeated: true, randomSeed: 0x2468);
        RoomEnemySlot drawingEmitter = drawing.Enemies.Slots[0];
        RequireState(drawing.Enemies, drawingEmitter).FunctionTimer = 1;
        StepCentered(drawing, room, assets, drawingEmitter);
        drawing.Enemies.StepEnemyProjectiles(
            assets.LevelData,
            samus: null,
            nmiFrameCounter8: 0);
        var oam = new OamBuffer();
        oam.BeginFrame();
        (ushort cameraX, ushort cameraY) = CenterCamera(room, drawingEmitter);
        drawing.Enemies.DrawEnemyProjectiles(oam, cameraX, cameraY);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount < 2)
        {
            throw new InvalidDataException(
                $"Falling Spark projectile/trail emitted only {oam.LastFinalizedSpriteCount} OBJ pieces.");
        }
    }

    private static void VerifyShotPowerBombContactAndGrapple(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedSparks loaded = Load(bus, room, assets, bossDefeated: true, randomSeed: 0x3456);
        RoomEnemySlot actor = loaded.Enemies.Slots[1];
        StepCentered(loaded, room, assets, actor);

        var shots = new SamusProjectileSystem();
        SamusProjectileSlot shot = shots.Slots[0];
        ArmProjectile(shot, actor);
        int hits = loaded.Enemies.ResolveOrdinaryProjectileHits(
            bus,
            shots,
            new SamusBombProjectileSystem(),
            loaded.Samus);
        if (hits != 1 || actor.Health != 80 || shot.Direction != 2 ||
            shot.InstructionPointer != 0x9000 || !shot.IsActive)
        {
            throw new InvalidDataException(
                $"Spark pass-through shot failed: hits={hits}, health={actor.Health}, " +
                $"direction=${shot.Direction:X4}, list=${shot.InstructionPointer:X4}, " +
                $"active={shot.IsActive}.");
        }

        int powerBombHits = loaded.Enemies.ResolveOrdinaryPowerBombHits(
            bus,
            actor.XPosition,
            actor.YPosition,
            explosionRadius: 64);
        if (powerBombHits != 0 || actor.Health != 80)
        {
            throw new InvalidDataException(
                $"Indestructible Spark accepted {powerBombHits} power bombs; health={actor.Health}.");
        }

        loaded.Samus.InvincibilityTimer = 0;
        loaded.Samus.XPosition = actor.XPosition;
        loaded.Samus.YPosition = actor.YPosition;
        ushort health = loaded.Samus.Health;
        if (!loaded.Enemies.ResolveOrdinarySamusContact(loaded.Samus, 0) ||
            loaded.Samus.Health != health - 30 || !loaded.Samus.KnockbackActive)
        {
            throw new InvalidDataException(
                $"Spark body contact failed: health={health}->{loaded.Samus.Health}, " +
                $"knockback={loaded.Samus.KnockbackActive}.");
        }

        LoadedSparks grappleLoad = Load(
            bus,
            room,
            assets,
            bossDefeated: true,
            randomSeed: 0x3456);
        RoomEnemySlot grappleActor = grappleLoad.Enemies.Slots[1];
        StepCentered(grappleLoad, room, assets, grappleActor);
        GrappleEnemyCollision grapple = grappleLoad.Enemies.ResolveGrappleEndpoint(
            grappleActor.XPosition,
            grappleActor.YPosition);
        if (!grapple.Collided || grapple.Reaction != GrappleEnemyReaction.HurtSamus ||
            grapple.EnemyDamage != 30 || grappleActor.AiHandlerBits != 1)
        {
            throw new InvalidDataException(
                $"Spark grapple failed: collided={grapple.Collided}, reaction=" +
                $"{grapple.Reaction}, damage={grapple.EnemyDamage}, handler=" +
                $"${grappleActor.AiHandlerBits:X4}.");
        }
        StepCentered(grappleLoad, room, assets, grappleActor);
        if (grappleActor.AiHandlerBits != 4)
            throw new InvalidDataException("Spark grapple-hurt handler did not enter frozen/cancel state.");
    }

    private static void VerifyRandomTimerPath(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedSparks loaded = Load(bus, room, assets, bossDefeated: true, randomSeed: 0x4567);
        RoomEnemySlot actor = loaded.Enemies.Slots[1];
        SparkEnemyState state = RequireState(loaded.Enemies, actor);
        state.BaseFunctionTime = 0xffff;
        state.Function = SparkEnemyFunction.IntermittentActive;
        state.FunctionTimer = 1;
        StepCentered(loaded, room, assets, actor);
        if (state.Function != SparkEnemyFunction.IntermittentInactive ||
            state.FunctionTimer < 12 || state.FunctionTimer > 75)
        {
            throw new InvalidDataException(
                $"Spark randomized timer failed: function=$A8:{(ushort)state.Function:X4}, " +
                $"timer={state.FunctionTimer}, expected 12..75.");
        }
    }

    private static void VerifyRandomTableOverread(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        ushort seed = FindSeedWhoseNextRandomSelectsTableOverread();
        LoadedSparks loaded = Load(bus, room, assets, bossDefeated: true, randomSeed: seed);
        RoomEnemySlot emitter = loaded.Enemies.Slots[0];
        RequireState(loaded.Enemies, emitter).FunctionTimer = 1;
        StepCentered(loaded, room, assets, emitter);
        RoomEnemyProjectileSlot projectile = loaded.Enemies.EnemyProjectiles.Single(candidate =>
            candidate.Kind == RoomEnemyProjectileKind.FallingSpark);
        ushort expectedWhole = ReadWord(bus, 0x86f3f0);
        ushort expectedFraction = ReadWord(bus, 0x86f3f2);
        if (projectile.Variable1 != expectedWhole || projectile.Variable0 != expectedFraction)
        {
            throw new InvalidDataException(
                $"Falling Spark table overread failed: velocity=" +
                $"${projectile.Variable1:X4}.${projectile.Variable0:X4}, expected " +
                $"ROM $86:F3F0=${expectedWhole:X4}.${expectedFraction:X4}.");
        }
    }

    private static void VerifyAlwaysActiveRetailVariant(SuperMetroidAddressSpace bus)
    {
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(
            bus,
            MainShaftRoom,
            new RoomStateSelectionContext(default, BossBits: 1, false, false));
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        // Kzan was the final untranslated family in this state. Load the entire untouched
        // population now so the always-active Spark variant is proven in its actual mix of
        // Sbugs, Atomics, Kzan pairs, and the second Spark rather than behind a terminator.
        LoadedSparks loaded = Load(
            bus,
            room,
            assets,
            true,
            randomSeed: 0x5678);
        RoomEnemySlot actor = loaded.Enemies.Slots.Single(candidate =>
            candidate.EnemyDefinitionPointer == SparkDefinition &&
            candidate.XPosition == 0x0469 && candidate.YPosition == 0x071a);
        SparkEnemyState state = RequireState(loaded.Enemies, actor);
        if (actor.XPosition != 0x0469 || actor.YPosition != 0x071a ||
            actor.Parameter1 != 0 || actor.Parameter2 != 0 ||
            state.Function != SparkEnemyFunction.AlwaysActive ||
            state.FunctionTimer != 0 || actor.CurrentInstruction != 0xe5d1)
        {
            throw new InvalidDataException(
                $"Main Shaft always-active Spark failed: position=" +
                $"(${actor.XPosition:X4},${actor.YPosition:X4}), params=" +
                $"${actor.Parameter1:X4}/${actor.Parameter2:X4}, function=" +
                $"$A8:{(ushort)state.Function:X4}, timer={state.FunctionTimer}, " +
                $"list=$A8:{actor.CurrentInstruction:X4}.");
        }

        ushort x = actor.XPosition;
        ushort y = actor.YPosition;
        for (int frame = 0; frame < 32; frame++)
            StepCentered(loaded, room, assets, actor);
        if (actor.XPosition != x || actor.YPosition != y ||
            state.Function != SparkEnemyFunction.AlwaysActive)
        {
            throw new InvalidDataException("Always-active Spark moved or changed function.");
        }
    }

    private static void VerifyPreBossAbsoluteWramOr(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        byte oldLow = bus.ReadByte(0x7e0100);
        byte oldHigh = bus.ReadByte(0x7e0101);
        try
        {
            bus.WriteByte(0x7e0100, 0x55);
            bus.WriteByte(0x7e0101, 0x04);
            LoadedSparks loaded = Load(
                bus,
                room,
                assets,
                bossDefeated: false,
                randomSeed: 0x6789);
            ushort expected = unchecked((ushort)(0x2000 | 0x0455));
            if (loaded.Enemies.Slots[0].Properties != expected)
            {
                throw new InvalidDataException(
                    $"Spark absolute-WRAM OR failed: properties=" +
                    $"${loaded.Enemies.Slots[0].Properties:X4}, expected ${expected:X4}.");
            }
        }
        finally
        {
            bus.WriteByte(0x7e0100, oldLow);
            bus.WriteByte(0x7e0101, oldHigh);
        }
    }

    private static LoadedSparks Load(
        ISnesAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        bool bossDefeated,
        ushort randomSeed,
        ushort? populationPointer = null)
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusState.FacingRightNormalPose,
            XPosition = 0,
            YPosition = 0,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        var random = new Bank80SystemState(randomSeed);
        var enemies = new RoomEnemySystem();
        enemies.Load(
            bus,
            populationPointer ?? room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            random.NextRandom,
            random.SetRandomNumber,
            readRandomNumber: () => random.RandomNumber,
            level: assets.LevelData,
            samus: samus,
            isAreaBossDefeated: () => bossDefeated);
        return new LoadedSparks(enemies, samus);
    }

    private static void StepCentered(
        LoadedSparks loaded,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        RoomEnemySlot actor)
    {
        (ushort cameraX, ushort cameraY) = CenterCamera(room, actor);
        loaded.Enemies.StepFrame(
            cameraX,
            cameraY,
            timeIsFrozen: false,
            loaded.Samus,
            level: assets.LevelData);
    }

    private static (ushort X, ushort Y) CenterCamera(
        CartridgeRoomHeader room,
        RoomEnemySlot actor)
    {
        int maximumX = Math.Max(0, room.WidthInScreens * 256 - 256);
        int maximumY = Math.Max(0, room.HeightInScreens * 256 - 224);
        return (
            unchecked((ushort)Math.Clamp(actor.XPosition - 128, 0, maximumX)),
            unchecked((ushort)Math.Clamp(actor.YPosition - 112, 0, maximumY)));
    }

    private static SparkEnemyState RequireState(
        RoomEnemySystem enemies,
        RoomEnemySlot actor) =>
        enemies.SparkStates[actor.SlotIndex] ?? throw new InvalidDataException(
            $"Spark slot {actor.SlotIndex} has no typed state.");

    private static void ArmProjectile(SamusProjectileSlot projectile, RoomEnemySlot target)
    {
        projectile.ClearFields();
        projectile.Type = 0;
        projectile.Damage = 20;
        projectile.Direction = 0x0012;
        projectile.XPosition = target.XPosition;
        projectile.YPosition = target.YPosition;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    private static ushort FindSeedWhoseNextRandomSelectsTableOverread()
    {
        for (int seed = 0; seed <= ushort.MaxValue; seed++)
        {
            var random = new Bank80SystemState(unchecked((ushort)seed));
            if ((random.NextRandom() & 0x001c) == 0x001c)
                return unchecked((ushort)seed);
        }
        throw new InvalidDataException("No RNG seed selected falling-Spark table offset $1C.");
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private readonly record struct LoadedSparks(RoomEnemySystem Enemies, SamusState Samus);
    private readonly record struct AnimationResult(int MapCount);
    private readonly record struct ProjectileResult(int MapCount, int TrailMaps);
}
