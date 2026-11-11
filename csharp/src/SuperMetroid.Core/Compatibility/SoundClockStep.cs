namespace SuperMetroid.Core.Compatibility;

/// <summary>One step of the <see cref="SoundAcceptanceClock{TLibrary, TSound}"/> within an update, with the request checks it ran.</summary>
/// <typeparam name="TLibrary">A sound library.</typeparam>
/// <typeparam name="TSound">The sound a library is asked for.</typeparam>
/// <param name="SampleOffset">Sound samples from the update's start at which the step runs.</param>
/// <param name="Steps">Elapsed step periods the step accounts for: at most one pending period plus the whole periods since it.</param>
/// <param name="ChecksRequests">Whether the check timer overflowed on this step and checked every library's request.</param>
/// <param name="Checks">Each library's check when <paramref name="ChecksRequests"/> is set, keyed by the library.</param>
/// <remarks>
/// Produced by the sound acceptance clock for each of its steps within an update. Evidence: the original sound program's
/// timer ticks on its own clock of sound samples, so its ticks fall at a sample offset that drifts from update to update,
/// and only the ticks on which its request-check counter overflows read the requests; the sound acceptance clock check
/// matches the acknowledgements and the sound these steps schedule against a recording of the emulated sound program.
/// </remarks>
[CompatibilityService("one step of the sound acceptance clock, with the request checks it ran")]
[IndependentFlag("whether this clock step checked the libraries' requests, a fact of the step's timing")]
public readonly record struct SoundClockStep<TLibrary, TSound>(
    int SampleOffset,
    int Steps,
    bool ChecksRequests,
    SoundRequestChecks<TLibrary, TSound> Checks)
    where TLibrary : struct, Enum
    where TSound : struct, IEquatable<TSound>;
