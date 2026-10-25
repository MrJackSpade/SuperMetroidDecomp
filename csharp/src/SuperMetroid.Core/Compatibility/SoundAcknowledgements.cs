namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What each sound library reports back as acknowledged. The game's sound handlers compare it with the request they are
/// waiting on, so it decides when a handler moves on to its next request (<see cref="SoundAcceptanceClock{TSound}"/>).
/// </summary>
/// <typeparam name="TSound">The sound a library is asked for.</typeparam>
[CompatibilityService("the request each sound library reports back as acknowledged")]
public readonly struct SoundAcknowledgements<TSound> : IEquatable<SoundAcknowledgements<TSound>>
    where TSound : struct, IEquatable<TSound>
{
    /// <summary>The sound libraries that report acknowledgements: libraries one, two and three.</summary>
    public const int LibraryCount = 3;

    /// <summary>
    /// Each library's acknowledgement, indexed by its zero-based library (library one at zero); null for the default
    /// value, in which no library acknowledges anything.
    /// </summary>
    private readonly SoundAcknowledgement<TSound>[]? byLibrary;

    /// <summary>The acknowledgements of libraries one, two and three, in that order.</summary>
    /// <param name="byLibrary">One acknowledgement per library, library one first.</param>
    /// <exception cref="ArgumentException"><paramref name="byLibrary"/> does not hold exactly one per library.</exception>
    public SoundAcknowledgements(params ReadOnlySpan<SoundAcknowledgement<TSound>> byLibrary)
    {
        this.byLibrary = Ensure.LengthEqual(byLibrary, LibraryCount).ToArray();
    }

    /// <summary>Each library acknowledging the given request, or reporting nothing for none; library one first.</summary>
    /// <param name="byLibrary">One request per library, library one first.</param>
    /// <exception cref="ArgumentException"><paramref name="byLibrary"/> does not hold exactly one per library.</exception>
    public static SoundAcknowledgements<TSound> Of(params ReadOnlySpan<TSound?> byLibrary)
    {
        _ = Ensure.LengthEqual(byLibrary, LibraryCount);
        var acknowledgements = new SoundAcknowledgement<TSound>[LibraryCount];
        for (int library = 0; library < LibraryCount; library++)
            acknowledgements[library] = SoundAcknowledgement<TSound>.Of(byLibrary[library]);
        return new SoundAcknowledgements<TSound>(acknowledgements);
    }

    /// <summary>The acknowledgement of the library at zero-based <paramref name="library"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="library"/> is outside 0 through 2.</exception>
    public SoundAcknowledgement<TSound> this[int library] => (uint)library < LibraryCount
        ? byLibrary is null ? default : byLibrary[library]
        : throw new ArgumentOutOfRangeException(nameof(library), library, "Sound libraries are indexed 0 through 2.");

    /// <inheritdoc />
    public bool Equals(SoundAcknowledgements<TSound> other)
    {
        for (int library = 0; library < LibraryCount; library++)
            if (!this[library].Equals(other[library]))
                return false;
        return true;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is SoundAcknowledgements<TSound> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(this[0], this[1], this[2]);

    /// <inheritdoc />
    public override string ToString() => $"[{this[0]}, {this[1]}, {this[2]}]";
}
