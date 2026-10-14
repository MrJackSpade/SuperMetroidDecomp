using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.Torizo;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Bomb and Golden Torizo Chozo-orb loops, impacts, and shot-drop bytecode against the cartridge.</summary>
internal static class TorizoChozoOrbInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge oracle for both Torizo orb projectile families.</summary>
    internal static void VerifyTorizoChozoOrbInstructionProgramDefinitions() =>
        Suite(nameof(VerifyTorizoChozoOrbInstructionProgramDefinitions), () => VerifyTorizoChozoOrbInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));

    /// <summary>Exercises four producer loops, wall and floor impacts, and both shot drops with compiled mechanics reads forbidden.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Torizo Chozo orb instruction program definitions")]
    internal static void VerifyTorizoChozoOrbInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < TorizoChozoOrbInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                TorizoChozoOrbInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | definition.Address);
            AssertEqual(definition.Value, native,
                $"Torizo Chozo-orb mechanics word $86:{definition.Address:X4}");
        }

        var guard = NewTorizoChozoOrbInstructionReadGuard(rom);
        Action<RoomEnemySystem, RoomEnemySlot> spawnBomb =
            (enemies, torizo) => enemies.BombTorizoBehavior.SpawnBombTorizoChozoOrb(torizo);
        Action<RoomEnemySystem, RoomEnemySlot> spawnGolden =
            (enemies, torizo) => enemies.BombTorizoBehavior.SpawnGoldenTorizoChozoOrb(torizo);

        foreach (bool golden in new[] { false, true })
        foreach (bool facingRight in new[] { false, true })
        {
            RoomEnemySystem enemies = CreateSystem(guard);
            RoomEnemySlot torizo = CreateTorizo(enemies, facingRight);
            (golden ? spawnGolden : spawnBomb)(enemies, torizo);
            RoomEnemyProjectileSlot orb = enemies.Projectiles.EnemyProjectiles[^1];
            ushort expectedProgram = facingRight
                ? TorizoChozoOrbInstructionProgramDefinitions.MovingRight
                : TorizoChozoOrbInstructionProgramDefinitions.MovingLeft;
            AssertEqual(expectedProgram, orb.InstructionPointer,
                $"{(golden ? "Golden" : "Bomb")} Torizo {facingRight} orb program");

            for (int frame = 1; frame <= 86; frame++)
            {
                enemies.Projectiles.ProcessEnemyProjectileInstructions(orb, null, Position16<RoomPixels>.Zero);
                AssertTrue(orb.IsActive,
                    $"{(golden ? "Golden" : "Bomb")} Torizo {facingRight} orb loop frame {frame}");
            }
            AssertEqual((ushort)0x0055, orb.InstructionTimer.RawValue,
                $"{(golden ? "Golden" : "Bomb")} Torizo {facingRight} orb reloads 85-frame pose");
            AssertEqual(unchecked((ushort)(expectedProgram + 4)), orb.InstructionPointer,
                $"{(golden ? "Golden" : "Bomb")} Torizo {facingRight} orb loop cursor");
        }

        RoomEnemySystem wallEnemies = CreateSystem(guard);
        RoomEnemySlot wallTorizo = CreateTorizo(wallEnemies, facingRight: false);
        spawnBomb(wallEnemies, wallTorizo);
        RoomEnemyProjectileSlot wall = wallEnemies.Projectiles.EnemyProjectiles[^1];
        wall.InstructionPointer = TorizoChozoOrbInstructionProgramDefinitions.WallImpact;
        wall.InstructionTimer = NativeCountdown.Start(1);
        for (int frame = 1; frame <= 20; frame++)
        {
            wallEnemies.Projectiles.ProcessEnemyProjectileInstructions(wall, null, Position16<RoomPixels>.Zero);
            AssertTrue(wall.IsActive, $"Torizo orb wall impact frame {frame}");
            AssertEqual((ushort)EnemyProjectilePreInstruction.RTS_868170, wall.PreInstruction,
                $"Torizo orb wall impact clears callback on frame {frame}");
            AssertTrue(wall.PersistsOnSamusContact,
                $"Torizo orb wall impact persists on Samus contact on frame {frame}");
        }
        wallEnemies.Projectiles.ProcessEnemyProjectileInstructions(wall, null, Position16<RoomPixels>.Zero);
        AssertTrue(!wall.IsActive,
            "Torizo orb wall impact deletes after five four-frame poses");

        RoomEnemySystem floorEnemies = CreateSystem(guard);
        RoomEnemySlot floorTorizo = CreateTorizo(floorEnemies, facingRight: true);
        spawnGolden(floorEnemies, floorTorizo);
        RoomEnemyProjectileSlot floor = floorEnemies.Projectiles.EnemyProjectiles[^1];
        AssertTrue(!floor.CanDamageSamus,
            "Golden Torizo airborne orb starts unable to damage Samus");
        floor.InstructionPointer = TorizoChozoOrbInstructionProgramDefinitions.FloorImpact;
        floor.InstructionTimer = NativeCountdown.Start(1);
        for (int frame = 1; frame <= 39; frame++)
        {
            floorEnemies.Projectiles.ProcessEnemyProjectileInstructions(floor, null, Position16<RoomPixels>.Zero);
            AssertTrue(floor.IsActive, $"Torizo orb floor impact frame {frame}");
            AssertTrue(floor.CanDamageSamus,
                $"Torizo orb floor impact clears no-contact bit on frame {frame}");
            AssertTrue(floor.PersistsOnSamusContact,
                $"Torizo orb floor impact persists on contact on frame {frame}");
            AssertEqual(EnemyProjectileDrawPriority.High, floor.DrawPriority,
                $"Torizo orb floor impact remains high priority on frame {frame}");
        }
        floorEnemies.Projectiles.ProcessEnemyProjectileInstructions(floor, null, Position16<RoomPixels>.Zero);
        AssertTrue(!floor.IsActive,
            "Torizo orb floor impact deletes after exact 4/5/6/7/8/9-frame poses");

        Suite(nameof(VerifyShotDrop), () => VerifyShotDrop(golden: false));
        Suite(nameof(VerifyShotDrop), () => VerifyShotDrop(golden: true));

        AssertEqual(0, guard.ObservedWords.Count,
            "compiled Torizo orb visuals require no cartridge reads");
        for (int index = 0; index < TorizoChozoOrbInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort address = TorizoChozoOrbInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0x86, address, out ushort selector),
                "Torizo orb compiled selector exists");
            AssertEqual(ReadVerificationWord(rom, 0x860000 | address), selector,
                "Torizo orb compiled selector matches original operand");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Torizo Chozo-orb mechanics byte");
        AssertThrows<InvalidDataException>(
            () => TorizoChozoOrbInstructionProgramDefinitions.ReadMechanicsWord(0xab17),
            "Torizo Chozo-orb spritemap is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => TorizoChozoOrbInstructionProgramDefinitions.ReadMechanicsWord(0xab4d),
            "Torizo Chozo-orb packed sound byte is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => TorizoChozoOrbInstructionProgramDefinitions.ReadMechanicsWord(0xab8a),
            "adjacent drop callback body is rejected as orb mechanics");

        _ = ProbeTorizoChozoOrbInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeTorizoChozoOrbInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Torizo Chozo-orb allocation probe consumes data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Torizo Chozo-orb mechanics lookups allocate no storage");

        Console.WriteLine(
            "Torizo Chozo-orb instruction mechanics: forty compiled words, four real " +
            "producer loops, exact wall/floor impacts, and both shot/drop operands pass " +
            "with mechanics bytes forbidden.");

        void VerifyShotDrop(bool golden)
        {
            RoomEnemySystem enemies = CreateSystem(guard);
            RoomEnemySlot torizo = CreateTorizo(enemies, facingRight: false);
            (golden ? spawnGolden : spawnBomb)(enemies, torizo);
            if (golden)
            {
                enemies.BombTorizoBehavior._torizoState = new TorizoEnemyState(torizo, isGolden: true);
            }

            RoomEnemyProjectileSlot orb = enemies.Projectiles.EnemyProjectiles[^1];
            orb.Position = orb.Position.WithWhole(new(0x0350, 0x0260));
            orb.InstructionPointer = TorizoChozoOrbInstructionProgramDefinitions.Shot;
            orb.InstructionTimer = NativeCountdown.Start(1);
            for (int frame = 1; frame <= 20; frame++)
            {
                enemies.Projectiles.ProcessEnemyProjectileInstructions(orb, null, Position16<RoomPixels>.Zero);
                AssertTrue(orb.IsActive,
                    $"{(golden ? "Golden" : "Bomb")} Torizo orb shot frame {frame}");
            }
            enemies.Projectiles.ProcessEnemyProjectileInstructions(orb, null, Position16<RoomPixels>.Zero);
            AssertTrue(!orb.IsActive,
                $"{(golden ? "Golden" : "Bomb")} Torizo orb shot deletes after drop callback");
            AssertEqual(1, enemies.TorizoOrbDropRequests.Count,
                $"{(golden ? "Golden" : "Bomb")} Torizo orb shot publishes one drop");
            // The drop callback spawns its pickup from the selected header's chance table at
            // the orb's position; that pickup is the observable trace of header and placement.
            EnemyDefinitionId expectedHeader = golden
                ? EnemyDefinitionId.GoldenTorizoOrb
                : EnemyDefinitionId.BombTorizoOrb;
            RoomEnemyDefinition expectedDefinition =
                RoomEnemyAuxiliaryDefinitionCatalog.TryGet(expectedHeader, out RoomEnemyDefinition auxiliary)
                    ? auxiliary
                    : RoomEnemyDefinitionCatalog.Get(expectedHeader);
            RoomEnemyProjectileSlot pickup = enemies.Projectiles.EnemyProjectiles.Single(slot =>
                slot.Kind == RoomEnemyProjectileKind.EnemyDeathPickup);
            AssertEqual(expectedDefinition.ItemDropChancesPointer,
                pickup.ItemDropChancesPointerOverride,
                $"{(golden ? "Golden" : "Bomb")} Torizo orb shot drop header");
            AssertEqual((ushort)0x0350, pickup.Position.X.Whole,
                $"{(golden ? "Golden" : "Bomb")} Torizo orb shot drop X");
            AssertEqual((ushort)0x0260, pickup.Position.Y.Whole,
                $"{(golden ? "Golden" : "Bomb")} Torizo orb shot drop Y");
        }

        RoomEnemySystem CreateSystem(ISnesAddressSpace bus)
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(bus);
            enemies.VerificationServices().NextRandom = (Func<ushort>)(() => 0x0080);
            enemies.DeathsAndDrops._samusForEnemyDrops = new SamusState();
            return enemies;
        }

        static RoomEnemySlot CreateTorizo(RoomEnemySystem enemies, bool facingRight)
        {
            RoomEnemySlot torizo = enemies.SlotTable.Slots[0];
            torizo.Position = torizo.Position.WithWhole(new(1000, 500));
            torizo.Parameter1 = facingRight ? (ushort)0x8000 : (ushort)0;
            return torizo;
        }
    }

    /// <summary>Consumes warmed moving and shot mechanics words for a lookup allocation check.</summary>
    private static int ProbeTorizoChozoOrbInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += TorizoChozoOrbInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? TorizoChozoOrbInstructionProgramDefinitions.MovingLeft
                    : TorizoChozoOrbInstructionProgramDefinitions.Shot);
        }
        return checksum;
    }

    /// <summary>Forbids migrated orb mechanics bytes and records attempted visual operand reads.</summary>
    private static CartridgeReadGuard NewTorizoChozoOrbInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(TorizoChozoOrbInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Torizo Chozo-orb byte ${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase, TorizoChozoOrbInstructionProgramDefinitions.PresentationWordCount, TorizoChozoOrbInstructionProgramDefinitions.PresentationWordAddress);

    /// <summary>Runs the Torizo Chozo-orb instruction comparison against the local retail cartridge.</summary>
    [VerificationCommand("--torizo-chozo-orb-instruction-mechanics")]
    internal static int TorizoChozoOrbInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyTorizoChozoOrbInstructionProgramDefinitions), () => VerifyTorizoChozoOrbInstructionProgramDefinitions());
        return 0;
    }
}
