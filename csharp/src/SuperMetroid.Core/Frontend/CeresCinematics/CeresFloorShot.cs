namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// What the Ceres cinematics' turning floor shows (#1467): the gunship's front and rear views on the approach to Ceres,
/// the station, the station's explosion with or without the departing gunship, and the gunship's rear view on the approach
/// to Zebes. Each is one picture of the whole floor, which the import composes once.
/// </summary>
public enum CeresFloorShot
{
    /// <summary>The gunship seen from the front as it flies toward the camera.</summary>
    FlightFront,
    /// <summary>The gunship seen from behind as it flies toward Ceres.</summary>
    FlightRear,
    /// <summary>The station, before its final explosion.</summary>
    Station,
    /// <summary>The station's final explosion with the gunship leaving it.</summary>
    Explosion,
    /// <summary>The station's final explosion with Samus still aboard: no gunship leaves.</summary>
    ExplosionWithoutGunship,
    /// <summary>The gunship seen from behind as it flies toward Zebes.</summary>
    ZebesApproach,
}
