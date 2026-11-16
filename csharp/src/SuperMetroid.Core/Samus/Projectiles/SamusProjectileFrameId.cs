namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// One frame a Samus projectile animation shows (#1301, #2124), named for what it shows: the shot of its animation and
/// the pose of the picture it shows. A frame that shows the same picture as an earlier frame of its animation at another
/// point of the animation is that frame's twin, named for it with Twin, then Twin and a letter. What a frame pictures is installed
/// only (#1596); its values are arbitrary.
/// </summary>
internal enum SamusProjectileFrameId
{
    /// <summary>A frame of <see cref="SamusProjectileSegment.PowerUpFacingRight"/> showing the Power picture, up.</summary>
    PowerUpFacingRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PowerUpRight"/> showing the Power picture, up right.</summary>
    PowerUpRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PowerRight"/> showing the Power picture, right.</summary>
    PowerRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PowerDownRight"/> showing the Power picture, down right.</summary>
    PowerDownRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PowerDownFacingRight"/> showing the Power picture, down.</summary>
    PowerDownFacingRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PowerDownLeft"/> showing the Power picture, down left.</summary>
    PowerDownLeft,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PowerLeft"/> showing the Power picture, left.</summary>
    PowerLeft,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PowerUpLeft"/> showing the Power picture, up left.</summary>
    PowerUpLeft,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveUpFacingRight"/> showing nothing.</summary>
    WaveUpFacingRightHidden,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownFacingRight"/> showing the Wave picture, centre.</summary>
    WaveDownFacingRightCentre,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownFacingRight"/> showing the Wave picture, down, right 1.</summary>
    WaveDownFacingRightRight1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownFacingRight"/> showing the Wave picture, down, right 2.</summary>
    WaveDownFacingRightRight2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownFacingRight"/> showing the Wave picture, down, right 3.</summary>
    WaveDownFacingRightRight3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownFacingRight"/> showing the Wave picture, down, right 4.</summary>
    WaveDownFacingRightRight4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownFacingRight"/> showing the Wave picture, down, right 3, as <see cref="WaveDownFacingRightRight3"/> does.</summary>
    WaveDownFacingRightRight3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownFacingRight"/> showing the Wave picture, down, right 2, as <see cref="WaveDownFacingRightRight2"/> does.</summary>
    WaveDownFacingRightRight2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownFacingRight"/> showing the Wave picture, down, right 1, as <see cref="WaveDownFacingRightRight1"/> does.</summary>
    WaveDownFacingRightRight1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownFacingRight"/> showing the Wave picture, centre, as <see cref="WaveDownFacingRightCentre"/> does.</summary>
    WaveDownFacingRightCentreTwin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownFacingRight"/> showing the Wave picture, down, left 1.</summary>
    WaveDownFacingRightLeft1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownFacingRight"/> showing the Wave picture, down, left 2.</summary>
    WaveDownFacingRightLeft2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownFacingRight"/> showing the Wave picture, down, left 3.</summary>
    WaveDownFacingRightLeft3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownFacingRight"/> showing the Wave picture, down, left 4.</summary>
    WaveDownFacingRightLeft4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownFacingRight"/> showing the Wave picture, down, left 3, as <see cref="WaveDownFacingRightLeft3"/> does.</summary>
    WaveDownFacingRightLeft3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownFacingRight"/> showing the Wave picture, down, left 2, as <see cref="WaveDownFacingRightLeft2"/> does.</summary>
    WaveDownFacingRightLeft2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownFacingRight"/> showing the Wave picture, down, left 1, as <see cref="WaveDownFacingRightLeft1"/> does.</summary>
    WaveDownFacingRightLeft1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveUpRight"/> showing the Wave picture, centre.</summary>
    WaveUpRightCentre,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveUpRight"/> showing the Wave picture, up right, up left 1.</summary>
    WaveUpRightUpLeft1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveUpRight"/> showing the Wave picture, up right, up left 2.</summary>
    WaveUpRightUpLeft2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveUpRight"/> showing the Wave picture, up right, up left 3.</summary>
    WaveUpRightUpLeft3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveUpRight"/> showing the Wave picture, up right, up left 4.</summary>
    WaveUpRightUpLeft4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveUpRight"/> showing the Wave picture, up right, up left 3, as <see cref="WaveUpRightUpLeft3"/> does.</summary>
    WaveUpRightUpLeft3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveUpRight"/> showing the Wave picture, up right, up left 2, as <see cref="WaveUpRightUpLeft2"/> does.</summary>
    WaveUpRightUpLeft2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveUpRight"/> showing the Wave picture, up right, up left 1, as <see cref="WaveUpRightUpLeft1"/> does.</summary>
    WaveUpRightUpLeft1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveUpRight"/> showing the Wave picture, centre, as <see cref="WaveUpRightCentre"/> does.</summary>
    WaveUpRightCentreTwin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveUpRight"/> showing the Wave picture, up right, down right 1.</summary>
    WaveUpRightDownRight1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveUpRight"/> showing the Wave picture, up right, down right 2.</summary>
    WaveUpRightDownRight2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveUpRight"/> showing the Wave picture, up right, down right 3.</summary>
    WaveUpRightDownRight3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveUpRight"/> showing the Wave picture, up right, down right 4.</summary>
    WaveUpRightDownRight4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveUpRight"/> showing the Wave picture, up right, down right 3, as <see cref="WaveUpRightDownRight3"/> does.</summary>
    WaveUpRightDownRight3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveUpRight"/> showing the Wave picture, up right, down right 2, as <see cref="WaveUpRightDownRight2"/> does.</summary>
    WaveUpRightDownRight2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveUpRight"/> showing the Wave picture, up right, down right 1, as <see cref="WaveUpRightDownRight1"/> does.</summary>
    WaveUpRightDownRight1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveRight"/> showing the Wave picture, centre.</summary>
    WaveRightCentre,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveRight"/> showing the Wave picture, right, up 1.</summary>
    WaveRightUp1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveRight"/> showing the Wave picture, right, up 2.</summary>
    WaveRightUp2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveRight"/> showing the Wave picture, right, up 3.</summary>
    WaveRightUp3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveRight"/> showing the Wave picture, right, up 4.</summary>
    WaveRightUp4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveRight"/> showing the Wave picture, right, up 3, as <see cref="WaveRightUp3"/> does.</summary>
    WaveRightUp3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveRight"/> showing the Wave picture, right, up 2, as <see cref="WaveRightUp2"/> does.</summary>
    WaveRightUp2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveRight"/> showing the Wave picture, right, up 1, as <see cref="WaveRightUp1"/> does.</summary>
    WaveRightUp1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveRight"/> showing the Wave picture, centre, as <see cref="WaveRightCentre"/> does.</summary>
    WaveRightCentreTwin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveRight"/> showing the Wave picture, right, down 1.</summary>
    WaveRightDown1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveRight"/> showing the Wave picture, right, down 2.</summary>
    WaveRightDown2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveRight"/> showing the Wave picture, right, down 3.</summary>
    WaveRightDown3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveRight"/> showing the Wave picture, right, down 4.</summary>
    WaveRightDown4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveRight"/> showing the Wave picture, right, down 3, as <see cref="WaveRightDown3"/> does.</summary>
    WaveRightDown3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveRight"/> showing the Wave picture, right, down 2, as <see cref="WaveRightDown2"/> does.</summary>
    WaveRightDown2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveRight"/> showing the Wave picture, right, down 1, as <see cref="WaveRightDown1"/> does.</summary>
    WaveRightDown1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownRight"/> showing the Wave picture, centre.</summary>
    WaveDownRightCentre,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownRight"/> showing the Wave picture, down right, up right 1.</summary>
    WaveDownRightUpRight1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownRight"/> showing the Wave picture, down right, up right 2.</summary>
    WaveDownRightUpRight2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownRight"/> showing the Wave picture, down right, up right 3.</summary>
    WaveDownRightUpRight3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownRight"/> showing the Wave picture, down right, up right 4.</summary>
    WaveDownRightUpRight4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownRight"/> showing the Wave picture, down right, up right 3, as <see cref="WaveDownRightUpRight3"/> does.</summary>
    WaveDownRightUpRight3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownRight"/> showing the Wave picture, down right, up right 2, as <see cref="WaveDownRightUpRight2"/> does.</summary>
    WaveDownRightUpRight2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownRight"/> showing the Wave picture, down right, up right 1, as <see cref="WaveDownRightUpRight1"/> does.</summary>
    WaveDownRightUpRight1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownRight"/> showing the Wave picture, centre, as <see cref="WaveDownRightCentre"/> does.</summary>
    WaveDownRightCentreTwin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownRight"/> showing the Wave picture, down right, down left 1.</summary>
    WaveDownRightDownLeft1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownRight"/> showing the Wave picture, down right, down left 2.</summary>
    WaveDownRightDownLeft2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownRight"/> showing the Wave picture, down right, down left 3.</summary>
    WaveDownRightDownLeft3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownRight"/> showing the Wave picture, down right, down left 4.</summary>
    WaveDownRightDownLeft4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownRight"/> showing the Wave picture, down right, down left 3, as <see cref="WaveDownRightDownLeft3"/> does.</summary>
    WaveDownRightDownLeft3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownRight"/> showing the Wave picture, down right, down left 2, as <see cref="WaveDownRightDownLeft2"/> does.</summary>
    WaveDownRightDownLeft2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownRight"/> showing the Wave picture, down right, down left 1, as <see cref="WaveDownRightDownLeft1"/> does.</summary>
    WaveDownRightDownLeft1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.IceUpFacingRight"/> showing the Ice picture, phase 1.</summary>
    IceUpFacingRightPhase1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.IceUpFacingRight"/> showing the Ice picture, phase 2.</summary>
    IceUpFacingRightPhase2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.IceUpFacingRight"/> showing the Ice picture, phase 3.</summary>
    IceUpFacingRightPhase3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.IceUpFacingRight"/> showing the Ice picture, phase 4.</summary>
    IceUpFacingRightPhase4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerUpFacingRight"/> showing the Spazer picture, up, seed.</summary>
    SpazerUpFacingRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerUpFacingRight"/> showing the Spazer picture, up, spread 2.</summary>
    SpazerUpFacingRightSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerUpFacingRightLoop"/> showing the Spazer picture, up, spread 5.</summary>
    SpazerUpFacingRightSpread5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerUpRight"/> showing the Spazer picture, up right, seed.</summary>
    SpazerUpRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerUpRight"/> showing the Spazer picture, up right, spread 2.</summary>
    SpazerUpRightSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerUpRightLoop"/> showing the Spazer picture, up right, spread 5.</summary>
    SpazerUpRightSpread5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerRight"/> showing the Spazer picture, right, seed.</summary>
    SpazerRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerRight"/> showing the Spazer picture, right, spread 2.</summary>
    SpazerRightSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerRightLoop"/> showing the Spazer picture, right, spread 5.</summary>
    SpazerRightSpread5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerDownRight"/> showing the Spazer picture, down right, seed.</summary>
    SpazerDownRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerDownRight"/> showing the Spazer picture, down right, spread 2.</summary>
    SpazerDownRightSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerDownRightLoop"/> showing the Spazer picture, down right, spread 5.</summary>
    SpazerDownRightSpread5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerDownFacingRight"/> showing the Spazer picture, down, seed.</summary>
    SpazerDownFacingRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerDownFacingRight"/> showing the Spazer picture, down, spread 2.</summary>
    SpazerDownFacingRightSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerDownFacingRightLoop"/> showing the Spazer picture, down, spread 5.</summary>
    SpazerDownFacingRightSpread5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerDownLeft"/> showing the Spazer picture, down left, seed.</summary>
    SpazerDownLeftSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerDownLeft"/> showing the Spazer picture, down left, spread 1.</summary>
    SpazerDownLeftSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerDownLeftLoop"/> showing the Spazer picture, down left, spread 4.</summary>
    SpazerDownLeftSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerLeft"/> showing the Spazer picture, left, seed.</summary>
    SpazerLeftSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerLeft"/> showing the Spazer picture, left, spread 2.</summary>
    SpazerLeftSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerLeftLoop"/> showing the Spazer picture, left, spread 5.</summary>
    SpazerLeftSpread5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerUpLeft"/> showing the Spazer picture, up left, seed.</summary>
    SpazerUpLeftSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerUpLeft"/> showing the Spazer picture, up left, spread 2.</summary>
    SpazerUpLeftSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerUpLeftLoop"/> showing the Spazer picture, up left, spread 5.</summary>
    SpazerUpLeftSpread5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpFacingRight"/> showing the Spazer picture, up, seed.</summary>
    SpazerWaveUpFacingRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpFacingRight"/> showing the Spazer picture, up, spread 1.</summary>
    SpazerWaveUpFacingRightSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpFacingRight"/> showing the Spazer picture, up, spread 2.</summary>
    SpazerWaveUpFacingRightSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpFacingRight"/> showing the Spazer picture, up, spread 3.</summary>
    SpazerWaveUpFacingRightSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpFacingRight"/> showing the Spazer picture, up, spread 4.</summary>
    SpazerWaveUpFacingRightSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpFacingRight"/> showing the Spazer picture, up, spread 5.</summary>
    SpazerWaveUpFacingRightSpread5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpFacingRight"/> showing the Spazer picture, up, spread 4, as <see cref="SpazerWaveUpFacingRightSpread4"/> does.</summary>
    SpazerWaveUpFacingRightSpread4Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpFacingRight"/> showing the Spazer picture, up, spread 3, as <see cref="SpazerWaveUpFacingRightSpread3"/> does.</summary>
    SpazerWaveUpFacingRightSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpFacingRight"/> showing the Spazer picture, up, spread 2, as <see cref="SpazerWaveUpFacingRightSpread2"/> does.</summary>
    SpazerWaveUpFacingRightSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpFacingRight"/> showing the Spazer picture, up, spread 1, as <see cref="SpazerWaveUpFacingRightSpread1"/> does.</summary>
    SpazerWaveUpFacingRightSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpRight"/> showing the Spazer picture, up right, seed.</summary>
    SpazerWaveUpRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpRight"/> showing the Spazer picture, up right, spread 1.</summary>
    SpazerWaveUpRightSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpRight"/> showing the Spazer picture, up right, spread 2.</summary>
    SpazerWaveUpRightSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpRight"/> showing the Spazer picture, up right, spread 3.</summary>
    SpazerWaveUpRightSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpRight"/> showing the Spazer picture, up right, spread 4.</summary>
    SpazerWaveUpRightSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpRight"/> showing the Spazer picture, up right, spread 5.</summary>
    SpazerWaveUpRightSpread5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpRight"/> showing the Spazer picture, up right, spread 4, as <see cref="SpazerWaveUpRightSpread4"/> does.</summary>
    SpazerWaveUpRightSpread4Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpRight"/> showing the Spazer picture, up right, spread 3, as <see cref="SpazerWaveUpRightSpread3"/> does.</summary>
    SpazerWaveUpRightSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpRight"/> showing the Spazer picture, up right, spread 2, as <see cref="SpazerWaveUpRightSpread2"/> does.</summary>
    SpazerWaveUpRightSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpRight"/> showing the Spazer picture, up right, spread 1, as <see cref="SpazerWaveUpRightSpread1"/> does.</summary>
    SpazerWaveUpRightSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveRight"/> showing the Spazer picture, right, seed.</summary>
    SpazerWaveRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveRight"/> showing the Spazer picture, right, spread 1.</summary>
    SpazerWaveRightSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveRight"/> showing the Spazer picture, right, spread 2.</summary>
    SpazerWaveRightSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveRight"/> showing the Spazer picture, right, spread 3.</summary>
    SpazerWaveRightSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveRight"/> showing the Spazer picture, right, spread 4.</summary>
    SpazerWaveRightSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveRight"/> showing the Spazer picture, right, spread 5.</summary>
    SpazerWaveRightSpread5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveRight"/> showing the Spazer picture, right, spread 4, as <see cref="SpazerWaveRightSpread4"/> does.</summary>
    SpazerWaveRightSpread4Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveRight"/> showing the Spazer picture, right, spread 3, as <see cref="SpazerWaveRightSpread3"/> does.</summary>
    SpazerWaveRightSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveRight"/> showing the Spazer picture, right, spread 2, as <see cref="SpazerWaveRightSpread2"/> does.</summary>
    SpazerWaveRightSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveRight"/> showing the Spazer picture, right, spread 1, as <see cref="SpazerWaveRightSpread1"/> does.</summary>
    SpazerWaveRightSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownRight"/> showing the Spazer picture, down right, seed.</summary>
    SpazerWaveDownRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownRight"/> showing the Spazer picture, down right, spread 1.</summary>
    SpazerWaveDownRightSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownRight"/> showing the Spazer picture, down right, spread 2.</summary>
    SpazerWaveDownRightSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownRight"/> showing the Spazer picture, down right, spread 3.</summary>
    SpazerWaveDownRightSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownRight"/> showing the Spazer picture, down right, spread 4.</summary>
    SpazerWaveDownRightSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownRight"/> showing the Spazer picture, down right, spread 5.</summary>
    SpazerWaveDownRightSpread5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownRight"/> showing the Spazer picture, down right, spread 4, as <see cref="SpazerWaveDownRightSpread4"/> does.</summary>
    SpazerWaveDownRightSpread4Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownRight"/> showing the Spazer picture, down right, spread 3, as <see cref="SpazerWaveDownRightSpread3"/> does.</summary>
    SpazerWaveDownRightSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownRight"/> showing the Spazer picture, down right, spread 2, as <see cref="SpazerWaveDownRightSpread2"/> does.</summary>
    SpazerWaveDownRightSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownRight"/> showing the Spazer picture, down right, spread 1, as <see cref="SpazerWaveDownRightSpread1"/> does.</summary>
    SpazerWaveDownRightSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownFacingRight"/> showing the Spazer picture, down, seed.</summary>
    SpazerWaveDownFacingRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownFacingRight"/> showing the Spazer picture, down, spread 1.</summary>
    SpazerWaveDownFacingRightSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownFacingRight"/> showing the Spazer picture, down, spread 2.</summary>
    SpazerWaveDownFacingRightSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownFacingRight"/> showing the Spazer picture, down, spread 3.</summary>
    SpazerWaveDownFacingRightSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownFacingRight"/> showing the Spazer picture, down, spread 4.</summary>
    SpazerWaveDownFacingRightSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownFacingRight"/> showing the Spazer picture, down, spread 5.</summary>
    SpazerWaveDownFacingRightSpread5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownFacingRight"/> showing the Spazer picture, down, spread 4, as <see cref="SpazerWaveDownFacingRightSpread4"/> does.</summary>
    SpazerWaveDownFacingRightSpread4Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownFacingRight"/> showing the Spazer picture, down, spread 3, as <see cref="SpazerWaveDownFacingRightSpread3"/> does.</summary>
    SpazerWaveDownFacingRightSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownFacingRight"/> showing the Spazer picture, down, spread 2, as <see cref="SpazerWaveDownFacingRightSpread2"/> does.</summary>
    SpazerWaveDownFacingRightSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownFacingRight"/> showing the Spazer picture, down, spread 1, as <see cref="SpazerWaveDownFacingRightSpread1"/> does.</summary>
    SpazerWaveDownFacingRightSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownLeft"/> showing the Spazer picture, down left, seed.</summary>
    SpazerWaveDownLeftSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownLeft"/> showing the Spazer picture, down left, spread 1.</summary>
    SpazerWaveDownLeftSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownLeft"/> showing the Spazer picture, down left, spread 2.</summary>
    SpazerWaveDownLeftSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownLeft"/> showing the Spazer picture, down left, spread 3.</summary>
    SpazerWaveDownLeftSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownLeft"/> showing the Spazer picture, down left, spread 4.</summary>
    SpazerWaveDownLeftSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownLeft"/> showing the Spazer picture, down left, spread 4, as <see cref="SpazerWaveDownLeftSpread4"/> does.</summary>
    SpazerWaveDownLeftSpread4Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownLeft"/> showing the Spazer picture, down left, spread 4, as <see cref="SpazerWaveDownLeftSpread4"/> does.</summary>
    SpazerWaveDownLeftSpread4TwinB,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownLeft"/> showing the Spazer picture, down left, spread 3, as <see cref="SpazerWaveDownLeftSpread3"/> does.</summary>
    SpazerWaveDownLeftSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownLeft"/> showing the Spazer picture, down left, spread 2, as <see cref="SpazerWaveDownLeftSpread2"/> does.</summary>
    SpazerWaveDownLeftSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownLeft"/> showing the Spazer picture, down left, spread 1, as <see cref="SpazerWaveDownLeftSpread1"/> does.</summary>
    SpazerWaveDownLeftSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveLeft"/> showing the Spazer picture, left, seed.</summary>
    SpazerWaveLeftSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveLeft"/> showing the Spazer picture, left, spread 1.</summary>
    SpazerWaveLeftSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveLeft"/> showing the Spazer picture, left, spread 2.</summary>
    SpazerWaveLeftSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveLeft"/> showing the Spazer picture, left, spread 3.</summary>
    SpazerWaveLeftSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveLeft"/> showing the Spazer picture, left, spread 4.</summary>
    SpazerWaveLeftSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveLeft"/> showing the Spazer picture, left, spread 5.</summary>
    SpazerWaveLeftSpread5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveLeft"/> showing the Spazer picture, left, spread 4, as <see cref="SpazerWaveLeftSpread4"/> does.</summary>
    SpazerWaveLeftSpread4Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveLeft"/> showing the Spazer picture, left, spread 3, as <see cref="SpazerWaveLeftSpread3"/> does.</summary>
    SpazerWaveLeftSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveLeft"/> showing the Spazer picture, left, spread 2, as <see cref="SpazerWaveLeftSpread2"/> does.</summary>
    SpazerWaveLeftSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveLeft"/> showing the Spazer picture, left, spread 1, as <see cref="SpazerWaveLeftSpread1"/> does.</summary>
    SpazerWaveLeftSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpLeft"/> showing the Spazer picture, up left, seed.</summary>
    SpazerWaveUpLeftSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpLeft"/> showing the Spazer picture, up left, spread 1.</summary>
    SpazerWaveUpLeftSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpLeft"/> showing the Spazer picture, up left, spread 2.</summary>
    SpazerWaveUpLeftSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpLeft"/> showing the Spazer picture, up left, spread 3.</summary>
    SpazerWaveUpLeftSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpLeft"/> showing the Spazer picture, up left, spread 4.</summary>
    SpazerWaveUpLeftSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpLeft"/> showing the Spazer picture, up left, spread 5.</summary>
    SpazerWaveUpLeftSpread5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpLeft"/> showing the Spazer picture, up left, spread 4, as <see cref="SpazerWaveUpLeftSpread4"/> does.</summary>
    SpazerWaveUpLeftSpread4Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpLeft"/> showing the Spazer picture, up left, spread 3, as <see cref="SpazerWaveUpLeftSpread3"/> does.</summary>
    SpazerWaveUpLeftSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpLeft"/> showing the Spazer picture, up left, spread 2, as <see cref="SpazerWaveUpLeftSpread2"/> does.</summary>
    SpazerWaveUpLeftSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpLeft"/> showing the Spazer picture, up left, spread 1, as <see cref="SpazerWaveUpLeftSpread1"/> does.</summary>
    SpazerWaveUpLeftSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaUpFacingRight"/> showing the Plasma Startup picture, up, core.</summary>
    PlasmaUpFacingRightCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaUpFacingRightLoop"/> showing the Plasma picture, up.</summary>
    PlasmaUpFacingRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaUpRight"/> showing the Plasma Startup picture, up right, core.</summary>
    PlasmaUpRightCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaUpRightLoop"/> showing the Plasma picture, up right.</summary>
    PlasmaUpRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaRight"/> showing the Plasma Startup picture, right, core.</summary>
    PlasmaRightCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaRightLoop"/> showing the Plasma picture, right.</summary>
    PlasmaRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaDownRight"/> showing the Plasma Startup picture, down right, core.</summary>
    PlasmaDownRightCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaDownRightLoop"/> showing the Plasma picture, down right.</summary>
    PlasmaDownRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaIceWaveUpFacingRight"/> showing the Plasma Startup picture, up, core.</summary>
    PlasmaIceWaveUpFacingRightCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveUpFacingRight"/> showing the Plasma Wave picture, up short, core.</summary>
    PlasmaWaveUpFacingRightShortCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveUpFacingRight"/> showing the Plasma Wave picture, up short, spread 1.</summary>
    PlasmaWaveUpFacingRightShortSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveUpFacingRight"/> showing the Plasma Wave picture, up short, spread 2.</summary>
    PlasmaWaveUpFacingRightShortSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveUpFacingRight"/> showing the Plasma Wave picture, up short, spread 3.</summary>
    PlasmaWaveUpFacingRightShortSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveUpFacingRight"/> showing the Plasma Wave picture, up short, spread 4.</summary>
    PlasmaWaveUpFacingRightShortSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveUpFacingRight"/> showing the Plasma Wave picture, up short, spread 3, as <see cref="PlasmaWaveUpFacingRightShortSpread3"/> does.</summary>
    PlasmaWaveUpFacingRightShortSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveUpFacingRight"/> showing the Plasma Wave picture, up short, spread 2, as <see cref="PlasmaWaveUpFacingRightShortSpread2"/> does.</summary>
    PlasmaWaveUpFacingRightShortSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveUpFacingRight"/> showing the Plasma Wave picture, up short, spread 1, as <see cref="PlasmaWaveUpFacingRightShortSpread1"/> does.</summary>
    PlasmaWaveUpFacingRightShortSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaIceWaveUpRight"/> showing the Plasma Startup picture, up right, core.</summary>
    PlasmaIceWaveUpRightCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveUpRight"/> showing the Plasma Wave picture, up right short, core.</summary>
    PlasmaWaveUpRightShortCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveUpRight"/> showing the Plasma Wave picture, up right short, spread 1.</summary>
    PlasmaWaveUpRightShortSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveUpRight"/> showing the Plasma Wave picture, up right short, spread 2.</summary>
    PlasmaWaveUpRightShortSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveUpRight"/> showing the Plasma Wave picture, up right short, spread 3.</summary>
    PlasmaWaveUpRightShortSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveUpRight"/> showing the Plasma Wave picture, up right short, spread 4.</summary>
    PlasmaWaveUpRightShortSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveUpRight"/> showing the Plasma Wave picture, up right short, spread 3, as <see cref="PlasmaWaveUpRightShortSpread3"/> does.</summary>
    PlasmaWaveUpRightShortSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveUpRight"/> showing the Plasma Wave picture, up right short, spread 2, as <see cref="PlasmaWaveUpRightShortSpread2"/> does.</summary>
    PlasmaWaveUpRightShortSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveUpRight"/> showing the Plasma Wave picture, up right short, spread 1, as <see cref="PlasmaWaveUpRightShortSpread1"/> does.</summary>
    PlasmaWaveUpRightShortSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveRight"/> showing the Plasma Startup picture, right, core.</summary>
    PlasmaWaveRightCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveRightLoop"/> showing the Plasma Wave picture, right short, core.</summary>
    PlasmaWaveRightShortCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveRightLoop"/> showing the Plasma Wave picture, right short, spread 1.</summary>
    PlasmaWaveRightShortSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveRightLoop"/> showing the Plasma Wave picture, right short, spread 2.</summary>
    PlasmaWaveRightShortSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveRightLoop"/> showing the Plasma Wave picture, right short, spread 3.</summary>
    PlasmaWaveRightShortSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveRightLoop"/> showing the Plasma Wave picture, right short, spread 4.</summary>
    PlasmaWaveRightShortSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveRightLoop"/> showing the Plasma Wave picture, right short, spread 3, as <see cref="PlasmaWaveRightShortSpread3"/> does.</summary>
    PlasmaWaveRightShortSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveRightLoop"/> showing the Plasma Wave picture, right short, spread 2, as <see cref="PlasmaWaveRightShortSpread2"/> does.</summary>
    PlasmaWaveRightShortSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveRightLoop"/> showing the Plasma Wave picture, right short, spread 1, as <see cref="PlasmaWaveRightShortSpread1"/> does.</summary>
    PlasmaWaveRightShortSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaIceWaveDownRight"/> showing the Plasma Startup picture, down right, core.</summary>
    PlasmaIceWaveDownRightCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveDownRight"/> showing the Plasma Wave picture, down right short, core.</summary>
    PlasmaWaveDownRightShortCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveDownRight"/> showing the Plasma Wave picture, down right short, spread 1.</summary>
    PlasmaWaveDownRightShortSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveDownRight"/> showing the Plasma Wave picture, down right short, spread 2.</summary>
    PlasmaWaveDownRightShortSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveDownRight"/> showing the Plasma Wave picture, down right short, spread 3.</summary>
    PlasmaWaveDownRightShortSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveDownRight"/> showing the Plasma Wave picture, down right short, spread 4.</summary>
    PlasmaWaveDownRightShortSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveDownRight"/> showing the Plasma Wave picture, down right short, spread 3, as <see cref="PlasmaWaveDownRightShortSpread3"/> does.</summary>
    PlasmaWaveDownRightShortSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveDownRight"/> showing the Plasma Wave picture, down right short, spread 2, as <see cref="PlasmaWaveDownRightShortSpread2"/> does.</summary>
    PlasmaWaveDownRightShortSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveDownRight"/> showing the Plasma Wave picture, down right short, spread 1, as <see cref="PlasmaWaveDownRightShortSpread1"/> does.</summary>
    PlasmaWaveDownRightShortSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPowerUpFacingRight"/> showing the Charged Power picture, up.</summary>
    ChargedPowerUpFacingRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPowerUpFacingRight"/> showing the Charged Power picture, up alternate.</summary>
    ChargedPowerUpFacingRightAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPowerUpRight"/> showing the Charged Power picture, up right.</summary>
    ChargedPowerUpRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPowerUpRight"/> showing the Charged Power picture, up right alternate.</summary>
    ChargedPowerUpRightAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPowerRight"/> showing the Charged Power picture, right.</summary>
    ChargedPowerRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPowerRight"/> showing the Charged Power picture, right alternate.</summary>
    ChargedPowerRightAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPowerDownRight"/> showing the Charged Power picture, down right.</summary>
    ChargedPowerDownRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPowerDownRight"/> showing the Charged Power picture, down right alternate.</summary>
    ChargedPowerDownRightAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPowerDownFacingRight"/> showing the Charged Power picture, down.</summary>
    ChargedPowerDownFacingRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPowerDownFacingRight"/> showing the Charged Power picture, down alternate.</summary>
    ChargedPowerDownFacingRightAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPowerDownLeft"/> showing the Charged Power picture, down left.</summary>
    ChargedPowerDownLeft,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPowerDownLeft"/> showing the Charged Power picture, down left alternate.</summary>
    ChargedPowerDownLeftAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPowerLeft"/> showing the Charged Power picture, left.</summary>
    ChargedPowerLeft,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPowerLeft"/> showing the Charged Power picture, left alternate.</summary>
    ChargedPowerLeftAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPowerUpLeft"/> showing the Charged Power picture, up left.</summary>
    ChargedPowerUpLeft,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPowerUpLeft"/> showing the Charged Power picture, up left alternate.</summary>
    ChargedPowerUpLeftAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveUpFacingRight"/> showing nothing.</summary>
    ChargedWaveUpFacingRightHidden,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownFacingRight"/> showing the Charged Wave picture, core.</summary>
    ChargedWaveDownFacingRightCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownFacingRight"/> showing the Charged Wave picture, core alternate.</summary>
    ChargedWaveDownFacingRightCoreAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownFacingRight"/> showing the Charged Wave picture, down, spread 1.</summary>
    ChargedWaveDownFacingRightSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownFacingRight"/> showing the Charged Wave picture, down, spread 1 alternate.</summary>
    ChargedWaveDownFacingRightSpread1Alternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownFacingRight"/> showing the Charged Wave picture, down, spread 2.</summary>
    ChargedWaveDownFacingRightSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownFacingRight"/> showing the Charged Wave picture, down, spread 2 alternate.</summary>
    ChargedWaveDownFacingRightSpread2Alternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownFacingRight"/> showing the Charged Wave picture, down, spread 3.</summary>
    ChargedWaveDownFacingRightSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownFacingRight"/> showing the Charged Wave picture, down, spread 3 alternate.</summary>
    ChargedWaveDownFacingRightSpread3Alternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownFacingRight"/> showing the Charged Wave picture, down, spread 4.</summary>
    ChargedWaveDownFacingRightSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownFacingRight"/> showing the Charged Wave picture, down, spread 4 alternate.</summary>
    ChargedWaveDownFacingRightSpread4Alternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownFacingRight"/> showing the Charged Wave picture, down, spread 3, as <see cref="ChargedWaveDownFacingRightSpread3"/> does.</summary>
    ChargedWaveDownFacingRightSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownFacingRight"/> showing the Charged Wave picture, down, spread 3 alternate, as <see cref="ChargedWaveDownFacingRightSpread3Alternate"/> does.</summary>
    ChargedWaveDownFacingRightSpread3AlternateTwin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownFacingRight"/> showing the Charged Wave picture, down, spread 2, as <see cref="ChargedWaveDownFacingRightSpread2"/> does.</summary>
    ChargedWaveDownFacingRightSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownFacingRight"/> showing the Charged Wave picture, down, spread 2 alternate, as <see cref="ChargedWaveDownFacingRightSpread2Alternate"/> does.</summary>
    ChargedWaveDownFacingRightSpread2AlternateTwin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownFacingRight"/> showing the Charged Wave picture, down, spread 1 alternate, as <see cref="ChargedWaveDownFacingRightSpread1Alternate"/> does.</summary>
    ChargedWaveDownFacingRightSpread1AlternateTwin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownFacingRight"/> showing the Charged Wave picture, down, spread 1, as <see cref="ChargedWaveDownFacingRightSpread1"/> does.</summary>
    ChargedWaveDownFacingRightSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveUpRight"/> showing the Charged Wave picture, core.</summary>
    ChargedWaveUpRightCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveUpRight"/> showing the Charged Wave picture, core alternate.</summary>
    ChargedWaveUpRightCoreAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveUpRight"/> showing the Charged Wave picture, up right, spread 1.</summary>
    ChargedWaveUpRightSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveUpRight"/> showing the Charged Wave picture, up right, spread 1 alternate.</summary>
    ChargedWaveUpRightSpread1Alternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveUpRight"/> showing the Charged Wave picture, up right, spread 2.</summary>
    ChargedWaveUpRightSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveUpRight"/> showing the Charged Wave picture, up right, spread 2 alternate.</summary>
    ChargedWaveUpRightSpread2Alternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveUpRight"/> showing the Charged Wave picture, up right, spread 3.</summary>
    ChargedWaveUpRightSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveUpRight"/> showing the Charged Wave picture, up right, spread 3 alternate.</summary>
    ChargedWaveUpRightSpread3Alternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveUpRight"/> showing the Charged Wave picture, up right, spread 4.</summary>
    ChargedWaveUpRightSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveUpRight"/> showing the Charged Wave picture, up right, spread 4 alternate.</summary>
    ChargedWaveUpRightSpread4Alternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveUpRight"/> showing the Charged Wave picture, up right, spread 3, as <see cref="ChargedWaveUpRightSpread3"/> does.</summary>
    ChargedWaveUpRightSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveUpRight"/> showing the Charged Wave picture, up right, spread 3 alternate, as <see cref="ChargedWaveUpRightSpread3Alternate"/> does.</summary>
    ChargedWaveUpRightSpread3AlternateTwin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveUpRight"/> showing the Charged Wave picture, up right, spread 2, as <see cref="ChargedWaveUpRightSpread2"/> does.</summary>
    ChargedWaveUpRightSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveUpRight"/> showing the Charged Wave picture, up right, spread 2 alternate, as <see cref="ChargedWaveUpRightSpread2Alternate"/> does.</summary>
    ChargedWaveUpRightSpread2AlternateTwin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveUpRight"/> showing the Charged Wave picture, up right, spread 1, as <see cref="ChargedWaveUpRightSpread1"/> does.</summary>
    ChargedWaveUpRightSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveUpRight"/> showing the Charged Wave picture, up right, spread 1 alternate, as <see cref="ChargedWaveUpRightSpread1Alternate"/> does.</summary>
    ChargedWaveUpRightSpread1AlternateTwin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRight"/> showing the Charged Wave picture, core.</summary>
    ChargedWaveRightCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRight"/> showing the Charged Wave picture, core alternate.</summary>
    ChargedWaveRightCoreAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRight"/> showing the Charged Wave picture, right, spread 1.</summary>
    ChargedWaveRightSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRight"/> showing the Charged Wave picture, right, spread 1 alternate.</summary>
    ChargedWaveRightSpread1Alternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRight"/> showing the Charged Wave picture, right, spread 2.</summary>
    ChargedWaveRightSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRight"/> showing the Charged Wave picture, right, spread 2 alternate.</summary>
    ChargedWaveRightSpread2Alternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRight"/> showing the Charged Wave picture, right, spread 3.</summary>
    ChargedWaveRightSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRight"/> showing the Charged Wave picture, right, spread 3 alternate.</summary>
    ChargedWaveRightSpread3Alternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRight"/> showing the Charged Wave picture, right, spread 4.</summary>
    ChargedWaveRightSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRight"/> showing the Charged Wave picture, right, spread 4 alternate.</summary>
    ChargedWaveRightSpread4Alternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRight"/> showing the Charged Wave picture, right, spread 3, as <see cref="ChargedWaveRightSpread3"/> does.</summary>
    ChargedWaveRightSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRight"/> showing the Charged Wave picture, right, spread 3 alternate, as <see cref="ChargedWaveRightSpread3Alternate"/> does.</summary>
    ChargedWaveRightSpread3AlternateTwin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRight"/> showing the Charged Wave picture, right, spread 2, as <see cref="ChargedWaveRightSpread2"/> does.</summary>
    ChargedWaveRightSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRight"/> showing the Charged Wave picture, right, spread 2 alternate, as <see cref="ChargedWaveRightSpread2Alternate"/> does.</summary>
    ChargedWaveRightSpread2AlternateTwin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRight"/> showing the Charged Wave picture, right, spread 1 alternate, as <see cref="ChargedWaveRightSpread1Alternate"/> does.</summary>
    ChargedWaveRightSpread1AlternateTwin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRight"/> showing the Charged Wave picture, right, spread 1, as <see cref="ChargedWaveRightSpread1"/> does.</summary>
    ChargedWaveRightSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownRight"/> showing the Charged Wave picture, core.</summary>
    ChargedWaveDownRightCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownRight"/> showing the Charged Wave picture, core alternate.</summary>
    ChargedWaveDownRightCoreAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownRight"/> showing the Charged Wave picture, down right, spread 1.</summary>
    ChargedWaveDownRightSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownRight"/> showing the Charged Wave picture, down right, spread 1 alternate.</summary>
    ChargedWaveDownRightSpread1Alternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownRight"/> showing the Charged Wave picture, down right, spread 2.</summary>
    ChargedWaveDownRightSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownRight"/> showing the Charged Wave picture, down right, spread 2 alternate.</summary>
    ChargedWaveDownRightSpread2Alternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownRight"/> showing the Charged Wave picture, down right, spread 3.</summary>
    ChargedWaveDownRightSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownRight"/> showing the Charged Wave picture, down right, spread 3 alternate.</summary>
    ChargedWaveDownRightSpread3Alternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownRight"/> showing the Charged Wave picture, down right, spread 4.</summary>
    ChargedWaveDownRightSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownRight"/> showing the Charged Wave picture, down right, spread 4 alternate.</summary>
    ChargedWaveDownRightSpread4Alternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownRight"/> showing the Charged Wave picture, down right, spread 3, as <see cref="ChargedWaveDownRightSpread3"/> does.</summary>
    ChargedWaveDownRightSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownRight"/> showing the Charged Wave picture, down right, spread 3 alternate, as <see cref="ChargedWaveDownRightSpread3Alternate"/> does.</summary>
    ChargedWaveDownRightSpread3AlternateTwin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownRight"/> showing the Charged Wave picture, down right, spread 2, as <see cref="ChargedWaveDownRightSpread2"/> does.</summary>
    ChargedWaveDownRightSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownRight"/> showing the Charged Wave picture, down right, spread 2 alternate, as <see cref="ChargedWaveDownRightSpread2Alternate"/> does.</summary>
    ChargedWaveDownRightSpread2AlternateTwin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownRight"/> showing the Charged Wave picture, down right, spread 1, as <see cref="ChargedWaveDownRightSpread1"/> does.</summary>
    ChargedWaveDownRightSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownRight"/> showing the Charged Wave picture, down right, spread 1 alternate, as <see cref="ChargedWaveDownRightSpread1Alternate"/> does.</summary>
    ChargedWaveDownRightSpread1AlternateTwin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceUpFacingRight"/> showing the Charged Ice picture, phase 1.</summary>
    ChargedIceUpFacingRightPhase1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceUpFacingRight"/> showing the Charged Ice picture, phase 2.</summary>
    ChargedIceUpFacingRightPhase2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceUpFacingRight"/> showing the Charged Ice picture, phase 3.</summary>
    ChargedIceUpFacingRightPhase3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceUpFacingRight"/> showing the Charged Ice picture, phase 4.</summary>
    ChargedIceUpFacingRightPhase4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveUpFacingRight"/> showing nothing.</summary>
    ChargedIceWaveUpFacingRightHidden,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownFacingRight"/> showing the Charged Ice Wave picture, core.</summary>
    ChargedIceWaveDownFacingRightCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownFacingRight"/> showing the Charged Ice Wave picture, core alternate.</summary>
    ChargedIceWaveDownFacingRightCoreAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownFacingRight"/> showing the Charged Ice Wave picture, down, spread 1.</summary>
    ChargedIceWaveDownFacingRightSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownFacingRight"/> showing the Charged Ice Wave picture, down, spread 1 alternate.</summary>
    ChargedIceWaveDownFacingRightSpread1Alternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownFacingRight"/> showing the Charged Ice Wave picture, down, spread 2.</summary>
    ChargedIceWaveDownFacingRightSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownFacingRight"/> showing the Charged Ice Wave picture, down, spread 2 alternate.</summary>
    ChargedIceWaveDownFacingRightSpread2Alternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownFacingRight"/> showing the Charged Ice Wave picture, down, spread 3.</summary>
    ChargedIceWaveDownFacingRightSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownFacingRight"/> showing the Charged Ice Wave picture, down, spread 3 alternate.</summary>
    ChargedIceWaveDownFacingRightSpread3Alternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownFacingRight"/> showing the Charged Ice Wave picture, down, spread 4.</summary>
    ChargedIceWaveDownFacingRightSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownFacingRight"/> showing the Charged Ice Wave picture, down, spread 4 alternate.</summary>
    ChargedIceWaveDownFacingRightSpread4Alternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownFacingRight"/> showing the Charged Ice Wave picture, down, spread 3, as <see cref="ChargedIceWaveDownFacingRightSpread3"/> does.</summary>
    ChargedIceWaveDownFacingRightSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownFacingRight"/> showing the Charged Ice Wave picture, down, spread 3 alternate, as <see cref="ChargedIceWaveDownFacingRightSpread3Alternate"/> does.</summary>
    ChargedIceWaveDownFacingRightSpread3AlternateTwin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownFacingRight"/> showing the Charged Ice Wave picture, down, spread 2, as <see cref="ChargedIceWaveDownFacingRightSpread2"/> does.</summary>
    ChargedIceWaveDownFacingRightSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownFacingRight"/> showing the Charged Ice Wave picture, down, spread 2 alternate, as <see cref="ChargedIceWaveDownFacingRightSpread2Alternate"/> does.</summary>
    ChargedIceWaveDownFacingRightSpread2AlternateTwin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownFacingRight"/> showing the Charged Ice Wave picture, down, spread 1 alternate, as <see cref="ChargedIceWaveDownFacingRightSpread1Alternate"/> does.</summary>
    ChargedIceWaveDownFacingRightSpread1AlternateTwin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownFacingRight"/> showing the Charged Ice Wave picture, down, spread 1, as <see cref="ChargedIceWaveDownFacingRightSpread1"/> does.</summary>
    ChargedIceWaveDownFacingRightSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveUpRight"/> showing the Charged Ice Wave picture, core.</summary>
    ChargedIceWaveUpRightCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveUpRight"/> showing the Charged Ice Wave picture, core alternate.</summary>
    ChargedIceWaveUpRightCoreAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveUpRight"/> showing the Charged Ice Wave picture, up right, spread 1.</summary>
    ChargedIceWaveUpRightSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveUpRight"/> showing the Charged Ice Wave picture, up right, spread 1 alternate.</summary>
    ChargedIceWaveUpRightSpread1Alternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveUpRight"/> showing the Charged Ice Wave picture, up right, spread 2.</summary>
    ChargedIceWaveUpRightSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveUpRight"/> showing the Charged Ice Wave picture, up right, spread 2 alternate.</summary>
    ChargedIceWaveUpRightSpread2Alternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveUpRight"/> showing the Charged Ice Wave picture, up right, spread 3.</summary>
    ChargedIceWaveUpRightSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveUpRight"/> showing the Charged Ice Wave picture, up right, spread 3 alternate.</summary>
    ChargedIceWaveUpRightSpread3Alternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveUpRight"/> showing the Charged Ice Wave picture, up right, spread 4.</summary>
    ChargedIceWaveUpRightSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveUpRight"/> showing the Charged Ice Wave picture, up right, spread 4 alternate.</summary>
    ChargedIceWaveUpRightSpread4Alternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveUpRight"/> showing the Charged Ice Wave picture, up right, spread 3, as <see cref="ChargedIceWaveUpRightSpread3"/> does.</summary>
    ChargedIceWaveUpRightSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveUpRight"/> showing the Charged Ice Wave picture, up right, spread 3 alternate, as <see cref="ChargedIceWaveUpRightSpread3Alternate"/> does.</summary>
    ChargedIceWaveUpRightSpread3AlternateTwin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveUpRight"/> showing the Charged Ice Wave picture, up right, spread 2, as <see cref="ChargedIceWaveUpRightSpread2"/> does.</summary>
    ChargedIceWaveUpRightSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveUpRight"/> showing the Charged Ice Wave picture, up right, spread 2 alternate, as <see cref="ChargedIceWaveUpRightSpread2Alternate"/> does.</summary>
    ChargedIceWaveUpRightSpread2AlternateTwin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveUpRight"/> showing the Charged Ice Wave picture, up right, spread 1, as <see cref="ChargedIceWaveUpRightSpread1"/> does.</summary>
    ChargedIceWaveUpRightSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveUpRight"/> showing the Charged Ice Wave picture, up right, spread 1 alternate, as <see cref="ChargedIceWaveUpRightSpread1Alternate"/> does.</summary>
    ChargedIceWaveUpRightSpread1AlternateTwin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRight"/> showing the Charged Ice Wave picture, core.</summary>
    ChargedIceWaveRightCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRight"/> showing the Charged Ice Wave picture, core alternate.</summary>
    ChargedIceWaveRightCoreAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRight"/> showing the Charged Ice Wave picture, right, spread 1.</summary>
    ChargedIceWaveRightSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRight"/> showing the Charged Ice Wave picture, right, spread 1 alternate.</summary>
    ChargedIceWaveRightSpread1Alternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRight"/> showing the Charged Ice Wave picture, right, spread 2.</summary>
    ChargedIceWaveRightSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRight"/> showing the Charged Ice Wave picture, right, spread 2 alternate.</summary>
    ChargedIceWaveRightSpread2Alternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRight"/> showing the Charged Ice Wave picture, right, spread 3.</summary>
    ChargedIceWaveRightSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRight"/> showing the Charged Ice Wave picture, right, spread 3 alternate.</summary>
    ChargedIceWaveRightSpread3Alternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRight"/> showing the Charged Ice Wave picture, right, spread 4.</summary>
    ChargedIceWaveRightSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRight"/> showing the Charged Ice Wave picture, right, spread 4 alternate.</summary>
    ChargedIceWaveRightSpread4Alternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRight"/> showing the Charged Ice Wave picture, right, spread 3, as <see cref="ChargedIceWaveRightSpread3"/> does.</summary>
    ChargedIceWaveRightSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRight"/> showing the Charged Ice Wave picture, right, spread 3 alternate, as <see cref="ChargedIceWaveRightSpread3Alternate"/> does.</summary>
    ChargedIceWaveRightSpread3AlternateTwin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRight"/> showing the Charged Ice Wave picture, right, spread 2, as <see cref="ChargedIceWaveRightSpread2"/> does.</summary>
    ChargedIceWaveRightSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRight"/> showing the Charged Ice Wave picture, right, spread 2 alternate, as <see cref="ChargedIceWaveRightSpread2Alternate"/> does.</summary>
    ChargedIceWaveRightSpread2AlternateTwin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRight"/> showing the Charged Ice Wave picture, right, spread 1 alternate, as <see cref="ChargedIceWaveRightSpread1Alternate"/> does.</summary>
    ChargedIceWaveRightSpread1AlternateTwin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRight"/> showing the Charged Ice Wave picture, right, spread 1, as <see cref="ChargedIceWaveRightSpread1"/> does.</summary>
    ChargedIceWaveRightSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownRight"/> showing the Charged Ice Wave picture, core.</summary>
    ChargedIceWaveDownRightCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownRight"/> showing the Charged Ice Wave picture, core alternate.</summary>
    ChargedIceWaveDownRightCoreAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownRight"/> showing the Charged Ice Wave picture, down right, spread 1.</summary>
    ChargedIceWaveDownRightSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownRight"/> showing the Charged Ice Wave picture, down right, spread 1 alternate.</summary>
    ChargedIceWaveDownRightSpread1Alternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownRight"/> showing the Charged Ice Wave picture, down right, spread 2.</summary>
    ChargedIceWaveDownRightSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownRight"/> showing the Charged Ice Wave picture, down right, spread 2 alternate.</summary>
    ChargedIceWaveDownRightSpread2Alternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownRight"/> showing the Charged Ice Wave picture, down right, spread 3.</summary>
    ChargedIceWaveDownRightSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownRight"/> showing the Charged Ice Wave picture, down right, spread 3 alternate.</summary>
    ChargedIceWaveDownRightSpread3Alternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownRight"/> showing the Charged Ice Wave picture, down right, spread 4.</summary>
    ChargedIceWaveDownRightSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownRight"/> showing the Charged Ice Wave picture, down right, spread 4 alternate.</summary>
    ChargedIceWaveDownRightSpread4Alternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownRight"/> showing the Charged Ice Wave picture, down right, spread 3, as <see cref="ChargedIceWaveDownRightSpread3"/> does.</summary>
    ChargedIceWaveDownRightSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownRight"/> showing the Charged Ice Wave picture, down right, spread 3 alternate, as <see cref="ChargedIceWaveDownRightSpread3Alternate"/> does.</summary>
    ChargedIceWaveDownRightSpread3AlternateTwin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownRight"/> showing the Charged Ice Wave picture, down right, spread 2, as <see cref="ChargedIceWaveDownRightSpread2"/> does.</summary>
    ChargedIceWaveDownRightSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownRight"/> showing the Charged Ice Wave picture, down right, spread 2 alternate, as <see cref="ChargedIceWaveDownRightSpread2Alternate"/> does.</summary>
    ChargedIceWaveDownRightSpread2AlternateTwin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownRight"/> showing the Charged Ice Wave picture, down right, spread 1, as <see cref="ChargedIceWaveDownRightSpread1"/> does.</summary>
    ChargedIceWaveDownRightSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownRight"/> showing the Charged Ice Wave picture, down right, spread 1 alternate, as <see cref="ChargedIceWaveDownRightSpread1Alternate"/> does.</summary>
    ChargedIceWaveDownRightSpread1AlternateTwin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerUpFacingRight"/> showing the Spazer Startup picture, up, stage 1.</summary>
    ChargedSpazerUpFacingRightStage1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerUpFacingRight"/> showing the Spazer Startup picture, vertical alternate, stage 1.</summary>
    ChargedSpazerUpFacingRightVerticalAlternateStage1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerUpFacingRight"/> showing the Spazer Startup picture, up, stage 2.</summary>
    ChargedSpazerUpFacingRightStage2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerUpFacingRight"/> showing the Spazer Startup picture, vertical alternate, stage 2.</summary>
    ChargedSpazerUpFacingRightVerticalAlternateStage2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerUpFacingRight"/> showing the Charged Spazer picture, up, seed.</summary>
    ChargedSpazerUpFacingRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerUpFacingRight"/> showing the Charged Spazer picture, vertical alternate, seed.</summary>
    ChargedSpazerUpFacingRightVerticalAlternateSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerUpFacingRight"/> showing the Charged Spazer picture, up, spread 2.</summary>
    ChargedSpazerUpFacingRightSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerUpFacingRight"/> showing the Charged Spazer picture, vertical alternate, spread 2.</summary>
    ChargedSpazerUpFacingRightVerticalAlternateSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerUpFacingRightLoop"/> showing the Charged Spazer picture, up, spread 5.</summary>
    ChargedSpazerUpFacingRightSpread5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerUpFacingRightLoop"/> showing the Charged Spazer picture, vertical alternate, spread 5.</summary>
    ChargedSpazerUpFacingRightVerticalAlternateSpread5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerUpRight"/> showing the Spazer Startup picture, up right, stage 1.</summary>
    ChargedSpazerUpRightStage1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerUpRight"/> showing the Spazer Startup picture, up right alternate, stage 1.</summary>
    ChargedSpazerUpRightAlternateStage1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerUpRight"/> showing the Spazer Startup picture, up right, stage 2.</summary>
    ChargedSpazerUpRightStage2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerUpRight"/> showing the Spazer Startup picture, up right alternate, stage 2.</summary>
    ChargedSpazerUpRightAlternateStage2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerUpRight"/> showing the Charged Spazer picture, up right, seed.</summary>
    ChargedSpazerUpRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerUpRight"/> showing the Charged Spazer picture, up right alternate, seed.</summary>
    ChargedSpazerUpRightAlternateSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerUpRight"/> showing the Charged Spazer picture, up right, spread 2.</summary>
    ChargedSpazerUpRightSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerUpRight"/> showing the Charged Spazer picture, up right alternate, spread 2.</summary>
    ChargedSpazerUpRightAlternateSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerUpRightLoop"/> showing the Charged Spazer picture, up right, spread 5.</summary>
    ChargedSpazerUpRightSpread5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerUpRightLoop"/> showing the Charged Spazer picture, up right alternate, spread 5.</summary>
    ChargedSpazerUpRightAlternateSpread5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerRight"/> showing the Spazer Startup picture, right, stage 1.</summary>
    ChargedSpazerRightStage1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerRight"/> showing the Spazer Startup picture, horizontal alternate, stage 1.</summary>
    ChargedSpazerRightHorizontalAlternateStage1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerRight"/> showing the Spazer Startup picture, right, stage 2.</summary>
    ChargedSpazerRightStage2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerRight"/> showing the Spazer Startup picture, horizontal alternate, stage 2.</summary>
    ChargedSpazerRightHorizontalAlternateStage2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerRight"/> showing the Charged Spazer picture, left, seed.</summary>
    ChargedSpazerRightLeftSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerRight"/> showing the Charged Spazer picture, horizontal alternate, seed.</summary>
    ChargedSpazerRightHorizontalAlternateSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerRight"/> showing the Charged Spazer picture, left, spread 2.</summary>
    ChargedSpazerRightLeftSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerRight"/> showing the Charged Spazer picture, horizontal alternate, spread 2.</summary>
    ChargedSpazerRightHorizontalAlternateSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerRightLoop"/> showing the Charged Spazer picture, left, spread 5.</summary>
    ChargedSpazerRightLeftSpread5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerRightLoop"/> showing the Charged Spazer picture, horizontal alternate, spread 5.</summary>
    ChargedSpazerRightHorizontalAlternateSpread5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerDownRight"/> showing the Spazer Startup picture, down right, stage 1.</summary>
    ChargedSpazerDownRightStage1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerDownRight"/> showing the Spazer Startup picture, down right alternate, stage 1.</summary>
    ChargedSpazerDownRightAlternateStage1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerDownRight"/> showing the Spazer Startup picture, down right, stage 2.</summary>
    ChargedSpazerDownRightStage2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerDownRight"/> showing the Spazer Startup picture, down right alternate, stage 2.</summary>
    ChargedSpazerDownRightAlternateStage2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerDownRight"/> showing the Charged Spazer picture, down right, seed.</summary>
    ChargedSpazerDownRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerDownRight"/> showing the Charged Spazer picture, down right alternate, seed.</summary>
    ChargedSpazerDownRightAlternateSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerDownRight"/> showing the Charged Spazer picture, down right, spread 2.</summary>
    ChargedSpazerDownRightSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerDownRight"/> showing the Charged Spazer picture, down right alternate, spread 2.</summary>
    ChargedSpazerDownRightAlternateSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerDownRightLoop"/> showing the Charged Spazer picture, down right, spread 5.</summary>
    ChargedSpazerDownRightSpread5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerDownRightLoop"/> showing the Charged Spazer picture, down right alternate, spread 5.</summary>
    ChargedSpazerDownRightAlternateSpread5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRight"/> showing the Spazer Startup picture, up, stage 1.</summary>
    ChargedSpazerWaveUpFacingRightStage1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRight"/> showing the Spazer Startup picture, vertical alternate, stage 1.</summary>
    ChargedSpazerWaveUpFacingRightVerticalAlternateStage1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRight"/> showing the Spazer Startup picture, up, stage 2.</summary>
    ChargedSpazerWaveUpFacingRightStage2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRight"/> showing the Spazer Startup picture, vertical alternate, stage 2.</summary>
    ChargedSpazerWaveUpFacingRightVerticalAlternateStage2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRightLoop"/> showing the Charged Spazer picture, up, seed.</summary>
    ChargedSpazerWaveUpFacingRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRightLoop"/> showing the Charged Spazer picture, vertical alternate, seed.</summary>
    ChargedSpazerWaveUpFacingRightVerticalAlternateSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRightLoop"/> showing the Charged Spazer picture, up, spread 1.</summary>
    ChargedSpazerWaveUpFacingRightSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRightLoop"/> showing the Charged Spazer picture, vertical alternate, spread 1.</summary>
    ChargedSpazerWaveUpFacingRightVerticalAlternateSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRightLoop"/> showing the Charged Spazer picture, up, spread 2.</summary>
    ChargedSpazerWaveUpFacingRightSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRightLoop"/> showing the Charged Spazer picture, vertical alternate, spread 2.</summary>
    ChargedSpazerWaveUpFacingRightVerticalAlternateSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRightLoop"/> showing the Charged Spazer picture, up, spread 3.</summary>
    ChargedSpazerWaveUpFacingRightSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRightLoop"/> showing the Charged Spazer picture, vertical alternate, spread 3.</summary>
    ChargedSpazerWaveUpFacingRightVerticalAlternateSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRightLoop"/> showing the Charged Spazer picture, up, spread 4.</summary>
    ChargedSpazerWaveUpFacingRightSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRightLoop"/> showing the Charged Spazer picture, vertical alternate, spread 4.</summary>
    ChargedSpazerWaveUpFacingRightVerticalAlternateSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRightLoop"/> showing the Charged Spazer picture, up, spread 5.</summary>
    ChargedSpazerWaveUpFacingRightSpread5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRightLoop"/> showing the Charged Spazer picture, vertical alternate, spread 5.</summary>
    ChargedSpazerWaveUpFacingRightVerticalAlternateSpread5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRightLoop"/> showing the Charged Spazer picture, up, spread 4, as <see cref="ChargedSpazerWaveUpFacingRightSpread4"/> does.</summary>
    ChargedSpazerWaveUpFacingRightSpread4Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRightLoop"/> showing the Charged Spazer picture, vertical alternate, spread 4, as <see cref="ChargedSpazerWaveUpFacingRightVerticalAlternateSpread4"/> does.</summary>
    ChargedSpazerWaveUpFacingRightVerticalAlternateSpread4Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRightLoop"/> showing the Charged Spazer picture, up, spread 3, as <see cref="ChargedSpazerWaveUpFacingRightSpread3"/> does.</summary>
    ChargedSpazerWaveUpFacingRightSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRightLoop"/> showing the Charged Spazer picture, vertical alternate, spread 3, as <see cref="ChargedSpazerWaveUpFacingRightVerticalAlternateSpread3"/> does.</summary>
    ChargedSpazerWaveUpFacingRightVerticalAlternateSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRightLoop"/> showing the Charged Spazer picture, up, spread 2, as <see cref="ChargedSpazerWaveUpFacingRightSpread2"/> does.</summary>
    ChargedSpazerWaveUpFacingRightSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRightLoop"/> showing the Charged Spazer picture, vertical alternate, spread 2, as <see cref="ChargedSpazerWaveUpFacingRightVerticalAlternateSpread2"/> does.</summary>
    ChargedSpazerWaveUpFacingRightVerticalAlternateSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRightLoop"/> showing the Charged Spazer picture, up, spread 1, as <see cref="ChargedSpazerWaveUpFacingRightSpread1"/> does.</summary>
    ChargedSpazerWaveUpFacingRightSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRightLoop"/> showing the Charged Spazer picture, vertical alternate, spread 1, as <see cref="ChargedSpazerWaveUpFacingRightVerticalAlternateSpread1"/> does.</summary>
    ChargedSpazerWaveUpFacingRightVerticalAlternateSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRight"/> showing the Spazer Startup picture, up right, stage 1.</summary>
    ChargedSpazerWaveUpRightStage1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRight"/> showing the Spazer Startup picture, up right alternate, stage 1.</summary>
    ChargedSpazerWaveUpRightAlternateStage1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRight"/> showing the Spazer Startup picture, up right, stage 2.</summary>
    ChargedSpazerWaveUpRightStage2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRight"/> showing the Spazer Startup picture, up right alternate, stage 2.</summary>
    ChargedSpazerWaveUpRightAlternateStage2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightLoop"/> showing the Charged Spazer picture, up right, seed.</summary>
    ChargedSpazerWaveUpRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightLoop"/> showing the Charged Spazer picture, up right alternate, seed.</summary>
    ChargedSpazerWaveUpRightAlternateSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightLoop"/> showing the Charged Spazer picture, up right, spread 1.</summary>
    ChargedSpazerWaveUpRightSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightLoop"/> showing the Charged Spazer picture, up right alternate, spread 1.</summary>
    ChargedSpazerWaveUpRightAlternateSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightLoop"/> showing the Charged Spazer picture, up right, spread 2.</summary>
    ChargedSpazerWaveUpRightSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightLoop"/> showing the Charged Spazer picture, up right alternate, spread 2.</summary>
    ChargedSpazerWaveUpRightAlternateSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightLoop"/> showing the Charged Spazer picture, up right, spread 3.</summary>
    ChargedSpazerWaveUpRightSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightLoop"/> showing the Charged Spazer picture, up right alternate, spread 3.</summary>
    ChargedSpazerWaveUpRightAlternateSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightLoop"/> showing the Charged Spazer picture, up right, spread 4.</summary>
    ChargedSpazerWaveUpRightSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightLoop"/> showing the Charged Spazer picture, up right alternate, spread 4.</summary>
    ChargedSpazerWaveUpRightAlternateSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightLoop"/> showing the Charged Spazer picture, up right, spread 5.</summary>
    ChargedSpazerWaveUpRightSpread5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightLoop"/> showing the Charged Spazer picture, up right alternate, spread 5.</summary>
    ChargedSpazerWaveUpRightAlternateSpread5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightLoop"/> showing the Charged Spazer picture, up right, spread 4, as <see cref="ChargedSpazerWaveUpRightSpread4"/> does.</summary>
    ChargedSpazerWaveUpRightSpread4Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightLoop"/> showing the Charged Spazer picture, up right alternate, spread 4, as <see cref="ChargedSpazerWaveUpRightAlternateSpread4"/> does.</summary>
    ChargedSpazerWaveUpRightAlternateSpread4Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightLoop"/> showing the Charged Spazer picture, up right, spread 3, as <see cref="ChargedSpazerWaveUpRightSpread3"/> does.</summary>
    ChargedSpazerWaveUpRightSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightLoop"/> showing the Charged Spazer picture, up right alternate, spread 3, as <see cref="ChargedSpazerWaveUpRightAlternateSpread3"/> does.</summary>
    ChargedSpazerWaveUpRightAlternateSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightLoop"/> showing the Charged Spazer picture, up right, spread 2, as <see cref="ChargedSpazerWaveUpRightSpread2"/> does.</summary>
    ChargedSpazerWaveUpRightSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightLoop"/> showing the Charged Spazer picture, up right alternate, spread 2, as <see cref="ChargedSpazerWaveUpRightAlternateSpread2"/> does.</summary>
    ChargedSpazerWaveUpRightAlternateSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightLoop"/> showing the Charged Spazer picture, up right, spread 1, as <see cref="ChargedSpazerWaveUpRightSpread1"/> does.</summary>
    ChargedSpazerWaveUpRightSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightLoop"/> showing the Charged Spazer picture, up right alternate, spread 1, as <see cref="ChargedSpazerWaveUpRightAlternateSpread1"/> does.</summary>
    ChargedSpazerWaveUpRightAlternateSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRight"/> showing the Spazer Startup picture, right, stage 1.</summary>
    ChargedSpazerWaveRightStage1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRight"/> showing the Spazer Startup picture, horizontal alternate, stage 1.</summary>
    ChargedSpazerWaveRightHorizontalAlternateStage1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRight"/> showing the Spazer Startup picture, right, stage 2.</summary>
    ChargedSpazerWaveRightStage2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRight"/> showing the Spazer Startup picture, horizontal alternate, stage 2.</summary>
    ChargedSpazerWaveRightHorizontalAlternateStage2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightLoop"/> showing the Charged Spazer picture, right, seed.</summary>
    ChargedSpazerWaveRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightLoop"/> showing the Charged Spazer picture, horizontal alternate, seed.</summary>
    ChargedSpazerWaveRightHorizontalAlternateSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightLoop"/> showing the Charged Spazer picture, right, spread 1.</summary>
    ChargedSpazerWaveRightSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightLoop"/> showing the Charged Spazer picture, horizontal alternate, spread 1.</summary>
    ChargedSpazerWaveRightHorizontalAlternateSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightLoop"/> showing the Charged Spazer picture, right, spread 2.</summary>
    ChargedSpazerWaveRightSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightLoop"/> showing the Charged Spazer picture, horizontal alternate, spread 2.</summary>
    ChargedSpazerWaveRightHorizontalAlternateSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightLoop"/> showing the Charged Spazer picture, right, spread 3.</summary>
    ChargedSpazerWaveRightSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightLoop"/> showing the Charged Spazer picture, horizontal alternate, spread 3.</summary>
    ChargedSpazerWaveRightHorizontalAlternateSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightLoop"/> showing the Charged Spazer picture, right, spread 4.</summary>
    ChargedSpazerWaveRightSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightLoop"/> showing the Charged Spazer picture, horizontal alternate, spread 4.</summary>
    ChargedSpazerWaveRightHorizontalAlternateSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightLoop"/> showing the Charged Spazer picture, right, spread 5.</summary>
    ChargedSpazerWaveRightSpread5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightLoop"/> showing the Charged Spazer picture, horizontal alternate, spread 5.</summary>
    ChargedSpazerWaveRightHorizontalAlternateSpread5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightLoop"/> showing the Charged Spazer picture, right, spread 4, as <see cref="ChargedSpazerWaveRightSpread4"/> does.</summary>
    ChargedSpazerWaveRightSpread4Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightLoop"/> showing the Charged Spazer picture, horizontal alternate, spread 4, as <see cref="ChargedSpazerWaveRightHorizontalAlternateSpread4"/> does.</summary>
    ChargedSpazerWaveRightHorizontalAlternateSpread4Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightLoop"/> showing the Charged Spazer picture, right, spread 3, as <see cref="ChargedSpazerWaveRightSpread3"/> does.</summary>
    ChargedSpazerWaveRightSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightLoop"/> showing the Charged Spazer picture, horizontal alternate, spread 3, as <see cref="ChargedSpazerWaveRightHorizontalAlternateSpread3"/> does.</summary>
    ChargedSpazerWaveRightHorizontalAlternateSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightLoop"/> showing the Charged Spazer picture, right, spread 2, as <see cref="ChargedSpazerWaveRightSpread2"/> does.</summary>
    ChargedSpazerWaveRightSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightLoop"/> showing the Charged Spazer picture, horizontal alternate, spread 2, as <see cref="ChargedSpazerWaveRightHorizontalAlternateSpread2"/> does.</summary>
    ChargedSpazerWaveRightHorizontalAlternateSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightLoop"/> showing the Charged Spazer picture, right, spread 1, as <see cref="ChargedSpazerWaveRightSpread1"/> does.</summary>
    ChargedSpazerWaveRightSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightLoop"/> showing the Charged Spazer picture, horizontal alternate, spread 1, as <see cref="ChargedSpazerWaveRightHorizontalAlternateSpread1"/> does.</summary>
    ChargedSpazerWaveRightHorizontalAlternateSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRight"/> showing the Spazer Startup picture, down right, stage 1.</summary>
    ChargedSpazerWaveDownRightStage1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRight"/> showing the Spazer Startup picture, down right alternate, stage 1.</summary>
    ChargedSpazerWaveDownRightAlternateStage1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRight"/> showing the Spazer Startup picture, down right, stage 2.</summary>
    ChargedSpazerWaveDownRightStage2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRight"/> showing the Spazer Startup picture, down right alternate, stage 2.</summary>
    ChargedSpazerWaveDownRightAlternateStage2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightLoop"/> showing the Charged Spazer picture, down right, seed.</summary>
    ChargedSpazerWaveDownRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightLoop"/> showing the Charged Spazer picture, down right alternate, seed.</summary>
    ChargedSpazerWaveDownRightAlternateSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightLoop"/> showing the Charged Spazer picture, down right, spread 1.</summary>
    ChargedSpazerWaveDownRightSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightLoop"/> showing the Charged Spazer picture, down right alternate, spread 1.</summary>
    ChargedSpazerWaveDownRightAlternateSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightLoop"/> showing the Charged Spazer picture, down right, spread 2.</summary>
    ChargedSpazerWaveDownRightSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightLoop"/> showing the Charged Spazer picture, down right alternate, spread 2.</summary>
    ChargedSpazerWaveDownRightAlternateSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightLoop"/> showing the Charged Spazer picture, down right, spread 3.</summary>
    ChargedSpazerWaveDownRightSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightLoop"/> showing the Charged Spazer picture, down right alternate, spread 3.</summary>
    ChargedSpazerWaveDownRightAlternateSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightLoop"/> showing the Charged Spazer picture, down right, spread 4.</summary>
    ChargedSpazerWaveDownRightSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightLoop"/> showing the Charged Spazer picture, down right alternate, spread 4.</summary>
    ChargedSpazerWaveDownRightAlternateSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightLoop"/> showing the Charged Spazer picture, down right, spread 5.</summary>
    ChargedSpazerWaveDownRightSpread5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightLoop"/> showing the Charged Spazer picture, down right alternate, spread 5.</summary>
    ChargedSpazerWaveDownRightAlternateSpread5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightLoop"/> showing the Charged Spazer picture, down right, spread 4, as <see cref="ChargedSpazerWaveDownRightSpread4"/> does.</summary>
    ChargedSpazerWaveDownRightSpread4Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightLoop"/> showing the Charged Spazer picture, down right alternate, spread 4, as <see cref="ChargedSpazerWaveDownRightAlternateSpread4"/> does.</summary>
    ChargedSpazerWaveDownRightAlternateSpread4Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightLoop"/> showing the Charged Spazer picture, down right, spread 3, as <see cref="ChargedSpazerWaveDownRightSpread3"/> does.</summary>
    ChargedSpazerWaveDownRightSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightLoop"/> showing the Charged Spazer picture, down right alternate, spread 3, as <see cref="ChargedSpazerWaveDownRightAlternateSpread3"/> does.</summary>
    ChargedSpazerWaveDownRightAlternateSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightLoop"/> showing the Charged Spazer picture, down right, spread 2, as <see cref="ChargedSpazerWaveDownRightSpread2"/> does.</summary>
    ChargedSpazerWaveDownRightSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightLoop"/> showing the Charged Spazer picture, down right alternate, spread 2, as <see cref="ChargedSpazerWaveDownRightAlternateSpread2"/> does.</summary>
    ChargedSpazerWaveDownRightAlternateSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightLoop"/> showing the Charged Spazer picture, down right, spread 1, as <see cref="ChargedSpazerWaveDownRightSpread1"/> does.</summary>
    ChargedSpazerWaveDownRightSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightLoop"/> showing the Charged Spazer picture, down right alternate, spread 1, as <see cref="ChargedSpazerWaveDownRightAlternateSpread1"/> does.</summary>
    ChargedSpazerWaveDownRightAlternateSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRight"/> showing the Spazer Startup picture, down, stage 1.</summary>
    ChargedSpazerWaveDownFacingRightStage1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRight"/> showing the Spazer Startup picture, vertical alternate, stage 1.</summary>
    ChargedSpazerWaveDownFacingRightVerticalAlternateStage1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRight"/> showing the Spazer Startup picture, down, stage 2.</summary>
    ChargedSpazerWaveDownFacingRightStage2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRight"/> showing the Spazer Startup picture, vertical alternate, stage 2.</summary>
    ChargedSpazerWaveDownFacingRightVerticalAlternateStage2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRightLoop"/> showing the Charged Spazer picture, down, seed.</summary>
    ChargedSpazerWaveDownFacingRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRightLoop"/> showing the Charged Spazer picture, vertical alternate, seed.</summary>
    ChargedSpazerWaveDownFacingRightVerticalAlternateSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRightLoop"/> showing the Charged Spazer picture, down, spread 1.</summary>
    ChargedSpazerWaveDownFacingRightSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRightLoop"/> showing the Charged Spazer picture, vertical alternate, spread 1.</summary>
    ChargedSpazerWaveDownFacingRightVerticalAlternateSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRightLoop"/> showing the Charged Spazer picture, down, spread 2.</summary>
    ChargedSpazerWaveDownFacingRightSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRightLoop"/> showing the Charged Spazer picture, vertical alternate, spread 2.</summary>
    ChargedSpazerWaveDownFacingRightVerticalAlternateSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRightLoop"/> showing the Charged Spazer picture, down, spread 3.</summary>
    ChargedSpazerWaveDownFacingRightSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRightLoop"/> showing the Charged Spazer picture, vertical alternate, spread 3.</summary>
    ChargedSpazerWaveDownFacingRightVerticalAlternateSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRightLoop"/> showing the Charged Spazer picture, down, spread 4.</summary>
    ChargedSpazerWaveDownFacingRightSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRightLoop"/> showing the Charged Spazer picture, vertical alternate, spread 4.</summary>
    ChargedSpazerWaveDownFacingRightVerticalAlternateSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRightLoop"/> showing the Charged Spazer picture, down, spread 5.</summary>
    ChargedSpazerWaveDownFacingRightSpread5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRightLoop"/> showing the Charged Spazer picture, vertical alternate, spread 5.</summary>
    ChargedSpazerWaveDownFacingRightVerticalAlternateSpread5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRightLoop"/> showing the Charged Spazer picture, down, spread 4, as <see cref="ChargedSpazerWaveDownFacingRightSpread4"/> does.</summary>
    ChargedSpazerWaveDownFacingRightSpread4Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRightLoop"/> showing the Charged Spazer picture, vertical alternate, spread 4, as <see cref="ChargedSpazerWaveDownFacingRightVerticalAlternateSpread4"/> does.</summary>
    ChargedSpazerWaveDownFacingRightVerticalAlternateSpread4Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRightLoop"/> showing the Charged Spazer picture, down, spread 3, as <see cref="ChargedSpazerWaveDownFacingRightSpread3"/> does.</summary>
    ChargedSpazerWaveDownFacingRightSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRightLoop"/> showing the Charged Spazer picture, vertical alternate, spread 3, as <see cref="ChargedSpazerWaveDownFacingRightVerticalAlternateSpread3"/> does.</summary>
    ChargedSpazerWaveDownFacingRightVerticalAlternateSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRightLoop"/> showing the Charged Spazer picture, down, spread 2, as <see cref="ChargedSpazerWaveDownFacingRightSpread2"/> does.</summary>
    ChargedSpazerWaveDownFacingRightSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRightLoop"/> showing the Charged Spazer picture, vertical alternate, spread 2, as <see cref="ChargedSpazerWaveDownFacingRightVerticalAlternateSpread2"/> does.</summary>
    ChargedSpazerWaveDownFacingRightVerticalAlternateSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRightLoop"/> showing the Charged Spazer picture, down, spread 1, as <see cref="ChargedSpazerWaveDownFacingRightSpread1"/> does.</summary>
    ChargedSpazerWaveDownFacingRightSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRightLoop"/> showing the Charged Spazer picture, vertical alternate, spread 1, as <see cref="ChargedSpazerWaveDownFacingRightVerticalAlternateSpread1"/> does.</summary>
    ChargedSpazerWaveDownFacingRightVerticalAlternateSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeft"/> showing the Spazer Startup picture, up right, stage 1.</summary>
    ChargedSpazerWaveDownLeftUpRightStage1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeft"/> showing the Spazer Startup picture, up right alternate, stage 1.</summary>
    ChargedSpazerWaveDownLeftUpRightAlternateStage1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeft"/> showing the Spazer Startup picture, up right, stage 2.</summary>
    ChargedSpazerWaveDownLeftUpRightStage2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeft"/> showing the Spazer Startup picture, up right alternate, stage 2.</summary>
    ChargedSpazerWaveDownLeftUpRightAlternateStage2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftLoop"/> showing the Charged Spazer picture, down left, seed.</summary>
    ChargedSpazerWaveDownLeftSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftLoop"/> showing the Charged Spazer picture, up right alternate, seed.</summary>
    ChargedSpazerWaveDownLeftUpRightAlternateSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftLoop"/> showing the Charged Spazer picture, down left, spread 1.</summary>
    ChargedSpazerWaveDownLeftSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftLoop"/> showing the Charged Spazer picture, up right alternate, spread 1.</summary>
    ChargedSpazerWaveDownLeftUpRightAlternateSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftLoop"/> showing the Charged Spazer picture, down left, spread 2.</summary>
    ChargedSpazerWaveDownLeftSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftLoop"/> showing the Charged Spazer picture, up right alternate, spread 2.</summary>
    ChargedSpazerWaveDownLeftUpRightAlternateSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftLoop"/> showing the Charged Spazer picture, down left, spread 3.</summary>
    ChargedSpazerWaveDownLeftSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftLoop"/> showing the Charged Spazer picture, up right alternate, spread 3.</summary>
    ChargedSpazerWaveDownLeftUpRightAlternateSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftLoop"/> showing the Charged Spazer picture, down left, spread 4.</summary>
    ChargedSpazerWaveDownLeftSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftLoop"/> showing the Charged Spazer picture, up right alternate, spread 4.</summary>
    ChargedSpazerWaveDownLeftUpRightAlternateSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftLoop"/> showing the Charged Spazer picture, down left, spread 5.</summary>
    ChargedSpazerWaveDownLeftSpread5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftLoop"/> showing the Charged Spazer picture, up right alternate, spread 5.</summary>
    ChargedSpazerWaveDownLeftUpRightAlternateSpread5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftLoop"/> showing the Charged Spazer picture, down left, spread 4, as <see cref="ChargedSpazerWaveDownLeftSpread4"/> does.</summary>
    ChargedSpazerWaveDownLeftSpread4Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftLoop"/> showing the Charged Spazer picture, up right alternate, spread 4, as <see cref="ChargedSpazerWaveDownLeftUpRightAlternateSpread4"/> does.</summary>
    ChargedSpazerWaveDownLeftUpRightAlternateSpread4Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftLoop"/> showing the Charged Spazer picture, down left, spread 3, as <see cref="ChargedSpazerWaveDownLeftSpread3"/> does.</summary>
    ChargedSpazerWaveDownLeftSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftLoop"/> showing the Charged Spazer picture, up right alternate, spread 3, as <see cref="ChargedSpazerWaveDownLeftUpRightAlternateSpread3"/> does.</summary>
    ChargedSpazerWaveDownLeftUpRightAlternateSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftLoop"/> showing the Charged Spazer picture, down left, spread 2, as <see cref="ChargedSpazerWaveDownLeftSpread2"/> does.</summary>
    ChargedSpazerWaveDownLeftSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftLoop"/> showing the Charged Spazer picture, up right alternate, spread 2, as <see cref="ChargedSpazerWaveDownLeftUpRightAlternateSpread2"/> does.</summary>
    ChargedSpazerWaveDownLeftUpRightAlternateSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftLoop"/> showing the Charged Spazer picture, down left, spread 1, as <see cref="ChargedSpazerWaveDownLeftSpread1"/> does.</summary>
    ChargedSpazerWaveDownLeftSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftLoop"/> showing the Charged Spazer picture, up right alternate, spread 1, as <see cref="ChargedSpazerWaveDownLeftUpRightAlternateSpread1"/> does.</summary>
    ChargedSpazerWaveDownLeftUpRightAlternateSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeft"/> showing the Spazer Startup picture, left, stage 1.</summary>
    ChargedSpazerWaveLeftStage1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeft"/> showing the Spazer Startup picture, horizontal alternate, stage 1.</summary>
    ChargedSpazerWaveLeftHorizontalAlternateStage1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeft"/> showing the Spazer Startup picture, left, stage 2.</summary>
    ChargedSpazerWaveLeftStage2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeft"/> showing the Spazer Startup picture, horizontal alternate, stage 2.</summary>
    ChargedSpazerWaveLeftHorizontalAlternateStage2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftLoop"/> showing the Charged Spazer picture, left, seed.</summary>
    ChargedSpazerWaveLeftSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftLoop"/> showing the Charged Spazer picture, horizontal alternate, seed.</summary>
    ChargedSpazerWaveLeftHorizontalAlternateSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftLoop"/> showing the Charged Spazer picture, left, spread 1.</summary>
    ChargedSpazerWaveLeftSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftLoop"/> showing the Charged Spazer picture, horizontal alternate, spread 1.</summary>
    ChargedSpazerWaveLeftHorizontalAlternateSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftLoop"/> showing the Charged Spazer picture, left, spread 2.</summary>
    ChargedSpazerWaveLeftSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftLoop"/> showing the Charged Spazer picture, horizontal alternate, spread 2.</summary>
    ChargedSpazerWaveLeftHorizontalAlternateSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftLoop"/> showing the Charged Spazer picture, left, spread 3.</summary>
    ChargedSpazerWaveLeftSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftLoop"/> showing the Charged Spazer picture, horizontal alternate, spread 3.</summary>
    ChargedSpazerWaveLeftHorizontalAlternateSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftLoop"/> showing the Charged Spazer picture, left, spread 4.</summary>
    ChargedSpazerWaveLeftSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftLoop"/> showing the Charged Spazer picture, horizontal alternate, spread 4.</summary>
    ChargedSpazerWaveLeftHorizontalAlternateSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftLoop"/> showing the Charged Spazer picture, left, spread 5.</summary>
    ChargedSpazerWaveLeftSpread5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftLoop"/> showing the Charged Spazer picture, horizontal alternate, spread 5.</summary>
    ChargedSpazerWaveLeftHorizontalAlternateSpread5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftLoop"/> showing the Charged Spazer picture, left, spread 4, as <see cref="ChargedSpazerWaveLeftSpread4"/> does.</summary>
    ChargedSpazerWaveLeftSpread4Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftLoop"/> showing the Charged Spazer picture, horizontal alternate, spread 4, as <see cref="ChargedSpazerWaveLeftHorizontalAlternateSpread4"/> does.</summary>
    ChargedSpazerWaveLeftHorizontalAlternateSpread4Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftLoop"/> showing the Charged Spazer picture, left, spread 3, as <see cref="ChargedSpazerWaveLeftSpread3"/> does.</summary>
    ChargedSpazerWaveLeftSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftLoop"/> showing the Charged Spazer picture, horizontal alternate, spread 3, as <see cref="ChargedSpazerWaveLeftHorizontalAlternateSpread3"/> does.</summary>
    ChargedSpazerWaveLeftHorizontalAlternateSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftLoop"/> showing the Charged Spazer picture, left, spread 2, as <see cref="ChargedSpazerWaveLeftSpread2"/> does.</summary>
    ChargedSpazerWaveLeftSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftLoop"/> showing the Charged Spazer picture, horizontal alternate, spread 2, as <see cref="ChargedSpazerWaveLeftHorizontalAlternateSpread2"/> does.</summary>
    ChargedSpazerWaveLeftHorizontalAlternateSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftLoop"/> showing the Charged Spazer picture, left, spread 1, as <see cref="ChargedSpazerWaveLeftSpread1"/> does.</summary>
    ChargedSpazerWaveLeftSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftLoop"/> showing the Charged Spazer picture, horizontal alternate, spread 1, as <see cref="ChargedSpazerWaveLeftHorizontalAlternateSpread1"/> does.</summary>
    ChargedSpazerWaveLeftHorizontalAlternateSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeft"/> showing the Spazer Startup picture, up left, stage 1.</summary>
    ChargedSpazerWaveUpLeftStage1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeft"/> showing the Spazer Startup picture, down right alternate, stage 1.</summary>
    ChargedSpazerWaveUpLeftDownRightAlternateStage1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeft"/> showing the Spazer Startup picture, up left, stage 2.</summary>
    ChargedSpazerWaveUpLeftStage2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeft"/> showing the Spazer Startup picture, down right alternate, stage 2.</summary>
    ChargedSpazerWaveUpLeftDownRightAlternateStage2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftLoop"/> showing the Charged Spazer picture, up left, seed.</summary>
    ChargedSpazerWaveUpLeftSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftLoop"/> showing the Charged Spazer picture, down right alternate, seed.</summary>
    ChargedSpazerWaveUpLeftDownRightAlternateSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftLoop"/> showing the Charged Spazer picture, up left, spread 1.</summary>
    ChargedSpazerWaveUpLeftSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftLoop"/> showing the Charged Spazer picture, down right alternate, spread 1.</summary>
    ChargedSpazerWaveUpLeftDownRightAlternateSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftLoop"/> showing the Charged Spazer picture, up left, spread 2.</summary>
    ChargedSpazerWaveUpLeftSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftLoop"/> showing the Charged Spazer picture, down right alternate, spread 2.</summary>
    ChargedSpazerWaveUpLeftDownRightAlternateSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftLoop"/> showing the Charged Spazer picture, up left, spread 3.</summary>
    ChargedSpazerWaveUpLeftSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftLoop"/> showing the Charged Spazer picture, down right alternate, spread 3.</summary>
    ChargedSpazerWaveUpLeftDownRightAlternateSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftLoop"/> showing the Charged Spazer picture, up left, spread 4.</summary>
    ChargedSpazerWaveUpLeftSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftLoop"/> showing the Charged Spazer picture, down right alternate, spread 4.</summary>
    ChargedSpazerWaveUpLeftDownRightAlternateSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftLoop"/> showing the Charged Spazer picture, up left, spread 5.</summary>
    ChargedSpazerWaveUpLeftSpread5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftLoop"/> showing the Charged Spazer picture, down right alternate, spread 5.</summary>
    ChargedSpazerWaveUpLeftDownRightAlternateSpread5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftLoop"/> showing the Charged Spazer picture, up left, spread 4, as <see cref="ChargedSpazerWaveUpLeftSpread4"/> does.</summary>
    ChargedSpazerWaveUpLeftSpread4Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftLoop"/> showing the Charged Spazer picture, down right alternate, spread 4, as <see cref="ChargedSpazerWaveUpLeftDownRightAlternateSpread4"/> does.</summary>
    ChargedSpazerWaveUpLeftDownRightAlternateSpread4Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftLoop"/> showing the Charged Spazer picture, up left, spread 3, as <see cref="ChargedSpazerWaveUpLeftSpread3"/> does.</summary>
    ChargedSpazerWaveUpLeftSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftLoop"/> showing the Charged Spazer picture, down right alternate, spread 3, as <see cref="ChargedSpazerWaveUpLeftDownRightAlternateSpread3"/> does.</summary>
    ChargedSpazerWaveUpLeftDownRightAlternateSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftLoop"/> showing the Charged Spazer picture, up left, spread 2, as <see cref="ChargedSpazerWaveUpLeftSpread2"/> does.</summary>
    ChargedSpazerWaveUpLeftSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftLoop"/> showing the Charged Spazer picture, down right alternate, spread 2, as <see cref="ChargedSpazerWaveUpLeftDownRightAlternateSpread2"/> does.</summary>
    ChargedSpazerWaveUpLeftDownRightAlternateSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftLoop"/> showing the Charged Spazer picture, up left, spread 1, as <see cref="ChargedSpazerWaveUpLeftSpread1"/> does.</summary>
    ChargedSpazerWaveUpLeftSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftLoop"/> showing the Charged Spazer picture, down right alternate, spread 1, as <see cref="ChargedSpazerWaveUpLeftDownRightAlternateSpread1"/> does.</summary>
    ChargedSpazerWaveUpLeftDownRightAlternateSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaUpFacingRight"/> showing the Plasma Startup picture, up, core.</summary>
    ChargedPlasmaUpFacingRightCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaUpFacingRight"/> showing the Plasma Startup picture, up alternate, core.</summary>
    ChargedPlasmaUpFacingRightAlternateCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaUpFacingRight"/> showing the Plasma Startup picture, up, short.</summary>
    ChargedPlasmaUpFacingRightShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaUpFacingRight"/> showing the Plasma Startup picture, up alternate, short.</summary>
    ChargedPlasmaUpFacingRightAlternateShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaUpFacingRight"/> showing the Plasma Startup picture, up, long.</summary>
    ChargedPlasmaUpFacingRightLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaUpFacingRight"/> showing the Plasma Startup picture, up alternate, long.</summary>
    ChargedPlasmaUpFacingRightAlternateLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaUpFacingRightLoop"/> showing the Plasma Startup picture, up, full.</summary>
    ChargedPlasmaUpFacingRightFull,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaUpFacingRightLoop"/> showing the Plasma Startup picture, up alternate, full.</summary>
    ChargedPlasmaUpFacingRightAlternateFull,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaUpRight"/> showing the Plasma Startup picture, up right, core.</summary>
    ChargedPlasmaUpRightCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaUpRight"/> showing the Plasma Startup picture, up right alternate, core.</summary>
    ChargedPlasmaUpRightAlternateCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaUpRight"/> showing the Plasma Startup picture, up right, short.</summary>
    ChargedPlasmaUpRightShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaUpRight"/> showing the Plasma Startup picture, up right alternate, short.</summary>
    ChargedPlasmaUpRightAlternateShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaUpRight"/> showing the Plasma Startup picture, up right, long.</summary>
    ChargedPlasmaUpRightLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaUpRight"/> showing the Plasma Startup picture, up right alternate, long.</summary>
    ChargedPlasmaUpRightAlternateLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaUpRightLoop"/> showing the Plasma Startup picture, up right, full.</summary>
    ChargedPlasmaUpRightFull,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaUpRightLoop"/> showing the Plasma Startup picture, up right alternate, full.</summary>
    ChargedPlasmaUpRightAlternateFull,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaRight"/> showing the Plasma Startup picture, right, core.</summary>
    ChargedPlasmaRightCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaRight"/> showing the Plasma Startup picture, right alternate, core.</summary>
    ChargedPlasmaRightAlternateCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaRight"/> showing the Plasma Startup picture, right, short.</summary>
    ChargedPlasmaRightShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaRight"/> showing the Plasma Startup picture, right alternate, short.</summary>
    ChargedPlasmaRightAlternateShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaRight"/> showing the Plasma Startup picture, right, long.</summary>
    ChargedPlasmaRightLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaRight"/> showing the Plasma Startup picture, right alternate, long.</summary>
    ChargedPlasmaRightAlternateLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaRightLoop"/> showing the Plasma Startup picture, right, full.</summary>
    ChargedPlasmaRightFull,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaRightLoop"/> showing the Plasma Startup picture, right alternate, full.</summary>
    ChargedPlasmaRightAlternateFull,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaDownRight"/> showing the Plasma Startup picture, down right, core.</summary>
    ChargedPlasmaDownRightCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaDownRight"/> showing the Plasma Startup picture, down right alternate, core.</summary>
    ChargedPlasmaDownRightAlternateCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaDownRight"/> showing the Plasma Startup picture, down right, short.</summary>
    ChargedPlasmaDownRightShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaDownRight"/> showing the Plasma Startup picture, down right alternate, short.</summary>
    ChargedPlasmaDownRightAlternateShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaDownRight"/> showing the Plasma Startup picture, down right, long.</summary>
    ChargedPlasmaDownRightLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaDownRight"/> showing the Plasma Startup picture, down right alternate, long.</summary>
    ChargedPlasmaDownRightAlternateLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaDownRightLoop"/> showing the Plasma Startup picture, down right, full.</summary>
    ChargedPlasmaDownRightFull,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaDownRightLoop"/> showing the Plasma Startup picture, down right alternate, full.</summary>
    ChargedPlasmaDownRightAlternateFull,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpFacingRight"/> showing the Plasma Startup picture, up, core.</summary>
    ChargedPlasmaWaveUpFacingRightCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpFacingRight"/> showing the Plasma Startup picture, up alternate, core.</summary>
    ChargedPlasmaWaveUpFacingRightAlternateCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpFacingRight"/> showing the Plasma Startup picture, up, short.</summary>
    ChargedPlasmaWaveUpFacingRightShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpFacingRight"/> showing the Plasma Startup picture, up alternate, short.</summary>
    ChargedPlasmaWaveUpFacingRightAlternateShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpFacingRight"/> showing the Plasma Startup picture, up, long.</summary>
    ChargedPlasmaWaveUpFacingRightLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpFacingRight"/> showing the Plasma Startup picture, up alternate, long.</summary>
    ChargedPlasmaWaveUpFacingRightAlternateLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpFacingRightLoop"/> showing the Plasma Wave picture, up long, core.</summary>
    ChargedPlasmaWaveUpFacingRightLongCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpFacingRightLoop"/> showing the Plasma Wave picture, up alternate, core, as <see cref="ChargedPlasmaWaveUpFacingRightAlternateCore"/> does.</summary>
    ChargedPlasmaWaveUpFacingRightAlternateCoreTwin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpFacingRightLoop"/> showing the Plasma Wave picture, up long, spread 1.</summary>
    ChargedPlasmaWaveUpFacingRightLongSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpFacingRightLoop"/> showing the Plasma Wave picture, up alternate, spread 1.</summary>
    ChargedPlasmaWaveUpFacingRightAlternateSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpFacingRightLoop"/> showing the Plasma Wave picture, up long, spread 2.</summary>
    ChargedPlasmaWaveUpFacingRightLongSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpFacingRightLoop"/> showing the Plasma Wave picture, up alternate, spread 2.</summary>
    ChargedPlasmaWaveUpFacingRightAlternateSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpFacingRightLoop"/> showing the Plasma Wave picture, up long, spread 3.</summary>
    ChargedPlasmaWaveUpFacingRightLongSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpFacingRightLoop"/> showing the Plasma Wave picture, up alternate, spread 3.</summary>
    ChargedPlasmaWaveUpFacingRightAlternateSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpFacingRightLoop"/> showing the Plasma Wave picture, up long, spread 4.</summary>
    ChargedPlasmaWaveUpFacingRightLongSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpFacingRightLoop"/> showing the Plasma Wave picture, up alternate, spread 4.</summary>
    ChargedPlasmaWaveUpFacingRightAlternateSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpFacingRightLoop"/> showing the Plasma Wave picture, up long, spread 3, as <see cref="ChargedPlasmaWaveUpFacingRightLongSpread3"/> does.</summary>
    ChargedPlasmaWaveUpFacingRightLongSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpFacingRightLoop"/> showing the Plasma Wave picture, up alternate, spread 3, as <see cref="ChargedPlasmaWaveUpFacingRightAlternateSpread3"/> does.</summary>
    ChargedPlasmaWaveUpFacingRightAlternateSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpFacingRightLoop"/> showing the Plasma Wave picture, up long, spread 2, as <see cref="ChargedPlasmaWaveUpFacingRightLongSpread2"/> does.</summary>
    ChargedPlasmaWaveUpFacingRightLongSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpFacingRightLoop"/> showing the Plasma Wave picture, up alternate, spread 2, as <see cref="ChargedPlasmaWaveUpFacingRightAlternateSpread2"/> does.</summary>
    ChargedPlasmaWaveUpFacingRightAlternateSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpFacingRightLoop"/> showing the Plasma Wave picture, up long, spread 1, as <see cref="ChargedPlasmaWaveUpFacingRightLongSpread1"/> does.</summary>
    ChargedPlasmaWaveUpFacingRightLongSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpFacingRightLoop"/> showing the Plasma Wave picture, up alternate, spread 1, as <see cref="ChargedPlasmaWaveUpFacingRightAlternateSpread1"/> does.</summary>
    ChargedPlasmaWaveUpFacingRightAlternateSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpRight"/> showing the Plasma Startup picture, up right, core.</summary>
    ChargedPlasmaWaveUpRightCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpRight"/> showing the Plasma Startup picture, up right alternate, core.</summary>
    ChargedPlasmaWaveUpRightAlternateCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpRight"/> showing the Plasma Startup picture, up right, short.</summary>
    ChargedPlasmaWaveUpRightShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpRight"/> showing the Plasma Startup picture, up right alternate, short.</summary>
    ChargedPlasmaWaveUpRightAlternateShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpRight"/> showing the Plasma Startup picture, up right, long.</summary>
    ChargedPlasmaWaveUpRightLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpRight"/> showing the Plasma Startup picture, up right alternate, long.</summary>
    ChargedPlasmaWaveUpRightAlternateLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpRightLoop"/> showing the Plasma Wave picture, up right long, core.</summary>
    ChargedPlasmaWaveUpRightLongCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpRightLoop"/> showing the Plasma Wave picture, up right alternate, core, as <see cref="ChargedPlasmaWaveUpRightAlternateCore"/> does.</summary>
    ChargedPlasmaWaveUpRightAlternateCoreTwin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpRightLoop"/> showing the Plasma Wave picture, up right long, spread 1.</summary>
    ChargedPlasmaWaveUpRightLongSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpRightLoop"/> showing the Plasma Wave picture, up right alternate, spread 1.</summary>
    ChargedPlasmaWaveUpRightAlternateSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpRightLoop"/> showing the Plasma Wave picture, up right long, spread 2.</summary>
    ChargedPlasmaWaveUpRightLongSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpRightLoop"/> showing the Plasma Wave picture, up right alternate, spread 2.</summary>
    ChargedPlasmaWaveUpRightAlternateSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpRightLoop"/> showing the Plasma Wave picture, up right long, spread 3.</summary>
    ChargedPlasmaWaveUpRightLongSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpRightLoop"/> showing the Plasma Wave picture, up right alternate, spread 3.</summary>
    ChargedPlasmaWaveUpRightAlternateSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpRightLoop"/> showing the Plasma Wave picture, up right long, spread 4.</summary>
    ChargedPlasmaWaveUpRightLongSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpRightLoop"/> showing the Plasma Wave picture, up right alternate, spread 4.</summary>
    ChargedPlasmaWaveUpRightAlternateSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpRightLoop"/> showing the Plasma Wave picture, up right long, spread 3, as <see cref="ChargedPlasmaWaveUpRightLongSpread3"/> does.</summary>
    ChargedPlasmaWaveUpRightLongSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpRightLoop"/> showing the Plasma Wave picture, up right alternate, spread 3, as <see cref="ChargedPlasmaWaveUpRightAlternateSpread3"/> does.</summary>
    ChargedPlasmaWaveUpRightAlternateSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpRightLoop"/> showing the Plasma Wave picture, up right long, spread 2, as <see cref="ChargedPlasmaWaveUpRightLongSpread2"/> does.</summary>
    ChargedPlasmaWaveUpRightLongSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpRightLoop"/> showing the Plasma Wave picture, up right alternate, spread 2, as <see cref="ChargedPlasmaWaveUpRightAlternateSpread2"/> does.</summary>
    ChargedPlasmaWaveUpRightAlternateSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpRightLoop"/> showing the Plasma Wave picture, up right long, spread 1, as <see cref="ChargedPlasmaWaveUpRightLongSpread1"/> does.</summary>
    ChargedPlasmaWaveUpRightLongSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpRightLoop"/> showing the Plasma Wave picture, up right alternate, spread 1, as <see cref="ChargedPlasmaWaveUpRightAlternateSpread1"/> does.</summary>
    ChargedPlasmaWaveUpRightAlternateSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRight"/> showing the Plasma Startup picture, right, core.</summary>
    ChargedPlasmaWaveRightCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRight"/> showing the Plasma Startup picture, right alternate, core.</summary>
    ChargedPlasmaWaveRightAlternateCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRight"/> showing the Plasma Startup picture, right, short.</summary>
    ChargedPlasmaWaveRightShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRight"/> showing the Plasma Startup picture, right alternate, short.</summary>
    ChargedPlasmaWaveRightAlternateShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRight"/> showing the Plasma Startup picture, right, long.</summary>
    ChargedPlasmaWaveRightLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRight"/> showing the Plasma Startup picture, right alternate, long.</summary>
    ChargedPlasmaWaveRightAlternateLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRightLoop"/> showing the Plasma Wave picture, right long, core.</summary>
    ChargedPlasmaWaveRightLongCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRightLoop"/> showing the Plasma Wave picture, right alternate, core, as <see cref="ChargedPlasmaWaveRightAlternateCore"/> does.</summary>
    ChargedPlasmaWaveRightAlternateCoreTwin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRightLoop"/> showing the Plasma Wave picture, right long, spread 1.</summary>
    ChargedPlasmaWaveRightLongSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRightLoop"/> showing the Plasma Wave picture, right alternate, spread 1.</summary>
    ChargedPlasmaWaveRightAlternateSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRightLoop"/> showing the Plasma Wave picture, right long, spread 2.</summary>
    ChargedPlasmaWaveRightLongSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRightLoop"/> showing the Plasma Wave picture, right alternate, spread 2.</summary>
    ChargedPlasmaWaveRightAlternateSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRightLoop"/> showing the Plasma Wave picture, right long, spread 3.</summary>
    ChargedPlasmaWaveRightLongSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRightLoop"/> showing the Plasma Wave picture, right alternate, spread 3.</summary>
    ChargedPlasmaWaveRightAlternateSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRightLoop"/> showing the Plasma Wave picture, right long, spread 4.</summary>
    ChargedPlasmaWaveRightLongSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRightLoop"/> showing the Plasma Wave picture, right alternate, spread 4.</summary>
    ChargedPlasmaWaveRightAlternateSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRightLoop"/> showing the Plasma Wave picture, right long, spread 3, as <see cref="ChargedPlasmaWaveRightLongSpread3"/> does.</summary>
    ChargedPlasmaWaveRightLongSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRightLoop"/> showing the Plasma Wave picture, right alternate, spread 3, as <see cref="ChargedPlasmaWaveRightAlternateSpread3"/> does.</summary>
    ChargedPlasmaWaveRightAlternateSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRightLoop"/> showing the Plasma Wave picture, right long, spread 2, as <see cref="ChargedPlasmaWaveRightLongSpread2"/> does.</summary>
    ChargedPlasmaWaveRightLongSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRightLoop"/> showing the Plasma Wave picture, right alternate, spread 2, as <see cref="ChargedPlasmaWaveRightAlternateSpread2"/> does.</summary>
    ChargedPlasmaWaveRightAlternateSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRightLoop"/> showing the Plasma Wave picture, right long, spread 1, as <see cref="ChargedPlasmaWaveRightLongSpread1"/> does.</summary>
    ChargedPlasmaWaveRightLongSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRightLoop"/> showing the Plasma Wave picture, right alternate, spread 1, as <see cref="ChargedPlasmaWaveRightAlternateSpread1"/> does.</summary>
    ChargedPlasmaWaveRightAlternateSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveDownRight"/> showing the Plasma Startup picture, down right, core.</summary>
    ChargedPlasmaWaveDownRightCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveDownRight"/> showing the Plasma Startup picture, down right alternate, core.</summary>
    ChargedPlasmaWaveDownRightAlternateCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveDownRight"/> showing the Plasma Startup picture, down right, short.</summary>
    ChargedPlasmaWaveDownRightShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveDownRight"/> showing the Plasma Startup picture, down right alternate, short.</summary>
    ChargedPlasmaWaveDownRightAlternateShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveDownRight"/> showing the Plasma Startup picture, down right, long.</summary>
    ChargedPlasmaWaveDownRightLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveDownRight"/> showing the Plasma Startup picture, down right alternate, long.</summary>
    ChargedPlasmaWaveDownRightAlternateLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveDownRightLoop"/> showing the Plasma Wave picture, down right long, core.</summary>
    ChargedPlasmaWaveDownRightLongCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveDownRightLoop"/> showing the Plasma Wave picture, down right alternate, core, as <see cref="ChargedPlasmaWaveDownRightAlternateCore"/> does.</summary>
    ChargedPlasmaWaveDownRightAlternateCoreTwin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveDownRightLoop"/> showing the Plasma Wave picture, down right long, spread 1.</summary>
    ChargedPlasmaWaveDownRightLongSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveDownRightLoop"/> showing the Plasma Wave picture, down right alternate, spread 1.</summary>
    ChargedPlasmaWaveDownRightAlternateSpread1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveDownRightLoop"/> showing the Plasma Wave picture, down right long, spread 2.</summary>
    ChargedPlasmaWaveDownRightLongSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveDownRightLoop"/> showing the Plasma Wave picture, down right alternate, spread 2.</summary>
    ChargedPlasmaWaveDownRightAlternateSpread2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveDownRightLoop"/> showing the Plasma Wave picture, down right long, spread 3.</summary>
    ChargedPlasmaWaveDownRightLongSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveDownRightLoop"/> showing the Plasma Wave picture, down right alternate, spread 3.</summary>
    ChargedPlasmaWaveDownRightAlternateSpread3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveDownRightLoop"/> showing the Plasma Wave picture, down right long, spread 4.</summary>
    ChargedPlasmaWaveDownRightLongSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveDownRightLoop"/> showing the Plasma Wave picture, down right alternate, spread 4.</summary>
    ChargedPlasmaWaveDownRightAlternateSpread4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveDownRightLoop"/> showing the Plasma Wave picture, down right long, spread 3, as <see cref="ChargedPlasmaWaveDownRightLongSpread3"/> does.</summary>
    ChargedPlasmaWaveDownRightLongSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveDownRightLoop"/> showing the Plasma Wave picture, down right alternate, spread 3, as <see cref="ChargedPlasmaWaveDownRightAlternateSpread3"/> does.</summary>
    ChargedPlasmaWaveDownRightAlternateSpread3Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveDownRightLoop"/> showing the Plasma Wave picture, down right long, spread 2, as <see cref="ChargedPlasmaWaveDownRightLongSpread2"/> does.</summary>
    ChargedPlasmaWaveDownRightLongSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveDownRightLoop"/> showing the Plasma Wave picture, down right alternate, spread 2, as <see cref="ChargedPlasmaWaveDownRightAlternateSpread2"/> does.</summary>
    ChargedPlasmaWaveDownRightAlternateSpread2Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveDownRightLoop"/> showing the Plasma Wave picture, down right long, spread 1, as <see cref="ChargedPlasmaWaveDownRightLongSpread1"/> does.</summary>
    ChargedPlasmaWaveDownRightLongSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveDownRightLoop"/> showing the Plasma Wave picture, down right alternate, spread 1, as <see cref="ChargedPlasmaWaveDownRightAlternateSpread1"/> does.</summary>
    ChargedPlasmaWaveDownRightAlternateSpread1Twin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.MissileUpFacingRight"/> showing the Missile picture, up.</summary>
    MissileUpFacingRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.MissileUpRight"/> showing the Missile picture, up right.</summary>
    MissileUpRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.MissileRight"/> showing the Missile picture, right.</summary>
    MissileRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.MissileDownRight"/> showing the Missile picture, down right.</summary>
    MissileDownRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.MissileDownFacingRight"/> showing the Missile picture, down.</summary>
    MissileDownFacingRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.MissileDownLeft"/> showing the Missile picture, down left.</summary>
    MissileDownLeft,
    /// <summary>A frame of <see cref="SamusProjectileSegment.MissileLeft"/> showing the Missile picture, left.</summary>
    MissileLeft,
    /// <summary>A frame of <see cref="SamusProjectileSegment.MissileUpLeft"/> showing the Missile picture, up left.</summary>
    MissileUpLeft,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SuperMissileUpFacingRight"/> showing the Super Missile picture, up.</summary>
    SuperMissileUpFacingRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SuperMissileUpRight"/> showing the Super Missile picture, up right.</summary>
    SuperMissileUpRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SuperMissileRight"/> showing the Super Missile picture, right.</summary>
    SuperMissileRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SuperMissileDownRight"/> showing the Super Missile picture, down right.</summary>
    SuperMissileDownRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SuperMissileDownFacingRight"/> showing the Super Missile picture, down.</summary>
    SuperMissileDownFacingRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SuperMissileDownLeft"/> showing the Super Missile picture, down left.</summary>
    SuperMissileDownLeft,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SuperMissileLeft"/> showing the Super Missile picture, left.</summary>
    SuperMissileLeft,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SuperMissileUpLeft"/> showing the Super Missile picture, up left.</summary>
    SuperMissileUpLeft,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SuperMissileLink"/> showing nothing.</summary>
    SuperMissileLinkHidden,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PowerBomb"/> showing the Power Bomb picture, phase 1.</summary>
    PowerBombPhase1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PowerBomb"/> showing the Power Bomb picture, phase 2.</summary>
    PowerBombPhase2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PowerBomb"/> showing the Power Bomb picture, phase 3.</summary>
    PowerBombPhase3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.FastPowerBomb"/> showing the Power Bomb picture, phase 1.</summary>
    FastPowerBombPhase1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.FastPowerBomb"/> showing the Power Bomb picture, phase 2.</summary>
    FastPowerBombPhase2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.FastPowerBomb"/> showing the Power Bomb picture, phase 3.</summary>
    FastPowerBombPhase3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.Bomb"/> showing the Bomb picture, phase 1.</summary>
    BombPhase1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.Bomb"/> showing the Bomb picture, phase 2.</summary>
    BombPhase2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.Bomb"/> showing the Bomb picture, phase 3.</summary>
    BombPhase3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.Bomb"/> showing the Bomb picture, phase 4.</summary>
    BombPhase4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.FastBomb"/> showing the Bomb picture, phase 1.</summary>
    FastBombPhase1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.FastBomb"/> showing the Bomb picture, phase 2.</summary>
    FastBombPhase2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.FastBomb"/> showing the Bomb picture, phase 3.</summary>
    FastBombPhase3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.FastBomb"/> showing the Bomb picture, phase 4.</summary>
    FastBombPhase4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.BeamExplosion"/> showing the Beam Explosion picture, stage 1.</summary>
    BeamExplosionStage1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.BeamExplosion"/> showing the Beam Explosion picture, stage 2.</summary>
    BeamExplosionStage2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.BeamExplosion"/> showing the Beam Explosion picture, stage 3.</summary>
    BeamExplosionStage3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.BeamExplosion"/> showing the Beam Explosion picture, stage 4.</summary>
    BeamExplosionStage4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.BeamExplosion"/> showing the Beam Explosion picture, stage 5.</summary>
    BeamExplosionStage5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.BeamExplosion"/> showing the Beam Explosion picture, stage 6.</summary>
    BeamExplosionStage6,
    /// <summary>A frame of <see cref="SamusProjectileSegment.MissileExplosion"/> showing the Missile Explosion picture, stage 1.</summary>
    MissileExplosionStage1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.MissileExplosion"/> showing the Missile Explosion picture, stage 2.</summary>
    MissileExplosionStage2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.MissileExplosion"/> showing the Missile Explosion picture, stage 3.</summary>
    MissileExplosionStage3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.MissileExplosion"/> showing the Missile Explosion picture, stage 4.</summary>
    MissileExplosionStage4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.MissileExplosion"/> showing the Missile Explosion picture, stage 5.</summary>
    MissileExplosionStage5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.MissileExplosion"/> showing the Missile Explosion picture, stage 6.</summary>
    MissileExplosionStage6,
    /// <summary>A frame of <see cref="SamusProjectileSegment.BombExplosion"/> showing the Bomb Explosion picture, stage 1.</summary>
    BombExplosionStage1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.BombExplosion"/> showing the Bomb Explosion picture, stage 2.</summary>
    BombExplosionStage2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.BombExplosion"/> showing the Bomb Explosion picture, stage 3.</summary>
    BombExplosionStage3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.BombExplosion"/> showing the Bomb Explosion picture, stage 4.</summary>
    BombExplosionStage4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.BombExplosion"/> showing the Bomb Explosion picture, stage 5.</summary>
    BombExplosionStage5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaSpecialAttack"/> showing the Bomb Explosion picture, stage 1.</summary>
    PlasmaSpecialAttackStage1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaSpecialAttack"/> showing the Bomb Explosion picture, stage 2.</summary>
    PlasmaSpecialAttackStage2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaSpecialAttack"/> showing the Bomb Explosion picture, stage 3.</summary>
    PlasmaSpecialAttackStage3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaSpecialAttack"/> showing the Bomb Explosion picture, stage 4.</summary>
    PlasmaSpecialAttackStage4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaSpecialAttack"/> showing the Bomb Explosion picture, stage 5.</summary>
    PlasmaSpecialAttackStage5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SuperMissileExplosion"/> showing the Super Missile Explosion picture, stage 1.</summary>
    SuperMissileExplosionStage1,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SuperMissileExplosion"/> showing the Super Missile Explosion picture, stage 2.</summary>
    SuperMissileExplosionStage2,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SuperMissileExplosion"/> showing the Super Missile Explosion picture, stage 3.</summary>
    SuperMissileExplosionStage3,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SuperMissileExplosion"/> showing the Super Missile Explosion picture, stage 4.</summary>
    SuperMissileExplosionStage4,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SuperMissileExplosion"/> showing the Super Missile Explosion picture, stage 5.</summary>
    SuperMissileExplosionStage5,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SuperMissileExplosion"/> showing the Super Missile Explosion picture, stage 6.</summary>
    SuperMissileExplosionStage6,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ShinesparkEchoUpFacingRight"/> showing nothing.</summary>
    ShinesparkEchoUpFacingRightHidden,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ShinesparkEchoUpFacingRight"/> showing nothing, as <see cref="ShinesparkEchoUpFacingRightHidden"/> does.</summary>
    ShinesparkEchoUpFacingRightHiddenTwin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ShinesparkEchoUpFacingRight"/> showing nothing, as <see cref="ShinesparkEchoUpFacingRightHidden"/> does.</summary>
    ShinesparkEchoUpFacingRightHiddenTwinB,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ShinesparkEchoUpFacingRight"/> showing nothing, as <see cref="ShinesparkEchoUpFacingRightHidden"/> does.</summary>
    ShinesparkEchoUpFacingRightHiddenTwinC,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerSpecialAttackTrailUpFacingRight"/> showing nothing.</summary>
    SpazerSpecialAttackTrailUpFacingRightHidden,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerSpecialAttackTrailUpFacingRight"/> showing nothing, as <see cref="SpazerSpecialAttackTrailUpFacingRightHidden"/> does.</summary>
    SpazerSpecialAttackTrailUpFacingRightHiddenTwin,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerSpecialAttackTrailUpFacingRightLoop"/> showing nothing, as <see cref="SpazerSpecialAttackTrailUpFacingRightHidden"/> does.</summary>
    SpazerSpecialAttackTrailUpFacingRightHiddenTwinB,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveSpecialAttack"/> showing the Charged Wave picture, core.</summary>
    WaveSpecialAttackCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveSpecialAttack"/> showing the Charged Wave picture, core alternate.</summary>
    WaveSpecialAttackCoreAlternate,
}
