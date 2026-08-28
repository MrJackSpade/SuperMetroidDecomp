using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>
/// ROM-backed regression for both versions of the Wrecked Ship Work Robot. The basement's
/// ordinary state supplies its complete pre-Phantoon population; the defeated-boss state is
/// now loaded through its untouched six-record population: two robots, three Atomics, and the
/// final Spark. Robot headers, code, lists, graphics, palette, projectiles, and collision data
/// remain cartridge sourced.
/// </summary>
internal static class WorkRobotAudit
{
    private const ushort BasementRoom = 0xcc6f;
    private const ushort PoweredDefinition = 0xe8ff;
    private const ushort NoPowerDefinition = 0xe93f;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader dormantRoom = CartridgeRoomHeader.Load(bus, BasementRoom);
        CartridgeRoomAssets dormantAssets = CartridgeRoomAssets.Load(bus, dormantRoom);
        LoadedRobots dormant = Load(bus, dormantRoom, dormantAssets, bossDefeated: false);
        VerifyDormantPopulation(dormantRoom, dormant);
        VerifyMovingSolidContact(dormant, dormantAssets);

        CartridgeRoomHeader poweredRoom = CartridgeRoomHeader.Load(
            bus,
            BasementRoom,
            new RoomStateSelectionContext(default, BossBits: 1, false, false));
        CartridgeRoomAssets poweredAssets = CartridgeRoomAssets.Load(bus, poweredRoom);
        LoadedRobots powered = Load(bus, poweredRoom, poweredAssets, bossDefeated: true);
        VerifyPoweredInitialization(poweredRoom, powered);
        NaturalResult natural = VerifyNaturalBehavior(powered, poweredRoom, poweredAssets);
        VerifyDrawing(powered, poweredRoom);
        VerifyShotRecoil(bus, poweredRoom, poweredAssets);
        VerifyPreBossShotGate(bus, poweredRoom, poweredAssets);

