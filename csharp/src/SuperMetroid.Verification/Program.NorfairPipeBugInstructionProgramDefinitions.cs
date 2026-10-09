using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Runs the Norfair Pipe Bug instruction and mechanics checks against the retail ROM in the verification directory.</summary>
    private static void VerifyNorfairPipeBugInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyNorfairPipeBugInstructionProgramDefinitions), () => VerifyNorfairPipeBugInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks the four directional instruction loops and verifies compiled mechanics and visual selectors against cartridge data.</summary>
    /// <param name="rom">Retail address space providing expected mechanics and presentation words.</param>
    private static void VerifyNorfairPipeBugInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
        for (int index = 0;
             index < NorfairPipeBugInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                NorfairPipeBugInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadNorfairPipeBugInstructionWord(rom, 0xb30000 | definition.Address),
                $"Norfair Pipe Bug instruction mechanics word $B3:{definition.Address:X4}");
        }

        var guard = new NorfairPipeBugInstructionProgramReadGuard(rom);
        (ushort Program, ushort Cursor, int Frames)[] programs =
        [
            (NorfairPipeBugInstructionProgramDefinitions.RisingLeft, 0x8ae5, 17),
            (NorfairPipeBugInstructionProgramDefinitions.FlyingLeft, 0x8b09, 7),
            (NorfairPipeBugInstructionProgramDefinitions.RisingRight, 0x8b25, 17),
            (NorfairPipeBugInstructionProgramDefinitions.FlyingRight, 0x8b49, 7),
        ];
        foreach ((ushort program, ushort cursor, int frames) in programs)
        {
            RoomEnemySystem enemies = CreateSystem(program, out RoomEnemySlot slot);
            RunProgram(enemies, slot, frames);
            AssertEqual(cursor, slot.CurrentInstruction,
                $"Norfair Pipe Bug program $B3:{program:X4} loops");
        }

        AssertEqual(0, guard.ForbiddenPresentationReadAttempts,
            "Norfair Pipe Bug programs never read installed visual selectors");
        for (int index = 0;
             index < NorfairPipeBugInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort address =
                NorfairPipeBugInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            AssertEqual(ReadNorfairPipeBugInstructionWord(rom, 0xb30000 | address),
                PipeBugVisualDefinitions.FrameAt(
                    PipeBugDefinitions.NorfairEnemyDefinition, address),
                $"compiled Norfair Pipe Bug frame $B3:{address:X4}");
        }
        AssertThrows<InvalidDataException>(
            () => PipeBugVisualDefinitions.FrameAt(
                PipeBugDefinitions.NorfairEnemyDefinition, 0x8b61),
            "unlisted Norfair Pipe Bug visual operand fails loudly");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled Norfair Pipe Bug mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => NorfairPipeBugInstructionProgramDefinitions.ReadMechanicsWord(0x8ae3),
            "Norfair Pipe Bug spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => NorfairPipeBugInstructionProgramDefinitions.ReadMechanicsWord(0x8b61),
            "adjacent Norfair Pipe Bug initializer is rejected as mechanics");

        _ = ProbeNorfairPipeBugInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeNorfairPipeBugInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Norfair Pipe Bug allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Norfair Pipe Bug mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Norfair Pipe Bug instruction mechanics: thirty-six compiled words, four " +
            "complete loops pass with twenty-eight visual selectors and mechanics " +
            "source words forbidden.");

        RoomEnemySystem CreateSystem(ushort program, out RoomEnemySlot slot)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            var initialize = typeof(RoomEnemySystem).GetMethod(
                "InitializeNorfairPipeBug", flags)!
                .CreateDelegate<Action<RoomEnemySlot>>(enemies);
            MethodInfo install = typeof(RoomEnemySystem).GetMethod(
                "InstallPipeBugInstruction", flags)!;
            slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = PipeBugDefinitions.NorfairEnemyDefinition;
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

    /// <summary>Repeats compiled mechanics lookups so the warmed allocation assertion measures steady-state behavior.</summary>
    /// <returns>A checksum that keeps the lookup results observable.</returns>
    private static int ProbeNorfairPipeBugInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += NorfairPipeBugInstructionProgramDefinitions.ReadMechanicsWord(
                NorfairPipeBugInstructionProgramDefinitions.RisingLeft);
        }
        return checksum;
    }

    /// <summary>Reads one little-endian instruction word from the retail address space.</summary>
    /// <param name="bus">Address space supplying the word bytes.</param>
    /// <param name="address">Address of the low byte, followed by the high byte.</param>
    /// <returns>The combined 16-bit value.</returns>
    private static ushort ReadNorfairPipeBugInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    /// <summary>Wraps address-space reads to reject runtime access to compiled mechanics bytes and installed visual selectors.</summary>
    /// <param name="source">Underlying address space for reads and writes that pass the guard.</param>
    private sealed class NorfairPipeBugInstructionProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Number of attempted reads of presentation operands that should be supplied by compiled visual definitions.</summary>
        internal int ForbiddenPresentationReadAttempts { get; private set; }
        /// <summary>Number of attempted reads of mechanics bytes that should be supplied by compiled instruction definitions.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes a cartridge-import request through the guard's checked byte-read path.</summary>
        /// <param name="address">Cartridge address requested by the caller.</param>
        /// <returns>The underlying byte when neither forbidden range is accessed.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects forbidden mechanics or presentation reads and delegates other addresses to the wrapped source.</summary>
        /// <param name="address">Address requested from the SNES address space.</param>
        /// <returns>The underlying byte for an allowed read.</returns>
        public byte ReadByte(int address)
        {
            if (NorfairPipeBugInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Norfair Pipe Bug mechanics ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xb30000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < NorfairPipeBugInstructionProgramDefinitionsTooling.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        NorfairPipeBugInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        ForbiddenPresentationReadAttempts++;
                        throw new InvalidOperationException(
                            $"Production read installed Norfair Pipe Bug selector ${address:X6}.");
                    }
                }
            }
            return source.ReadByte(address);
        }
        /// <summary>Forwards a write unchanged to the wrapped address space.</summary>
        /// <param name="address">Destination address.</param>
        /// <param name="value">Byte to store.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
