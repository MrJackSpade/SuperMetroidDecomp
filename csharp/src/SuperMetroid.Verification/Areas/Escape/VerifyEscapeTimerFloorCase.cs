using SuperMetroid.Core.Escape;

namespace SuperMetroid.Verification.Areas.Escape;

/// <summary>The phase and activity of one escape timer in the floor check.</summary>
/// <param name="Phase">The timer's phase.</param>
/// <param name="IsActive">Whether the timer is running.</param>
internal readonly record struct VerifyEscapeTimerFloorCase(EscapeTimerPhase Phase, bool IsActive);
