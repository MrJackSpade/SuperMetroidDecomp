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
    /// <summary>The delay, in ticks, of each frame of ordinary running.</summary>
    internal static IReadOnlyList<int> OrdinaryDelays { get; } = [2, 2, 2, 2, 2, 2, 2, 2, 2, 2];

    /// <summary>
    /// The delay, in ticks, of each frame of each authored boost stage; the index is the boost stage, zero through four.
    /// </summary>
    internal static IReadOnlyList<IReadOnlyList<int>> BoostStageDelays { get; } =
    [
        [3, 3, 3, 3, 3, 3, 3, 3, 3, 3],
        [2, 3, 2, 3, 2, 3, 2, 3, 2, 3],
        [2, 2, 2, 2, 2, 2, 2, 2, 2, 2],
        [1, 2, 1, 2, 1, 2, 1, 2, 1, 2],
        [1, 1, 1, 1, 1, 1, 1, 1, 1, 1],
    ];

    /// <summary>
    /// The cadence countdown the boost restarts with after advancing onto each authored stage: stages zero through
    /// three restart at one, stage four at two.
    /// </summary>
    internal static IReadOnlyList<int> CountdownAfterAdvance { get; } = [1, 1, 1, 1, 2];

    /// <summary>The cadence of each authored boost stage; the index is the boost stage, zero through four.</summary>
    internal static IReadOnlyList<SamusRunningCadence> BoostStageCadences { get; } =
    [
        SamusRunningCadence.BoostStage0,
        SamusRunningCadence.BoostStage1,
        SamusRunningCadence.BoostStage2,
        SamusRunningCadence.BoostStage3,
        SamusRunningCadence.BoostStage4,
    ];
}
