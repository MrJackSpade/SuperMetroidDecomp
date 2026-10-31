using static SuperMetroid.Core.Samus.Movement.SamusRunningCadenceDefinitions;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>How Samus's running cadences are chosen and read: the frame delays a run animates by, and a boost stage's restart.</summary>
internal static class SamusRunningCadences
{
    /// <summary>
    /// The selection the Speed Booster echo's queue request leaves: the occupancy the request saw when admitted, or
    /// <see cref="SamusRunningCadenceDefinitions.RejectedEchoSelection"/> when the queue already held six requests.
    /// </summary>
    internal static int EchoQueueSelection(SoundQueueAdmission admission) =>
        admission.IsAdmitted ? admission.Occupancy : RejectedEchoSelection;

    /// <summary>The boost cadence <paramref name="selection"/> picks: an authored stage, or the unauthored cadence at five.</summary>
    /// <exception cref="InvalidDataException">The selection lies past what the echo's queue request can report.</exception>
    internal static SamusRunningCadence SpeedBoostCadence(int selection) => selection switch
    {
        0 => SamusRunningCadence.BoostStage0,
        1 => SamusRunningCadence.BoostStage1,
        2 => SamusRunningCadence.BoostStage2,
        3 => SamusRunningCadence.BoostStage3,
        4 => SamusRunningCadence.BoostStage4,
        MaximumSoundQueueSelection => SamusRunningCadence.UnauthoredBoost,
        _ => throw new InvalidDataException(
            $"Speed Booster selection {selection} lies past what the echo's queue request can report."),
    };

    /// <summary>The cadence countdown, 0 through 2, the boost restarts with after advancing onto <paramref name="selection"/>.</summary>
    /// <exception cref="InvalidDataException">The selection lies past what the echo's queue request can report.</exception>
    internal static int CountdownAfterAdvance(int selection) => SpeedBoostCadence(selection) switch
    {
        SamusRunningCadence.UnauthoredBoost => UnauthoredBoostCadence.CountdownAfterAdvance,
        SamusRunningCadence.BoostStage0 or SamusRunningCadence.BoostStage1 or SamusRunningCadence.BoostStage2 or
            SamusRunningCadence.BoostStage3 or SamusRunningCadence.BoostStage4 =>
            SamusRunningCadenceDefinitions.CountdownAfterAdvance[selection],
        SamusRunningCadence.Ordinary => throw new InvalidOperationException("Ordinary running is not a boost stage."),
        var undefined => throw new InvalidOperationException($"Undefined {nameof(SamusRunningCadence)} {undefined}."),
    };

    /// <summary>The delay, in ticks, of running frame <paramref name="frame"/> on <paramref name="cadence"/>.</summary>
    /// <param name="unauthored">The session's unauthored Speed Booster cadence, which times the unauthored cadence's frames.</param>
    /// <param name="cadence">The cadence the run animates by.</param>
    /// <param name="frame">The running frame, zero through nine.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frame"/> is not one of the ten running frames.</exception>
    internal static int Delay(UnauthoredBoostCadence unauthored, SamusRunningCadence cadence, int frame)
    {
        if ((uint)frame >= (uint)OrdinaryDelays.Count)
            throw new ArgumentOutOfRangeException(nameof(frame), frame, "A run has ten frames.");
        return cadence switch
        {
            SamusRunningCadence.Ordinary => OrdinaryDelays[frame],
            SamusRunningCadence.BoostStage0 => BoostStageDelays[0][frame],
            SamusRunningCadence.BoostStage1 => BoostStageDelays[1][frame],
            SamusRunningCadence.BoostStage2 => BoostStageDelays[2][frame],
            SamusRunningCadence.BoostStage3 => BoostStageDelays[3][frame],
            SamusRunningCadence.BoostStage4 => BoostStageDelays[4][frame],
            SamusRunningCadence.UnauthoredBoost => unauthored.Delay(frame),
            _ => throw new InvalidOperationException($"Undefined {nameof(SamusRunningCadence)} {cadence}."),
        };
    }
}
