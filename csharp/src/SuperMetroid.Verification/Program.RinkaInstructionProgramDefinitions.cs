using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Compares both Rinka programs with the pinned ROM, then executes them through the
    /// production interpreter while every compiled mechanics byte is unavailable.
    /// </summary>
    private static void VerifyRinkaInstructionProgramDefinitions()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine(
                "  Rinka instruction mechanics: cartridge comparison skipped " +
                "(private ROM absent).");
            return;
        }

        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        for (int index = 0;
             index < RinkaInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                RinkaInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadRinkaProgramWord(rom, definition.Address),
                $"Rinka mechanics word $A2:{definition.Address:X4}");
        }

        var guarded = new RinkaInstructionReadGuard(rom);
        (RoomEnemySystem ordinarySystem, RoomEnemySlot ordinary) = RunRinkaProgram(
            guarded,
            RinkaInstructionProgramDefinitions.OrdinaryInitial,
            special: false);
        AssertEqual(RinkaEnemyFunction.AimDelay, ordinarySystem.RinkaStates[0]!.Function,
            "ordinary Rinka fire callback selects aim-delay AI");
        AssertTrue(!ordinary.Properties.HasAny(EnemyProperties.Invisible),
            "ordinary Rinka fire callback makes the actor visible");
        AssertTrue(!ordinary.Properties.HasAny(EnemyProperties.ProcessOffScreen),
            "ordinary Rinka program does not enable off-screen processing");

        (RoomEnemySystem specialSystem, RoomEnemySlot special) = RunRinkaProgram(
            guarded,
            RinkaInstructionProgramDefinitions.SpecialInitial,
            special: true);
        AssertEqual(RinkaEnemyFunction.AimDelay, specialSystem.RinkaStates[0]!.Function,
            "special Rinka fire callback selects aim-delay AI");
        AssertTrue(!special.Properties.HasAny(EnemyProperties.Invisible),
            "special Rinka fire callback makes the actor visible");
        AssertTrue(special.Properties.HasAny(EnemyProperties.ProcessOffScreen),
            "special Rinka setup retains off-screen processing after firing");

        AssertEqual(0, guarded.ObservedPresentationWords.Count,
            "Rinka execution uses compiled spritemap selectors");
        for (int index = 0;
             index < RinkaInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                RinkaInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertCompiledEnemyVisualSelector(rom, RoomEnemySystem.RinkaDefinition,
                0xa2, address, $"Rinka $A2:{address:X4}");
        }
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "production execution avoids every compiled Rinka mechanics byte");

        AssertThrows<InvalidDataException>(
            () => RinkaInstructionProgramDefinitions.ReadMechanicsWord(0xb9e4),
            "interleaved Rinka spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => RinkaInstructionProgramDefinitions.ReadMechanicsWord(
                RinkaInstructionCodes.UNUSED_Instruction_Rinka_GotoYIfCounterGreaterThan2_A2B9A2),
            "unused Rinka conditional callback cannot acquire an invented operand");
        AssertThrows<InvalidDataException>(
            () => RinkaInstructionProgramDefinitions.ReadMechanicsWord(0xffff),
            "restored pointer outside the translated Rinka programs fails loudly");

        _ = RinkaInstructionProgramDefinitions.ReadMechanicsWord(
            RinkaInstructionProgramDefinitions.OrdinaryInitial);
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += RinkaInstructionProgramDefinitions.ReadMechanicsWord(
                RinkaInstructionProgramDefinitions.OrdinaryInitial);
        }
        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        AssertTrue(checksum != 0, "Rinka allocation probe consumes live data");
        AssertEqual(0L, allocated,
            "warmed Rinka mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            $"  Rinka instruction mechanics: " +
            $"{RinkaInstructionProgramDefinitionsTooling.MechanicsWordCount} words, " +
            $"{RinkaInstructionProgramDefinitions.PresentationWordCount} live " +
            "spritemap words, and both production programs pass with mechanics reads " +
            "forbidden.");
    }

    /// <summary>Executes one Rinka instruction program through the production interpreter for enough frames to observe its loop.</summary>
    /// <param name="bus">Guarded address space that rejects compiled mechanics reads and tracks visual operand access.</param>
    /// <param name="initialPointer">Native initial instruction pointer for the ordinary or special firing program.</param>
    /// <param name="special">Whether to initialize the special Rinka variant, which retains off-screen processing.</param>
    /// <returns>The configured enemy system and its Rinka slot after the instruction sequence runs.</returns>
    private static (RoomEnemySystem System, RoomEnemySlot Rinka) RunRinkaProgram(
        RinkaInstructionReadGuard bus,
        ushort initialPointer,
        bool special)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        RoomEnemySlot rinka = enemies.Slots[0];
        rinka.EnemyDefinitionPointer = RoomEnemySystem.RinkaDefinition;
        rinka.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        rinka.Parameter1 = special ? (ushort)1 : (ushort)0;
        rinka.CurrentInstruction = initialPointer;
        rinka.InstructionTimer = 1;

        var state = new RinkaEnemyState(rinka)
        {
            Function = RinkaEnemyFunction.WatchForLeavingViewport,
        };
        var states = (RinkaEnemyState?[])typeof(RoomEnemySystem)
            .GetField("_rinkaStates", flags)!
            .GetValue(enemies)!;
        states[0] = state;
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);

        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        object?[] arguments = [rinka, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];

        // One complete list is 127 timer frames plus zero-time callback/goto dispatch. The
        // margin proves that the native terminal goto loops rather than merely reaching it.
        for (int frame = 0; frame < 180; frame++)
            process.Invoke(enemies, arguments);

        return (enemies, rinka);
    }

    /// <summary>Reads a little-endian mechanics word from bank $A2 at a bank-local instruction address.</summary>
    /// <param name="source">Cartridge address space containing the native program.</param>
    /// <param name="address">Bank-local address of the low byte.</param>
    /// <returns>The two source bytes combined into a 16-bit word.</returns>
    private static ushort ReadRinkaProgramWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa20000 | address) |
            source.ReadByte(0xa20000 | unchecked((ushort)(address + 1))) << 8));

    /// <summary>Rejects runtime reads of compiled Rinka mechanics and records accesses to presentation operands.</summary>
    /// <param name="source">Underlying cartridge address space for reads outside the protected mechanics data.</param>
    private sealed class RinkaInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Presentation word addresses observed while Rinka instructions execute.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];

        /// <summary>Number of attempts to reread mechanics bytes supplied by compiled definitions.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes cartridge-byte requests through the guarded byte-read path.</summary>
        /// <param name="address">Cartridge byte address to read.</param>
        /// <returns>The underlying byte when the address is permitted.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects compiled mechanics reads, records presentation operands, and forwards other byte reads.</summary>
        /// <param name="address">CPU-visible byte address to read.</param>
        /// <returns>The underlying byte when the address is allowed.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to compiled Rinka mechanics data.</exception>
        public byte ReadByte(int address)
        {
            if (RinkaInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Rinka mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa20000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < RinkaInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        RinkaInstructionProgramDefinitions.PresentationWordAddress(index);
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

        /// <summary>Forwards a byte write to the underlying address space.</summary>
        /// <param name="address">CPU-visible byte address to write.</param>
        /// <param name="value">Byte value to store.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
