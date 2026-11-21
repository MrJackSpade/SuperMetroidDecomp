using static SuperMetroid.Core.Samus.Movement.SamusRunningCadenceDefinitions;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>How Samus's running cadences are chosen and read: the frame delays a run animates by, and a boost stage's restart.</summary>
internal static class SamusRunningCadences
{
<<<<<<< ours
    /// <summary>The cadence of each authored boost stage, by its stage.</summary>
    private static readonly Dictionary<int, SamusBoostCadence> CadenceOfStage = StagesOf(Timings);
||||||| base
    /// <summary>The cadence of each authored boost stage, by its stage.</summary>
    private static readonly Dictionary<int, SamusRunningCadence> CadenceOfStage = StagesOf(Timings);
=======
    /// <summary>
    /// The cadence of each authored boost stage, at its stage: the catalog's boost stages, checked once to name each of the
    /// <see cref="SamusSpeedBoostCounter.AuthoredStages"/> stages exactly once, which every boost counter and echo restart
    /// stays among.
    /// </summary>
    private static readonly SpeedBoostCadence[] CadenceOfStage = StagesOf(Timings);
>>>>>>> theirs

    /// <summary>The cadence the boost restarts on after its echo's sound request found <paramref name="echo"/>, as <paramref name="restart"/> picks it.</summary>
    /// <param name="restart">Samus's rule for how the boost restarts after its echo.</param>
    /// <param name="echo">What the echo's sound request found.</param>
<<<<<<< ours
    internal static SamusBoostCadence CadenceAfterEcho(BoostRestartAfterEcho restart, SoundQueueAdmission echo) =>
||||||| base
    internal static SamusRunningCadence CadenceAfterEcho(BoostRestartAfterEcho restart, SoundQueueAdmission echo) =>
=======
    internal static SpeedBoostCadence CadenceAfterEcho(BoostRestartAfterEcho restart, SoundQueueAdmission echo) =>
>>>>>>> theirs
        Ensure.NotNull(restart).PicksUnauthoredCadence(echo)
<<<<<<< ours
            ? new SamusBoostCadence(SamusRunningCadence.UnauthoredBoost, UnauthoredBoostCadence.CountdownAfterAdvance)
            : SpeedBoostCadence(restart.AuthoredStageAfterEcho(echo));
||||||| base
            ? SamusRunningCadence.UnauthoredBoost
            : SpeedBoostCadence(restart.AuthoredStageAfterEcho(echo));
=======
            ? SpeedBoostCadence.Unauthored
            : StageCadence(restart.AuthoredStageAfterEcho(echo));
>>>>>>> theirs

<<<<<<< ours
    /// <summary>The cadence of authored boost stage <paramref name="stage"/> and the countdown the boost restarts with on it.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The stage is not one of the authored stages.</exception>
    internal static SamusBoostCadence SpeedBoostCadence(int stage) =>
        CadenceOfStage.TryGetValue(stage, out SamusBoostCadence cadence)
            ? cadence
            : throw new ArgumentOutOfRangeException(nameof(stage), stage, "The boost has five authored stages.");
||||||| base
    /// <summary>The cadence of authored boost stage <paramref name="stage"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The stage is not one of the authored stages.</exception>
    internal static SamusRunningCadence SpeedBoostCadence(int stage) =>
        CadenceOfStage.TryGetValue(stage, out SamusRunningCadence cadence)
            ? cadence
            : throw new ArgumentOutOfRangeException(nameof(stage), stage, "The boost has five authored stages.");
=======
    /// <summary>The cadence of authored boost stage <paramref name="stage"/>, one of the counter's stages.</summary>
    /// <param name="stage">The stage, zero through four.</param>
    internal static SpeedBoostCadence StageCadence(int stage) => CadenceOfStage[stage];
>>>>>>> theirs

<<<<<<< ours
    /// <summary>The delay, in ticks, of running frame <paramref name="frame"/> on <paramref name="cadence"/>.</summary>
    /// <param name="unauthored">The session's unauthored Speed Booster cadence, which times the unauthored cadence's frames.</param>
    /// <param name="cadence">The cadence the run animates by.</param>
||||||| base
    /// <summary>The countdown, 0 through 2, the boost restarts with on <paramref name="cadence"/>.</summary>
    /// <exception cref="InvalidOperationException">The cadence is ordinary running's, which is no boost cadence.</exception>
    internal static int CountdownAfterAdvance(SamusRunningCadence cadence) =>
        cadence == SamusRunningCadence.UnauthoredBoost
            ? UnauthoredBoostCadence.CountdownAfterAdvance
            : (TimingOf(cadence).Boost ?? throw new InvalidOperationException("Ordinary running is not a boost stage.")).CountdownAfterAdvance;

