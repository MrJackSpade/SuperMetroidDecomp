namespace SuperMetroid.Core.Enemies;

/// <summary>The cartridge random-number word as enemy AI uses it (#633).</summary>
public interface IEnemyRandomNumbers
{
    /// <summary>Advances the RNG (<c>GenerateRandomNumber</c>) and returns the new word.</summary>
    ushort Generate();

    /// <summary>Samples the current random word without advancing it.</summary>
    ushort Read();

    /// <summary>Overwrites the random word, as some initializers seed it.</summary>
    void Seed(ushort value);
}
