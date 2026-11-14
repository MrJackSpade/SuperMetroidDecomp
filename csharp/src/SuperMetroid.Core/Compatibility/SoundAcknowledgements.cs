using System.Collections.Immutable;

namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What each sound library reports back as acknowledged. The game's sound queues compare it with the request they are
/// waiting on, so it decides when a queue moves on to its next request
/// (<see cref="SoundAcceptanceClock{TLibrary, TSound}"/>). A library left unlisted, and every library of the default
/// value, acknowledges nothing.
/// </summary>
/// <typeparam name="TLibrary">A sound library.</typeparam>
/// <typeparam name="TSound">The sound a library is asked for.</typeparam>
/// <remarks>
/// Reported by the sound acceptance clock after each update and read by the next update's sound queues. Evidence: each
/// of the original game's sound queues compares its own library's reply with the request it waits on and moves on when
/// they match; the sound acceptance clock check compares every library's reply on every update with a recording of the
/// emulated sound program's, and the SMV replays trace each handler's state beside its queue.
/// </remarks>
[CompatibilityService("the request each sound library reports back as acknowledged")]
public readonly struct SoundAcknowledgements<TLibrary, TSound> : IEquatable<SoundAcknowledgements<TLibrary, TSound>>
    where TLibrary : struct, Enum
    where TSound : struct, IEquatable<TSound>
{
    /// <summary>Each listed library's acknowledgement.</summary>
    private readonly PerSoundLibrary<TLibrary, SoundAcknowledgement<TSound>> byLibrary;

    /// <summary>The acknowledgement of each of <paramref name="libraries"/>.</summary>
    /// <param name="libraries">The libraries, distinct.</param>
    /// <param name="acknowledgementOf">Each library's acknowledgement, asked once per library in the order of <paramref name="libraries"/>.</param>
    /// <exception cref="ArgumentException"><paramref name="libraries"/> is default or holds a library twice.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="acknowledgementOf"/> is null.</exception>
    public SoundAcknowledgements(ImmutableArray<TLibrary> libraries, Func<TLibrary, SoundAcknowledgement<TSound>> acknowledgementOf) =>
        byLibrary = new PerSoundLibrary<TLibrary, SoundAcknowledgement<TSound>>(libraries, acknowledgementOf);

    /// <summary>The acknowledgement of <paramref name="library"/>; nothing for a library left unlisted.</summary>
    public SoundAcknowledgement<TSound> this[TLibrary library] => byLibrary[library];

    /// <summary>Two acknowledgements are equal when every library acknowledges the same in both.</summary>
    public bool Equals(SoundAcknowledgements<TLibrary, TSound> other) => byLibrary.Equals(other.byLibrary);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is SoundAcknowledgements<TLibrary, TSound> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => byLibrary.GetHashCode();

    /// <inheritdoc />
    public override string ToString() => byLibrary.ToString();
}
