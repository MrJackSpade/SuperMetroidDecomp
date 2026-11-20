namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Samus's Speed Booster progress (#1470): the boost stage, zero through <see cref="FullStage"/>, and the countdown of
/// runs, one per run the running animation completes, to the next stage. How a run counted off an empty countdown takes
/// the boost down a stage is <see cref="BoostRestartAfterEcho"/>'s.
/// </summary>
public readonly record struct SamusSpeedBoostCounter
{
    /// <summary>The full boost stage: active boosting for blocks, contact and the echo, and enough to store a shinespark.</summary>
    private const int FullStage = 4;

    /// <summary>The authored boost stages, zero through <see cref="FullStage"/>.</summary>
    public const int AuthoredStages = FullStage + 1;

    /// <summary>
    /// Holds <paramref name="stage"/>, zero through <see cref="FullStage"/>, and <paramref name="countdown"/>, zero through
    /// <see cref="BoostRestartAfterEcho.CountdownAfterBorrow"/>.
    /// </summary>
    public SamusSpeedBoostCounter(int stage, int countdown)
    {
        Stage = Ensure.BetweenInclusive(0, FullStage, stage);
        Countdown = Ensure.BetweenInclusive(0, BoostRestartAfterEcho.CountdownAfterBorrow, countdown);
    }

    /// <summary>The full boost stage with no cadence left, as a shinespark windup publishes it.</summary>
    public static SamusSpeedBoostCounter AtFullStage => new(FullStage, 0);

    /// <summary>The boost stage, zero through <see cref="FullStage"/>.</summary>
    [IntegerQuantity("how many stages the boost has reached: it rises by one, drops by one and is compared with its range, not one of a set of categories")]
    public int Stage { get; }

    /// <summary>Runs the running animation completes before the next stage.</summary>
    private int Countdown { get; }

    /// <summary>Whether the boost is at its full stage.</summary>
    public bool IsAtFullStage => Stage == FullStage;

    /// <summary>One completed run's countdown, as <paramref name="restart"/> counts it.</summary>
    /// <param name="restart">Samus's rule for how the boost restarts and is taken down a stage.</param>
    /// <returns>The counted-down counter and whether its countdown is now zero.</returns>
    /// <exception cref="InvalidOperationException">Stage zero with no runs left has no stage to go down from.</exception>
    public SpeedBoostCountdown CountDownOneRun(BoostRestartAfterEcho restart)
    {
        BoostRunCountdown next = Ensure.NotNull(restart).CountDownOneRun(Stage, Countdown);
        var counter = new SamusSpeedBoostCounter(next.Stage, next.Countdown);
        return new SpeedBoostCountdown(counter, counter.Countdown == 0);
    }

    /// <summary>The counter one stage higher; it is advanced only while the boost is not yet full.</summary>
    public SamusSpeedBoostCounter AdvanceStage() => new(Stage + 1, Countdown);

    /// <summary>The same stage with its countdown restarted at <paramref name="countdown"/>.</summary>
    public SamusSpeedBoostCounter WithCountdown(int countdown) => new(Stage, countdown);
}
