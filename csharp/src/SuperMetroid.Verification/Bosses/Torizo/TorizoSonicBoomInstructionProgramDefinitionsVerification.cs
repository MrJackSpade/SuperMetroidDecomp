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
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.Torizo;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Bomb and Golden Torizo sonic-boom launch, travel, and wall-impact bytecode.</summary>
internal static class TorizoSonicBoomInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge oracle for both Torizo sonic-boom producers.</summary>
    internal static void VerifyTorizoSonicBoomInstructionProgramDefinitions() =>
        Suite(nameof(VerifyTorizoSonicBoomInstructionProgramDefinitions), () => VerifyTorizoSonicBoomInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));

    /// <summary>Exercises four facing-dependent launches and five impact cycles, including visual operands and exact RNG consumption.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Torizo sonic boom instruction program definitions")]
    internal static void VerifyTorizoSonicBoomInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < TorizoSonicBoomInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                TorizoSonicBoomInstructionProgramDefinitionsTooling.MechanicsWord(index);
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | definition.Address);
            AssertEqual(definition.Value, native,
                $"Torizo sonic-boom mechanics word $86:{definition.Address:X4}");
        }

        var spriteArtwork = InstalledFixtures.EnemyTiles.ProjectileSpritemaps
            ?? throw new InvalidDataException("Projectile fixture requires installed sprites.");
        var executedOperands = new HashSet<ushort>();
        var guard = NewTorizoSonicBoomInstructionReadGuard(rom);
        Action<RoomEnemySystem, RoomEnemySlot, ushort> spawnBomb =
            (enemies, torizo, parameter) => enemies.BombTorizoBehavior.SpawnBombTorizoSonicBoom(torizo, parameter);
        Action<RoomEnemySystem, RoomEnemySlot, ushort> spawnGolden =
            (enemies, torizo, parameter) => enemies.BombTorizoBehavior.SpawnGoldenTorizoSonicBoom(torizo, parameter);

        foreach (bool golden in new[] { false, true })
        foreach (bool facingRight in new[] { false, true })
        {
            var random = new Queue<ushort>([facingRight ? (ushort)1 : (ushort)0]);
            RoomEnemySystem enemies = CreateSystem(guard, random);
            RoomEnemySlot torizo = CreateTorizo(enemies, facingRight);
            (golden ? spawnGolden : spawnBomb)(enemies, torizo, 0);
            RoomEnemyProjectileSlot boom = enemies.Projectiles.EnemyProjectiles[^1];
            ushort expectedProgram = facingRight
                ? TorizoSonicBoomInstructionProgramDefinitions.FiredRight
                : TorizoSonicBoomInstructionProgramDefinitions.FiredLeft;
            AssertEqual(expectedProgram, boom.InstructionPointer,
                $"{(golden ? "Golden" : "Bomb")} Torizo {facingRight} sonic-boom program");
            AssertEqual(unchecked((ushort)(1000 + (facingRight ? 32 : -32))),
                boom.Position.X.Whole,
                $"{(golden ? "Golden" : "Bomb")} Torizo {facingRight} sonic-boom X origin");
            AssertEqual(unchecked((ushort)(500 + (facingRight ? -12 : 20))),
                boom.Position.Y.Whole,
                $"{(golden ? "Golden" : "Bomb")} Torizo {facingRight} sonic-boom random Y origin");
            AssertEqual(facingRight ? (ushort)624 : unchecked((ushort)-624),
                boom.Velocity.X.RawValue,
                $"{(golden ? "Golden" : "Bomb")} Torizo {facingRight} sonic-boom velocity");

            for (int frame = 1; frame <= 93; frame++)
            {
                enemies.Projectiles.ProcessEnemyProjectileInstructions(boom, null, Position16<RoomPixels>.Zero);
                VerifyExecutedProjectileFrame(rom, boom, spriteArtwork, executedOperands);
                AssertTrue(boom.IsActive,
                    $"{(golden ? "Golden" : "Bomb")} Torizo {facingRight} sonic-boom frame {frame}");
            }
            ushort movingProgram = facingRight
                ? TorizoSonicBoomInstructionProgramDefinitions.MovingRight
                : TorizoSonicBoomInstructionProgramDefinitions.MovingLeft;
            AssertEqual((ushort)0x0050, boom.InstructionTimer.RawValue,
                $"{(golden ? "Golden" : "Bomb")} Torizo {facingRight} reloads moving pose");
            AssertEqual(unchecked((ushort)(movingProgram + 4)), boom.InstructionPointer,
                $"{(golden ? "Golden" : "Bomb")} Torizo {facingRight} moving loop cursor");
            AssertEqual(0, random.Count,
                $"{(golden ? "Golden" : "Bomb")} Torizo {facingRight} launch RNG count");
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
        var impactRandom = new Queue<ushort>(
        [
            0,
            0, 1, 2,
            0, 1, 2,
            0, 1, 2,
            0, 1, 2,
            0, 1, 2,
        ]);
        RoomEnemySystem impactEnemies = CreateSystem(guard, impactRandom);
        RoomEnemySlot impactTorizo = CreateTorizo(impactEnemies, facingRight: true);
        spawnBomb(impactEnemies, impactTorizo, 0);
        RoomEnemyProjectileSlot impact = impactEnemies.Projectiles.EnemyProjectiles[^1];
        impact.Position = impact.Position.WithWholeX(157).WithWholeY(128).WithFractionX(0);
        impact.Velocity = impact.Velocity.WithX(new SnesSignedEightEight(624));
        impact.BlocksSamusProjectiles = true;
        impact.CanDamageSamus = true;
        impact.DrawPriority = EnemyProjectileDrawPriority.Low;
        BombTorizoBehavior.RunBombTorizoSonicBoomPreInstruction(impact, wallRoom);
        AssertEqual(TorizoSonicBoomInstructionProgramDefinitions.WallImpact,
            impact.InstructionPointer,
            "Torizo sonic boom wall collision selects compiled impact program");
        AssertEqual((ushort)1, impact.InstructionTimer.RawValue,
            "Torizo sonic boom wall collision arms impact immediately");
        AssertEqual((ushort)157, impact.Variable0,
            "Torizo sonic boom retains wall-aligned impact X center");
        AssertEqual((ushort)128, impact.Variable1,
            "Torizo sonic boom retains impact Y center");

        for (int frame = 1; frame <= 60; frame++)
        {
            impactEnemies.Projectiles.ProcessEnemyProjectileInstructions(impact, null, Position16<RoomPixels>.Zero);
            VerifyExecutedProjectileFrame(rom, impact, spriteArtwork, executedOperands);
            AssertTrue(impact.IsActive, $"Torizo sonic-boom impact frame {frame}");
            AssertEqual((ushort)EnemyProjectilePreInstruction.RTS_868170, impact.PreInstruction,
                $"Torizo sonic-boom impact clears movement on frame {frame}");
            AssertTrue(!impact.BlocksSamusProjectiles,
                $"Torizo sonic-boom impact disables shot collision on frame {frame}");
            AssertTrue(!impact.CanDamageSamus,
                $"Torizo sonic-boom impact disables Samus collision on frame {frame}");
            AssertEqual(EnemyProjectileDrawPriority.High, impact.DrawPriority,
                $"Torizo sonic-boom impact is high priority on frame {frame}");
            if ((frame - 1) % 12 == 0)
            {
                int cycle = (frame - 1) / 12;
                AssertEqual((ushort)158, impact.Position.X.Whole,
                    $"Torizo sonic-boom impact cycle {cycle + 1} X jitter");
                AssertEqual((ushort)130, impact.Position.Y.Whole,
                    $"Torizo sonic-boom impact cycle {cycle + 1} Y jitter");
                AssertEqual(unchecked((ushort)(5 - cycle)), impact.GeneralTimer,
                    $"Torizo sonic-boom impact cycle {cycle + 1} timer");
            }
        }
        impactEnemies.Projectiles.ProcessEnemyProjectileInstructions(impact, null, Position16<RoomPixels>.Zero);
        Suite(nameof(VerifyExecutedProjectileFrame), () => VerifyExecutedProjectileFrame(rom, impact, spriteArtwork, executedOperands));
        AssertTrue(!impact.IsActive,
            "Torizo sonic-boom impact deletes after five exact twelve-frame cycles");
        AssertEqual(0, impactRandom.Count,
            "Torizo sonic-boom launch and five jitter cycles consume exact RNG words");

        AssertEqual(0, guard.ObservedWords.Count,
            "TorizoSonicBoom execution performs no live spritemap operand reads");
        AssertEqual(TorizoSonicBoomInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count, "TorizoSonicBoom executes every native visual operand");
        for (int index = 0; index < TorizoSonicBoomInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort address = TorizoSonicBoomInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"TorizoSonicBoom executes native presentation operand {address:X4}");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0x86, address, out ushort selector),
                "TorizoSonicBoom has a compiled visual selector");
            AssertEqual(ReadVerificationWord(rom, 0x860000 | address), selector,
                "TorizoSonicBoom compiled selector matches the cartridge");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Torizo sonic-boom mechanics byte");
        AssertThrows<InvalidDataException>(
            () => TorizoSonicBoomInstructionProgramDefinitions.ReadMechanicsWord(0xadc4),
            "Torizo sonic-boom spritemap is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => TorizoSonicBoomInstructionProgramDefinitions.ReadMechanicsWord(0xadc1),
            "Torizo sonic-boom packed left sound byte is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => TorizoSonicBoomInstructionProgramDefinitions.ReadMechanicsWord(0xadd4),
            "Torizo sonic-boom packed right sound byte is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => TorizoSonicBoomInstructionProgramDefinitions.ReadMechanicsWord(0xae15),
            "adjacent sonic-boom initialization AI is rejected as program mechanics");

        _ = ProbeTorizoSonicBoomInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeTorizoSonicBoomInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Torizo sonic-boom allocation probe consumes data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Torizo sonic-boom mechanics lookups allocate no storage");

        Console.WriteLine(
            "Torizo sonic-boom instruction mechanics: thirty-one compiled words, four " +
            "real launch loops, the real wall handoff, and five exact impact cycles pass " +
            "with mechanics bytes forbidden.");

        RoomEnemySystem CreateSystem(ISnesAddressSpace bus, Queue<ushort> random)
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(bus);
            enemies.VerificationServices().NextRandom = (Func<ushort>)random.Dequeue;
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

    /// <summary>Consumes warmed launch and impact mechanics words to check lookup allocations.</summary>
    private static int ProbeTorizoSonicBoomInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += TorizoSonicBoomInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? TorizoSonicBoomInstructionProgramDefinitions.FiredLeft
                    : TorizoSonicBoomInstructionProgramDefinitions.WallImpact);
        }
        return checksum;
    }

    /// <summary>Forbids migrated sonic-boom mechanics bytes and observes visual operand reads.</summary>
    private static CartridgeReadGuard NewTorizoSonicBoomInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(TorizoSonicBoomInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Torizo sonic-boom byte ${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase, TorizoSonicBoomInstructionProgramDefinitions.PresentationWordCount, TorizoSonicBoomInstructionProgramDefinitions.PresentationWordAddress);

    /// <summary>Runs the sonic-boom instruction comparison against the local retail cartridge.</summary>
    [VerificationCommand("--torizo-sonic-boom-instruction-mechanics")]
    internal static int TorizoSonicBoomInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyTorizoSonicBoomInstructionProgramDefinitions), () => VerifyTorizoSonicBoomInstructionProgramDefinitions());
        return 0;
    }
}
