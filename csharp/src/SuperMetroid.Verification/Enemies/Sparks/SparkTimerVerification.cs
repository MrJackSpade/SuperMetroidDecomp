using SuperMetroid.Core.Enemies.Sparks;
namespace SuperMetroid.Verification.Enemies.Sparks;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the Spark's owned countdown: it expires on the update whose step would reach zero.</summary>
internal static class SparkTimerVerification
{
    /// <summary>
    /// A countdown at one expires on its next update and keeps its value for the reset that follows; a counting Spark
    /// never waits nothing, so a countdown at zero is refused (#1846).
    /// </summary>
    internal static void VerifySparkTimer()
    {
        var state = new SparkEnemyState { ActionTimer = 1 };
        AssertTrue(SparkBehavior.DecrementSparkTimerAndTestExpired(state), "a countdown at one expires");
        AssertEqual(1, state.ActionTimer, "the expiring update leaves the countdown for its reset");

        state.ActionTimer = 0;
        AssertThrows<InvalidOperationException>(() => SparkBehavior.DecrementSparkTimerAndTestExpired(state), "a countdown at zero is refused");
        Console.WriteLine("Spark timer: the expiry matches and a run-out countdown is refused.");
    }

    /// <summary>Runs the focused Spark timer check.</summary>
    [VerificationCommand("--spark-timer")]
    internal static int SparkTimerCommand()
    {
        VerifySparkTimer();
        return 0;
    }
}
