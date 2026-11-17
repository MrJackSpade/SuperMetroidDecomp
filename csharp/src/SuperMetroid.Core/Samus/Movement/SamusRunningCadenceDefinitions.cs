namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Running cadence and boost-stage timing: gameplay rules, not editable art.</summary>
/// <remarks>
/// Each cadence times the ten running
/// frames; the running pose's own animation closes the loop after the tenth. Ordinary frames take two ticks, and the
/// boost stages take three; two and three alternating; two; one and two alternating; and one. A stage advance restarts
/// the boost's countdown at one for stages zero through three and at two for stage four. Which cadence the boost
/// restarts on after its echo is <see cref="SuperMetroid.Core.Compatibility.BoostRestartAfterEcho"/>'s; the cadence a
/// full sound queue selects is not authored, and <see cref="SuperMetroid.Core.Compatibility.UnauthoredBoostCadence"/>
/// reproduces it (#1473). This is the Japanese and North American release's timing, not an interpolation of the European release's.
/// </remarks>
internal static class SamusRunningCadenceDefinitions
{
    /// <summary>Each authored cadence's timing: ordinary running's and each Speed Booster stage's (#2324).</summary>
    internal static IReadOnlyDictionary<SamusRunningCadence, SamusRunningCadenceTiming> Timings { get; } =
        new Dictionary<SamusRunningCadence, SamusRunningCadenceTiming>
        {
            [SamusRunningCadence.Ordinary] = new([2, 2, 2, 2, 2, 2, 2, 2, 2, 2], Boost: null),
            [SamusRunningCadence.BoostStage0] = new([3, 3, 3, 3, 3, 3, 3, 3, 3, 3], new(Stage: 0, CountdownAfterAdvance: 1)),
            [SamusRunningCadence.BoostStage1] = new([2, 3, 2, 3, 2, 3, 2, 3, 2, 3], new(Stage: 1, CountdownAfterAdvance: 1)),
            [SamusRunningCadence.BoostStage2] = new([2, 2, 2, 2, 2, 2, 2, 2, 2, 2], new(Stage: 2, CountdownAfterAdvance: 1)),
            [SamusRunningCadence.BoostStage3] = new([1, 2, 1, 2, 1, 2, 1, 2, 1, 2], new(Stage: 3, CountdownAfterAdvance: 1)),
            [SamusRunningCadence.BoostStage4] = new([1, 1, 1, 1, 1, 1, 1, 1, 1, 1], new(Stage: 4, CountdownAfterAdvance: 2)),
        };
}
