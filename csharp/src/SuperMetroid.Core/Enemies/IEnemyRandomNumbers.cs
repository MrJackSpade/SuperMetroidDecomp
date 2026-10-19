namespace SuperMetroid.Core.Enemies;

/// <summary>The cartridge random-number word as enemy AI uses it (#633).</summary>
public interface IEnemyRandomNumbers
{
    /// <summary>Advances the RNG (<c>GenerateRandomNumber</c>) and returns the new state as a sample, 0 through 65535.</summary>
    int Generate();

    /// <summary>Samples the current random state, 0 through 65535, without advancing it.</summary>
    int Read();

    /// <summary>Overwrites the random state, as some initializers seed it.</summary>
    void Seed(int value);
}
