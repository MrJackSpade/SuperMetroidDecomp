namespace SuperMetroid.Core.Samus.Movement;

/// <summary>NTSC running cadence and boost-stage timing: gameplay rules, not editable art.</summary>
/// <remarks>
/// Independently reviewed for #1165 against the complete NTSC J/U v1.0 catalog. Each cadence times the ten running
/// frames; the running pose's own animation closes the loop after the tenth. Ordinary frames take two ticks, and the
/// boost stages take three; two and three alternating; two; one and two alternating; and one. A stage advance restarts
/// the boost's cadence countdown at one for stages zero through three and at two for stage four. The cadence a full
/// sound queue selects is not authored; <see cref="SuperMetroid.Core.Compatibility.UnauthoredBoostCadence"/> reproduces
/// it (#1473). This is NTSC policy, not a PAL interpolation rule.
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

    /// <summary>
    /// The largest selection the Speed Booster echo's queue request reports: an admitted request reports the
    /// occupancy it saw, zero through five, and five selects the unauthored cadence.
    /// </summary>
    internal const int MaximumSoundQueueSelection = 5;

    /// <summary>
    /// The selection a rejected echo leaves: the queue request reports the rejected sound itself where an admitted
    /// request reports the occupancy, and the Speed Booster echo is library three's third sound.
    /// </summary>
    internal const int RejectedEchoSelection = 3;
}
