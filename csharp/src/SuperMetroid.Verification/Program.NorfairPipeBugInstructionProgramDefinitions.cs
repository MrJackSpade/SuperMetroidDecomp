using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyNorfairPipeBugInstructionProgramDefinitions()
    {
        VerifyNorfairPipeBugInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyNorfairPipeBugInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
        for (int index = 0;
             index < NorfairPipeBugInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            NorfairPipeBugInstructionMechanicsWord definition =
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
             index < NorfairPipeBugInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                NorfairPipeBugInstructionProgramDefinitions.PresentationWordAddress(index);
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

    private static ushort ReadNorfairPipeBugInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class NorfairPipeBugInstructionProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal int ForbiddenPresentationReadAttempts { get; private set; }
        internal int ForbiddenReadAttempts { get; private set; }
        public byte ReadByte(int address)
        {
            if (NorfairPipeBugInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Norfair Pipe Bug mechanics ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xb30000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < NorfairPipeBugInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        NorfairPipeBugInstructionProgramDefinitions.PresentationWordAddress(index);
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
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
