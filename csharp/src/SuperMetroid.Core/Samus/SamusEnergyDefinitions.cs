namespace SuperMetroid.Core.Samus;

/// <summary>Samus's energy at the start of a game and the level below which it is critical (#2280).</summary>
internal static class SamusEnergyDefinitions
{
    /// <summary>The energy Samus starts a game with and her maximum before any energy tank.</summary>
    internal const int StartingEnergy = 99;

    /// <summary>Energy at or above this is healthy; below it the critical-energy warning sounds.</summary>
    internal const int HealthyEnergy = 31;
}
