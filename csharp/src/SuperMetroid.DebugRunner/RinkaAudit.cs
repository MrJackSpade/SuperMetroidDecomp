using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Untouched-ROM audit for Tourian's ordinary Rinkas. It selects the event-$10 state of
/// room $8F:DAE1 so the cartridge supplies a four-Rinka-only population, then proves load,
/// instruction animation, homing flight, viewport recycling, contact, shot death, generic
/// death-projectile respawn, and the private power-bomb callback.
/// </summary>
internal static class RinkaAudit
{
    private const ushort DefinitionPointer = 0xd23f;
    private const ushort RoomPointer = 0xdae1;
    private const ushort ExpectedStatePointer = 0xdb0d;
    private const ushort ExpectedPopulationPointer = 0xe516;
    private const ushort ExpectedTilesetPointer = 0x916c;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        VerifyHeader(bus);

        // Selector $8F:E612 tests event index $10. Byte index two, bit zero is therefore
        // the smallest literal context that chooses state $DB0D over default state $DAF3.
        byte[] events = new byte[3];
        events[2] = 1;
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(
            bus,
            RoomPointer,
            new RoomStateSelectionContext(
                events,
                BossBits: 0,
                HasMorphBallAndMissiles: false,
                HasPowerBombs: false));
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        if (room.State.Pointer != ExpectedStatePointer ||
            room.State.EnemyPopulationPointer != ExpectedPopulationPointer ||
            room.State.EnemyTilesetPointer != ExpectedTilesetPointer ||
            room.WidthInScreens != 6 || room.HeightInScreens != 1)
        {
            throw new InvalidDataException(
                $"Rinka room selection mismatch: state=${room.State.Pointer:X4}, " +
                $"population/set=${room.State.EnemyPopulationPointer:X4}/" +
                $"${room.State.EnemyTilesetPointer:X4}, dimensions=" +
                $"{room.WidthInScreens}x{room.HeightInScreens}.");
        }

