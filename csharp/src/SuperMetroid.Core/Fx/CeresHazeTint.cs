namespace SuperMetroid.Core.Fx;

/// <summary>
/// The tint the Ceres haze adds (#2482), chosen once as the room's haze starts: later defeats do not change it. Its values
/// are arbitrary.
/// </summary>
public enum CeresHazeTint
{
    /// <summary>The blue haze of the station while Ridley lives.</summary>
    WhileRidleyLives,

    /// <summary>The red haze of the station once Ridley is dead.</summary>
    AfterRidleyDies,
}
