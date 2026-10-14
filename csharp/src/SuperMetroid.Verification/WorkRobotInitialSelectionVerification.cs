using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>Checks Work Robot's initial list selector against native words and production initialization.</summary>
internal static class WorkRobotInitialSelectionVerification
{
    /// <summary>Compares three retail pointers plus the adjacent fourth word, then verifies selector bounds.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Work Robot initial selection")]
    internal static void VerifyWorkRobotInitialSelection(SuperMetroidAddressSpace rom)
    {
        ushort NativeWord(int selector)
        {
            int address = EnemyRomTablePointers.WorkRobot.InitialInstructionListWords + 2 * selector;
            return CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
        }
        for (ushort parameter = 0; parameter < 4; parameter++)
        {
            ushort expected = NativeWord(parameter);
            AssertEqual(expected, WorkRobotInitializationDefinitions.GetInitialInstruction(parameter),
                $"Work Robot original selection {parameter}, including adjacent opcode");
            var system = new RoomEnemySystem();
            var slot = new RoomEnemySlot(0) { Parameter1 = parameter };
            system.WorkRobotBehavior.InitializeWorkRobotNoPower(slot, new WorkRobotEnemyState(slot));
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
