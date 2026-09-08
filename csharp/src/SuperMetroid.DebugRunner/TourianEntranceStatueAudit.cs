using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// Retail-ROM audit for the inert three-record Tourian entrance statue and the three
/// bank-$86 actors created by its first record.
/// </summary>
internal static class TourianEntranceStatueAudit
{
    private const ushort RoomPointer = 0xa66a;
    private const ushort PopulationPointer = 0x9081;
    private const ushort Definition = 0xefff;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, RoomPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        if (room.State.Pointer != 0xa677 ||
            room.State.EnemyPopulationPointer != PopulationPointer)
        {
            throw new InvalidDataException(
                $"Tourian entrance state mismatch: room/state=${room.Pointer:X4}/" +
                $"${room.State.Pointer:X4}, population=${room.State.EnemyPopulationPointer:X4}.");
        }

        VerifyPopulation(bus);
        VerifyActors(bus, room, assets);
        VerifyRoomUnlockPipeline(romPath);
        Console.WriteLine(
            "Tourian entrance statue audit passed: three retail enemy records selected " +
            "their ROM lists/palettes, spawned the base/Ridley/Phantoon bank-$86 actors, " +
            "advanced instruction lists, drew OBJ, published completion, and retained native " +
            "contact/projectile immunity.");
        return 0;
    }

    private static void VerifyPopulation(ISnesAddressSpace bus)
    {
        ushort[][] expected =
        [
            [Definition, 0x0078, 0x00b8, 0, 0x2000, 0, 0, 0],
            [Definition, 0x008e, 0x0055, 0, 0x2000, 0, 2, 0],
            [Definition, 0x0084, 0x0088, 0, 0x2000, 0, 4, 0],
        ];
        for (int record = 0; record < expected.Length; record++)
        {
            // Population $9081 begins with one Sciser. The statue records start at $9091.
            int address = 0xa10000 | (0x9091 + record * 16);
            for (int word = 0; word < 8; word++)
            {
                ushort actual = ReadWord(bus, address + word * 2);
                if (actual != expected[record][word])
                {
                    throw new InvalidDataException(
                        $"Tourian statue record {record} word {word} is ${actual:X4}, " +
                        $"expected ${expected[record][word]:X4}.");
                }
            }
        }

        RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(bus, Definition);
        if (definition.Bank != 0xaa || definition.InitializationAiPointer != 0xd7c8 ||
            definition.MainAiPointer != 0xd7c7 || definition.TouchAiPointer != 0x804c ||
            definition.ShotAiPointer != 0x804c)
        {
            throw new InvalidDataException(
                $"Tourian statue header mismatch: bank=${definition.Bank:X2}, init/main=" +
                $"${definition.InitializationAiPointer:X4}/${definition.MainAiPointer:X4}, " +
                $"touch/shot=${definition.TouchAiPointer:X4}/${definition.ShotAiPointer:X4}.");
        }
    }

    /// <summary>Checks the production room pipeline, not just its three inert enemy records.</summary>
    private static void VerifyRoomUnlockPipeline(string romPath)
    {
        foreach (int bossCount in new[] { 0, 3, 4 })
        {
            bool allBossesDead = bossCount == 4;
            var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
            var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
                foreach (AreaId area in new[] { AreaId.Brinstar, AreaId.Norfair, AreaId.WreckedShip, AreaId.Maridia }.Take(bossCount))
                    runtime.System.SetBossBits(area, BossBits.AreaBoss);
            runtime.LoadCartridgeRoomForDebug(RoomPointer);
            var offsets = new HashSet<short>();
            for (int frame = 0; frame < 3400; frame++)
            {
                runtime.StepFrame(0);
                offsets.Add(runtime.TourianStatues.VerticalOffset);
            }
            if (runtime.System.HasEvent(EventNumber.TourianUnlocked) != allBossesDead)
                throw new InvalidDataException("Statue room ignored the four-boss prerequisite or failed to unlock.");
            if (!allBossesDead) continue;
            if (offsets.Count != TourianStatueRomData.DescentDistance + 1)
                throw new InvalidDataException("Statue descent skipped native intermediate pixel positions.");
            Console.WriteLine($"Tourian sequence: {offsets.Count} descent positions, all four bosses released.");
            runtime.LoadCartridgeRoomForDebug(RoomPointer);
            for (int frame = 0; frame < 4; frame++) runtime.StepFrame(0);
            if (runtime.TourianStatues.VerticalOffset != -TourianStatueRomData.DescentDistance ||
                runtime.Camera!.Scrolls.ReadState(1) != RoomScrollState.Green)
                throw new InvalidDataException("Unlocked statue room did not restore its descent and scroll state on re-entry.");
        }
    }

    private static void VerifyActors(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState(0x1234);
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            XPosition = 0x0078,
            YPosition = 0x00b8,
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
            samus: samus);

        RoomEnemySlot[] statues = enemies.Slots
            .Take(enemies.EnemyCount)
            .Where(slot => slot.EnemyDefinitionPointer == Definition)
            .ToArray();
        if (statues.Length != 3 || statues.Select(slot => slot.Parameter1)
                .SequenceEqual([(ushort)0, (ushort)2, (ushort)4]) == false ||
            statues.Any(slot => slot.PaletteIndex != 0 || slot.CurrentInstruction == 0))
        {
            throw new InvalidDataException(
                $"Tourian statue initialization mismatch: count={statues.Length}, params=" +
                $"[{string.Join(',', statues.Select(slot => slot.Parameter1))}], lists=" +
                $"[{string.Join(',', statues.Select(slot => $"${slot.CurrentInstruction:X4}"))}].");
        }

        RoomEnemyProjectileKind[] expectedKinds =
        [
            RoomEnemyProjectileKind.TourianStatueBaseDecoration,
            RoomEnemyProjectileKind.TourianStatueRidley,
            RoomEnemyProjectileKind.TourianStatuePhantoon,
        ];
        foreach (RoomEnemyProjectileKind kind in expectedKinds)
        {
            if (!enemies.EnemyProjectiles.Any(projectile => projectile.Kind == kind))
                throw new InvalidDataException($"Tourian statue did not spawn ${kind:X4}.");
        }

        var enemyMaps = new HashSet<ushort>();
        var projectileMaps = new HashSet<ushort>();
        for (int frame = 0; frame < 240; frame++)
        {
            enemies.StepFrame(0, 0, false, samus, level: assets.LevelData);
            enemies.StepEnemyProjectiles(assets.LevelData, samus);
            foreach (RoomEnemySlot statue in statues)
                enemyMaps.Add(statue.SpritemapPointer);
            foreach (RoomEnemyProjectileSlot projectile in enemies.EnemyProjectiles)
            {
                if (projectile.IsActive)
                    projectileMaps.Add(projectile.SpritemapPointer);
            }
        }
        if (enemyMaps.Count < 1 || enemyMaps.Contains(0) || projectileMaps.Count < 3 ||
            !enemies.TourianEntranceStatueFinished)
        {
            throw new InvalidDataException(
                $"Tourian statue animation incomplete: enemy maps={enemyMaps.Count}, " +
                $"projectile maps={projectileMaps.Count}, finished=" +
                $"{enemies.TourianEntranceStatueFinished}.");
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        enemies.DrawEnemyProjectiles(oam, 0, 0);
        enemies.DrawLayers(oam, 0, 0, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("Tourian statue emitted no OBJ pieces.");

        ushort healthBefore = samus.Health;
        samus.InvincibilityTimer = 0;
        _ = enemies.ResolveOrdinarySamusContact(samus, controllerInput: 0);
        if (samus.Health != healthBefore)
            throw new InvalidDataException("Inert Tourian statue incorrectly damaged Samus.");

        var shots = new SamusProjectileSystem();
        var bombs = new SamusBombProjectileSystem();
        ArmProjectile(shots.Slots[0], statues[0]);
        ushort statueHealth = statues[0].Health;
        int hits = enemies.ResolveOrdinaryProjectileHits(bus, shots, bombs, samus);
        if (hits != 0 || statues[0].Health != statueHealth)
        {
            throw new InvalidDataException(
                $"Tourian statue shot immunity mismatch: hits={hits}, " +
                $"health={statueHealth}->{statues[0].Health}.");
        }
    }

    private static void ArmProjectile(SamusProjectileSlot projectile, RoomEnemySlot target)
    {
        projectile.ClearFields();
        projectile.Type = 0;
        projectile.Damage = 20;
        projectile.Direction = (ushort)SamusProjectileDirection.Right;
        projectile.XPosition = target.XPosition;
        projectile.YPosition = target.YPosition;
        projectile.XRadius = 16;
        projectile.YRadius = 32;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}
