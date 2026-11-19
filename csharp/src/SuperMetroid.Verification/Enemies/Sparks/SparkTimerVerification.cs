using SuperMetroid.Core;
using SuperMetroid.Core.Enemies.Sparks;
using SuperMetroid.Core.Animation;
namespace SuperMetroid.Verification.Enemies.Sparks;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the Spark's owned countdown: it expires on the update whose step would reach zero.</summary>
internal static class SparkTimerVerification
{
    /// <summary>
    /// A wait with one update left expires on its next update; a counting Spark never waits nothing, so a wait already over
    /// is refused (#1846).
    /// </summary>
    internal static void VerifySparkTimer()
    {
        var state = new SparkEnemyState(SparkAction.AlwaysActive, new(SparkAnimations.StartingWith(SparkSegment.Active), AnimationPlace.StartOfAnimation)) { ActionTimer = UpdateCountdown.Lasting(1) };
        AssertTrue(SparkBehavior.DecrementSparkTimerAndTestExpired(state), "a countdown at one expires");

        state.ActionTimer = UpdateCountdown.Ended();
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
