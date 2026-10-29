namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>One Ceres Ridley eye-fade shade selection or the terminal handoff.</summary>
/// <param name="IsComplete">Whether the schedule has ended and the body fade begins.</param>
/// <param name="Shade">The eye shade shown this step, zero (full eye paint) through fifteen (black); zero at completion.</param>
[IndependentFlag("whether the eye fade finished at this step")]
internal readonly record struct CeresRidleyEyeFadeStep(bool IsComplete, int Shade);
