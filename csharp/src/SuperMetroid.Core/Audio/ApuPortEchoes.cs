using System.Runtime.CompilerServices;

namespace SuperMetroid.Core.Audio;

/// <summary>One echo for each of the four APU ports, indexed in register order (<see cref="ApuPorts.InRegisterOrder"/>).</summary>
[InlineArray(4)]
public struct ApuPortEchoes : IEquatable<ApuPortEchoes>
{
    /// <summary>The music port's echo; the other ports' follow it in register order.</summary>
    private int first;

    /// <summary>Two echo sets are equal when every element matches; inline arrays have no built-in value equality.</summary>
    public readonly bool Equals(ApuPortEchoes other)
    {
        for (int index = 0; index < 4; index++)
            if (!EqualityComparer<int>.Default.Equals(this[index], other[index])) return false;
        return true;
    }

    /// <summary>Compares with another boxed <see cref="ApuPortEchoes"/> element by element.</summary>
    public override readonly bool Equals(object? obj) => obj is ApuPortEchoes other && Equals(other);

    /// <summary>Combines every element's hash in index order.</summary>
    public override readonly int GetHashCode()
    {
        var hash = new HashCode();
        for (int index = 0; index < 4; index++) hash.Add(this[index]);
        return hash.ToHashCode();
    }
}
