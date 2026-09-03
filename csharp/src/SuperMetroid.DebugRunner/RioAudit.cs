using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Untouched-ROM behavior audit for the single Rio in Crateria room $8F:98E2. The room's
/// other three actors are already-translated Mero flies, so this fixture exercises Rio in
/// real terrain without hiding failures behind a synthetic one-enemy population.
/// </summary>
internal static class RioAudit
{
    private const ushort DefinitionPointer = 0xd27f;
    private const ushort RoomPointer = 0x98e2;
    private const ushort ExpectedStatePointer = 0x98ef;
    private const ushort ExpectedPopulationPointer = 0x8b87;
    private const ushort ExpectedTilesetPointer = 0x825f;
    // Center the three-screen room on Rio's ceiling perch. A camera at $0080 would leave
    // the actor's natural rightward return arc one pixel beyond the processing rectangle,
    // freezing the audit for a host-camera reason unrelated to its cartridge AI.
    private const ushort CameraX = 0x00e0;
    private const ushort CameraY = 0x0000;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        VerifyHeader(bus);

        CartridgeRoomHeader room = CartridgeRoomHeader.Load(
            bus,
            RoomPointer,
            new RoomStateSelectionContext(
                Array.Empty<byte>(),
                BossBits: BossBits.None,
                HasMorphBallAndMissiles: false,
                HasPowerBombs: false));
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        if (room.State.Pointer != ExpectedStatePointer ||
            room.State.EnemyPopulationPointer != ExpectedPopulationPointer ||
            room.State.EnemyTilesetPointer != ExpectedTilesetPointer ||
            room.WidthInScreens != 3 || room.HeightInScreens != 1)
        {
            throw new InvalidDataException(
                $"Rio room selection mismatch: state=${room.State.Pointer:X4}, " +
                $"population/set=${room.State.EnemyPopulationPointer:X4}/" +
                $"${room.State.EnemyTilesetPointer:X4}, dimensions=" +
                $"{room.WidthInScreens}x{room.HeightInScreens}.");
        }

