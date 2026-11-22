using static SuperMetroid.Core.DefinitionRun;
namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Running cadence and boost-stage timing: gameplay rules, not editable art.</summary>
/// <remarks>
/// Each cadence times the running frames; the running pose's own animation closes the loop after the last. Ordinary
/// frames take the same ticks each, and the boost stages quicken from slower than ordinary running to quicker. A stage
/// advance restarts the boost's countdown at its stage's restart value. Which cadence the boost
/// restarts on after its echo is <see cref="SuperMetroid.Core.Compatibility.BoostRestartAfterEcho"/>'s; the cadence a
/// full sound queue selects is not authored, and <see cref="SuperMetroid.Core.Compatibility.UnauthoredBoostCadence"/>
/// reproduces it (#1473).
/// </remarks>
internal static partial class SamusRunningCadenceDefinitions
{
    /// <summary>Each authored cadence's timing: ordinary running's and each Speed Booster stage's (#2324).</summary>
    internal static IReadOnlyDictionary<SamusRunningCadence, SamusRunningCadenceTiming> Timings { get; } =
        new Dictionary<SamusRunningCadence, SamusRunningCadenceTiming>
        {
            [SamusRunningCadence.Ordinary] = new([.. Repeated(2, 10)], Boost: null),
            [SamusRunningCadence.BoostStage0] = new([.. Repeated(3, 10)], new(Stage: 0, CountdownAfterAdvance: 1)),
            [SamusRunningCadence.BoostStage1] = new([2, 3, 2, 3, 2, 3, 2, 3, 2, 3], new(Stage: 1, CountdownAfterAdvance: 1)),
            [SamusRunningCadence.BoostStage2] = new([.. Repeated(2, 10)], new(Stage: 2, CountdownAfterAdvance: 1)),
            [SamusRunningCadence.BoostStage3] = new([1, 2, 1, 2, 1, 2, 1, 2, 1, 2], new(Stage: 3, CountdownAfterAdvance: 1)),
            [SamusRunningCadence.BoostStage4] = new([.. Repeated(1, 10)], new(Stage: 4, CountdownAfterAdvance: 2)),
        };
}
