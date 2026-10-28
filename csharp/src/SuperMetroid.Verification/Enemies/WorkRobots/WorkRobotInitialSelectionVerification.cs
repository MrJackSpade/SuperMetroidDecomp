using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.WorkRobots;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.WorkRobots;

/// <summary>Checks Work Robot's initial animation selection against native words and production initialization.</summary>
internal static class WorkRobotInitialSelectionVerification
{
    /// <summary>
    /// The three retail table words name the three unpowered programs and the production initializer installs them; the
    /// native overrun selector three and out-of-range selectors fail loudly.
    /// </summary>
    internal static void VerifyWorkRobotInitialSelection(SuperMetroidAddressSpace rom)
    {
        for (ushort parameter = 0; parameter < 3; parameter++)
        {
            int address = EnemyRomTablePointers.WorkRobot.InitialInstructionListWords + 2 * parameter;
            WorkRobotProgram expected = WorkRobotPoses.InitialProgram(parameter);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(address),
                NativeEnemyProgramOracle.EntryOf(WorkRobotNativeListAddresses.Entries, expected),
                $"Work Robot original selection {parameter}");
            var system = new RoomEnemySystem(new EnemySession());
            var slot = new RoomEnemySlot(0, EnemySlotFixtures.Registry);
            slot.SetSpawnParameter1(parameter);
            var state = new WorkRobotEnemyState();
            system.WorkRobotBehavior.InitializeWorkRobotNoPower(slot, state);
            AssertEqual(AnimationProgramCursor<WorkRobotProgram>.Start(expected), state.Program,
                $"Work Robot production selection {parameter} without cartridge access");
        }
        AssertThrows<InvalidDataException>(() => WorkRobotPoses.InitialProgram(3),
            "Work Robot selector three overruns the table into code");
        {
            var system = new RoomEnemySystem(new EnemySession());
            var slot = new RoomEnemySlot(0, EnemySlotFixtures.Registry);
            slot.SetSpawnParameter1(3);
            AssertThrows<InvalidDataException>(() => system.WorkRobotBehavior.InitializeWorkRobotNoPower(slot, new WorkRobotEnemyState()),
                "Work Robot production initializer rejects the overrun selector");
        }
        foreach (int invalid in new[] { -1, 4, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(
                () => WorkRobotPoses.InitialProgram(invalid),
                $"Work Robot unsupported selector {invalid}");
        Console.WriteLine("Work Robot switch: three original programs, the overrun selector's loud failure, production initialization and bounds pass.");
    }
}
