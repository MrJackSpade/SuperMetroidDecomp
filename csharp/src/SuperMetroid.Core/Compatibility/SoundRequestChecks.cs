using System.Runtime.CompilerServices;

namespace SuperMetroid.Core.Compatibility;

/// <summary>One clock step's request check of each sound library, indexed by zero-based library index.</summary>
/// <typeparam name="TSound">The sound a library is asked for.</typeparam>
[CompatibilityService("one clock step's request check of each sound library")]
[InlineArray(SoundClockTiming.LibraryCount)]
public struct SoundRequestChecks<TSound> : IEquatable<SoundRequestChecks<TSound>>
    where TSound : struct, IEquatable<TSound>
{
    /// <summary>Library 1's check; the other libraries' follow it in queue order.</summary>
    private SoundRequestCheck<TSound> first;

    /// <summary>Two check sets are equal when every element matches; inline arrays have no built-in value equality.</summary>
    public readonly bool Equals(SoundRequestChecks<TSound> other)
    {
        for (int index = 0; index < SoundClockTiming.LibraryCount; index++)
            if (!EqualityComparer<SoundRequestCheck<TSound>>.Default.Equals(this[index], other[index])) return false;
        return true;
    }

    /// <summary>Compares with another boxed <see cref="SoundRequestChecks{TSound}"/> element by element.</summary>
    public override readonly bool Equals(object? obj) => obj is SoundRequestChecks<TSound> other && Equals(other);

    /// <summary>Combines every element's hash in index order.</summary>
    public override readonly int GetHashCode()
    {
        var hash = new HashCode();
        for (int index = 0; index < SoundClockTiming.LibraryCount; index++) hash.Add(this[index]);
        return hash.ToHashCode();
    }
}
