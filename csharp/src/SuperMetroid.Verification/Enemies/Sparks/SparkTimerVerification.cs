using SuperMetroid.Core.Enemies.Sparks;
namespace SuperMetroid.Verification.Enemies.Sparks;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the Spark's owned countdown: the cartridge's <c>DEC; BEQ</c> expiry and its wrap from zero.</summary>
internal static class SparkTimerVerification
{
    /// <summary>
    /// A countdown at one expires on its next update and keeps its value for the reset that follows; a countdown at
    /// zero wraps to $FFFF instead of expiring.
    /// </summary>
    internal static void VerifySparkTimer()
    {
        var state = new SparkEnemyState { ActionTimer = 1 };
        AssertTrue(SparkBehavior.DecrementSparkTimerAndTestExpired(state), "a countdown at one expires");
        AssertEqual(1, state.ActionTimer, "the expiring update leaves the countdown for its reset");

        state.ActionTimer = 0;
        AssertTrue(!SparkBehavior.DecrementSparkTimerAndTestExpired(state), "a countdown at zero does not expire");
        AssertEqual(-1, state.ActionTimer, "a countdown at zero steps below zero");
        Console.WriteLine("Spark timer: the expiry and the zero wrap match.");
    }

    /// <summary>Runs the focused Spark timer check.</summary>
    [VerificationCommand("--spark-timer")]
    internal static int SparkTimerCommand()
    {
        VerifySparkTimer();
        return 0;
    }
}
