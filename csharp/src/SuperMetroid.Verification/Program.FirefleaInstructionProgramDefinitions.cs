using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Runs the Fireflea instruction-definition checks against the installed retail ROM.
    /// </summary>
    private static void VerifyFirefleaInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyFirefleaInstructionProgramDefinitions), () => VerifyFirefleaInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>
    /// Compares compiled mechanics and all 52 presentation selectors with the cartridge,
    /// then exercises the real initializer and instruction loop while guarding source bytes.
    /// </summary>
    /// <param name="rom">Retail address space containing Fireflea's native instruction data.</param>
    private static void VerifyFirefleaInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

        AssertEqual(54, FirefleaInstructionProgramDefinitionsTooling.MechanicsWordCount,
            "Fireflea compiled mechanics word count");
        AssertEqual(52, FirefleaInstructionProgramDefinitionsTooling.PresentationWordCount,
            "Fireflea live presentation word count");
        for (int index = 0;
             index < FirefleaInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                FirefleaInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadFirefleaInstructionWord(rom, definition.Address),
                $"Fireflea mechanics word $A3:{definition.Address:X4}");
        }

        var guard = new FirefleaInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = RoomEnemySystem.FirefleaDefinition;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };
        slot.Parameter1 = 0;
        slot.Parameter2 = 0;
        typeof(RoomEnemySystem).GetMethod("InitializeFireflea", flags)!
            .Invoke(enemies, [slot]);
        AssertEqual(FirefleaInstructionProgramDefinitions.Loop,
            slot.CurrentInstruction,
            "Fireflea real initializer selects compiled loop");

        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        object?[] arguments =
            [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        for (int call = 0;
             call < FirefleaInstructionProgramDefinitions.FrameCount + 1;
             call++)
        {
            slot.InstructionTimer = 1;
            process.Invoke(enemies, arguments);
            ushort operand = FirefleaInstructionProgramDefinitionsTooling.PresentationWordAddress(
                call % FirefleaInstructionProgramDefinitions.FrameCount);
            ushort native = ReadFirefleaInstructionWord(rom, operand);
            AssertEqual(native, EnemySpritemapDefinitions.FirefleaFrameAt(operand),
                $"compiled Fireflea visual selector $A3:{operand:X4}");
            AssertEqual(native, slot.SpritemapPointer,
                $"Fireflea production frame {call} matches cartridge selection");
        }
        AssertEqual(unchecked((ushort)(FirefleaInstructionProgramDefinitions.Loop + 4)),
            slot.CurrentInstruction,
            "Fireflea completes all 52 frames and loops to the first frame");

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled Fireflea mechanics and visual bytes");

        AssertThrows<InvalidDataException>(
            () => FirefleaInstructionProgramDefinitions.ReadMechanicsWord(
                FirefleaInstructionProgramDefinitionsTooling.PresentationWordAddress(0)),
            "Fireflea spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => FirefleaInstructionProgramDefinitions.ReadMechanicsWord(
                FirefleaInstructionProgramDefinitions.AdjacentUnusedData),
            "adjacent unused Fireflea data is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => EnemySpritemapDefinitions.FirefleaFrameAt(
                FirefleaInstructionProgramDefinitions.AdjacentUnusedData),
            "adjacent unused Fireflea data is rejected as presentation");

        _ = ProbeFirefleaInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeFirefleaInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Fireflea allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Fireflea mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Fireflea instruction mechanics: 54 compiled words, the complete 52-frame " +
            "loop, and 52 exact frame selections pass with mechanics and visual source bytes forbidden.");
    }

    /// <summary>
    /// Repeatedly reads both supported loop words so the caller can check warmed lookup
    /// allocation without the checksum being optimized away.
    /// </summary>
    /// <returns>A checksum over the mechanics words read by the probe.</returns>
    private static int ProbeFirefleaInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += FirefleaInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? FirefleaInstructionProgramDefinitions.Loop
                    : unchecked((ushort)(FirefleaInstructionProgramDefinitions.Loop + 4)));
        }
        return checksum;
    }

    /// <summary>Reads a little-endian instruction word from bank $A3.</summary>
    /// <param name="source">Cartridge address space containing the instruction bytes.</param>
    /// <param name="address">Bank-local address of the word's low byte.</param>
    /// <returns>The low byte combined with the following byte as the high byte.</returns>
    private static ushort ReadFirefleaInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa30000 | address) |
            source.ReadByte(0xa30000 | unchecked((ushort)(address + 1))) << 8));

    /// <summary>
    /// Prevents production from reading Fireflea mechanics and presentation words directly
    /// from the cartridge while forwarding unrelated address-space operations.
    /// </summary>
    /// <param name="source">Underlying cartridge address space for permitted reads and writes.</param>
    private sealed class FirefleaInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Gets the number of attempted reads of guarded mechanics or presentation bytes.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes an import-time cartridge read through the runtime read guard.</summary>
        /// <param name="address">Absolute cartridge address requested by the importer.</param>
        /// <returns>The source byte when the address is not guarded.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads of compiled mechanics and presentation bytes, forwarding all others.</summary>
        /// <param name="address">Absolute address requested by production code.</param>
        /// <returns>The source byte when the address is permitted.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to a compiled Fireflea word.</exception>
        public byte ReadByte(int address)
        {
            if (FirefleaInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                IsPresentationByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Fireflea instruction byte ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        /// <summary>
        /// Tests whether an address is either byte of a Fireflea presentation word in bank $A3.
        /// </summary>
        /// <param name="address">Absolute cartridge address to classify.</param>
        /// <returns><see langword="true"/> if the address is a presentation-selector byte.</returns>
        private static bool IsPresentationByte(int address) =>
            (address & 0xff0000) == 0xa30000 &&
            (FirefleaInstructionProgramDefinitions.IsPresentationWord(
                unchecked((ushort)address)) ||
             FirefleaInstructionProgramDefinitions.IsPresentationWord(
                unchecked((ushort)(address - 1))));

        /// <summary>Forwards a write unchanged to the wrapped cartridge address space.</summary>
        /// <param name="address">Absolute destination address.</param>
        /// <param name="value">Byte to write.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
