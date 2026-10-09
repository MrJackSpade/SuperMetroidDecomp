using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Runs the Yellow Pipe Bug instruction checks against the installed retail ROM.</summary>
    private static void VerifyYellowPipeBugInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyYellowPipeBugInstructionProgramDefinitions), () => VerifyYellowPipeBugInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>
    /// Compares compiled mechanics and presentation selectors with bank $B3, then exercises
    /// all four production programs while the guard rejects reads from those ROM tables.
    /// </summary>
    /// <param name="rom">Retail address space containing the native Yellow Pipe Bug programs.</param>
    private static void VerifyYellowPipeBugInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
        for (int index = 0;
             index < YellowPipeBugInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                YellowPipeBugInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadYellowPipeBugInstructionWord(rom, 0xb30000 | definition.Address),
                $"Yellow Pipe Bug instruction mechanics word $B3:{definition.Address:X4}");
        }

        var guard = new YellowPipeBugInstructionProgramReadGuard(rom);
        (ushort Program, ushort Cursor, int Frames)[] programs =
        [
            (YellowPipeBugInstructionProgramDefinitions.FlyingLeft, 0x8f00, 17),
            (YellowPipeBugInstructionProgramDefinitions.ArcingLeft, 0x8f14, 5),
            (YellowPipeBugInstructionProgramDefinitions.FlyingRight, 0x8f28, 17),
            (YellowPipeBugInstructionProgramDefinitions.ArcingRight, 0x8f3c, 5),
        ];
        foreach ((ushort program, ushort cursor, int frames) in programs)
        {
            RoomEnemySystem enemies = CreateSystem(program, out RoomEnemySlot slot);
            RunProgram(enemies, slot, frames);
            AssertEqual(cursor, slot.CurrentInstruction,
                $"Yellow Pipe Bug program $B3:{program:X4} loops");
        }

        AssertEqual(0, guard.ForbiddenPresentationReadAttempts,
            "Yellow Pipe Bug programs never read installed visual selectors");
        for (int index = 0;
             index < YellowPipeBugInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort address =
                YellowPipeBugInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            AssertEqual(ReadYellowPipeBugInstructionWord(rom, 0xb30000 | address),
                PipeBugVisualDefinitions.FrameAt(
                    PipeBugDefinitions.YellowEnemyDefinition, address),
                $"compiled Yellow Pipe Bug frame $B3:{address:X4}");
        }
        AssertThrows<InvalidDataException>(
            () => PipeBugVisualDefinitions.FrameAt(
                PipeBugDefinitions.YellowEnemyDefinition, 0x8f4c),
            "unlisted Yellow Pipe Bug visual operand fails loudly");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled Yellow Pipe Bug mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => YellowPipeBugInstructionProgramDefinitions.ReadMechanicsWord(0x8efe),
            "Yellow Pipe Bug spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => YellowPipeBugInstructionProgramDefinitions.ReadMechanicsWord(0x8f4c),
            "adjacent Yellow Pipe Bug initializer is rejected as mechanics");

        _ = ProbeYellowPipeBugInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeYellowPipeBugInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Yellow Pipe Bug allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Yellow Pipe Bug mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Yellow Pipe Bug instruction mechanics: twenty-four compiled words, four " +
            "complete loops pass with sixteen visual selectors and mechanics source " +
            "words forbidden.");

        RoomEnemySystem CreateSystem(ushort program, out RoomEnemySlot slot)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            var initialize = typeof(RoomEnemySystem).GetMethod(
                "InitializeYellowPipeBug", flags)!
                .CreateDelegate<Action<RoomEnemySlot>>(enemies);
            MethodInfo install = typeof(RoomEnemySystem).GetMethod(
                "InstallPipeBugInstruction", flags)!;
            slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = PipeBugDefinitions.YellowEnemyDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xb3 };
            initialize(slot);
            install.Invoke(null, [slot, program]);
            return enemies;
        }

        static void RunProgram(RoomEnemySystem enemies, RoomEnemySlot slot, int frames)
        {
            MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            for (int frame = 0; frame < frames; frame++)
                process.Invoke(enemies, arguments);
        }
    }

    /// <summary>Repeatedly reads the flying program's mechanics word for the warmed allocation check.</summary>
    /// <returns>A checksum that keeps the resolved instruction values observable.</returns>
    private static int ProbeYellowPipeBugInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += YellowPipeBugInstructionProgramDefinitions.ReadMechanicsWord(
                YellowPipeBugInstructionProgramDefinitions.FlyingLeft);
        }
        return checksum;
    }

    /// <summary>Reads a little-endian instruction word from the supplied cartridge address.</summary>
    /// <param name="bus">Address space containing the two instruction bytes.</param>
    /// <param name="address">Absolute address of the word's low byte.</param>
    /// <returns>The low byte combined with the following byte as the high byte.</returns>
    private static ushort ReadYellowPipeBugInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    /// <summary>
    /// Rejects runtime reads of compiled Yellow Pipe Bug mechanics and installed visual
    /// selectors, while forwarding unrelated address-space accesses to the source.
    /// </summary>
    /// <param name="source">Underlying address space used for permitted reads and writes.</param>
    private sealed class YellowPipeBugInstructionProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Gets the number of rejected attempts to read installed visual selectors.</summary>
        internal int ForbiddenPresentationReadAttempts { get; private set; }

        /// <summary>Gets the number of rejected attempts to read compiled mechanics bytes.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes an import-time cartridge read through the same ROM access guard.</summary>
        /// <param name="address">Absolute cartridge address requested by the importer.</param>
        /// <returns>The source byte when the address is not a guarded mechanics or selector byte.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads from compiled mechanics and presentation tables and forwards others.</summary>
        /// <param name="address">Absolute address requested by production code.</param>
        /// <returns>The source byte when the address is permitted.</returns>
        /// <exception cref="InvalidOperationException">The address identifies a guarded mechanics or presentation byte.</exception>
        public byte ReadByte(int address)
        {
            if (YellowPipeBugInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Yellow Pipe Bug mechanics ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xb30000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < YellowPipeBugInstructionProgramDefinitionsTooling.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        YellowPipeBugInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        ForbiddenPresentationReadAttempts++;
                        throw new InvalidOperationException(
                            $"Production read installed Yellow Pipe Bug selector ${address:X6}.");
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
