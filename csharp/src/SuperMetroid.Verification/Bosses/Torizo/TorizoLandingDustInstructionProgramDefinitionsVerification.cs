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
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.Torizo;
using SuperMetroid.Verification.Harness;

/// <summary>Checks both Bomb Torizo foot-dust instruction lists against bank $86 and live projectile motion.</summary>
internal static class TorizoLandingDustInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the pinned cartridge and runs the landing-dust program comparison.</summary>
    internal static void VerifyTorizoLandingDustInstructionProgramDefinitions() =>
        Suite(nameof(VerifyTorizoLandingDustInstructionProgramDefinitions), () => VerifyTorizoLandingDustInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));

    /// <summary>Checks foot spawns, four rising poses, deletion, visual selectors, and guarded mechanics reads.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Torizo landing dust instruction program definitions")]
    internal static void VerifyTorizoLandingDustInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < TorizoLandingDustInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                TorizoLandingDustInstructionProgramDefinitionsTooling.MechanicsWord(index);
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | definition.Address);
            AssertEqual(definition.Value, native,
                $"Torizo landing-dust mechanics word $86:{definition.Address:X4}");
        }

        var spriteArtwork = InstalledFixtures.EnemyTiles.ProjectileSpritemaps
            ?? throw new InvalidDataException("Projectile fixture requires installed sprites.");
        var executedOperands = new HashSet<ushort>();
        var guard = NewTorizoLandingDustInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(guard);
        Action<RoomEnemySlot, bool> spawn = enemies.BombTorizoBehavior.SpawnBombTorizoLandingDust;

        RoomEnemySlot torizo = enemies.SlotTable.Slots[0];
        torizo.Position = torizo.Position.WithWhole(new(300, 120));
        spawn(torizo, true);
        spawn(torizo, false);
        RoomEnemyProjectileSlot right = enemies.Projectiles.EnemyProjectiles.First(
            projectile => projectile.Kind == RoomEnemyProjectileKind.BombTorizoRightFootDust);
        RoomEnemyProjectileSlot left = enemies.Projectiles.EnemyProjectiles.First(
            projectile => projectile.Kind == RoomEnemyProjectileKind.BombTorizoLeftFootDust);

        AssertEqual((ushort)324, right.Position.X.Whole, "right-foot dust producer X position");
        AssertEqual((ushort)276, left.Position.X.Whole, "left-foot dust producer X position");
        AssertEqual((ushort)168, right.Position.Y.Whole, "right-foot dust producer Y position");
        AssertEqual((ushort)168, left.Position.Y.Whole, "left-foot dust producer Y position");
        AssertEqual(TorizoLandingDustInstructionProgramDefinitions.RightFoot,
            right.InstructionPointer, "right-foot dust program selection");
        AssertEqual(TorizoLandingDustInstructionProgramDefinitions.LeftFoot,
            left.InstructionPointer, "left-foot dust program selection");

        Run(right, 4);
        Run(left, 4);
        AssertEqual((ushort)156, right.Position.Y.Whole,
            "right-foot dust rises exactly twelve pixels across its four poses");
        AssertEqual((ushort)156, left.Position.Y.Whole,
            "left-foot dust rises exactly twelve pixels across its four poses");
        Run(right, 1);
        Run(left, 1);
        AssertTrue(!right.IsActive && !left.IsActive,
            "both landing-dust programs delete after their fourth pose");

        AssertEqual(0, guard.ObservedWords.Count,
            "TorizoLandingDust execution performs no live spritemap operand reads");
        AssertEqual(TorizoLandingDustInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count, "TorizoLandingDust executes every native visual operand");
        for (int index = 0; index < TorizoLandingDustInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort address = TorizoLandingDustInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"TorizoLandingDust executes native presentation operand {address:X4}");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0x86, address, out ushort selector),
                "TorizoLandingDust has a compiled visual selector");
            AssertEqual(ReadVerificationWord(rom, 0x860000 | address), selector,
                "TorizoLandingDust compiled selector matches the cartridge");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Torizo landing-dust mechanics byte");
        AssertThrows<InvalidDataException>(
            () => TorizoLandingDustInstructionProgramDefinitions.ReadMechanicsWord(0xaf9f),
            "Torizo landing-dust spritemap is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => TorizoLandingDustInstructionProgramDefinitions.ReadMechanicsWord(0xaf92),
            "Torizo landing-dust callback body is rejected as mechanics");

        _ = ProbeTorizoLandingDustInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeTorizoLandingDustInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Torizo landing-dust allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Torizo landing-dust mechanics lookups allocate no storage");

        Console.WriteLine(
            "Torizo landing-dust instruction mechanics: sixteen compiled words, both " +
            "real foot producers, exact twelve-pixel rise/deletion, and eight installed " +
            "sprite frames match native OAM with zero live operand reads.");

        void Run(RoomEnemyProjectileSlot projectile, int steps)
        {
            for (int step = 0; step < steps; step++)
            {
                projectile.InstructionTimer = NativeCountdown.Start(1);
                enemies.Projectiles.ProcessEnemyProjectileInstructions(projectile, null, Position16<RoomPixels>.Zero);
                VerifyExecutedProjectileFrame(rom, projectile, spriteArtwork, executedOperands);
            }
        }
    }

    /// <summary>Repeats warmed foot-dust mechanics lookups for the caller's allocation measurement.</summary>
    private static int ProbeTorizoLandingDustInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += TorizoLandingDustInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? TorizoLandingDustInstructionProgramDefinitions.RightFoot
                    : TorizoLandingDustInstructionProgramDefinitions.LeftFoot);
        }
        return checksum;
    }

    /// <summary>Forbids compiled control-byte rereads and observes native spritemap operands.</summary>
    private static CartridgeReadGuard NewTorizoLandingDustInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(TorizoLandingDustInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Torizo landing-dust byte ${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase, TorizoLandingDustInstructionProgramDefinitions.PresentationWordCount, TorizoLandingDustInstructionProgramDefinitions.PresentationWordAddress);

    /// <summary>Dispatches the Torizo landing-dust instruction verification command.</summary>
    [VerificationCommand("--torizo-landing-dust-instruction-mechanics")]
    internal static int TorizoLandingDustInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyTorizoLandingDustInstructionProgramDefinitions), () => VerifyTorizoLandingDustInstructionProgramDefinitions());
        return 0;
    }
}
