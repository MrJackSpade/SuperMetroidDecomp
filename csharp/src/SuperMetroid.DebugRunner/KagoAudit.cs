using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Untouched-ROM audit for Forgotten Highway's two Kagos and the independently scheduled
/// bank-$86 bugs emitted by their custom shot callback. This deliberately verifies the full
/// producer/consumer chain instead of directly fabricating a projectile slot.
/// </summary>
internal static class KagoAudit
{
    private const ushort RoomPointer = 0x9552;
    private const ushort StatePointer = 0x955f;
    private const ushort PopulationPointer = 0x8b3e;
    private const ushort TilesetPointer = 0x823d;
    private const ushort DefinitionPointer = 0xe7ff;
    private const ushort EmptySpritemap = 0x804d;

    private static readonly ushort[] KagoMaps = [0xabda, 0xabf0, 0xac06];

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, RoomPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        VerifyRetailHeader(bus);
        VerifyRetailProjectileDefinition(bus);

        RoomEnemySystem enemies = LoadRoom(bus, room, assets, out SamusState samus);
        RoomEnemySlot[] population = enemies.Slots.Take(enemies.EnemyCount).ToArray();
        if (room.State.Pointer != StatePointer ||
            room.State.EnemyPopulationPointer != PopulationPointer ||
            room.State.EnemyTilesetPointer != TilesetPointer ||
            room.WidthInScreens != 1 || room.HeightInScreens != 4 ||
            enemies.EnemyCount != 2 || enemies.DeathQuota != 2 ||
            population.Any(actor => actor.EnemyDefinitionPointer != DefinitionPointer) ||
            population.Any(actor => enemies.KagoStates[actor.SlotIndex] is null))
        {
            throw new InvalidDataException(
                $"Forgotten Highway Kago load mismatch: state=${room.State.Pointer:X4}, " +
                $"population=${room.State.EnemyPopulationPointer:X4}, set=" +
                $"${room.State.EnemyTilesetPointer:X4}, dimensions=" +
                $"{room.WidthInScreens}x{room.HeightInScreens}, count/quota=" +
                $"{enemies.EnemyCount}/{enemies.DeathQuota}.");
        }

        VerifyPopulationInitialization(enemies, population);
        VerifyShellAnimationAndNaturalBugCycle(bus, room, assets);
        VerifyBugContactDamage(bus, room, assets);
        VerifyElevenHitDeath(bus, room, assets);

