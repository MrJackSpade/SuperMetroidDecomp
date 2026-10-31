using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Boyon;
namespace SuperMetroid.Verification.Enemies.Boyon;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks that a kill empties the Boyon values the cartridge keeps in the enemy's common record.</summary>
internal static class BoyonRecordStateVerification
{
    /// <summary>
    /// Installs a Boyon state on a slot with its record-clearing operation, gives every value the record holds a
    /// nonzero value, and confirms a kill empties each one.
    /// </summary>
    internal static void VerifyBoyonRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()));
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new BoyonEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.SpeedMultiplier = 1;
        state.Speed = 1;
        state.JumpHeight = 1;
        state.DistanceAccumulator = 1;
        state.SpeedTableIndex = 1;
        state.Bouncing = true;

        slot.Clear();
        AssertEqual<int>(0, state.SpeedMultiplier, "a kill empties SpeedMultiplier");
        AssertEqual<int>(0, state.Speed, "a kill empties Speed");
        AssertEqual<int>(0, state.JumpHeight, "a kill empties JumpHeight");
        AssertEqual<int>(0, state.DistanceAccumulator, "a kill empties DistanceAccumulator");
        AssertEqual<int>(0, state.SpeedTableIndex, "a kill empties SpeedTableIndex");
        AssertEqual<bool>(false, state.Bouncing, "a kill empties Bouncing");
        Console.WriteLine("Boyon record state: a kill empties every value the record holds.");
    }

    /// <summary>Runs the focused Boyon record-state check.</summary>
    [VerificationCommand("--boyon-record-state")]
    internal static int BoyonRecordStateCommand()
    {
        VerifyBoyonRecordState();
        return 0;
    }
}
