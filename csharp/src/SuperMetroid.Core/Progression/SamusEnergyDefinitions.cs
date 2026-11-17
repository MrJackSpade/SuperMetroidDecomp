namespace SuperMetroid.Core.Progression;

/// <summary>Samus's energy at the start of a game, the energy a tank holds and the level below which it is critical (#2280, #2368).</summary>
internal static class SamusEnergyDefinitions
{
    /// <summary>The energy Samus starts a game with and her maximum before any energy tank.</summary>
    internal const int StartingEnergy = 99;

    /// <summary>
    /// The energy one energy tank or reserve tank holds: each collected adds it to her capacity, and the HUD, file select, the
    /// equipment screen and the ending count her tanks in it.
    /// </summary>
    internal const int EnergyPerTank = 100;

    /// <summary>Energy at or above this is healthy; below it the critical-energy warning sounds.</summary>
    internal const int HealthyEnergy = 31;
}
