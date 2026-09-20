using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyOwtchInstructionProgramDefinitions()
    {
        VerifyOwtchInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyOwtchInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        for (int index = 0;
             index < OwtchInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            OwtchInstructionMechanicsWord definition =
                OwtchInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadOwtchInstructionWord(rom, 0xa20000 | definition.Address),
                $"Owtch instruction mechanics word $A2:{definition.Address:X4}");
        }

        var guard = new OwtchInstructionProgramReadGuard(rom);
        VerifyProgram(
            initialState: OwtchBehaviorState.MovingRight,
            program: OwtchInstructionProgramDefinitions.MovingLeft,
            expectedState: OwtchBehaviorState.MovingLeft,
            expectedLoopCursor: 0xa3b1,
            direction: "left");
        VerifyProgram(
            initialState: OwtchBehaviorState.MovingLeft,
            program: OwtchInstructionProgramDefinitions.MovingRight,
            expectedState: OwtchBehaviorState.MovingRight,
            expectedLoopCursor: 0xa3c3,
            direction: "right");

        AssertEqual(OwtchInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all live Owtch spritemap words remain cartridge reads");
        for (int index = 0;
             index < OwtchInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                OwtchInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Owtch presentation word $A2:{address:X4}");
        }

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Owtch mechanics byte");
        AssertThrows<InvalidDataException>(
            () => OwtchInstructionProgramDefinitions.ReadMechanicsWord(0xa3af),
            "interleaved Owtch spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => OwtchInstructionProgramDefinitions.ReadMechanicsWord(0xa3cf),
            "adjacent Owtch data table is rejected as mechanics");

        _ = ProbeOwtchInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeOwtchInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Owtch allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Owtch mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Owtch instruction mechanics: twelve compiled words, both directional " +
            "callbacks and complete animation loops, and six live spritemap reads " +
            "pass with mechanics bytes forbidden.");

        void VerifyProgram(
            OwtchBehaviorState initialState,
            ushort program,
            OwtchBehaviorState expectedState,
            ushort expectedLoopCursor,
            string direction)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            var initialize = typeof(RoomEnemySystem).GetMethod("InitializeOwtch", flags)!
                .CreateDelegate<Action<RoomEnemySlot>>(enemies);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.OwtchDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
            slot.Parameter1 = (ushort)initialState;
            initialize(slot);

            OwtchEnemyState state = enemies.OwtchStates[0]!;
            state.Behavior = initialState;
            slot.CurrentInstruction = program;
            slot.InstructionTimer = 1;
            slot.Timer = 0;
            RunOwtchProgram(enemies, slot, 25);

            AssertEqual(expectedState, state.Behavior,
                $"Owtch {direction} callback selects native behavior");
            AssertEqual(expectedLoopCursor, slot.CurrentInstruction,
                $"Owtch {direction} program returns through its native goto");
        }

        static void RunOwtchProgram(
            RoomEnemySystem enemies,
            RoomEnemySlot slot,
            int frames)
        {
            MethodInfo process = typeof(RoomEnemySystem).GetMethod(
                "ProcessInstructions", flags)!;
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            for (int frame = 0; frame < frames; frame++)
                process.Invoke(enemies, arguments);
        }
    }

    private static int ProbeOwtchInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += OwtchInstructionProgramDefinitions.ReadMechanicsWord(
                OwtchInstructionProgramDefinitions.MovingLeft);
        }
        return checksum;
    }

    private static ushort ReadOwtchInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class OwtchInstructionProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (OwtchInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Owtch mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa20000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < OwtchInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        OwtchInstructionProgramDefinitions.PresentationWordAddress(index);
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
