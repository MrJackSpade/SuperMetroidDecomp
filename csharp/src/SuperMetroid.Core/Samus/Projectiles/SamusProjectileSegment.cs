namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The Samus projectiles' animations (#1301): one segment per animation a definition or loop enters.</summary>
public enum SamusProjectileSegment
{
    /// <summary>The Power up facing right animation.</summary>
    PowerUpFacingRight,
    /// <summary>The Power up right animation.</summary>
    PowerUpRight,
    /// <summary>The Power right animation.</summary>
    PowerRight,
    /// <summary>The Power down right animation.</summary>
    PowerDownRight,
    /// <summary>The Power down facing right animation.</summary>
    PowerDownFacingRight,
    /// <summary>The Power down left animation.</summary>
    PowerDownLeft,
    /// <summary>The Power left animation.</summary>
    PowerLeft,
    /// <summary>The Power up left animation.</summary>
    PowerUpLeft,
    /// <summary>The Wave up facing right animation.</summary>
    WaveUpFacingRight,
    /// <summary>The Wave down facing right animation.</summary>
    WaveDownFacingRight,
    /// <summary>The Wave up right animation.</summary>
    WaveUpRight,
    /// <summary>The Wave right animation.</summary>
    WaveRight,
    /// <summary>The Wave down right animation.</summary>
    WaveDownRight,
    /// <summary>The Ice up facing right animation.</summary>
    IceUpFacingRight,
    /// <summary>The Spazer up facing right animation.</summary>
    SpazerUpFacingRight,
    /// <summary>The Spazer up facing right animation's repeating part.</summary>
    SpazerUpFacingRightLoop,
    /// <summary>The Spazer up right animation.</summary>
    SpazerUpRight,
    /// <summary>The Spazer up right animation's repeating part.</summary>
    SpazerUpRightLoop,
    /// <summary>The Spazer right animation.</summary>
    SpazerRight,
    /// <summary>The Spazer right animation's repeating part.</summary>
    SpazerRightLoop,
    /// <summary>The Spazer down right animation.</summary>
    SpazerDownRight,
    /// <summary>The Spazer down right animation's repeating part.</summary>
    SpazerDownRightLoop,
    /// <summary>The Spazer down facing right animation.</summary>
    SpazerDownFacingRight,
    /// <summary>The Spazer down facing right animation's repeating part.</summary>
    SpazerDownFacingRightLoop,
    /// <summary>The Spazer down left animation.</summary>
    SpazerDownLeft,
    /// <summary>The Spazer down left animation's repeating part.</summary>
    SpazerDownLeftLoop,
    /// <summary>The Spazer left animation.</summary>
    SpazerLeft,
    /// <summary>The Spazer left animation's repeating part.</summary>
    SpazerLeftLoop,
    /// <summary>The Spazer up left animation.</summary>
    SpazerUpLeft,
    /// <summary>The Spazer up left animation's repeating part.</summary>
    SpazerUpLeftLoop,
    /// <summary>The Spazer Wave up facing right animation.</summary>
    SpazerWaveUpFacingRight,
    /// <summary>The Spazer Wave up right animation.</summary>
    SpazerWaveUpRight,
    /// <summary>The Spazer Wave right animation.</summary>
    SpazerWaveRight,
    /// <summary>The Spazer Wave down right animation.</summary>
    SpazerWaveDownRight,
    /// <summary>The Spazer Wave down facing right animation.</summary>
    SpazerWaveDownFacingRight,
    /// <summary>The Spazer Wave down left animation.</summary>
    SpazerWaveDownLeft,
    /// <summary>The Spazer Wave left animation.</summary>
    SpazerWaveLeft,
    /// <summary>The Spazer Wave up left animation.</summary>
    SpazerWaveUpLeft,
    /// <summary>The Plasma up facing right animation.</summary>
    PlasmaUpFacingRight,
    /// <summary>The Plasma up facing right animation's repeating part.</summary>
    PlasmaUpFacingRightLoop,
    /// <summary>The Plasma up right animation.</summary>
    PlasmaUpRight,
    /// <summary>The Plasma up right animation's repeating part.</summary>
    PlasmaUpRightLoop,
    /// <summary>The Plasma right animation.</summary>
    PlasmaRight,
    /// <summary>The Plasma right animation's repeating part.</summary>
    PlasmaRightLoop,
    /// <summary>The Plasma down right animation.</summary>
    PlasmaDownRight,
    /// <summary>The Plasma down right animation's repeating part.</summary>
    PlasmaDownRightLoop,
    /// <summary>The Plasma Ice Wave up facing right animation.</summary>
    PlasmaIceWaveUpFacingRight,
    /// <summary>The Plasma Wave up facing right animation.</summary>
    PlasmaWaveUpFacingRight,
    /// <summary>The Plasma Ice Wave up right animation.</summary>
    PlasmaIceWaveUpRight,
    /// <summary>The Plasma Wave up right animation.</summary>
    PlasmaWaveUpRight,
    /// <summary>The Plasma Wave right animation.</summary>
    PlasmaWaveRight,
    /// <summary>The Plasma Wave right animation's repeating part.</summary>
    PlasmaWaveRightLoop,
    /// <summary>The Plasma Ice Wave down right animation.</summary>
    PlasmaIceWaveDownRight,
    /// <summary>The Plasma Wave down right animation.</summary>
    PlasmaWaveDownRight,
    /// <summary>The charged Power up facing right animation.</summary>
    ChargedPowerUpFacingRight,
    /// <summary>The charged Power up right animation.</summary>
    ChargedPowerUpRight,
    /// <summary>The charged Power right animation.</summary>
    ChargedPowerRight,
    /// <summary>The charged Power down right animation.</summary>
    ChargedPowerDownRight,
    /// <summary>The charged Power down facing right animation.</summary>
    ChargedPowerDownFacingRight,
    /// <summary>The charged Power down left animation.</summary>
    ChargedPowerDownLeft,
    /// <summary>The charged Power left animation.</summary>
    ChargedPowerLeft,
    /// <summary>The charged Power up left animation.</summary>
    ChargedPowerUpLeft,
    /// <summary>The charged Wave up facing right animation.</summary>
    ChargedWaveUpFacingRight,
    /// <summary>The charged Wave down facing right animation.</summary>
    ChargedWaveDownFacingRight,
    /// <summary>The charged Wave up right animation.</summary>
    ChargedWaveUpRight,
    /// <summary>The charged Wave right animation.</summary>
    ChargedWaveRight,
    /// <summary>The charged Wave down right animation.</summary>
    ChargedWaveDownRight,
    /// <summary>The charged Ice up facing right animation.</summary>
    ChargedIceUpFacingRight,
    /// <summary>The charged Ice Wave up facing right animation.</summary>
    ChargedIceWaveUpFacingRight,
    /// <summary>The charged Ice Wave down facing right animation.</summary>
    ChargedIceWaveDownFacingRight,
    /// <summary>The charged Ice Wave up right animation.</summary>
    ChargedIceWaveUpRight,
    /// <summary>The charged Ice Wave right animation.</summary>
    ChargedIceWaveRight,
    /// <summary>The charged Ice Wave down right animation.</summary>
    ChargedIceWaveDownRight,
    /// <summary>The charged Spazer up facing right animation.</summary>
    ChargedSpazerUpFacingRight,
    /// <summary>The charged Spazer up facing right animation's repeating part.</summary>
    ChargedSpazerUpFacingRightLoop,
    /// <summary>The charged Spazer up right animation.</summary>
    ChargedSpazerUpRight,
    /// <summary>The charged Spazer up right animation's repeating part.</summary>
    ChargedSpazerUpRightLoop,
    /// <summary>The charged Spazer right animation.</summary>
    ChargedSpazerRight,
    /// <summary>The charged Spazer right animation's repeating part.</summary>
    ChargedSpazerRightLoop,
    /// <summary>The charged Spazer down right animation.</summary>
    ChargedSpazerDownRight,
    /// <summary>The charged Spazer down right animation's repeating part.</summary>
    ChargedSpazerDownRightLoop,
    /// <summary>The charged Spazer Wave up facing right animation.</summary>
    ChargedSpazerWaveUpFacingRight,
    /// <summary>The charged Spazer Wave up facing right animation's repeating part.</summary>
    ChargedSpazerWaveUpFacingRightLoop,
    /// <summary>The charged Spazer Wave up right animation.</summary>
    ChargedSpazerWaveUpRight,
    /// <summary>The charged Spazer Wave up right animation's repeating part.</summary>
    ChargedSpazerWaveUpRightLoop,
    /// <summary>The charged Spazer Wave right animation.</summary>
    ChargedSpazerWaveRight,
    /// <summary>The charged Spazer Wave right animation's repeating part.</summary>
    ChargedSpazerWaveRightLoop,
    /// <summary>The charged Spazer Wave down right animation.</summary>
    ChargedSpazerWaveDownRight,
    /// <summary>The charged Spazer Wave down right animation's repeating part.</summary>
    ChargedSpazerWaveDownRightLoop,
    /// <summary>The charged Spazer Wave down facing right animation.</summary>
    ChargedSpazerWaveDownFacingRight,
    /// <summary>The charged Spazer Wave down facing right animation's repeating part.</summary>
    ChargedSpazerWaveDownFacingRightLoop,
    /// <summary>The charged Spazer Wave down left animation.</summary>
    ChargedSpazerWaveDownLeft,
    /// <summary>The charged Spazer Wave down left animation's repeating part.</summary>
    ChargedSpazerWaveDownLeftLoop,
    /// <summary>The charged Spazer Wave left animation.</summary>
    ChargedSpazerWaveLeft,
    /// <summary>The charged Spazer Wave left animation's repeating part.</summary>
    ChargedSpazerWaveLeftLoop,
    /// <summary>The charged Spazer Wave up left animation.</summary>
    ChargedSpazerWaveUpLeft,
    /// <summary>The charged Spazer Wave up left animation's repeating part.</summary>
    ChargedSpazerWaveUpLeftLoop,
    /// <summary>The charged Plasma up facing right animation.</summary>
    ChargedPlasmaUpFacingRight,
    /// <summary>The charged Plasma up facing right animation's repeating part.</summary>
    ChargedPlasmaUpFacingRightLoop,
    /// <summary>The charged Plasma up right animation.</summary>
    ChargedPlasmaUpRight,
    /// <summary>The charged Plasma up right animation's repeating part.</summary>
    ChargedPlasmaUpRightLoop,
    /// <summary>The charged Plasma right animation.</summary>
    ChargedPlasmaRight,
    /// <summary>The charged Plasma right animation's repeating part.</summary>
    ChargedPlasmaRightLoop,
    /// <summary>The charged Plasma down right animation.</summary>
    ChargedPlasmaDownRight,
    /// <summary>The charged Plasma down right animation's repeating part.</summary>
    ChargedPlasmaDownRightLoop,
    /// <summary>The charged Plasma Wave up facing right animation.</summary>
    ChargedPlasmaWaveUpFacingRight,
    /// <summary>The charged Plasma Wave up facing right animation's repeating part.</summary>
    ChargedPlasmaWaveUpFacingRightLoop,
    /// <summary>The charged Plasma Wave up right animation.</summary>
    ChargedPlasmaWaveUpRight,
    /// <summary>The charged Plasma Wave up right animation's repeating part.</summary>
    ChargedPlasmaWaveUpRightLoop,
    /// <summary>The charged Plasma Wave right animation.</summary>
    ChargedPlasmaWaveRight,
    /// <summary>The charged Plasma Wave right animation's repeating part.</summary>
    ChargedPlasmaWaveRightLoop,
    /// <summary>The charged Plasma Wave down right animation.</summary>
    ChargedPlasmaWaveDownRight,
    /// <summary>The charged Plasma Wave down right animation's repeating part.</summary>
    ChargedPlasmaWaveDownRightLoop,
    /// <summary>The Missile up facing right animation.</summary>
    MissileUpFacingRight,
    /// <summary>The Missile up right animation.</summary>
    MissileUpRight,
    /// <summary>The Missile right animation.</summary>
    MissileRight,
    /// <summary>The Missile down right animation.</summary>
    MissileDownRight,
    /// <summary>The Missile down facing right animation.</summary>
    MissileDownFacingRight,
    /// <summary>The Missile down left animation.</summary>
    MissileDownLeft,
    /// <summary>The Missile left animation.</summary>
    MissileLeft,
    /// <summary>The Missile up left animation.</summary>
    MissileUpLeft,
    /// <summary>The Super Missile up facing right animation.</summary>
    SuperMissileUpFacingRight,
    /// <summary>The Super Missile up right animation.</summary>
    SuperMissileUpRight,
    /// <summary>The Super Missile right animation.</summary>
    SuperMissileRight,
    /// <summary>The Super Missile down right animation.</summary>
    SuperMissileDownRight,
    /// <summary>The Super Missile down facing right animation.</summary>
    SuperMissileDownFacingRight,
    /// <summary>The Super Missile down left animation.</summary>
    SuperMissileDownLeft,
    /// <summary>The Super Missile left animation.</summary>
    SuperMissileLeft,
    /// <summary>The Super Missile up left animation.</summary>
    SuperMissileUpLeft,
    /// <summary>The Super Missile link animation.</summary>
    SuperMissileLink,
    /// <summary>The Power Bomb animation.</summary>
    PowerBomb,
    /// <summary>The fast Power Bomb animation.</summary>
    FastPowerBomb,
    /// <summary>The Bomb animation.</summary>
    Bomb,
    /// <summary>The fast Bomb animation.</summary>
    FastBomb,
    /// <summary>The beam explosion animation.</summary>
    BeamExplosion,
    /// <summary>The Missile explosion animation.</summary>
    MissileExplosion,
    /// <summary>The Bomb explosion animation.</summary>
    BombExplosion,
    /// <summary>The Plasma SBA animation.</summary>
    PlasmaSBA,
    /// <summary>The Super Missile explosion animation.</summary>
    SuperMissileExplosion,
    /// <summary>The Shinespark echo up facing right animation.</summary>
    ShinesparkEchoUpFacingRight,
    /// <summary>The Spazer SBA trail up facing right animation.</summary>
    SpazerSBATrailUpFacingRight,
    /// <summary>The Spazer SBA trail up facing right animation's repeating part.</summary>
    SpazerSBATrailUpFacingRightLoop,
    /// <summary>The Wave SBA animation.</summary>
    WaveSBA,
}
