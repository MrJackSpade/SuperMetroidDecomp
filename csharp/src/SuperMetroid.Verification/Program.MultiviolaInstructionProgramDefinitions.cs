using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Runs the Multiviola instruction checks against the installed retail ROM.</summary>
    private static void VerifyMultiviolaInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyMultiviolaInstructionProgramDefinitions), () => VerifyMultiviolaInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>
    /// Compares compiled mechanics with bank $A2 and exercises the real initializer and
    /// instruction loop while checking that production avoids mechanics and selector reads.
    /// </summary>
    /// <param name="rom">Retail address space containing Multiviola's native instruction data.</param>
    private static void VerifyMultiviolaInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < MultiviolaInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                MultiviolaInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadMultiviolaInstructionWord(rom, definition.Address),
                $"Multiviola mechanics word $A2:{definition.Address:X4}");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = new MultiviolaInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        Type type = typeof(RoomEnemySystem);
        type.GetField("_bus", flags)!.SetValue(enemies, guard);
        var initialize = type.GetMethod("InitializeMultiviola", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        MethodInfo process = type.GetMethod("ProcessInstructions", flags)!;

        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = RoomEnemySystem.MultiviolaDefinition;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        slot.Parameter1 = 0x0058;
        slot.Parameter2 = 1;
        initialize(slot);
        AssertEqual(MultiviolaInstructionProgramDefinitions.Flying,
            slot.CurrentInstruction,
            "real Multiviola initializer installs compiled flying program");

        object?[] processArguments =
            [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        for (int call = 0; call < 15; call++)
        {
            slot.InstructionTimer = 1;
            process.Invoke(enemies, processArguments);
        }
        AssertEqual(unchecked((ushort)(MultiviolaInstructionProgramDefinitions.Flying + 4)),
            slot.CurrentInstruction,
            "Multiviola program completes its native goto and first repeated frame");
        // The artwork suite compares all fourteen compiled visual selectors to
        // the cartridge. Live instruction execution must no longer read them.
        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "Multiviola execution uses compiled spritemap selectors");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Multiviola mechanics byte");
        AssertThrows<InvalidDataException>(
            () => MultiviolaInstructionProgramDefinitions.ReadMechanicsWord(0xb2de),
            "Multiviola spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => MultiviolaInstructionProgramDefinitions.ReadMechanicsWord(0xb318),
            "unused Multiviola program is rejected as production mechanics");

        _ = ProbeMultiviolaInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeMultiviolaInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Multiviola allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Multiviola mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Multiviola instruction mechanics: 16 compiled words, the complete production " +
            "loop and compiled spritemap selectors pass with mechanics bytes forbidden.");
    }

    /// <summary>
    /// Repeatedly resolves the flying and terminal instruction words for the caller's
    /// warmed lookup allocation check.
    /// </summary>
    /// <returns>A checksum that keeps the looked-up mechanics values observable.</returns>
    private static int ProbeMultiviolaInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += MultiviolaInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? MultiviolaInstructionProgramDefinitions.Flying
                    : (ushort)0xb314);
        }
        return checksum;
    }

    /// <summary>Reads a little-endian instruction word from bank $A2.</summary>
    /// <param name="source">Cartridge address space containing the instruction bytes.</param>
    /// <param name="address">Bank-local address of the word's low byte.</param>
    /// <returns>The low byte combined with the following byte as the high byte.</returns>
    private static ushort ReadMultiviolaInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa20000 | address) |
            source.ReadByte(0xa20000 | unchecked((ushort)(address + 1))) << 8));

    /// <summary>
    /// Rejects runtime reads of compiled Multiviola mechanics words and records presentation
    /// selector reads so the verifier can confirm both remain supplied by compiled data.
    /// </summary>
    /// <param name="source">Underlying cartridge address space for permitted reads and writes.</param>
    private sealed class MultiviolaInstructionReadGuard(
        ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Gets the distinct presentation-word addresses observed during execution.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];

        /// <summary>Gets the number of attempted reads from compiled mechanics bytes.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes an import-time cartridge read through the guarded address-space path.</summary>
        /// <param name="address">Absolute cartridge address requested by the importer.</param>
        /// <returns>The source byte when the address is not a compiled mechanics byte.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>
        /// Rejects compiled mechanics reads, records access to known presentation words,
        /// and forwards other bytes from the wrapped address space.
        /// </summary>
        /// <param name="address">Absolute address requested by production code.</param>
        /// <returns>The source byte when the address is not forbidden.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to a compiled mechanics word.</exception>
        public byte ReadByte(int address)
        {
            if (MultiviolaInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Multiviola mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa20000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < MultiviolaInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        MultiviolaInstructionProgramDefinitions.PresentationWordAddress(index);
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

        /// <summary>Forwards a byte write unchanged to the wrapped address space.</summary>
        /// <param name="address">Absolute destination address.</param>
        /// <param name="value">Byte to write.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
