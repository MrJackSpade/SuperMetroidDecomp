using System.Runtime.CompilerServices;

namespace SuperMetroid.Core.Compatibility;

/// <summary>One clock step's request check of each sound library, indexed by zero-based library index.</summary>
[CompatibilityService("one clock step's request check of each sound library")]
[InlineArray(SoundAcceptanceClock.LibraryCount)]
public struct SoundRequestChecks : IEquatable<SoundRequestChecks>
{
    /// <summary>Library 1's check; the other libraries' follow it in queue order.</summary>
    private SoundRequestCheck first;

    /// <summary>Two check sets are equal when every element matches; inline arrays have no built-in value equality.</summary>
    public readonly bool Equals(SoundRequestChecks other)
    {
        for (int index = 0; index < SoundAcceptanceClock.LibraryCount; index++)
            if (!EqualityComparer<SoundRequestCheck>.Default.Equals(this[index], other[index])) return false;
        return true;
    }

    /// <summary>Compares with another boxed <see cref="SoundRequestChecks"/> element by element.</summary>
    public override readonly bool Equals(object? obj) => obj is SoundRequestChecks other && Equals(other);

    /// <summary>Combines every element's hash in index order.</summary>
    public override readonly int GetHashCode()
    {
        var hash = new HashCode();
        for (int index = 0; index < SoundAcceptanceClock.LibraryCount; index++) hash.Add(this[index]);
        return hash.ToHashCode();
    }
}
