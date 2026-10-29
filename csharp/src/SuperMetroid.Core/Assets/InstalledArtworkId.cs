namespace SuperMetroid.Core.Assets;

/// <summary>
/// Installed artwork the game loads by name (#1422): character runs and one tilemap row that rooms, the HUD, the escape,
/// Samus's beams and Grapple and the gunship's takeoff show. A name only; the installed content resolves
/// it.
/// </summary>
public enum InstalledArtworkId
{
    /// <summary>The standard gameplay HUD characters.</summary>
    StandardHudTiles = 1,
    /// <summary>The escape timer's first character run, its digits.</summary>
    EscapeTimerFirstTiles,
    /// <summary>The escape timer's second character run.</summary>
    EscapeTimerSecondTiles,
    /// <summary>Power beam artwork.</summary>
    BeamPowerTiles,
    /// <summary>Wave beam artwork.</summary>
    BeamWaveTiles,
    /// <summary>Ice beam artwork.</summary>
    BeamIceTiles,
    /// <summary>Ice/Wave artwork.</summary>
    BeamIceWaveTiles,
    /// <summary>Spazer artwork.</summary>
    BeamSpazerTiles,
    /// <summary>Spazer/Wave artwork.</summary>
    BeamSpazerWaveTiles,
    /// <summary>Spazer/Ice artwork.</summary>
    BeamSpazerIceTiles,
    /// <summary>Spazer/Ice/Wave artwork.</summary>
    BeamSpazerIceWaveTiles,
    /// <summary>Plasma artwork.</summary>
    BeamPlasmaTiles,
    /// <summary>Plasma/Wave artwork.</summary>
    BeamPlasmaWaveTiles,
    /// <summary>Plasma/Ice artwork.</summary>
    BeamPlasmaIceTiles,
    /// <summary>Plasma/Ice/Wave artwork.</summary>
    BeamPlasmaIceWaveTiles,
    /// <summary>The eight ice/wave projectile-trail characters.</summary>
    ProjectileIceWaveTrailTiles,
    /// <summary>The four missile/Super Missile trail characters.</summary>
    ProjectileMissileTrailTiles,
    /// <summary>The first Grapple endpoint frame.</summary>
    GrapplePointFirstTiles,
    /// <summary>The second Grapple endpoint frame.</summary>
    GrapplePointSecondTiles,
    /// <summary>The third Grapple endpoint frame.</summary>
    GrapplePointThirdTiles,
    /// <summary>The fourth Grapple endpoint frame.</summary>
    GrapplePointFourthTiles,
    /// <summary>The four horizontal Grapple segment frames.</summary>
    GrappleHorizontalSegmentTiles,
    /// <summary>The four diagonal Grapple segment frames.</summary>
    GrappleDiagonalSegmentTiles,
    /// <summary>The four vertical Grapple segment frames.</summary>
    GrappleVerticalSegmentTiles,
    /// <summary>First editable gunship takeoff character run.</summary>
    GunshipLiftoffFirstTiles,
    /// <summary>Second editable gunship takeoff character run.</summary>
    GunshipLiftoffSecondTiles,
    /// <summary>Third editable gunship takeoff character run.</summary>
    GunshipLiftoffThirdTiles,
    /// <summary>Fourth editable gunship takeoff character run.</summary>
    GunshipLiftoffFourthTiles,
    /// <summary>Fifth editable gunship takeoff character run.</summary>
    GunshipLiftoffFifthTiles,
    /// <summary>The Chainsaw beam's characters.</summary>
    BeamChainsawTiles,
    /// <summary>The SpaceTime beam's characters.</summary>
    BeamSpacetimeTiles,
    /// <summary>The standard sprite characters every room loads first.</summary>
    StandardObjectTiles,
    /// <summary>The gameplay HUD's unchanging first tilemap row.</summary>
    HudTopRowTilemap,
}
