using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.WorkRobots;
namespace SuperMetroid.Verification.Enemies.WorkRobots;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that a kill empties the WorkRobot values the cartridge keeps in the enemy's common record.</summary>
internal static class WorkRobotRecordStateVerification
{
    /// <summary>
    /// Installs a WorkRobot state on a slot with its record-clearing operation, gives every value the record holds a
    /// nonzero value, and confirms a kill empties each one.
    /// </summary>
    internal static void VerifyWorkRobotRecordState()
    {
        var enemies = new RoomEnemySystem();
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new WorkRobotEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.LaserVelocity = new EightEightVector<RoomPixels>(new SnesSignedEightEight(1), default);
        state.LaserCooldown = NativeCountdown.Start(1);
        state.BackupPosition = new Position16<RoomPixels>(1, 0);
        state.FallVelocity = SnesSignedSixteenSixteen.FromParts(1, 0);

        slot.Clear();
        AssertEqual<EightEightVector<RoomPixels>>(default(EightEightVector<RoomPixels>), state.LaserVelocity, "a kill empties LaserVelocity");
        AssertEqual<NativeCountdown>(NativeCountdown.Start(0), state.LaserCooldown, "a kill empties LaserCooldown");
        AssertEqual<Position16<RoomPixels>>(Position16<RoomPixels>.Zero, state.BackupPosition, "a kill empties BackupPosition");
        AssertEqual<SnesSignedSixteenSixteen>(SnesSignedSixteenSixteen.Zero, state.FallVelocity, "a kill empties FallVelocity");
        Console.WriteLine("WorkRobot record state: a kill empties every value the record holds.");
    }

    /// <summary>Runs the focused WorkRobot record-state check.</summary>
    [VerificationCommand("--work-robot-record-state")]
    internal static int WorkRobotRecordStateCommand()
    {
        VerifyWorkRobotRecordState();
        return 0;
    }
}
