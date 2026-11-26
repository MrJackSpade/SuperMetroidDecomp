using Engine.Shared;

namespace Engine.Tests.Core;

/// <summary>The stand-in game's random sequence: a small generator that gives the same numbers for the same seed.</summary>
public sealed class PlainRandomSequence : IRandomSequence
{
    /// <summary>The generator's state, never zero.</summary>
    private uint _state;

    /// <summary>A sequence started from <paramref name="seed"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The seed is zero, from which this generator cannot move.</exception>
    public PlainRandomSequence(uint seed)
    {
        ArgumentOutOfRangeException.ThrowIfZero(seed);
        _state = seed;
    }

    /// <inheritdoc/>
    public uint Current => _state;

    /// <inheritdoc/>
    public void Restart(uint seed)
    {
        ArgumentOutOfRangeException.ThrowIfZero(seed);
        _state = seed;
    }

    /// <inheritdoc/>
    public uint Next()
    {
        _state ^= _state << 13;
        _state ^= _state >> 17;
        _state ^= _state << 5;
        return _state;
    }
}
