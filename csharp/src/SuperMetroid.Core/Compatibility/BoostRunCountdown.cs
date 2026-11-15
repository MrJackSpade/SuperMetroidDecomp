namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// A Speed Booster boost after one completed run (<see cref="BoostRestartAfterEcho.CountDownOneRun"/>): its stage, and
/// the runs left before the next, which a run counted off an empty countdown leaves a stage lower with the borrow's runs
/// ahead.
/// </summary>
/// <remarks>
/// Evidence: the original keeps the stage and the countdown as the high and low bytes of one word and decrements the
/// whole word, so an empty countdown borrows from the stage (<see cref="BoostRestartAfterEcho"/>).
/// </remarks>
/// <param name="Stage">The boost stage.</param>
/// <param name="Countdown">The runs left before the next stage.</param>
[CompatibilityService("a Speed Booster boost after one completed run, a stage lower with the borrow's runs ahead when its countdown was empty")]
public readonly record struct BoostRunCountdown(int Stage, int Countdown);
