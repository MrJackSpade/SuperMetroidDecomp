using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Registers the retail-ROM verification suite for compiled Dachora instruction programs.</summary>
    private static void VerifyDachoraInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyDachoraInstructionProgramDefinitions), () => VerifyDachoraInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Compares compiled mechanics with the cartridge and runs every supported Dachora program through the production instruction processor while guarding migrated reads.</summary>
    /// <param name="rom">Retail address space used for reference words and selector verification.</param>
    private static void VerifyDachoraInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

        AssertEqual(110, DachoraInstructionProgramDefinitionsTooling.MechanicsWordCount,
            "Dachora compiled mechanics word count");
        AssertEqual(81, DachoraInstructionProgramDefinitions.PresentationWordCount,
            "Dachora live presentation word count");
        for (int index = 0;
             index < DachoraInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                DachoraInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadDachoraInstructionWord(rom, definition.Address),
                $"Dachora mechanics word $A7:{definition.Address:X4}");
        }

        var executedOperands = new HashSet<ushort>();
        var guard = new DachoraInstructionReadGuard(rom);
        MethodInfo initialize = typeof(RoomEnemySystem).GetMethod("InitializeDachora", flags)!;
        (ushort Parameter, ushort Expected)[] initializerCases =
        [
            (0, DachoraInstructionProgramDefinitions.IdleLeft),
            (1, DachoraInstructionProgramDefinitions.IdleRight),
            (0xfffe, DachoraInstructionProgramDefinitions.EchoLeft),
            (0xffff, DachoraInstructionProgramDefinitions.EchoRight),
        ];
        foreach ((ushort parameter, ushort expected) in initializerCases)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.DachoraDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa7 };
            slot.Parameter1 = parameter;
            initialize.Invoke(enemies, [slot]);
            AssertEqual(expected, slot.CurrentInstruction,
                $"Dachora initializer parameter ${parameter:X4}");
        }

        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        for (int index = 0;
             index < DachoraInstructionProgramDefinitions.ProgramCount;
             index++)
        {
            DachoraInstructionProgram program =
                DachoraInstructionProgramDefinitions.Program(index);
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.DachoraDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa7 };
            slot.CurrentInstruction = program.Entry;
            ExecuteDachoraProgram(rom, executedOperands,
                enemies,
                process,
                slot,
                program.FrameCount + 1);
            AssertEqual(unchecked((ushort)(program.Entry + 4)), slot.CurrentInstruction,
                program.Loops
                    ? $"Dachora program $A7:{program.Entry:X4} loops through every frame"
                    : $"Dachora program $A7:{program.Entry:X4} reaches terminal sleep");
        }

        AssertEqual(unchecked((ushort)(DachoraInstructionProgramDefinitions.RunningLeft + 0x1c)),
            DachoraInstructionProgramDefinitions.RunningLeftFast,
            "Dachora left speed-up preserves native in-place cursor offset");
        AssertEqual(unchecked((ushort)(DachoraInstructionProgramDefinitions.RunningLeftFast + 0x1c)),
            DachoraInstructionProgramDefinitions.RunningLeftVeryFast,
            "Dachora left second speed-up preserves native in-place cursor offset");
        AssertEqual(unchecked((ushort)(DachoraInstructionProgramDefinitions.RunningRight + 0x1c)),
            DachoraInstructionProgramDefinitions.RunningRightFast,
            "Dachora right speed-up preserves native in-place cursor offset");
        AssertEqual(unchecked((ushort)(DachoraInstructionProgramDefinitions.RunningRightFast + 0x1c)),
            DachoraInstructionProgramDefinitions.RunningRightVeryFast,
            "Dachora right second speed-up preserves native in-place cursor offset");

        AssertEqual(DachoraInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count,
            "all executed visual selectors match the cartridge");
        for (int index = 0;
             index < DachoraInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                DachoraInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"production execution covers Dachora presentation word $A7:{address:X4}");
        }
        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "compiled visual selectors require no runtime cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Dachora mechanics byte");

        AssertThrows<InvalidDataException>(
            () => DachoraInstructionProgramDefinitions.ReadMechanicsWord(
                DachoraInstructionProgramDefinitions.PresentationWordAddress(0)),
            "Dachora spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => DachoraInstructionProgramDefinitions.ReadMechanicsWord(
                DachoraInstructionProgramDefinitions.UnusedChargeLeft),
            "retail-unused left charge program is outside production mechanics");

        _ = ProbeDachoraInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeDachoraInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Dachora allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Dachora mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Dachora instruction mechanics: 110 compiled words, all fifteen body/echo " +
            "programs and speed tiers, and 81 executed selectors match cartridge data with runtime " +
            "ROM reads forbidden.");
    }

    /// <summary>Runs a program for the requested number of instruction calls and records each presentation selector encountered.</summary>
    /// <param name="rom">Address space used to verify the executed selector against its native operand.</param>
    /// <param name="executedOperands">Set receiving each verified selector address.</param>
    /// <param name="enemies">Enemy system whose production instruction processor executes the program.</param>
    /// <param name="process">Reflected production instruction-processing method.</param>
    /// <param name="slot">Enemy slot initialized at the selected program entry.</param>
    /// <param name="callCount">Number of eligible instruction calls to perform.</param>
    private static void ExecuteDachoraProgram(
        ISnesAddressSpace rom,
        HashSet<ushort> executedOperands,
        RoomEnemySystem enemies,
        MethodInfo process,
        RoomEnemySlot slot,
        int callCount)
    {
        object?[] arguments =
            [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        for (int call = 0; call < callCount; call++)
        {
            slot.InstructionTimer = 1;
            process.Invoke(enemies, arguments);
            VerifyExecutedEnemySelector(rom, slot, executedOperands);
        }
    }

    /// <summary>Warms and repeatedly reads compiled Dachora mechanics entries so steady-state lookup allocations can be measured by the caller.</summary>
    /// <returns>A checksum that keeps the repeated lookup results observable.</returns>
    private static int ProbeDachoraInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += DachoraInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? DachoraInstructionProgramDefinitions.RunningLeft
                    : DachoraInstructionProgramDefinitions.FallingRight);
        }
        return checksum;
    }

    /// <summary>Reads one little-endian mechanics word from bank $A7 for comparison with its compiled definition.</summary>
    /// <param name="source">Retail address space containing the original instruction data.</param>
    /// <param name="address">Bank-relative address of the low byte.</param>
    /// <returns>The adjacent cartridge bytes combined as an unsigned 16-bit word.</returns>
    private static ushort ReadDachoraInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa70000 | address) |
            source.ReadByte(0xa70000 | unchecked((ushort)(address + 1))) << 8));

    /// <summary>Guards production cartridge access against rereading compiled Dachora mechanics while recording use of presentation operands.</summary>
    /// <param name="source">Underlying address space for permitted reads and writes.</param>
    private sealed class DachoraInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Unique bank-relative presentation operand addresses observed during production execution.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];

        /// <summary>Number of rejected reads into the compiled Dachora mechanics ranges.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes import-source reads through the same mechanics guard and presentation tracking as ordinary reads.</summary>
        /// <param name="address">Cartridge address requested by the importer.</param>
        /// <returns>The underlying byte when it is not part of compiled mechanics.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads of compiled mechanics, records matching presentation operand accesses, and forwards other reads.</summary>
        /// <param name="address">Address-space location requested by production code.</param>
        /// <returns>The underlying byte when the address is not in a guarded mechanics range.</returns>
        /// <exception cref="InvalidOperationException">The address identifies a compiled mechanics byte.</exception>
        public byte ReadByte(int address)
        {
            if (DachoraInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Dachora mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa70000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < DachoraInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        DachoraInstructionProgramDefinitions.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        ObservedPresentationWords.Add(presentation);
                        break;
                    }
                }
            }
            return source.ReadByte(address);
        }

        /// <summary>Forwards writes unchanged; this guard tracks and rejects reads only.</summary>
        /// <param name="address">Address-space location to write.</param>
        /// <param name="value">Byte value passed to the underlying source.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
