using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Registers the Kzan instruction-program verification against the pinned retail cartridge.</summary>
    private static void VerifyKzanInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyKzanInstructionProgramDefinitions), () => VerifyKzanInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks the compiled mechanics words and executes Kzan's production instruction sequence through a ROM read guard.</summary>
    /// <param name="rom">Pinned retail address space used to verify native word values and permitted presentation reads.</param>
    private static void VerifyKzanInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < KzanInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                KzanInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadKzanInstructionWord(rom, 0xa60000 | definition.Address),
                $"Kzan instruction mechanics word $A6:{definition.Address:X4}");
        }

        var guard = new KzanInstructionProgramReadGuard(rom);
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        var initialize = typeof(RoomEnemySystem).GetMethod("InitializeKzanTop", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = RoomEnemySystem.KzanTopDefinition;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa6 };
        slot.YPosition = 0x0080;
        initialize(slot);
        AssertEqual(KzanInstructionProgramDefinitions.Idle, slot.CurrentInstruction,
            "Kzan initializer installs compiled program identity");

        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        object?[] arguments =
            [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        process.Invoke(enemies, arguments);
        AssertEqual(ReadKzanInstructionWord(rom, 0xa60000 |
                KzanInstructionProgramDefinitionsTooling.PresentationWord), slot.SpritemapPointer,
            "Kzan selects its exact native frame without cartridge reads");
        process.Invoke(enemies, arguments);
        AssertEqual(unchecked((ushort)(KzanInstructionProgramDefinitions.Idle + 4)),
            slot.CurrentInstruction,
            "Kzan reaches terminal native sleep");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled Kzan mechanics bytes");

        AssertThrows<InvalidDataException>(
            () => KzanInstructionProgramDefinitions.ReadMechanicsWord(
                KzanInstructionProgramDefinitionsTooling.PresentationWord),
            "Kzan spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => KzanInstructionProgramDefinitions.ReadMechanicsWord(0x8b2f),
            "adjacent Kzan initializer code is rejected as mechanics");

        _ = ProbeKzanInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeKzanInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Kzan allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Kzan mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Kzan instruction mechanics: two compiled words, the complete production " +
            "program and exact frame selection pass with instruction bytes forbidden.");
    }

    /// <summary>Repeatedly reads the compiled idle word so the caller can measure warmed lookup allocations.</summary>
    /// <returns>A checksum that keeps the repeated mechanics reads observable.</returns>
    private static int ProbeKzanInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += KzanInstructionProgramDefinitions.ReadMechanicsWord(
                KzanInstructionProgramDefinitions.Idle);
        }
        return checksum;
    }

    /// <summary>Reads one little-endian mechanics word from the supplied banked cartridge address space.</summary>
    /// <param name="bus">Retail cartridge address space used for the two byte reads.</param>
    /// <param name="address">Banked address of the word's low byte.</param>
    /// <returns>The low byte followed by the high byte as a 16-bit value.</returns>
    private static ushort ReadKzanInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    /// <summary>Rejects cartridge reads of compiled Kzan mechanics and visual operands during production instruction execution.</summary>
    /// <param name="source">Address space supplying all reads permitted by the guard.</param>
    private sealed class KzanInstructionProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Counts blocked reads of compiled mechanics bytes; attempts to read the visual operand are rejected separately.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes cartridge access through the Kzan mechanics and presentation guard.</summary>
        /// <param name="address">Cartridge address requested by instruction processing.</param>
        /// <returns>The source byte when the address is not guarded.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads of compiled mechanics or the compiled visual operand and forwards other reads.</summary>
        /// <param name="address">CPU address requested by production execution.</param>
        /// <returns>The wrapped source byte for an allowed address.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to compiled Kzan mechanics or its visual operand.</exception>
        public byte ReadByte(int address)
        {
            if (KzanInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Kzan mechanics byte ${address:X6}.");
            }

            int presentation = 0xa60000 | KzanInstructionProgramDefinitionsTooling.PresentationWord;
            if (address == presentation || address == presentation + 1)
            {
                throw new InvalidOperationException("Production read the compiled Kzan visual operand.");
            }

            return source.ReadByte(address);
        }

        /// <summary>Forwards writes to the wrapped address space.</summary>
        /// <param name="address">Address to update.</param>
        /// <param name="value">Byte to store at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
