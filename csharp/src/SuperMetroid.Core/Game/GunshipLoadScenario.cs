namespace SuperMetroid.Core.Game;

/// <summary>
/// Native <c>loading_game_state</c> branch sampled while the Landing Site gunship is
/// initialized. These are mutually exclusive loader scenarios, not combinable flags.
/// </summary>
public enum GunshipLoadScenario
{
    /// <summary>Ordinary room loading initializes the Landing Site gunship in its normal parked scenario.</summary>
    Ordinary,
    /// <summary>The Ceres-escape loader branch initializes the gunship descent, landing, and Samus-ejection sequence.</summary>
    EscapingCeres,
}
