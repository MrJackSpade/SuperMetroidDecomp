namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What each sound library reports back as acknowledged. The game's sound handlers compare it with the request they are
/// waiting on, so it decides when a handler moves on to its next request (<see cref="SoundAcceptanceClock{TSound}"/>).
/// </summary>
/// <typeparam name="TSound">The sound a library is asked for.</typeparam>
/// <param name="Library1">Library one's acknowledgement.</param>
/// <param name="Library2">Library two's acknowledgement.</param>
/// <param name="Library3">Library three's acknowledgement.</param>
[CompatibilityService("the request each sound library reports back as acknowledged")]
public readonly record struct SoundAcknowledgements<TSound>(
    SoundAcknowledgement<TSound> Library1,
    SoundAcknowledgement<TSound> Library2,
    SoundAcknowledgement<TSound> Library3)
    where TSound : struct, IEquatable<TSound>
{
    /// <summary>Each library acknowledging the given request, or reporting nothing for none.</summary>
    public static SoundAcknowledgements<TSound> Of(TSound? library1, TSound? library2, TSound? library3) => new(
        SoundAcknowledgement<TSound>.Of(library1), SoundAcknowledgement<TSound>.Of(library2), SoundAcknowledgement<TSound>.Of(library3));

    /// <summary>The acknowledgement of the library at zero-based <paramref name="library"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="library"/> is outside 0 through 2.</exception>
    public SoundAcknowledgement<TSound> this[int library] => library switch
    {
        0 => Library1,
        1 => Library2,
        2 => Library3,
        _ => throw new ArgumentOutOfRangeException(nameof(library), library, "Sound libraries are indexed 0 through 2."),
    };
}
