namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How the Speed Booster's boost restarts once it reaches its full stage and requests its echo's sound, and how a
/// completed run counted off an empty countdown takes the boost down a stage (#1911). Samus's run climbs the boost
/// stages by counting completed runs down; reaching the full stage requests the echo's sound, and the cadence the boost
/// then restarts on is not the full stage's: an admitted request picks the authored stage numbered by how many requests
/// it found waiting, or the unauthored cadence (<see cref="UnauthoredBoostCadence"/>) when it found as many requests
/// waiting as there are authored stages, and a request turned away picks stage three's. The restart's countdown follows
/// the cadence picked, so the unauthored cadence restarts with none left: its next completed run takes the boost down to
/// the stage below full, with <see cref="CountdownAfterBorrow"/> runs to climb back. The stage reached is what Speed
/// Booster blocks, boost contact damage and a stored shinespark test.
/// </summary>
/// <remarks>
/// A rule with no state, created with each Samus (<c>SamusCompatibility</c>) and given to her horizontal speeds.
/// Consulted by her running animation after movement, when a completed run with Dash held counts down the boost and,
/// on reaching the full stage, the echo's sound request answers. Evidence: the original keeps the stage in the high
/// byte and the countdown in the low byte of one word and decrements the whole word, so a countdown of zero borrows from
/// the stage and leaves the low byte's largest value; it then indexes its restart and cadence tables by the value the
/// room-library sound request leaves (<c>$90:85A6</c>), which is the occupancy the request saw, or the echo's own sound
/// number, three, when the queue turned it away. The running cadence check (<c>VerifyRunningCadence</c>) replays every
/// stage, countdown and gate against that word arithmetic and every queue report against the cartridge's tables, and
/// the Ridley full movie's and the 13% movie's running cadences depend on the occupancy.
/// </remarks>
[CompatibilityService("the Speed Booster's boost restarts on the cadence its echo's sound request picks by how many requests it found waiting, and a run counted off an empty countdown takes the boost down a stage")]
public sealed class BoostRestartAfterEcho
{
    /// <summary>The runs a boost taken down a stage counts down before it climbs back.</summary>
    public const int CountdownAfterBorrow = 255;

    /// <summary>The authored stage a turned-away echo request picks.</summary>
    public const int TurnedAwayEchoStage = 3;

    /// <summary>The authored boost stages, the full stage among them.</summary>
    private readonly int _authoredStages;

    /// <summary>Creates the rule for a boost of <paramref name="authoredStages"/> authored stages.</summary>
    /// <param name="authoredStages">The authored stages, more than <see cref="TurnedAwayEchoStage"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">The stages do not include the one a turned-away request picks.</exception>
    public BoostRestartAfterEcho(int authoredStages) =>
        _authoredStages = Ensure.BetweenInclusive(TurnedAwayEchoStage + 1, int.MaxValue, authoredStages);

    /// <summary>
    /// The authored stage whose cadence the boost restarts on after its echo's sound request, or none for the unauthored
    /// cadence: an admitted request picks the stage numbered by the requests it found waiting, and the unauthored cadence
    /// when it found as many as there are authored stages; a turned-away request picks <see cref="TurnedAwayEchoStage"/>.
    /// </summary>
    /// <param name="echo">What the echo's sound request found.</param>
    /// <exception cref="InvalidOperationException">
    /// The request was admitted with more requests waiting than the echo's priority admits.
    /// </exception>
    public int? StageAfterEcho(SoundQueueAdmission echo)
    {
        if (!echo.IsAdmitted)
            return TurnedAwayEchoStage;
        if (echo.Occupancy < _authoredStages)
            return echo.Occupancy;
        if (echo.Occupancy == _authoredStages)
            return null;
        throw new InvalidOperationException(
            $"The echo's sound request was admitted with {echo.Occupancy} requests waiting, more than its priority admits.");
    }

    /// <summary>
    /// Counts one completed run off a boost at <paramref name="stage"/> with <paramref name="countdown"/> runs left: a
    /// countdown left goes down by one, and an empty one takes the boost down a stage with
    /// <see cref="CountdownAfterBorrow"/> runs to climb back.
    /// </summary>
    /// <param name="stage">The boost stage, zero or more.</param>
    /// <param name="countdown">The runs left before the next stage, zero through <see cref="CountdownAfterBorrow"/>.</param>
    /// <exception cref="InvalidOperationException">A stage-zero boost with no runs left has no stage to go down from.</exception>
    public BoostRunCountdown CountDownOneRun(int stage, int countdown)
    {
        Ensure.AtLeastZero(stage);
        Ensure.BetweenInclusive(0, CountdownAfterBorrow, countdown);
        if (countdown > 0)
            return new BoostRunCountdown(stage, countdown - 1);
        if (stage > 0)
            return new BoostRunCountdown(stage - 1, CountdownAfterBorrow);
        throw new InvalidOperationException("A stage-zero Speed Booster boost with no runs left has no stage to go down from.");
    }
}
