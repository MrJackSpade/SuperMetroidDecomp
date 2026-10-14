using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Powamp;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.Powamp;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Powamp spike animation loops and private deletion across all eight burst directions.</summary>
internal static class PowampSpikeInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail ROM and runs the Powamp spike instruction comparison.</summary>
    internal static void VerifyPowampSpikeInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyPowampSpikeInstructionProgramDefinitions), () => VerifyPowampSpikeInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks live burst frames, installed visual operands, collision deletion, and forbidden mechanics reads.</summary>
    internal static void VerifyPowampSpikeInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        const BindingFlags staticFlags = BindingFlags.Static | BindingFlags.NonPublic;
        for (int index = 0;
             index < PowampSpikeInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                PowampSpikeInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | definition.Address),
                $"Powamp-spike mechanics word $86:{definition.Address:X4}");
        }

        var guard = NewPowampSpikeInstructionReadGuard(rom);
        var selectedPresentationWords = new HashSet<ushort>();
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(guard);

        var body = new RoomEnemySlot(0)
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(128, 112)),
            VramTilesIndex = 0x0200,
            PaletteIndex = 0x0c00,
        };
        enemies.PowampBehavior.SpawnPowampSpikeBurst(body);

        RoomEnemyProjectileSlot[] spikes = enemies.Projectiles.EnemyProjectiles
            .Where(projectile => projectile.Kind == RoomEnemyProjectileKind.PowampSpike)
            .ToArray();
        AssertEqual(8, spikes.Length,
            "real Powamp burst producer spawns all eight projectile directions");
        AssertTrue(spikes.Select(spike => spike.DirectionParameter).Order().SequenceEqual(
                Enumerable.Range(0, 8).Select(value => (ushort)value)),
            "real Powamp burst owns every native direction exactly once");
        foreach (RoomEnemyProjectileSlot spike in spikes)
        {
            AssertEqual(PowampSpikeInstructionProgramDefinitions.Initial,
                spike.InstructionPointer,
                "real Powamp spike producer selects the named animation loop");
            RunForcedTicks(spike, 4);
            AssertEqual(
                unchecked((ushort)(PowampSpikeInstructionProgramDefinitions.Initial + 4)),
                spike.InstructionPointer,
                "Powamp spike completes all three frames and loops to its first frame");
            AssertTrue(spike.IsActive,
                "Powamp spike animation remains active until room collision deletes it");
        }

        PowampBehavior.BeginPowampSpikeDeletion(spikes[0]);
        AssertEqual(PowampSpikeInstructionProgramDefinitions.Delete,
            spikes[0].InstructionPointer,
            "production collision handoff installs the named private delete list");
        RunForcedTicks(spikes[0], 1);
        AssertTrue(!spikes[0].IsActive,
            "Powamp spike private delete list clears the projectile");

        AssertEqual(PowampSpikeInstructionProgramDefinitions.PresentationWordCount,
            selectedPresentationWords.Count,
            "all live Powamp-spike frames select installed operands");
        for (int index = 0;
             index < PowampSpikeInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = PowampSpikeInstructionProgramDefinitions
                .PresentationWordAddress(index);
            AssertTrue(selectedPresentationWords.Contains(address),
                $"production execution selects Powamp-spike presentation $86:{address:X4}");
        }

        AssertEqual(0, guard.ObservedWords.Count,
            "Powamp-spike frames never read presentation operands from ROM");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Powamp-spike mechanics byte");
        AssertThrows<InvalidDataException>(
            () => PowampSpikeInstructionProgramDefinitions.ReadMechanicsWord(0xd20a),
            "Powamp-spike spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => PowampSpikeInstructionProgramDefinitions.ReadMechanicsWord(0xd21a),
            "adjacent Powamp-spike velocity table is rejected as mechanics");

        _ = ProbePowampSpikeInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbePowampSpikeInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Powamp-spike allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Powamp-spike mechanics lookups allocate no storage");

        Console.WriteLine(
            "Powamp-spike instruction mechanics: six compiled words, all eight real burst " +
            "directions, complete loops, private collision deletion, and three live " +
            "installed frame selectors pass without instruction ROM reads.");

        void RunForcedTicks(RoomEnemyProjectileSlot projectile, int count)
        {
            for (int tick = 0; tick < count; tick++)
            {
                projectile.InstructionTimer = NativeCountdown.Start(1);
                enemies.Projectiles.ProcessEnemyProjectileInstructions(projectile, new SamusState(), Position16<RoomPixels>.Zero);
                if (projectile.IsActive && !projectile.InstructionTimer.IsZero)
                {
                    ushort operand = (ushort)(projectile.InstructionPointer - 2);
                    AssertEqual(operand, projectile.PresentationOperandAddress,
                        "Powamp spike selects the just-executed native frame operand");
                    AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | ((ushort)(operand - 2))),
                        projectile.InstructionTimer.RawValue, "Powamp spike retains the native frame duration");
                    selectedPresentationWords.Add(projectile.PresentationOperandAddress);
                }
            }
        }
    }

    /// <summary>Alternates animation and delete control words to detect warmed lookup allocations.</summary>
    private static int ProbePowampSpikeInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += PowampSpikeInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? PowampSpikeInstructionProgramDefinitions.Initial
                    : PowampSpikeInstructionProgramDefinitions.Delete);
        }
        return checksum;
    }

    /// <summary>Blocks migrated spike mechanics and records access to visual operands.</summary>
    private static CartridgeReadGuard NewPowampSpikeInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(PowampSpikeInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Powamp-spike mechanics byte ${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase, PowampSpikeInstructionProgramDefinitions.PresentationWordCount,
                PowampSpikeInstructionProgramDefinitions.PresentationWordAddress);

    /// <summary>Runs the focused Powamp spike instruction check against the retail ROM.</summary>
    [VerificationCommand("--powamp-spike-instruction-mechanics")]
    internal static int PowampSpikeInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyPowampSpikeInstructionProgramDefinitions), () => VerifyPowampSpikeInstructionProgramDefinitions());
        return 0;
    }
}
