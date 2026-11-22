namespace SuperMetroid.Core.Samus.Movement;

/// <summary>How one authored running cadence times the run (#2324).</summary>
/// <param name="Delays">The delay, in ticks, of each of the ten running frames.</param>
/// <param name="Boost">The Speed Booster stage the cadence is, with its restart countdown; null for ordinary running.</param>
internal sealed partial record SamusRunningCadenceTiming(IReadOnlyList<int> Delays, SamusBoostStageTiming? Boost);