        Console.WriteLine(
            "Kago audit passed: untouched Forgotten Highway loaded both shells; slow/fast " +
            "ROM animation, earthquake and eleven-hit death counter, natural bug idle sound, " +
            "source-distance shot activation, randomized jump, wall/floor/gravity movement, " +
            "20-damage contact, five-map shot death, palette change, and exact $E7FF drop " +
            "request were verified.");
        return 0;
    }

    private static void VerifyPopulationInitialization(
        RoomEnemySystem enemies,
        RoomEnemySlot[] population)
    {
        ushort[] expectedX = [0x0050, 0x00a8];
        ushort[] expectedY = [0x0340, 0x0200];
        for (int index = 0; index < population.Length; index++)
        {
            RoomEnemySlot actor = population[index];
            KagoEnemyState state = State(enemies, actor);
            if (actor.XPosition != expectedX[index] || actor.YPosition != expectedY[index] ||
                actor.Parameter1 != 0x000a || actor.Parameter2 != 0 ||
                actor.Properties != 0xa000 || actor.Health != 1600 ||
                actor.CurrentInstruction != 0xab1e ||
                actor.SpritemapPointer != EmptySpritemap ||
                state.Function != KagoEnemyFunction.InstallNoOp ||
                state.UsesFastAnimation || state.HitCounter != 10 ||
                state.DeathAnimationStarted || state.SpawnedBugCount != 0)
            {
                throw new InvalidDataException(
                    $"Forgotten Highway Kago {index} initialization mismatch: position=" +
                    $"(${actor.XPosition:X4},${actor.YPosition:X4}), params=" +
                    $"${actor.Parameter1:X4}/${actor.Parameter2:X4}, properties=" +
                    $"${actor.Properties:X4}, health={actor.Health}, list=" +
                    $"$A8:{actor.CurrentInstruction:X4}, function={state.Function}, " +
                    $"fast={state.UsesFastAnimation}, counter={state.HitCounter}.");
            }
        }
    }

    private static void VerifyShellAnimationAndNaturalBugCycle(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        RoomEnemySystem enemies = LoadRoom(bus, room, assets, out SamusState samus);
        RoomEnemySlot actor = enemies.Slots[0];
        KagoEnemyState state = State(enemies, actor);
        Isolate(enemies, actor);
        (ushort cameraX, ushort cameraY) = CenterCamera(room, actor);

        var slowMaps = new HashSet<ushort>();
        for (int frame = 0; frame < 41; frame++)
        {
            enemies.StepFrame(cameraX, cameraY, false, samus, level: assets.LevelData);
            slowMaps.Add(actor.SpritemapPointer);
        }
        if (state.Function != KagoEnemyFunction.NoOp || !KagoMaps.All(slowMaps.Contains))
        {
            throw new InvalidDataException(
                $"Kago slow loop mismatch: function={state.Function}, maps=" +
                $"{string.Join(',', slowMaps.Select(value => $"${value:X4}"))}.");
        }

        var shots = new SamusProjectileSystem();
        var bombs = new SamusBombProjectileSystem();
        ArmBeam(shots.Slots[0], actor.XPosition, actor.YPosition, type: 0x0001);
        if (enemies.ResolveOrdinaryProjectileHits(bus, shots, bombs, samus) != 1)
            throw new InvalidDataException("Kago shell did not accept its custom shot callback.");

        RoomEnemyProjectileSlot bug = enemies.EnemyProjectiles.Single(projectile =>
            projectile.Kind == RoomEnemyProjectileKind.KagoBug);
        KagoBugProjectileState initialBug = enemies.InspectKagoBug(bug);
        if (!state.UsesFastAnimation || state.HitCounter != 9 ||
            state.SpawnedBugCount != 1 || actor.CurrentInstruction != 0xab32 ||
            actor.InstructionTimer != 1 || actor.Health != 1600 ||
            enemies.EarthquakeType != 2 || enemies.EarthquakeTimer != 16 ||
            bug.XPosition != actor.XPosition || bug.YPosition != actor.YPosition ||
            bug.XSubposition != 0 || bug.YSubposition != 0 ||
            bug.InstructionPointer != 0x9c7d || bug.PreInstruction != 0xd0ca ||
            bug.Damage != 20 || bug.XRadius != 4 || bug.YRadius != 4 ||
            !bug.CanDamageSamus || bug.PersistsOnSamusContact ||
            bug.BlocksSamusProjectiles || bug.CollisionOption != 0 ||
            initialBug.SourceEnemyNativeIndex != actor.NativeIndex ||
            initialBug.IdleTimer != 2 || initialBug.SoundTimer != 6)
        {
            throw new InvalidDataException(
                $"Kago first-shot/bug initialization mismatch: fast={state.UsesFastAnimation}, " +
                $"counter/spawns={state.HitCounter}/{state.SpawnedBugCount}, list=" +
                $"$A8:{actor.CurrentInstruction:X4}, health={actor.Health}, quake=" +
                $"{enemies.EarthquakeType}/{enemies.EarthquakeTimer}, bug position=" +
                $"(${bug.XPosition:X4},${bug.YPosition:X4}), bug list/pre=" +
                $"${bug.InstructionPointer:X4}/${bug.PreInstruction:X4}, damage/radii=" +
                $"{bug.Damage}/{bug.XRadius}/{bug.YRadius}, idle/sound=" +
                $"{initialBug.IdleTimer}/{initialBug.SoundTimer}.");
        }

        var fastMaps = new HashSet<ushort>();
        var bugMaps = new HashSet<ushort>();
        var bugPositions = new HashSet<(ushort X, ushort Y)>();
        bool sawSound = false;
        bool sawJump = false;
        bool sawFall = false;
        bool sawLandedLoop = false;
        bool enabledShotCollision = false;
        for (int frame = 0; frame < 256 && !sawLandedLoop; frame++)
        {
            enemies.StepFrame(cameraX, cameraY, false, samus, level: assets.LevelData);
            fastMaps.Add(actor.SpritemapPointer);
            enemies.StepEnemyProjectiles(
                assets.LevelData,
                samus: null,
                cameraX: cameraX,
                cameraY: cameraY);
            if (!bug.IsActive)
                throw new InvalidDataException("Natural Kago bug disappeared before landing.");
            bugMaps.Add(bug.SpritemapPointer);
            bugPositions.Add((bug.XPosition, bug.YPosition));
            sawSound |= enemies.LastKagoBugSoundEffect == 0x006c;
            sawJump |= bug.PreInstruction == 0xd0ec;
            sawFall |= bug.PreInstruction == 0xd128;
            enabledShotCollision |= bug.BlocksSamusProjectiles;
            sawLandedLoop = sawFall && bug.InstructionPointer is >= 0xd040 and <= 0xd04a;
        }

        if (!KagoMaps.All(fastMaps.Contains) || bugMaps.All(map => map is 0 or 0x8000) ||
            bugPositions.Count < 8 || !sawSound || !sawJump || !sawFall ||
            !sawLandedLoop || !enabledShotCollision)
        {
            throw new InvalidDataException(
                $"Kago natural bug cycle mismatch: shell maps=" +
                $"{string.Join(',', fastMaps.Select(value => $"${value:X4}"))}, bug maps=" +
                $"{string.Join(',', bugMaps.Select(value => $"${value:X4}"))}, positions=" +
                $"{bugPositions.Count}, sound/jump/fall/land/shot=" +
                $"{sawSound}/{sawJump}/{sawFall}/{sawLandedLoop}/{enabledShotCollision}.");
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        enemies.DrawLayers(oam, cameraX, cameraY, 0, 7);
        enemies.DrawEnemyProjectiles(oam, cameraX, cameraY);
        oam.FinalizeFrame();
        // The landed frame's retail maps contain four shell pieces plus one bug piece.
        // Header part-count nine describes enemy allocation, not an OAM-piece minimum.
        if (oam.LastFinalizedSpriteCount != 5)
            throw new InvalidDataException($"Kago composite emitted {oam.LastFinalizedSpriteCount}, not five, OBJ pieces.");

        // Place a live non-plasma beam in the same native 32-pixel cell. Flag zero must
        // switch to the projectile definition's shot list rather than producing Waffle's dud.
        ArmBeam(shots.Slots[0], bug.XPosition, bug.YPosition, type: 0x0001);
        if (enemies.ResolveEnemyProjectileSamusProjectileHits(bus, shots, bombs) != 1 ||
            bug.PreInstruction != 0x84fb || bug.InstructionPointer != 0xd064 ||
            bug.InstructionTimer != 1 || bug.BlocksSamusProjectiles ||
            bug.CollidedProjectileType != 0x0001 ||
            enemies.LastEnemyProjectileDudSoundEffect is not null)
        {
            throw new InvalidDataException(
                $"Kago bug shot dispatch mismatch: pre/list/timer=" +
                $"${bug.PreInstruction:X4}/${bug.InstructionPointer:X4}/{bug.InstructionTimer}, " +
                $"blocks={bug.BlocksSamusProjectiles}, collided=" +
                $"${bug.CollidedProjectileType:X4}, dud=" +
                $"{enemies.LastEnemyProjectileDudSoundEffect?.ToString("X4") ?? "none"}.");
        }

        var shotMaps = new HashSet<ushort>();
        for (int frame = 0; frame < 64 && bug.IsActive; frame++)
        {
            enemies.StepEnemyProjectiles(
                assets.LevelData,
                samus: null,
                cameraX: cameraX,
                cameraY: cameraY);
            if (bug.IsActive)
                shotMaps.Add(bug.SpritemapPointer);
        }
        KagoBugDropRequest? drop = enemies.LastKagoBugDropRequest;
        if (bug.IsActive || shotMaps.Count != 5 || drop is null ||
            drop.Value.EnemyDefinitionPointer != DefinitionPointer ||
            drop.Value.EnemyProjectileNativeIndex != checked((ushort)(bug.SlotIndex * 2)) ||
            drop.Value.X == 0 && drop.Value.Y == 0)
        {
            throw new InvalidDataException(
                $"Kago bug shot death mismatch: live={bug.IsActive}, maps={shotMaps.Count}, " +
                $"drop={(drop is null ? "none" : $"${drop.Value.EnemyDefinitionPointer:X4} " +
                    $"at (${drop.Value.X:X4},${drop.Value.Y:X4})")}.");
        }
    }

    private static void VerifyBugContactDamage(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        RoomEnemySystem enemies = LoadRoom(bus, room, assets, out SamusState samus);
        RoomEnemySlot actor = enemies.Slots[0];
        Isolate(enemies, actor);
        (ushort cameraX, ushort cameraY) = CenterCamera(room, actor);
        enemies.StepFrame(cameraX, cameraY, false, samus, level: assets.LevelData);

        var shots = new SamusProjectileSystem();
        var bombs = new SamusBombProjectileSystem();
        ArmBeam(shots.Slots[0], actor.XPosition, actor.YPosition, type: 0x0001);
        _ = enemies.ResolveOrdinaryProjectileHits(bus, shots, bombs, samus);
        RoomEnemyProjectileSlot bug = enemies.EnemyProjectiles.Single(projectile =>
            projectile.Kind == RoomEnemyProjectileKind.KagoBug);

        samus.XPosition = bug.XPosition;
        samus.YPosition = bug.YPosition;
        samus.Health = 999;
        samus.InvincibilityTimer = 0;
        enemies.StepEnemyProjectiles(
            assets.LevelData,
            samus,
            cameraX: cameraX,
            cameraY: cameraY);
        if (samus.Health != 979 || !samus.KnockbackActive || bug.IsActive)
        {
            throw new InvalidDataException(
                $"Kago bug contact mismatch: health={samus.Health}, " +
                $"knockback={samus.KnockbackActive}, live={bug.IsActive}.");
        }
    }

    private static void VerifyElevenHitDeath(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        RoomEnemySystem enemies = LoadRoom(bus, room, assets, out SamusState samus);
        RoomEnemySlot actor = enemies.Slots[0];
        KagoEnemyState state = State(enemies, actor);
        Isolate(enemies, actor);
        (ushort cameraX, ushort cameraY) = CenterCamera(room, actor);
        enemies.StepFrame(cameraX, cameraY, false, samus, level: assets.LevelData);

        var shots = new SamusProjectileSystem();
        var bombs = new SamusBombProjectileSystem();
        for (int hit = 1; hit <= 11; hit++)
        {
            ArmBeam(shots.Slots[0], actor.XPosition, actor.YPosition, type: 0x0000);
            if (enemies.ResolveOrdinaryProjectileHits(bus, shots, bombs, samus) != 1)
                throw new InvalidDataException($"Kago rejected accepted shell hit {hit}.");

            ushort expectedCounter = unchecked((ushort)(10 - hit));
            bool expectedDeath = hit == 11;
            if (state.HitCounter != expectedCounter ||
                state.DeathAnimationStarted != expectedDeath ||
                actor.Properties.HasAny(EnemyProperties.Deleted) != expectedDeath ||
                state.SpawnedBugCount != hit || enemies.EarthquakeType != 2 ||
                enemies.EarthquakeTimer != 16)
            {
                throw new InvalidDataException(
                    $"Kago hit {hit} mismatch: counter=${state.HitCounter:X4}, " +
                    $"death={state.DeathAnimationStarted}, properties=" +
                    $"${actor.Properties:X4}, bugs={state.SpawnedBugCount}, quake=" +
                    $"{enemies.EarthquakeType}/{enemies.EarthquakeTimer}.");
            }

            // Native Kago can fill the shared eighteen-slot pool, but this counter audit is
            // about all eleven callbacks. Consume each naturally spawned 20-damage bug via
            // the common Samus-contact pass so the next hit has a physical free slot.
            RoomEnemyProjectileSlot bug = enemies.EnemyProjectiles.Single(projectile =>
                projectile.Kind == RoomEnemyProjectileKind.KagoBug);
            samus.XPosition = bug.XPosition;
            samus.YPosition = bug.YPosition;
            samus.InvincibilityTimer = 0;
            enemies.StepEnemyProjectiles(
                assets.LevelData,
                samus,
                cameraX: cameraX,
                cameraY: cameraY);

            // This isolated audit consumes eleven contacts without advancing Samus's own
            // five-frame special-movement dispatcher. Restore the exact neutral test pose
            // between contacts so the next native knockback request is not falsely pending.
            samus.KnockbackActive = false;
            samus.KnockbackDirection = 0;
            samus.KnockbackTimer = 0;
            samus.Pose = SamusState.FacingRightNormalPose;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
        }

        if (state.HitCounter != 0xffff || !state.DeathAnimationStarted ||
            enemies.EnemiesKilled != 1 || !actor.Properties.HasAny(EnemyProperties.Deleted))
        {
            throw new InvalidDataException(
                $"Kago did not die on the eleventh hit: counter=${state.HitCounter:X4}, " +
                $"death={state.DeathAnimationStarted}, killed={enemies.EnemiesKilled}, " +
                $"properties=${actor.Properties:X4}.");
        }
    }

    private static void VerifyRetailHeader(ISnesAddressSpace bus)
    {
        RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(bus, DefinitionPointer);
        if (definition.TileDataSize != 0x0800 || definition.PalettePointer != 0xaafe ||
            definition.Health != 1600 || definition.Damage != 0 ||
            definition.XRadius != 16 || definition.YRadius != 16 ||
            definition.Bank != 0xa8 || definition.InitializationAiPointer != 0xab46 ||
            definition.PartCount != 9 || definition.MainAiPointer != 0xab75 ||
            definition.GrappleAiPointer != 0x800f || definition.HurtAiPointer != 0x804c ||
            definition.FrozenAiPointer != 0x8041 || definition.DeathAnimation != 4 ||
            definition.TouchAiPointer != 0x8023 || definition.ShotAiPointer != 0xab83 ||
            definition.Layer != 5 || definition.VulnerabilityPointer != 0xf026)
        {
            throw new InvalidDataException("Retail Kago header does not match $A0:E7FF.");
        }
    }

    private static void VerifyRetailProjectileDefinition(ISnesAddressSpace bus)
    {
        const int definition = 0x86d02e;
        if (ReadWord(bus, definition) != 0xd088 ||
            ReadWord(bus, definition + 2) != 0xd0eb ||
            ReadWord(bus, definition + 4) != 0x9c7d ||
            ReadWord(bus, definition + 6) != 0x0404 ||
            ReadWord(bus, definition + 8) != 0x0014 ||
            ReadWord(bus, definition + 10) != 0 ||
            ReadWord(bus, definition + 12) != 0xd064)
        {
            throw new InvalidDataException(
                "Enemy projectile definition $86:D02E differs from retail data.");
        }
    }

    private static RoomEnemySystem LoadRoom(
        ISnesAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        out SamusState samus)
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState();
        samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
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
            samus: samus);
        return enemies;
    }

    private static void Isolate(RoomEnemySystem enemies, RoomEnemySlot retained)
    {
        foreach (RoomEnemySlot actor in enemies.Slots.Take(enemies.EnemyCount))
        {
            if (!ReferenceEquals(actor, retained))
                actor.Properties = actor.Properties.With(EnemyProperties.Deleted);
        }
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

    private static void ArmBeam(
        SamusProjectileSlot projectile,
        ushort x,
        ushort y,
        ushort type)
    {
        projectile.ClearFields();
        projectile.Type = type;
        projectile.Damage = 20;
        projectile.Direction = (ushort)SamusProjectileDirection.Right;
        projectile.XPosition = x;
        projectile.YPosition = y;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    private static KagoEnemyState State(RoomEnemySystem enemies, RoomEnemySlot actor) =>
        enemies.KagoStates[actor.SlotIndex] ?? throw new InvalidDataException(
            $"Kago slot {actor.SlotIndex} has no typed state.");

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}
