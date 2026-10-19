using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Hoppers;
namespace SuperMetroid.Verification.Enemies.Hoppers;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that a kill empties the Hopper values the cartridge keeps in the enemy's common record.</summary>
internal static class HopperRecordStateVerification
{
    /// <summary>
    /// Installs a Hopper state on a slot with its record-clearing operation, gives every value the record holds a
    /// nonzero value, and confirms a kill empties each one.
    /// </summary>
    internal static void VerifyHopperRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new HopperEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.Function = Enum.GetValues<HopperEnemyFunction>()[^1];
        state.YSpeedTableIndex = 1;
        state.XVelocity = 1;
        state.YSpeedTableIndexDelta = 1;

        slot.Clear();
        AssertEqual<HopperEnemyFunction?>(null, state.Function, "a kill empties Function");
        AssertEqual<int>(0, state.YSpeedTableIndex, "a kill empties YSpeedTableIndex");
        AssertEqual<short>(0, state.XVelocity, "a kill empties XVelocity");
        AssertEqual<int>(0, state.YSpeedTableIndexDelta, "a kill empties YSpeedTableIndexDelta");
        Console.WriteLine("Hopper record state: a kill empties every value the record holds.");
    }

    /// <summary>Runs the focused Hopper record-state check.</summary>
    [VerificationCommand("--hopper-record-state")]
    internal static int HopperRecordStateCommand()
    {
        VerifyHopperRecordState();
        return 0;
    }
}
