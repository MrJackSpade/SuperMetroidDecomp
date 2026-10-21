namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>A list of trail placements, read by the projectile's animation frame.</summary>
internal enum ProjectileTrailPlacementList
{
    /// <summary>Where <see cref="ProjectileTrailFamily.UnchargedDefault"/> trails start for every direction.</summary>
    UnchargedDefault,

    /// <summary>Where <see cref="ProjectileTrailFamily.UnchargedWave"/> trails start for vertical travel.</summary>
    UnchargedWaveVertical,

    /// <summary>Where <see cref="ProjectileTrailFamily.UnchargedWave"/> trails start for horizontal travel.</summary>
    UnchargedWaveHorizontal,

    /// <summary>Where <see cref="ProjectileTrailFamily.UnchargedWave"/> trails start for up-right and down-left travel.</summary>
    UnchargedWaveRisingDiagonal,

    /// <summary>Where <see cref="ProjectileTrailFamily.UnchargedWave"/> trails start for down-right and up-left travel.</summary>
    UnchargedWaveFallingDiagonal,

    /// <summary>Where <see cref="ProjectileTrailFamily.UnchargedIceSpazer"/> trails start for upward travel.</summary>
    UnchargedIceSpazerUp,

    /// <summary>Where <see cref="ProjectileTrailFamily.UnchargedIceSpazer"/> trails start for downward travel.</summary>
    UnchargedIceSpazerDown,

    /// <summary>Where <see cref="ProjectileTrailFamily.UnchargedIceSpazer"/> trails start for up-right travel.</summary>
    UnchargedIceSpazerUpRight,

    /// <summary>Where <see cref="ProjectileTrailFamily.UnchargedIceSpazer"/> trails start for rightward travel.</summary>
    UnchargedIceSpazerRight,

    /// <summary>Where <see cref="ProjectileTrailFamily.UnchargedIceSpazer"/> trails start for down-right travel.</summary>
    UnchargedIceSpazerDownRight,

    /// <summary>Where <see cref="ProjectileTrailFamily.UnchargedIceSpazer"/> trails start for down-left travel.</summary>
    UnchargedIceSpazerDownLeft,

    /// <summary>Where <see cref="ProjectileTrailFamily.UnchargedIceSpazer"/> trails start for leftward travel.</summary>
    UnchargedIceSpazerLeft,

    /// <summary>Where <see cref="ProjectileTrailFamily.UnchargedIceSpazer"/> trails start for up-left travel.</summary>
    UnchargedIceSpazerUpLeft,

    /// <summary>Where <see cref="ProjectileTrailFamily.UnchargedWaveIceSpazer"/> trails start for upward travel.</summary>
    UnchargedWaveIceSpazerUp,

    /// <summary>Where <see cref="ProjectileTrailFamily.UnchargedWaveIceSpazer"/> trails start for up-right travel.</summary>
    UnchargedWaveIceSpazerUpRight,

    /// <summary>Where <see cref="ProjectileTrailFamily.UnchargedWaveIceSpazer"/> trails start for rightward travel.</summary>
    UnchargedWaveIceSpazerRight,

    /// <summary>Where <see cref="ProjectileTrailFamily.UnchargedWaveIceSpazer"/> trails start for down-right travel.</summary>
    UnchargedWaveIceSpazerDownRight,

    /// <summary>Where <see cref="ProjectileTrailFamily.UnchargedWaveIceSpazer"/> trails start for downward travel.</summary>
    UnchargedWaveIceSpazerDown,

    /// <summary>Where <see cref="ProjectileTrailFamily.UnchargedWaveIceSpazer"/> trails start for down-left travel.</summary>
    UnchargedWaveIceSpazerDownLeft,

    /// <summary>Where <see cref="ProjectileTrailFamily.UnchargedWaveIceSpazer"/> trails start for leftward travel.</summary>
    UnchargedWaveIceSpazerLeft,

    /// <summary>Where <see cref="ProjectileTrailFamily.UnchargedWaveIceSpazer"/> trails start for up-left travel.</summary>
    UnchargedWaveIceSpazerUpLeft,

    /// <summary>Where <see cref="ProjectileTrailFamily.UnchargedIcePlasma"/> trails start for upward travel.</summary>
    UnchargedIcePlasmaUp,

    /// <summary>Where <see cref="ProjectileTrailFamily.UnchargedIcePlasma"/> trails start for up-right travel.</summary>
    UnchargedIcePlasmaUpRight,

    /// <summary>Where <see cref="ProjectileTrailFamily.UnchargedIcePlasma"/> trails start for rightward travel.</summary>
    UnchargedIcePlasmaRight,

    /// <summary>Where <see cref="ProjectileTrailFamily.UnchargedIcePlasma"/> trails start for down-right travel.</summary>
    UnchargedIcePlasmaDownRight,

    /// <summary>Where <see cref="ProjectileTrailFamily.UnchargedIcePlasma"/> trails start for downward travel.</summary>
    UnchargedIcePlasmaDown,

