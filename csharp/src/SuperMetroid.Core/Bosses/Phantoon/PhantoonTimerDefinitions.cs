using static SuperMetroid.Core.Bosses.Phantoon.PhantoonTimerLength;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's phase durations: his first flame's delay, and each eight-way random phase timer's short, medium and long
/// durations with the length each of its eight picks takes (#2262).
/// </summary>
public static class PhantoonTimerDefinitions
{
    /// <summary>The updates before his first flame.</summary>
    public const int InitialFlameDelayFrames = 120;

    /// <summary>
    /// The open-eye damage opportunity, picked from the random number. Its waiting phase has no movement-derived duration;
    /// changing a pick changes the attack opportunity.
    /// </summary>
    /// <remarks>The eight picks have no order in time or space: the list itself is how the random number chooses.</remarks>
    public static PhantoonTimerSchedule VulnerableWindow { get; } = new(
        new PhantoonTimerDurations(Short: 15, Medium: 30, Long: 60),
        [Long, Medium, Short, Medium, Long, Medium, Short, Long]);

    /// <summary>
    /// The closed-eye moving phase, picked from the random number, or for the first round by
    /// <see cref="PhantoonCadenceDefinitions.FirstEyeClosedChoice"/>. Expiry opens the eye at its current position,
    /// independently of path completion.
    /// </summary>
    public static PhantoonTimerSchedule EyeClosed { get; } = new(
        new PhantoonTimerDurations(Short: 60, Medium: 360, Long: 720),
        [Long, Short, Medium, Long, Medium, Short, Medium, Long]);

    /// <summary>
    /// The hidden wait after the flame rain's fade-out completes, picked from the random number; the next location is
    /// chosen only on expiry, so neither fading nor travel determines this wait.
    /// </summary>
    public static PhantoonTimerSchedule RainHiding { get; } = new(
        new PhantoonTimerDurations(Short: 30, Medium: 60, Long: 120),
        [Medium, Long, Short, Medium, Short, Medium, Short, Short]);
}