        VerifyAnimationFlightAndContact(bus, room, assets);
        VerifyShotDeathAndRespawn(bus, room, assets);
        VerifyNormalBombDeath(bus, room, assets);
        VerifyPowerBombDeath(bus, room, assets);
        Console.WriteLine(
            "Rinka untouched-room audit passed: four retail actors loaded, animated, " +
            "aimed, flew, recycled, damaged Samus, accepted beam/normal-bomb/power-bomb " +
            "damage, " +
            "ran variant-zero death frames, and respawned from their population snapshot.");
        return 0;
    }

    private static void VerifyAnimationFlightAndContact(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        (RoomEnemySystem enemies, SamusState samus) = LoadRoom(bus, room, assets);
        RoomEnemySlot[] rinkas = GetRinkas(enemies);
        if (enemies.EnemyCount != 4 || rinkas.Length != 4 ||
            rinkas.Any(slot => enemies.RinkaStates[slot.SlotIndex] is null) ||
            rinkas.Any(slot => slot.PaletteIndex != 0x0400 ||
                slot.CurrentInstruction != 0xb9e0 ||
                slot.Health != 10))
        {
            throw new InvalidDataException("Rinka population did not initialize from $A1:E516.");
        }

        RoomEnemySlot target = rinkas[0];
        ushort spawnX = target.XPosition;
        ushort spawnY = target.YPosition;
        var maps = new HashSet<ushort>();
        bool sawVisibleAim = false;
        bool sawFlight = false;
        bool sawRecycledHiddenState = false;
        for (int frame = 0; frame < 520; frame++)
        {
            enemies.StepFrame(
                cameraX: 0,
                cameraY: 0,
                timeIsFrozen: false,
                samus,
                level: assets.LevelData);
            RinkaEnemyState state = enemies.RinkaStates[target.SlotIndex]
                ?? throw new InvalidDataException("Live Rinka lost its typed state.");
            maps.Add(target.SpritemapPointer);
            sawVisibleAim |= state.Function == RinkaEnemyFunction.AimDelay &&
                !target.Properties.HasAny(EnemyProperties.Invisible);
            sawFlight |= state.Function == RinkaEnemyFunction.Flying &&
                (state.XVelocity != 0 || state.YVelocity != 0);
            sawRecycledHiddenState |= sawFlight &&
                state.Function == RinkaEnemyFunction.WatchForLeavingViewport &&
                target.XPosition == spawnX && target.YPosition == spawnY &&
                target.Properties.HasAny(EnemyProperties.Invisible);
            if (sawVisibleAim && sawFlight && sawRecycledHiddenState)
                break;
        }

        if (!sawVisibleAim || !sawFlight || !sawRecycledHiddenState ||
            !maps.Contains(0xba38) || maps.All(pointer => pointer is 0 or 0x804d))
        {
            throw new InvalidDataException(
                $"Rinka cycle mismatch: aim/flight/recycle=" +
                $"{sawVisibleAim}/{sawFlight}/{sawRecycledHiddenState}, maps={maps.Count}.");
        }

        // Rebuild the interactive list on a freshly visible target, then place Samus on the
        // actor after AI. A normal contact must subtract the header's forty damage and start
        // knockback while leaving Rinka health untouched.
        (enemies, samus) = LoadRoom(bus, room, assets);
        target = GetRinkas(enemies)[0];
        StepUntilInteractive(enemies, samus, assets.LevelData, target);
        samus.Health = 999;
        samus.InvincibilityTimer = 0;
        samus.KnockbackActive = false;
        samus.XPosition = target.XPosition;
        samus.YPosition = target.YPosition;
        if (!enemies.ResolveOrdinarySamusContact(samus, controllerInput: 0) ||
            samus.Health != 959 || !samus.KnockbackActive || target.Health != 10)
        {
            throw new InvalidDataException(
                $"Rinka contact mismatch: Samus health={samus.Health}, " +
                $"knockback={samus.KnockbackActive}, Rinka health={target.Health}.");
        }
    }

    private static void VerifyShotDeathAndRespawn(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        (RoomEnemySystem enemies, SamusState samus) = LoadRoom(bus, room, assets);
        RoomEnemySlot target = GetRinkas(enemies)[0];
        StepUntilInteractive(enemies, samus, assets.LevelData, target);

        var shots = new SamusProjectileSystem();
        var bombs = new SamusBombProjectileSystem();
        ArmPowerBeam(shots.Slots[0], target);
        if (enemies.ResolveOrdinaryProjectileHits(bus, shots, bombs, samus) != 1 ||
            target.EnemyDefinitionPointer != 0xdaff || target.Health != 0 ||
            enemies.EnemiesKilled != 0 ||
            enemies.EnemyProjectiles.Count(projectile =>
                projectile.Kind == RoomEnemyProjectileKind.EnemyDeathExplosion) != 1)
        {
            throw new InvalidDataException(
                $"Rinka beam death mismatch: ptr=${target.EnemyDefinitionPointer:X4}, " +
                $"health={target.Health}, kills={enemies.EnemiesKilled}, projectiles=" +
                $"{enemies.ActiveEnemyProjectileCount}.");
        }

        // Variant zero's six visible frame durations total 31 ticks. The translated drop
        // seam then takes the cartridge's $ECA3 no-pickup tail for 64 ticks before $EF10
        // reconstructs the exact physical slot. A generous fixed bound proves termination
        // without baking an off-by-one assumption into the audit.
        for (int frame = 0;
             frame < 128 && target.EnemyDefinitionPointer != DefinitionPointer;
             frame++)
        {
            enemies.StepEnemyProjectiles(
                assets.LevelData,
                samus: null,
                cameraX: 0,
                cameraY: 0);
        }
        if (target.EnemyDefinitionPointer != DefinitionPointer || target.Health != 10 ||
            target.XPosition != 0x0028 || target.YPosition != 0x0048 ||
            target.CurrentInstruction != 0xb9e0 ||
            enemies.RinkaStates[target.SlotIndex] is null)
        {
            throw new InvalidDataException(
                $"Rinka death respawn mismatch: ptr=${target.EnemyDefinitionPointer:X4}, " +
                $"health={target.Health}, position=({target.XPosition:X4},{target.YPosition:X4}), " +
                $"list=${target.CurrentInstruction:X4}.");
        }
    }

    private static void VerifyPowerBombDeath(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        (RoomEnemySystem enemies, SamusState samus) = LoadRoom(bus, room, assets);
        RoomEnemySlot target = GetRinkas(enemies)[0];
        StepUntilInteractive(enemies, samus, assets.LevelData, target);
        int reactions = enemies.ResolveOrdinaryPowerBombHits(
            bus,
            target.XPosition,
            target.YPosition,
            explosionRadius: 32);
        if (reactions != 1 || target.EnemyDefinitionPointer != 0xdaff ||
            enemies.EnemiesKilled != 0)
        {
            throw new InvalidDataException(
                $"Rinka power-bomb mismatch: reactions={reactions}, " +
                $"ptr=${target.EnemyDefinitionPointer:X4}, kills={enemies.EnemiesKilled}.");
        }
    }

    private static void VerifyNormalBombDeath(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        (RoomEnemySystem enemies, SamusState samus) = LoadRoom(bus, room, assets);
        RoomEnemySlot target = GetRinkas(enemies)[0];
        StepUntilInteractive(enemies, samus, assets.LevelData, target);

        var bombs = new SamusBombProjectileSystem();
        var shots = new SamusProjectileSystem();
        ArmNormalBomb(bombs.Slots[0], target);
        int hits = enemies.ResolveOrdinaryBombHits(bombs, shots, samus);
        if (hits != 1 || (bombs.Slots[0].Direction & 0x0010) == 0 ||
            target.EnemyDefinitionPointer != 0xdaff || target.Health != 0 ||
            enemies.EnemiesKilled != 0 ||
            enemies.EnemyProjectiles.Count(projectile =>
                projectile.Kind == RoomEnemyProjectileKind.EnemyDeathExplosion) != 1)
        {
            throw new InvalidDataException(
                $"Rinka normal-bomb death mismatch: hits={hits}, direction=" +
                $"${bombs.Slots[0].Direction:X4}, ptr=${target.EnemyDefinitionPointer:X4}, " +
                $"health={target.Health}, kills={enemies.EnemiesKilled}, projectiles=" +
                $"{enemies.ActiveEnemyProjectileCount}.");
        }
    }

    private static (RoomEnemySystem Enemies, SamusState Samus) LoadRoom(
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
            XPosition = 0x0080,
            YPosition = 0x0080,
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
            cameraX: 0,
            cameraY: 0);
        return (enemies, samus);
    }

    private static RoomEnemySlot[] GetRinkas(RoomEnemySystem enemies) =>
        enemies.Slots
            .Take(enemies.EnemyCount)
            .Where(slot => slot.EnemyDefinitionPointer == DefinitionPointer)
            .ToArray();

    private static void StepUntilInteractive(
        RoomEnemySystem enemies,
        SamusState samus,
        RoomLevelData level,
        RoomEnemySlot target)
    {
        for (int frame = 0; frame < 96; frame++)
        {
            enemies.StepFrame(0, 0, false, samus, level: level);
            if (enemies.InteractiveEnemyIndexes.Contains(target.NativeIndex))
                return;
        }
        throw new InvalidDataException("Rinka did not become interactive through ROM instruction $B9C7.");
    }

    private static void ArmPowerBeam(SamusProjectileSlot projectile, RoomEnemySlot target)
    {
        projectile.ClearFields();
        projectile.Type = 0;
        projectile.Damage = 20;
        projectile.Direction = (ushort)SamusProjectileDirection.Right;
        projectile.XPosition = target.XPosition;
        projectile.YPosition = target.YPosition;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    private static void ArmNormalBomb(
        SamusBombProjectileSlot bomb,
        RoomEnemySlot target)
    {
        bomb.ClearFields();
        // Rinka's bomb vulnerability is one. The shared half-damage convention turns this
        // twenty-point explosion into the exact ten points needed to enter `$A2:B960`.
        bomb.Type = SamusBombProjectileSystem.NormalBombType;
        bomb.Damage = 20;
        bomb.Direction = (ushort)SamusProjectileDirection.Right;
        bomb.XPosition = target.XPosition;
        bomb.YPosition = target.YPosition;
        bomb.XRadius = 16;
        bomb.YRadius = 16;
        bomb.BombTimer = 0;
        bomb.InstructionPointer = 0xa06b;
        bomb.InstructionTimer = 1;
    }

    private static void VerifyHeader(ISnesAddressSpace bus)
    {
        RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(bus, DefinitionPointer);
        if (definition.TileDataSize != 0x0600 || definition.PalettePointer != 0xba5b ||
            definition.Health != 10 || definition.Damage != 40 ||
            definition.XRadius != 8 || definition.YRadius != 8 || definition.Bank != 0xa2 ||
            definition.InitializationAiPointer != 0xb602 || definition.PartCount != 1 ||
            definition.MainAiPointer != 0xb7c4 || definition.GrappleAiPointer != 0x800f ||
            definition.HurtAiPointer != 0x804c || definition.FrozenAiPointer != 0xb929 ||
            definition.PowerBombReactionPointer != 0xb953 ||
            definition.TouchAiPointer != 0xb947 || definition.ShotAiPointer != 0xb94d ||
            definition.InitialSpritemapPointer != 0 ||
            definition.TileDataAddress != 0xaeb800 || definition.Layer != 2 ||
            definition.ItemDropChancesPointer != 0xf374 ||
            definition.VulnerabilityPointer != 0xec1c || definition.NamePointer != 0xe06f)
        {
            throw new InvalidDataException("Retail Rinka header words do not match $A0:D23F.");
        }
    }
}
