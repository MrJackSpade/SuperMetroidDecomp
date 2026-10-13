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
using static ImportedSpritemapsVerification;
using static KraidSmallProgramsVerification;
using static SuiteTimingVerification;

/// <summary>Checks Kraid lint instruction timing and sprite selection against the bank A7 cartridge programs.</summary>
internal static class KraidLintInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge to compare Kraid lint instructions with compiled definitions.</summary>
    internal static void VerifyKraidLintInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyKraidLintInstructionProgramDefinitions), () => VerifyKraidLintInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Runs both lint programs for all three lint positions and checks their duration, sprite, and cartridge access.</summary>
    internal static void VerifyKraidLintInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        Suite(nameof(VerifyKraidLintMechanicsMapping), () => VerifyKraidLintMechanicsMapping(rom));
        Suite(nameof(VerifyKraidLintPresentationMapping), () => VerifyKraidLintPresentationMapping());

        var guard = NewKraidLintInstructionGuard(rom);
        var executedOperands = new HashSet<ushort>();
        EnemyDefinitionId[] definitions =
        [
            EnemyDefinitionId.KraidLintTop,
            EnemyDefinitionId.KraidLintMiddle,
            EnemyDefinitionId.KraidLintBottom,
        ];
        ushort[] programs =
        [
            KraidLintInstructionProgramDefinitions.Initial,
            KraidLintInstructionProgramDefinitions.PostGrowth,
        ];
        foreach (EnemyDefinitionId definition in definitions)
        {
            foreach (ushort program in programs)
            {
                var enemies = new RoomEnemySystem();
                enemies.BindFixtureBus(guard);
                RoomEnemySlot lint = enemies.SlotTable.Slots[0];
                lint.EnemyDefinitionPointer = definition;
                lint.Definition = default(RoomEnemyDefinition) with { Bank = 0xa7 };
                lint.CurrentInstruction = program;
                lint.InstructionTimer = NativeCountdown.Start(1);

                enemies.Instructions.ProcessInstructions(lint, null, null, Position16<RoomPixels>.Zero, (ushort)0);
                VerifyExecutedEnemySelector(rom, lint, executedOperands);
                AssertEqual((ushort)0x7fff, lint.InstructionTimer.RawValue,
                    $"Kraid lint ${(int)definition:X4}/${program:X4} installs native duration");
                AssertEqual(unchecked((ushort)(program + 4)), lint.CurrentInstruction,
                    $"Kraid lint ${(int)definition:X4}/${program:X4} advances to sleep");
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa70000 | program + 2)),
                    lint.SpritemapPointer,
                    $"Kraid lint ${(int)definition:X4}/${program:X4} keeps native spritemap");
            }
        }

        AssertEqual(0, guard.ObservedWords.Count,
            "Enemy presentation performs zero live cartridge reads");
        AssertEqual(KraidLintInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count, "Every native visual operand executes");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Kraid lint mechanics byte");
        for (int index = 0;
             index < KraidLintInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                KraidLintInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"production execution selects Kraid lint presentation word $A7:{address:X4}");
            AssertThrows<InvalidDataException>(
                () => KraidLintInstructionProgramDefinitions.ReadMechanicsWord(address),
                $"Kraid lint spritemap $A7:{address:X4} is rejected as mechanics");
        }
        AssertThrows<InvalidDataException>(
            () => KraidLintInstructionProgramDefinitions.ReadMechanicsWord(
                KraidLintInstructionProgramDefinitions.FirstAdjacentFootProgram),
            "adjacent Kraid foot program is rejected as lint mechanics");

        _ = ProbeKraidLintInstructionMechanicsAllocation();
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeKraidLintInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Kraid lint allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - allocatedBefore,
            "warmed Kraid lint mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Kraid lint instruction mechanics: four compiled words, both programs, all " +
            "three lint definitions and two native sprite selections pass with mechanics " +
            "bytes forbidden.");
    }

    /// <summary>Repeatedly reads the compiled initial instruction to detect allocations in the mechanics lookup.</summary>
    private static int ProbeKraidLintInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += KraidLintInstructionProgramDefinitions.ReadMechanicsWord(
                KraidLintInstructionProgramDefinitions.Initial);
        }
        return checksum;
    }

    /// <summary>Rejects live mechanics reads while recording bank A7 presentation operand reads.</summary>
    private static CartridgeReadGuard NewKraidLintInstructionGuard(ISnesAddressSpace source)
    {
        var guard = new CartridgeReadGuard(source)
            .Forbid(KraidLintInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                    address => $"Production read compiled Kraid lint mechanics byte ${address:X6}.");
        // Records every presentation word whose low or high byte is read.
        guard.Observe(address => (SnesAddressMath.BankBase(address)) == 0xa70000, address =>
        {
            ushort bankAddress = unchecked((ushort)address);
            for (int index = 0; index < KraidLintInstructionProgramDefinitions.PresentationWordCount; index++)
            {
                ushort presentation = KraidLintInstructionProgramDefinitions.PresentationWordAddress(index);
                if (bankAddress == presentation || bankAddress == unchecked((ushort)(presentation + 1)))
                    guard.ObservedWords.Add(presentation);
            }
        });
        return guard;
    }

    /// <summary>Exposes the Kraid lint instruction checks to the verification command dispatcher.</summary>
    [VerificationCommand("--kraid-lint-instruction-mechanics")]
    internal static int KraidLintInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyKraidLintInstructionProgramDefinitions), () => VerifyKraidLintInstructionProgramDefinitions());
        return 0;
    }
}
