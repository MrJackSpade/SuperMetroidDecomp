namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The <see cref="SoundRequestCheckTimer"/> after one sound clock step, and whether that step checks the sound
/// requests. Produced on every clock step of the <see cref="SoundAcceptanceClock{TSound}"/>; its evidence is the
/// timer's.
/// </summary>
/// <param name="Next">The timer after the step, holding the remainder past a full period.</param>
/// <param name="ChecksRequests">Whether the step reached a full period, which is when the libraries check the requests.</param>
[CompatibilityService("the request-check timer after one clock step, and whether the step checks the sound requests")]
public readonly record struct SoundRequestCheckTimerStep(SoundRequestCheckTimer Next, bool ChecksRequests);