    /// <summary>The delay, in ticks, of running frame <paramref name="frame"/> on <paramref name="cadence"/>.</summary>
    /// <param name="unauthored">The session's unauthored Speed Booster cadence, which times the unauthored cadence's frames.</param>
    /// <param name="cadence">The cadence the run animates by.</param>
=======
    /// <summary>The delay, in ticks, of ordinary running's frame <paramref name="frame"/>.</summary>
>>>>>>> theirs
    /// <param name="frame">The running frame, zero through nine.</param>
    internal static int OrdinaryDelay(int frame) => Timings[SamusRunningCadence.Ordinary].Delays[frame];

<<<<<<< ours
    /// <summary>The cadence of each boost stage <paramref name="timings"/> holds, with its restart countdown, by its stage.</summary>
    private static Dictionary<int, SamusBoostCadence> StagesOf(IReadOnlyDictionary<SamusRunningCadence, SamusRunningCadenceTiming> timings)
||||||| base
    /// <summary>The cadence of each boost stage <paramref name="timings"/> holds, by its stage.</summary>
    private static Dictionary<int, SamusRunningCadence> StagesOf(IReadOnlyDictionary<SamusRunningCadence, SamusRunningCadenceTiming> timings)
=======
    /// <summary>The cadence of each authored boost stage <paramref name="timings"/> holds, at its stage.</summary>
    /// <exception cref="InvalidDataException">The catalog does not name each authored stage exactly once.</exception>
    private static SpeedBoostCadence[] StagesOf(IReadOnlyDictionary<SamusRunningCadence, SamusRunningCadenceTiming> timings)
>>>>>>> theirs
    {
<<<<<<< ours
        var stages = new Dictionary<int, SamusBoostCadence>();
        foreach ((SamusRunningCadence cadence, SamusRunningCadenceTiming timing) in timings)
||||||| base
        var stages = new Dictionary<int, SamusRunningCadence>();
        foreach ((SamusRunningCadence cadence, SamusRunningCadenceTiming timing) in timings)
=======
        var stages = new SpeedBoostCadence?[SamusSpeedBoostCounter.AuthoredStages];
        foreach (SamusRunningCadenceTiming timing in timings.Values)
>>>>>>> theirs
        {
<<<<<<< ours
            if (timing.Boost is { } boost)
                stages.Add(boost.Stage, new SamusBoostCadence(cadence, boost.CountdownAfterAdvance));
||||||| base
            if (timing.Boost is { } boost)
                stages.Add(boost.Stage, cadence);
=======
            if (timing.Boost is not { } boost)
                continue;
            if (boost.Stage >= stages.Length || stages[boost.Stage] is not null)
                throw new InvalidDataException($"The running cadences name boost stage {boost.Stage} outside the authored stages or twice.");
            stages[boost.Stage] = SpeedBoostCadence.Authored(timing, boost);
>>>>>>> theirs
        }

        var cadences = new SpeedBoostCadence[stages.Length];
        for (int stage = 0; stage < stages.Length; stage++)
            cadences[stage] = stages[stage] ?? throw new InvalidDataException($"The running cadences name no boost stage {stage}.");
        return cadences;
    }
}
