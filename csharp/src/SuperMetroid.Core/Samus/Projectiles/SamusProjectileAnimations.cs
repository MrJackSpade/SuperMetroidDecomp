using SuperMetroid.Core.Animation;
using P = SuperMetroid.Core.Samus.Projectiles.SamusProjectileSegment;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The Samus projectiles' animations (#1301):
/// each timed step shows a <see cref="SamusProjectileFrame"/> for its duration, and a goto or
/// delete ends its animation.
/// </summary>
internal static class SamusProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<SamusProjectileSegment, SamusProjectileFrame, NoAnimationCue> Segments { get; } = new(
        new Dictionary<SamusProjectileSegment, AnimationBeat<SamusProjectileFrame, NoAnimationCue>[]>
    {
        [P.PowerUpFacingRight] =
        [
            Show(P.PowerUpFacingRight, 0, 15),
        ],
        [P.PowerUpRight] =
        [
            Show(P.PowerUpRight, 0, 15),
        ],
        [P.PowerRight] =
        [
            Show(P.PowerRight, 0, 15),
        ],
        [P.PowerDownRight] =
        [
            Show(P.PowerDownRight, 0, 15),
        ],
        [P.PowerDownFacingRight] =
        [
            Show(P.PowerDownFacingRight, 0, 15),
        ],
        [P.PowerDownLeft] =
        [
            Show(P.PowerDownLeft, 0, 15),
        ],
        [P.PowerLeft] =
        [
            Show(P.PowerLeft, 0, 15),
        ],
        [P.PowerUpLeft] =
        [
            Show(P.PowerUpLeft, 0, 15),
        ],
        [P.WaveUpFacingRight] =
        [
            Show(P.WaveUpFacingRight, 0, 4),
        ],
        [P.WaveDownFacingRight] =
        [
            Show(P.WaveDownFacingRight, 0, 1),
            Show(P.WaveDownFacingRight, 1, 1),
            Show(P.WaveDownFacingRight, 2, 1),
            Show(P.WaveDownFacingRight, 3, 1),
            Show(P.WaveDownFacingRight, 4, 1),
            Show(P.WaveDownFacingRight, 5, 1),
            Show(P.WaveDownFacingRight, 6, 1),
            Show(P.WaveDownFacingRight, 7, 1),
            Show(P.WaveDownFacingRight, 8, 1),
            Show(P.WaveDownFacingRight, 9, 1),
            Show(P.WaveDownFacingRight, 10, 1),
            Show(P.WaveDownFacingRight, 11, 1),
            Show(P.WaveDownFacingRight, 12, 1),
            Show(P.WaveDownFacingRight, 13, 1),
            Show(P.WaveDownFacingRight, 14, 1),
            Show(P.WaveDownFacingRight, 15, 1),
        ],
        [P.WaveUpRight] =
        [
            Show(P.WaveUpRight, 0, 1),
            Show(P.WaveUpRight, 1, 1),
            Show(P.WaveUpRight, 2, 1),
            Show(P.WaveUpRight, 3, 1),
            Show(P.WaveUpRight, 4, 1),
            Show(P.WaveUpRight, 5, 1),
            Show(P.WaveUpRight, 6, 1),
            Show(P.WaveUpRight, 7, 1),
            Show(P.WaveUpRight, 8, 1),
            Show(P.WaveUpRight, 9, 1),
            Show(P.WaveUpRight, 10, 1),
            Show(P.WaveUpRight, 11, 1),
            Show(P.WaveUpRight, 12, 1),
            Show(P.WaveUpRight, 13, 1),
            Show(P.WaveUpRight, 14, 1),
            Show(P.WaveUpRight, 15, 1),
        ],
        [P.WaveRight] =
        [
            Show(P.WaveRight, 0, 1),
            Show(P.WaveRight, 1, 1),
            Show(P.WaveRight, 2, 1),
            Show(P.WaveRight, 3, 1),
            Show(P.WaveRight, 4, 1),
            Show(P.WaveRight, 5, 1),
            Show(P.WaveRight, 6, 1),
            Show(P.WaveRight, 7, 1),
            Show(P.WaveRight, 8, 1),
            Show(P.WaveRight, 9, 1),
            Show(P.WaveRight, 10, 1),
            Show(P.WaveRight, 11, 1),
            Show(P.WaveRight, 12, 1),
            Show(P.WaveRight, 13, 1),
            Show(P.WaveRight, 14, 1),
            Show(P.WaveRight, 15, 1),
        ],
        [P.WaveDownRight] =
        [
            Show(P.WaveDownRight, 0, 1),
            Show(P.WaveDownRight, 1, 1),
            Show(P.WaveDownRight, 2, 1),
            Show(P.WaveDownRight, 3, 1),
            Show(P.WaveDownRight, 4, 1),
            Show(P.WaveDownRight, 5, 1),
            Show(P.WaveDownRight, 6, 1),
            Show(P.WaveDownRight, 7, 1),
            Show(P.WaveDownRight, 8, 1),
            Show(P.WaveDownRight, 9, 1),
            Show(P.WaveDownRight, 10, 1),
            Show(P.WaveDownRight, 11, 1),
            Show(P.WaveDownRight, 12, 1),
            Show(P.WaveDownRight, 13, 1),
            Show(P.WaveDownRight, 14, 1),
            Show(P.WaveDownRight, 15, 1),
        ],
        [P.IceUpFacingRight] =
        [
            Show(P.IceUpFacingRight, 0, 1),
            Show(P.IceUpFacingRight, 1, 1),
            Show(P.IceUpFacingRight, 2, 1),
            Show(P.IceUpFacingRight, 3, 1),
        ],
        [P.SpazerUpFacingRight] =
        [
            Show(P.SpazerUpFacingRight, 0, 2),
            Show(P.SpazerUpFacingRight, 1, 2),
        ],
        [P.SpazerUpFacingRightLoop] =
        [
            Show(P.SpazerUpFacingRightLoop, 0, 2),
        ],
        [P.SpazerUpRight] =
        [
            Show(P.SpazerUpRight, 0, 2),
            Show(P.SpazerUpRight, 1, 2),
        ],
        [P.SpazerUpRightLoop] =
        [
            Show(P.SpazerUpRightLoop, 0, 2),
        ],
        [P.SpazerRight] =
        [
            Show(P.SpazerRight, 0, 2),
            Show(P.SpazerRight, 1, 2),
        ],
        [P.SpazerRightLoop] =
        [
            Show(P.SpazerRightLoop, 0, 2),
        ],
        [P.SpazerDownRight] =
        [
            Show(P.SpazerDownRight, 0, 2),
            Show(P.SpazerDownRight, 1, 2),
        ],
        [P.SpazerDownRightLoop] =
        [
            Show(P.SpazerDownRightLoop, 0, 2),
        ],
        [P.SpazerDownFacingRight] =
        [
            Show(P.SpazerDownFacingRight, 0, 2),
            Show(P.SpazerDownFacingRight, 1, 2),
        ],
        [P.SpazerDownFacingRightLoop] =
        [
            Show(P.SpazerDownFacingRightLoop, 0, 2),
        ],
        [P.SpazerDownLeft] =
        [
            Show(P.SpazerDownLeft, 0, 2),
            Show(P.SpazerDownLeft, 1, 2),
        ],
        [P.SpazerDownLeftLoop] =
        [
            Show(P.SpazerDownLeftLoop, 0, 2),
        ],
        [P.SpazerLeft] =
        [
            Show(P.SpazerLeft, 0, 2),
            Show(P.SpazerLeft, 1, 2),
        ],
        [P.SpazerLeftLoop] =
        [
            Show(P.SpazerLeftLoop, 0, 2),
        ],
        [P.SpazerUpLeft] =
        [
            Show(P.SpazerUpLeft, 0, 2),
            Show(P.SpazerUpLeft, 1, 2),
        ],
        [P.SpazerUpLeftLoop] =
        [
            Show(P.SpazerUpLeftLoop, 0, 2),
        ],
        [P.SpazerWaveUpFacingRight] =
        [
            Show(P.SpazerWaveUpFacingRight, 0, 2),
            Show(P.SpazerWaveUpFacingRight, 1, 2),
            Show(P.SpazerWaveUpFacingRight, 2, 2),
            Show(P.SpazerWaveUpFacingRight, 3, 2),
            Show(P.SpazerWaveUpFacingRight, 4, 2),
            Show(P.SpazerWaveUpFacingRight, 5, 2),
            Show(P.SpazerWaveUpFacingRight, 6, 2),
            Show(P.SpazerWaveUpFacingRight, 7, 2),
            Show(P.SpazerWaveUpFacingRight, 8, 2),
            Show(P.SpazerWaveUpFacingRight, 9, 2),
        ],
        [P.SpazerWaveUpRight] =
        [
            Show(P.SpazerWaveUpRight, 0, 2),
            Show(P.SpazerWaveUpRight, 1, 2),
            Show(P.SpazerWaveUpRight, 2, 2),
            Show(P.SpazerWaveUpRight, 3, 2),
            Show(P.SpazerWaveUpRight, 4, 2),
            Show(P.SpazerWaveUpRight, 5, 2),
            Show(P.SpazerWaveUpRight, 6, 2),
            Show(P.SpazerWaveUpRight, 7, 2),
            Show(P.SpazerWaveUpRight, 8, 2),
            Show(P.SpazerWaveUpRight, 9, 2),
        ],
        [P.SpazerWaveRight] =
        [
            Show(P.SpazerWaveRight, 0, 2),
            Show(P.SpazerWaveRight, 1, 2),
            Show(P.SpazerWaveRight, 2, 2),
            Show(P.SpazerWaveRight, 3, 2),
            Show(P.SpazerWaveRight, 4, 2),
            Show(P.SpazerWaveRight, 5, 2),
            Show(P.SpazerWaveRight, 6, 2),
            Show(P.SpazerWaveRight, 7, 2),
            Show(P.SpazerWaveRight, 8, 2),
            Show(P.SpazerWaveRight, 9, 2),
        ],
        [P.SpazerWaveDownRight] =
        [
            Show(P.SpazerWaveDownRight, 0, 2),
            Show(P.SpazerWaveDownRight, 1, 2),
            Show(P.SpazerWaveDownRight, 2, 2),
            Show(P.SpazerWaveDownRight, 3, 2),
            Show(P.SpazerWaveDownRight, 4, 2),
            Show(P.SpazerWaveDownRight, 5, 2),
            Show(P.SpazerWaveDownRight, 6, 2),
            Show(P.SpazerWaveDownRight, 7, 2),
            Show(P.SpazerWaveDownRight, 8, 2),
            Show(P.SpazerWaveDownRight, 9, 2),
        ],
        [P.SpazerWaveDownFacingRight] =
        [
            Show(P.SpazerWaveDownFacingRight, 0, 2),
            Show(P.SpazerWaveDownFacingRight, 1, 2),
            Show(P.SpazerWaveDownFacingRight, 2, 2),
            Show(P.SpazerWaveDownFacingRight, 3, 2),
            Show(P.SpazerWaveDownFacingRight, 4, 2),
            Show(P.SpazerWaveDownFacingRight, 5, 2),
            Show(P.SpazerWaveDownFacingRight, 6, 2),
            Show(P.SpazerWaveDownFacingRight, 7, 2),
            Show(P.SpazerWaveDownFacingRight, 8, 2),
            Show(P.SpazerWaveDownFacingRight, 9, 2),
        ],
        [P.SpazerWaveDownLeft] =
        [
            Show(P.SpazerWaveDownLeft, 0, 2),
            Show(P.SpazerWaveDownLeft, 1, 2),
            Show(P.SpazerWaveDownLeft, 2, 2),
            Show(P.SpazerWaveDownLeft, 3, 2),
            Show(P.SpazerWaveDownLeft, 4, 2),
            Show(P.SpazerWaveDownLeft, 5, 2),
            Show(P.SpazerWaveDownLeft, 6, 2),
            Show(P.SpazerWaveDownLeft, 7, 2),
            Show(P.SpazerWaveDownLeft, 8, 2),
            Show(P.SpazerWaveDownLeft, 9, 2),
        ],
        [P.SpazerWaveLeft] =
        [
            Show(P.SpazerWaveLeft, 0, 2),
            Show(P.SpazerWaveLeft, 1, 2),
            Show(P.SpazerWaveLeft, 2, 2),
            Show(P.SpazerWaveLeft, 3, 2),
            Show(P.SpazerWaveLeft, 4, 2),
            Show(P.SpazerWaveLeft, 5, 2),
            Show(P.SpazerWaveLeft, 6, 2),
            Show(P.SpazerWaveLeft, 7, 2),
            Show(P.SpazerWaveLeft, 8, 2),
            Show(P.SpazerWaveLeft, 9, 2),
        ],
        [P.SpazerWaveUpLeft] =
        [
            Show(P.SpazerWaveUpLeft, 0, 2),
            Show(P.SpazerWaveUpLeft, 1, 2),
            Show(P.SpazerWaveUpLeft, 2, 2),
            Show(P.SpazerWaveUpLeft, 3, 2),
            Show(P.SpazerWaveUpLeft, 4, 2),
            Show(P.SpazerWaveUpLeft, 5, 2),
            Show(P.SpazerWaveUpLeft, 6, 2),
            Show(P.SpazerWaveUpLeft, 7, 2),
            Show(P.SpazerWaveUpLeft, 8, 2),
            Show(P.SpazerWaveUpLeft, 9, 2),
        ],
        [P.PlasmaUpFacingRight] =
        [
            Show(P.PlasmaUpFacingRight, 0, 1),
        ],
        [P.PlasmaUpFacingRightLoop] =
        [
            Show(P.PlasmaUpFacingRightLoop, 0, 15),
        ],
        [P.PlasmaUpRight] =
        [
            Show(P.PlasmaUpRight, 0, 1),
        ],
        [P.PlasmaUpRightLoop] =
        [
            Show(P.PlasmaUpRightLoop, 0, 15),
        ],
        [P.PlasmaRight] =
        [
            Show(P.PlasmaRight, 0, 1),
        ],
        [P.PlasmaRightLoop] =
        [
            Show(P.PlasmaRightLoop, 0, 15),
        ],
        [P.PlasmaDownRight] =
        [
            Show(P.PlasmaDownRight, 0, 1),
        ],
        [P.PlasmaDownRightLoop] =
        [
            Show(P.PlasmaDownRightLoop, 0, 15),
        ],
        [P.PlasmaIceWaveUpFacingRight] =
        [
            Show(P.PlasmaIceWaveUpFacingRight, 0, 1),
        ],
        [P.PlasmaWaveUpFacingRight] =
        [
            Show(P.PlasmaWaveUpFacingRight, 0, 2),
            Show(P.PlasmaWaveUpFacingRight, 1, 2),
            Show(P.PlasmaWaveUpFacingRight, 2, 2),
            Show(P.PlasmaWaveUpFacingRight, 3, 2),
            Show(P.PlasmaWaveUpFacingRight, 4, 2),
            Show(P.PlasmaWaveUpFacingRight, 5, 2),
            Show(P.PlasmaWaveUpFacingRight, 6, 2),
            Show(P.PlasmaWaveUpFacingRight, 7, 2),
        ],
        [P.PlasmaIceWaveUpRight] =
        [
            Show(P.PlasmaIceWaveUpRight, 0, 1),
        ],
        [P.PlasmaWaveUpRight] =
        [
            Show(P.PlasmaWaveUpRight, 0, 2),
            Show(P.PlasmaWaveUpRight, 1, 2),
            Show(P.PlasmaWaveUpRight, 2, 2),
            Show(P.PlasmaWaveUpRight, 3, 2),
            Show(P.PlasmaWaveUpRight, 4, 2),
            Show(P.PlasmaWaveUpRight, 5, 2),
            Show(P.PlasmaWaveUpRight, 6, 2),
            Show(P.PlasmaWaveUpRight, 7, 2),
        ],
        [P.PlasmaWaveRight] =
        [
            Show(P.PlasmaWaveRight, 0, 1),
        ],
        [P.PlasmaWaveRightLoop] =
        [
            Show(P.PlasmaWaveRightLoop, 0, 2),
            Show(P.PlasmaWaveRightLoop, 1, 2),
            Show(P.PlasmaWaveRightLoop, 2, 2),
            Show(P.PlasmaWaveRightLoop, 3, 2),
            Show(P.PlasmaWaveRightLoop, 4, 2),
            Show(P.PlasmaWaveRightLoop, 5, 2),
            Show(P.PlasmaWaveRightLoop, 6, 2),
            Show(P.PlasmaWaveRightLoop, 7, 2),
        ],
        [P.PlasmaIceWaveDownRight] =
        [
            Show(P.PlasmaIceWaveDownRight, 0, 1),
        ],
        [P.PlasmaWaveDownRight] =
        [
            Show(P.PlasmaWaveDownRight, 0, 2),
            Show(P.PlasmaWaveDownRight, 1, 2),
            Show(P.PlasmaWaveDownRight, 2, 2),
            Show(P.PlasmaWaveDownRight, 3, 2),
            Show(P.PlasmaWaveDownRight, 4, 2),
            Show(P.PlasmaWaveDownRight, 5, 2),
            Show(P.PlasmaWaveDownRight, 6, 2),
            Show(P.PlasmaWaveDownRight, 7, 2),
        ],
        [P.ChargedPowerUpFacingRight] =
        [
            Show(P.ChargedPowerUpFacingRight, 0, 1),
            Show(P.ChargedPowerUpFacingRight, 1, 1),
        ],
        [P.ChargedPowerUpRight] =
        [
            Show(P.ChargedPowerUpRight, 0, 1),
            Show(P.ChargedPowerUpRight, 1, 1),
        ],
        [P.ChargedPowerRight] =
        [
            Show(P.ChargedPowerRight, 0, 1),
            Show(P.ChargedPowerRight, 1, 1),
        ],
        [P.ChargedPowerDownRight] =
        [
            Show(P.ChargedPowerDownRight, 0, 1),
            Show(P.ChargedPowerDownRight, 1, 1),
        ],
        [P.ChargedPowerDownFacingRight] =
        [
            Show(P.ChargedPowerDownFacingRight, 0, 1),
            Show(P.ChargedPowerDownFacingRight, 1, 1),
        ],
        [P.ChargedPowerDownLeft] =
        [
            Show(P.ChargedPowerDownLeft, 0, 1),
            Show(P.ChargedPowerDownLeft, 1, 1),
        ],
        [P.ChargedPowerLeft] =
        [
            Show(P.ChargedPowerLeft, 0, 1),
            Show(P.ChargedPowerLeft, 1, 1),
        ],
        [P.ChargedPowerUpLeft] =
        [
            Show(P.ChargedPowerUpLeft, 0, 1),
            Show(P.ChargedPowerUpLeft, 1, 1),
        ],
        [P.ChargedWaveUpFacingRight] =
        [
            Show(P.ChargedWaveUpFacingRight, 0, 3),
        ],
        [P.ChargedWaveDownFacingRight] =
        [
            Show(P.ChargedWaveDownFacingRight, 0, 1),
            Show(P.ChargedWaveDownFacingRight, 1, 1),
            Show(P.ChargedWaveDownFacingRight, 2, 1),
            Show(P.ChargedWaveDownFacingRight, 3, 1),
            Show(P.ChargedWaveDownFacingRight, 4, 1),
            Show(P.ChargedWaveDownFacingRight, 5, 1),
            Show(P.ChargedWaveDownFacingRight, 6, 1),
            Show(P.ChargedWaveDownFacingRight, 7, 1),
            Show(P.ChargedWaveDownFacingRight, 8, 1),
            Show(P.ChargedWaveDownFacingRight, 9, 1),
            Show(P.ChargedWaveDownFacingRight, 10, 1),
            Show(P.ChargedWaveDownFacingRight, 11, 1),
            Show(P.ChargedWaveDownFacingRight, 12, 1),
            Show(P.ChargedWaveDownFacingRight, 13, 1),
            Show(P.ChargedWaveDownFacingRight, 14, 1),
            Show(P.ChargedWaveDownFacingRight, 15, 1),
        ],
        [P.ChargedWaveUpRight] =
        [
            Show(P.ChargedWaveUpRight, 0, 1),
            Show(P.ChargedWaveUpRight, 1, 1),
            Show(P.ChargedWaveUpRight, 2, 1),
            Show(P.ChargedWaveUpRight, 3, 1),
            Show(P.ChargedWaveUpRight, 4, 1),
            Show(P.ChargedWaveUpRight, 5, 1),
            Show(P.ChargedWaveUpRight, 6, 1),
            Show(P.ChargedWaveUpRight, 7, 1),
            Show(P.ChargedWaveUpRight, 8, 1),
            Show(P.ChargedWaveUpRight, 9, 1),
            Show(P.ChargedWaveUpRight, 10, 1),
            Show(P.ChargedWaveUpRight, 11, 1),
            Show(P.ChargedWaveUpRight, 12, 1),
            Show(P.ChargedWaveUpRight, 13, 1),
            Show(P.ChargedWaveUpRight, 14, 1),
            Show(P.ChargedWaveUpRight, 15, 1),
        ],
        [P.ChargedWaveRight] =
        [
            Show(P.ChargedWaveRight, 0, 1),
            Show(P.ChargedWaveRight, 1, 1),
            Show(P.ChargedWaveRight, 2, 1),
            Show(P.ChargedWaveRight, 3, 1),
            Show(P.ChargedWaveRight, 4, 1),
            Show(P.ChargedWaveRight, 5, 1),
            Show(P.ChargedWaveRight, 6, 1),
            Show(P.ChargedWaveRight, 7, 1),
            Show(P.ChargedWaveRight, 8, 1),
            Show(P.ChargedWaveRight, 9, 1),
            Show(P.ChargedWaveRight, 10, 1),
            Show(P.ChargedWaveRight, 11, 1),
            Show(P.ChargedWaveRight, 12, 1),
            Show(P.ChargedWaveRight, 13, 1),
            Show(P.ChargedWaveRight, 14, 1),
            Show(P.ChargedWaveRight, 15, 1),
        ],
        [P.ChargedWaveDownRight] =
        [
            Show(P.ChargedWaveDownRight, 0, 1),
            Show(P.ChargedWaveDownRight, 1, 1),
            Show(P.ChargedWaveDownRight, 2, 1),
            Show(P.ChargedWaveDownRight, 3, 1),
            Show(P.ChargedWaveDownRight, 4, 1),
            Show(P.ChargedWaveDownRight, 5, 1),
            Show(P.ChargedWaveDownRight, 6, 1),
            Show(P.ChargedWaveDownRight, 7, 1),
            Show(P.ChargedWaveDownRight, 8, 1),
            Show(P.ChargedWaveDownRight, 9, 1),
            Show(P.ChargedWaveDownRight, 10, 1),
            Show(P.ChargedWaveDownRight, 11, 1),
            Show(P.ChargedWaveDownRight, 12, 1),
            Show(P.ChargedWaveDownRight, 13, 1),
            Show(P.ChargedWaveDownRight, 14, 1),
            Show(P.ChargedWaveDownRight, 15, 1),
        ],
        [P.ChargedIceUpFacingRight] =
        [
            Show(P.ChargedIceUpFacingRight, 0, 1),
            Show(P.ChargedIceUpFacingRight, 1, 1),
            Show(P.ChargedIceUpFacingRight, 2, 1),
            Show(P.ChargedIceUpFacingRight, 3, 1),
        ],
        [P.ChargedIceWaveUpFacingRight] =
        [
            Show(P.ChargedIceWaveUpFacingRight, 0, 3),
        ],
        [P.ChargedIceWaveDownFacingRight] =
        [
            Show(P.ChargedIceWaveDownFacingRight, 0, 1),
            Show(P.ChargedIceWaveDownFacingRight, 1, 1),
            Show(P.ChargedIceWaveDownFacingRight, 2, 1),
            Show(P.ChargedIceWaveDownFacingRight, 3, 1),
            Show(P.ChargedIceWaveDownFacingRight, 4, 1),
            Show(P.ChargedIceWaveDownFacingRight, 5, 1),
            Show(P.ChargedIceWaveDownFacingRight, 6, 1),
            Show(P.ChargedIceWaveDownFacingRight, 7, 1),
            Show(P.ChargedIceWaveDownFacingRight, 8, 1),
            Show(P.ChargedIceWaveDownFacingRight, 9, 1),
            Show(P.ChargedIceWaveDownFacingRight, 10, 1),
            Show(P.ChargedIceWaveDownFacingRight, 11, 1),
            Show(P.ChargedIceWaveDownFacingRight, 12, 1),
            Show(P.ChargedIceWaveDownFacingRight, 13, 1),
            Show(P.ChargedIceWaveDownFacingRight, 14, 1),
            Show(P.ChargedIceWaveDownFacingRight, 15, 1),
        ],
        [P.ChargedIceWaveUpRight] =
        [
            Show(P.ChargedIceWaveUpRight, 0, 1),
            Show(P.ChargedIceWaveUpRight, 1, 1),
            Show(P.ChargedIceWaveUpRight, 2, 1),
            Show(P.ChargedIceWaveUpRight, 3, 1),
            Show(P.ChargedIceWaveUpRight, 4, 1),
            Show(P.ChargedIceWaveUpRight, 5, 1),
            Show(P.ChargedIceWaveUpRight, 6, 1),
            Show(P.ChargedIceWaveUpRight, 7, 1),
            Show(P.ChargedIceWaveUpRight, 8, 1),
            Show(P.ChargedIceWaveUpRight, 9, 1),
            Show(P.ChargedIceWaveUpRight, 10, 1),
            Show(P.ChargedIceWaveUpRight, 11, 1),
            Show(P.ChargedIceWaveUpRight, 12, 1),
            Show(P.ChargedIceWaveUpRight, 13, 1),
            Show(P.ChargedIceWaveUpRight, 14, 1),
            Show(P.ChargedIceWaveUpRight, 15, 1),
        ],
        [P.ChargedIceWaveRight] =
        [
            Show(P.ChargedIceWaveRight, 0, 1),
            Show(P.ChargedIceWaveRight, 1, 1),
            Show(P.ChargedIceWaveRight, 2, 1),
            Show(P.ChargedIceWaveRight, 3, 1),
            Show(P.ChargedIceWaveRight, 4, 1),
            Show(P.ChargedIceWaveRight, 5, 1),
            Show(P.ChargedIceWaveRight, 6, 1),
            Show(P.ChargedIceWaveRight, 7, 1),
            Show(P.ChargedIceWaveRight, 8, 1),
            Show(P.ChargedIceWaveRight, 9, 1),
            Show(P.ChargedIceWaveRight, 10, 1),
            Show(P.ChargedIceWaveRight, 11, 1),
            Show(P.ChargedIceWaveRight, 12, 1),
            Show(P.ChargedIceWaveRight, 13, 1),
            Show(P.ChargedIceWaveRight, 14, 1),
            Show(P.ChargedIceWaveRight, 15, 1),
        ],
        [P.ChargedIceWaveDownRight] =
        [
            Show(P.ChargedIceWaveDownRight, 0, 1),
            Show(P.ChargedIceWaveDownRight, 1, 1),
            Show(P.ChargedIceWaveDownRight, 2, 1),
            Show(P.ChargedIceWaveDownRight, 3, 1),
            Show(P.ChargedIceWaveDownRight, 4, 1),
            Show(P.ChargedIceWaveDownRight, 5, 1),
            Show(P.ChargedIceWaveDownRight, 6, 1),
            Show(P.ChargedIceWaveDownRight, 7, 1),
            Show(P.ChargedIceWaveDownRight, 8, 1),
            Show(P.ChargedIceWaveDownRight, 9, 1),
            Show(P.ChargedIceWaveDownRight, 10, 1),
            Show(P.ChargedIceWaveDownRight, 11, 1),
            Show(P.ChargedIceWaveDownRight, 12, 1),
            Show(P.ChargedIceWaveDownRight, 13, 1),
            Show(P.ChargedIceWaveDownRight, 14, 1),
            Show(P.ChargedIceWaveDownRight, 15, 1),
        ],
        [P.ChargedSpazerUpFacingRight] =
        [
            Show(P.ChargedSpazerUpFacingRight, 0, 1),
            Show(P.ChargedSpazerUpFacingRight, 1, 1),
            Show(P.ChargedSpazerUpFacingRight, 2, 1),
            Show(P.ChargedSpazerUpFacingRight, 3, 1),
            Show(P.ChargedSpazerUpFacingRight, 4, 1),
            Show(P.ChargedSpazerUpFacingRight, 5, 1),
            Show(P.ChargedSpazerUpFacingRight, 6, 1),
            Show(P.ChargedSpazerUpFacingRight, 7, 1),
        ],
        [P.ChargedSpazerUpFacingRightLoop] =
        [
            Show(P.ChargedSpazerUpFacingRightLoop, 0, 1),
            Show(P.ChargedSpazerUpFacingRightLoop, 1, 1),
        ],
        [P.ChargedSpazerUpRight] =
        [
            Show(P.ChargedSpazerUpRight, 0, 1),
            Show(P.ChargedSpazerUpRight, 1, 1),
            Show(P.ChargedSpazerUpRight, 2, 1),
            Show(P.ChargedSpazerUpRight, 3, 1),
            Show(P.ChargedSpazerUpRight, 4, 1),
            Show(P.ChargedSpazerUpRight, 5, 1),
            Show(P.ChargedSpazerUpRight, 6, 1),
            Show(P.ChargedSpazerUpRight, 7, 1),
        ],
        [P.ChargedSpazerUpRightLoop] =
        [
            Show(P.ChargedSpazerUpRightLoop, 0, 1),
            Show(P.ChargedSpazerUpRightLoop, 1, 1),
        ],
        [P.ChargedSpazerRight] =
        [
            Show(P.ChargedSpazerRight, 0, 1),
            Show(P.ChargedSpazerRight, 1, 1),
            Show(P.ChargedSpazerRight, 2, 1),
            Show(P.ChargedSpazerRight, 3, 1),
            Show(P.ChargedSpazerRight, 4, 1),
            Show(P.ChargedSpazerRight, 5, 1),
            Show(P.ChargedSpazerRight, 6, 1),
            Show(P.ChargedSpazerRight, 7, 1),
        ],
        [P.ChargedSpazerRightLoop] =
        [
            Show(P.ChargedSpazerRightLoop, 0, 1),
            Show(P.ChargedSpazerRightLoop, 1, 1),
        ],
        [P.ChargedSpazerDownRight] =
        [
            Show(P.ChargedSpazerDownRight, 0, 1),
            Show(P.ChargedSpazerDownRight, 1, 1),
            Show(P.ChargedSpazerDownRight, 2, 1),
            Show(P.ChargedSpazerDownRight, 3, 1),
            Show(P.ChargedSpazerDownRight, 4, 1),
            Show(P.ChargedSpazerDownRight, 5, 1),
            Show(P.ChargedSpazerDownRight, 6, 1),
            Show(P.ChargedSpazerDownRight, 7, 1),
        ],
        [P.ChargedSpazerDownRightLoop] =
        [
            Show(P.ChargedSpazerDownRightLoop, 0, 1),
            Show(P.ChargedSpazerDownRightLoop, 1, 1),
        ],
        [P.ChargedSpazerWaveUpFacingRight] =
        [
            Show(P.ChargedSpazerWaveUpFacingRight, 0, 1),
            Show(P.ChargedSpazerWaveUpFacingRight, 1, 1),
            Show(P.ChargedSpazerWaveUpFacingRight, 2, 1),
            Show(P.ChargedSpazerWaveUpFacingRight, 3, 1),
        ],
        [P.ChargedSpazerWaveUpFacingRightLoop] =
        [
            Show(P.ChargedSpazerWaveUpFacingRightLoop, 0, 1),
            Show(P.ChargedSpazerWaveUpFacingRightLoop, 1, 1),
            Show(P.ChargedSpazerWaveUpFacingRightLoop, 2, 1),
            Show(P.ChargedSpazerWaveUpFacingRightLoop, 3, 1),
            Show(P.ChargedSpazerWaveUpFacingRightLoop, 4, 1),
            Show(P.ChargedSpazerWaveUpFacingRightLoop, 5, 1),
            Show(P.ChargedSpazerWaveUpFacingRightLoop, 6, 1),
            Show(P.ChargedSpazerWaveUpFacingRightLoop, 7, 1),
            Show(P.ChargedSpazerWaveUpFacingRightLoop, 8, 1),
            Show(P.ChargedSpazerWaveUpFacingRightLoop, 9, 1),
            Show(P.ChargedSpazerWaveUpFacingRightLoop, 10, 1),
            Show(P.ChargedSpazerWaveUpFacingRightLoop, 11, 1),
            Show(P.ChargedSpazerWaveUpFacingRightLoop, 12, 1),
            Show(P.ChargedSpazerWaveUpFacingRightLoop, 13, 1),
            Show(P.ChargedSpazerWaveUpFacingRightLoop, 14, 1),
            Show(P.ChargedSpazerWaveUpFacingRightLoop, 15, 1),
            Show(P.ChargedSpazerWaveUpFacingRightLoop, 16, 1),
            Show(P.ChargedSpazerWaveUpFacingRightLoop, 17, 1),
            Show(P.ChargedSpazerWaveUpFacingRightLoop, 18, 1),
            Show(P.ChargedSpazerWaveUpFacingRightLoop, 19, 1),
        ],
        [P.ChargedSpazerWaveUpRight] =
        [
            Show(P.ChargedSpazerWaveUpRight, 0, 1),
            Show(P.ChargedSpazerWaveUpRight, 1, 1),
            Show(P.ChargedSpazerWaveUpRight, 2, 1),
            Show(P.ChargedSpazerWaveUpRight, 3, 1),
        ],
        [P.ChargedSpazerWaveUpRightLoop] =
        [
            Show(P.ChargedSpazerWaveUpRightLoop, 0, 1),
            Show(P.ChargedSpazerWaveUpRightLoop, 1, 1),
            Show(P.ChargedSpazerWaveUpRightLoop, 2, 1),
            Show(P.ChargedSpazerWaveUpRightLoop, 3, 1),
            Show(P.ChargedSpazerWaveUpRightLoop, 4, 1),
            Show(P.ChargedSpazerWaveUpRightLoop, 5, 1),
            Show(P.ChargedSpazerWaveUpRightLoop, 6, 1),
            Show(P.ChargedSpazerWaveUpRightLoop, 7, 1),
            Show(P.ChargedSpazerWaveUpRightLoop, 8, 1),
            Show(P.ChargedSpazerWaveUpRightLoop, 9, 1),
            Show(P.ChargedSpazerWaveUpRightLoop, 10, 1),
            Show(P.ChargedSpazerWaveUpRightLoop, 11, 1),
            Show(P.ChargedSpazerWaveUpRightLoop, 12, 1),
            Show(P.ChargedSpazerWaveUpRightLoop, 13, 1),
            Show(P.ChargedSpazerWaveUpRightLoop, 14, 1),
            Show(P.ChargedSpazerWaveUpRightLoop, 15, 1),
            Show(P.ChargedSpazerWaveUpRightLoop, 16, 1),
            Show(P.ChargedSpazerWaveUpRightLoop, 17, 1),
            Show(P.ChargedSpazerWaveUpRightLoop, 18, 1),
            Show(P.ChargedSpazerWaveUpRightLoop, 19, 1),
        ],
        [P.ChargedSpazerWaveRight] =
        [
            Show(P.ChargedSpazerWaveRight, 0, 1),
            Show(P.ChargedSpazerWaveRight, 1, 1),
            Show(P.ChargedSpazerWaveRight, 2, 1),
            Show(P.ChargedSpazerWaveRight, 3, 1),
        ],
        [P.ChargedSpazerWaveRightLoop] =
        [
            Show(P.ChargedSpazerWaveRightLoop, 0, 1),
            Show(P.ChargedSpazerWaveRightLoop, 1, 1),
            Show(P.ChargedSpazerWaveRightLoop, 2, 1),
            Show(P.ChargedSpazerWaveRightLoop, 3, 1),
            Show(P.ChargedSpazerWaveRightLoop, 4, 1),
            Show(P.ChargedSpazerWaveRightLoop, 5, 1),
            Show(P.ChargedSpazerWaveRightLoop, 6, 1),
            Show(P.ChargedSpazerWaveRightLoop, 7, 1),
            Show(P.ChargedSpazerWaveRightLoop, 8, 1),
            Show(P.ChargedSpazerWaveRightLoop, 9, 1),
            Show(P.ChargedSpazerWaveRightLoop, 10, 1),
            Show(P.ChargedSpazerWaveRightLoop, 11, 1),
            Show(P.ChargedSpazerWaveRightLoop, 12, 1),
            Show(P.ChargedSpazerWaveRightLoop, 13, 1),
            Show(P.ChargedSpazerWaveRightLoop, 14, 1),
            Show(P.ChargedSpazerWaveRightLoop, 15, 1),
            Show(P.ChargedSpazerWaveRightLoop, 16, 1),
            Show(P.ChargedSpazerWaveRightLoop, 17, 1),
            Show(P.ChargedSpazerWaveRightLoop, 18, 1),
            Show(P.ChargedSpazerWaveRightLoop, 19, 1),
        ],
        [P.ChargedSpazerWaveDownRight] =
        [
            Show(P.ChargedSpazerWaveDownRight, 0, 1),
            Show(P.ChargedSpazerWaveDownRight, 1, 1),
            Show(P.ChargedSpazerWaveDownRight, 2, 1),
            Show(P.ChargedSpazerWaveDownRight, 3, 1),
        ],
        [P.ChargedSpazerWaveDownRightLoop] =
        [
            Show(P.ChargedSpazerWaveDownRightLoop, 0, 1),
            Show(P.ChargedSpazerWaveDownRightLoop, 1, 1),
            Show(P.ChargedSpazerWaveDownRightLoop, 2, 1),
            Show(P.ChargedSpazerWaveDownRightLoop, 3, 1),
            Show(P.ChargedSpazerWaveDownRightLoop, 4, 1),
            Show(P.ChargedSpazerWaveDownRightLoop, 5, 1),
            Show(P.ChargedSpazerWaveDownRightLoop, 6, 1),
            Show(P.ChargedSpazerWaveDownRightLoop, 7, 1),
            Show(P.ChargedSpazerWaveDownRightLoop, 8, 1),
            Show(P.ChargedSpazerWaveDownRightLoop, 9, 1),
            Show(P.ChargedSpazerWaveDownRightLoop, 10, 1),
            Show(P.ChargedSpazerWaveDownRightLoop, 11, 1),
            Show(P.ChargedSpazerWaveDownRightLoop, 12, 1),
            Show(P.ChargedSpazerWaveDownRightLoop, 13, 1),
            Show(P.ChargedSpazerWaveDownRightLoop, 14, 1),
            Show(P.ChargedSpazerWaveDownRightLoop, 15, 1),
            Show(P.ChargedSpazerWaveDownRightLoop, 16, 1),
            Show(P.ChargedSpazerWaveDownRightLoop, 17, 1),
            Show(P.ChargedSpazerWaveDownRightLoop, 18, 1),
            Show(P.ChargedSpazerWaveDownRightLoop, 19, 1),
        ],
        [P.ChargedSpazerWaveDownFacingRight] =
        [
            Show(P.ChargedSpazerWaveDownFacingRight, 0, 1),
            Show(P.ChargedSpazerWaveDownFacingRight, 1, 1),
            Show(P.ChargedSpazerWaveDownFacingRight, 2, 1),
            Show(P.ChargedSpazerWaveDownFacingRight, 3, 1),
        ],
        [P.ChargedSpazerWaveDownFacingRightLoop] =
        [
            Show(P.ChargedSpazerWaveDownFacingRightLoop, 0, 1),
            Show(P.ChargedSpazerWaveDownFacingRightLoop, 1, 1),
            Show(P.ChargedSpazerWaveDownFacingRightLoop, 2, 1),
            Show(P.ChargedSpazerWaveDownFacingRightLoop, 3, 1),
            Show(P.ChargedSpazerWaveDownFacingRightLoop, 4, 1),
            Show(P.ChargedSpazerWaveDownFacingRightLoop, 5, 1),
            Show(P.ChargedSpazerWaveDownFacingRightLoop, 6, 1),
            Show(P.ChargedSpazerWaveDownFacingRightLoop, 7, 1),
            Show(P.ChargedSpazerWaveDownFacingRightLoop, 8, 1),
            Show(P.ChargedSpazerWaveDownFacingRightLoop, 9, 1),
            Show(P.ChargedSpazerWaveDownFacingRightLoop, 10, 1),
            Show(P.ChargedSpazerWaveDownFacingRightLoop, 11, 1),
            Show(P.ChargedSpazerWaveDownFacingRightLoop, 12, 1),
            Show(P.ChargedSpazerWaveDownFacingRightLoop, 13, 1),
            Show(P.ChargedSpazerWaveDownFacingRightLoop, 14, 1),
            Show(P.ChargedSpazerWaveDownFacingRightLoop, 15, 1),
            Show(P.ChargedSpazerWaveDownFacingRightLoop, 16, 1),
            Show(P.ChargedSpazerWaveDownFacingRightLoop, 17, 1),
            Show(P.ChargedSpazerWaveDownFacingRightLoop, 18, 1),
            Show(P.ChargedSpazerWaveDownFacingRightLoop, 19, 1),
        ],
        [P.ChargedSpazerWaveDownLeft] =
        [
            Show(P.ChargedSpazerWaveDownLeft, 0, 1),
            Show(P.ChargedSpazerWaveDownLeft, 1, 1),
            Show(P.ChargedSpazerWaveDownLeft, 2, 1),
            Show(P.ChargedSpazerWaveDownLeft, 3, 1),
        ],
        [P.ChargedSpazerWaveDownLeftLoop] =
        [
            Show(P.ChargedSpazerWaveDownLeftLoop, 0, 1),
            Show(P.ChargedSpazerWaveDownLeftLoop, 1, 1),
            Show(P.ChargedSpazerWaveDownLeftLoop, 2, 1),
            Show(P.ChargedSpazerWaveDownLeftLoop, 3, 1),
            Show(P.ChargedSpazerWaveDownLeftLoop, 4, 1),
            Show(P.ChargedSpazerWaveDownLeftLoop, 5, 1),
            Show(P.ChargedSpazerWaveDownLeftLoop, 6, 1),
            Show(P.ChargedSpazerWaveDownLeftLoop, 7, 1),
            Show(P.ChargedSpazerWaveDownLeftLoop, 8, 1),
            Show(P.ChargedSpazerWaveDownLeftLoop, 9, 1),
            Show(P.ChargedSpazerWaveDownLeftLoop, 10, 1),
            Show(P.ChargedSpazerWaveDownLeftLoop, 11, 1),
            Show(P.ChargedSpazerWaveDownLeftLoop, 12, 1),
            Show(P.ChargedSpazerWaveDownLeftLoop, 13, 1),
            Show(P.ChargedSpazerWaveDownLeftLoop, 14, 1),
            Show(P.ChargedSpazerWaveDownLeftLoop, 15, 1),
            Show(P.ChargedSpazerWaveDownLeftLoop, 16, 1),
            Show(P.ChargedSpazerWaveDownLeftLoop, 17, 1),
            Show(P.ChargedSpazerWaveDownLeftLoop, 18, 1),
            Show(P.ChargedSpazerWaveDownLeftLoop, 19, 1),
        ],
        [P.ChargedSpazerWaveLeft] =
        [
            Show(P.ChargedSpazerWaveLeft, 0, 1),
            Show(P.ChargedSpazerWaveLeft, 1, 1),
            Show(P.ChargedSpazerWaveLeft, 2, 1),
            Show(P.ChargedSpazerWaveLeft, 3, 1),
        ],
        [P.ChargedSpazerWaveLeftLoop] =
        [
            Show(P.ChargedSpazerWaveLeftLoop, 0, 1),
            Show(P.ChargedSpazerWaveLeftLoop, 1, 1),
            Show(P.ChargedSpazerWaveLeftLoop, 2, 1),
            Show(P.ChargedSpazerWaveLeftLoop, 3, 1),
            Show(P.ChargedSpazerWaveLeftLoop, 4, 1),
            Show(P.ChargedSpazerWaveLeftLoop, 5, 1),
            Show(P.ChargedSpazerWaveLeftLoop, 6, 1),
            Show(P.ChargedSpazerWaveLeftLoop, 7, 1),
            Show(P.ChargedSpazerWaveLeftLoop, 8, 1),
            Show(P.ChargedSpazerWaveLeftLoop, 9, 1),
            Show(P.ChargedSpazerWaveLeftLoop, 10, 1),
            Show(P.ChargedSpazerWaveLeftLoop, 11, 1),
            Show(P.ChargedSpazerWaveLeftLoop, 12, 1),
            Show(P.ChargedSpazerWaveLeftLoop, 13, 1),
            Show(P.ChargedSpazerWaveLeftLoop, 14, 1),
            Show(P.ChargedSpazerWaveLeftLoop, 15, 1),
            Show(P.ChargedSpazerWaveLeftLoop, 16, 1),
            Show(P.ChargedSpazerWaveLeftLoop, 17, 1),
            Show(P.ChargedSpazerWaveLeftLoop, 18, 1),
            Show(P.ChargedSpazerWaveLeftLoop, 19, 1),
        ],
        [P.ChargedSpazerWaveUpLeft] =
        [
            Show(P.ChargedSpazerWaveUpLeft, 0, 1),
            Show(P.ChargedSpazerWaveUpLeft, 1, 1),
            Show(P.ChargedSpazerWaveUpLeft, 2, 1),
            Show(P.ChargedSpazerWaveUpLeft, 3, 1),
        ],
        [P.ChargedSpazerWaveUpLeftLoop] =
        [
            Show(P.ChargedSpazerWaveUpLeftLoop, 0, 1),
            Show(P.ChargedSpazerWaveUpLeftLoop, 1, 1),
            Show(P.ChargedSpazerWaveUpLeftLoop, 2, 1),
            Show(P.ChargedSpazerWaveUpLeftLoop, 3, 1),
            Show(P.ChargedSpazerWaveUpLeftLoop, 4, 1),
            Show(P.ChargedSpazerWaveUpLeftLoop, 5, 1),
            Show(P.ChargedSpazerWaveUpLeftLoop, 6, 1),
            Show(P.ChargedSpazerWaveUpLeftLoop, 7, 1),
            Show(P.ChargedSpazerWaveUpLeftLoop, 8, 1),
            Show(P.ChargedSpazerWaveUpLeftLoop, 9, 1),
            Show(P.ChargedSpazerWaveUpLeftLoop, 10, 1),
            Show(P.ChargedSpazerWaveUpLeftLoop, 11, 1),
            Show(P.ChargedSpazerWaveUpLeftLoop, 12, 1),
            Show(P.ChargedSpazerWaveUpLeftLoop, 13, 1),
            Show(P.ChargedSpazerWaveUpLeftLoop, 14, 1),
            Show(P.ChargedSpazerWaveUpLeftLoop, 15, 1),
            Show(P.ChargedSpazerWaveUpLeftLoop, 16, 1),
            Show(P.ChargedSpazerWaveUpLeftLoop, 17, 1),
            Show(P.ChargedSpazerWaveUpLeftLoop, 18, 1),
            Show(P.ChargedSpazerWaveUpLeftLoop, 19, 1),
        ],
        [P.ChargedPlasmaUpFacingRight] =
        [
            Show(P.ChargedPlasmaUpFacingRight, 0, 1),
            Show(P.ChargedPlasmaUpFacingRight, 1, 1),
            Show(P.ChargedPlasmaUpFacingRight, 2, 1),
            Show(P.ChargedPlasmaUpFacingRight, 3, 1),
            Show(P.ChargedPlasmaUpFacingRight, 4, 1),
            Show(P.ChargedPlasmaUpFacingRight, 5, 1),
        ],
        [P.ChargedPlasmaUpFacingRightLoop] =
        [
            Show(P.ChargedPlasmaUpFacingRightLoop, 0, 1),
            Show(P.ChargedPlasmaUpFacingRightLoop, 1, 1),
        ],
        [P.ChargedPlasmaUpRight] =
        [
            Show(P.ChargedPlasmaUpRight, 0, 1),
            Show(P.ChargedPlasmaUpRight, 1, 1),
            Show(P.ChargedPlasmaUpRight, 2, 1),
            Show(P.ChargedPlasmaUpRight, 3, 1),
            Show(P.ChargedPlasmaUpRight, 4, 1),
            Show(P.ChargedPlasmaUpRight, 5, 1),
        ],
        [P.ChargedPlasmaUpRightLoop] =
        [
            Show(P.ChargedPlasmaUpRightLoop, 0, 1),
            Show(P.ChargedPlasmaUpRightLoop, 1, 1),
        ],
        [P.ChargedPlasmaRight] =
        [
            Show(P.ChargedPlasmaRight, 0, 1),
            Show(P.ChargedPlasmaRight, 1, 1),
            Show(P.ChargedPlasmaRight, 2, 1),
            Show(P.ChargedPlasmaRight, 3, 1),
            Show(P.ChargedPlasmaRight, 4, 1),
            Show(P.ChargedPlasmaRight, 5, 1),
        ],
        [P.ChargedPlasmaRightLoop] =
        [
            Show(P.ChargedPlasmaRightLoop, 0, 1),
            Show(P.ChargedPlasmaRightLoop, 1, 1),
        ],
        [P.ChargedPlasmaDownRight] =
        [
            Show(P.ChargedPlasmaDownRight, 0, 1),
            Show(P.ChargedPlasmaDownRight, 1, 1),
            Show(P.ChargedPlasmaDownRight, 2, 1),
            Show(P.ChargedPlasmaDownRight, 3, 1),
            Show(P.ChargedPlasmaDownRight, 4, 1),
            Show(P.ChargedPlasmaDownRight, 5, 1),
        ],
        [P.ChargedPlasmaDownRightLoop] =
        [
            Show(P.ChargedPlasmaDownRightLoop, 0, 1),
            Show(P.ChargedPlasmaDownRightLoop, 1, 1),
        ],
        [P.ChargedPlasmaWaveUpFacingRight] =
        [
            Show(P.ChargedPlasmaWaveUpFacingRight, 0, 1),
            Show(P.ChargedPlasmaWaveUpFacingRight, 1, 1),
            Show(P.ChargedPlasmaWaveUpFacingRight, 2, 1),
            Show(P.ChargedPlasmaWaveUpFacingRight, 3, 1),
            Show(P.ChargedPlasmaWaveUpFacingRight, 4, 1),
            Show(P.ChargedPlasmaWaveUpFacingRight, 5, 1),
        ],
        [P.ChargedPlasmaWaveUpFacingRightLoop] =
        [
            Show(P.ChargedPlasmaWaveUpFacingRightLoop, 0, 1),
            Show(P.ChargedPlasmaWaveUpFacingRightLoop, 1, 1),
            Show(P.ChargedPlasmaWaveUpFacingRightLoop, 2, 1),
            Show(P.ChargedPlasmaWaveUpFacingRightLoop, 3, 1),
            Show(P.ChargedPlasmaWaveUpFacingRightLoop, 4, 1),
            Show(P.ChargedPlasmaWaveUpFacingRightLoop, 5, 1),
            Show(P.ChargedPlasmaWaveUpFacingRightLoop, 6, 1),
            Show(P.ChargedPlasmaWaveUpFacingRightLoop, 7, 1),
            Show(P.ChargedPlasmaWaveUpFacingRightLoop, 8, 1),
            Show(P.ChargedPlasmaWaveUpFacingRightLoop, 9, 1),
            Show(P.ChargedPlasmaWaveUpFacingRightLoop, 10, 1),
            Show(P.ChargedPlasmaWaveUpFacingRightLoop, 11, 1),
            Show(P.ChargedPlasmaWaveUpFacingRightLoop, 12, 1),
            Show(P.ChargedPlasmaWaveUpFacingRightLoop, 13, 1),
            Show(P.ChargedPlasmaWaveUpFacingRightLoop, 14, 1),
            Show(P.ChargedPlasmaWaveUpFacingRightLoop, 15, 1),
        ],
        [P.ChargedPlasmaWaveUpRight] =
        [
            Show(P.ChargedPlasmaWaveUpRight, 0, 1),
            Show(P.ChargedPlasmaWaveUpRight, 1, 1),
            Show(P.ChargedPlasmaWaveUpRight, 2, 1),
            Show(P.ChargedPlasmaWaveUpRight, 3, 1),
            Show(P.ChargedPlasmaWaveUpRight, 4, 1),
            Show(P.ChargedPlasmaWaveUpRight, 5, 1),
        ],
        [P.ChargedPlasmaWaveUpRightLoop] =
        [
            Show(P.ChargedPlasmaWaveUpRightLoop, 0, 1),
            Show(P.ChargedPlasmaWaveUpRightLoop, 1, 1),
            Show(P.ChargedPlasmaWaveUpRightLoop, 2, 1),
            Show(P.ChargedPlasmaWaveUpRightLoop, 3, 1),
            Show(P.ChargedPlasmaWaveUpRightLoop, 4, 1),
            Show(P.ChargedPlasmaWaveUpRightLoop, 5, 1),
            Show(P.ChargedPlasmaWaveUpRightLoop, 6, 1),
            Show(P.ChargedPlasmaWaveUpRightLoop, 7, 1),
            Show(P.ChargedPlasmaWaveUpRightLoop, 8, 1),
            Show(P.ChargedPlasmaWaveUpRightLoop, 9, 1),
            Show(P.ChargedPlasmaWaveUpRightLoop, 10, 1),
            Show(P.ChargedPlasmaWaveUpRightLoop, 11, 1),
            Show(P.ChargedPlasmaWaveUpRightLoop, 12, 1),
            Show(P.ChargedPlasmaWaveUpRightLoop, 13, 1),
            Show(P.ChargedPlasmaWaveUpRightLoop, 14, 1),
            Show(P.ChargedPlasmaWaveUpRightLoop, 15, 1),
        ],
        [P.ChargedPlasmaWaveRight] =
        [
            Show(P.ChargedPlasmaWaveRight, 0, 1),
            Show(P.ChargedPlasmaWaveRight, 1, 1),
            Show(P.ChargedPlasmaWaveRight, 2, 1),
            Show(P.ChargedPlasmaWaveRight, 3, 1),
            Show(P.ChargedPlasmaWaveRight, 4, 1),
            Show(P.ChargedPlasmaWaveRight, 5, 1),
        ],
        [P.ChargedPlasmaWaveRightLoop] =
        [
            Show(P.ChargedPlasmaWaveRightLoop, 0, 1),
            Show(P.ChargedPlasmaWaveRightLoop, 1, 1),
            Show(P.ChargedPlasmaWaveRightLoop, 2, 1),
            Show(P.ChargedPlasmaWaveRightLoop, 3, 1),
            Show(P.ChargedPlasmaWaveRightLoop, 4, 1),
            Show(P.ChargedPlasmaWaveRightLoop, 5, 1),
            Show(P.ChargedPlasmaWaveRightLoop, 6, 1),
            Show(P.ChargedPlasmaWaveRightLoop, 7, 1),
            Show(P.ChargedPlasmaWaveRightLoop, 8, 1),
            Show(P.ChargedPlasmaWaveRightLoop, 9, 1),
            Show(P.ChargedPlasmaWaveRightLoop, 10, 1),
            Show(P.ChargedPlasmaWaveRightLoop, 11, 1),
            Show(P.ChargedPlasmaWaveRightLoop, 12, 1),
            Show(P.ChargedPlasmaWaveRightLoop, 13, 1),
            Show(P.ChargedPlasmaWaveRightLoop, 14, 1),
            Show(P.ChargedPlasmaWaveRightLoop, 15, 1),
        ],
        [P.ChargedPlasmaWaveDownRight] =
        [
            Show(P.ChargedPlasmaWaveDownRight, 0, 1),
            Show(P.ChargedPlasmaWaveDownRight, 1, 1),
            Show(P.ChargedPlasmaWaveDownRight, 2, 1),
            Show(P.ChargedPlasmaWaveDownRight, 3, 1),
            Show(P.ChargedPlasmaWaveDownRight, 4, 1),
            Show(P.ChargedPlasmaWaveDownRight, 5, 1),
        ],
        [P.ChargedPlasmaWaveDownRightLoop] =
        [
            Show(P.ChargedPlasmaWaveDownRightLoop, 0, 1),
            Show(P.ChargedPlasmaWaveDownRightLoop, 1, 1),
            Show(P.ChargedPlasmaWaveDownRightLoop, 2, 1),
            Show(P.ChargedPlasmaWaveDownRightLoop, 3, 1),
            Show(P.ChargedPlasmaWaveDownRightLoop, 4, 1),
            Show(P.ChargedPlasmaWaveDownRightLoop, 5, 1),
            Show(P.ChargedPlasmaWaveDownRightLoop, 6, 1),
            Show(P.ChargedPlasmaWaveDownRightLoop, 7, 1),
            Show(P.ChargedPlasmaWaveDownRightLoop, 8, 1),
            Show(P.ChargedPlasmaWaveDownRightLoop, 9, 1),
            Show(P.ChargedPlasmaWaveDownRightLoop, 10, 1),
            Show(P.ChargedPlasmaWaveDownRightLoop, 11, 1),
            Show(P.ChargedPlasmaWaveDownRightLoop, 12, 1),
            Show(P.ChargedPlasmaWaveDownRightLoop, 13, 1),
            Show(P.ChargedPlasmaWaveDownRightLoop, 14, 1),
            Show(P.ChargedPlasmaWaveDownRightLoop, 15, 1),
        ],
        [P.MissileUpFacingRight] =
        [
            Show(P.MissileUpFacingRight, 0, 15),
        ],
        [P.MissileUpRight] =
        [
            Show(P.MissileUpRight, 0, 15),
        ],
        [P.MissileRight] =
        [
            Show(P.MissileRight, 0, 15),
        ],
        [P.MissileDownRight] =
        [
            Show(P.MissileDownRight, 0, 15),
        ],
        [P.MissileDownFacingRight] =
        [
            Show(P.MissileDownFacingRight, 0, 15),
        ],
        [P.MissileDownLeft] =
        [
            Show(P.MissileDownLeft, 0, 15),
        ],
        [P.MissileLeft] =
        [
            Show(P.MissileLeft, 0, 15),
        ],
        [P.MissileUpLeft] =
        [
            Show(P.MissileUpLeft, 0, 15),
        ],
        [P.SuperMissileUpFacingRight] =
        [
            Show(P.SuperMissileUpFacingRight, 0, 15),
        ],
        [P.SuperMissileUpRight] =
        [
            Show(P.SuperMissileUpRight, 0, 15),
        ],
        [P.SuperMissileRight] =
        [
            Show(P.SuperMissileRight, 0, 15),
        ],
        [P.SuperMissileDownRight] =
        [
            Show(P.SuperMissileDownRight, 0, 15),
        ],
        [P.SuperMissileDownFacingRight] =
        [
            Show(P.SuperMissileDownFacingRight, 0, 15),
        ],
        [P.SuperMissileDownLeft] =
        [
            Show(P.SuperMissileDownLeft, 0, 15),
        ],
        [P.SuperMissileLeft] =
        [
            Show(P.SuperMissileLeft, 0, 15),
        ],
        [P.SuperMissileUpLeft] =
        [
            Show(P.SuperMissileUpLeft, 0, 15),
        ],
        [P.SuperMissileLink] =
        [
            Show(P.SuperMissileLink, 0, 15),
        ],
        [P.PowerBomb] =
        [
            Show(P.PowerBomb, 0, 5),
            Show(P.PowerBomb, 1, 5),
            Show(P.PowerBomb, 2, 5),
        ],
        [P.FastPowerBomb] =
        [
            Show(P.FastPowerBomb, 0, 1),
            Show(P.FastPowerBomb, 1, 1),
            Show(P.FastPowerBomb, 2, 1),
        ],
        [P.Bomb] =
        [
            Show(P.Bomb, 0, 5),
            Show(P.Bomb, 1, 5),
            Show(P.Bomb, 2, 5),
            Show(P.Bomb, 3, 5),
        ],
        [P.FastBomb] =
        [
            Show(P.FastBomb, 0, 1),
            Show(P.FastBomb, 1, 1),
            Show(P.FastBomb, 2, 1),
            Show(P.FastBomb, 3, 1),
        ],
        [P.BeamExplosion] =
        [
            Show(P.BeamExplosion, 0, 3),
            Show(P.BeamExplosion, 1, 3),
            Show(P.BeamExplosion, 2, 3),
            Show(P.BeamExplosion, 3, 3),
            Show(P.BeamExplosion, 4, 3),
            Show(P.BeamExplosion, 5, 3),
        ],
        [P.MissileExplosion] =
        [
            Show(P.MissileExplosion, 0, 3),
            Show(P.MissileExplosion, 1, 3),
            Show(P.MissileExplosion, 2, 3),
            Show(P.MissileExplosion, 3, 3),
            Show(P.MissileExplosion, 4, 3),
            Show(P.MissileExplosion, 5, 3),
        ],
        [P.BombExplosion] =
        [
            Show(P.BombExplosion, 0, 2),
            Show(P.BombExplosion, 1, 2),
            Show(P.BombExplosion, 2, 2),
            Show(P.BombExplosion, 3, 2),
            Show(P.BombExplosion, 4, 2),
        ],
        [P.PlasmaSBA] =
        [
            Show(P.PlasmaSBA, 0, 2),
            Show(P.PlasmaSBA, 1, 2),
            Show(P.PlasmaSBA, 2, 2),
            Show(P.PlasmaSBA, 3, 2),
            Show(P.PlasmaSBA, 4, 2),
        ],
        [P.SuperMissileExplosion] =
        [
            Show(P.SuperMissileExplosion, 0, 5),
            Show(P.SuperMissileExplosion, 1, 5),
            Show(P.SuperMissileExplosion, 2, 5),
            Show(P.SuperMissileExplosion, 3, 5),
            Show(P.SuperMissileExplosion, 4, 5),
            Show(P.SuperMissileExplosion, 5, 5),
        ],
        [P.ShinesparkEchoUpFacingRight] =
        [
            Show(P.ShinesparkEchoUpFacingRight, 0, 2),
            Show(P.ShinesparkEchoUpFacingRight, 1, 2),
            Show(P.ShinesparkEchoUpFacingRight, 2, 2),
            Show(P.ShinesparkEchoUpFacingRight, 3, 2),
        ],
        [P.SpazerSBATrailUpFacingRight] =
        [
            Show(P.SpazerSBATrailUpFacingRight, 0, 2),
            Show(P.SpazerSBATrailUpFacingRight, 1, 2),
        ],
        [P.SpazerSBATrailUpFacingRightLoop] =
        [
            Show(P.SpazerSBATrailUpFacingRightLoop, 0, 2),
        ],
        [P.WaveSBA] =
        [
            Show(P.WaveSBA, 0, 8),
            Show(P.WaveSBA, 1, 8),
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
        [P.PlasmaSBA] = new([new(P.PlasmaSBA, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SuperMissileExplosion] = new([new(P.SuperMissileExplosion, 0, 6, 1)], AnimationEnd.Stops),
        [P.ShinesparkEchoUpFacingRight] = new([new(P.ShinesparkEchoUpFacingRight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SpazerSBATrailUpFacingRight] = new([new(P.SpazerSBATrailUpFacingRight, 0, 2, 1), new(P.SpazerSBATrailUpFacingRightLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SpazerSBATrailUpFacingRightLoop] = new([new(P.SpazerSBATrailUpFacingRightLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.WaveSBA] = new([new(P.WaveSBA, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<SamusProjectileSegment> StartingWith(SamusProjectileSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="segment"/>'s frame <paramref name="show"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<SamusProjectileFrame, NoAnimationCue> Show(SamusProjectileSegment segment, int show, int duration) => new AnimationBeat<SamusProjectileFrame, NoAnimationCue>.Frame(new(segment, show), duration);
}