    /// <summary>Where <see cref="ProjectileTrailFamily.UnchargedIcePlasma"/> trails start for down-left travel.</summary>
    UnchargedIcePlasmaDownLeft,

    /// <summary>Where <see cref="ProjectileTrailFamily.UnchargedIcePlasma"/> trails start for leftward travel.</summary>
    UnchargedIcePlasmaLeft,

    /// <summary>Where <see cref="ProjectileTrailFamily.UnchargedIcePlasma"/> trails start for up-left travel.</summary>
    UnchargedIcePlasmaUpLeft,

    /// <summary>Where <see cref="ProjectileTrailFamily.UnchargedWaveIcePlasma"/> trails start for upward travel.</summary>
    UnchargedWaveIcePlasmaUp,

    /// <summary>Where <see cref="ProjectileTrailFamily.UnchargedWaveIcePlasma"/> trails start for up-right travel.</summary>
    UnchargedWaveIcePlasmaUpRight,

    /// <summary>Where <see cref="ProjectileTrailFamily.UnchargedWaveIcePlasma"/> trails start for rightward travel.</summary>
    UnchargedWaveIcePlasmaRight,

    /// <summary>Where <see cref="ProjectileTrailFamily.UnchargedWaveIcePlasma"/> trails start for down-right travel.</summary>
    UnchargedWaveIcePlasmaDownRight,

    /// <summary>Where <see cref="ProjectileTrailFamily.UnchargedWaveIcePlasma"/> trails start for downward travel.</summary>
    UnchargedWaveIcePlasmaDown,

    /// <summary>Where <see cref="ProjectileTrailFamily.UnchargedWaveIcePlasma"/> trails start for down-left travel.</summary>
    UnchargedWaveIcePlasmaDownLeft,

    /// <summary>Where <see cref="ProjectileTrailFamily.UnchargedWaveIcePlasma"/> trails start for leftward travel.</summary>
    UnchargedWaveIcePlasmaLeft,

    /// <summary>Where <see cref="ProjectileTrailFamily.UnchargedWaveIcePlasma"/> trails start for up-left travel.</summary>
    UnchargedWaveIcePlasmaUpLeft,

    /// <summary>Where <see cref="ProjectileTrailFamily.ChargedDefault"/> trails start for every direction.</summary>
    ChargedDefault,

    /// <summary>Where <see cref="ProjectileTrailFamily.ChargedWave"/> trails start for vertical travel.</summary>
    ChargedWaveVertical,

    /// <summary>Where <see cref="ProjectileTrailFamily.ChargedWave"/> trails start for horizontal travel.</summary>
    ChargedWaveHorizontal,

    /// <summary>Where <see cref="ProjectileTrailFamily.ChargedWave"/> trails start for up-right and down-left travel.</summary>
    ChargedWaveRisingDiagonal,

    /// <summary>Where <see cref="ProjectileTrailFamily.ChargedWave"/> trails start for down-right and up-left travel.</summary>
    ChargedWaveFallingDiagonal,

    /// <summary>Where <see cref="ProjectileTrailFamily.ChargedIceSpazer"/> trails start for upward travel.</summary>
    ChargedIceSpazerUp,

    /// <summary>Where <see cref="ProjectileTrailFamily.ChargedIceSpazer"/> trails start for up-right travel.</summary>
    ChargedIceSpazerUpRight,

    /// <summary>Where <see cref="ProjectileTrailFamily.ChargedIceSpazer"/> trails start for rightward travel.</summary>
    ChargedIceSpazerRight,

    /// <summary>Where <see cref="ProjectileTrailFamily.ChargedIceSpazer"/> trails start for down-right travel.</summary>
    ChargedIceSpazerDownRight,

    /// <summary>Where <see cref="ProjectileTrailFamily.ChargedIceSpazer"/> trails start for downward travel.</summary>
    ChargedIceSpazerDown,

    /// <summary>Where <see cref="ProjectileTrailFamily.ChargedIceSpazer"/> trails start for down-left travel.</summary>
    ChargedIceSpazerDownLeft,

    /// <summary>Where <see cref="ProjectileTrailFamily.ChargedIceSpazer"/> trails start for leftward travel.</summary>
    ChargedIceSpazerLeft,

    /// <summary>Where <see cref="ProjectileTrailFamily.ChargedIceSpazer"/> trails start for up-left travel.</summary>
    ChargedIceSpazerUpLeft,

    /// <summary>Where <see cref="ProjectileTrailFamily.ChargedWaveIceSpazer"/> trails start for upward travel.</summary>
    ChargedWaveIceSpazerUp,

    /// <summary>Where <see cref="ProjectileTrailFamily.ChargedWaveIceSpazer"/> trails start for up-right travel.</summary>
    ChargedWaveIceSpazerUpRight,

    /// <summary>Where <see cref="ProjectileTrailFamily.ChargedWaveIceSpazer"/> trails start for rightward travel.</summary>
    ChargedWaveIceSpazerRight,

