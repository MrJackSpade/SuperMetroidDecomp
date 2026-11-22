using static SuperMetroid.Core.Samus.Movement.SamusRunningCadenceDefinitions;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>How Samus's running cadences are chosen and read: the frame delays a run animates by, and a boost stage's restart.</summary>
internal static partial class SamusRunningCadences
{
    /// <summary>
    /// The cadence of each authored boost stage, at its stage: the catalog's boost stages, checked once to name each of the
    /// <see cref="SamusSpeedBoostCounter.AuthoredStages"/> stages exactly once, which every boost counter and echo restart
    /// stays among.
    /// </summary>
    private static readonly SpeedBoostCadence[] CadenceOfStage = StagesOf(Timings);

    /// <summary>The cadence the boost restarts on after its echo's sound request found <paramref name="echo"/>, as <paramref name="restart"/> picks it.</summary>
    /// <param name="restart">Samus's rule for how the boost restarts after its echo.</param>
    /// <param name="echo">What the echo's sound request found.</param>
    internal static SpeedBoostCadence CadenceAfterEcho(BoostRestartAfterEcho restart, SoundQueueAdmission echo) =>
        Ensure.NotNull(restart).PicksUnauthoredCadence(echo)
            ? SpeedBoostCadence.Unauthored
            : StageCadence(restart.AuthoredStageAfterEcho(echo));

    /// <summary>The cadence of authored boost stage <paramref name="stage"/>, one of the counter's stages.</summary>
    /// <param name="stage">The stage, zero through four.</param>
    internal static SpeedBoostCadence StageCadence(int stage) => CadenceOfStage[stage];

    /// <summary>The delay, in ticks, of ordinary running's frame <paramref name="frame"/>.</summary>
    /// <param name="frame">The running frame, zero through nine.</param>
    internal static int OrdinaryDelay(int frame) => Timings[SamusRunningCadence.Ordinary].Delays[frame];

    /// <summary>The cadence of each authored boost stage <paramref name="timings"/> holds, at its stage.</summary>
    /// <exception cref="InvalidDataException">The catalog does not name each authored stage exactly once.</exception>
    private static SpeedBoostCadence[] StagesOf(IReadOnlyDictionary<SamusRunningCadence, SamusRunningCadenceTiming> timings)
    {
        var cadences = new SpeedBoostCadence[SamusSpeedBoostCounter.AuthoredStages];
        var named = new bool[cadences.Length];
        foreach (SamusRunningCadenceTiming timing in timings.Values)
        {
            if (timing.Boost is not { } boost)
                continue;
            if (boost.Stage >= cadences.Length || named[boost.Stage])
                throw new InvalidDataException($"The running cadences name boost stage {boost.Stage} outside the authored stages or twice.");
            cadences[boost.Stage] = SpeedBoostCadence.Authored(timing, boost);
            named[boost.Stage] = true;
        }
        int unnamed = Array.IndexOf(named, false);
        if (unnamed >= 0)
            throw new InvalidDataException($"The running cadences name no boost stage {unnamed}.");
        return cadences;
    }
}
