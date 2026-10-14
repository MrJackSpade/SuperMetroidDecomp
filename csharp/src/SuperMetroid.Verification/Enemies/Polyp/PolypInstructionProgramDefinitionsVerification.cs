using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Polyp;
using SuperMetroid.Core.Programs;
namespace SuperMetroid.Verification.Enemies.Polyp;
using SuperMetroid.Verification.Harness;

/// <summary>Checks compiled Polyp control words and live frame selection without runtime ROM reads.</summary>
internal static class PolypInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail source and runs the Polyp instruction comparison.</summary>
    internal static void VerifyPolypInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyPolypInstructionProgramDefinitions), () => VerifyPolypInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks native words, real initializer and interpreter behavior, forbidden reads, and allocation.</summary>
    internal static void VerifyPolypInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < PolypInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                PolypInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa20000 | definition.Address),
                $"Polyp mechanics word $A2:{definition.Address:X4}");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = NewPolypInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        Type type = typeof(RoomEnemySystem);
        enemies.BindFixtureBus(guard);
        enemies.VerificationServices().SeedRandom = (Action<ushort>)(_ => { });
        var initialize = new Action<RoomEnemySlot>(enemies.PolypBehavior.InitializePolyp);

        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.EnemyDefinitionPointer = EnemyDefinitionId.LavaRocks;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        initialize(slot);
        AssertEqual(PolypInstructionProgramDefinitions.Stationary,
            slot.CurrentInstruction,
            "real Polyp initializer installs compiled stationary program");

        object?[] processArguments =
            [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
        slot.InstructionTimer = NativeCountdown.Start(1);
        EnemyInstructionRunner.Process(enemies, processArguments);
        AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa20000 | PolypInstructionProgramDefinitions.PresentationWord), slot.SpritemapPointer,
            "Polyp selects its exact native frame without cartridge reads");
        slot.InstructionTimer = NativeCountdown.Start(1);
        EnemyInstructionRunner.Process(enemies, processArguments);
        AssertEqual(unchecked((ushort)(PolypInstructionProgramDefinitions.Stationary + 4)),
            slot.CurrentInstruction,
            "Polyp program reaches terminal sleep");
        AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa20000 | PolypInstructionProgramDefinitions.PresentationWord),
            slot.SpritemapPointer, "Actual Polyp stationary sprite matches the native operand");
        AssertEqual(slot.SpritemapPointer,
            PolypInstructionProgramDefinitions.FrameAt(PolypInstructionProgramDefinitions.PresentationWord),
            "Named Polyp selector installed by the actual program");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids both compiled Polyp mechanics words");
        AssertThrows<InvalidDataException>(
            () => PolypInstructionProgramDefinitions.ReadMechanicsWord(
                PolypInstructionProgramDefinitions.PresentationWord),
            "Polyp spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => PolypInstructionProgramDefinitions.ReadMechanicsWord(0xb520),
            "adjacent cooldown table is rejected as instruction mechanics");

        _ = ProbePolypInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbePolypInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Polyp allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Polyp mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Polyp instruction mechanics: two compiled words, the real initializer, " +
            "terminal sleep, and exact frame selection pass with instruction bytes forbidden.");
    }

    /// <summary>Repeatedly reads both compiled Polyp controls to detect per-frame allocations.</summary>
    private static int ProbePolypInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += PolypInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? PolypInstructionProgramDefinitions.Stationary
                    : (ushort)0xb51e);
        }
        return checksum;
    }

    /// <summary>Rejects cartridge reads of compiled control words or the selected visual operand.</summary>
    private static CartridgeReadGuard NewPolypInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(PolypInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Polyp mechanics byte ${address:X6}.")
            .Forbid(address => address - (0xa20000 | PolypInstructionProgramDefinitions.PresentationWord) is 0 or 1,
                _ => "Production read the compiled Polyp visual operand.");

    /// <summary>Runs the focused Polyp mechanics verification command.</summary>
    [VerificationCommand("--polyp-instruction-mechanics")]
    internal static int PolypInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyPolypInstructionProgramDefinitions), () => VerifyPolypInstructionProgramDefinitions());
        return 0;
    }
}