        VerifyDiveAndReturn(bus, room, assets);
        VerifyContactAndWeaponDamage(bus, room, assets);
        Console.WriteLine(
            "Rio untouched-room audit passed: retail load, ROM animation, mirrored dive, " +
            "terrain collision, perch return, contact damage, beam death, " +
            "and power-bomb death agree.");
        return 0;
    }

    private static void VerifyDiveAndReturn(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        (RoomEnemySystem enemies, SamusState samus) = LoadRoom(bus, room, assets);
        RoomEnemySlot rio = GetRio(enemies);
        RioEnemyState state = enemies.RioStates[rio.SlotIndex]
            ?? throw new InvalidDataException("Retail Rio did not receive typed state.");
        if (enemies.EnemyCount != 4 || rio.XPosition != 0x014b || rio.YPosition != 0x0045 ||
            rio.Health != 45 || rio.CurrentInstruction != 0xbb4b ||
            state.Function != RioEnemyFunction.WaitingForSamus ||
            state.InstalledInstructionList != 0)
        {
            throw new InvalidDataException(
                $"Rio initialization mismatch: count={enemies.EnemyCount}, " +
                $"position=({rio.XPosition:X4},{rio.YPosition:X4}), health={rio.Health}, " +
                $"list=${rio.CurrentInstruction:X4}, function=${(ushort)state.Function:X4}.");
        }

        // Samus begins to the left and below the ceiling perch, within the strict 160-pixel
        // horizontal trigger. One enemy frame must both select the dive state and advance
        // the first ROM animation frame.
        enemies.StepFrame(CameraX, CameraY, false, samus, level: assets.LevelData);
        ushort initialYVelocity = ReadWord(bus, 0xa2bbbb);
        ushort initialXVelocity = ReadWord(bus, 0xa2bbbf);
        if (state.Function != RioEnemyFunction.Diving ||
            state.YVelocity != initialYVelocity ||
            state.XVelocity != unchecked((ushort)-(short)initialXVelocity) ||
            state.InstalledInstructionList != 0xbb7f ||
            enemies.LastRioSoundEffect != 0x0065 || rio.SpritemapPointer == 0)
        {
            throw new InvalidDataException(
                $"Rio dive start mismatch: function=${(ushort)state.Function:X4}, " +
                $"velocity=({state.XVelocity:X4},{state.YVelocity:X4}), " +
                $"list=${state.InstalledInstructionList:X4}, " +
                $"sound={enemies.LastRioSoundEffect?.ToString("X4") ?? "none"}, " +
                $"map=${rio.SpritemapPointer:X4}.");
        }

        var maps = new HashSet<ushort> { rio.SpritemapPointer };
        bool sawLateDiveAnimation = false;
        bool sawBounce = false;
        bool sawLandingWait = false;
        bool sawIdleAgain = false;
        for (int frame = 0; frame < 1200; frame++)
        {
            enemies.StepFrame(CameraX, CameraY, false, samus, level: assets.LevelData);
            maps.Add(rio.SpritemapPointer);
            sawLateDiveAnimation |= state.InstalledInstructionList == 0xbb97;
            sawBounce |= state.Function == RioEnemyFunction.BouncingBackToPerch;
            sawLandingWait |= state.Function == RioEnemyFunction.WaitingForLandingAnimation &&
                state.InstalledInstructionList == 0xbba3;
            sawIdleAgain |= sawLandingWait &&
                state.Function == RioEnemyFunction.WaitingForSamus &&
                state.InstalledInstructionList == 0xbb53;
            if (sawIdleAgain)
                break;
        }

        // In this particular retail room the solid ledge catches both left and right dives
        // before velocity crosses zero, so collision enters Rio_4 directly and Rio_5's
        // hover branch is legitimately absent. The host still translates Rio_5 literally;
        // this audit refuses to fake an occurrence by moving the cartridge population.
        if (!sawLateDiveAnimation || !sawBounce ||
            !sawLandingWait || !sawIdleAgain || maps.Count < 4)
        {
            throw new InvalidDataException(
                $"Rio cycle mismatch: late/bounce/land/idle=" +
                $"{sawLateDiveAnimation}/{sawBounce}/" +
                $"{sawLandingWait}/{sawIdleAgain}, maps={maps.Count}, " +
                $"position=({rio.XPosition:X4},{rio.YPosition:X4}), " +
                $"velocity=({state.XVelocity:X4},{state.YVelocity:X4}), " +
                $"function=${(ushort)state.Function:X4}, list=${state.InstalledInstructionList:X4}.");
        }

        // A second load with Samus on the right proves Rio_1's signed mirroring rather than
        // merely showing that the left-facing population happens to move.
        (enemies, samus) = LoadRoom(bus, room, assets);
        rio = GetRio(enemies);
        state = enemies.RioStates[rio.SlotIndex]!;
        samus.XPosition = unchecked((ushort)(rio.XPosition + 32));
        enemies.StepFrame(CameraX, CameraY, false, samus, level: assets.LevelData);
        if (state.XVelocity != initialXVelocity)
        {
            throw new InvalidDataException(
                $"Rio right-facing velocity ${state.XVelocity:X4} did not mirror " +
                $"retail source ${initialXVelocity:X4}.");
        }
    }

    private static void VerifyContactAndWeaponDamage(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        (RoomEnemySystem enemies, SamusState samus) = LoadRoom(bus, room, assets);
        RoomEnemySlot rio = GetRio(enemies);
        enemies.StepFrame(CameraX, CameraY, false, samus, level: assets.LevelData);
        samus.Health = 999;
        samus.InvincibilityTimer = 0;
        samus.KnockbackActive = false;
        samus.XPosition = rio.XPosition;
        samus.YPosition = rio.YPosition;
        if (!enemies.ResolveOrdinarySamusContact(samus, controllerInput: 0) ||
            samus.Health != 984 || !samus.KnockbackActive || rio.Health != 45)
        {
            throw new InvalidDataException(
                $"Rio contact mismatch: Samus health={samus.Health}, " +
                $"knockback={samus.KnockbackActive}, Rio health={rio.Health}.");
        }

        (enemies, samus) = LoadRoom(bus, room, assets);
        rio = GetRio(enemies);
        enemies.StepFrame(CameraX, CameraY, false, samus, level: assets.LevelData);
        var shots = new SamusProjectileSystem();
        var bombs = new SamusBombProjectileSystem();
        ArmLethalBeam(shots.Slots[0], rio);
        if (enemies.ResolveOrdinaryProjectileHits(bus, shots, bombs, samus) != 1 ||
            !rio.Properties.HasAny(EnemyProperties.Deleted) || rio.Health != 0 ||
            enemies.EnemiesKilled != 1)
        {
            throw new InvalidDataException(
                $"Rio beam death mismatch: health={rio.Health}, " +
                $"deleted={rio.Properties.HasAny(EnemyProperties.Deleted)}, " +
                $"kills={enemies.EnemiesKilled}.");
        }

        (enemies, samus) = LoadRoom(bus, room, assets);
        rio = GetRio(enemies);
        int reactions = enemies.ResolveOrdinaryPowerBombHits(
            bus,
            rio.XPosition,
            rio.YPosition,
            explosionRadius: 32);
        if (reactions != 1 || !rio.Properties.HasAny(EnemyProperties.Deleted) ||
            rio.Health != 0 || enemies.EnemiesKilled != 1)
        {
            throw new InvalidDataException(
                $"Rio power-bomb mismatch: reactions={reactions}, health={rio.Health}, " +
                $"deleted={rio.Properties.HasAny(EnemyProperties.Deleted)}, " +
                $"kills={enemies.EnemiesKilled}.");
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
            Pose = SamusPoseIds.FacingRightNormalPose,
            XPosition = 0x012b,
            YPosition = 0x00c0,
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
        return (enemies, samus);
    }

    private static RoomEnemySlot GetRio(RoomEnemySystem enemies) =>
        enemies.Slots
            .Take(enemies.EnemyCount)
            .Single(slot => slot.EnemyDefinitionPointer == DefinitionPointer);

    private static void ArmLethalBeam(SamusProjectileSlot projectile, RoomEnemySlot target)
    {
        projectile.ClearFields();
        projectile.Type = 0;
        projectile.Damage = 100;
        projectile.Direction = (ushort)SamusProjectileDirection.Right;
        projectile.XPosition = target.XPosition;
        projectile.YPosition = target.YPosition;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    private static void VerifyHeader(ISnesAddressSpace bus)
    {
        RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(bus, DefinitionPointer);
        if (definition.TileDataSize != 0x0400 || definition.PalettePointer != 0xba7b ||
            definition.Health != 45 || definition.Damage != 15 ||
            definition.XRadius != 16 || definition.YRadius != 7 ||
            definition.Bank != 0xa2 || definition.HurtAiTime != 0 ||
            definition.HurtSoundEffect != 0x0036 || definition.BossId != 0 ||
            definition.InitializationAiPointer != 0xbbcd || definition.PartCount != 1 ||
            definition.MainAiPointer != 0xbbe3 || definition.GrappleAiPointer != 0x800f ||
            definition.HurtAiPointer != 0x804c || definition.FrozenAiPointer != 0x8041 ||
            definition.TimeFrozenAiPointer != 0 || definition.DeathAnimation != 2 ||
            definition.PowerBombReactionPointer != 0 || definition.VariantIndex != 0 ||
            definition.TouchAiPointer != 0x8023 || definition.ShotAiPointer != 0x802d ||
            definition.InitialSpritemapPointer != 0 ||
            definition.TileDataAddress != 0xaeb000 || definition.Layer != 5 ||
            definition.ItemDropChancesPointer != 0xf1fa ||
            definition.VulnerabilityPointer != 0xec1c || definition.NamePointer != 0xe07d)
        {
            throw new InvalidDataException("Retail Rio header words do not match $A0:D27F.");
        }
    }

    private static ushort ReadWord(SuperMetroidAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}
