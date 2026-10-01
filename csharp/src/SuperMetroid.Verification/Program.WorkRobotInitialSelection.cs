using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyWorkRobotInitialSelection(SuperMetroidAddressSpace rom)
    {
        ushort NativeWord(int selector)
        {
            int address = EnemyRomTablePointers.WorkRobot.InitialInstructionListWords + 2 * selector;
            return (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        }
        MethodInfo initialize = typeof(RoomEnemySystem).GetMethod(
            "InitializeWorkRobotNoPower", BindingFlags.Instance | BindingFlags.NonPublic)!;
        for (ushort parameter = 0; parameter < 4; parameter++)
        {
            ushort expected = NativeWord(parameter);
            AssertEqual(expected, WorkRobotInitializationDefinitions.GetInitialInstruction(parameter),
                $"Work Robot original selection {parameter}, including adjacent opcode");
            var system = new RoomEnemySystem();
            var slot = new RoomEnemySlot(0) { Parameter1 = parameter };
            initialize.Invoke(system, [slot, new WorkRobotEnemyState(slot)]);
            AssertEqual(expected, slot.CurrentInstruction,
                $"Work Robot production selection {parameter} without cartridge access");
        }
        foreach (int invalid in new[] { -1, 4, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(
                () => WorkRobotInitializationDefinitions.GetInitialInstruction(invalid),
                $"Work Robot unsupported selector {invalid}");
        Console.WriteLine("Work Robot switch: three original pointers, native opcode overread, production initialization and bounds pass.");
    }
}
