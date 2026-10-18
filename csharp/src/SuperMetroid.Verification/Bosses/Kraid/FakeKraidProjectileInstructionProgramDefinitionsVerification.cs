using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.Kraid.FakeKraidProjectileProgramMappingsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Bosses.Kraid;
using SuperMetroid.Verification.Harness;

/// <summary>Checks compiled Fake Kraid spit and spike instruction programs against retail words and live projectile execution.</summary>
internal static class FakeKraidProjectileInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail ROM and runs the Fake Kraid projectile mechanics verification suite.</summary>
    internal static void VerifyFakeKraidProjectileInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyFakeKraidProjectileInstructionProgramDefinitions), () => VerifyFakeKraidProjectileInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Exercises all three real projectile producers through frame selection, terminal sleep, and shared deletion while guarding compiled mechanics bytes.</summary>
    internal static void VerifyFakeKraidProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyFakeKraidProjectileMechanicsMapping), () => VerifyFakeKraidProjectileMechanicsMapping(rom));
        Suite(nameof(VerifyFakeKraidProjectilePresentationAddresses), () => VerifyFakeKraidProjectilePresentationAddresses());

        var guard = new CartridgeReadGuard(rom)
            .Forbid(FakeKraidProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Fake Kraid mechanics byte ${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase,
                FakeKraidProjectileInstructionProgramDefinitions.PresentationWordCount,
                FakeKraidProjectileInstructionProgramDefinitions.PresentationWordAddress);
        var selectedPresentationWords = new HashSet<ushort>();
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);

        var source = new RoomEnemySlot(0, EnemySlotFixtures.Registry)
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(128, 112)),
            VramTilesIndex = 0x0200,
            PaletteIndex = 0x0c00,
        };
        var state = new FakeKraidEnemyState();

        AssertTrue((bool)enemies.FakeKraidBehavior.SpawnFakeKraidSpit(source, (short)12, new EightEightVector<RoomPixels>(0x0180, -0x0200)),
            "real Fake Kraid spit producer allocates its projectile");
        state.FacingDelta = -4;
        AssertTrue((bool)enemies.FakeKraidBehavior.SpawnFakeKraidSpike(source, state, 0),
            "real Fake Kraid left-spike producer allocates its projectile");
        state.FacingDelta = 4;
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
            AssertEqual((ushort)0, projectile.InstructionTimer.RawValue,
                $"{testCase.Kind} sleep leaves the timer stopped");

            projectile.StartAnimation(EnemyProjectileDeleteAnimation.Instance);
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
            projectile.InstructionTimer = NativeCountdown.Start(1);
            enemies.Projectiles.ProcessEnemyProjectileInstructions(projectile, new SamusState(), Position16<RoomPixels>.Zero);
            if (projectile.IsActive && !projectile.InstructionTimer.IsZero)
            {
                ushort operand = (ushort)(projectile.InstructionPointer - 2);
                AssertEqual(operand, projectile.PresentationOperandAddress,
                    "Fake Kraid selects the just-executed native frame operand");
                AssertEqual(ReadFakeKraidProjectileInstructionWord(rom, (ushort)(operand - 2)),
                    projectile.InstructionTimer.RawValue, "Fake Kraid frame retains the native duration");
                selectedPresentationWords.Add(projectile.PresentationOperandAddress);
            }
        }
    }

    /// <summary>Repeatedly reads the compiled spit and spike words to confirm warmed mechanics lookups allocate no memory.</summary>
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

    /// <summary>Reads one bank-$86 projectile program word from the retail ROM as an exact verification oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for fake Kraid projectile instruction words")]
    internal static ushort ReadFakeKraidProjectileInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | address);

    /// <summary>Runs the Fake Kraid projectile instruction mechanics command and returns its suite status.</summary>
    [VerificationCommand("--fake-kraid-projectile-instruction-mechanics")]
    internal static int FakeKraidProjectileInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyFakeKraidProjectileInstructionProgramDefinitions), () => VerifyFakeKraidProjectileInstructionProgramDefinitions());
        return 0;
    }
}
