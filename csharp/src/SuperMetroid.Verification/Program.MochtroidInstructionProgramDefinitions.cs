using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Registers the retail-ROM verification suite for the compiled Mochtroid instruction programs.</summary>
    private static void VerifyMochtroidInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyMochtroidInstructionProgramDefinitions), () => VerifyMochtroidInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks the compiled mechanics words and runs the production initializer, list switch, and both instruction loops with guarded cartridge access.</summary>
    /// <param name="rom">Retail address space used for reference mechanics words and installed visual-selector checks.</param>
    private static void VerifyMochtroidInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < MochtroidInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                MochtroidInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadMochtroidInstructionWord(rom, definition.Address),
                $"Mochtroid mechanics word $A3:{definition.Address:X4}");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = new MochtroidInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        Type type = typeof(RoomEnemySystem);
        type.GetField("_bus", flags)!.SetValue(enemies, guard);
        var initialize = type.GetMethod("InitializeMochtroid", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        MethodInfo install = type.GetMethod(
            "SetMochtroidInstructionList",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        MethodInfo process = type.GetMethod("ProcessInstructions", flags)!;
        RoomEnemySlot mochtroid = enemies.Slots[0];
        mochtroid.EnemyDefinitionPointer = EnemyDefinitionPointers.Mochtroid;
        mochtroid.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };
        initialize(mochtroid);
        MochtroidEnemyState state = enemies.MochtroidStates[0]!;
        AssertEqual(MochtroidInstructionProgramDefinitions.FreeFlight,
            mochtroid.CurrentInstruction,
            "real Mochtroid initializer installs compiled free-flight loop");

        ExecuteMochtroidProgram(enemies, process, mochtroid, callCount: 5);
        AssertEqual(unchecked((ushort)(MochtroidInstructionProgramDefinitions.FreeFlight + 4)),
            mochtroid.CurrentInstruction,
            "Mochtroid free-flight program loops to its second frame");

        install.Invoke(null,
            [mochtroid, state, MochtroidInstructionProgramDefinitions.Attached]);
        AssertEqual(MochtroidInstructionProgramDefinitions.Attached,
            state.InstalledInstructionList,
            "production Mochtroid list switch installs attached loop");
        ExecuteMochtroidProgram(enemies, process, mochtroid, callCount: 5);
        AssertEqual(unchecked((ushort)(MochtroidInstructionProgramDefinitions.Attached + 4)),
            mochtroid.CurrentInstruction,
            "Mochtroid attached program loops to its second frame");

        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "Mochtroid execution uses compiled spritemap selectors");
        for (int index = 0;
             index < MochtroidInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort operand = MochtroidInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            AssertCompiledEnemyVisualSelector(rom, EnemyDefinitionPointers.Mochtroid,
                0xa3, operand, $"Mochtroid $A3:{operand:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Mochtroid mechanics byte");
        AssertThrows<InvalidDataException>(
            () => MochtroidInstructionProgramDefinitions.ReadMechanicsWord(
                MochtroidInstructionProgramDefinitionsTooling.PresentationWordAddress(0)),
            "Mochtroid spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => MochtroidInstructionProgramDefinitions.ReadMechanicsWord(
                MochtroidInstructionProgramDefinitions.FirstAdjacentMechanicsData),
            "adjacent Mochtroid shake data is rejected as instruction mechanics");

        _ = ProbeMochtroidInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeMochtroidInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Mochtroid allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Mochtroid mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Mochtroid instruction mechanics: twelve compiled words, the real initializer, " +
            "the production state switch, both complete loops, and eight compiled spritemap " +
            "selectors pass with mechanics bytes forbidden.");
    }

    /// <summary>Executes the selected Mochtroid instruction program through the production processor for a fixed number of eligible calls.</summary>
    /// <param name="enemies">Enemy system whose production processor is invoked.</param>
    /// <param name="process">Reflected production instruction-processing method.</param>
    /// <param name="mochtroid">Enemy slot whose instruction timer and list are advanced.</param>
    /// <param name="callCount">Number of processor calls to perform.</param>
    private static void ExecuteMochtroidProgram(
        RoomEnemySystem enemies,
        MethodInfo process,
        RoomEnemySlot mochtroid,
        int callCount)
    {
        object?[] arguments =
            [mochtroid, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        for (int call = 0; call < callCount; call++)
        {
            mochtroid.InstructionTimer = 1;
            process.Invoke(enemies, arguments);
        }
    }

    /// <summary>Warms and repeatedly reads both compiled instruction loops so the caller can measure steady-state lookup allocations.</summary>
    /// <returns>A checksum that keeps the lookup results observable.</returns>
    private static int ProbeMochtroidInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += MochtroidInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? MochtroidInstructionProgramDefinitions.FreeFlight
                    : MochtroidInstructionProgramDefinitions.Attached);
        }
        return checksum;
    }

    /// <summary>Reads a little-endian instruction mechanics word from bank $A3 for comparison with compiled data.</summary>
    /// <param name="source">Retail address space containing the original instruction list.</param>
    /// <param name="address">Bank-relative address of the low byte.</param>
    /// <returns>The adjacent cartridge bytes combined into an unsigned 16-bit word.</returns>
    private static ushort ReadMochtroidInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa30000 | address) |
            source.ReadByte(0xa30000 | unchecked((ushort)(address + 1))) << 8));

    /// <summary>Wraps cartridge access to reject reads of compiled Mochtroid mechanics and record any reads of compiled presentation operands.</summary>
    /// <param name="source">Underlying address space for permitted reads and writes.</param>
    private sealed class MochtroidInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Unique presentation operand addresses observed while production code runs.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];

        /// <summary>Number of attempted reads rejected for targeting compiled mechanics bytes.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes import reads through the same compiled-mechanics guard and presentation tracking as ordinary byte reads.</summary>
        /// <param name="address">Cartridge address requested by the importer.</param>
        /// <returns>The underlying byte when the address is not a compiled mechanics byte.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects compiled mechanics reads, records presentation operand reads, and forwards all other byte requests.</summary>
        /// <param name="address">Address-space location requested by production code.</param>
        /// <returns>The underlying byte when the address is not in a compiled mechanics range.</returns>
        /// <exception cref="InvalidOperationException">The request targets a compiled Mochtroid mechanics byte.</exception>
        public byte ReadByte(int address)
        {
            if (MochtroidInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Mochtroid mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa30000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < MochtroidInstructionProgramDefinitionsTooling.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        MochtroidInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
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

        /// <summary>Forwards writes without restriction; this wrapper observes and rejects reads only.</summary>
        /// <param name="address">Address-space location to write.</param>
        /// <param name="value">Byte value passed to the underlying source.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
