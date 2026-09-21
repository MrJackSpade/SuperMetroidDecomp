using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyKraidLintInstructionProgramDefinitions()
    {
        VerifyKraidLintInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyKraidLintInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        for (int index = 0;
             index < KraidLintInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            KraidLintInstructionMechanicsWord definition =
                KraidLintInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadKraidLintInstructionWord(rom, 0xa70000 | definition.Address),
                $"Kraid lint instruction mechanics word $A7:{definition.Address:X4}");
        }

        var guard = new KraidLintInstructionReadGuard(rom);
        ushort[] definitions =
        [
            RoomEnemySystem.KraidTopLintDefinition,
            RoomEnemySystem.KraidMiddleLintDefinition,
            RoomEnemySystem.KraidBottomLintDefinition,
        ];
        ushort[] programs =
        [
            KraidLintInstructionProgramDefinitions.Initial,
            KraidLintInstructionProgramDefinitions.PostGrowth,
        ];
        foreach (ushort definition in definitions)
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
                    [lint, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0]);
                AssertEqual((ushort)0x7fff, lint.InstructionTimer,
                    $"Kraid lint ${definition:X4}/${program:X4} installs native duration");
                AssertEqual(unchecked((ushort)(program + 4)), lint.CurrentInstruction,
                    $"Kraid lint ${definition:X4}/${program:X4} advances to sleep");
                AssertEqual(ReadKraidLintInstructionWord(rom, 0xa70000 | program + 2),
                    lint.SpritemapPointer,
                    $"Kraid lint ${definition:X4}/${program:X4} keeps live spritemap");
            }
        }

        AssertEqual(KraidLintInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "both Kraid lint spritemaps remain cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Kraid lint mechanics byte");
        for (int index = 0;
             index < KraidLintInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                KraidLintInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Kraid lint presentation word $A7:{address:X4}");
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
            "three lint definitions and two live spritemap reads pass with mechanics " +
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

    private static ushort ReadKraidLintInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class KraidLintInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (KraidLintInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Kraid lint mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa70000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < KraidLintInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        KraidLintInstructionProgramDefinitions.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        ObservedPresentationWords.Add(presentation);
                    }
                }
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
