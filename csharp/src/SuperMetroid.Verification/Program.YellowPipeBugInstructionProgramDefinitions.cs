using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyYellowPipeBugInstructionProgramDefinitions()
    {
        VerifyYellowPipeBugInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyYellowPipeBugInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
        for (int index = 0;
             index < YellowPipeBugInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            YellowPipeBugInstructionMechanicsWord definition =
                YellowPipeBugInstructionProgramDefinitions.MechanicsWord(index);
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
             index < YellowPipeBugInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                YellowPipeBugInstructionProgramDefinitions.PresentationWordAddress(index);
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

    private static ushort ReadYellowPipeBugInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class YellowPipeBugInstructionProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal int ForbiddenPresentationReadAttempts { get; private set; }
        internal int ForbiddenReadAttempts { get; private set; }
        public byte ReadByte(int address)
        {
            if (YellowPipeBugInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Yellow Pipe Bug mechanics ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xb30000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < YellowPipeBugInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        YellowPipeBugInstructionProgramDefinitions.PresentationWordAddress(index);
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
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
