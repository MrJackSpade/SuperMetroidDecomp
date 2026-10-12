using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static FakeKraidProjectileProgramMappingsVerification;
using static SuiteTimingVerification;

/// <summary>Represents fake kraid projectile instruction program definitions verification.</summary>
internal static class FakeKraidProjectileInstructionProgramDefinitionsVerification
{
    /// <summary>Verifies fake kraid projectile instruction program definitions.</summary>
    internal static void VerifyFakeKraidProjectileInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyFakeKraidProjectileInstructionProgramDefinitions), () => VerifyFakeKraidProjectileInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Verifies fake kraid projectile instruction program definitions.</summary>
    internal static void VerifyFakeKraidProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        Suite(nameof(VerifyFakeKraidProjectileMechanicsMapping), () => VerifyFakeKraidProjectileMechanicsMapping(rom));
        Suite(nameof(VerifyFakeKraidProjectilePresentationAddresses), () => VerifyFakeKraidProjectilePresentationAddresses());

        var guard = new CartridgeReadGuard(rom)
            .Forbid(address => FakeKraidProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                    CommonEnemyProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address),
                address => $"Production read compiled Fake Kraid mechanics byte ${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase,
                FakeKraidProjectileInstructionProgramDefinitions.PresentationWordCount,
                FakeKraidProjectileInstructionProgramDefinitions.PresentationWordAddress);
        var selectedPresentationWords = new HashSet<ushort>();
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(guard);

        var source = new RoomEnemySlot(0)
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(128, 112)),
            VramTilesIndex = 0x0200,
            PaletteIndex = 0x0c00,
        };
        var state = new FakeKraidEnemyState(source);

        AssertTrue((bool)enemies.FakeKraidBehavior.SpawnFakeKraidSpit(source, (short)12, new EightEightVector<RoomPixels>(0x0180, -0x0200)),
            "real Fake Kraid spit producer allocates its projectile");
        source.VariableC = unchecked((ushort)-4);
        AssertTrue((bool)enemies.FakeKraidBehavior.SpawnFakeKraidSpike(source, state, 0),
            "real Fake Kraid left-spike producer allocates its projectile");
        source.VariableC = 4;
        AssertTrue((bool)enemies.FakeKraidBehavior.SpawnFakeKraidSpike(source, state, 1),
            "real Fake Kraid right-spike producer allocates its projectile");

        var cases = new[]
        {
            (Kind: RoomEnemyProjectileKind.FakeKraidSpit,
                Initial: FakeKraidProjectileInstructionProgramDefinitions.Spit,
                Sleep: FakeKraidProjectileInstructionProgramDefinitions.SpitSleep),
            (Kind: RoomEnemyProjectileKind.FakeKraidSpikeLeft,
                Initial: FakeKraidProjectileInstructionProgramDefinitions.SpikeLeft,
                Sleep: FakeKraidProjectileInstructionProgramDefinitions.SpikeLeftSleep),
            (Kind: RoomEnemyProjectileKind.FakeKraidSpikeRight,
                Initial: FakeKraidProjectileInstructionProgramDefinitions.SpikeRight,
                Sleep: FakeKraidProjectileInstructionProgramDefinitions.SpikeRightSleep),
        };

        foreach (var testCase in cases)
        {
            RoomEnemyProjectileSlot projectile = enemies.Projectiles.EnemyProjectiles.Single(
                candidate => candidate.Kind == testCase.Kind);
            AssertEqual(testCase.Initial, projectile.InstructionPointer,
                $"{testCase.Kind} selects its named cartridge program");
            RunForcedTick(projectile);
            AssertEqual(testCase.Sleep, projectile.InstructionPointer,
                $"{testCase.Kind} schedules its terminal sleep");
            RunForcedTick(projectile);
            AssertEqual(testCase.Sleep, projectile.InstructionPointer,
                $"{testCase.Kind} sleeps at the authored opcode");
            AssertEqual((ushort)0, projectile.InstructionTimer,
                $"{testCase.Kind} sleep leaves the timer stopped");

            projectile.InstructionPointer =
                CommonEnemyProjectileInstructionProgramDefinitions.Delete;
            RunForcedTick(projectile);
            AssertTrue(!projectile.IsActive,
                $"{testCase.Kind} shot reaction reaches the compiled shared delete program");
        }

        AssertEqual(
            FakeKraidProjectileInstructionProgramDefinitions.PresentationWordCount,
            selectedPresentationWords.Count,
            "all live Fake Kraid projectile frames select installed operands");
        for (int index = 0;
             index < FakeKraidProjectileInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = FakeKraidProjectileInstructionProgramDefinitions
                .PresentationWordAddress(index);
            AssertTrue(selectedPresentationWords.Contains(address),
                $"production execution selects Fake Kraid presentation $86:{address:X4}");
        }

        AssertEqual(0, guard.ObservedWords.Count,
            "Fake Kraid frames never read presentation operands from ROM");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Fake Kraid and shared-delete mechanics byte");
        AssertThrows<InvalidDataException>(
            () => FakeKraidProjectileInstructionProgramDefinitions.ReadMechanicsWord(0x9ddc),
            "Fake Kraid spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => FakeKraidProjectileInstructionProgramDefinitions.ReadMechanicsWord(0x9dec),
            "adjacent Fake Kraid initializer is rejected as mechanics");

        _ = ProbeFakeKraidProjectileInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeFakeKraidProjectileInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Fake Kraid allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Fake Kraid projectile mechanics lookups allocate no storage");

        Console.WriteLine(
            "Fake Kraid projectile instruction mechanics: six compiled words, all three " +
            "real spit/spike producers, three terminal sleeps, shared shot deletion, and " +
            "three installed frame selectors pass without instruction ROM reads.");

        void RunForcedTick(RoomEnemyProjectileSlot projectile)
        {
            projectile.InstructionTimer = 1;
            enemies.Projectiles.ProcessEnemyProjectileInstructions(projectile, new SamusState(), Position16<RoomPixels>.Zero);
            if (projectile.IsActive && projectile.InstructionTimer != 0)
            {
                ushort operand = (ushort)(projectile.InstructionPointer - 2);
                AssertEqual(operand, projectile.PresentationOperandAddress,
                    "Fake Kraid selects the just-executed native frame operand");
                AssertEqual(ReadFakeKraidProjectileInstructionWord(rom, (ushort)(operand - 2)),
                    projectile.InstructionTimer, "Fake Kraid frame retains the native duration");
                selectedPresentationWords.Add(projectile.PresentationOperandAddress);
            }
        }
    }

    /// <summary>Probe fake kraid projectile instruction mechanics allocation.</summary>
    private static int ProbeFakeKraidProjectileInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += FakeKraidProjectileInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? FakeKraidProjectileInstructionProgramDefinitions.Spit
                    : FakeKraidProjectileInstructionProgramDefinitions.SpikeRight);
        }
        return checksum;
    }

    /// <summary>Reads fake kraid projectile instruction word.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for fake Kraid projectile instruction words")]
    internal static ushort ReadFakeKraidProjectileInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | address);

    /// <summary>Fake kraid projectile instruction mechanics command.</summary>
    [VerificationCommand("--fake-kraid-projectile-instruction-mechanics")]
    internal static int FakeKraidProjectileInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyFakeKraidProjectileInstructionProgramDefinitions), () => VerifyFakeKraidProjectileInstructionProgramDefinitions());
        return 0;
    }
}
