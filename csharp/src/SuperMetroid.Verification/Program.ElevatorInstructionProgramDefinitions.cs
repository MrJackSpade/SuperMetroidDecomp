using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyElevatorInstructionProgramDefinitions()
    {
        VerifyElevatorInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyElevatorInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < ElevatorInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            ElevatorInstructionMechanicsWord definition =
                ElevatorInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadElevatorInstructionWord(rom, definition.Address),
                $"elevator mechanics word $A3:{definition.Address:X4}");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = new ElevatorInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        Type type = typeof(RoomEnemySystem);
        type.GetField("_bus", flags)!.SetValue(enemies, guard);
        MethodInfo initialize = type.GetMethod("InitializeElevator", flags)!;
        MethodInfo process = type.GetMethod("ProcessInstructions", flags)!;
        RoomEnemySlot elevator = enemies.Slots[0];
        elevator.EnemyDefinitionPointer = RoomEnemySystem.ElevatorDefinition;
        elevator.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };
        initialize.Invoke(enemies, [elevator, null]);
        AssertEqual(ElevatorInstructionProgramDefinitions.Loop,
            elevator.CurrentInstruction,
            "real elevator initializer installs compiled animation loop");

        object?[] arguments =
            [elevator, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        for (int call = 0; call < 3; call++)
        {
            elevator.InstructionTimer = 1;
            process.Invoke(enemies, arguments);
        }
        AssertEqual((ushort)0x94da, elevator.CurrentInstruction,
            "elevator two-frame program loops to its second mechanics word");
        AssertEqual(2, guard.ObservedPresentationWords.Count,
            "both elevator spritemap operands remain cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled elevator mechanics byte");
        AssertThrows<InvalidDataException>(
            () => ElevatorInstructionProgramDefinitions.ReadMechanicsWord(
                ElevatorInstructionProgramDefinitions.PresentationWordAddress(0)),
            "elevator spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => ElevatorInstructionProgramDefinitions.ReadMechanicsWord(
                ElevatorInstructionProgramDefinitions.FirstAdjacentMechanicsData),
            "adjacent elevator input table is rejected as instruction mechanics");

        _ = ProbeElevatorInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeElevatorInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "elevator allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed elevator mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Elevator instruction mechanics: four compiled words, the real initializer, " +
            "the complete two-frame loop, and two live spritemap reads pass with mechanics " +
            "bytes forbidden.");
    }

    private static int ProbeElevatorInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += ElevatorInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0 ? ElevatorInstructionProgramDefinitions.Loop : (ushort)0x94de);
        }
        return checksum;
    }

    private static ushort ReadElevatorInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa30000 | address) |
            source.ReadByte(0xa30000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class ElevatorInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (ElevatorInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled elevator mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa30000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < ElevatorInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        ElevatorInstructionProgramDefinitions.PresentationWordAddress(index);
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
