namespace Engine.Shared;

/// <summary>
/// The gameplay random generator. A session has one, shared by everything in gameplay that needs chance, and the order
/// of draws is part of how the game plays out. Effects that are only for show never draw from it.
/// </summary>
public sealed class GameRandom
{
    /// <summary>The sequence the draws come from.</summary>
    private readonly IRandomSequence _sequence;

    /// <summary>A generator drawing from <paramref name="sequence"/>.</summary>
    public GameRandom(IRandomSequence sequence)
    {
        ArgumentNullException.ThrowIfNull(sequence);
        _sequence = sequence;
    }

    /// <summary>The next number of the sequence, as it comes.</summary>
    public uint Next() => _sequence.Next();

    /// <summary>A number from 0 up to, not including, <paramref name="count"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The count is not positive.</exception>
    public int NextBelow(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
        return (int)(_sequence.Next() % (uint)count);
    }
}
