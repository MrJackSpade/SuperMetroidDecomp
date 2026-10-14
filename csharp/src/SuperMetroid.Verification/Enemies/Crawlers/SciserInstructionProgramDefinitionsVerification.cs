using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.Core.Programs;
namespace SuperMetroid.Verification.Enemies.Crawlers;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Sciser's four surface animation loops and compiled instruction data against bank $A3.</summary>
internal static class SciserInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the pinned cartridge and runs the Sciser instruction comparison.</summary>
    internal static void VerifySciserInstructionProgramDefinitions()
    {
        Suite(nameof(VerifySciserInstructionProgramDefinitions), () => VerifySciserInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks native words, live crawler handoffs, read boundaries, and warmed lookup allocation.</summary>
    internal static void VerifySciserInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

        for (int index = 0;
             index < SciserInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                SciserInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadSciserInstructionWord(rom, definition.Address),
                $"Sciser mechanics word $A3:{definition.Address:X4}");
        }

        var guard = NewSciserInstructionReadGuard(rom);
        foreach (CrawlerSurfaceOrientation orientation in Enum.GetValues<CrawlerSurfaceOrientation>())
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guard);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.EnemyDefinitionPointer = EnemyDefinitionId.Sciser;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };
            slot.CurrentInstruction = (ushort)orientation;
            slot.Parameter1 = 0;
            enemies.CrawlersBehavior.InitializeCrawler(slot, CrawlerAnimationFamily.Sciser, (ushort?)8);

            ushort entry = CrawlerAnimationDefinitions.InitialInstruction(
                CrawlerAnimationFamily.Sciser, orientation);
            AssertEqual(entry, slot.CurrentInstruction,
                $"Sciser real initializer selects {orientation}");
            ExecuteSciserProgram(enemies, slot, callCount: 5);
            CrawlerEnemyFunction expected = orientation is
                CrawlerSurfaceOrientation.UpsideRight or
                CrawlerSurfaceOrientation.UpsideLeft
                    ? CrawlerEnemyFunction.CrawlingVertically
                    : CrawlerEnemyFunction.CrawlingHorizontally;
            AssertEqual(expected, (enemies.CrawlerStates[0] ?? throw new InvalidOperationException("Verification requires enemies.CrawlerStates[0].")).Function,
                $"Sciser {orientation} publishes native movement function");
            AssertEqual(unchecked((ushort)(entry + 8)), slot.CurrentInstruction,
                $"Sciser {orientation} completes and loops all four frames");
        }

        AssertEqual(0, guard.ObservedWords.Count,
            "Sciser instruction execution uses compiled spritemap selectors");
        for (int index = 0;
             index < SciserInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort address =
                SciserInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            AssertTrue(!guard.ObservedWords.Contains(address),
                $"production execution avoids Sciser presentation word $A3:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Sciser mechanics byte");

        AssertThrows<InvalidDataException>(
            () => SciserInstructionProgramDefinitions.ReadMechanicsWord(
                SciserInstructionProgramDefinitionsTooling.PresentationWordAddress(0)),
            "Sciser spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => SciserInstructionProgramDefinitions.ReadMechanicsWord(
                SciserInstructionProgramDefinitions.AdjacentPreviousCode),
            "adjacent pre-Sciser code is rejected as mechanics");

        _ = ProbeSciserInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeSciserInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Sciser allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Sciser mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Sciser instruction mechanics: thirty-two compiled words, all four surface " +
            "loops and movement callbacks, and sixteen compiled spritemap selectors pass " +
            "with both instruction and presentation ROM reads forbidden.");
    }

    /// <summary>Advances the real enemy instruction runner through a requested number of Sciser steps.</summary>
    private static void ExecuteSciserProgram(
        RoomEnemySystem enemies,
        RoomEnemySlot slot,
        int callCount)
    {
        object?[] arguments =
            [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
        for (int call = 0; call < callCount; call++)
        {
            slot.InstructionTimer = NativeCountdown.Start(1);
            EnemyInstructionRunner.Process(enemies, arguments);
        }
    }

    /// <summary>Repeats warmed Sciser control-word lookups for the caller's allocation measurement.</summary>
    private static int ProbeSciserInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += SciserInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? SciserInstructionProgramDefinitions.UpsideRight
                    : SciserInstructionProgramDefinitions.UpsideUp);
        }
        return checksum;
    }

    /// <summary>Reads a bank $A3 instruction word from the pinned cartridge for comparison.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Sciser instruction words")]
    internal static ushort ReadSciserInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank(0xa30000 | address);

    /// <summary>Forbids compiled mechanics rereads and observes native spritemap operand access.</summary>
    private static CartridgeReadGuard NewSciserInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(SciserInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Sciser mechanics byte ${address:X6}.")
            .ObserveWords(0xa30000, SciserInstructionProgramDefinitionsTooling.PresentationWordCount, SciserInstructionProgramDefinitionsTooling.PresentationWordAddress);

    /// <summary>Dispatches the Sciser instruction-program verification command.</summary>
    [VerificationCommand("--sciser-instruction-program-definitions")]
    internal static int SciserInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifySciserInstructionProgramDefinitions), () => VerifySciserInstructionProgramDefinitions());
        return 0;
    }
}
