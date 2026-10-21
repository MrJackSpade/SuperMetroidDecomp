using SuperMetroid.Core.Escape;

namespace SuperMetroid.Verification.Areas.Escape;

/// <summary>The state and is active of one verify escape timer floor case.</summary>
/// <param name="State">The state.</param>
/// <param name="IsActive">The is active.</param>
internal readonly record struct VerifyEscapeTimerFloorCase(EscapeTimerState State, bool IsActive);
