namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>The random fields the ninja and wall Space Pirates read to choose their moves.</summary>
internal static class SpacePirateDecisions
{
    /// <summary>
    /// The random field a ninja Space Pirate draws until it is nonzero before its divekick: bits zero and one. Each of
    /// the three accepted values selects the same divekick.
    /// </summary>
    internal static readonly RandomSampleField DivekickChoice = new(0, 2);

    /// <summary>The random bit that sends a wall Space Pirate climbing up rather than down: bit zero.</summary>
    internal static readonly RandomSampleField ClimbUp = new(0, 1);
}
