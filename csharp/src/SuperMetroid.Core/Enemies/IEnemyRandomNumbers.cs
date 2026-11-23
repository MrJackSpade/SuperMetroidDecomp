namespace SuperMetroid.Core.Enemies;

/// <summary>The game's random numbers as enemy AI draws them (#633).</summary>
public interface IEnemyRandomNumbers
{
    /// <summary>Advances the shared random sequence and returns the new state as a sample, 0 through 65535.</summary>
    int Generate();

    /// <summary>Advances the shared random sequence without sampling it, as a draw whose value decides nothing does.</summary>
    void Advance();

    /// <summary>Samples the current random state, 0 through 65535, without advancing it.</summary>
    int Read();

    /// <summary>Overwrites the random state with a species' seed, as its placement or activation does.</summary>
    void Reseed(SpeciesRandomReseed reseed);
}
