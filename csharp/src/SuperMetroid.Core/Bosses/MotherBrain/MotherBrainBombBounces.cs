namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The gravity of Mother Brain's bomb after each bounce.</summary>
internal static class MotherBrainBombBounces
{
    /// <summary>The bounces a bomb makes before it expires.</summary>
    internal static int Count => MotherBrainBombBounceDefinitions.BounceAccelerations.Length;

    /// <summary>
    /// The gravity of a bomb that has bounced <paramref name="bounces"/> times, one through <see cref="Count"/>; none
    /// once it has landed from its last bounce and expires.
    /// </summary>
    /// <exception cref="InvalidDataException"><paramref name="bounces"/> is not a bounce the bomb makes or expires after.</exception>
    internal static int? YAcceleration(int bounces)
    {
        if (bounces == Count + 1)
            return null;
        if (bounces < 1 || bounces > Count)
            throw new InvalidDataException($"Mother Brain bomb bounce {bounces} is not one of its {Count} authored bounces.");
        return MotherBrainBombBounceDefinitions.BounceAccelerations[bounces - 1];
    }
}
