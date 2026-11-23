namespace Engine.Shared;

/// <summary>
/// The sequence of numbers behind <see cref="GameRandom"/>. The engine has a plain one of its own; a game whose
/// outcomes must follow a particular sequence supplies it here.
/// </summary>
public interface IRandomSequence
{
    /// <summary>The next number of the sequence, any 32-bit value.</summary>
    uint Next();

    /// <summary>The number the sequence last gave, without moving on.</summary>
    uint Current { get; }

    /// <summary>Starts the sequence again from <paramref name="seed"/>.</summary>
    void Restart(uint seed);
}
