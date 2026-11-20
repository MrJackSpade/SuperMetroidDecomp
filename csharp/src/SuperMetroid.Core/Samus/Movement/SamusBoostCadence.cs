namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// A cadence the Speed Booster runs on (#2554): an authored boost stage's or the unauthored one a full sound queue selects,
/// with the countdown the boost restarts with on it. Ordinary running is no boost cadence, so none holds it.
/// </summary>
/// <param name="Cadence">The cadence the run animates by.</param>
/// <param name="CountdownAfterAdvance">The countdown, 0 through 2, the boost restarts with on the cadence.</param>
internal readonly record struct SamusBoostCadence(SamusRunningCadence Cadence, int CountdownAfterAdvance);
