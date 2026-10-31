using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Bull;
namespace SuperMetroid.Verification.Enemies.Bull;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks that a kill empties the Bull values the cartridge keeps in the enemy's common record.</summary>
internal static class BullRecordStateVerification
{
    /// <summary>
    /// Installs a Bull state on a slot with its record-clearing operation, gives every value the record holds a
    /// nonzero value, and confirms a kill empties each one.
    /// </summary>
    internal static void VerifyBullRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()));
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new BullEnemyState(previous: null);
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.Function = Enum.GetValues<BullEnemyFunction>()[^1];
        state.Acceleration = 1;
        state.AccelerationDelta = 1;
        state.Speed = 1;
        state.ActivationTimer = WrappingCountdown.Start(1);
        state.AccelerationIntervalTimer = WrappingCountdown.Start(1);

        slot.Clear();
        AssertEqual<BullEnemyFunction?>(null, state.Function, "a kill empties Function");
        AssertEqual<int>(0, state.Acceleration, "a kill empties Acceleration");
        AssertEqual<int>(0, state.AccelerationDelta, "a kill empties AccelerationDelta");
        AssertEqual<int>(0, state.Speed, "a kill empties Speed");
        AssertEqual<WrappingCountdown>(WrappingCountdown.Start(0), state.ActivationTimer, "a kill empties ActivationTimer");
        AssertEqual<WrappingCountdown>(WrappingCountdown.Start(0), state.AccelerationIntervalTimer, "a kill empties AccelerationIntervalTimer");
        Console.WriteLine("Bull record state: a kill empties every value the record holds.");
    }

    /// <summary>Runs the focused Bull record-state check.</summary>
    [VerificationCommand("--bull-record-state")]
    internal static int BullRecordStateCommand()
    {
        VerifyBullRecordState();
        return 0;
    }
}
