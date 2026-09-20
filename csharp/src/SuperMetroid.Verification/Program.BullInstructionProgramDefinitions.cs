using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyBullInstructionProgramDefinitions()
    {
        VerifyBullInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyBullInstructionProgramDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic |
            BindingFlags.Static;
        for (int index = 0;
             index < BullInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            BullInstructionMechanicsWord definition =
                BullInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadBullInstructionWord(rom, 0xa80000 | definition.Address),
                $"Bull instruction mechanics word $A8:{definition.Address:X4}");
        }

        var guard = new BullInstructionProgramReadGuard(rom);
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        var initialize = typeof(RoomEnemySystem).GetMethod("InitializeBull", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        MethodInfo resolveShot = typeof(RoomEnemySystem).GetMethod(
            "ResolveBullImmuneShot", flags)!;
        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = RoomEnemySystem.BullDefinition;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
        initialize(slot);
        AssertEqual(BullInstructionProgramDefinitions.Normal, slot.CurrentInstruction,
            "Bull initializer program");

        object?[] processArguments =
            [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        for (int frame = 0; frame < 41; frame++)
            process.Invoke(enemies, processArguments);
        AssertEqual(unchecked((ushort)(BullInstructionProgramDefinitions.Normal + 4)),
            slot.CurrentInstruction,
            "Bull normal program completes its native animation loop");
        AssertEqual((ushort)10, slot.InstructionTimer,
            "Bull normal loop restores its ten-frame duration");

        BullEnemyState state = enemies.BullStates[0] ?? throw new InvalidDataException(
            "Bull initializer did not publish typed state.");
        resolveShot.Invoke(null, [slot, state, (ushort)2]);
        AssertEqual(BullInstructionProgramDefinitions.Shot, slot.CurrentInstruction,
            "Bull immune-shot program");
        for (int frame = 0; frame < 61; frame++)
            process.Invoke(enemies, processArguments);
        AssertEqual(unchecked((ushort)(BullInstructionProgramDefinitions.Normal + 4)),
            slot.CurrentInstruction,
            "Bull shot program repeats five times and returns to normal");
        AssertEqual((ushort)0, slot.Timer, "Bull shot loop exhausts its native repeat timer");

        AssertEqual(BullInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all live Bull spritemap words remain cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled Bull mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => BullInstructionProgramDefinitions.ReadMechanicsWord(0xd843),
            "Bull spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => BullInstructionProgramDefinitions.ReadMechanicsWord(0xd871),
            "adjacent Bull shot-angle data is rejected as instruction mechanics");

        _ = ProbeBullInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeBullInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Bull allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Bull mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Bull instruction mechanics: sixteen compiled words, the complete normal " +
            "loop, the five-cycle immune-shot response, and eight live spritemap reads " +
            "pass with mechanics bytes forbidden.");
    }

    private static int ProbeBullInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
            checksum += BullInstructionProgramDefinitions.ReadMechanicsWord(
                BullInstructionProgramDefinitions.Normal);
        return checksum;
    }

    private static ushort ReadBullInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class BullInstructionProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (BullInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Bull mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa80000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < BullInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        BullInstructionProgramDefinitions.PresentationWordAddress(index);
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
