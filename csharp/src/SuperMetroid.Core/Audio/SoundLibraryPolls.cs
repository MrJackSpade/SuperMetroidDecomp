using System.Runtime.CompilerServices;

namespace SuperMetroid.Core.Audio;

/// <summary>One driver tick's poll of each sound-effect library, indexed by host queue index (<see cref="SoundEffectLibraries.InQueueOrder"/>).</summary>
[InlineArray(SoundEffectLibraries.Count)]
public struct SoundLibraryPolls : IEquatable<SoundLibraryPolls>
{
    /// <summary>Library 1's poll; the other libraries' follow it in queue order.</summary>
    private SpcSoundEffectPoll first;

    /// <summary>Two poll sets are equal when every element matches; inline arrays have no built-in value equality.</summary>
    public readonly bool Equals(SoundLibraryPolls other)
    {
        for (int index = 0; index < SoundEffectLibraries.Count; index++)
            if (!EqualityComparer<SpcSoundEffectPoll>.Default.Equals(this[index], other[index])) return false;
        return true;
    }

    /// <summary>Compares with another boxed <see cref="SoundLibraryPolls"/> element by element.</summary>
    public override readonly bool Equals(object? obj) => obj is SoundLibraryPolls other && Equals(other);

    /// <summary>Combines every element's hash in index order.</summary>
    public override readonly int GetHashCode()
    {
        var hash = new HashCode();
        for (int index = 0; index < SoundEffectLibraries.Count; index++) hash.Add(this[index]);
        return hash.ToHashCode();
    }
}
