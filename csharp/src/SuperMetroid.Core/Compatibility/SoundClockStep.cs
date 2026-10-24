namespace SuperMetroid.Core.Compatibility;

/// <summary>One step of the <see cref="SoundAcceptanceClock{TSound}"/> within an update, with the request checks it ran.</summary>
/// <typeparam name="TSound">The sound a library is asked for.</typeparam>
/// <param name="SampleOffset">Sound samples from the update's start at which the step runs.</param>
/// <param name="Steps">Elapsed step periods the step accounts for: at most one pending period plus the whole periods since it.</param>
/// <param name="ChecksRequests">Whether the check timer overflowed on this step and checked all three libraries' requests.</param>
/// <param name="Checks">Each library's check when <paramref name="ChecksRequests"/> is set, indexed by zero-based library index.</param>
[CompatibilityService("one step of the sound acceptance clock, with the request checks it ran")]
[IndependentFlag("whether this clock step checked the libraries' requests, a fact of the step's timing")]
public readonly record struct SoundClockStep<TSound>(
    int SampleOffset,
    int Steps,
    bool ChecksRequests,
    SoundRequestChecks<TSound> Checks)
    where TSound : struct, IEquatable<TSound>;
