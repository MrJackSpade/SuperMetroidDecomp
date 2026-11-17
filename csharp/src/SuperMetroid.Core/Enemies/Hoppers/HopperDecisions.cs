namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>The random field a Sidehopper reads to choose its next hop.</summary>
internal static class HopperDecisions
{
    /// <summary>The random choice that makes a Sidehopper prepare a big hop rather than a small one: taken by an odd sample.</summary>
    internal static readonly RandomSampleField BigHop = new(0, 1);

    /// <summary>The seed every hopper writes to the room's random generator as it is placed, before drawing once (#2339).</summary>
    internal const int SpawnRandomSeed = 37;
}
