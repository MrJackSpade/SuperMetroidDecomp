using System.Collections.Immutable;

namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// One value for each of a set of sound libraries, keyed by the library: what each library reports back
/// (<see cref="SoundAcknowledgements{TLibrary, TSound}"/>) or what each library's request check acted on
/// (<see cref="SoundRequestChecks{TLibrary, TSound}"/>). A library the set does not hold, and every library of the default
/// value, reads as the value's default, so two sets are equal when every library reads the same in both.
/// </summary>
/// <typeparam name="TLibrary">A sound library; the set finds a library by equality and never reads its value.</typeparam>
/// <typeparam name="TValue">What each library holds.</typeparam>
/// <remarks>
/// Built once per acknowledgement or request check by the <see cref="SoundAcceptanceClock{TLibrary, TSound}"/>. Evidence:
/// the original game and sound program pass each library's request and reply through that library's own channel, so
/// one library's reply never stands for another's; the sound acceptance clock check compares every library's
/// acknowledgement on every update with a recording of the emulated sound program's.
/// </remarks>
[CompatibilityService("one value for each sound library, keyed by the library")]
public readonly struct PerSoundLibrary<TLibrary, TValue> : IEquatable<PerSoundLibrary<TLibrary, TValue>>
    where TLibrary : struct, Enum
    where TValue : struct, IEquatable<TValue>
{
    /// <summary>The libraries the set holds, distinct; default for the default value, which holds none.</summary>
    private readonly ImmutableArray<TLibrary> libraries;

    /// <summary>Each held library's value, in the order of <see cref="libraries"/>; default exactly when the libraries are.</summary>
    private readonly ImmutableArray<TValue> values;

    /// <summary>Holds <paramref name="valueOf"/> of each of <paramref name="libraries"/>.</summary>
    /// <param name="libraries">The libraries, distinct.</param>
    /// <param name="valueOf">Each library's value, asked once per library in the order of <paramref name="libraries"/>.</param>
    /// <exception cref="ArgumentException"><paramref name="libraries"/> is default or holds a library twice.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="valueOf"/> is null.</exception>
    public PerSoundLibrary(ImmutableArray<TLibrary> libraries, Func<TLibrary, TValue> valueOf)
    {
        this.libraries = RequireDistinct(libraries);
        values = [.. libraries.Select(Ensure.NotNull(valueOf))];
    }

    /// <summary>The value of <paramref name="library"/>; the default value for a library the set does not hold.</summary>
    public TValue this[TLibrary library] => PositionOf(library) is int position and >= 0 ? values[position] : default;

    /// <summary>Two sets are equal when every library either holds reads the same in both.</summary>
    public bool Equals(PerSoundLibrary<TLibrary, TValue> other) => ReadsSameIn(other) && other.ReadsSameIn(this);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is PerSoundLibrary<TLibrary, TValue> other && Equals(other);

    /// <summary>
    /// Combines each library's hash with its value's, for every library whose value is not the default; unordered, and
    /// blind to default values, as the equality is.
    /// </summary>
    public override int GetHashCode() => Held().Where(entry => !entry.Value.Equals(default))
        .Aggregate(0, (hash, entry) => hash ^ HashCode.Combine(entry.Key, entry.Value));

    /// <inheritdoc />
    public override string ToString() => $"[{string.Join(", ", Held().Select(entry => $"{entry.Key}: {entry.Value}"))}]";

    /// <summary>Each library the set holds with its value, in order; none for the default value.</summary>
    private IEnumerable<KeyValuePair<TLibrary, TValue>> Held() => libraries.IsDefault
        ? []
        : libraries.Zip(values, (library, value) => new KeyValuePair<TLibrary, TValue>(library, value));

    /// <summary>Where the set holds <paramref name="library"/>'s value, or a negative position when it holds none.</summary>
    private int PositionOf(TLibrary library) => libraries.IsDefault ? -1 : libraries.IndexOf(library);

    /// <summary>Whether every library this set holds reads the same in <paramref name="other"/>.</summary>
    private bool ReadsSameIn(PerSoundLibrary<TLibrary, TValue> other) =>
        Held().All(entry => entry.Value.Equals(other[entry.Key]));

    /// <summary>Returns <paramref name="libraries"/> when it holds each library once.</summary>
    /// <exception cref="ArgumentException"><paramref name="libraries"/> is default or holds a library twice.</exception>
    private static ImmutableArray<TLibrary> RequireDistinct(ImmutableArray<TLibrary> libraries) =>
        libraries.IsDefault ? throw new ArgumentException("The sound libraries are required.", nameof(libraries))
        : libraries.Distinct().Count() != libraries.Length ? throw new ArgumentException("Each sound library is listed once.", nameof(libraries))
        : libraries;
}
