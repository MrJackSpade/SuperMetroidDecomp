namespace SuperMetroid.Core.Compatibility;

/// <summary>A Speed Booster boost after one completed run (<see cref="BoostRestartAfterEcho.CountDownOneRun"/>).</summary>
/// <param name="Stage">The boost stage.</param>
/// <param name="Countdown">The runs left before the next stage.</param>
public readonly record struct BoostRunCountdown(int Stage, int Countdown);
