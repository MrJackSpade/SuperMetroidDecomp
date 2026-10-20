using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Sbug;
namespace SuperMetroid.Verification.Enemies.Sbug;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that a kill empties the Sbug values the cartridge keeps in the enemy's common record.</summary>
internal static class SbugRecordStateVerification
{
    /// <summary>
    /// Installs a Sbug state on a slot with its record-clearing operation, gives every value the record holds a
    /// nonzero value, and confirms a kill empties each one.
    /// </summary>
    internal static void VerifySbugRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new SbugEnemyState(slot);
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.StraightMovementTimer = NativeCountdown.Start(1);
        state.Function = Enum.GetValues<SbugEnemyFunction>()[^1];
        state.FaceCustomHeading(SnesAngle.FromTableIndex(1));
        state.MovementTimer = NativeCountdown.Start(1);

        slot.Clear();
        AssertEqual<NativeCountdown>(NativeCountdown.Start(0), state.StraightMovementTimer, "a kill empties StraightMovementTimer");
        AssertEqual<SbugEnemyFunction?>(null, state.Function, "a kill empties Function");
        AssertEqual<SnesAngle>(SnesAngle.FromTableIndex(0), state.CustomHeading, "a kill empties CustomHeading");
        AssertEqual<NativeCountdown>(NativeCountdown.Start(0), state.MovementTimer, "a kill empties MovementTimer");
        Console.WriteLine("Sbug record state: a kill empties every value the record holds.");
    }

    /// <summary>Runs the focused Sbug record-state check.</summary>
    [VerificationCommand("--sbug-record-state")]
    internal static int SbugRecordStateCommand()
    {
        VerifySbugRecordState();
        return 0;
    }
}
