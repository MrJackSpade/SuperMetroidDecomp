using System.Collections.Immutable;

namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// One clock step's request check of each sound library, keyed by the library. A library left unlisted, and every library
/// of the default value, reads as a check that acted on no request and saw no change.
/// </summary>
/// <typeparam name="TLibrary">A sound library.</typeparam>
/// <typeparam name="TSound">The sound a library is asked for.</typeparam>
[CompatibilityService("one clock step's request check of each sound library")]
public readonly struct SoundRequestChecks<TLibrary, TSound> : IEquatable<SoundRequestChecks<TLibrary, TSound>>
    where TLibrary : struct, Enum
    where TSound : struct, IEquatable<TSound>
{
    /// <summary>Each listed library's check.</summary>
    private readonly PerSoundLibrary<TLibrary, SoundRequestCheck<TSound>> byLibrary;

    /// <summary>The check of each of <paramref name="libraries"/>.</summary>
    /// <param name="libraries">The libraries, distinct.</param>
    /// <param name="checkOf">Each library's check, run once per library in the order of <paramref name="libraries"/>.</param>
    /// <exception cref="ArgumentException"><paramref name="libraries"/> is default or holds a library twice.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="checkOf"/> is null.</exception>
    public SoundRequestChecks(ImmutableArray<TLibrary> libraries, Func<TLibrary, SoundRequestCheck<TSound>> checkOf) =>
        byLibrary = new PerSoundLibrary<TLibrary, SoundRequestCheck<TSound>>(libraries, checkOf);

    /// <summary>The check of <paramref name="library"/>; no request and no change for a library left unlisted.</summary>
    public SoundRequestCheck<TSound> this[TLibrary library] => byLibrary[library];

    /// <summary>Two check sets are equal when every library's check is the same in both.</summary>
    public bool Equals(SoundRequestChecks<TLibrary, TSound> other) => byLibrary.Equals(other.byLibrary);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is SoundRequestChecks<TLibrary, TSound> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => byLibrary.GetHashCode();

    /// <inheritdoc />
    public override string ToString() => byLibrary.ToString();
}
