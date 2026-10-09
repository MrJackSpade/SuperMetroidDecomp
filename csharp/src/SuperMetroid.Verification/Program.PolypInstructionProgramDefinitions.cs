using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks compiled Polyp instruction mechanics and execution against the retail ROM.</summary>
    private static void VerifyPolypInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyPolypInstructionProgramDefinitions), () => VerifyPolypInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Verifies native mechanics words and runs the production Polyp initializer and instruction loop.</summary>
    /// <param name="rom">Retail address space supplying the expected instruction words and frame operand.</param>
    private static void VerifyPolypInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < PolypInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                PolypInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadPolypInstructionWord(rom, definition.Address),
                $"Polyp mechanics word $A2:{definition.Address:X4}");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = new PolypInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        Type type = typeof(RoomEnemySystem);
        type.GetField("_bus", flags)!.SetValue(enemies, guard);
        type.GetField("_setRandomNumber", flags)!.SetValue(
            enemies,
            (Action<ushort>)(_ => { }));
        var initialize = type.GetMethod("InitializePolyp", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        MethodInfo process = type.GetMethod("ProcessInstructions", flags)!;

        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = RoomEnemySystem.PolypDefinition;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        initialize(slot);
        AssertEqual(PolypInstructionProgramDefinitions.Stationary,
            slot.CurrentInstruction,
            "real Polyp initializer installs compiled stationary program");

        object?[] processArguments =
            [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        slot.InstructionTimer = 1;
        process.Invoke(enemies, processArguments);
        AssertEqual(ReadPolypInstructionWord(rom,
                PolypInstructionProgramDefinitions.PresentationWord), slot.SpritemapPointer,
            "Polyp selects its exact native frame without cartridge reads");
        slot.InstructionTimer = 1;
        process.Invoke(enemies, processArguments);
        AssertEqual(unchecked((ushort)(PolypInstructionProgramDefinitions.Stationary + 4)),
            slot.CurrentInstruction,
            "Polyp program reaches terminal sleep");
        AssertEqual(ReadPolypInstructionWord(rom, PolypInstructionProgramDefinitions.PresentationWord),
            slot.SpritemapPointer, "Actual Polyp stationary sprite matches the native operand");
        AssertEqual(slot.SpritemapPointer,
            PolypInstructionProgramDefinitions.FrameAt(PolypInstructionProgramDefinitions.PresentationWord),
            "Named Polyp selector installed by the actual program");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids both compiled Polyp mechanics words");
        AssertThrows<InvalidDataException>(
            () => PolypInstructionProgramDefinitions.ReadMechanicsWord(
                PolypInstructionProgramDefinitions.PresentationWord),
            "Polyp spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => PolypInstructionProgramDefinitions.ReadMechanicsWord(0xb520),
            "adjacent cooldown table is rejected as instruction mechanics");

        _ = ProbePolypInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbePolypInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Polyp allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Polyp mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Polyp instruction mechanics: two compiled words, the real initializer, " +
            "terminal sleep, and exact frame selection pass with instruction bytes forbidden.");
    }

    /// <summary>Warms and repeats compiled Polyp mechanics lookups for the allocation measurement.</summary>
    private static int ProbePolypInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += PolypInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? PolypInstructionProgramDefinitions.Stationary
                    : (ushort)0xb51e);
        }
        return checksum;
    }

    /// <summary>Reads a little-endian instruction word from bank $A2 at the specified offset.</summary>
    /// <param name="source">Address space containing the native instruction bytes.</param>
    /// <param name="address">Bank-relative address of the word's low byte.</param>
    /// <returns>The two bytes combined with the low byte in the least significant position.</returns>
    private static ushort ReadPolypInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa20000 | address) |
            source.ReadByte(0xa20000 | unchecked((ushort)(address + 1))) << 8));

    /// <summary>Rejects production reads from compiled Polyp mechanics and presentation operands.</summary>
    /// <param name="source">Underlying address space used for permitted reads and forwarded writes.</param>
    private sealed class PolypInstructionReadGuard(
        ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Gets the number of attempted reads from the compiled Polyp mechanics words.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes an imported cartridge read through the mechanics and presentation guards.</summary>
        /// <param name="address">Cartridge address requested by production code.</param>
        /// <returns>The wrapped byte unless the address is a forbidden compiled word.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads of compiled Polyp words and forwards other addresses to the wrapped bus.</summary>
        /// <param name="address">Address requested by production code.</param>
        /// <returns>The byte from the wrapped source for an allowed address.</returns>
        public byte ReadByte(int address)
        {
            if (PolypInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Polyp mechanics byte ${address:X6}.");
            }

            int presentation = 0xa20000 | PolypInstructionProgramDefinitions.PresentationWord;
            if (address == presentation || address == presentation + 1)
                throw new InvalidOperationException("Production read the compiled Polyp visual operand.");
            return source.ReadByte(address);
        }

        /// <summary>Forwards writes unchanged to the wrapped address space.</summary>
        /// <param name="address">Destination address.</param>
        /// <param name="value">Byte to store at the destination.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