    /// <summary>Where <see cref="ProjectileTrailFamily.ChargedWaveIceSpazer"/> trails start for down-right travel.</summary>
    ChargedWaveIceSpazerDownRight,

    /// <summary>Where <see cref="ProjectileTrailFamily.ChargedWaveIceSpazer"/> trails start for downward travel.</summary>
    ChargedWaveIceSpazerDown,

    /// <summary>Where <see cref="ProjectileTrailFamily.ChargedWaveIceSpazer"/> trails start for down-left travel.</summary>
    ChargedWaveIceSpazerDownLeft,

    /// <summary>Where <see cref="ProjectileTrailFamily.ChargedWaveIceSpazer"/> trails start for leftward travel.</summary>
    ChargedWaveIceSpazerLeft,

    /// <summary>Where <see cref="ProjectileTrailFamily.ChargedWaveIceSpazer"/> trails start for up-left travel.</summary>
    ChargedWaveIceSpazerUpLeft,

    /// <summary>Where <see cref="ProjectileTrailFamily.ChargedIcePlasma"/> trails start for upward travel.</summary>
    ChargedIcePlasmaUp,

    /// <summary>Where <see cref="ProjectileTrailFamily.ChargedIcePlasma"/> trails start for up-right travel.</summary>
    ChargedIcePlasmaUpRight,

    /// <summary>Where <see cref="ProjectileTrailFamily.ChargedIcePlasma"/> trails start for rightward travel.</summary>
    ChargedIcePlasmaRight,

    /// <summary>Where <see cref="ProjectileTrailFamily.ChargedIcePlasma"/> trails start for down-right travel.</summary>
    ChargedIcePlasmaDownRight,

    /// <summary>Where <see cref="ProjectileTrailFamily.ChargedIcePlasma"/> trails start for downward travel.</summary>
    ChargedIcePlasmaDown,

    /// <summary>Where <see cref="ProjectileTrailFamily.ChargedIcePlasma"/> trails start for down-left travel.</summary>
    ChargedIcePlasmaDownLeft,

    /// <summary>Where <see cref="ProjectileTrailFamily.ChargedIcePlasma"/> trails start for leftward travel.</summary>
    ChargedIcePlasmaLeft,

    /// <summary>Where <see cref="ProjectileTrailFamily.ChargedIcePlasma"/> trails start for up-left travel.</summary>
    ChargedIcePlasmaUpLeft,

    /// <summary>Where <see cref="ProjectileTrailFamily.ChargedWaveIcePlasma"/> trails start for upward travel.</summary>
    ChargedWaveIcePlasmaUp,

    /// <summary>Where <see cref="ProjectileTrailFamily.ChargedWaveIcePlasma"/> trails start for up-right travel.</summary>
    ChargedWaveIcePlasmaUpRight,

    /// <summary>Where <see cref="ProjectileTrailFamily.ChargedWaveIcePlasma"/> trails start for rightward travel.</summary>
    ChargedWaveIcePlasmaRight,

    /// <summary>Where <see cref="ProjectileTrailFamily.ChargedWaveIcePlasma"/> trails start for down-right travel.</summary>
    ChargedWaveIcePlasmaDownRight,

    /// <summary>Where <see cref="ProjectileTrailFamily.ChargedWaveIcePlasma"/> trails start for downward travel.</summary>
    ChargedWaveIcePlasmaDown,

    /// <summary>Where <see cref="ProjectileTrailFamily.ChargedWaveIcePlasma"/> trails start for down-left travel.</summary>
    ChargedWaveIcePlasmaDownLeft,

    /// <summary>Where <see cref="ProjectileTrailFamily.ChargedWaveIcePlasma"/> trails start for leftward travel.</summary>
    ChargedWaveIcePlasmaLeft,

    /// <summary>Where <see cref="ProjectileTrailFamily.ChargedWaveIcePlasma"/> trails start for up-left travel.</summary>
    ChargedWaveIcePlasmaUpLeft,

    /// <summary>Where <see cref="ProjectileTrailFamily.SbaWaveSpazer"/> trails start for vertical travel.</summary>
    SbaWaveSpazerVertical,

    /// <summary>Where <see cref="ProjectileTrailFamily.SbaWaveSpazer"/> trails start for up-right and down-left travel.</summary>
    SbaWaveSpazerRisingDiagonal,

    /// <summary>Where <see cref="ProjectileTrailFamily.SbaWaveSpazer"/> trails start for horizontal travel.</summary>
    SbaWaveSpazerHorizontal,

    /// <summary>Where <see cref="ProjectileTrailFamily.SbaWaveSpazer"/> trails start for down-right and up-left travel.</summary>
    SbaWaveSpazerFallingDiagonal,

    /// <summary>Where <see cref="ProjectileTrailFamily.SbaSpazer"/> trails start for every direction but straight down.</summary>
    SbaSpazer,

    /// <summary>Where <see cref="ProjectileTrailFamily.SbaSpazer"/> trails start for downward travel.</summary>
    SbaSpazerDown,
}
