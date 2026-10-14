using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Bosses.Torizo;
using SuperMetroid.Verification.Harness;

/// <summary>Retail and guarded-production checks for Golden Torizo's normal, wall, and floor eye-beam programs.</summary>
internal static class GoldenTorizoEyeBeamInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the pinned cartridge and runs the eye-beam mechanics suite.</summary>
    internal static void VerifyGoldenTorizoEyeBeamInstructionProgramDefinitions() =>
        Suite(nameof(VerifyGoldenTorizoEyeBeamInstructionProgramDefinitions), () => VerifyGoldenTorizoEyeBeamInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));

    /// <summary>Checks compiled control words, both firing facings, exact impact timing and damage transitions, visual selectors, and ROM-read ownership.</summary>
    internal static void VerifyGoldenTorizoEyeBeamInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < GoldenTorizoEyeBeamInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                GoldenTorizoEyeBeamInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = unchecked((ushort)(
                SnesLittleEndian.Word(GoldenTorizoEyeBeamInstructionProgramDefinitionsRomByte(rom, EnemyProjectileCodePointers.BankBase | definition.Address), GoldenTorizoEyeBeamInstructionProgramDefinitionsRomByte(rom, EnemyProjectileCodePointers.BankBase |
                    unchecked((ushort)(definition.Address + 1))))));
            AssertEqual(definition.Value, native,
                $"Golden Torizo eye-beam mechanics word $86:{definition.Address:X4}");
        }

        var spriteArtwork = InstalledFixtures.EnemyTiles.ProjectileSpritemaps
            ?? throw new InvalidDataException("Projectile fixture requires installed sprites.");
        var executedOperands = new HashSet<ushort>();
        var guard = new CartridgeReadGuard(rom)
            .Forbid(GoldenTorizoEyeBeamInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Golden Torizo eye-beam byte ${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase,
                GoldenTorizoEyeBeamInstructionProgramDefinitions.PresentationWordCount,
                GoldenTorizoEyeBeamInstructionProgramDefinitions.PresentationWordAddress);
        Action<RoomEnemySystem, RoomEnemySlot, ushort> spawn =
            (enemies, torizo, parameter) => enemies.BombTorizoBehavior.SpawnGoldenTorizoEyeBeam(torizo, parameter);

        foreach (bool facingRight in new[] { false, true })
        {
            var random = new Queue<ushort>([0x0080, 0x0080, 0x0010]);
            RoomEnemySystem enemies = CreateSystem(guard, random, eyeBeamExplosions: false);
            RoomEnemySlot torizo = enemies.SlotTable.Slots[0];
            torizo.Position = torizo.Position.WithWhole(new(1000, 500));
            torizo.Parameter1 = facingRight ? (ushort)0x8000 : (ushort)0;
            spawn(enemies, torizo, 0);
            RoomEnemyProjectileSlot beam = enemies.Projectiles.EnemyProjectiles[^1];
            AssertEqual(GoldenTorizoEyeBeamInstructionProgramDefinitions.Normal,
                beam.InstructionPointer,
                $"Golden Torizo {facingRight} eye beam normal program");
            AssertEqual(unchecked((ushort)(1000 + (facingRight ? 20 : -20))),
                beam.Position.X.Whole,
                $"Golden Torizo {facingRight} eye beam X origin");
            AssertEqual((ushort)470, beam.Position.Y.Whole,
                $"Golden Torizo {facingRight} eye beam Y origin");
            AssertEqual(
                (ushort)EnemyProjectilePreInstruction.GoldenTorizoEyeBeam,
                beam.PreInstruction,
                $"Golden Torizo {facingRight} eye beam movement callback");

            for (int frame = 1; frame <= 6; frame++)
            {
                Process(enemies, beam, null);
                AssertTrue(beam.IsActive,
                    $"Golden Torizo {facingRight} eye beam normal frame {frame}");
            }
            AssertEqual((ushort)1, beam.InstructionTimer.RawValue,
                $"Golden Torizo {facingRight} eye beam reloads five-frame loop");
            AssertEqual(unchecked((ushort)(
                    GoldenTorizoEyeBeamInstructionProgramDefinitions.Normal + 4)),
                beam.InstructionPointer,
                $"Golden Torizo {facingRight} eye beam stable loop cursor");
            AssertEqual(0, random.Count,
                $"Golden Torizo {facingRight} eye beam consumes three RNG words");
        }

        ushort[] wallWords = new ushort[32 * 32];
        for (int row = 0; row < 32; row++)
        {
            wallWords[row * 32 + 10] = RoomLevelWord.Create(
                0,
                LevelBlockFlipFlags.None,
                RoomCollisionType.SolidBlock).Raw;
        }
        RoomLevelData wallRoom = CreateRoom(new(32, 32),
            wallWords,
            new byte[wallWords.Length]);
        RoomEnemySystem wallEnemies = CreateSystem(
            guard,
            new Queue<ushort>([0x0080, 0x0080, 0x0010]),
            eyeBeamExplosions: false);
        RoomEnemyProjectileSlot wallBeam = SpawnBeam(wallEnemies, spawn);
        wallBeam.Position = wallBeam.Position.WithWholeX(157).WithWholeY(128).WithFractionX(0);
        wallBeam.Velocity = new(new SnesSignedEightEight(256), new SnesSignedEightEight(0));
        BombTorizoBehavior.RunGoldenTorizoEyeBeamPreInstruction(wallBeam, wallRoom);
        AssertEqual(GoldenTorizoEyeBeamInstructionProgramDefinitions.WallImpact,
            wallBeam.InstructionPointer,
            "Golden Torizo eye beam wall collision selects wall impact");
        AssertEqual((ushort)1, wallBeam.InstructionTimer.RawValue,
            "Golden Torizo eye beam wall impact is immediately armed");
        for (int frame = 1; frame <= 20; frame++)
        {
            Process(wallEnemies, wallBeam, null);
            AssertTrue(wallBeam.IsActive,
                $"Golden Torizo eye beam wall impact frame {frame}");
            AssertEqual((ushort)EnemyProjectilePreInstruction.RTS_868170, wallBeam.PreInstruction,
                $"Golden Torizo eye beam wall impact clears movement frame {frame}");
        }
        Process(wallEnemies, wallBeam, null);
        AssertTrue(!wallBeam.IsActive,
            "Golden Torizo eye beam wall impact deletes after five four-frame poses");

        ushort[] floorWords = new ushort[32 * 32];
        for (int column = 0; column < 32; column++)
        {
            floorWords[10 * 32 + column] = RoomLevelWord.Create(
                0,
                LevelBlockFlipFlags.None,
                RoomCollisionType.SolidBlock).Raw;
        }
        RoomLevelData floorRoom = CreateRoom(new(32, 32),
            floorWords,
            new byte[floorWords.Length]);

        RoomEnemySystem disabledEnemies = CreateSystem(
            guard,
            new Queue<ushort>([0x0080, 0x0080, 0x0010]),
            eyeBeamExplosions: false);
        RoomEnemyProjectileSlot disabledBeam = SpawnBeam(disabledEnemies, spawn);
        CollideWithFloor(disabledEnemies, disabledBeam);
        for (int frame = 1; frame <= 9; frame++)
        {
            Process(disabledEnemies, disabledBeam, null);
            AssertTrue(disabledBeam.IsActive,
                $"disabled Golden Torizo eye-beam floor loop frame {frame}");
        }
        AssertEqual((ushort)8, disabledBeam.InstructionTimer.RawValue,
            "disabled Golden Torizo eye-beam explosion reloads blank wait");
        AssertEqual(unchecked((ushort)(
                GoldenTorizoEyeBeamInstructionProgramDefinitions.FloorImpactLoop + 4)),
            disabledBeam.InstructionPointer,
            "disabled Golden Torizo eye-beam explosion stays in blank loop");
        AssertTrue(!disabledBeam.CanDamageSamus,
            "disabled Golden Torizo eye-beam explosion remains harmless");

        RoomEnemySystem enabledEnemies = CreateSystem(
            guard,
            new Queue<ushort>([0x0080, 0x0080, 0x0010]),
            eyeBeamExplosions: true);
        RoomEnemyProjectileSlot enabledBeam = SpawnBeam(enabledEnemies, spawn);
        CollideWithFloor(enabledEnemies, enabledBeam);
        for (int frame = 1; frame <= 8; frame++)
        {
            Process(enabledEnemies, enabledBeam, null);
            AssertTrue(enabledBeam.IsActive,
                $"enabled Golden Torizo eye-beam blank floor frame {frame}");
            AssertTrue(!enabledBeam.CanDamageSamus,
                $"enabled Golden Torizo eye-beam blank floor frame {frame} is harmless");
        }
        for (int frame = 1; frame <= 39; frame++)
        {
            Process(enabledEnemies, enabledBeam, null);
            AssertTrue(enabledBeam.IsActive,
                $"enabled Golden Torizo eye-beam explosion frame {frame}");
            AssertEqual((ushort)EnemyProjectilePreInstruction.RTS_868170, enabledBeam.PreInstruction,
                $"enabled Golden Torizo eye-beam explosion clears movement frame {frame}");
            AssertEqual(frame >= 10, enabledBeam.CanDamageSamus,
                $"enabled Golden Torizo eye-beam collision transition frame {frame}");
        }
        Process(enabledEnemies, enabledBeam, null);
        AssertTrue(!enabledBeam.IsActive,
            "enabled Golden Torizo eye-beam floor impact deletes after exact lifetime");

        AssertEqual(0, guard.ObservedWords.Count,
            "GoldenTorizoEyeBeam execution performs no live spritemap operand reads");
        AssertEqual(GoldenTorizoEyeBeamInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count, "GoldenTorizoEyeBeam executes every native visual operand");
        for (int index = 0; index < GoldenTorizoEyeBeamInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort address = GoldenTorizoEyeBeamInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"GoldenTorizoEyeBeam executes native presentation operand {address:X4}");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0x86, address, out ushort selector),
                "GoldenTorizoEyeBeam has a compiled visual selector");
            AssertEqual(ReadVerificationWord(rom, 0x860000 | address), selector,
                "GoldenTorizoEyeBeam compiled selector matches the cartridge");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Golden Torizo eye-beam mechanics byte");
        AssertThrows<InvalidDataException>(
            () => GoldenTorizoEyeBeamInstructionProgramDefinitions.ReadMechanicsWord(0xb3d1),
            "Golden Torizo eye-beam spritemap is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => GoldenTorizoEyeBeamInstructionProgramDefinitions.ReadMechanicsWord(0xb3f1),
            "Golden Torizo eye-beam packed sound byte is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => GoldenTorizoEyeBeamInstructionProgramDefinitions.ReadMechanicsWord(0xb428),
            "adjacent eye-beam definition is rejected as program mechanics");

        _ = ProbeGoldenTorizoEyeBeamInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeGoldenTorizoEyeBeamInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Golden Torizo eye-beam allocation probe consumes data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Golden Torizo eye-beam mechanics lookups allocate no storage");

        Console.WriteLine(
            "Golden Torizo eye-beam instruction mechanics: twenty-eight compiled words, " +
            "both real producers, wall impact, and both floor branches pass with " +
            "mechanics bytes forbidden.");

        // Place a right-facing Golden Torizo at the fixture origin and return its spawned eye beam.
        RoomEnemyProjectileSlot SpawnBeam(RoomEnemySystem enemies, Action<RoomEnemySystem, RoomEnemySlot, ushort> spawnMethod)
        {
            RoomEnemySlot torizo = enemies.SlotTable.Slots[0];
            torizo.Position = torizo.Position.WithWhole(new(1000, 500));
            torizo.Parameter1 = 0x8000;
            spawnMethod(enemies, torizo, 0);
            return enemies.Projectiles.EnemyProjectiles[^1];
        }

        // Drive a downward beam into the fixture floor and check its impact program and low-nibble alignment.
        void CollideWithFloor(RoomEnemySystem enemies, RoomEnemyProjectileSlot beam)
        {
            beam.Position = beam.Position.WithWhole(new(128, 157));
            beam.Velocity = beam.Velocity.WithX(new SnesSignedEightEight(0));
            beam.Position = beam.Position.WithFractionY(0);
            beam.Velocity = beam.Velocity.WithY(new SnesSignedEightEight(256));
            BombTorizoBehavior.RunGoldenTorizoEyeBeamPreInstruction(beam, floorRoom);
            AssertEqual(GoldenTorizoEyeBeamInstructionProgramDefinitions.FloorImpact,
                beam.InstructionPointer,
                "Golden Torizo eye beam floor collision selects floor impact");
            AssertEqual((ushort)1, beam.InstructionTimer.RawValue,
                "Golden Torizo eye beam floor impact is immediately armed");
            AssertEqual((ushort)6, unchecked((ushort)(beam.Position.Y.Whole & 0x000f)),
                "Golden Torizo eye beam floor impact uses native low-nibble alignment");
        }

        // Advance one production projectile instruction frame and check its sprite against retail art.
        void Process(RoomEnemySystem system, RoomEnemyProjectileSlot projectile, SamusState? samus)
        {
            system.Projectiles.ProcessEnemyProjectileInstructions(projectile, samus, Position16<RoomPixels>.Zero);
            VerifyExecutedProjectileFrame(rom, projectile, spriteArtwork, executedOperands);
        }
        // Create a guarded Golden Torizo fixture with deterministic RNG and the selected floor-explosion attack flag.
        RoomEnemySystem CreateSystem(
            ISnesAddressSpace bus,
            Queue<ushort> random,
            bool eyeBeamExplosions)
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(bus);
            enemies.VerificationServices().NextRandom = (Func<ushort>)random.Dequeue;
            RoomEnemySlot torizo = enemies.SlotTable.Slots[0];
            var state = new TorizoEnemyState(torizo, isGolden: true)
            {
                AttackFlags = eyeBeamExplosions ? (ushort)0x8000 : (ushort)0,
            };
            enemies.BombTorizoBehavior._torizoState = state;
            return enemies;
        }
    }

    /// <summary>Reads a cartridge byte solely as an oracle for the compiled bank-$86 eye-beam controls.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Golden Torizo eye beam instruction program definitions")]
    private static byte GoldenTorizoEyeBeamInstructionProgramDefinitionsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Alternates normal and wall-impact control lookups to measure warmed per-frame allocation.</summary>
    private static int ProbeGoldenTorizoEyeBeamInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += GoldenTorizoEyeBeamInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? GoldenTorizoEyeBeamInstructionProgramDefinitions.WallImpact
                    : GoldenTorizoEyeBeamInstructionProgramDefinitions.Normal);
        }
        return checksum;
    }

    /// <summary>Runs the Golden Torizo eye-beam mechanics comparison from its command-line switch.</summary>
    [VerificationCommand("--golden-torizo-eye-beam-instruction-mechanics")]
    internal static int GoldenTorizoEyeBeamInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyGoldenTorizoEyeBeamInstructionProgramDefinitions), () => VerifyGoldenTorizoEyeBeamInstructionProgramDefinitions());
        return 0;
    }
}
