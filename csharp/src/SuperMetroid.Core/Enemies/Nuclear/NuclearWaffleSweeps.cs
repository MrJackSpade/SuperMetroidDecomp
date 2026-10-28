namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>Chooses a Nuclear Waffle's sweep.</summary>
internal static class NuclearWaffleSweeps
{
    /// <summary>One of the two mirrored sweeps, including endpoints, link spacing, and turn thresholds.</summary>
    /// <exception cref="InvalidDataException"><paramref name="direction"/> is two or more.</exception>
    internal static NuclearWaffleSweepDefinition Sweep(int direction)
    {
        if (direction >= NuclearWaffleDefinitions.Sweeps.Length)
            throw new InvalidDataException($"Nuclear Waffle direction {direction} is outside the two native sweeps.");
        return NuclearWaffleDefinitions.Sweeps[direction];
    }
}
