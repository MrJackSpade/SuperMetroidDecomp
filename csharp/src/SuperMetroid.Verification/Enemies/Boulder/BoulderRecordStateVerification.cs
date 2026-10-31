using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Boulder;
namespace SuperMetroid.Verification.Enemies.Boulder;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks that a kill empties the Boulder values the cartridge keeps in the enemy's common record.</summary>
internal static class BoulderRecordStateVerification
{
    /// <summary>
    /// Installs a Boulder state on a slot with its record-clearing operation, gives every value the record holds a
    /// nonzero value, and confirms a kill empties each one.
    /// </summary>
    internal static void VerifyBoulderRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()));
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new BoulderEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.Function = Enum.GetValues<BoulderAiFunction>()[^1];
        state.TravelSpeedAccumulator = 1;
        state.BounceSpeedAccumulator = 1;
        state.BounceCounter = WrappingCountdown.Start(1);
        state.Roll = BoulderRoll.Left;

        slot.Clear();
        AssertEqual<BoulderAiFunction?>(null, state.Function, "a kill empties Function");
        AssertEqual<int>(0, state.TravelSpeedAccumulator, "a kill empties TravelSpeedAccumulator");
        AssertEqual<int>(0, state.BounceSpeedAccumulator, "a kill empties BounceSpeedAccumulator");
        AssertEqual<WrappingCountdown>(WrappingCountdown.Start(0), state.BounceCounter, "a kill empties BounceCounter");
        AssertEqual(BoulderRoll.Right, state.Roll, "a kill empties the roll");
        Console.WriteLine("Boulder record state: a kill empties every value the record holds.");
    }

    /// <summary>Runs the focused Boulder record-state check.</summary>
    [VerificationCommand("--boulder-record-state")]
    internal static int BoulderRecordStateCommand()
    {
        VerifyBoulderRecordState();
        return 0;
    }
}
