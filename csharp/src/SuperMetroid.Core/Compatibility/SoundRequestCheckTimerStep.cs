namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The <see cref="SoundRequestCheckTimer"/> after one sound clock step, and whether that step checks the sound
/// requests. Produced on every clock step of the <see cref="SoundAcceptanceClock{TLibrary, TSound}"/>.
/// </summary>
/// <remarks>
/// Evidence: the original sound program adds to its request-check counter on each tick of its timer, keeps what is left
/// past a full count, and reads the requests on the tick that overflows it; the request-check timer check steps the timer
/// across periods and its overflows, and the sound acceptance clock check compares the acceptance it paces with a
/// recording of the emulated sound program.
/// </remarks>
/// <param name="Next">The timer after the step, holding the remainder past a full period.</param>
/// <param name="ChecksRequests">Whether the step reached a full period, which is when the libraries check the requests.</param>
[CompatibilityService("the request-check timer after one clock step, and whether the step checks the sound requests")]
public readonly record struct SoundRequestCheckTimerStep(SoundRequestCheckTimer Next, bool ChecksRequests);
