using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.Botwoon;
namespace SuperMetroid.Verification.Bosses.Botwoon;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks that a kill empties the Botwoon values the cartridge keeps in the enemy's common record.</summary>
internal static class BotwoonRecordStateVerification
{
    /// <summary>
    /// Installs a Botwoon state on a slot with its record-clearing operation, gives every value the record holds a
    /// nonzero value, and confirms a kill empties each one.
    /// </summary>
    internal static void VerifyBotwoonRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()));
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new BotwoonEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.Function = Enum.GetValues<BotwoonEnemyFunction>()[^1];
        state.MovementFunction = Enum.GetValues<BotwoonMovementFunction>()[^1];
        state.HeadFunction = Enum.GetValues<BotwoonHeadFunction>()[^1];

        slot.Clear();
        AssertEqual<BotwoonEnemyFunction?>(null, state.Function, "a kill empties Function");
        AssertEqual<BotwoonMovementFunction?>(null, state.MovementFunction, "a kill empties MovementFunction");
        AssertEqual<BotwoonHeadFunction?>(null, state.HeadFunction, "a kill empties HeadFunction");
        Console.WriteLine("Botwoon record state: a kill empties every value the record holds.");
    }

    /// <summary>Runs the focused Botwoon record-state check.</summary>
    [VerificationCommand("--botwoon-record-state")]
    internal static int BotwoonRecordStateCommand()
    {
        VerifyBotwoonRecordState();
        return 0;
    }
}
