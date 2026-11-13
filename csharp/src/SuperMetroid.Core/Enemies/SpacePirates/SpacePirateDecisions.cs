namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>The random fields the ninja and wall Space Pirates read to choose their moves.</summary>
internal static class SpacePirateDecisions
{
    /// <summary>
    /// The random field a ninja Space Pirate draws until it is nonzero before its divekick: the sample modulo 4. Each of
    /// the three accepted values selects the same divekick.
    /// </summary>
    internal static readonly RandomSampleField DivekickChoice = new(0, 2);

    /// <summary>The random choice that sends a wall Space Pirate climbing up rather than down: taken by an odd sample.</summary>
    internal static readonly RandomSampleField ClimbUp = new(0, 1);
}
