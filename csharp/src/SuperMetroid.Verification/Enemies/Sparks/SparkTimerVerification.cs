using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Sparks;
namespace SuperMetroid.Verification.Enemies.Sparks;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the Spark's owned countdown: the cartridge's <c>DEC; BEQ</c> expiry, its wrap from zero, and the cleared record.</summary>
internal static class SparkTimerVerification
{
    /// <summary>
    /// A countdown at one expires on its next update and keeps its value for the reset that follows; a countdown at
    /// zero wraps to $FFFF instead of expiring; a kill empties the record.
    /// </summary>
    internal static void VerifySparkTimer()
    {
        var state = new SparkEnemyState { FunctionTimer = NativeCountdown.Start(1) };
        AssertTrue(SparkBehavior.DecrementSparkTimerAndTestExpired(state), "a countdown at one expires");
        AssertEqual((ushort)1, state.FunctionTimer.RawValue, "the expiring update leaves the countdown for its reset");

        state.FunctionTimer = NativeCountdown.Start(0);
        AssertTrue(!SparkBehavior.DecrementSparkTimerAndTestExpired(state), "a countdown at zero does not expire");
        AssertEqual(ushort.MaxValue, state.FunctionTimer.RawValue, "a countdown at zero wraps");

        var enemies = new RoomEnemySystem();
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        state.Function = SparkEnemyFunction.AlwaysActive;
        state.BaseFunctionTime = 0x8000;
        slot.SetSpeciesState(state, state.ClearWithRecord);
        slot.Clear();
        AssertEqual((0, (ushort)0), (state.BaseFunctionTime, state.FunctionTimer.RawValue), "a kill empties the base time and countdown");
        AssertEqual(null, state.Function, "a kill empties the function");
        Console.WriteLine("Spark timer: the expiry, the zero wrap and the cleared record match.");
    }

    /// <summary>Runs the focused Spark timer check.</summary>
    [VerificationCommand("--spark-timer")]
    internal static int SparkTimerCommand()
    {
        VerifySparkTimer();
        return 0;
    }
}
