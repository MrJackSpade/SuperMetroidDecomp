namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Samus's Speed Booster progress (#1470): the boost stage, zero through <see cref="FullStage"/>, and the running
/// cadence counted down, in animation commands, to the next stage. Counting the cadence down at zero borrows from the
/// stage, dropping one stage with a full cadence ahead of it; that is reachable when the echo's queue selection resets
/// a newly full boost's cadence to zero.
/// </summary>
public readonly record struct SamusSpeedBoostCounter
{
    /// <summary>The full boost stage: active boosting for blocks, contact and the echo, and enough to store a shinespark.</summary>
    public const int FullStage = 4;

    /// <summary>The cadence a borrow leaves ahead of the dropped stage, and the largest cadence.</summary>
    private const int CadenceAfterBorrow = 255;

    /// <summary>Holds <paramref name="stage"/>, zero through <see cref="FullStage"/>, and <paramref name="cadence"/>, 0 through 255.</summary>
    public SamusSpeedBoostCounter(int stage, int cadence)
    {
        Stage = Ensure.BetweenInclusive(0, FullStage, stage);
        Cadence = Ensure.BetweenInclusive(0, CadenceAfterBorrow, cadence);
    }

    /// <summary>The full boost stage with no cadence left, as a shinespark windup publishes it.</summary>
    public static SamusSpeedBoostCounter AtFullStage => new(FullStage, 0);

    /// <summary>The boost stage, zero through <see cref="FullStage"/>.</summary>
    [IntegerQuantity("how many stages the boost has reached: it rises by one, borrows down by one and is compared with its range, not one of a set of categories")]
    public int Stage { get; }

    /// <summary>Animation commands left before the next stage, 0 through 255.</summary>
    public int Cadence { get; }

    /// <summary>Whether the boost is at its full stage.</summary>
    public bool IsAtFullStage => Stage == FullStage;

    /// <summary>
    /// One animation command's countdown, which borrows a stage when the cadence is already zero.
    /// </summary>
    /// <returns>The counted-down counter and whether its cadence is now zero.</returns>
    /// <exception cref="InvalidOperationException">Stage zero with no cadence left has no stage to borrow from.</exception>
    public SpeedBoostCountdown DecrementForAnimationCommand()
    {
        SamusSpeedBoostCounter next = Cadence > 0
            ? new(Stage, Cadence - 1)
            : Stage > 0
                ? new(Stage - 1, CadenceAfterBorrow)
                : throw new InvalidOperationException("A stage-zero Speed Booster counter with no cadence left has no stage to borrow from.");
        return new SpeedBoostCountdown(next, next.Cadence == 0);
    }

    /// <summary>The counter one stage higher; the caller checks that the boost is not yet full.</summary>
    public SamusSpeedBoostCounter AdvanceStage() => new(Stage + 1, Cadence);

    /// <summary>The same stage with its cadence reset to <paramref name="cadence"/>, 0 through 255.</summary>
    public SamusSpeedBoostCounter WithCadence(int cadence) => new(Stage, cadence);
}
