namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>One link of Ridley's tail as his tail is made (#2238): its lengths and the angle it starts at.</summary>
/// <param name="RestDistance">The link's rest length from the link before it.</param>
/// <param name="LongestDistance">The longest the link stretches while whipping, stabbing or bouncing.</param>
/// <param name="InitialAngle">The angle the link starts at.</param>
internal readonly record struct RidleyTailLinkDefinition(
    SignedEightEight RestDistance,
    SignedEightEight LongestDistance,
    RidleyTailAngle InitialAngle);
