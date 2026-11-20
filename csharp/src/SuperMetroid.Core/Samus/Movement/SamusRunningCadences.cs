using static SuperMetroid.Core.Samus.Movement.SamusRunningCadenceDefinitions;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>How Samus's running cadences are chosen and read: the frame delays a run animates by, and a boost stage's restart.</summary>
internal static class SamusRunningCadences
{
    /// <summary>The cadence of each authored boost stage, by its stage.</summary>
    private static readonly Dictionary<int, SamusRunningCadence> CadenceOfStage = StagesOf(Timings);

    /// <summary>The cadence the boost restarts on after its echo's sound request found <paramref name="echo"/>, as <paramref name="restart"/> picks it.</summary>
    /// <param name="restart">Samus's rule for how the boost restarts after its echo.</param>
    /// <param name="echo">What the echo's sound request found.</param>
    internal static SamusRunningCadence CadenceAfterEcho(BoostRestartAfterEcho restart, SoundQueueAdmission echo) =>
        Ensure.NotNull(restart).PicksUnauthoredCadence(echo)
            ? SamusRunningCadence.UnauthoredBoost
            : SpeedBoostCadence(restart.AuthoredStageAfterEcho(echo));

    /// <summary>The cadence of authored boost stage <paramref name="stage"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The stage is not one of the authored stages.</exception>
    internal static SamusRunningCadence SpeedBoostCadence(int stage) =>
        CadenceOfStage.TryGetValue(stage, out SamusRunningCadence cadence)
            ? cadence
            : throw new ArgumentOutOfRangeException(nameof(stage), stage, "The boost has five authored stages.");

    /// <summary>The countdown, 0 through 2, the boost restarts with on <paramref name="cadence"/>.</summary>
    /// <exception cref="InvalidOperationException">The cadence is ordinary running's, which is no boost cadence.</exception>
    internal static int CountdownAfterAdvance(SamusRunningCadence cadence) =>
        cadence == SamusRunningCadence.UnauthoredBoost
            ? UnauthoredBoostCadence.CountdownAfterAdvance
            : (TimingOf(cadence).Boost ?? throw new InvalidOperationException("Ordinary running is not a boost stage.")).CountdownAfterAdvance;

    /// <summary>The delay, in ticks, of running frame <paramref name="frame"/> on <paramref name="cadence"/>.</summary>
    /// <param name="unauthored">The session's unauthored Speed Booster cadence, which times the unauthored cadence's frames.</param>
    /// <param name="cadence">The cadence the run animates by.</param>
    /// <param name="frame">The running frame, zero through nine.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frame"/> is not one of the ten running frames.</exception>
    internal static int Delay(UnauthoredBoostCadence unauthored, SamusRunningCadence cadence, int frame)
    {
        IReadOnlyList<int> ordinary = Timings[SamusRunningCadence.Ordinary].Delays;
        if (frame < 0 || frame >= ordinary.Count)
            throw new ArgumentOutOfRangeException(nameof(frame), frame, "A run has ten frames.");
        return cadence == SamusRunningCadence.UnauthoredBoost
            ? Ensure.NotNull(unauthored).Delay(frame)
            : TimingOf(cadence).Delays[frame];
    }

    /// <summary>The cadence of each boost stage <paramref name="timings"/> holds, by its stage.</summary>
    private static Dictionary<int, SamusRunningCadence> StagesOf(IReadOnlyDictionary<SamusRunningCadence, SamusRunningCadenceTiming> timings)
    {
        var stages = new Dictionary<int, SamusRunningCadence>();
        foreach ((SamusRunningCadence cadence, SamusRunningCadenceTiming timing) in timings)
        {
            if (timing.Boost is { } boost)
                stages.Add(boost.Stage, cadence);
        }

        return stages;
    }

    /// <summary>The authored timing of <paramref name="cadence"/>.</summary>
    /// <exception cref="InvalidOperationException">The cadence is not an authored one.</exception>
    private static SamusRunningCadenceTiming TimingOf(SamusRunningCadence cadence) =>
        Timings.TryGetValue(cadence, out SamusRunningCadenceTiming? timing)
            ? timing
            : throw new InvalidOperationException($"Undefined {nameof(SamusRunningCadence)} {cadence}.");
}
