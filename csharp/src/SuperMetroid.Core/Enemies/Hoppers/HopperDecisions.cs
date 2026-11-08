namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>The random field a Sidehopper reads to choose its next hop.</summary>
internal static class HopperDecisions
{
    /// <summary>The random bit that makes a Sidehopper prepare a big hop rather than a small one: bit zero.</summary>
    internal static readonly RandomSampleField BigHop = new(0, 1);
}
