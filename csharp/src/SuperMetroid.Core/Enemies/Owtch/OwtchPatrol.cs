namespace SuperMetroid.Core.Enemies.Owtch;

/// <summary>How far an Owtch patrols and how long it waits underground.</summary>
internal static class OwtchPatrol
{
    /// <summary>The patrol distance patrol selector <paramref name="index"/> selects.</summary>
    /// <exception cref="InvalidDataException"><paramref name="index"/> is eight or more.</exception>
    internal static ushort TravelDistance(int index)
    {
        if (index >= OwtchMovementDefinitions.TravelDistances.Length)
            throw new InvalidDataException(
                $"Owtch travel-distance index {index} is outside the eight authored values.");
        return OwtchMovementDefinitions.TravelDistances[index];
    }

    /// <summary>The underground wait burial selector <paramref name="index"/> selects.</summary>
    /// <exception cref="InvalidDataException"><paramref name="index"/> is six or more.</exception>
    internal static ushort UndergroundTimer(int index)
    {
        if (index >= OwtchMovementDefinitions.UndergroundTimers.Length)
            throw new InvalidDataException(
                $"Owtch underground-timer index {index} is outside the six authored values.");
        return OwtchMovementDefinitions.UndergroundTimers[index];
    }
}
