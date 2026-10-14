using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.Cacatac;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Cacatac spike projectile programs, live spawns, and presentation selectors against bank $86.</summary>
internal static class CacatacProjectileInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the pinned cartridge and runs the Cacatac projectile comparison.</summary>
    internal static void VerifyCacatacProjectileInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyCacatacProjectileInstructionProgramDefinitions), () => VerifyCacatacProjectileInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks all ten directional programs through spawn, frame install, terminal sleep, and read guards.</summary>
    internal static void VerifyCacatacProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < CacatacProjectileInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                CacatacProjectileInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWord((0x860000 | definition.Address)),
                $"Cacatac spike instruction mechanics word $86:{definition.Address:X4}");
        }

        var guard = NewCacatacProjectileInstructionProgramReadGuard(rom);
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(guard);
        Action<RoomEnemySlot, ushort> spawn = enemies.CacatacBehavior.SpawnCacatacSpike;
        RoomEnemySlot source = enemies.SlotTable.Slots[0];
        source.Position = source.Position.WithWhole(new(0x1234, 0x5678));

        foreach (CacatacSpikeDirection direction in Enum.GetValues<CacatacSpikeDirection>())
        {
            foreach (RoomEnemyProjectileSlot projectile in enemies.Projectiles.EnemyProjectiles)
                projectile.Clear();

            spawn(source, (ushort)direction);
            RoomEnemyProjectileSlot actual = enemies.Projectiles.EnemyProjectiles.Single(
                projectile => projectile.Kind == RoomEnemyProjectileKind.CacatacSpike);
            ushort program = CacatacProjectileDefinitions.InstructionList(direction);
            AssertEqual(program, actual.InstructionPointer,
                $"Cacatac spike {direction} begins at its named program");

            enemies.Projectiles.ProcessEnemyProjectileInstructions(actual, (SamusState?)null, Position16<RoomPixels>.Zero);
            AssertEqual(unchecked((ushort)(program + 4)), actual.InstructionPointer,
                $"Cacatac spike {direction} installs its live spritemap");
            AssertEqual(unchecked((ushort)(program + 2)),
                actual.PresentationOperandAddress,
                $"Cacatac spike {direction} selects its installed cartridge frame");

            enemies.Projectiles.ProcessEnemyProjectileInstructions(actual, (SamusState?)null, Position16<RoomPixels>.Zero);
            AssertEqual(unchecked((ushort)(program + 4)), actual.InstructionPointer,
                $"Cacatac spike {direction} sleeps at its terminal instruction");
            AssertEqual((ushort)0, actual.InstructionTimer.RawValue,
                $"Cacatac spike {direction} terminal sleep is stable");
        }

        AssertEqual(0, guard.ObservedWords.Count,
            "Cacatac spike frames use installed presentation without cartridge reads");
        for (int index = 0;
             index < CacatacProjectileInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                CacatacProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(!guard.ObservedWords.Contains(address),
                $"production execution avoids Cacatac spike presentation $86:{address:X4}");
        }

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled Cacatac spike mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => CacatacProjectileInstructionProgramDefinitions.ReadMechanicsWord(0xd930),
            "Cacatac spike spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => CacatacProjectileInstructionProgramDefinitions.ReadMechanicsWord(0xd96a),
            "adjacent Cacatac spike selector table is rejected as mechanics");

        _ = ProbeCacatacProjectileInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeCacatacProjectileInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Cacatac spike allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Cacatac spike mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Cacatac spike instruction mechanics: twenty compiled words, ten complete " +
            "production programs, and ten installed frame selectors pass with cartridge bytes " +
            "forbidden.");
    }

    /// <summary>Repeats warmed spike instruction lookups for the caller's allocation measurement.</summary>
    private static int ProbeCacatacProjectileInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += CacatacProjectileInstructionProgramDefinitions.ReadMechanicsWord(
                CacatacProjectileInstructionProgramDefinitions.LeftFacingUp);
        }
        return checksum;
    }

    /// <summary>Forbids compiled mechanics rereads and observes native presentation word access.</summary>
    private static CartridgeReadGuard NewCacatacProjectileInstructionProgramReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(CacatacProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Cacatac spike mechanics ${address:X6}.")
            .ObserveWords(0x860000, CacatacProjectileInstructionProgramDefinitions.PresentationWordCount, CacatacProjectileInstructionProgramDefinitions.PresentationWordAddress);

    /// <summary>Runs the cartridge-backed Cacatac fixture through its short command alias.</summary>
    [VerificationCommand("--cacatac-fixture")]
    internal static int CacatacFixtureCommand()
    {
     VerifyCacatacProjectileInstructionProgramDefinitions(); return 0; 
    }

    /// <summary>Dispatches the Cacatac projectile instruction mechanics verification command.</summary>
    [VerificationCommand("--cacatac-projectile-instruction-mechanics")]
    internal static int CacatacProjectileInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyCacatacProjectileInstructionProgramDefinitions), () => VerifyCacatacProjectileInstructionProgramDefinitions());
        return 0;
    }
}
