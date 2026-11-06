namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>How far a Fireflea moves from its centre.</summary>
internal static class FirefleaOrbit
{
    /// <summary>The movement radius parameter two's high byte selects.</summary>
    /// <exception cref="InvalidDataException"><paramref name="index"/> is eight or more.</exception>
    internal static int Radius(int index)
    {
        if (index >= FirefleaMovementDefinitions.Radii.Length)
            throw new InvalidDataException(
                $"Fireflea radius index {index} is outside the eight authored radii.");
        return FirefleaMovementDefinitions.Radii[index];
    }
}
