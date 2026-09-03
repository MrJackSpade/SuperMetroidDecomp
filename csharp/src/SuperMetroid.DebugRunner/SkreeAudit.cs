using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>
/// ROM-backed autonomous and combat regression for ordinary Skree definition $DB7F.
/// The awake Parlor state proves its authored mixed-population behavior; isolated combat
/// cases retain one unchanged Skree record and insert a terminator immediately afterward so
/// contact and weapon dispatch cannot accidentally select a neighboring Zoomer or Ripper.
/// </summary>
internal static class SkreeAudit
{
    private const ushort ParlorRoomPointer = 0x92fd;
    private const ushort AwakeParlorStatePointer = 0x932e;
    private const ushort SkreeDefinition = 0xdb7f;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var awakeEvents = new byte[] { 1 }; // Event zero selects normal state $8F:932E.
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(
            bus,
            ParlorRoomPointer,
            new RoomStateSelectionContext(awakeEvents, 0, false, false));
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);

        AutonomousResult autonomous = VerifyAutonomousLifecycle(bus, room, assets);
        ushort skreePopulation = FindFirstPopulationRecord(
            bus,
            room.State.EnemyPopulationPointer,
            SkreeDefinition);
        VerifyInitialization(bus, room, assets, skreePopulation);
        VerifyContactAttack(bus, room, assets, skreePopulation);
        VerifyNonlethalShotDamage(bus, room, assets, skreePopulation);
        VerifyLethalShotBurst(bus, room, assets, skreePopulation);
        VerifyPowerBombDeath(bus, room, assets, skreePopulation);

        Console.WriteLine(
            "Skree audit passed: awake Parlor loaded 11 Zoomers, three Skrees, and two " +
            $"Rippers; {autonomous.AnimationMaps} ROM maps, dive/burrow movement, sounds " +
            "$5B/$5C, floor impact, four-particle attack debris, and terminal burrowing " +
            "completed; contact dealt 10 damage, a beam dealt 10/15 health, a lethal " +
            "shot emitted the exact four-projectile burst, and a power bomb killed it " +
            "through the retail vulnerability path.");
        return 0;
    }

    private static AutonomousResult VerifyAutonomousLifecycle(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedSkreeRoom loaded = LoadWholePopulation(bus, room, assets);
        int zoomerCount = loaded.Enemies.Slots.Count(
            slot => slot.EnemyDefinitionPointer == 0xdcff);
        int skreeCount = loaded.Enemies.Slots.Count(
            slot => slot.EnemyDefinitionPointer == SkreeDefinition);
        int ripperCount = loaded.Enemies.Slots.Count(
            slot => slot.EnemyDefinitionPointer == 0xd47f);
        if (room.State.Pointer != AwakeParlorStatePointer || loaded.Enemies.EnemyCount != 16 ||
            zoomerCount != 11 || skreeCount != 3 || ripperCount != 2)
        {
            throw new InvalidDataException(
                $"Awake Parlor selected state ${room.State.Pointer:X4} with " +
                $"{loaded.Enemies.EnemyCount} actors: Zoomer={zoomerCount}, " +
                $"Skree={skreeCount}, Ripper={ripperCount}.");
        }

        RoomEnemySlot firstZoomerSlot = loaded.Enemies.Slots[0];
        ushort firstZoomerStartX = firstZoomerSlot.XPosition;
        var animationMaps = new HashSet<ushort>();
        bool sawPreparing = false;
        bool sawDive = false;
        bool sawBurrow = false;
        bool sawDiveSound = false;
        bool sawBurrowSound = false;
        bool sawParticles = false;
        bool sawTerminalDeletion = false;

        for (int frame = 0; frame < 180; frame++)
        {
            loaded.Enemies.StepFrame(
                cameraX: 0x0200,
                cameraY: 0,
                timeIsFrozen: false,
                loaded.Samus,
                level: assets.LevelData);
            loaded.Enemies.StepEnemyProjectiles(
                assets.LevelData,
                loaded.Samus,
                cameraX: 0x0200,
                cameraY: 0);

            foreach (RoomEnemySlot actor in loaded.Enemies.Slots.Where(
                slot => slot.EnemyDefinitionPointer == SkreeDefinition))
            {
                SkreeEnemyState state = State(loaded.Enemies, actor);
                sawPreparing |= state.Function == SkreeEnemyFunction.PreparingAttack;
                sawDive |= state.Function == SkreeEnemyFunction.Diving;
                sawBurrow |= state.Function == SkreeEnemyFunction.Burrowing;
                sawTerminalDeletion |= actor.Properties.HasAny(EnemyProperties.Deleted);
                if (actor.SpritemapPointer is not (0 or 0x804d))
                    animationMaps.Add(actor.SpritemapPointer);
            }

            sawDiveSound |= loaded.Enemies.LastSkreeSoundEffect == 0x005b;
            sawBurrowSound |= loaded.Enemies.LastSkreeSoundEffect == 0x005c;
            sawParticles |= loaded.Enemies.EnemyProjectiles.Any(IsSkreeParticle);
        }

        CrawlerEnemyState firstZoomer = loaded.Enemies.CrawlerStates[0]
            ?? throw new InvalidDataException("Awake Parlor slot zero has no crawler state.");
        if (firstZoomer.Function == CrawlerEnemyFunction.InstructionPending ||
            firstZoomerSlot.XPosition == firstZoomerStartX || animationMaps.Count < 3 ||
            !sawPreparing || !sawDive || !sawBurrow || !sawDiveSound ||
            !sawBurrowSound || !sawParticles || !sawTerminalDeletion)
        {
            throw new InvalidDataException(
                $"Awake Parlor lifecycle incomplete: Zoomer=$A3:" +
                $"{(ushort)firstZoomer.Function:X4} X=${firstZoomerStartX:X4}->" +
                $"${firstZoomerSlot.XPosition:X4}, maps={animationMaps.Count}, " +
                $"prepare/dive/burrow={sawPreparing}/{sawDive}/{sawBurrow}, " +
                $"sounds={sawDiveSound}/{sawBurrowSound}, particles/deleted=" +
                $"{sawParticles}/{sawTerminalDeletion}.");
        }

        return new AutonomousResult(animationMaps.Count);
    }

    private static void VerifyInitialization(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        ushort populationPointer)
    {
        LoadedSkreeRoom loaded = LoadSingle(bus, room, assets, populationPointer);
        RoomEnemySlot actor = loaded.Actor;
        SkreeEnemyState state = State(loaded.Enemies, actor);
        RoomEnemyDefinition definition = actor.Definition;
        if (loaded.Enemies.EnemyCount != 1 || actor.EnemyDefinitionPointer != SkreeDefinition ||
            actor.Properties != 0x2000 || actor.Parameter1 != 0 || actor.Parameter2 != 0 ||
            actor.CurrentInstruction != 0xc65e || actor.InstructionTimer != 1 ||
            state.Function != SkreeEnemyFunction.Idling || state.BurrowTimer != 0 ||
            state.RequestedInstructionIndex != 0 || state.InstalledInstructionIndex != 0 ||
            state.AttackReady || definition.Bank != 0xa3 ||
            definition.InitializationAiPointer != 0xc6ae ||
            definition.MainAiPointer != 0xc6c7 ||
            definition.TouchAiPointer != 0x8023 || definition.ShotAiPointer != 0xc7f5 ||
            definition.Health != 15 || definition.Damage != 10)
        {
            throw new InvalidDataException(
                $"Skree initialization mismatch: count={loaded.Enemies.EnemyCount}, " +
                $"definition=${actor.EnemyDefinitionPointer:X4}, properties=" +
                $"${actor.Properties:X4}, list=${actor.CurrentInstruction:X4}, " +
                $"function=$A3:{(ushort)state.Function:X4}, vars=" +
                $"{state.BurrowTimer}/{state.RequestedInstructionIndex}/" +
                $"{state.InstalledInstructionIndex}/{state.AttackReady}, header=" +
                $"${definition.Bank:X2}:{definition.InitializationAiPointer:X4}/" +
                $"${definition.MainAiPointer:X4}/${definition.TouchAiPointer:X4}/" +
                $"${definition.ShotAiPointer:X4}.");
        }
    }

    private static void VerifyContactAttack(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        ushort populationPointer)
    {
        LoadedSkreeRoom loaded = LoadSingle(bus, room, assets, populationPointer);
        StepOnActor(loaded, assets);
        loaded.Samus.XPosition = loaded.Actor.XPosition;
        loaded.Samus.YPosition = loaded.Actor.YPosition;
        ushort healthBefore = loaded.Samus.Health;
        bool contacted = loaded.Enemies.ResolveOrdinarySamusContact(
            loaded.Samus,
            controllerInput: 0,
            assets.LevelData);
        if (!contacted || loaded.Samus.Health != healthBefore - 10 ||
            !loaded.Samus.KnockbackActive || loaded.Samus.InvincibilityTimer != 0x0060)
        {
            throw new InvalidDataException(
                $"Skree contact mismatch: contact={contacted}, health=" +
                $"{healthBefore}->{loaded.Samus.Health}, knockback=" +
                $"{loaded.Samus.KnockbackActive}, invincibility=" +
                $"{loaded.Samus.InvincibilityTimer}.");
        }
    }

    private static void VerifyNonlethalShotDamage(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        ushort populationPointer)
    {
        LoadedSkreeRoom loaded = LoadSingle(bus, room, assets, populationPointer);
        StepOnActor(loaded, assets);
        var projectiles = new SamusProjectileSystem();
        var sharedProjectiles = new SamusBombProjectileSystem();
        ArmBeam(projectiles.Slots[0], loaded.Actor, damage: 10);
        int hits = loaded.Enemies.ResolveOrdinaryProjectileHits(
            bus,
            projectiles,
            sharedProjectiles,
            loaded.Samus);
        if (hits != 1 || loaded.Actor.Health != 5 || loaded.Actor.FlashTimer != 12 ||
            loaded.Actor.Properties.HasAny(EnemyProperties.Deleted) ||
            projectiles.Slots[0].PackedType.Family != SamusProjectileFamily.BeamExplosion ||
            loaded.Enemies.EnemyProjectiles.Any(IsSkreeParticle))
        {
            throw new InvalidDataException(
                $"Skree nonlethal beam mismatch: hits={hits}, health=" +
                $"{loaded.Actor.Health}, flash={loaded.Actor.FlashTimer}, deleted=" +
                $"{loaded.Actor.Properties.HasAny(EnemyProperties.Deleted)}, projectile=" +
                $"${projectiles.Slots[0].Type:X4}, particles=" +
                $"{loaded.Enemies.EnemyProjectiles.Count(IsSkreeParticle)}.");
        }
    }

    private static void VerifyLethalShotBurst(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        ushort populationPointer)
    {
        LoadedSkreeRoom loaded = LoadSingle(bus, room, assets, populationPointer);
        StepOnActor(loaded, assets);
        var projectiles = new SamusProjectileSystem();
        var sharedProjectiles = new SamusBombProjectileSystem();
        ushort sourceX = loaded.Actor.XPosition;
        ushort sourceY = loaded.Actor.YPosition;
        ushort graphicsIndex = unchecked((ushort)(
            loaded.Actor.VramTilesIndex | loaded.Actor.PaletteIndex));
        ArmBeam(projectiles.Slots[0], loaded.Actor, damage: 20);
        int hits = loaded.Enemies.ResolveOrdinaryProjectileHits(
            bus,
            projectiles,
            sharedProjectiles,
            loaded.Samus);

        RoomEnemyProjectileSlot[] particles = loaded.Enemies.EnemyProjectiles
            .Where(IsSkreeParticle)
            .ToArray();
        if (hits != 1 || loaded.Actor.Health != 0 ||
            !loaded.Actor.Properties.HasAny(EnemyProperties.Deleted) ||
            loaded.Enemies.EnemiesKilled != 1 || particles.Length != 4)
        {
            throw new InvalidDataException(
                $"Skree lethal beam mismatch: hits={hits}, health={loaded.Actor.Health}, " +
                $"deleted={loaded.Actor.Properties.HasAny(EnemyProperties.Deleted)}, " +
                $"kills={loaded.Enemies.EnemiesKilled}, particles={particles.Length}.");
        }

        AssertParticle(
            particles,
            RoomEnemyProjectileKind.SkreeParticleDownRight,
            unchecked((ushort)(sourceX + 6)), sourceY, 0x0140, 0xfcff, graphicsIndex);
        AssertParticle(
            particles,
            RoomEnemyProjectileKind.SkreeParticleUpRight,
            unchecked((ushort)(sourceX + 6)), sourceY, 0x0060, 0xfbff, graphicsIndex);
        AssertParticle(
            particles,
            RoomEnemyProjectileKind.SkreeParticleDownLeft,
            unchecked((ushort)(sourceX - 6)), sourceY, 0xfec0, 0xfcff, graphicsIndex);
        AssertParticle(
            particles,
            RoomEnemyProjectileKind.SkreeParticleUpLeft,
            unchecked((ushort)(sourceX - 6)), sourceY, 0xffa0, 0xfbff, graphicsIndex);

        // Bank-$86 owns the debris after the enemy slot is released. One projectile frame
        // must apply each signed 8.8 velocity and gravity instead of leaving decorative art.
        (RoomEnemyProjectileSlot Particle, ushort X, ushort Y)[] starts = particles
            .Select(particle => (particle, particle.XPosition, particle.YPosition))
            .ToArray();
        loaded.Enemies.StepEnemyProjectiles(
            assets.LevelData,
            loaded.Samus,
            cameraX: CameraX(loaded.Actor),
            cameraY: CameraY(loaded.Actor));
        if (starts.Any(start =>
            start.Particle.XPosition == start.X && start.Particle.YPosition == start.Y))
        {
            throw new InvalidDataException("One or more Skree death particles did not move.");
        }
    }

    private static void VerifyPowerBombDeath(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        ushort populationPointer)
    {
        LoadedSkreeRoom loaded = LoadSingle(bus, room, assets, populationPointer);
        int reactions = loaded.Enemies.ResolveOrdinaryPowerBombHits(
            bus,
            loaded.Actor.XPosition,
            loaded.Actor.YPosition,
            explosionRadius: 32,
            loaded.Samus);
        int particles = loaded.Enemies.EnemyProjectiles.Count(IsSkreeParticle);
        if (reactions != 1 || loaded.Actor.Health != 0 ||
            !loaded.Actor.Properties.HasAny(EnemyProperties.Deleted) ||
            loaded.Enemies.EnemiesKilled != 1 || particles != 0)
        {
            throw new InvalidDataException(
                $"Skree power-bomb mismatch: reactions={reactions}, health=" +
                $"{loaded.Actor.Health}, deleted=" +
                $"{loaded.Actor.Properties.HasAny(EnemyProperties.Deleted)}, " +
                $"kills={loaded.Enemies.EnemiesKilled}, particles={particles}.");
        }
    }

    private static LoadedSkreeRoom LoadWholePopulation(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedSkreeRoom loaded = Load(
            bus,
            room,
            assets,
            bus,
            room.State.EnemyPopulationPointer);
        RoomEnemySlot actor = loaded.Enemies.Slots.First(
            slot => slot.EnemyDefinitionPointer == SkreeDefinition);
        return loaded with { Actor = actor };
    }

    private static LoadedSkreeRoom LoadSingle(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        ushort populationPointer)
    {
        var singleBus = new PopulationPrefixAddressSpace(
            bus,
            populationPointer,
            retainedRecordCount: 1,
            deathQuota: 0);
        return Load(bus, room, assets, singleBus, populationPointer);
    }

    private static LoadedSkreeRoom Load(
        SuperMetroidAddressSpace cartridgeBus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        ISnesAddressSpace loadBus,
        ushort populationPointer)
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusPoseIds.FacingRightNormalPose,
            XPosition = 0x02be,
            YPosition = 0x0080,
        };
        samus.RefreshCollisionRadii(loadBus);
        samus.InitializeAnimation(loadBus);
        var random = new Bank80SystemState();
        var enemies = new RoomEnemySystem();
        enemies.Load(
            loadBus,
            populationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            random.NextRandom,
            random.SetRandomNumber,
            readRandomNumber: () => random.RandomNumber,
            level: assets.LevelData,
            samus: samus);
        RoomEnemySlot actor = enemies.Slots[0];
        return new LoadedSkreeRoom(enemies, samus, actor);
    }

    private static void StepOnActor(LoadedSkreeRoom loaded, CartridgeRoomAssets assets) =>
        loaded.Enemies.StepFrame(
            CameraX(loaded.Actor),
            CameraY(loaded.Actor),
            timeIsFrozen: false,
            loaded.Samus,
            level: assets.LevelData);

    private static ushort CameraX(RoomEnemySlot actor) =>
        unchecked((ushort)Math.Max(0, actor.XPosition - 128));

    private static ushort CameraY(RoomEnemySlot actor) =>
        unchecked((ushort)Math.Max(0, actor.YPosition - 112));

    private static ushort FindFirstPopulationRecord(
        ISnesAddressSpace bus,
        ushort populationPointer,
        ushort definitionPointer)
    {
        ushort cursor = populationPointer;
        for (int record = 0; record < RoomEnemySystem.MaximumEnemyCount; record++)
        {
            ushort candidate = ReadWord(bus, 0xa10000 | cursor);
            if (candidate == definitionPointer)
                return cursor;
            if (candidate == 0xffff)
                break;
            cursor = unchecked((ushort)(cursor + 16));
        }
        throw new InvalidDataException(
            $"Population $A1:{populationPointer:X4} contains no Skree.");
    }

    private static SkreeEnemyState State(RoomEnemySystem enemies, RoomEnemySlot actor) =>
        enemies.SkreeStates[actor.SlotIndex] ?? throw new InvalidDataException(
            $"Skree slot {actor.SlotIndex} has no typed state.");

    private static bool IsSkreeParticle(RoomEnemyProjectileSlot projectile) =>
        projectile.Kind is >= RoomEnemyProjectileKind.SkreeParticleDownRight and
            <= RoomEnemyProjectileKind.SkreeParticleUpLeft;

    private static void AssertParticle(
        IEnumerable<RoomEnemyProjectileSlot> particles,
        RoomEnemyProjectileKind kind,
        ushort x,
        ushort y,
        ushort xVelocity,
        ushort yVelocity,
        ushort graphicsIndex)
    {
        RoomEnemyProjectileSlot particle = particles.Single(candidate => candidate.Kind == kind);
        if (particle.XPosition != x || particle.YPosition != y ||
            particle.XVelocity != xVelocity || particle.YVelocity != yVelocity ||
            particle.InstructionPointer != 0x8abd || particle.InstructionTimer != 1 ||
            particle.PreInstruction != 0x8b5d || particle.GraphicsIndex != graphicsIndex ||
            particle.XRadius != 2 || particle.YRadius != 2)
        {
            throw new InvalidDataException(
                $"Skree particle {kind} mismatch: position=" +
                $"(${particle.XPosition:X4},${particle.YPosition:X4}), velocity=" +
                $"${particle.XVelocity:X4}/${particle.YVelocity:X4}, list/pre=" +
                $"${particle.InstructionPointer:X4}/${particle.PreInstruction:X4}, " +
                $"graphics=${particle.GraphicsIndex:X4}, radii=" +
                $"{particle.XRadius}/{particle.YRadius}.");
        }
    }

    private static void ArmBeam(
        SamusProjectileSlot projectile,
        RoomEnemySlot target,
        ushort damage)
    {
        projectile.ClearFields();
        projectile.Type = 0;
        projectile.Damage = damage;
        projectile.Direction = 2;
        projectile.XPosition = target.XPosition;
        projectile.YPosition = target.YPosition;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private sealed record LoadedSkreeRoom(
        RoomEnemySystem Enemies,
        SamusState Samus,
        RoomEnemySlot Actor);

    private readonly record struct AutonomousResult(int AnimationMaps);
}