        Console.WriteLine(
            "Work Robot audit passed: unchanged Wrecked Ship states loaded dormant and " +
            $"powered robots; ROM animation produced {natural.AnimationMaps} maps, motion, " +
            $"both facings, {natural.LaserDirections} laser vectors, palette cycling, solid " +
            "contact, post-Phantoon shot recoil, and pre-Phantoon shot pass-through.");
        return 0;
    }

    private static void VerifyDormantPopulation(CartridgeRoomHeader room, LoadedRobots loaded)
    {
        RoomEnemySlot[] population = loaded.Enemies.Slots.Take(loaded.Enemies.EnemyCount).ToArray();
        RoomEnemySlot[] robots = population
            .Where(slot => slot.EnemyDefinitionPointer == NoPowerDefinition)
            .ToArray();
        if (room.State.Pointer != 0xcc81 || robots.Length != 2 ||
            population[^2] != robots[0] || population[^1] != robots[1])
        {
            throw new InvalidDataException(
                $"Dormant basement population failed: state=${room.State.Pointer:X4}, " +
                $"count={population.Length}, robots={robots.Length}.");
        }

        ushort[] expectedX = [0x004d, 0x0370];
        ushort[] expectedLists = [0xc6d9, 0xc6d3];
        for (int index = 0; index < robots.Length; index++)
        {
            RoomEnemySlot robot = robots[index];
            WorkRobotEnemyState state = RequireState(loaded.Enemies, robot);
            if (robot.XPosition != expectedX[index] || robot.YPosition != 0x00c0 ||
                robot.Health != 450 || robot.Definition.Damage != 80 ||
                robot.XRadius != 12 || robot.YRadius != 32 || robot.Layer != 5 ||
                robot.Definition.Bank != 0xa8 ||
                robot.Definition.InitializationAiPointer != 0xcbcc ||
                robot.Definition.MainAiPointer != 0xcc66 ||
                robot.Definition.TouchAiPointer != 0xd174 ||
                robot.Definition.ShotAiPointer != 0xd18d || state.Powered ||
                robot.CurrentInstruction != expectedLists[index] ||
                state.YSubvelocity != 0 || state.YVelocity != 1)
            {
                throw new InvalidDataException(
                    $"Dormant robot {index} initialization failed: position=" +
                    $"(${robot.XPosition:X4},${robot.YPosition:X4}), health/damage=" +
                    $"{robot.Health}/{robot.Definition.Damage}, list=${robot.CurrentInstruction:X4}, " +
                    $"powered={state.Powered}, fall={state.YVelocity:X4}:{state.YSubvelocity:X4}.");
            }
        }
    }

    private static void VerifyMovingSolidContact(LoadedRobots loaded, CartridgeRoomAssets assets)
    {
        RoomEnemySlot robot = loaded.Enemies.Slots
            .First(slot => slot.EnemyDefinitionPointer == NoPowerDefinition);
        Step(loaded, assets, robot);
        loaded.Samus.XPosition = unchecked((ushort)(robot.XPosition + 1));
        loaded.Samus.YPosition = robot.YPosition;
        ushort health = loaded.Samus.Health;
        if (!loaded.Enemies.ResolveOrdinarySamusContact(loaded.Samus, 0) ||
            loaded.Samus.Health != health ||
            loaded.Samus.Kinematics.ExtraXDisplacement != 4)
        {
            throw new InvalidDataException(
                $"Work Robot solid touch failed: health={health}->{loaded.Samus.Health}, " +
                $"extra X=${loaded.Samus.Kinematics.ExtraXDisplacement:X4}.");
        }
    }

    private static void VerifyPoweredInitialization(CartridgeRoomHeader room, LoadedRobots loaded)
    {
        RoomEnemySlot[] atomics = loaded.Enemies.Slots
            .Take(loaded.Enemies.EnemyCount)
            .Where(slot => slot.EnemyDefinitionPointer == 0xe9ff)
            .ToArray();
        RoomEnemySlot[] sparks = loaded.Enemies.Slots
            .Take(loaded.Enemies.EnemyCount)
            .Where(slot => slot.EnemyDefinitionPointer == 0xea3f)
            .ToArray();
        if (room.State.Pointer != 0xcc9b || loaded.Enemies.EnemyCount != 6 ||
            atomics.Length != 3 || atomics.Any(actor =>
                loaded.Enemies.AtomicStates[actor.SlotIndex] is null) ||
            sparks.Length != 1 || loaded.Enemies.SparkStates[sparks[0].SlotIndex] is null)
            throw new InvalidDataException(
                $"Powered basement population failed: state=${room.State.Pointer:X4}, " +
                $"count={loaded.Enemies.EnemyCount}, Atomics={atomics.Length}/" +
                $"{atomics.Count(actor => loaded.Enemies.AtomicStates[actor.SlotIndex] is not null)}, " +
                $"Sparks={sparks.Length}.");

        ushort[] expectedX = [0x004d, 0x0370];
        for (int index = 0; index < 2; index++)
        {
            RoomEnemySlot robot = loaded.Enemies.Slots[index];
            WorkRobotEnemyState state = RequireState(loaded.Enemies, robot);
            if (robot.EnemyDefinitionPointer != PoweredDefinition ||
                robot.XPosition != expectedX[index] || robot.YPosition != 0x00b0 ||
                robot.Health != 800 || robot.Definition.Damage != 80 ||
                robot.XRadius != 12 || robot.YRadius != 32 || robot.Layer != 5 ||
                robot.Definition.InitializationAiPointer != 0xcb77 ||
                robot.Definition.MainAiPointer != 0xcc36 ||
                robot.Definition.TouchAiPointer != 0xd174 ||
                robot.Definition.ShotAiPointer != 0xd192 || !state.Powered ||
                robot.CurrentInstruction != 0xc6e5 || robot.InstructionTimer != 4 ||
                state.LaserXVelocity != 0xfe00 || state.LaserCooldown != 0)
            {
                throw new InvalidDataException(
                    $"Powered robot {index} initialization failed: definition=" +
                    $"${robot.EnemyDefinitionPointer:X4}, position=" +
                    $"(${robot.XPosition:X4},${robot.YPosition:X4}), list/timer=" +
                    $"${robot.CurrentInstruction:X4}/{robot.InstructionTimer}, laser=" +
                    $"${state.LaserXVelocity:X4}/{state.LaserCooldown}, powered={state.Powered}.");
            }
        }
    }

    private static NaturalResult VerifyNaturalBehavior(
        LoadedRobots loaded,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        RoomEnemySlot robot = loaded.Enemies.Slots[0];
        WorkRobotEnemyState state = RequireState(loaded.Enemies, robot);
        ushort startingX = robot.XPosition;
        var maps = new HashSet<ushort>();
        var facings = new HashSet<short>();
        var vectors = new HashSet<(short X, short Y)>();
        bool sawFootstep = false;
        bool sawLaserSound = false;
        bool paletteChanged = false;
        int paletteBase = 128 + ((robot.PaletteIndex >> 9) & 7) * 16 + 9;
        ushort[] initialPalette = loaded.Cgram.Colors.Slice(paletteBase, 4).ToArray();

        loaded.Samus.XPosition = 0x0200;
        loaded.Samus.YPosition = 0x0040;
        for (int frame = 0; frame < 6000; frame++)
        {
            Step(loaded, assets, robot, room);
            loaded.Enemies.StepEnemyProjectiles(assets.LevelData, loaded.Samus);
            maps.Add(robot.SpritemapPointer);
            facings.Add(unchecked((short)state.LaserXVelocity));
            sawFootstep |= loaded.Enemies.LastWorkRobotSoundEffect == 0x0068;
            sawLaserSound |= loaded.Enemies.LastWorkRobotSoundEffect == 0x0067;
            paletteChanged |= !loaded.Cgram.Colors.Slice(paletteBase, 4).SequenceEqual(initialPalette);
            foreach (RoomEnemyProjectileSlot projectile in loaded.Enemies.EnemyProjectiles)
            {
                if (projectile.Kind is >= (RoomEnemyProjectileKind)0xd2a6 and
                    <= (RoomEnemyProjectileKind)0xd2de)
                {
                    vectors.Add((unchecked((short)projectile.XVelocity),
                        unchecked((short)projectile.YVelocity)));
                }
            }
        }

        bool moved = robot.XPosition != startingX;
        if (!moved || maps.Count < 8 || !facings.Contains(-0x0200) ||
            !facings.Contains(0x0200) || vectors.Count < 4 || !sawFootstep ||
            !sawLaserSound || !paletteChanged)
        {
            throw new InvalidDataException(
                $"Powered robot natural behavior failed: X=${startingX:X4}->" +
                $"${robot.XPosition:X4}, maps={maps.Count}, facings=" +
                $"{string.Join(',', facings)}, vectors={string.Join(',', vectors)}, " +
                $"sounds={sawFootstep}/{sawLaserSound}, palette={paletteChanged}.");
        }

        return new NaturalResult(maps.Count, vectors.Count);
    }

    private static void VerifyDrawing(LoadedRobots loaded, CartridgeRoomHeader room)
    {
        var oam = new OamBuffer();
        oam.BeginFrame();
        (ushort cameraX, ushort cameraY) = CenterCamera(room, loaded.Enemies.Slots[0]);
        loaded.Enemies.DrawLayers(oam, cameraX, cameraY, 0, 7);
        loaded.Enemies.DrawEnemyProjectiles(oam, cameraX, cameraY);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("Powered Work Robots emitted no ROM-authored OBJ.");
    }

    private static void VerifyShotRecoil(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedRobots loaded = Load(bus, room, assets, bossDefeated: true);
        RoomEnemySlot robot = loaded.Enemies.Slots[0];
        WorkRobotEnemyState state = RequireState(loaded.Enemies, robot);
        Step(loaded, assets, robot, room);
        loaded.Samus.XPosition = unchecked((ushort)(robot.XPosition - 32));
        var projectiles = new SamusProjectileSystem();
        ArmProjectile(projectiles.Slots[0], robot);
        int hits = loaded.Enemies.ResolveOrdinaryProjectileHits(
            bus,
            projectiles,
            new SamusBombProjectileSystem(),
            loaded.Samus);
        if (hits != 1 || robot.Health != 800 || robot.CurrentInstruction != 0xc7bb ||
            robot.InstructionTimer != 1 || state.LaserCooldown < 0x0040 ||
            projectiles.Slots[0].InstructionPointer == 0x9000)
        {
            throw new InvalidDataException(
                $"Powered Work Robot shot recoil failed: hits={hits}, health={robot.Health}, " +
                $"list=${robot.CurrentInstruction:X4}, timer={robot.InstructionTimer}, " +
                $"cooldown={state.LaserCooldown}, projectile=" +
                $"${projectiles.Slots[0].InstructionPointer:X4}.");
        }
    }

    private static void VerifyPreBossShotGate(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedRobots loaded = Load(bus, room, assets, bossDefeated: false);
        RoomEnemySlot robot = loaded.Enemies.Slots[0];
        if (RequireState(loaded.Enemies, robot).Powered)
            throw new InvalidDataException("Pre-Phantoon powered definition failed to deactivate.");
        Step(loaded, assets, robot, room);
        var projectiles = new SamusProjectileSystem();
        ArmProjectile(projectiles.Slots[0], robot);
        int hits = loaded.Enemies.ResolveOrdinaryProjectileHits(
            bus,
            projectiles,
            new SamusBombProjectileSystem(),
            loaded.Samus);
        if (hits != 0 || projectiles.Slots[0].InstructionPointer != 0x9000)
        {
            throw new InvalidDataException(
                $"Pre-Phantoon Work Robot shot gate failed: hits={hits}, projectile=" +
                $"${projectiles.Slots[0].InstructionPointer:X4}.");
        }
    }

    private static LoadedRobots Load(
        ISnesAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        bool bossDefeated)
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusState.FacingRightNormalPose,
            XPosition = 0x0200,
            YPosition = 0x0040,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        var random = new Bank80SystemState();
        random.SetRandomNumber(0x1234);
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
            isAreaBossDefeated: () => bossDefeated);
        return new LoadedRobots(enemies, samus, cgram);
    }

    private static void Step(
        LoadedRobots loaded,
        CartridgeRoomAssets assets,
        RoomEnemySlot target,
        CartridgeRoomHeader? room = null)
    {
        (ushort cameraX, ushort cameraY) = room is null
            ? ((ushort)0, (ushort)0)
            : CenterCamera(room, target);
        loaded.Enemies.StepFrame(
            cameraX,
            cameraY,
            false,
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

    private static void ArmProjectile(SamusProjectileSlot projectile, RoomEnemySlot target)
    {
        projectile.ClearFields();
        projectile.Type = 0;
        projectile.Damage = 20;
        projectile.Direction = 2;
        projectile.XPosition = target.XPosition;
        projectile.YPosition = target.YPosition;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    private static WorkRobotEnemyState RequireState(
        RoomEnemySystem enemies,
        RoomEnemySlot robot) =>
        enemies.WorkRobotStates[robot.SlotIndex] ?? throw new InvalidDataException(
            $"Work Robot slot {robot.SlotIndex} has no typed state.");

    private readonly record struct LoadedRobots(
        RoomEnemySystem Enemies,
        SamusState Samus,
        SnesCgram Cgram);

    private readonly record struct NaturalResult(int AnimationMaps, int LaserDirections);
}
