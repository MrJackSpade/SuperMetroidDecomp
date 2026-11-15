using static SuperMetroid.Core.Samus.Movement.SamusRunningCadenceDefinitions;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>How Samus's running cadences are chosen and read: the frame delays a run animates by, and a boost stage's restart.</summary>
internal static class SamusRunningCadences
{
    /// <summary>The cadence the boost restarts on after its echo's sound request found <paramref name="echo"/>, as <paramref name="restart"/> picks it.</summary>
    /// <param name="restart">Samus's rule for how the boost restarts after its echo.</param>
    /// <param name="echo">What the echo's sound request found.</param>
    internal static SamusRunningCadence CadenceAfterEcho(BoostRestartAfterEcho restart, SoundQueueAdmission echo) =>
        Ensure.NotNull(restart).StageAfterEcho(echo) is { } stage ? SpeedBoostCadence(stage) : SamusRunningCadence.UnauthoredBoost;

    /// <summary>The cadence of authored boost stage <paramref name="stage"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The stage is not one of the authored stages.</exception>
    internal static SamusRunningCadence SpeedBoostCadence(int stage) => stage switch
    {
        0 => SamusRunningCadence.BoostStage0,
        1 => SamusRunningCadence.BoostStage1,
        2 => SamusRunningCadence.BoostStage2,
        3 => SamusRunningCadence.BoostStage3,
        4 => SamusRunningCadence.BoostStage4,
        _ => throw new ArgumentOutOfRangeException(nameof(stage), stage, "The boost has five authored stages."),
    };

    /// <summary>The countdown, 0 through 2, the boost restarts with on <paramref name="cadence"/>.</summary>
    /// <exception cref="InvalidOperationException">The cadence is ordinary running's, which is no boost cadence.</exception>
    internal static int CountdownAfterAdvance(SamusRunningCadence cadence) => cadence switch
    {
        SamusRunningCadence.UnauthoredBoost => UnauthoredBoostCadence.CountdownAfterAdvance,
        SamusRunningCadence.BoostStage0 => SamusRunningCadenceDefinitions.CountdownAfterAdvance[0],
        SamusRunningCadence.BoostStage1 => SamusRunningCadenceDefinitions.CountdownAfterAdvance[1],
        SamusRunningCadence.BoostStage2 => SamusRunningCadenceDefinitions.CountdownAfterAdvance[2],
        SamusRunningCadence.BoostStage3 => SamusRunningCadenceDefinitions.CountdownAfterAdvance[3],
        SamusRunningCadence.BoostStage4 => SamusRunningCadenceDefinitions.CountdownAfterAdvance[4],
        SamusRunningCadence.Ordinary => throw new InvalidOperationException("Ordinary running is not a boost stage."),
        _ => throw new InvalidOperationException($"Undefined {nameof(SamusRunningCadence)} {cadence}."),
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
