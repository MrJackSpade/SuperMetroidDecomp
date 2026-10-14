using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
namespace SuperMetroid.Verification.Bosses.Torizo;
using SuperMetroid.Verification.Harness;

/// <summary>Retail-word and guarded-execution checks for Torizo low-health and death explosion programs.</summary>
internal static class TorizoExplosionInstructionProgramDefinitionsVerification
{
    /// <summary>Loads retail data for the Torizo explosion instruction check.</summary>
    internal static void VerifyTorizoExplosionInstructionProgramDefinitions() =>
        Suite(nameof(VerifyTorizoExplosionInstructionProgramDefinitions), () => VerifyTorizoExplosionInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));

    /// <summary>Checks low-health jitter cycles and both probabilistic death paths through exact lifetime and frame selection.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Torizo explosion instruction program definitions")]
    internal static void VerifyTorizoExplosionInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < TorizoExplosionInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                TorizoExplosionInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | definition.Address);
            AssertEqual(definition.Value, native,
                $"Torizo explosion mechanics word $86:{definition.Address:X4}");
        }

        var spriteArtwork = InstalledFixtures.EnemyTiles.ProjectileSpritemaps
            ?? throw new InvalidDataException("Projectile fixture requires installed sprites.");
        var executedOperands = new HashSet<ushort>();
        var guard = NewTorizoExplosionInstructionReadGuard(rom);

        var lowRandom = new Queue<ushort>(
            [0xc000, 7, 9, 0x0000, 1, 2, 0x8000, 3, 4]);
        RoomEnemySystem lowEnemies = CreateSystem(guard, lowRandom);
        RoomEnemySlot lowTorizo = CreateTorizo(lowEnemies);
        lowTorizo.Parameter1 = 0x8000;
        lowEnemies.BombTorizoBehavior.SpawnBombTorizoLowHealthExplosion(lowTorizo, (ushort)0);
        RoomEnemyProjectileSlot low = lowEnemies.Projectiles.EnemyProjectiles[^1];
        AssertEqual(TorizoExplosionInstructionProgramDefinitions.LowHealthInitial,
            low.InstructionPointer, "low-health explosion selects compiled program");
        AssertEqual((ushort)1012, low.Variable0, "low-health explosion retained X center");
        AssertEqual((ushort)492, low.Variable1, "low-health explosion retained Y center");

        for (int frame = 1; frame <= 36; frame++)
        {
            lowEnemies.Projectiles.ProcessEnemyProjectileInstructions(low, null, Position16<RoomPixels>.Zero);
            VerifyExecutedProjectileFrame(rom, low, spriteArtwork, executedOperands);
            AssertTrue(low.IsActive,
                $"low-health explosion remains active through frame {frame}");
            if (frame == 1)
            {
                AssertEqual((ushort)1005, low.Position.X.Whole,
                    "low-health explosion first negative X jitter");
                AssertEqual((ushort)483, low.Position.Y.Whole,
                    "low-health explosion first negative Y jitter");
                AssertEqual((ushort)3, low.GeneralTimer,
                    "low-health explosion installs three-cycle timer");
            }
            else if (frame == 13)
            {
                AssertEqual((ushort)1013, low.Position.X.Whole,
                    "low-health explosion second X jitter resets around center");
                AssertEqual((ushort)494, low.Position.Y.Whole,
                    "low-health explosion second Y jitter resets around center");
                AssertEqual((ushort)2, low.GeneralTimer,
                    "low-health explosion second cycle timer");
            }
            else if (frame == 25)
            {
                AssertEqual((ushort)1009, low.Position.X.Whole,
                    "low-health explosion third X jitter resets around center");
                AssertEqual((ushort)496, low.Position.Y.Whole,
                    "low-health explosion third Y jitter resets around center");
                AssertEqual((ushort)1, low.GeneralTimer,
                    "low-health explosion final cycle timer");
            }
        }
        lowEnemies.Projectiles.ProcessEnemyProjectileInstructions(low, null, Position16<RoomPixels>.Zero);
        Suite(nameof(VerifyExecutedProjectileFrame), () => VerifyExecutedProjectileFrame(rom, low, spriteArtwork, executedOperands));
        AssertTrue(!low.IsActive,
            "low-health explosion deletes after three exact twelve-frame cycles");
        AssertEqual(0, lowRandom.Count,
            "low-health explosion consumes three random words per cycle");

        Suite(nameof(VerifyDeathPath), () => VerifyDeathPath(
            branchSample: 0,
            random: new Queue<ushort>(
                [0, 0xc000, 7, 25, 0x0000, 1, 20]),
            visibleFrames: 62,
            expectedFirst: (993, 491),
            expectedSecond: (1001, 504),
            secondCycleFrame: 32,
            "large explosions"));
        Suite(nameof(VerifyDeathPath), () => VerifyDeathPath(
            branchSample: 0xc000,
            random: new Queue<ushort>(
                [0xc000, 0x8000, 8, 9, 0x4000, 2, 12]),
            visibleFrames: 64,
            expectedFirst: (992, 505),
            expectedSecond: (1002, 492),
            secondCycleFrame: 33,
            "smoke"));

        AssertEqual(0, guard.ObservedWords.Count,
            "TorizoExplosion execution performs no live spritemap operand reads");
        AssertEqual(TorizoExplosionInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count, "TorizoExplosion executes every native visual operand");
        for (int index = 0; index < TorizoExplosionInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort address = TorizoExplosionInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"TorizoExplosion executes native presentation operand {address:X4}");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0x86, address, out ushort selector),
                "TorizoExplosion has a compiled visual selector");
            AssertEqual(ReadVerificationWord(rom, 0x860000 | address), selector,
                "TorizoExplosion compiled selector matches the cartridge");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Torizo explosion mechanics byte");
        AssertThrows<InvalidDataException>(
            () => TorizoExplosionInstructionProgramDefinitions.ReadMechanicsWord(0xa3e2),
            "Torizo explosion spritemap is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => TorizoExplosionInstructionProgramDefinitions.ReadMechanicsWord(0xa3df),
            "Torizo explosion packed sound byte is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => TorizoExplosionInstructionProgramDefinitions.ReadMechanicsWord(0xa456),
            "adjacent probability callback body is rejected as explosion mechanics");

        _ = ProbeTorizoExplosionInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeTorizoExplosionInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Torizo explosion allocation probe consumes data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Torizo explosion mechanics lookups allocate no storage");

        Console.WriteLine(
            "Torizo explosion instruction mechanics: fifty-three compiled words, the " +
            "real low-health producer's three cycles, and both probabilistic death paths " +
            "pass with exact jitter, lifetimes, fifteen native selectors and zero source reads.");

        void VerifyDeathPath(
            ushort branchSample,
            Queue<ushort> random,
            int visibleFrames,
            (int X, int Y) expectedFirst,
            (int X, int Y) expectedSecond,
            int secondCycleFrame,
            string scenario)
        {
            AssertEqual(branchSample, random.Peek(),
                $"Torizo death {scenario} branch sample fixture");
            RoomEnemySystem enemies = CreateSystem(guard, random);
            RoomEnemySlot torizo = CreateTorizo(enemies);
            enemies.BombTorizoBehavior.SpawnBombTorizoDeathExplosion(torizo);
            RoomEnemyProjectileSlot projectile = enemies.Projectiles.EnemyProjectiles[^1];
            AssertEqual(TorizoExplosionInstructionProgramDefinitions.DeathInitial,
                projectile.InstructionPointer,
                $"Torizo death {scenario} selects compiled program");

            for (int frame = 1; frame <= visibleFrames; frame++)
            {
                enemies.Projectiles.ProcessEnemyProjectileInstructions(projectile, null, Position16<RoomPixels>.Zero);
                VerifyExecutedProjectileFrame(rom, projectile, spriteArtwork, executedOperands);
                AssertTrue(projectile.IsActive,
                    $"Torizo death {scenario} remains active through frame {frame}");
                if (frame == 1)
                {
                    AssertEqual(unchecked((ushort)expectedFirst.X), projectile.Position.X.Whole,
                        $"Torizo death {scenario} first X jitter");
                    AssertEqual(unchecked((ushort)expectedFirst.Y), projectile.Position.Y.Whole,
                        $"Torizo death {scenario} first Y jitter");
                    AssertEqual((ushort)2, projectile.GeneralTimer,
                        $"Torizo death {scenario} installs two-cycle timer");
                }
                else if (frame == secondCycleFrame)
                {
                    AssertEqual(unchecked((ushort)expectedSecond.X), projectile.Position.X.Whole,
                        $"Torizo death {scenario} second X jitter resets around center");
                    AssertEqual(unchecked((ushort)expectedSecond.Y), projectile.Position.Y.Whole,
                        $"Torizo death {scenario} second Y jitter resets around center");
                    AssertEqual((ushort)1, projectile.GeneralTimer,
                        $"Torizo death {scenario} final cycle timer");
                }
            }
            enemies.Projectiles.ProcessEnemyProjectileInstructions(projectile, null, Position16<RoomPixels>.Zero);
            VerifyExecutedProjectileFrame(rom, projectile, spriteArtwork, executedOperands);
            AssertTrue(!projectile.IsActive,
                $"Torizo death {scenario} deletes after its second complete cycle");
            AssertEqual(0, random.Count,
                $"Torizo death {scenario} consumes branch plus six jitter RNG words");
        }

        RoomEnemySystem CreateSystem(ISnesAddressSpace bus, Queue<ushort> random)
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(bus);
            enemies.VerificationServices().NextRandom = (Func<ushort>)random.Dequeue;
            return enemies;
        }

        static RoomEnemySlot CreateTorizo(RoomEnemySystem enemies)
        {
            RoomEnemySlot torizo = enemies.SlotTable.Slots[0];
            torizo.Position = torizo.Position.WithWhole(new(1000, 500));
            return torizo;
        }
    }

    /// <summary>Alternates warmed low-health and death control lookups for allocation accounting.</summary>
    private static int ProbeTorizoExplosionInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += TorizoExplosionInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? TorizoExplosionInstructionProgramDefinitions.LowHealthInitial
                    : TorizoExplosionInstructionProgramDefinitions.DeathInitial);
        }
        return checksum;
    }

    /// <summary>Rejects migrated Torizo control reads while observing visual-operand accesses.</summary>
    private static CartridgeReadGuard NewTorizoExplosionInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(TorizoExplosionInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Torizo explosion byte ${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase, TorizoExplosionInstructionProgramDefinitions.PresentationWordCount, TorizoExplosionInstructionProgramDefinitions.PresentationWordAddress);

    /// <summary>Runs the retail-backed Torizo explosion instruction migration command.</summary>
    [VerificationCommand("--torizo-explosion-instruction-mechanics")]
    internal static int TorizoExplosionInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyTorizoExplosionInstructionProgramDefinitions), () => VerifyTorizoExplosionInstructionProgramDefinitions());
        return 0;
    }
}
