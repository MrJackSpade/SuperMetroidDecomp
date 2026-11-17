namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// The loading-game state sampled while the Landing Site gunship is
/// initialized. These are mutually exclusive loader scenarios, not combinable flags.
/// </summary>
public enum GunshipLoadScenario
{
    /// <summary>Ordinary room loading initializes the Landing Site gunship in its normal parked scenario.</summary>
    Ordinary,
    /// <summary>Arriving from the Ceres escape: the gunship descends, lands and lets Samus out.</summary>
    EscapingCeres,
}
