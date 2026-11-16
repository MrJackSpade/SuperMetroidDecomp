namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>The short, medium and long durations of one of Phantoon's random phase timers, in updates (#2262).</summary>
/// <param name="Short">The short duration.</param>
/// <param name="Medium">The medium duration.</param>
/// <param name="Long">The long duration.</param>
public readonly record struct PhantoonTimerDurations(int Short, int Medium, int Long);
