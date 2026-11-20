namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The gravity of Mother Brain's bomb after each bounce.</summary>
internal static class MotherBrainBombBounces
{
    /// <summary>The bounces a bomb makes before it expires.</summary>
    private static int Count => MotherBrainBombBounceDefinitions.BounceAccelerations.Length;

    /// <summary>Whether a bomb that has bounced <paramref name="bounces"/> times has landed from its last bounce, so it expires.</summary>
    internal static bool HasExpired(int bounces) => bounces == Count + 1;

    /// <summary>The gravity of a bomb that has bounced <paramref name="bounces"/> times, one through <see cref="Count"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="bounces"/> is not a bounce the bomb makes.</exception>
    internal static SignedEightEight YAcceleration(int bounces)
    {
        if (bounces < 1 || bounces > Count)
            throw new InvalidDataException($"Mother Brain bomb bounce {bounces} is not one of its {Count} authored bounces.");
        return MotherBrainBombBounceDefinitions.BounceAccelerations[bounces - 1];
    }
}
