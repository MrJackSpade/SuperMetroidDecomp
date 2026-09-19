using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyBoulderInstructionProgramDefinitions()
    {
        VerifyBoulderInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyBoulderInstructionProgramDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        for (int index = 0;
             index < BoulderInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            BoulderInstructionMechanicsWord definition =
                BoulderInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadBoulderInstructionWord(rom, 0xa60000 | definition.Address),
                $"Boulder instruction mechanics word $A6:{definition.Address:X4}");
        }

        var guard = new BoulderInstructionProgramReadGuard(rom);
        VerifyBoulderInstructionProgram(
            guard,
            parameter1: 0x0100,
            BoulderInstructionProgramDefinitions.Left,
            "left");
        VerifyBoulderInstructionProgram(
            guard,
            parameter1: 0x0000,
            BoulderInstructionProgramDefinitions.Right,
            "right");

        AssertEqual(
            BoulderInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all live Boulder spritemap words remain cartridge reads");
        for (int index = 0;
             index < BoulderInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = BoulderInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Boulder presentation word $A6:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Boulder mechanics byte");

        AssertThrows<InvalidDataException>(
            () => BoulderInstructionProgramDefinitions.ReadMechanicsWord(0x86a9),
            "interleaved Boulder spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => BoulderInstructionProgramDefinitions.ReadMechanicsWord(0x86ef),
            "adjacent Boulder data is rejected as mechanics");

        _ = ProbeBoulderInstructionMechanicsAllocation();
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeBoulderInstructionMechanicsAllocation();
        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        AssertTrue(checksum != 0, "Boulder allocation probe consumes live data");
        AssertEqual(0L, allocated,
            "warmed Boulder mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Boulder instruction mechanics: twenty compiled words, both mirrored " +
            "production loops, and sixteen live spritemap reads pass with mechanics " +
            "bytes forbidden.");

        static void VerifyBoulderInstructionProgram(
            BoulderInstructionProgramReadGuard guard,
            ushort parameter1,
            ushort expectedProgram,
            string description)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            var initialize = typeof(RoomEnemySystem).GetMethod("InitializeBoulder", flags)!
                .CreateDelegate<Action<RoomEnemySlot>>(enemies);
            MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;

            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.BoulderDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa6 };
            slot.CurrentInstruction = 0x0008;
            slot.InstructionTimer = 1;
            slot.Parameter1 = parameter1;
            slot.Parameter2 = 0x0108;
            slot.XPosition = 0x0100;
            slot.YPosition = 0x0200;
            initialize(slot);

            AssertEqual(expectedProgram, slot.CurrentInstruction,
                $"Boulder {description} initializer program selection");
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];

            // Eight eight-frame entries consume 64 actor frames. The margin crosses the
            // terminal goto and proves the production stream restarted its first frame.
            for (int frame = 0; frame < 70; frame++)
                process.Invoke(enemies, arguments);
            AssertEqual(unchecked((ushort)(expectedProgram + 4)), slot.CurrentInstruction,
                $"Boulder {description} program loops to its first frame");
        }
    }

    private static int ProbeBoulderInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += BoulderInstructionProgramDefinitions.ReadMechanicsWord(
                BoulderInstructionProgramDefinitions.Left);
        }
        return checksum;
    }

    private static ushort ReadBoulderInstructionWord(SuperMetroidAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class BoulderInstructionProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (BoulderInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Boulder mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa60000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < BoulderInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        BoulderInstructionProgramDefinitions.PresentationWordAddress(index);
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

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
