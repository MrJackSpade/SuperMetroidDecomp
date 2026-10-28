namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>How far a Cacatac patrols from where it starts.</summary>
internal static class CacatacPatrol
{
    /// <summary>The patrol's pixel distance for travel selector <paramref name="index"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="index"/> is six or more.</exception>
    internal static ushort TravelDistance(int index)
    {
        if (index >= CacatacMovementDefinitions.TravelDistances.Length)
            throw new InvalidDataException(
                $"Cacatac travel-distance index {index} is outside the 6-entry native table.");
        return CacatacMovementDefinitions.TravelDistances[index];
    }
}
