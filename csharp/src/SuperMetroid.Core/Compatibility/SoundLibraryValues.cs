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
/// <remarks>Built once per acknowledgement or request check by the <see cref="SoundAcceptanceClock{TLibrary, TSound}"/>; its evidence is the clock's.</remarks>
[CompatibilityService("one value for each sound library, keyed by the library")]
public readonly struct SoundLibraryValues<TLibrary, TValue> : IEquatable<SoundLibraryValues<TLibrary, TValue>>
    where TLibrary : struct, Enum
    where TValue : struct, IEquatable<TValue>
{
    /// <summary>The libraries the set holds, distinct; default for the default value, which holds none.</summary>
    private readonly ImmutableArray<TLibrary> libraries;

    /// <summary>Each held library's value, in the order of <see cref="libraries"/>; null for the default value.</summary>
    private readonly TValue[]? values;

    /// <summary>Holds <paramref name="valueOf"/> of each of <paramref name="libraries"/>.</summary>
    /// <param name="libraries">The libraries, distinct.</param>
    /// <param name="valueOf">Each library's value, asked once per library in the order of <paramref name="libraries"/>.</param>
    /// <exception cref="ArgumentException"><paramref name="libraries"/> is default or holds a library twice.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="valueOf"/> is null.</exception>
    public SoundLibraryValues(ImmutableArray<TLibrary> libraries, Func<TLibrary, TValue> valueOf)
    {
        this.libraries = RequireDistinct(libraries);
        Ensure.NotNull(valueOf);
        values = new TValue[libraries.Length];
        for (int position = 0; position < libraries.Length; position++)
            values[position] = valueOf(libraries[position]);
    }

    /// <summary>Holds <paramref name="values"/>, already in the order of <paramref name="libraries"/>, which are distinct.</summary>
    private SoundLibraryValues(ImmutableArray<TLibrary> libraries, TValue[] values)
    {
        this.libraries = libraries;
        this.values = values;
    }

    /// <summary>The value of <paramref name="library"/>; the default value for a library the set does not hold.</summary>
    public TValue this[TLibrary library]
    {
        get
        {
            int position = PositionOf(library);
            return position < 0 ? default : values![position];
        }
    }

    /// <summary>This set with <paramref name="library"/>'s value replaced by <paramref name="value"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The set does not hold <paramref name="library"/>.</exception>
    public SoundLibraryValues<TLibrary, TValue> With(TLibrary library, TValue value)
    {
        int position = PositionOf(library);
        if (position < 0)
            throw new ArgumentOutOfRangeException(nameof(library), library, "The set holds no value for this library.");
        TValue[] replaced = [.. values!];
        replaced[position] = value;
        return new SoundLibraryValues<TLibrary, TValue>(libraries, replaced);
    }

    /// <summary>Two sets are equal when every library either holds reads the same in both.</summary>
    public bool Equals(SoundLibraryValues<TLibrary, TValue> other) => ReadsSameIn(other) && other.ReadsSameIn(this);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is SoundLibraryValues<TLibrary, TValue> other && Equals(other);

    /// <summary>Combines each library's hash with its value's, for every library whose value is not the default.</summary>
    public override int GetHashCode()
    {
        int hash = 0;
        if (libraries.IsDefault)
            return hash;
        for (int position = 0; position < libraries.Length; position++)
        {
            // Unordered, and blind to default values, as the equality is.
            if (!values![position].Equals(default))
                hash ^= HashCode.Combine(libraries[position], values[position]);
        }
        return hash;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (libraries.IsDefault)
            return "[]";
        TValue[] held = values!;
        return $"[{string.Join(", ", libraries.Select((library, position) => $"{library}: {held[position]}"))}]";
    }

    /// <summary>Where the set holds <paramref name="library"/>'s value, or a negative position when it holds none.</summary>
    private int PositionOf(TLibrary library) => libraries.IsDefault ? -1 : libraries.IndexOf(library);

    /// <summary>Whether every library this set holds reads the same in <paramref name="other"/>.</summary>
    private bool ReadsSameIn(SoundLibraryValues<TLibrary, TValue> other)
    {
        if (libraries.IsDefault)
            return true;
        for (int position = 0; position < libraries.Length; position++)
        {
            if (!values![position].Equals(other[libraries[position]]))
                return false;
        }
        return true;
    }

    /// <summary>Returns <paramref name="libraries"/> when it holds each library once.</summary>
    /// <exception cref="ArgumentException"><paramref name="libraries"/> is default or holds a library twice.</exception>
    private static ImmutableArray<TLibrary> RequireDistinct(ImmutableArray<TLibrary> libraries)
    {
        if (libraries.IsDefault)
            throw new ArgumentException("The sound libraries are required.", nameof(libraries));
        if (libraries.Distinct().Count() != libraries.Length)
            throw new ArgumentException("Each sound library is listed once.", nameof(libraries));
        return libraries;
    }
}
