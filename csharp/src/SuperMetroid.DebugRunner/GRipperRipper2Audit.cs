using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>
/// ROM-backed audit for GRipper $D3FF and Ripper II $D43F. West Sand Hall supplies four
/// genuine GRippers with all shipped packed-speed selectors and patrol bounds; Red Ki
/// Hunter Shaft supplies a clean six-record Ripper II population. Nothing in either actor,
/// instruction list, speed table, collision map, palette, or graphics is synthesized.
/// </summary>
internal static class GRipperRipper2Audit
{
    private const ushort GRipperDefinition = 0xd3ff;
    private const ushort Ripper2Definition = 0xd43f;
    private const ushort GRipperRoomPointer = 0xab8f;
    private const ushort GRipperStatePointer = 0xab9c;
    private const ushort GRipperPopulationPointer = 0xb11d;
    private const ushort Ripper2RoomPointer = 0xb2da;
    private const ushort Ripper2StatePointer = 0xb2e7;
    private const ushort Ripper2PopulationPointer = 0xa48b;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        VerifyGRipper(bus);
        VerifyRipper2(bus);
        Console.WriteLine(
            "GRipper/Ripper II audit passed: four and six retail actors load, packed " +
            "speed selectors, patrol-bound and terrain reversals, three-frame ROM " +
            "animations, contact/missile/super damage, power-bomb immunity, both frozen " +
            "facing maps, death, and OBJ drawing agree.");
        return 0;
    }

    private static void VerifyGRipper(SuperMetroidAddressSpace bus)
    {
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, GRipperRoomPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        LoadedRoom loaded = LoadRoom(bus, room, assets);
        RoomEnemySlot[] actors = loaded.Enemies.Slots
            .Take(loaded.Enemies.EnemyCount)
            .Where(slot => slot.EnemyDefinitionPointer == GRipperDefinition)
            .ToArray();
        if (room.State.Pointer != GRipperStatePointer ||
            room.State.EnemyPopulationPointer != GRipperPopulationPointer ||
            loaded.Enemies.EnemyCount != 13 || actors.Length != 4)
        {
            throw new InvalidDataException(
                $"GRipper room mismatch: state/pop=${room.State.Pointer:X4}/" +
                $"${room.State.EnemyPopulationPointer:X4}, count/family=" +
                $"{loaded.Enemies.EnemyCount}/{actors.Length}.");
        }

        ushort[] expectedX = [0x00d1, 0x0198, 0x0228, 0x0360];
        ushort[] packedSelectors = [0x0010, 0x0120, 0x0020, 0x0110];
        ushort[] minimumX = [0x00b1, 0x0168, 0x0210, 0x0300];
        ushort[] maximumX = [0x0111, 0x01c8, 0x0268, 0x03c0];
        for (int index = 0; index < actors.Length; index++)
        {
            RoomEnemySlot actor = actors[index];
            RipperVariantEnemyState state = RequireState(loaded.Enemies, actor);
            ushort expectedOffset = unchecked((ushort)((packedSelectors[index] & 0xff) * 8));
            ushort expectedVelocity = ReadWord(bus, 0xa28187 + expectedOffset);
            ushort expectedSubvelocity = ReadWord(bus, 0xa28189 + expectedOffset);
            if (actor.XPosition != expectedX[index] || actor.Health != 200 ||
                actor.CurrentInstruction != 0xe1af ||
                state.SpeedTableByteOffset != expectedOffset ||
                state.XVelocity != expectedVelocity ||
                state.XSubvelocity != expectedSubvelocity ||
                unchecked((short)state.XVelocity) < 0 ||
                state.MinimumXPosition != minimumX[index] ||
                state.MaximumXPosition != maximumX[index])
            {
                throw new InvalidDataException(
                    $"GRipper {index} init mismatch: X=${actor.XPosition:X4}, list=" +
                    $"${actor.CurrentInstruction:X4}, speed=${state.XVelocity:X4}:" +
                    $"{state.XSubvelocity:X4}@${state.SpeedTableByteOffset:X4}, bounds=" +
                    $"${state.MinimumXPosition:X4}-${state.MaximumXPosition:X4}.");
            }
        }

        RoomEnemySlot patrol = actors[0];
        RipperVariantEnemyState patrolState = RequireState(loaded.Enemies, patrol);
        ushort startX = patrol.XPosition;
        bool movedRight = false;
        bool reversedLeft = false;
        var maps = new HashSet<ushort>();
        for (int frame = 0; frame < 512 && !reversedLeft; frame++)
        {
            StepCentered(loaded, assets, room, patrol);
            maps.Add(patrol.SpritemapPointer);
            movedRight |= unchecked((short)(patrol.XPosition - startX)) > 0;
            // CurrentInstruction is a live bytecode cursor, not a retained list origin;
            // velocity is the authoritative direction word after the reversal routine.
            reversedLeft = unchecked((short)patrolState.XVelocity) < 0;
        }
        if (!movedRight || !reversedLeft || maps.Count < 3 ||
            unchecked((short)(patrol.XPosition - patrolState.MaximumXPosition)) < 0)
        {
            throw new InvalidDataException(
                $"GRipper patrol mismatch: moved/reversed={movedRight}/{reversedLeft}, " +
                $"maps={maps.Count}, X/max=${patrol.XPosition:X4}/" +
                $"${patrolState.MaximumXPosition:X4}, velocity=${patrolState.XVelocity:X4}.");
        }

        VerifyContactDamage(loaded, assets, room, patrol, expectedDamage: 10);

        // GRipper's peculiar vulnerability record rejects all beam families but accepts a
        // normal missile with multiplier two. Common shot AI must therefore remove exactly
        // the supplied 100 damage before the private tail observes that it was not frozen.
        RoomEnemySlot missileTarget = actors[1];
        StepCentered(loaded, assets, room, missileTarget);
        var projectiles = new SamusProjectileSystem();
        var shared = new SamusBombProjectileSystem();
        ArmProjectile(projectiles.Slots[0], missileTarget, projectileType: 0x0100, damage: 100);
        if (loaded.Enemies.ResolveOrdinaryProjectileHits(
                bus,
                projectiles,
                shared,
                loaded.Samus) != 1 || missileTarget.Health != 100 ||
            missileTarget.FrozenTimer != 0)
        {
            throw new InvalidDataException(
                $"GRipper missile damage mismatch: health={missileTarget.Health}, " +
                $"frozen={missileTarget.FrozenTimer}.");
        }

        int powerBombReactions = loaded.Enemies.ResolveOrdinaryPowerBombHits(
            bus,
            actors[2].XPosition,
            actors[2].YPosition,
            explosionRadius: 64);
        if (powerBombReactions != 0 || actors[2].Health != 200)
        {
            throw new InvalidDataException(
                $"GRipper power-bomb immunity mismatch: reactions={powerBombReactions}, " +
                $"health={actors[2].Health}.");
        }

        VerifyDrawing(loaded, room, patrol, "GRipper");
    }

    private static void VerifyRipper2(SuperMetroidAddressSpace bus)
    {
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, Ripper2RoomPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        LoadedRoom loaded = LoadRoom(bus, room, assets);
        RoomEnemySlot[] actors = loaded.Enemies.Slots
            .Take(loaded.Enemies.EnemyCount)
            .ToArray();
        if (room.State.Pointer != Ripper2StatePointer ||
            room.State.EnemyPopulationPointer != Ripper2PopulationPointer ||
            actors.Length != 6 || actors.Any(actor =>
                actor.EnemyDefinitionPointer != Ripper2Definition))
        {
            throw new InvalidDataException(
                $"Ripper II room mismatch: state/pop=${room.State.Pointer:X4}/" +
                $"${room.State.EnemyPopulationPointer:X4}, count={actors.Length}.");
        }

        ushort negativeVelocity = ReadWord(bus, 0xa28187 + 0x0200 + 4);
        ushort negativeSubvelocity = ReadWord(bus, 0xa28187 + 0x0200 + 6);
        foreach (RoomEnemySlot actor in actors)
        {
            RipperVariantEnemyState state = RequireState(loaded.Enemies, actor);
            if (actor.Parameter1 != 0x0040 || actor.Parameter2 != 0 ||
                actor.Health != 200 || actor.CurrentInstruction != 0xe2e0 ||
                state.SpeedTableByteOffset != 0x0200 ||
                state.XVelocity != negativeVelocity ||
                state.XSubvelocity != negativeSubvelocity ||
                unchecked((short)state.XVelocity) >= 0)
            {
                throw new InvalidDataException(
                    $"Ripper II init mismatch: params=${actor.Parameter1:X4}/" +
                    $"${actor.Parameter2:X4}, list=${actor.CurrentInstruction:X4}, " +
                    $"speed=${state.XVelocity:X4}:{state.XSubvelocity:X4}@" +
                    $"${state.SpeedTableByteOffset:X4}.");
            }
        }

        RoomEnemySlot wallTarget = actors[0];
        RipperVariantEnemyState wallState = RequireState(loaded.Enemies, wallTarget);
        ushort startX = wallTarget.XPosition;
        bool movedLeft = false;
        bool reversedRight = false;
        var maps = new HashSet<ushort>();
        for (int frame = 0; frame < 1024 && !reversedRight; frame++)
        {
            StepCentered(loaded, assets, room, wallTarget);
            maps.Add(wallTarget.SpritemapPointer);
            movedLeft |= unchecked((short)(wallTarget.XPosition - startX)) < 0;
            reversedRight = unchecked((short)wallState.XVelocity) >= 0;
        }
        if (!movedLeft || !reversedRight || maps.Count < 3)
        {
            throw new InvalidDataException(
                $"Ripper II wall reversal mismatch: moved/reversed=" +
                $"{movedLeft}/{reversedRight}, maps={maps.Count}, X=" +
                $"${startX:X4}->${wallTarget.XPosition:X4}, speed=" +
                $"${wallState.XVelocity:X4}.");
        }

        VerifyContactDamage(loaded, assets, room, actors[1], expectedDamage: 10);
        VerifyRipper2FrozenFacing(bus, room, assets, expectPositiveVelocity: false);
        VerifyRipper2FrozenFacing(bus, room, assets, expectPositiveVelocity: true);

        // Super missiles use vulnerability multiplier two. A retail 300-damage projectile
        // must kill the 200-health actor through common AI before the private frozen-map
        // tail returns without changing the dead actor's zero freeze timer.
        loaded = LoadRoom(bus, room, assets);
        RoomEnemySlot lethal = loaded.Enemies.Slots[2];
        StepCentered(loaded, assets, room, lethal);
        var projectiles = new SamusProjectileSystem();
        var shared = new SamusBombProjectileSystem();
        ArmProjectile(projectiles.Slots[0], lethal, projectileType: 0x0200, damage: 300);
        if (loaded.Enemies.ResolveOrdinaryProjectileHits(
                bus,
                projectiles,
                shared,
                loaded.Samus) != 1 || lethal.Health != 0 ||
            !lethal.Properties.HasAny(EnemyProperties.Deleted))
        {
            throw new InvalidDataException(
                $"Ripper II super-missile death mismatch: health={lethal.Health}, " +
                $"properties=${lethal.Properties:X4}.");
        }

        VerifyDrawing(loaded, room, actors[0], "Ripper II");
    }

    private static void VerifyRipper2FrozenFacing(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        bool expectPositiveVelocity)
    {
        LoadedRoom loaded = LoadRoom(bus, room, assets);
        RoomEnemySlot actor = loaded.Enemies.Slots[0];
        RipperVariantEnemyState state = RequireState(loaded.Enemies, actor);
        if (expectPositiveVelocity)
        {
            for (int frame = 0; frame < 1024 && unchecked((short)state.XVelocity) < 0; frame++)
                StepCentered(loaded, assets, room, actor);
            if (unchecked((short)state.XVelocity) < 0)
                throw new InvalidDataException("Ripper II did not reverse before positive-facing freeze.");
        }
        else
        {
            StepCentered(loaded, assets, room, actor);
        }

        var projectiles = new SamusProjectileSystem();
        var shared = new SamusBombProjectileSystem();
        ArmProjectile(projectiles.Slots[0], actor, projectileType: 0x0002, damage: 20);
        ushort expectedMap = expectPositiveVelocity ? (ushort)0xe44b : (ushort)0xe43f;
        if (loaded.Enemies.ResolveOrdinaryProjectileHits(
                bus,
                projectiles,
                shared,
                loaded.Samus) != 1 || actor.FrozenTimer != 400 ||
            actor.SpritemapPointer != expectedMap || actor.Health != 200)
        {
            throw new InvalidDataException(
                $"Ripper II frozen-facing mismatch: positive={expectPositiveVelocity}, " +
                $"timer={actor.FrozenTimer}, map=${actor.SpritemapPointer:X4}/" +
                $"${expectedMap:X4}, health={actor.Health}.");
        }
    }

    private static void VerifyContactDamage(
        LoadedRoom loaded,
        CartridgeRoomAssets assets,
        CartridgeRoomHeader room,
        RoomEnemySlot actor,
        ushort expectedDamage)
    {
        StepCentered(loaded, assets, room, actor);
        loaded.Samus.XPosition = actor.XPosition;
        loaded.Samus.YPosition = actor.YPosition;
        loaded.Samus.Health = 999;
        loaded.Samus.InvincibilityTimer = 0;
        loaded.Samus.KnockbackActive = false;
        if (!loaded.Enemies.ResolveOrdinarySamusContact(loaded.Samus, 0) ||
            loaded.Samus.Health != 999 - expectedDamage ||
            !loaded.Samus.KnockbackActive)
        {
            throw new InvalidDataException(
                $"Enemy ${actor.EnemyDefinitionPointer:X4} contact mismatch: health=" +
                $"{loaded.Samus.Health}, knockback={loaded.Samus.KnockbackActive}.");
        }
    }

    private static LoadedRoom LoadRoom(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState();
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusState.FacingRightNormalPose,
            XPosition = 0x1000,
            YPosition = 0x1000,
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
        return new LoadedRoom(enemies, samus);
    }

    private static void StepCentered(
        LoadedRoom loaded,
        CartridgeRoomAssets assets,
        CartridgeRoomHeader room,
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

    private static void VerifyDrawing(
        LoadedRoom loaded,
        CartridgeRoomHeader room,
        RoomEnemySlot actor,
        string species)
    {
        var oam = new OamBuffer();
        oam.BeginFrame();
        (ushort cameraX, ushort cameraY) = CenterCamera(room, actor);
        loaded.Enemies.DrawLayers(oam, cameraX, cameraY, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException($"{species} emitted no live ROM OBJ pieces.");
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

    private static RipperVariantEnemyState RequireState(
        RoomEnemySystem enemies,
        RoomEnemySlot actor) =>
        enemies.RipperVariantStates[actor.SlotIndex] ?? throw new InvalidDataException(
            $"Enemy ${actor.EnemyDefinitionPointer:X4} slot {actor.SlotIndex} " +
            "did not receive typed Ripper-family state.");

    private static void ArmProjectile(
        SamusProjectileSlot projectile,
        RoomEnemySlot target,
        ushort projectileType,
        ushort damage)
    {
        projectile.ClearFields();
        projectile.Type = projectileType;
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

    private readonly record struct LoadedRoom(RoomEnemySystem Enemies, SamusState Samus);
}
