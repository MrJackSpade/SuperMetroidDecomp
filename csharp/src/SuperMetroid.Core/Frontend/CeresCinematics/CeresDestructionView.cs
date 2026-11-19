namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// What Ceres's destruction shows behind and among its actors (#1467, #1513, #2541): nothing before the station is set
/// up, the station's turning floor, the explosion with the gunship leaving it, or the floor cleared with Samus aboard;
/// then the Zebes reveal's planet picture, the gunship's rear view on the approach, and the planet alone once the gunship's
/// floor is hidden. The scene moves it on; it never goes back.
/// </summary>
internal enum CeresDestructionView
{
    /// <summary>Nothing: the scene waits, black, before it sets the station up.</summary>
    BeforeStation,
    /// <summary>The station's turning floor, with the asteroids and blasts in front of it.</summary>
    Station,
    /// <summary>The station's final explosion with the gunship leaving it, on the turning floor.</summary>
    GunshipLeavingExplosion,
    /// <summary>The floor the final explosion cleared with Samus still aboard, which shows nothing.</summary>
    StationCleared,
    /// <summary>The Zebes reveal's planet picture, pixelated in the reveal's blocks.</summary>
    ZebesReveal,
    /// <summary>The planet and its stars behind the gunship's turning floor on the approach to Zebes.</summary>
    ZebesApproach,
    /// <summary>The planet and its stars alone, the gunship's floor hidden before the closing hold.</summary>
    ZebesClose,
}
