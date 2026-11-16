using SuperMetroid.Core.Animation;
using F = SuperMetroid.Core.Samus.Projectiles.SamusProjectileFrameId;
using P = SuperMetroid.Core.Samus.Projectiles.SamusProjectileSegment;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The Samus projectiles' animations (#1301):
/// each timed step shows a <see cref="SamusProjectileFrameId"/> for its duration, and a goto or
/// delete ends its animation.
/// </summary>
internal static class SamusProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<SamusProjectileSegment, SamusProjectileFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<SamusProjectileSegment, AnimationBeat<SamusProjectileFrameId, NoAnimationCue>[]>
    {
        [P.PowerUpFacingRight] =
        [
            Show(F.PowerUpFacingRight, 15),
        ],
        [P.PowerUpRight] =
        [
            Show(F.PowerUpRight, 15),
        ],
        [P.PowerRight] =
        [
            Show(F.PowerRight, 15),
        ],
        [P.PowerDownRight] =
        [
            Show(F.PowerDownRight, 15),
        ],
        [P.PowerDownFacingRight] =
        [
            Show(F.PowerDownFacingRight, 15),
        ],
        [P.PowerDownLeft] =
        [
            Show(F.PowerDownLeft, 15),
        ],
        [P.PowerLeft] =
        [
            Show(F.PowerLeft, 15),
        ],
        [P.PowerUpLeft] =
        [
            Show(F.PowerUpLeft, 15),
        ],
        [P.WaveUpFacingRight] =
        [
            Show(F.WaveUpFacingRightHidden, 4),
        ],
        [P.WaveDownFacingRight] =
        [
            Show(F.WaveDownFacingRightCentre, 1),
            Show(F.WaveDownFacingRightRight1, 1),
            Show(F.WaveDownFacingRightRight2, 1),
            Show(F.WaveDownFacingRightRight3, 1),
            Show(F.WaveDownFacingRightRight4, 1),
            Show(F.WaveDownFacingRightRight3Twin, 1),
            Show(F.WaveDownFacingRightRight2Twin, 1),
            Show(F.WaveDownFacingRightRight1Twin, 1),
            Show(F.WaveDownFacingRightCentreTwin, 1),
            Show(F.WaveDownFacingRightLeft1, 1),
            Show(F.WaveDownFacingRightLeft2, 1),
            Show(F.WaveDownFacingRightLeft3, 1),
            Show(F.WaveDownFacingRightLeft4, 1),
            Show(F.WaveDownFacingRightLeft3Twin, 1),
            Show(F.WaveDownFacingRightLeft2Twin, 1),
            Show(F.WaveDownFacingRightLeft1Twin, 1),
        ],
        [P.WaveUpRight] =
        [
            Show(F.WaveUpRightCentre, 1),
            Show(F.WaveUpRightUpLeft1, 1),
            Show(F.WaveUpRightUpLeft2, 1),
            Show(F.WaveUpRightUpLeft3, 1),
            Show(F.WaveUpRightUpLeft4, 1),
            Show(F.WaveUpRightUpLeft3Twin, 1),
            Show(F.WaveUpRightUpLeft2Twin, 1),
            Show(F.WaveUpRightUpLeft1Twin, 1),
            Show(F.WaveUpRightCentreTwin, 1),
            Show(F.WaveUpRightDownRight1, 1),
            Show(F.WaveUpRightDownRight2, 1),
            Show(F.WaveUpRightDownRight3, 1),
            Show(F.WaveUpRightDownRight4, 1),
            Show(F.WaveUpRightDownRight3Twin, 1),
            Show(F.WaveUpRightDownRight2Twin, 1),
            Show(F.WaveUpRightDownRight1Twin, 1),
        ],
        [P.WaveRight] =
        [
            Show(F.WaveRightCentre, 1),
            Show(F.WaveRightUp1, 1),
            Show(F.WaveRightUp2, 1),
            Show(F.WaveRightUp3, 1),
            Show(F.WaveRightUp4, 1),
            Show(F.WaveRightUp3Twin, 1),
            Show(F.WaveRightUp2Twin, 1),
            Show(F.WaveRightUp1Twin, 1),
            Show(F.WaveRightCentreTwin, 1),
            Show(F.WaveRightDown1, 1),
            Show(F.WaveRightDown2, 1),
            Show(F.WaveRightDown3, 1),
            Show(F.WaveRightDown4, 1),
            Show(F.WaveRightDown3Twin, 1),
            Show(F.WaveRightDown2Twin, 1),
            Show(F.WaveRightDown1Twin, 1),
        ],
        [P.WaveDownRight] =
        [
            Show(F.WaveDownRightCentre, 1),
            Show(F.WaveDownRightUpRight1, 1),
            Show(F.WaveDownRightUpRight2, 1),
            Show(F.WaveDownRightUpRight3, 1),
            Show(F.WaveDownRightUpRight4, 1),
            Show(F.WaveDownRightUpRight3Twin, 1),
            Show(F.WaveDownRightUpRight2Twin, 1),
            Show(F.WaveDownRightUpRight1Twin, 1),
            Show(F.WaveDownRightCentreTwin, 1),
            Show(F.WaveDownRightDownLeft1, 1),
            Show(F.WaveDownRightDownLeft2, 1),
            Show(F.WaveDownRightDownLeft3, 1),
            Show(F.WaveDownRightDownLeft4, 1),
            Show(F.WaveDownRightDownLeft3Twin, 1),
            Show(F.WaveDownRightDownLeft2Twin, 1),
            Show(F.WaveDownRightDownLeft1Twin, 1),
        ],
        [P.IceUpFacingRight] =
        [
            Show(F.IceUpFacingRightPhase1, 1),
            Show(F.IceUpFacingRightPhase2, 1),
            Show(F.IceUpFacingRightPhase3, 1),
            Show(F.IceUpFacingRightPhase4, 1),
        ],
        [P.SpazerUpFacingRight] =
        [
            Show(F.SpazerUpFacingRightSeed, 2),
            Show(F.SpazerUpFacingRightSpread2, 2),
        ],
        [P.SpazerUpFacingRightLoop] =
        [
            Show(F.SpazerUpFacingRightSpread5, 2),
        ],
        [P.SpazerUpRight] =
        [
            Show(F.SpazerUpRightSeed, 2),
            Show(F.SpazerUpRightSpread2, 2),
        ],
        [P.SpazerUpRightLoop] =
        [
            Show(F.SpazerUpRightSpread5, 2),
        ],
        [P.SpazerRight] =
        [
            Show(F.SpazerRightSeed, 2),
            Show(F.SpazerRightSpread2, 2),
        ],
        [P.SpazerRightLoop] =
        [
            Show(F.SpazerRightSpread5, 2),
        ],
        [P.SpazerDownRight] =
        [
            Show(F.SpazerDownRightSeed, 2),
            Show(F.SpazerDownRightSpread2, 2),
        ],
        [P.SpazerDownRightLoop] =
        [
            Show(F.SpazerDownRightSpread5, 2),
        ],
        [P.SpazerDownFacingRight] =
        [
            Show(F.SpazerDownFacingRightSeed, 2),
            Show(F.SpazerDownFacingRightSpread2, 2),
        ],
        [P.SpazerDownFacingRightLoop] =
        [
            Show(F.SpazerDownFacingRightSpread5, 2),
        ],
        [P.SpazerDownLeft] =
        [
            Show(F.SpazerDownLeftSeed, 2),
            Show(F.SpazerDownLeftSpread1, 2),
        ],
        [P.SpazerDownLeftLoop] =
        [
            Show(F.SpazerDownLeftSpread4, 2),
        ],
        [P.SpazerLeft] =
        [
            Show(F.SpazerLeftSeed, 2),
            Show(F.SpazerLeftSpread2, 2),
        ],
        [P.SpazerLeftLoop] =
        [
            Show(F.SpazerLeftSpread5, 2),
        ],
        [P.SpazerUpLeft] =
        [
            Show(F.SpazerUpLeftSeed, 2),
            Show(F.SpazerUpLeftSpread2, 2),
        ],
        [P.SpazerUpLeftLoop] =
        [
            Show(F.SpazerUpLeftSpread5, 2),
        ],
        [P.SpazerWaveUpFacingRight] =
        [
            Show(F.SpazerWaveUpFacingRightSeed, 2),
            Show(F.SpazerWaveUpFacingRightSpread1, 2),
            Show(F.SpazerWaveUpFacingRightSpread2, 2),
            Show(F.SpazerWaveUpFacingRightSpread3, 2),
            Show(F.SpazerWaveUpFacingRightSpread4, 2),
            Show(F.SpazerWaveUpFacingRightSpread5, 2),
            Show(F.SpazerWaveUpFacingRightSpread4Twin, 2),
            Show(F.SpazerWaveUpFacingRightSpread3Twin, 2),
            Show(F.SpazerWaveUpFacingRightSpread2Twin, 2),
            Show(F.SpazerWaveUpFacingRightSpread1Twin, 2),
        ],
        [P.SpazerWaveUpRight] =
        [
            Show(F.SpazerWaveUpRightSeed, 2),
            Show(F.SpazerWaveUpRightSpread1, 2),
            Show(F.SpazerWaveUpRightSpread2, 2),
            Show(F.SpazerWaveUpRightSpread3, 2),
            Show(F.SpazerWaveUpRightSpread4, 2),
            Show(F.SpazerWaveUpRightSpread5, 2),
            Show(F.SpazerWaveUpRightSpread4Twin, 2),
            Show(F.SpazerWaveUpRightSpread3Twin, 2),
            Show(F.SpazerWaveUpRightSpread2Twin, 2),
            Show(F.SpazerWaveUpRightSpread1Twin, 2),
        ],
        [P.SpazerWaveRight] =
        [
            Show(F.SpazerWaveRightSeed, 2),
            Show(F.SpazerWaveRightSpread1, 2),
            Show(F.SpazerWaveRightSpread2, 2),
            Show(F.SpazerWaveRightSpread3, 2),
            Show(F.SpazerWaveRightSpread4, 2),
            Show(F.SpazerWaveRightSpread5, 2),
            Show(F.SpazerWaveRightSpread4Twin, 2),
            Show(F.SpazerWaveRightSpread3Twin, 2),
            Show(F.SpazerWaveRightSpread2Twin, 2),
            Show(F.SpazerWaveRightSpread1Twin, 2),
        ],
        [P.SpazerWaveDownRight] =
        [
            Show(F.SpazerWaveDownRightSeed, 2),
            Show(F.SpazerWaveDownRightSpread1, 2),
            Show(F.SpazerWaveDownRightSpread2, 2),
            Show(F.SpazerWaveDownRightSpread3, 2),
            Show(F.SpazerWaveDownRightSpread4, 2),
            Show(F.SpazerWaveDownRightSpread5, 2),
            Show(F.SpazerWaveDownRightSpread4Twin, 2),
            Show(F.SpazerWaveDownRightSpread3Twin, 2),
            Show(F.SpazerWaveDownRightSpread2Twin, 2),
            Show(F.SpazerWaveDownRightSpread1Twin, 2),
        ],
        [P.SpazerWaveDownFacingRight] =
        [
            Show(F.SpazerWaveDownFacingRightSeed, 2),
            Show(F.SpazerWaveDownFacingRightSpread1, 2),
            Show(F.SpazerWaveDownFacingRightSpread2, 2),
            Show(F.SpazerWaveDownFacingRightSpread3, 2),
            Show(F.SpazerWaveDownFacingRightSpread4, 2),
            Show(F.SpazerWaveDownFacingRightSpread5, 2),
            Show(F.SpazerWaveDownFacingRightSpread4Twin, 2),
            Show(F.SpazerWaveDownFacingRightSpread3Twin, 2),
            Show(F.SpazerWaveDownFacingRightSpread2Twin, 2),
            Show(F.SpazerWaveDownFacingRightSpread1Twin, 2),
        ],
        [P.SpazerWaveDownLeft] =
        [
            Show(F.SpazerWaveDownLeftSeed, 2),
            Show(F.SpazerWaveDownLeftSpread1, 2),
            Show(F.SpazerWaveDownLeftSpread2, 2),
            Show(F.SpazerWaveDownLeftSpread3, 2),
            Show(F.SpazerWaveDownLeftSpread4, 2),
            Show(F.SpazerWaveDownLeftSpread4Twin, 2),
            Show(F.SpazerWaveDownLeftSpread4TwinB, 2),
            Show(F.SpazerWaveDownLeftSpread3Twin, 2),
            Show(F.SpazerWaveDownLeftSpread2Twin, 2),
            Show(F.SpazerWaveDownLeftSpread1Twin, 2),
        ],
        [P.SpazerWaveLeft] =
        [
            Show(F.SpazerWaveLeftSeed, 2),
            Show(F.SpazerWaveLeftSpread1, 2),
            Show(F.SpazerWaveLeftSpread2, 2),
            Show(F.SpazerWaveLeftSpread3, 2),
            Show(F.SpazerWaveLeftSpread4, 2),
            Show(F.SpazerWaveLeftSpread5, 2),
            Show(F.SpazerWaveLeftSpread4Twin, 2),
            Show(F.SpazerWaveLeftSpread3Twin, 2),
            Show(F.SpazerWaveLeftSpread2Twin, 2),
            Show(F.SpazerWaveLeftSpread1Twin, 2),
        ],
        [P.SpazerWaveUpLeft] =
        [
            Show(F.SpazerWaveUpLeftSeed, 2),
            Show(F.SpazerWaveUpLeftSpread1, 2),
            Show(F.SpazerWaveUpLeftSpread2, 2),
            Show(F.SpazerWaveUpLeftSpread3, 2),
            Show(F.SpazerWaveUpLeftSpread4, 2),
            Show(F.SpazerWaveUpLeftSpread5, 2),
            Show(F.SpazerWaveUpLeftSpread4Twin, 2),
            Show(F.SpazerWaveUpLeftSpread3Twin, 2),
            Show(F.SpazerWaveUpLeftSpread2Twin, 2),
            Show(F.SpazerWaveUpLeftSpread1Twin, 2),
        ],
        [P.PlasmaUpFacingRight] =
        [
            Show(F.PlasmaUpFacingRightCore, 1),
        ],
        [P.PlasmaUpFacingRightLoop] =
        [
            Show(F.PlasmaUpFacingRight, 15),
        ],
        [P.PlasmaUpRight] =
        [
            Show(F.PlasmaUpRightCore, 1),
        ],
        [P.PlasmaUpRightLoop] =
        [
            Show(F.PlasmaUpRight, 15),
        ],
        [P.PlasmaRight] =
        [
            Show(F.PlasmaRightCore, 1),
        ],
        [P.PlasmaRightLoop] =
        [
            Show(F.PlasmaRight, 15),
        ],
        [P.PlasmaDownRight] =
        [
            Show(F.PlasmaDownRightCore, 1),
        ],
        [P.PlasmaDownRightLoop] =
        [
            Show(F.PlasmaDownRight, 15),
        ],
        [P.PlasmaIceWaveUpFacingRight] =
        [
            Show(F.PlasmaIceWaveUpFacingRightCore, 1),
        ],
        [P.PlasmaWaveUpFacingRight] =
        [
            Show(F.PlasmaWaveUpFacingRightShortCore, 2),
            Show(F.PlasmaWaveUpFacingRightShortSpread1, 2),
            Show(F.PlasmaWaveUpFacingRightShortSpread2, 2),
            Show(F.PlasmaWaveUpFacingRightShortSpread3, 2),
            Show(F.PlasmaWaveUpFacingRightShortSpread4, 2),
            Show(F.PlasmaWaveUpFacingRightShortSpread3Twin, 2),
            Show(F.PlasmaWaveUpFacingRightShortSpread2Twin, 2),
            Show(F.PlasmaWaveUpFacingRightShortSpread1Twin, 2),
        ],
        [P.PlasmaIceWaveUpRight] =
        [
            Show(F.PlasmaIceWaveUpRightCore, 1),
        ],
        [P.PlasmaWaveUpRight] =
        [
            Show(F.PlasmaWaveUpRightShortCore, 2),
            Show(F.PlasmaWaveUpRightShortSpread1, 2),
            Show(F.PlasmaWaveUpRightShortSpread2, 2),
            Show(F.PlasmaWaveUpRightShortSpread3, 2),
            Show(F.PlasmaWaveUpRightShortSpread4, 2),
            Show(F.PlasmaWaveUpRightShortSpread3Twin, 2),
            Show(F.PlasmaWaveUpRightShortSpread2Twin, 2),
            Show(F.PlasmaWaveUpRightShortSpread1Twin, 2),
        ],
        [P.PlasmaWaveRight] =
        [
            Show(F.PlasmaWaveRightCore, 1),
        ],
        [P.PlasmaWaveRightLoop] =
        [
            Show(F.PlasmaWaveRightShortCore, 2),
            Show(F.PlasmaWaveRightShortSpread1, 2),
            Show(F.PlasmaWaveRightShortSpread2, 2),
            Show(F.PlasmaWaveRightShortSpread3, 2),
            Show(F.PlasmaWaveRightShortSpread4, 2),
            Show(F.PlasmaWaveRightShortSpread3Twin, 2),
            Show(F.PlasmaWaveRightShortSpread2Twin, 2),
            Show(F.PlasmaWaveRightShortSpread1Twin, 2),
        ],
        [P.PlasmaIceWaveDownRight] =
        [
            Show(F.PlasmaIceWaveDownRightCore, 1),
        ],
        [P.PlasmaWaveDownRight] =
        [
            Show(F.PlasmaWaveDownRightShortCore, 2),
            Show(F.PlasmaWaveDownRightShortSpread1, 2),
            Show(F.PlasmaWaveDownRightShortSpread2, 2),
            Show(F.PlasmaWaveDownRightShortSpread3, 2),
            Show(F.PlasmaWaveDownRightShortSpread4, 2),
            Show(F.PlasmaWaveDownRightShortSpread3Twin, 2),
            Show(F.PlasmaWaveDownRightShortSpread2Twin, 2),
            Show(F.PlasmaWaveDownRightShortSpread1Twin, 2),
        ],
        [P.ChargedPowerUpFacingRight] =
        [
            Show(F.ChargedPowerUpFacingRight, 1),
            Show(F.ChargedPowerUpFacingRightAlternate, 1),
        ],
        [P.ChargedPowerUpRight] =
        [
            Show(F.ChargedPowerUpRight, 1),
            Show(F.ChargedPowerUpRightAlternate, 1),
        ],
        [P.ChargedPowerRight] =
        [
            Show(F.ChargedPowerRight, 1),
            Show(F.ChargedPowerRightAlternate, 1),
        ],
        [P.ChargedPowerDownRight] =
        [
            Show(F.ChargedPowerDownRight, 1),
            Show(F.ChargedPowerDownRightAlternate, 1),
        ],
        [P.ChargedPowerDownFacingRight] =
        [
            Show(F.ChargedPowerDownFacingRight, 1),
            Show(F.ChargedPowerDownFacingRightAlternate, 1),
        ],
        [P.ChargedPowerDownLeft] =
        [
            Show(F.ChargedPowerDownLeft, 1),
            Show(F.ChargedPowerDownLeftAlternate, 1),
        ],
        [P.ChargedPowerLeft] =
        [
            Show(F.ChargedPowerLeft, 1),
            Show(F.ChargedPowerLeftAlternate, 1),
        ],
        [P.ChargedPowerUpLeft] =
        [
            Show(F.ChargedPowerUpLeft, 1),
            Show(F.ChargedPowerUpLeftAlternate, 1),
        ],
        [P.ChargedWaveUpFacingRight] =
        [
            Show(F.ChargedWaveUpFacingRightHidden, 3),
        ],
        [P.ChargedWaveDownFacingRight] =
        [
            Show(F.ChargedWaveDownFacingRightCore, 1),
            Show(F.ChargedWaveDownFacingRightCoreAlternate, 1),
            Show(F.ChargedWaveDownFacingRightSpread1, 1),
            Show(F.ChargedWaveDownFacingRightSpread1Alternate, 1),
            Show(F.ChargedWaveDownFacingRightSpread2, 1),
            Show(F.ChargedWaveDownFacingRightSpread2Alternate, 1),
            Show(F.ChargedWaveDownFacingRightSpread3, 1),
            Show(F.ChargedWaveDownFacingRightSpread3Alternate, 1),
            Show(F.ChargedWaveDownFacingRightSpread4, 1),
            Show(F.ChargedWaveDownFacingRightSpread4Alternate, 1),
            Show(F.ChargedWaveDownFacingRightSpread3Twin, 1),
            Show(F.ChargedWaveDownFacingRightSpread3AlternateTwin, 1),
            Show(F.ChargedWaveDownFacingRightSpread2Twin, 1),
            Show(F.ChargedWaveDownFacingRightSpread2AlternateTwin, 1),
            Show(F.ChargedWaveDownFacingRightSpread1AlternateTwin, 1),
            Show(F.ChargedWaveDownFacingRightSpread1Twin, 1),
        ],
        [P.ChargedWaveUpRight] =
        [
            Show(F.ChargedWaveUpRightCore, 1),
            Show(F.ChargedWaveUpRightCoreAlternate, 1),
            Show(F.ChargedWaveUpRightSpread1, 1),
            Show(F.ChargedWaveUpRightSpread1Alternate, 1),
            Show(F.ChargedWaveUpRightSpread2, 1),
            Show(F.ChargedWaveUpRightSpread2Alternate, 1),
            Show(F.ChargedWaveUpRightSpread3, 1),
            Show(F.ChargedWaveUpRightSpread3Alternate, 1),
            Show(F.ChargedWaveUpRightSpread4, 1),
            Show(F.ChargedWaveUpRightSpread4Alternate, 1),
            Show(F.ChargedWaveUpRightSpread3Twin, 1),
            Show(F.ChargedWaveUpRightSpread3AlternateTwin, 1),
            Show(F.ChargedWaveUpRightSpread2Twin, 1),
            Show(F.ChargedWaveUpRightSpread2AlternateTwin, 1),
            Show(F.ChargedWaveUpRightSpread1Twin, 1),
            Show(F.ChargedWaveUpRightSpread1AlternateTwin, 1),
        ],
        [P.ChargedWaveRight] =
        [
            Show(F.ChargedWaveRightCore, 1),
            Show(F.ChargedWaveRightCoreAlternate, 1),
            Show(F.ChargedWaveRightSpread1, 1),
            Show(F.ChargedWaveRightSpread1Alternate, 1),
            Show(F.ChargedWaveRightSpread2, 1),
            Show(F.ChargedWaveRightSpread2Alternate, 1),
            Show(F.ChargedWaveRightSpread3, 1),
            Show(F.ChargedWaveRightSpread3Alternate, 1),
            Show(F.ChargedWaveRightSpread4, 1),
            Show(F.ChargedWaveRightSpread4Alternate, 1),
            Show(F.ChargedWaveRightSpread3Twin, 1),
            Show(F.ChargedWaveRightSpread3AlternateTwin, 1),
            Show(F.ChargedWaveRightSpread2Twin, 1),
            Show(F.ChargedWaveRightSpread2AlternateTwin, 1),
            Show(F.ChargedWaveRightSpread1AlternateTwin, 1),
            Show(F.ChargedWaveRightSpread1Twin, 1),
        ],
        [P.ChargedWaveDownRight] =
        [
            Show(F.ChargedWaveDownRightCore, 1),
            Show(F.ChargedWaveDownRightCoreAlternate, 1),
            Show(F.ChargedWaveDownRightSpread1, 1),
            Show(F.ChargedWaveDownRightSpread1Alternate, 1),
            Show(F.ChargedWaveDownRightSpread2, 1),
            Show(F.ChargedWaveDownRightSpread2Alternate, 1),
            Show(F.ChargedWaveDownRightSpread3, 1),
            Show(F.ChargedWaveDownRightSpread3Alternate, 1),
            Show(F.ChargedWaveDownRightSpread4, 1),
            Show(F.ChargedWaveDownRightSpread4Alternate, 1),
            Show(F.ChargedWaveDownRightSpread3Twin, 1),
            Show(F.ChargedWaveDownRightSpread3AlternateTwin, 1),
            Show(F.ChargedWaveDownRightSpread2Twin, 1),
            Show(F.ChargedWaveDownRightSpread2AlternateTwin, 1),
            Show(F.ChargedWaveDownRightSpread1Twin, 1),
            Show(F.ChargedWaveDownRightSpread1AlternateTwin, 1),
        ],
        [P.ChargedIceUpFacingRight] =
        [
            Show(F.ChargedIceUpFacingRightPhase1, 1),
            Show(F.ChargedIceUpFacingRightPhase2, 1),
            Show(F.ChargedIceUpFacingRightPhase3, 1),
            Show(F.ChargedIceUpFacingRightPhase4, 1),
        ],
        [P.ChargedIceWaveUpFacingRight] =
        [
            Show(F.ChargedIceWaveUpFacingRightHidden, 3),
        ],
        [P.ChargedIceWaveDownFacingRight] =
        [
            Show(F.ChargedIceWaveDownFacingRightCore, 1),
            Show(F.ChargedIceWaveDownFacingRightCoreAlternate, 1),
            Show(F.ChargedIceWaveDownFacingRightSpread1, 1),
            Show(F.ChargedIceWaveDownFacingRightSpread1Alternate, 1),
            Show(F.ChargedIceWaveDownFacingRightSpread2, 1),
            Show(F.ChargedIceWaveDownFacingRightSpread2Alternate, 1),
            Show(F.ChargedIceWaveDownFacingRightSpread3, 1),
            Show(F.ChargedIceWaveDownFacingRightSpread3Alternate, 1),
            Show(F.ChargedIceWaveDownFacingRightSpread4, 1),
            Show(F.ChargedIceWaveDownFacingRightSpread4Alternate, 1),
            Show(F.ChargedIceWaveDownFacingRightSpread3Twin, 1),
            Show(F.ChargedIceWaveDownFacingRightSpread3AlternateTwin, 1),
            Show(F.ChargedIceWaveDownFacingRightSpread2Twin, 1),
            Show(F.ChargedIceWaveDownFacingRightSpread2AlternateTwin, 1),
            Show(F.ChargedIceWaveDownFacingRightSpread1AlternateTwin, 1),
            Show(F.ChargedIceWaveDownFacingRightSpread1Twin, 1),
        ],
        [P.ChargedIceWaveUpRight] =
        [
            Show(F.ChargedIceWaveUpRightCore, 1),
            Show(F.ChargedIceWaveUpRightCoreAlternate, 1),
            Show(F.ChargedIceWaveUpRightSpread1, 1),
            Show(F.ChargedIceWaveUpRightSpread1Alternate, 1),
            Show(F.ChargedIceWaveUpRightSpread2, 1),
            Show(F.ChargedIceWaveUpRightSpread2Alternate, 1),
            Show(F.ChargedIceWaveUpRightSpread3, 1),
            Show(F.ChargedIceWaveUpRightSpread3Alternate, 1),
            Show(F.ChargedIceWaveUpRightSpread4, 1),
            Show(F.ChargedIceWaveUpRightSpread4Alternate, 1),
            Show(F.ChargedIceWaveUpRightSpread3Twin, 1),
            Show(F.ChargedIceWaveUpRightSpread3AlternateTwin, 1),
            Show(F.ChargedIceWaveUpRightSpread2Twin, 1),
            Show(F.ChargedIceWaveUpRightSpread2AlternateTwin, 1),
            Show(F.ChargedIceWaveUpRightSpread1Twin, 1),
            Show(F.ChargedIceWaveUpRightSpread1AlternateTwin, 1),
        ],
        [P.ChargedIceWaveRight] =
        [
            Show(F.ChargedIceWaveRightCore, 1),
            Show(F.ChargedIceWaveRightCoreAlternate, 1),
            Show(F.ChargedIceWaveRightSpread1, 1),
            Show(F.ChargedIceWaveRightSpread1Alternate, 1),
            Show(F.ChargedIceWaveRightSpread2, 1),
            Show(F.ChargedIceWaveRightSpread2Alternate, 1),
            Show(F.ChargedIceWaveRightSpread3, 1),
            Show(F.ChargedIceWaveRightSpread3Alternate, 1),
            Show(F.ChargedIceWaveRightSpread4, 1),
            Show(F.ChargedIceWaveRightSpread4Alternate, 1),
            Show(F.ChargedIceWaveRightSpread3Twin, 1),
            Show(F.ChargedIceWaveRightSpread3AlternateTwin, 1),
            Show(F.ChargedIceWaveRightSpread2Twin, 1),
            Show(F.ChargedIceWaveRightSpread2AlternateTwin, 1),
            Show(F.ChargedIceWaveRightSpread1AlternateTwin, 1),
            Show(F.ChargedIceWaveRightSpread1Twin, 1),
        ],
        [P.ChargedIceWaveDownRight] =
        [
            Show(F.ChargedIceWaveDownRightCore, 1),
            Show(F.ChargedIceWaveDownRightCoreAlternate, 1),
            Show(F.ChargedIceWaveDownRightSpread1, 1),
            Show(F.ChargedIceWaveDownRightSpread1Alternate, 1),
            Show(F.ChargedIceWaveDownRightSpread2, 1),
            Show(F.ChargedIceWaveDownRightSpread2Alternate, 1),
            Show(F.ChargedIceWaveDownRightSpread3, 1),
            Show(F.ChargedIceWaveDownRightSpread3Alternate, 1),
            Show(F.ChargedIceWaveDownRightSpread4, 1),
            Show(F.ChargedIceWaveDownRightSpread4Alternate, 1),
            Show(F.ChargedIceWaveDownRightSpread3Twin, 1),
            Show(F.ChargedIceWaveDownRightSpread3AlternateTwin, 1),
            Show(F.ChargedIceWaveDownRightSpread2Twin, 1),
            Show(F.ChargedIceWaveDownRightSpread2AlternateTwin, 1),
            Show(F.ChargedIceWaveDownRightSpread1Twin, 1),
            Show(F.ChargedIceWaveDownRightSpread1AlternateTwin, 1),
        ],
        [P.ChargedSpazerUpFacingRight] =
        [
            Show(F.ChargedSpazerUpFacingRightStage1, 1),
            Show(F.ChargedSpazerUpFacingRightVerticalAlternateStage1, 1),
            Show(F.ChargedSpazerUpFacingRightStage2, 1),
            Show(F.ChargedSpazerUpFacingRightVerticalAlternateStage2, 1),
            Show(F.ChargedSpazerUpFacingRightSeed, 1),
            Show(F.ChargedSpazerUpFacingRightVerticalAlternateSeed, 1),
            Show(F.ChargedSpazerUpFacingRightSpread2, 1),
            Show(F.ChargedSpazerUpFacingRightVerticalAlternateSpread2, 1),
        ],
        [P.ChargedSpazerUpFacingRightLoop] =
        [
            Show(F.ChargedSpazerUpFacingRightSpread5, 1),
            Show(F.ChargedSpazerUpFacingRightVerticalAlternateSpread5, 1),
        ],
        [P.ChargedSpazerUpRight] =
        [
            Show(F.ChargedSpazerUpRightStage1, 1),
            Show(F.ChargedSpazerUpRightAlternateStage1, 1),
            Show(F.ChargedSpazerUpRightStage2, 1),
            Show(F.ChargedSpazerUpRightAlternateStage2, 1),
            Show(F.ChargedSpazerUpRightSeed, 1),
            Show(F.ChargedSpazerUpRightAlternateSeed, 1),
            Show(F.ChargedSpazerUpRightSpread2, 1),
            Show(F.ChargedSpazerUpRightAlternateSpread2, 1),
        ],
        [P.ChargedSpazerUpRightLoop] =
        [
            Show(F.ChargedSpazerUpRightSpread5, 1),
            Show(F.ChargedSpazerUpRightAlternateSpread5, 1),
        ],
        [P.ChargedSpazerRight] =
        [
            Show(F.ChargedSpazerRightStage1, 1),
            Show(F.ChargedSpazerRightHorizontalAlternateStage1, 1),
            Show(F.ChargedSpazerRightStage2, 1),
            Show(F.ChargedSpazerRightHorizontalAlternateStage2, 1),
            Show(F.ChargedSpazerRightLeftSeed, 1),
            Show(F.ChargedSpazerRightHorizontalAlternateSeed, 1),
            Show(F.ChargedSpazerRightLeftSpread2, 1),
            Show(F.ChargedSpazerRightHorizontalAlternateSpread2, 1),
        ],
        [P.ChargedSpazerRightLoop] =
        [
            Show(F.ChargedSpazerRightLeftSpread5, 1),
            Show(F.ChargedSpazerRightHorizontalAlternateSpread5, 1),
        ],
        [P.ChargedSpazerDownRight] =
        [
            Show(F.ChargedSpazerDownRightStage1, 1),
            Show(F.ChargedSpazerDownRightAlternateStage1, 1),
            Show(F.ChargedSpazerDownRightStage2, 1),
            Show(F.ChargedSpazerDownRightAlternateStage2, 1),
            Show(F.ChargedSpazerDownRightSeed, 1),
            Show(F.ChargedSpazerDownRightAlternateSeed, 1),
            Show(F.ChargedSpazerDownRightSpread2, 1),
            Show(F.ChargedSpazerDownRightAlternateSpread2, 1),
        ],
        [P.ChargedSpazerDownRightLoop] =
        [
            Show(F.ChargedSpazerDownRightSpread5, 1),
            Show(F.ChargedSpazerDownRightAlternateSpread5, 1),
        ],
        [P.ChargedSpazerWaveUpFacingRight] =
        [
            Show(F.ChargedSpazerWaveUpFacingRightStage1, 1),
            Show(F.ChargedSpazerWaveUpFacingRightVerticalAlternateStage1, 1),
            Show(F.ChargedSpazerWaveUpFacingRightStage2, 1),
            Show(F.ChargedSpazerWaveUpFacingRightVerticalAlternateStage2, 1),
        ],
        [P.ChargedSpazerWaveUpFacingRightLoop] =
        [
            Show(F.ChargedSpazerWaveUpFacingRightSeed, 1),
            Show(F.ChargedSpazerWaveUpFacingRightVerticalAlternateSeed, 1),
            Show(F.ChargedSpazerWaveUpFacingRightSpread1, 1),
            Show(F.ChargedSpazerWaveUpFacingRightVerticalAlternateSpread1, 1),
            Show(F.ChargedSpazerWaveUpFacingRightSpread2, 1),
            Show(F.ChargedSpazerWaveUpFacingRightVerticalAlternateSpread2, 1),
            Show(F.ChargedSpazerWaveUpFacingRightSpread3, 1),
            Show(F.ChargedSpazerWaveUpFacingRightVerticalAlternateSpread3, 1),
            Show(F.ChargedSpazerWaveUpFacingRightSpread4, 1),
            Show(F.ChargedSpazerWaveUpFacingRightVerticalAlternateSpread4, 1),
            Show(F.ChargedSpazerWaveUpFacingRightSpread5, 1),
            Show(F.ChargedSpazerWaveUpFacingRightVerticalAlternateSpread5, 1),
            Show(F.ChargedSpazerWaveUpFacingRightSpread4Twin, 1),
            Show(F.ChargedSpazerWaveUpFacingRightVerticalAlternateSpread4Twin, 1),
            Show(F.ChargedSpazerWaveUpFacingRightSpread3Twin, 1),
            Show(F.ChargedSpazerWaveUpFacingRightVerticalAlternateSpread3Twin, 1),
            Show(F.ChargedSpazerWaveUpFacingRightSpread2Twin, 1),
            Show(F.ChargedSpazerWaveUpFacingRightVerticalAlternateSpread2Twin, 1),
            Show(F.ChargedSpazerWaveUpFacingRightSpread1Twin, 1),
            Show(F.ChargedSpazerWaveUpFacingRightVerticalAlternateSpread1Twin, 1),
        ],
        [P.ChargedSpazerWaveUpRight] =
        [
            Show(F.ChargedSpazerWaveUpRightStage1, 1),
            Show(F.ChargedSpazerWaveUpRightAlternateStage1, 1),
            Show(F.ChargedSpazerWaveUpRightStage2, 1),
            Show(F.ChargedSpazerWaveUpRightAlternateStage2, 1),
        ],
        [P.ChargedSpazerWaveUpRightLoop] =
        [
            Show(F.ChargedSpazerWaveUpRightSeed, 1),
            Show(F.ChargedSpazerWaveUpRightAlternateSeed, 1),
            Show(F.ChargedSpazerWaveUpRightSpread1, 1),
            Show(F.ChargedSpazerWaveUpRightAlternateSpread1, 1),
            Show(F.ChargedSpazerWaveUpRightSpread2, 1),
            Show(F.ChargedSpazerWaveUpRightAlternateSpread2, 1),
            Show(F.ChargedSpazerWaveUpRightSpread3, 1),
            Show(F.ChargedSpazerWaveUpRightAlternateSpread3, 1),
            Show(F.ChargedSpazerWaveUpRightSpread4, 1),
            Show(F.ChargedSpazerWaveUpRightAlternateSpread4, 1),
            Show(F.ChargedSpazerWaveUpRightSpread5, 1),
            Show(F.ChargedSpazerWaveUpRightAlternateSpread5, 1),
            Show(F.ChargedSpazerWaveUpRightSpread4Twin, 1),
            Show(F.ChargedSpazerWaveUpRightAlternateSpread4Twin, 1),
            Show(F.ChargedSpazerWaveUpRightSpread3Twin, 1),
            Show(F.ChargedSpazerWaveUpRightAlternateSpread3Twin, 1),
            Show(F.ChargedSpazerWaveUpRightSpread2Twin, 1),
            Show(F.ChargedSpazerWaveUpRightAlternateSpread2Twin, 1),
            Show(F.ChargedSpazerWaveUpRightSpread1Twin, 1),
            Show(F.ChargedSpazerWaveUpRightAlternateSpread1Twin, 1),
        ],
        [P.ChargedSpazerWaveRight] =
        [
            Show(F.ChargedSpazerWaveRightStage1, 1),
            Show(F.ChargedSpazerWaveRightHorizontalAlternateStage1, 1),
            Show(F.ChargedSpazerWaveRightStage2, 1),
            Show(F.ChargedSpazerWaveRightHorizontalAlternateStage2, 1),
        ],
        [P.ChargedSpazerWaveRightLoop] =
        [
            Show(F.ChargedSpazerWaveRightSeed, 1),
            Show(F.ChargedSpazerWaveRightHorizontalAlternateSeed, 1),
            Show(F.ChargedSpazerWaveRightSpread1, 1),
            Show(F.ChargedSpazerWaveRightHorizontalAlternateSpread1, 1),
            Show(F.ChargedSpazerWaveRightSpread2, 1),
            Show(F.ChargedSpazerWaveRightHorizontalAlternateSpread2, 1),
            Show(F.ChargedSpazerWaveRightSpread3, 1),
            Show(F.ChargedSpazerWaveRightHorizontalAlternateSpread3, 1),
            Show(F.ChargedSpazerWaveRightSpread4, 1),
            Show(F.ChargedSpazerWaveRightHorizontalAlternateSpread4, 1),
            Show(F.ChargedSpazerWaveRightSpread5, 1),
            Show(F.ChargedSpazerWaveRightHorizontalAlternateSpread5, 1),
            Show(F.ChargedSpazerWaveRightSpread4Twin, 1),
            Show(F.ChargedSpazerWaveRightHorizontalAlternateSpread4Twin, 1),
            Show(F.ChargedSpazerWaveRightSpread3Twin, 1),
            Show(F.ChargedSpazerWaveRightHorizontalAlternateSpread3Twin, 1),
            Show(F.ChargedSpazerWaveRightSpread2Twin, 1),
            Show(F.ChargedSpazerWaveRightHorizontalAlternateSpread2Twin, 1),
            Show(F.ChargedSpazerWaveRightSpread1Twin, 1),
            Show(F.ChargedSpazerWaveRightHorizontalAlternateSpread1Twin, 1),
        ],
        [P.ChargedSpazerWaveDownRight] =
        [
            Show(F.ChargedSpazerWaveDownRightStage1, 1),
            Show(F.ChargedSpazerWaveDownRightAlternateStage1, 1),
            Show(F.ChargedSpazerWaveDownRightStage2, 1),
            Show(F.ChargedSpazerWaveDownRightAlternateStage2, 1),
        ],
        [P.ChargedSpazerWaveDownRightLoop] =
        [
            Show(F.ChargedSpazerWaveDownRightSeed, 1),
            Show(F.ChargedSpazerWaveDownRightAlternateSeed, 1),
            Show(F.ChargedSpazerWaveDownRightSpread1, 1),
            Show(F.ChargedSpazerWaveDownRightAlternateSpread1, 1),
            Show(F.ChargedSpazerWaveDownRightSpread2, 1),
            Show(F.ChargedSpazerWaveDownRightAlternateSpread2, 1),
            Show(F.ChargedSpazerWaveDownRightSpread3, 1),
            Show(F.ChargedSpazerWaveDownRightAlternateSpread3, 1),
            Show(F.ChargedSpazerWaveDownRightSpread4, 1),
            Show(F.ChargedSpazerWaveDownRightAlternateSpread4, 1),
            Show(F.ChargedSpazerWaveDownRightSpread5, 1),
            Show(F.ChargedSpazerWaveDownRightAlternateSpread5, 1),
            Show(F.ChargedSpazerWaveDownRightSpread4Twin, 1),
            Show(F.ChargedSpazerWaveDownRightAlternateSpread4Twin, 1),
            Show(F.ChargedSpazerWaveDownRightSpread3Twin, 1),
            Show(F.ChargedSpazerWaveDownRightAlternateSpread3Twin, 1),
            Show(F.ChargedSpazerWaveDownRightSpread2Twin, 1),
            Show(F.ChargedSpazerWaveDownRightAlternateSpread2Twin, 1),
            Show(F.ChargedSpazerWaveDownRightSpread1Twin, 1),
            Show(F.ChargedSpazerWaveDownRightAlternateSpread1Twin, 1),
        ],
        [P.ChargedSpazerWaveDownFacingRight] =
        [
            Show(F.ChargedSpazerWaveDownFacingRightStage1, 1),
            Show(F.ChargedSpazerWaveDownFacingRightVerticalAlternateStage1, 1),
            Show(F.ChargedSpazerWaveDownFacingRightStage2, 1),
            Show(F.ChargedSpazerWaveDownFacingRightVerticalAlternateStage2, 1),
        ],
        [P.ChargedSpazerWaveDownFacingRightLoop] =
        [
            Show(F.ChargedSpazerWaveDownFacingRightSeed, 1),
            Show(F.ChargedSpazerWaveDownFacingRightVerticalAlternateSeed, 1),
            Show(F.ChargedSpazerWaveDownFacingRightSpread1, 1),
            Show(F.ChargedSpazerWaveDownFacingRightVerticalAlternateSpread1, 1),
            Show(F.ChargedSpazerWaveDownFacingRightSpread2, 1),
            Show(F.ChargedSpazerWaveDownFacingRightVerticalAlternateSpread2, 1),
            Show(F.ChargedSpazerWaveDownFacingRightSpread3, 1),
            Show(F.ChargedSpazerWaveDownFacingRightVerticalAlternateSpread3, 1),
            Show(F.ChargedSpazerWaveDownFacingRightSpread4, 1),
            Show(F.ChargedSpazerWaveDownFacingRightVerticalAlternateSpread4, 1),
            Show(F.ChargedSpazerWaveDownFacingRightSpread5, 1),
            Show(F.ChargedSpazerWaveDownFacingRightVerticalAlternateSpread5, 1),
            Show(F.ChargedSpazerWaveDownFacingRightSpread4Twin, 1),
            Show(F.ChargedSpazerWaveDownFacingRightVerticalAlternateSpread4Twin, 1),
            Show(F.ChargedSpazerWaveDownFacingRightSpread3Twin, 1),
            Show(F.ChargedSpazerWaveDownFacingRightVerticalAlternateSpread3Twin, 1),
            Show(F.ChargedSpazerWaveDownFacingRightSpread2Twin, 1),
            Show(F.ChargedSpazerWaveDownFacingRightVerticalAlternateSpread2Twin, 1),
            Show(F.ChargedSpazerWaveDownFacingRightSpread1Twin, 1),
            Show(F.ChargedSpazerWaveDownFacingRightVerticalAlternateSpread1Twin, 1),
        ],
        [P.ChargedSpazerWaveDownLeft] =
        [
            Show(F.ChargedSpazerWaveDownLeftUpRightStage1, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightAlternateStage1, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightStage2, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightAlternateStage2, 1),
        ],
        [P.ChargedSpazerWaveDownLeftLoop] =
        [
            Show(F.ChargedSpazerWaveDownLeftSeed, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightAlternateSeed, 1),
            Show(F.ChargedSpazerWaveDownLeftSpread1, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightAlternateSpread1, 1),
            Show(F.ChargedSpazerWaveDownLeftSpread2, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightAlternateSpread2, 1),
            Show(F.ChargedSpazerWaveDownLeftSpread3, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightAlternateSpread3, 1),
            Show(F.ChargedSpazerWaveDownLeftSpread4, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightAlternateSpread4, 1),
            Show(F.ChargedSpazerWaveDownLeftSpread5, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightAlternateSpread5, 1),
            Show(F.ChargedSpazerWaveDownLeftSpread4Twin, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightAlternateSpread4Twin, 1),
            Show(F.ChargedSpazerWaveDownLeftSpread3Twin, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightAlternateSpread3Twin, 1),
            Show(F.ChargedSpazerWaveDownLeftSpread2Twin, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightAlternateSpread2Twin, 1),
            Show(F.ChargedSpazerWaveDownLeftSpread1Twin, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightAlternateSpread1Twin, 1),
        ],
        [P.ChargedSpazerWaveLeft] =
        [
            Show(F.ChargedSpazerWaveLeftStage1, 1),
            Show(F.ChargedSpazerWaveLeftHorizontalAlternateStage1, 1),
            Show(F.ChargedSpazerWaveLeftStage2, 1),
            Show(F.ChargedSpazerWaveLeftHorizontalAlternateStage2, 1),
        ],
        [P.ChargedSpazerWaveLeftLoop] =
        [
            Show(F.ChargedSpazerWaveLeftSeed, 1),
            Show(F.ChargedSpazerWaveLeftHorizontalAlternateSeed, 1),
            Show(F.ChargedSpazerWaveLeftSpread1, 1),
            Show(F.ChargedSpazerWaveLeftHorizontalAlternateSpread1, 1),
            Show(F.ChargedSpazerWaveLeftSpread2, 1),
            Show(F.ChargedSpazerWaveLeftHorizontalAlternateSpread2, 1),
            Show(F.ChargedSpazerWaveLeftSpread3, 1),
            Show(F.ChargedSpazerWaveLeftHorizontalAlternateSpread3, 1),
            Show(F.ChargedSpazerWaveLeftSpread4, 1),
            Show(F.ChargedSpazerWaveLeftHorizontalAlternateSpread4, 1),
            Show(F.ChargedSpazerWaveLeftSpread5, 1),
            Show(F.ChargedSpazerWaveLeftHorizontalAlternateSpread5, 1),
            Show(F.ChargedSpazerWaveLeftSpread4Twin, 1),
            Show(F.ChargedSpazerWaveLeftHorizontalAlternateSpread4Twin, 1),
            Show(F.ChargedSpazerWaveLeftSpread3Twin, 1),
            Show(F.ChargedSpazerWaveLeftHorizontalAlternateSpread3Twin, 1),
            Show(F.ChargedSpazerWaveLeftSpread2Twin, 1),
            Show(F.ChargedSpazerWaveLeftHorizontalAlternateSpread2Twin, 1),
            Show(F.ChargedSpazerWaveLeftSpread1Twin, 1),
            Show(F.ChargedSpazerWaveLeftHorizontalAlternateSpread1Twin, 1),
        ],
        [P.ChargedSpazerWaveUpLeft] =
        [
            Show(F.ChargedSpazerWaveUpLeftStage1, 1),
            Show(F.ChargedSpazerWaveUpLeftDownRightAlternateStage1, 1),
            Show(F.ChargedSpazerWaveUpLeftStage2, 1),
            Show(F.ChargedSpazerWaveUpLeftDownRightAlternateStage2, 1),
        ],
        [P.ChargedSpazerWaveUpLeftLoop] =
        [
            Show(F.ChargedSpazerWaveUpLeftSeed, 1),
            Show(F.ChargedSpazerWaveUpLeftDownRightAlternateSeed, 1),
            Show(F.ChargedSpazerWaveUpLeftSpread1, 1),
            Show(F.ChargedSpazerWaveUpLeftDownRightAlternateSpread1, 1),
            Show(F.ChargedSpazerWaveUpLeftSpread2, 1),
            Show(F.ChargedSpazerWaveUpLeftDownRightAlternateSpread2, 1),
            Show(F.ChargedSpazerWaveUpLeftSpread3, 1),
            Show(F.ChargedSpazerWaveUpLeftDownRightAlternateSpread3, 1),
            Show(F.ChargedSpazerWaveUpLeftSpread4, 1),
            Show(F.ChargedSpazerWaveUpLeftDownRightAlternateSpread4, 1),
            Show(F.ChargedSpazerWaveUpLeftSpread5, 1),
            Show(F.ChargedSpazerWaveUpLeftDownRightAlternateSpread5, 1),
            Show(F.ChargedSpazerWaveUpLeftSpread4Twin, 1),
            Show(F.ChargedSpazerWaveUpLeftDownRightAlternateSpread4Twin, 1),
            Show(F.ChargedSpazerWaveUpLeftSpread3Twin, 1),
            Show(F.ChargedSpazerWaveUpLeftDownRightAlternateSpread3Twin, 1),
            Show(F.ChargedSpazerWaveUpLeftSpread2Twin, 1),
            Show(F.ChargedSpazerWaveUpLeftDownRightAlternateSpread2Twin, 1),
            Show(F.ChargedSpazerWaveUpLeftSpread1Twin, 1),
            Show(F.ChargedSpazerWaveUpLeftDownRightAlternateSpread1Twin, 1),
        ],
        [P.ChargedPlasmaUpFacingRight] =
        [
            Show(F.ChargedPlasmaUpFacingRightCore, 1),
            Show(F.ChargedPlasmaUpFacingRightAlternateCore, 1),
            Show(F.ChargedPlasmaUpFacingRightShort, 1),
            Show(F.ChargedPlasmaUpFacingRightAlternateShort, 1),
            Show(F.ChargedPlasmaUpFacingRightLong, 1),
            Show(F.ChargedPlasmaUpFacingRightAlternateLong, 1),
        ],
        [P.ChargedPlasmaUpFacingRightLoop] =
        [
            Show(F.ChargedPlasmaUpFacingRightFull, 1),
            Show(F.ChargedPlasmaUpFacingRightAlternateFull, 1),
        ],
        [P.ChargedPlasmaUpRight] =
        [
            Show(F.ChargedPlasmaUpRightCore, 1),
            Show(F.ChargedPlasmaUpRightAlternateCore, 1),
            Show(F.ChargedPlasmaUpRightShort, 1),
            Show(F.ChargedPlasmaUpRightAlternateShort, 1),
            Show(F.ChargedPlasmaUpRightLong, 1),
            Show(F.ChargedPlasmaUpRightAlternateLong, 1),
        ],
        [P.ChargedPlasmaUpRightLoop] =
        [
            Show(F.ChargedPlasmaUpRightFull, 1),
            Show(F.ChargedPlasmaUpRightAlternateFull, 1),
        ],
        [P.ChargedPlasmaRight] =
        [
            Show(F.ChargedPlasmaRightCore, 1),
            Show(F.ChargedPlasmaRightAlternateCore, 1),
            Show(F.ChargedPlasmaRightShort, 1),
            Show(F.ChargedPlasmaRightAlternateShort, 1),
            Show(F.ChargedPlasmaRightLong, 1),
            Show(F.ChargedPlasmaRightAlternateLong, 1),
        ],
        [P.ChargedPlasmaRightLoop] =
        [
            Show(F.ChargedPlasmaRightFull, 1),
            Show(F.ChargedPlasmaRightAlternateFull, 1),
        ],
        [P.ChargedPlasmaDownRight] =
        [
            Show(F.ChargedPlasmaDownRightCore, 1),
            Show(F.ChargedPlasmaDownRightAlternateCore, 1),
            Show(F.ChargedPlasmaDownRightShort, 1),
            Show(F.ChargedPlasmaDownRightAlternateShort, 1),
            Show(F.ChargedPlasmaDownRightLong, 1),
            Show(F.ChargedPlasmaDownRightAlternateLong, 1),
        ],
        [P.ChargedPlasmaDownRightLoop] =
        [
            Show(F.ChargedPlasmaDownRightFull, 1),
            Show(F.ChargedPlasmaDownRightAlternateFull, 1),
        ],
        [P.ChargedPlasmaWaveUpFacingRight] =
        [
            Show(F.ChargedPlasmaWaveUpFacingRightCore, 1),
            Show(F.ChargedPlasmaWaveUpFacingRightAlternateCore, 1),
            Show(F.ChargedPlasmaWaveUpFacingRightShort, 1),
            Show(F.ChargedPlasmaWaveUpFacingRightAlternateShort, 1),
            Show(F.ChargedPlasmaWaveUpFacingRightLong, 1),
            Show(F.ChargedPlasmaWaveUpFacingRightAlternateLong, 1),
        ],
        [P.ChargedPlasmaWaveUpFacingRightLoop] =
        [
            Show(F.ChargedPlasmaWaveUpFacingRightLongCore, 1),
            Show(F.ChargedPlasmaWaveUpFacingRightAlternateCoreTwin, 1),
            Show(F.ChargedPlasmaWaveUpFacingRightLongSpread1, 1),
            Show(F.ChargedPlasmaWaveUpFacingRightAlternateSpread1, 1),
            Show(F.ChargedPlasmaWaveUpFacingRightLongSpread2, 1),
            Show(F.ChargedPlasmaWaveUpFacingRightAlternateSpread2, 1),
            Show(F.ChargedPlasmaWaveUpFacingRightLongSpread3, 1),
            Show(F.ChargedPlasmaWaveUpFacingRightAlternateSpread3, 1),
            Show(F.ChargedPlasmaWaveUpFacingRightLongSpread4, 1),
            Show(F.ChargedPlasmaWaveUpFacingRightAlternateSpread4, 1),
            Show(F.ChargedPlasmaWaveUpFacingRightLongSpread3Twin, 1),
            Show(F.ChargedPlasmaWaveUpFacingRightAlternateSpread3Twin, 1),
            Show(F.ChargedPlasmaWaveUpFacingRightLongSpread2Twin, 1),
            Show(F.ChargedPlasmaWaveUpFacingRightAlternateSpread2Twin, 1),
            Show(F.ChargedPlasmaWaveUpFacingRightLongSpread1Twin, 1),
            Show(F.ChargedPlasmaWaveUpFacingRightAlternateSpread1Twin, 1),
        ],
        [P.ChargedPlasmaWaveUpRight] =
        [
            Show(F.ChargedPlasmaWaveUpRightCore, 1),
            Show(F.ChargedPlasmaWaveUpRightAlternateCore, 1),
            Show(F.ChargedPlasmaWaveUpRightShort, 1),
            Show(F.ChargedPlasmaWaveUpRightAlternateShort, 1),
            Show(F.ChargedPlasmaWaveUpRightLong, 1),
            Show(F.ChargedPlasmaWaveUpRightAlternateLong, 1),
        ],
        [P.ChargedPlasmaWaveUpRightLoop] =
        [
            Show(F.ChargedPlasmaWaveUpRightLongCore, 1),
            Show(F.ChargedPlasmaWaveUpRightAlternateCoreTwin, 1),
            Show(F.ChargedPlasmaWaveUpRightLongSpread1, 1),
            Show(F.ChargedPlasmaWaveUpRightAlternateSpread1, 1),
            Show(F.ChargedPlasmaWaveUpRightLongSpread2, 1),
            Show(F.ChargedPlasmaWaveUpRightAlternateSpread2, 1),
            Show(F.ChargedPlasmaWaveUpRightLongSpread3, 1),
            Show(F.ChargedPlasmaWaveUpRightAlternateSpread3, 1),
            Show(F.ChargedPlasmaWaveUpRightLongSpread4, 1),
            Show(F.ChargedPlasmaWaveUpRightAlternateSpread4, 1),
            Show(F.ChargedPlasmaWaveUpRightLongSpread3Twin, 1),
            Show(F.ChargedPlasmaWaveUpRightAlternateSpread3Twin, 1),
            Show(F.ChargedPlasmaWaveUpRightLongSpread2Twin, 1),
            Show(F.ChargedPlasmaWaveUpRightAlternateSpread2Twin, 1),
            Show(F.ChargedPlasmaWaveUpRightLongSpread1Twin, 1),
            Show(F.ChargedPlasmaWaveUpRightAlternateSpread1Twin, 1),
        ],
        [P.ChargedPlasmaWaveRight] =
        [
            Show(F.ChargedPlasmaWaveRightCore, 1),
            Show(F.ChargedPlasmaWaveRightAlternateCore, 1),
            Show(F.ChargedPlasmaWaveRightShort, 1),
            Show(F.ChargedPlasmaWaveRightAlternateShort, 1),
            Show(F.ChargedPlasmaWaveRightLong, 1),
            Show(F.ChargedPlasmaWaveRightAlternateLong, 1),
        ],
        [P.ChargedPlasmaWaveRightLoop] =
        [
            Show(F.ChargedPlasmaWaveRightLongCore, 1),
            Show(F.ChargedPlasmaWaveRightAlternateCoreTwin, 1),
            Show(F.ChargedPlasmaWaveRightLongSpread1, 1),
            Show(F.ChargedPlasmaWaveRightAlternateSpread1, 1),
            Show(F.ChargedPlasmaWaveRightLongSpread2, 1),
            Show(F.ChargedPlasmaWaveRightAlternateSpread2, 1),
            Show(F.ChargedPlasmaWaveRightLongSpread3, 1),
            Show(F.ChargedPlasmaWaveRightAlternateSpread3, 1),
            Show(F.ChargedPlasmaWaveRightLongSpread4, 1),
            Show(F.ChargedPlasmaWaveRightAlternateSpread4, 1),
            Show(F.ChargedPlasmaWaveRightLongSpread3Twin, 1),
            Show(F.ChargedPlasmaWaveRightAlternateSpread3Twin, 1),
            Show(F.ChargedPlasmaWaveRightLongSpread2Twin, 1),
            Show(F.ChargedPlasmaWaveRightAlternateSpread2Twin, 1),
            Show(F.ChargedPlasmaWaveRightLongSpread1Twin, 1),
            Show(F.ChargedPlasmaWaveRightAlternateSpread1Twin, 1),
        ],
        [P.ChargedPlasmaWaveDownRight] =
        [
            Show(F.ChargedPlasmaWaveDownRightCore, 1),
            Show(F.ChargedPlasmaWaveDownRightAlternateCore, 1),
            Show(F.ChargedPlasmaWaveDownRightShort, 1),
            Show(F.ChargedPlasmaWaveDownRightAlternateShort, 1),
            Show(F.ChargedPlasmaWaveDownRightLong, 1),
            Show(F.ChargedPlasmaWaveDownRightAlternateLong, 1),
        ],
        [P.ChargedPlasmaWaveDownRightLoop] =
        [
            Show(F.ChargedPlasmaWaveDownRightLongCore, 1),
            Show(F.ChargedPlasmaWaveDownRightAlternateCoreTwin, 1),
            Show(F.ChargedPlasmaWaveDownRightLongSpread1, 1),
            Show(F.ChargedPlasmaWaveDownRightAlternateSpread1, 1),
            Show(F.ChargedPlasmaWaveDownRightLongSpread2, 1),
            Show(F.ChargedPlasmaWaveDownRightAlternateSpread2, 1),
            Show(F.ChargedPlasmaWaveDownRightLongSpread3, 1),
            Show(F.ChargedPlasmaWaveDownRightAlternateSpread3, 1),
            Show(F.ChargedPlasmaWaveDownRightLongSpread4, 1),
            Show(F.ChargedPlasmaWaveDownRightAlternateSpread4, 1),
            Show(F.ChargedPlasmaWaveDownRightLongSpread3Twin, 1),
            Show(F.ChargedPlasmaWaveDownRightAlternateSpread3Twin, 1),
            Show(F.ChargedPlasmaWaveDownRightLongSpread2Twin, 1),
            Show(F.ChargedPlasmaWaveDownRightAlternateSpread2Twin, 1),
            Show(F.ChargedPlasmaWaveDownRightLongSpread1Twin, 1),
            Show(F.ChargedPlasmaWaveDownRightAlternateSpread1Twin, 1),
        ],
        [P.MissileUpFacingRight] =
        [
            Show(F.MissileUpFacingRight, 15),
        ],
        [P.MissileUpRight] =
        [
            Show(F.MissileUpRight, 15),
        ],
        [P.MissileRight] =
        [
            Show(F.MissileRight, 15),
        ],
        [P.MissileDownRight] =
        [
            Show(F.MissileDownRight, 15),
        ],
        [P.MissileDownFacingRight] =
        [
            Show(F.MissileDownFacingRight, 15),
        ],
        [P.MissileDownLeft] =
        [
            Show(F.MissileDownLeft, 15),
        ],
        [P.MissileLeft] =
        [
            Show(F.MissileLeft, 15),
        ],
        [P.MissileUpLeft] =
        [
            Show(F.MissileUpLeft, 15),
        ],
        [P.SuperMissileUpFacingRight] =
        [
            Show(F.SuperMissileUpFacingRight, 15),
        ],
        [P.SuperMissileUpRight] =
        [
            Show(F.SuperMissileUpRight, 15),
        ],
        [P.SuperMissileRight] =
        [
            Show(F.SuperMissileRight, 15),
        ],
        [P.SuperMissileDownRight] =
        [
            Show(F.SuperMissileDownRight, 15),
        ],
        [P.SuperMissileDownFacingRight] =
        [
            Show(F.SuperMissileDownFacingRight, 15),
        ],
        [P.SuperMissileDownLeft] =
        [
            Show(F.SuperMissileDownLeft, 15),
        ],
        [P.SuperMissileLeft] =
        [
            Show(F.SuperMissileLeft, 15),
        ],
        [P.SuperMissileUpLeft] =
        [
            Show(F.SuperMissileUpLeft, 15),
        ],
        [P.SuperMissileLink] =
        [
            Show(F.SuperMissileLinkHidden, 15),
        ],
        [P.PowerBomb] =
        [
            Show(F.PowerBombPhase1, 5),
            Show(F.PowerBombPhase2, 5),
            Show(F.PowerBombPhase3, 5),
        ],
        [P.FastPowerBomb] =
        [
            Show(F.FastPowerBombPhase1, 1),
            Show(F.FastPowerBombPhase2, 1),
            Show(F.FastPowerBombPhase3, 1),
        ],
        [P.Bomb] =
        [
            Show(F.BombPhase1, 5),
            Show(F.BombPhase2, 5),
            Show(F.BombPhase3, 5),
            Show(F.BombPhase4, 5),
        ],
        [P.FastBomb] =
        [
            Show(F.FastBombPhase1, 1),
            Show(F.FastBombPhase2, 1),
            Show(F.FastBombPhase3, 1),
            Show(F.FastBombPhase4, 1),
        ],
        [P.BeamExplosion] =
        [
            Show(F.BeamExplosionStage1, 3),
            Show(F.BeamExplosionStage2, 3),
            Show(F.BeamExplosionStage3, 3),
            Show(F.BeamExplosionStage4, 3),
            Show(F.BeamExplosionStage5, 3),
            Show(F.BeamExplosionStage6, 3),
        ],
        [P.MissileExplosion] =
        [
            Show(F.MissileExplosionStage1, 3),
            Show(F.MissileExplosionStage2, 3),
            Show(F.MissileExplosionStage3, 3),
            Show(F.MissileExplosionStage4, 3),
            Show(F.MissileExplosionStage5, 3),
            Show(F.MissileExplosionStage6, 3),
        ],
        [P.BombExplosion] =
        [
            Show(F.BombExplosionStage1, 2),
            Show(F.BombExplosionStage2, 2),
            Show(F.BombExplosionStage3, 2),
            Show(F.BombExplosionStage4, 2),
            Show(F.BombExplosionStage5, 2),
        ],
        [P.PlasmaSpecialAttack] =
        [
            Show(F.PlasmaSpecialAttackStage1, 2),
            Show(F.PlasmaSpecialAttackStage2, 2),
            Show(F.PlasmaSpecialAttackStage3, 2),
            Show(F.PlasmaSpecialAttackStage4, 2),
            Show(F.PlasmaSpecialAttackStage5, 2),
        ],
        [P.SuperMissileExplosion] =
        [
            Show(F.SuperMissileExplosionStage1, 5),
            Show(F.SuperMissileExplosionStage2, 5),
            Show(F.SuperMissileExplosionStage3, 5),
            Show(F.SuperMissileExplosionStage4, 5),
            Show(F.SuperMissileExplosionStage5, 5),
            Show(F.SuperMissileExplosionStage6, 5),
        ],
        [P.ShinesparkEchoUpFacingRight] =
        [
            Show(F.ShinesparkEchoUpFacingRightHidden, 2),
            Show(F.ShinesparkEchoUpFacingRightHiddenTwin, 2),
            Show(F.ShinesparkEchoUpFacingRightHiddenTwinB, 2),
            Show(F.ShinesparkEchoUpFacingRightHiddenTwinC, 2),
        ],
        [P.SpazerSpecialAttackTrailUpFacingRight] =
        [
            Show(F.SpazerSpecialAttackTrailUpFacingRightHidden, 2),
            Show(F.SpazerSpecialAttackTrailUpFacingRightHiddenTwin, 2),
        ],
        [P.SpazerSpecialAttackTrailUpFacingRightLoop] =
        [
            Show(F.SpazerSpecialAttackTrailUpFacingRightHiddenTwinB, 2),
        ],
        [P.WaveSpecialAttack] =
        [
            Show(F.WaveSpecialAttackCore, 8),
            Show(F.WaveSpecialAttackCoreAlternate, 8),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<SamusProjectileSegment, Animation<SamusProjectileSegment>> StartingAt { get; } = new Dictionary<SamusProjectileSegment, Animation<SamusProjectileSegment>>
    {
        [P.PowerUpFacingRight] = new([new(P.PowerUpFacingRight, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.PowerUpRight] = new([new(P.PowerUpRight, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.PowerRight] = new([new(P.PowerRight, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.PowerDownRight] = new([new(P.PowerDownRight, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.PowerDownFacingRight] = new([new(P.PowerDownFacingRight, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.PowerDownLeft] = new([new(P.PowerDownLeft, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.PowerLeft] = new([new(P.PowerLeft, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.PowerUpLeft] = new([new(P.PowerUpLeft, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.WaveUpFacingRight] = new([new(P.WaveUpFacingRight, 0, 1, 1), new(P.WaveDownFacingRight, 0, 16, 1)], AnimationEnd.LoopsFrom(1)),
        [P.WaveDownFacingRight] = new([new(P.WaveDownFacingRight, 0, 16, 1)], AnimationEnd.LoopsFrom(0)),
        [P.WaveUpRight] = new([new(P.WaveUpRight, 0, 16, 1)], AnimationEnd.LoopsFrom(0)),
        [P.WaveRight] = new([new(P.WaveRight, 0, 16, 1)], AnimationEnd.LoopsFrom(0)),
        [P.WaveDownRight] = new([new(P.WaveDownRight, 0, 16, 1)], AnimationEnd.LoopsFrom(0)),
        [P.IceUpFacingRight] = new([new(P.IceUpFacingRight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SpazerUpFacingRight] = new([new(P.SpazerUpFacingRight, 0, 2, 1), new(P.SpazerUpFacingRightLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SpazerUpFacingRightLoop] = new([new(P.SpazerUpFacingRightLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SpazerUpRight] = new([new(P.SpazerUpRight, 0, 2, 1), new(P.SpazerUpRightLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SpazerUpRightLoop] = new([new(P.SpazerUpRightLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SpazerRight] = new([new(P.SpazerRight, 0, 2, 1), new(P.SpazerRightLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SpazerRightLoop] = new([new(P.SpazerRightLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SpazerDownRight] = new([new(P.SpazerDownRight, 0, 2, 1), new(P.SpazerDownRightLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SpazerDownRightLoop] = new([new(P.SpazerDownRightLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SpazerDownFacingRight] = new([new(P.SpazerDownFacingRight, 0, 2, 1), new(P.SpazerDownFacingRightLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SpazerDownFacingRightLoop] = new([new(P.SpazerDownFacingRightLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SpazerDownLeft] = new([new(P.SpazerDownLeft, 0, 2, 1), new(P.SpazerDownLeftLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SpazerDownLeftLoop] = new([new(P.SpazerDownLeftLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SpazerLeft] = new([new(P.SpazerLeft, 0, 2, 1), new(P.SpazerLeftLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SpazerLeftLoop] = new([new(P.SpazerLeftLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SpazerUpLeft] = new([new(P.SpazerUpLeft, 0, 2, 1), new(P.SpazerUpLeftLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SpazerUpLeftLoop] = new([new(P.SpazerUpLeftLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SpazerWaveUpFacingRight] = new([new(P.SpazerWaveUpFacingRight, 0, 10, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SpazerWaveUpRight] = new([new(P.SpazerWaveUpRight, 0, 10, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SpazerWaveRight] = new([new(P.SpazerWaveRight, 0, 10, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SpazerWaveDownRight] = new([new(P.SpazerWaveDownRight, 0, 10, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SpazerWaveDownFacingRight] = new([new(P.SpazerWaveDownFacingRight, 0, 10, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SpazerWaveDownLeft] = new([new(P.SpazerWaveDownLeft, 0, 10, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SpazerWaveLeft] = new([new(P.SpazerWaveLeft, 0, 10, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SpazerWaveUpLeft] = new([new(P.SpazerWaveUpLeft, 0, 10, 1)], AnimationEnd.LoopsFrom(0)),
        [P.PlasmaUpFacingRight] = new([new(P.PlasmaUpFacingRight, 0, 1, 1), new(P.PlasmaUpFacingRightLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.PlasmaUpFacingRightLoop] = new([new(P.PlasmaUpFacingRightLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.PlasmaUpRight] = new([new(P.PlasmaUpRight, 0, 1, 1), new(P.PlasmaUpRightLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.PlasmaUpRightLoop] = new([new(P.PlasmaUpRightLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.PlasmaRight] = new([new(P.PlasmaRight, 0, 1, 1), new(P.PlasmaRightLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.PlasmaRightLoop] = new([new(P.PlasmaRightLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.PlasmaDownRight] = new([new(P.PlasmaDownRight, 0, 1, 1), new(P.PlasmaDownRightLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.PlasmaDownRightLoop] = new([new(P.PlasmaDownRightLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.PlasmaIceWaveUpFacingRight] = new([new(P.PlasmaIceWaveUpFacingRight, 0, 1, 1), new(P.PlasmaWaveUpFacingRight, 0, 8, 1)], AnimationEnd.LoopsFrom(1)),
        [P.PlasmaWaveUpFacingRight] = new([new(P.PlasmaWaveUpFacingRight, 0, 8, 1)], AnimationEnd.LoopsFrom(0)),
        [P.PlasmaIceWaveUpRight] = new([new(P.PlasmaIceWaveUpRight, 0, 1, 1), new(P.PlasmaWaveUpRight, 0, 8, 1)], AnimationEnd.LoopsFrom(1)),
        [P.PlasmaWaveUpRight] = new([new(P.PlasmaWaveUpRight, 0, 8, 1)], AnimationEnd.LoopsFrom(0)),
        [P.PlasmaWaveRight] = new([new(P.PlasmaWaveRight, 0, 1, 1), new(P.PlasmaWaveRightLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(1)),
        [P.PlasmaWaveRightLoop] = new([new(P.PlasmaWaveRightLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(0)),
        [P.PlasmaIceWaveDownRight] = new([new(P.PlasmaIceWaveDownRight, 0, 1, 1), new(P.PlasmaWaveDownRight, 0, 8, 1)], AnimationEnd.LoopsFrom(1)),
        [P.PlasmaWaveDownRight] = new([new(P.PlasmaWaveDownRight, 0, 8, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChargedPowerUpFacingRight] = new([new(P.ChargedPowerUpFacingRight, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChargedPowerUpRight] = new([new(P.ChargedPowerUpRight, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChargedPowerRight] = new([new(P.ChargedPowerRight, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChargedPowerDownRight] = new([new(P.ChargedPowerDownRight, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChargedPowerDownFacingRight] = new([new(P.ChargedPowerDownFacingRight, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChargedPowerDownLeft] = new([new(P.ChargedPowerDownLeft, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChargedPowerLeft] = new([new(P.ChargedPowerLeft, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChargedPowerUpLeft] = new([new(P.ChargedPowerUpLeft, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChargedWaveUpFacingRight] = new([new(P.ChargedWaveUpFacingRight, 0, 1, 1), new(P.ChargedWaveDownFacingRight, 0, 16, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ChargedWaveDownFacingRight] = new([new(P.ChargedWaveDownFacingRight, 0, 16, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChargedWaveUpRight] = new([new(P.ChargedWaveUpRight, 0, 16, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChargedWaveRight] = new([new(P.ChargedWaveRight, 0, 16, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChargedWaveDownRight] = new([new(P.ChargedWaveDownRight, 0, 16, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChargedIceUpFacingRight] = new([new(P.ChargedIceUpFacingRight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChargedIceWaveUpFacingRight] = new([new(P.ChargedIceWaveUpFacingRight, 0, 1, 1), new(P.ChargedIceWaveDownFacingRight, 0, 16, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ChargedIceWaveDownFacingRight] = new([new(P.ChargedIceWaveDownFacingRight, 0, 16, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChargedIceWaveUpRight] = new([new(P.ChargedIceWaveUpRight, 0, 16, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChargedIceWaveRight] = new([new(P.ChargedIceWaveRight, 0, 16, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChargedIceWaveDownRight] = new([new(P.ChargedIceWaveDownRight, 0, 16, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChargedSpazerUpFacingRight] = new([new(P.ChargedSpazerUpFacingRight, 0, 8, 1), new(P.ChargedSpazerUpFacingRightLoop, 0, 2, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ChargedSpazerUpFacingRightLoop] = new([new(P.ChargedSpazerUpFacingRightLoop, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChargedSpazerUpRight] = new([new(P.ChargedSpazerUpRight, 0, 8, 1), new(P.ChargedSpazerUpRightLoop, 0, 2, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ChargedSpazerUpRightLoop] = new([new(P.ChargedSpazerUpRightLoop, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChargedSpazerRight] = new([new(P.ChargedSpazerRight, 0, 8, 1), new(P.ChargedSpazerRightLoop, 0, 2, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ChargedSpazerRightLoop] = new([new(P.ChargedSpazerRightLoop, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChargedSpazerDownRight] = new([new(P.ChargedSpazerDownRight, 0, 8, 1), new(P.ChargedSpazerDownRightLoop, 0, 2, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ChargedSpazerDownRightLoop] = new([new(P.ChargedSpazerDownRightLoop, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChargedSpazerWaveUpFacingRight] = new([new(P.ChargedSpazerWaveUpFacingRight, 0, 4, 1), new(P.ChargedSpazerWaveUpFacingRightLoop, 0, 20, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ChargedSpazerWaveUpFacingRightLoop] = new([new(P.ChargedSpazerWaveUpFacingRightLoop, 0, 20, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChargedSpazerWaveUpRight] = new([new(P.ChargedSpazerWaveUpRight, 0, 4, 1), new(P.ChargedSpazerWaveUpRightLoop, 0, 20, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ChargedSpazerWaveUpRightLoop] = new([new(P.ChargedSpazerWaveUpRightLoop, 0, 20, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChargedSpazerWaveRight] = new([new(P.ChargedSpazerWaveRight, 0, 4, 1), new(P.ChargedSpazerWaveRightLoop, 0, 20, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ChargedSpazerWaveRightLoop] = new([new(P.ChargedSpazerWaveRightLoop, 0, 20, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChargedSpazerWaveDownRight] = new([new(P.ChargedSpazerWaveDownRight, 0, 4, 1), new(P.ChargedSpazerWaveDownRightLoop, 0, 20, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ChargedSpazerWaveDownRightLoop] = new([new(P.ChargedSpazerWaveDownRightLoop, 0, 20, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChargedSpazerWaveDownFacingRight] = new([new(P.ChargedSpazerWaveDownFacingRight, 0, 4, 1), new(P.ChargedSpazerWaveDownFacingRightLoop, 0, 20, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ChargedSpazerWaveDownFacingRightLoop] = new([new(P.ChargedSpazerWaveDownFacingRightLoop, 0, 20, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChargedSpazerWaveDownLeft] = new([new(P.ChargedSpazerWaveDownLeft, 0, 4, 1), new(P.ChargedSpazerWaveDownLeftLoop, 0, 20, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ChargedSpazerWaveDownLeftLoop] = new([new(P.ChargedSpazerWaveDownLeftLoop, 0, 20, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChargedSpazerWaveLeft] = new([new(P.ChargedSpazerWaveLeft, 0, 4, 1), new(P.ChargedSpazerWaveLeftLoop, 0, 20, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ChargedSpazerWaveLeftLoop] = new([new(P.ChargedSpazerWaveLeftLoop, 0, 20, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChargedSpazerWaveUpLeft] = new([new(P.ChargedSpazerWaveUpLeft, 0, 4, 1), new(P.ChargedSpazerWaveUpLeftLoop, 0, 20, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ChargedSpazerWaveUpLeftLoop] = new([new(P.ChargedSpazerWaveUpLeftLoop, 0, 20, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChargedPlasmaUpFacingRight] = new([new(P.ChargedPlasmaUpFacingRight, 0, 6, 1), new(P.ChargedPlasmaUpFacingRightLoop, 0, 2, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ChargedPlasmaUpFacingRightLoop] = new([new(P.ChargedPlasmaUpFacingRightLoop, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChargedPlasmaUpRight] = new([new(P.ChargedPlasmaUpRight, 0, 6, 1), new(P.ChargedPlasmaUpRightLoop, 0, 2, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ChargedPlasmaUpRightLoop] = new([new(P.ChargedPlasmaUpRightLoop, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChargedPlasmaRight] = new([new(P.ChargedPlasmaRight, 0, 6, 1), new(P.ChargedPlasmaRightLoop, 0, 2, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ChargedPlasmaRightLoop] = new([new(P.ChargedPlasmaRightLoop, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChargedPlasmaDownRight] = new([new(P.ChargedPlasmaDownRight, 0, 6, 1), new(P.ChargedPlasmaDownRightLoop, 0, 2, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ChargedPlasmaDownRightLoop] = new([new(P.ChargedPlasmaDownRightLoop, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChargedPlasmaWaveUpFacingRight] = new([new(P.ChargedPlasmaWaveUpFacingRight, 0, 6, 1), new(P.ChargedPlasmaWaveUpFacingRightLoop, 0, 16, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ChargedPlasmaWaveUpFacingRightLoop] = new([new(P.ChargedPlasmaWaveUpFacingRightLoop, 0, 16, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChargedPlasmaWaveUpRight] = new([new(P.ChargedPlasmaWaveUpRight, 0, 6, 1), new(P.ChargedPlasmaWaveUpRightLoop, 0, 16, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ChargedPlasmaWaveUpRightLoop] = new([new(P.ChargedPlasmaWaveUpRightLoop, 0, 16, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChargedPlasmaWaveRight] = new([new(P.ChargedPlasmaWaveRight, 0, 6, 1), new(P.ChargedPlasmaWaveRightLoop, 0, 16, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ChargedPlasmaWaveRightLoop] = new([new(P.ChargedPlasmaWaveRightLoop, 0, 16, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChargedPlasmaWaveDownRight] = new([new(P.ChargedPlasmaWaveDownRight, 0, 6, 1), new(P.ChargedPlasmaWaveDownRightLoop, 0, 16, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ChargedPlasmaWaveDownRightLoop] = new([new(P.ChargedPlasmaWaveDownRightLoop, 0, 16, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MissileUpFacingRight] = new([new(P.MissileUpFacingRight, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MissileUpRight] = new([new(P.MissileUpRight, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MissileRight] = new([new(P.MissileRight, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MissileDownRight] = new([new(P.MissileDownRight, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MissileDownFacingRight] = new([new(P.MissileDownFacingRight, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MissileDownLeft] = new([new(P.MissileDownLeft, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MissileLeft] = new([new(P.MissileLeft, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MissileUpLeft] = new([new(P.MissileUpLeft, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SuperMissileUpFacingRight] = new([new(P.SuperMissileUpFacingRight, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SuperMissileUpRight] = new([new(P.SuperMissileUpRight, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SuperMissileRight] = new([new(P.SuperMissileRight, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SuperMissileDownRight] = new([new(P.SuperMissileDownRight, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SuperMissileDownFacingRight] = new([new(P.SuperMissileDownFacingRight, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SuperMissileDownLeft] = new([new(P.SuperMissileDownLeft, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SuperMissileLeft] = new([new(P.SuperMissileLeft, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SuperMissileUpLeft] = new([new(P.SuperMissileUpLeft, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SuperMissileLink] = new([new(P.SuperMissileLink, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.PowerBomb] = new([new(P.PowerBomb, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FastPowerBomb] = new([new(P.FastPowerBomb, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Bomb] = new([new(P.Bomb, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FastBomb] = new([new(P.FastBomb, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.BeamExplosion] = new([new(P.BeamExplosion, 0, 6, 1)], AnimationEnd.Stops),
        [P.MissileExplosion] = new([new(P.MissileExplosion, 0, 6, 1)], AnimationEnd.Stops),
        [P.BombExplosion] = new([new(P.BombExplosion, 0, 5, 1)], AnimationEnd.Stops),
        [P.PlasmaSpecialAttack] = new([new(P.PlasmaSpecialAttack, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SuperMissileExplosion] = new([new(P.SuperMissileExplosion, 0, 6, 1)], AnimationEnd.Stops),
        [P.ShinesparkEchoUpFacingRight] = new([new(P.ShinesparkEchoUpFacingRight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SpazerSpecialAttackTrailUpFacingRight] = new([new(P.SpazerSpecialAttackTrailUpFacingRight, 0, 2, 1), new(P.SpazerSpecialAttackTrailUpFacingRightLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SpazerSpecialAttackTrailUpFacingRightLoop] = new([new(P.SpazerSpecialAttackTrailUpFacingRightLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.WaveSpecialAttack] = new([new(P.WaveSpecialAttack, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<SamusProjectileSegment> StartingWith(SamusProjectileSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<SamusProjectileFrameId, NoAnimationCue> Show(SamusProjectileFrameId frame, int duration) => new AnimationBeat<SamusProjectileFrameId, NoAnimationCue>.Frame(frame, duration);
}
