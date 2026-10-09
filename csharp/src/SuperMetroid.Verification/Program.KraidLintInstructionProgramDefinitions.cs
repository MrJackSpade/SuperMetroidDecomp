using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Registers the retail-ROM verification suite for Kraid lint instruction programs.</summary>
    private static void VerifyKraidLintInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyKraidLintInstructionProgramDefinitions), () => VerifyKraidLintInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Compares all three lint definitions and both instruction programs with cartridge data while verifying production execution uses compiled mechanics and selectors.</summary>
    /// <param name="rom">Retail address space containing the native instruction words and presentation operands used as references.</param>
    private static void VerifyKraidLintInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        Suite(nameof(VerifyKraidLintMechanicsMapping), () => VerifyKraidLintMechanicsMapping(rom));
        Suite(nameof(VerifyKraidLintPresentationMapping), () => VerifyKraidLintPresentationMapping());

        var guard = new KraidLintInstructionReadGuard(rom);
        var executedOperands = new HashSet<ushort>();
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
                VerifyExecutedEnemySelector(rom, lint, executedOperands);
                AssertEqual((ushort)0x7fff, lint.InstructionTimer,
                    $"Kraid lint ${definition:X4}/${program:X4} installs native duration");
                AssertEqual(unchecked((ushort)(program + 4)), lint.CurrentInstruction,
                    $"Kraid lint ${definition:X4}/${program:X4} advances to sleep");
                AssertEqual(ReadKraidLintInstructionWord(rom, 0xa70000 | program + 2),
                    lint.SpritemapPointer,
                    $"Kraid lint ${definition:X4}/${program:X4} keeps native spritemap");
            }
        }

        AssertEqual(0, guard.ObservedPresentationWords.Count,
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

    /// <summary>Warms and repeatedly reads a compiled Kraid lint instruction word so steady-state lookup allocations can be measured.</summary>
    /// <returns>A checksum that keeps the repeated mechanics lookups observable.</returns>
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

    /// <summary>Reads a little-endian instruction word from the cartridge address space for comparison with compiled values.</summary>
    /// <param name="bus">Retail address space containing the original Kraid lint program.</param>
    /// <param name="address">Address of the low byte; the high byte follows immediately.</param>
    /// <returns>The two bytes combined as an unsigned 16-bit word.</returns>
    private static ushort ReadKraidLintInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    /// <summary>Wraps cartridge access to reject compiled Kraid lint mechanics reads and track reads of compiled presentation operands.</summary>
    /// <param name="source">Underlying address space for permitted reads and writes.</param>
    private sealed class KraidLintInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Unique presentation operand addresses requested during production execution.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];

        /// <summary>Number of attempted reads rejected for targeting compiled Kraid lint mechanics.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes import-source reads through the same compiled-mechanics guard as ordinary reads.</summary>
        /// <param name="address">Cartridge address requested by the importer.</param>
        /// <returns>The underlying byte when the address is outside the compiled mechanics range.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads of compiled mechanics, records presentation operand accesses, and forwards other byte requests.</summary>
        /// <param name="address">Address-space location requested by production code.</param>
        /// <returns>The underlying byte when the address is outside the compiled mechanics range.</returns>
        /// <exception cref="InvalidOperationException">The address identifies a compiled Kraid lint mechanics byte.</exception>
        public byte ReadByte(int address)
        {
            if (KraidLintInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
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

        /// <summary>Forwards writes unchanged; this guard only restricts reads from compiled mechanics.</summary>
        /// <param name="address">Address-space location to write.</param>
        /// <param name="value">Byte value passed through to the underlying source.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
