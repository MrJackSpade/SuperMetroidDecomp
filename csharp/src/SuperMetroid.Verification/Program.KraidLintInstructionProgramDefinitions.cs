using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyKraidLintInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyKraidLintInstructionProgramDefinitions), () => VerifyKraidLintInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    private static void VerifyKraidLintInstructionProgramDefinitions(
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
                typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
                RoomEnemySlot lint = enemies.Slots[0];
                lint.EnemyDefinitionPointer = definition;
                lint.Definition = default(RoomEnemyDefinition) with { Bank = 0xa7 };
                lint.CurrentInstruction = program;
                lint.InstructionTimer = 1;

                MethodInfo process =
                    typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
                process.Invoke(
                    enemies,
                    [lint, null, null, Position16<RoomPixels>.Zero, (ushort)0]);
                VerifyExecutedEnemySelector(rom, lint, executedOperands);
                AssertEqual((ushort)0x7fff, lint.InstructionTimer,
                    $"Kraid lint ${(int)definition:X4}/${program:X4} installs native duration");
                AssertEqual(unchecked((ushort)(program + 4)), lint.CurrentInstruction,
                    $"Kraid lint ${(int)definition:X4}/${program:X4} advances to sleep");
                AssertEqual(ReadKraidLintInstructionWord(rom, 0xa70000 | program + 2),
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

    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid lint instruction words")]
    private static ushort ReadKraidLintInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        (ushort)(bus.ReadCartridgeByte(address) | bus.ReadCartridgeByte(address + 1) << 8);

    private static CartridgeReadGuard NewKraidLintInstructionGuard(ISnesAddressSpace source)
    {
        var guard = new CartridgeReadGuard(source)
            .Forbid(KraidLintInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                    address => $"Production read compiled Kraid lint mechanics byte ${address:X6}.");
        // Records every presentation word whose low or high byte is read.
        guard.Observe(address => (address & 0xff0000) == 0xa70000, address =>
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
}
