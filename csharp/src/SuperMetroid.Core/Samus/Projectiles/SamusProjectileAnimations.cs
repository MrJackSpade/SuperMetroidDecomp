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
            Show(F.WaveDownFacingRightRightNear, 1),
            Show(F.WaveDownFacingRightRightMidway, 1),
            Show(F.WaveDownFacingRightRightFar, 1),
            Show(F.WaveDownFacingRightRightPeak, 1),
            Show(F.WaveDownFacingRightRightFar, 1),
            Show(F.WaveDownFacingRightRightMidway, 1),
            Show(F.WaveDownFacingRightRightNear, 1),
            Show(F.WaveDownFacingRightCentre, 1),
            Show(F.WaveDownFacingRightLeftNear, 1),
            Show(F.WaveDownFacingRightLeftMidway, 1),
            Show(F.WaveDownFacingRightLeftFar, 1),
            Show(F.WaveDownFacingRightLeftPeak, 1),
            Show(F.WaveDownFacingRightLeftFar, 1),
            Show(F.WaveDownFacingRightLeftMidway, 1),
            Show(F.WaveDownFacingRightLeftNear, 1),
        ],
        [P.WaveUpRight] =
        [
            Show(F.WaveUpRightCentre, 1),
            Show(F.WaveUpRightUpLeftNear, 1),
            Show(F.WaveUpRightUpLeftMidway, 1),
            Show(F.WaveUpRightUpLeftFar, 1),
            Show(F.WaveUpRightUpLeftPeak, 1),
            Show(F.WaveUpRightUpLeftFar, 1),
            Show(F.WaveUpRightUpLeftMidway, 1),
            Show(F.WaveUpRightUpLeftNearNarrowHitbox, 1),
            Show(F.WaveUpRightCentreNarrowHitbox, 1),
            Show(F.WaveUpRightDownRightNear, 1),
            Show(F.WaveUpRightDownRightMidway, 1),
            Show(F.WaveUpRightDownRightFar, 1),
            Show(F.WaveUpRightDownRightPeak, 1),
            Show(F.WaveUpRightDownRightFar, 1),
            Show(F.WaveUpRightDownRightMidway, 1),
            Show(F.WaveUpRightDownRightNearWideHitbox, 1),
        ],
        [P.WaveRight] =
        [
            Show(F.WaveRightCentre, 1),
            Show(F.WaveRightUpNear, 1),
            Show(F.WaveRightUpMidway, 1),
            Show(F.WaveRightUpFar, 1),
            Show(F.WaveRightUpPeak, 1),
            Show(F.WaveRightUpFar, 1),
            Show(F.WaveRightUpMidway, 1),
            Show(F.WaveRightUpNear, 1),
            Show(F.WaveRightCentre, 1),
            Show(F.WaveRightDownNear, 1),
            Show(F.WaveRightDownMidway, 1),
            Show(F.WaveRightDownFar, 1),
            Show(F.WaveRightDownPeak, 1),
            Show(F.WaveRightDownFar, 1),
            Show(F.WaveRightDownMidway, 1),
            Show(F.WaveRightDownNear, 1),
        ],
        [P.WaveDownRight] =
        [
            Show(F.WaveDownRightCentre, 1),
            Show(F.WaveDownRightUpRightNear, 1),
            Show(F.WaveDownRightUpRightMidway, 1),
            Show(F.WaveDownRightUpRightFar, 1),
            Show(F.WaveDownRightUpRightPeak, 1),
            Show(F.WaveDownRightUpRightFar, 1),
            Show(F.WaveDownRightUpRightMidway, 1),
            Show(F.WaveDownRightUpRightNearNarrowHitbox, 1),
            Show(F.WaveDownRightCentreNarrowHitbox, 1),
            Show(F.WaveDownRightDownLeftNear, 1),
            Show(F.WaveDownRightDownLeftMidway, 1),
            Show(F.WaveDownRightDownLeftFar, 1),
            Show(F.WaveDownRightDownLeftPeak, 1),
            Show(F.WaveDownRightDownLeftFar, 1),
            Show(F.WaveDownRightDownLeftMidway, 1),
            Show(F.WaveDownRightDownLeftNearWideHitbox, 1),
        ],
        [P.IceUpFacingRight] =
        [
            Show(F.IceUpFacingRightStar, 1),
            Show(F.IceUpFacingRightCrossed, 1),
            Show(F.IceUpFacingRightNarrowStar, 1),
            Show(F.IceUpFacingRightCrossedMirrored, 1),
        ],
        [P.SpazerUpFacingRight] =
        [
            Show(F.SpazerUpFacingRightSeed, 2),
            Show(F.SpazerUpFacingRightParting, 2),
        ],
        [P.SpazerUpFacingRightLoop] =
        [
            Show(F.SpazerUpFacingRightSpread, 2),
        ],
        [P.SpazerUpRight] =
        [
            Show(F.SpazerUpRightSeed, 2),
            Show(F.SpazerUpRightParting, 2),
        ],
        [P.SpazerUpRightLoop] =
        [
            Show(F.SpazerUpRightSpread, 2),
        ],
        [P.SpazerRight] =
        [
            Show(F.SpazerRightSeed, 2),
            Show(F.SpazerRightParting, 2),
        ],
        [P.SpazerRightLoop] =
        [
            Show(F.SpazerRightSpread, 2),
        ],
        [P.SpazerDownRight] =
        [
            Show(F.SpazerDownRightSeed, 2),
            Show(F.SpazerDownRightParting, 2),
        ],
        [P.SpazerDownRightLoop] =
        [
            Show(F.SpazerDownRightSpread, 2),
        ],
        [P.SpazerDownFacingRight] =
        [
            Show(F.SpazerDownFacingRightSeed, 2),
            Show(F.SpazerDownFacingRightParting, 2),
        ],
        [P.SpazerDownFacingRightLoop] =
        [
            Show(F.SpazerDownFacingRightSpread, 2),
        ],
        [P.SpazerDownLeft] =
        [
            Show(F.SpazerDownLeftSeed, 2),
            Show(F.SpazerDownLeftParting, 2),
        ],
        [P.SpazerDownLeftLoop] =
        [
            Show(F.SpazerDownLeftSpread, 2),
        ],
        [P.SpazerLeft] =
        [
            Show(F.SpazerLeftSeed, 2),
            Show(F.SpazerLeftParting, 2),
        ],
        [P.SpazerLeftLoop] =
        [
            Show(F.SpazerLeftSpread, 2),
        ],
        [P.SpazerUpLeft] =
        [
            Show(F.SpazerUpLeftSeed, 2),
            Show(F.SpazerUpLeftParting, 2),
        ],
        [P.SpazerUpLeftLoop] =
        [
            Show(F.SpazerUpLeftSpread, 2),
        ],
        [P.SpazerWaveUpFacingRight] =
        [
            Show(F.SpazerWaveUpFacingRightSeed, 2),
            Show(F.SpazerWaveUpFacingRightSplitting, 2),
            Show(F.SpazerWaveUpFacingRightParting, 2),
            Show(F.SpazerWaveUpFacingRightSpreading, 2),
            Show(F.SpazerWaveUpFacingRightNearlySpread, 2),
            Show(F.SpazerWaveUpFacingRightSpread, 2),
            Show(F.SpazerWaveUpFacingRightNearlySpread, 2),
            Show(F.SpazerWaveUpFacingRightSpreading, 2),
            Show(F.SpazerWaveUpFacingRightPartingWithoutDefaultTrail, 2),
            Show(F.SpazerWaveUpFacingRightSplittingWithoutDefaultTrail, 2),
        ],
        [P.SpazerWaveUpRight] =
        [
            Show(F.SpazerWaveUpRightSeed, 2),
            Show(F.SpazerWaveUpRightSplitting, 2),
            Show(F.SpazerWaveUpRightParting, 2),
            Show(F.SpazerWaveUpRightSpreading, 2),
            Show(F.SpazerWaveUpRightNearlySpread, 2),
            Show(F.SpazerWaveUpRightSpread, 2),
            Show(F.SpazerWaveUpRightNearlySpread, 2),
            Show(F.SpazerWaveUpRightSpreading, 2),
            Show(F.SpazerWaveUpRightPartingWithoutDefaultTrail, 2),
            Show(F.SpazerWaveUpRightSplittingWithoutDefaultTrail, 2),
        ],
        [P.SpazerWaveRight] =
        [
            Show(F.SpazerWaveRightSeed, 2),
            Show(F.SpazerWaveRightSplitting, 2),
            Show(F.SpazerWaveRightParting, 2),
            Show(F.SpazerWaveRightSpreading, 2),
            Show(F.SpazerWaveRightNearlySpread, 2),
            Show(F.SpazerWaveRightSpread, 2),
            Show(F.SpazerWaveRightNearlySpread, 2),
            Show(F.SpazerWaveRightSpreading, 2),
            Show(F.SpazerWaveRightPartingWithoutDefaultTrail, 2),
            Show(F.SpazerWaveRightSplittingWithoutDefaultTrail, 2),
        ],
        [P.SpazerWaveDownRight] =
        [
            Show(F.SpazerWaveDownRightSeed, 2),
            Show(F.SpazerWaveDownRightSplitting, 2),
            Show(F.SpazerWaveDownRightParting, 2),
            Show(F.SpazerWaveDownRightSpreading, 2),
            Show(F.SpazerWaveDownRightNearlySpread, 2),
            Show(F.SpazerWaveDownRightSpread, 2),
            Show(F.SpazerWaveDownRightNearlySpread, 2),
            Show(F.SpazerWaveDownRightSpreading, 2),
            Show(F.SpazerWaveDownRightPartingWithoutDefaultTrail, 2),
            Show(F.SpazerWaveDownRightSplittingWithoutDefaultTrail, 2),
        ],
        [P.SpazerWaveDownFacingRight] =
        [
            Show(F.SpazerWaveDownFacingRightSeed, 2),
            Show(F.SpazerWaveDownFacingRightSplitting, 2),
            Show(F.SpazerWaveDownFacingRightParting, 2),
            Show(F.SpazerWaveDownFacingRightSpreading, 2),
            Show(F.SpazerWaveDownFacingRightNearlySpread, 2),
            Show(F.SpazerWaveDownFacingRightSpread, 2),
            Show(F.SpazerWaveDownFacingRightNearlySpread, 2),
            Show(F.SpazerWaveDownFacingRightSpreading, 2),
            Show(F.SpazerWaveDownFacingRightPartingWithoutDefaultTrail, 2),
            Show(F.SpazerWaveDownFacingRightSplittingWithoutDefaultTrail, 2),
        ],
        [P.SpazerWaveDownLeft] =
        [
            Show(F.SpazerWaveDownLeftSeed, 2),
            Show(F.SpazerWaveDownLeftParting, 2),
            Show(F.SpazerWaveDownLeftSpreading, 2),
            Show(F.SpazerWaveDownLeftNearlySpread, 2),
            Show(F.SpazerWaveDownLeftSpread, 2),
            Show(F.SpazerWaveDownLeftSpreadShiftedTrail, 2),
            Show(F.SpazerWaveDownLeftSpread, 2),
            Show(F.SpazerWaveDownLeftNearlySpread, 2),
            Show(F.SpazerWaveDownLeftSpreadingWithoutDefaultTrail, 2),
            Show(F.SpazerWaveDownLeftPartingWithoutDefaultTrail, 2),
        ],
        [P.SpazerWaveLeft] =
        [
            Show(F.SpazerWaveLeftSeed, 2),
            Show(F.SpazerWaveLeftSplitting, 2),
            Show(F.SpazerWaveLeftParting, 2),
            Show(F.SpazerWaveLeftSpreading, 2),
            Show(F.SpazerWaveLeftNearlySpread, 2),
            Show(F.SpazerWaveLeftSpread, 2),
            Show(F.SpazerWaveLeftNearlySpread, 2),
            Show(F.SpazerWaveLeftSpreading, 2),
            Show(F.SpazerWaveLeftPartingWithoutDefaultTrail, 2),
            Show(F.SpazerWaveLeftSplittingWithoutDefaultTrail, 2),
        ],
        [P.SpazerWaveUpLeft] =
        [
            Show(F.SpazerWaveUpLeftSeed, 2),
            Show(F.SpazerWaveUpLeftSplitting, 2),
            Show(F.SpazerWaveUpLeftParting, 2),
            Show(F.SpazerWaveUpLeftSpreading, 2),
            Show(F.SpazerWaveUpLeftNearlySpread, 2),
            Show(F.SpazerWaveUpLeftSpread, 2),
            Show(F.SpazerWaveUpLeftNearlySpread, 2),
            Show(F.SpazerWaveUpLeftSpreading, 2),
            Show(F.SpazerWaveUpLeftPartingWithoutDefaultTrail, 2),
            Show(F.SpazerWaveUpLeftSplittingWithoutDefaultTrail, 2),
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
            Show(F.PlasmaWaveUpFacingRightShortLobesNear, 2),
            Show(F.PlasmaWaveUpFacingRightShortLobesMidway, 2),
            Show(F.PlasmaWaveUpFacingRightShortLobesFar, 2),
            Show(F.PlasmaWaveUpFacingRightShortLobesPeak, 2),
            Show(F.PlasmaWaveUpFacingRightShortLobesFar, 2),
            Show(F.PlasmaWaveUpFacingRightShortLobesMidway, 2),
            Show(F.PlasmaWaveUpFacingRightShortLobesNearWithoutDefaultTrail, 2),
        ],
        [P.PlasmaIceWaveUpRight] =
        [
            Show(F.PlasmaIceWaveUpRightCore, 1),
        ],
        [P.PlasmaWaveUpRight] =
        [
            Show(F.PlasmaWaveUpRightShortCore, 2),
            Show(F.PlasmaWaveUpRightShortLobesNear, 2),
            Show(F.PlasmaWaveUpRightShortLobesMidway, 2),
            Show(F.PlasmaWaveUpRightShortLobesFar, 2),
            Show(F.PlasmaWaveUpRightShortLobesPeak, 2),
            Show(F.PlasmaWaveUpRightShortLobesFar, 2),
            Show(F.PlasmaWaveUpRightShortLobesMidway, 2),
            Show(F.PlasmaWaveUpRightShortLobesNearWithoutDefaultTrail, 2),
        ],
        [P.PlasmaWaveRight] =
        [
            Show(F.PlasmaWaveRightCore, 1),
        ],
        [P.PlasmaWaveRightLoop] =
        [
            Show(F.PlasmaWaveRightShortCore, 2),
            Show(F.PlasmaWaveRightShortLobesNear, 2),
            Show(F.PlasmaWaveRightShortLobesMidway, 2),
            Show(F.PlasmaWaveRightShortLobesFar, 2),
            Show(F.PlasmaWaveRightShortLobesPeak, 2),
            Show(F.PlasmaWaveRightShortLobesFar, 2),
            Show(F.PlasmaWaveRightShortLobesMidway, 2),
            Show(F.PlasmaWaveRightShortLobesNearWithoutDefaultTrail, 2),
        ],
        [P.PlasmaIceWaveDownRight] =
        [
            Show(F.PlasmaIceWaveDownRightCore, 1),
        ],
        [P.PlasmaWaveDownRight] =
        [
            Show(F.PlasmaWaveDownRightShortCore, 2),
            Show(F.PlasmaWaveDownRightShortLobesNear, 2),
            Show(F.PlasmaWaveDownRightShortLobesMidway, 2),
            Show(F.PlasmaWaveDownRightShortLobesFar, 2),
            Show(F.PlasmaWaveDownRightShortLobesPeak, 2),
            Show(F.PlasmaWaveDownRightShortLobesFar, 2),
            Show(F.PlasmaWaveDownRightShortLobesMidway, 2),
            Show(F.PlasmaWaveDownRightShortLobesNearWithoutDefaultTrail, 2),
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
            Show(F.ChargedWaveDownFacingRightLobesNear, 1),
            Show(F.ChargedWaveDownFacingRightLobesNearAlternate, 1),
            Show(F.ChargedWaveDownFacingRightLobesMidway, 1),
            Show(F.ChargedWaveDownFacingRightLobesMidwayAlternate, 1),
            Show(F.ChargedWaveDownFacingRightLobesFar, 1),
            Show(F.ChargedWaveDownFacingRightLobesFarAlternate, 1),
            Show(F.ChargedWaveDownFacingRightLobesPeak, 1),
            Show(F.ChargedWaveDownFacingRightLobesPeakAlternate, 1),
            Show(F.ChargedWaveDownFacingRightLobesFar, 1),
            Show(F.ChargedWaveDownFacingRightLobesFarAlternate, 1),
            Show(F.ChargedWaveDownFacingRightLobesMidway, 1),
            Show(F.ChargedWaveDownFacingRightLobesMidwayAlternate, 1),
            Show(F.ChargedWaveDownFacingRightLobesNearAlternate, 1),
            Show(F.ChargedWaveDownFacingRightLobesNear, 1),
        ],
        [P.ChargedWaveUpRight] =
        [
            Show(F.ChargedWaveUpRightCore, 1),
            Show(F.ChargedWaveUpRightCoreAlternate, 1),
            Show(F.ChargedWaveUpRightLobesNear, 1),
            Show(F.ChargedWaveUpRightLobesNearAlternate, 1),
            Show(F.ChargedWaveUpRightLobesMidway, 1),
            Show(F.ChargedWaveUpRightLobesMidwayAlternate, 1),
            Show(F.ChargedWaveUpRightLobesFar, 1),
            Show(F.ChargedWaveUpRightLobesFarAlternate, 1),
            Show(F.ChargedWaveUpRightLobesPeak, 1),
            Show(F.ChargedWaveUpRightLobesPeakAlternate, 1),
            Show(F.ChargedWaveUpRightLobesFar, 1),
            Show(F.ChargedWaveUpRightLobesFarAlternate, 1),
            Show(F.ChargedWaveUpRightLobesMidway, 1),
            Show(F.ChargedWaveUpRightLobesMidwayAlternate, 1),
            Show(F.ChargedWaveUpRightLobesNear, 1),
            Show(F.ChargedWaveUpRightLobesNearAlternate, 1),
        ],
        [P.ChargedWaveRight] =
        [
            Show(F.ChargedWaveRightCore, 1),
            Show(F.ChargedWaveRightCoreAlternate, 1),
            Show(F.ChargedWaveRightLobesNear, 1),
            Show(F.ChargedWaveRightLobesNearAlternate, 1),
            Show(F.ChargedWaveRightLobesMidway, 1),
            Show(F.ChargedWaveRightLobesMidwayAlternate, 1),
            Show(F.ChargedWaveRightLobesFar, 1),
            Show(F.ChargedWaveRightLobesFarAlternate, 1),
            Show(F.ChargedWaveRightLobesPeak, 1),
            Show(F.ChargedWaveRightLobesPeakAlternate, 1),
            Show(F.ChargedWaveRightLobesFar, 1),
            Show(F.ChargedWaveRightLobesFarAlternate, 1),
            Show(F.ChargedWaveRightLobesMidway, 1),
            Show(F.ChargedWaveRightLobesMidwayAlternate, 1),
            Show(F.ChargedWaveRightLobesNearAlternate, 1),
            Show(F.ChargedWaveRightLobesNear, 1),
        ],
        [P.ChargedWaveDownRight] =
        [
            Show(F.ChargedWaveDownRightCore, 1),
            Show(F.ChargedWaveDownRightCoreAlternate, 1),
            Show(F.ChargedWaveDownRightLobesNear, 1),
            Show(F.ChargedWaveDownRightLobesNearAlternate, 1),
            Show(F.ChargedWaveDownRightLobesMidway, 1),
            Show(F.ChargedWaveDownRightLobesMidwayAlternate, 1),
            Show(F.ChargedWaveDownRightLobesFar, 1),
            Show(F.ChargedWaveDownRightLobesFarAlternate, 1),
            Show(F.ChargedWaveDownRightLobesPeak, 1),
            Show(F.ChargedWaveDownRightLobesPeakAlternate, 1),
            Show(F.ChargedWaveDownRightLobesFar, 1),
            Show(F.ChargedWaveDownRightLobesFarAlternate, 1),
            Show(F.ChargedWaveDownRightLobesMidway, 1),
            Show(F.ChargedWaveDownRightLobesMidwayAlternate, 1),
            Show(F.ChargedWaveDownRightLobesNear, 1),
            Show(F.ChargedWaveDownRightLobesNearAlternate, 1),
        ],
        [P.ChargedIceUpFacingRight] =
        [
            Show(F.ChargedIceUpFacingRightCrossed, 1),
            Show(F.ChargedIceUpFacingRightStar, 1),
            Show(F.ChargedIceUpFacingRightCrossed, 1),
            Show(F.ChargedIceUpFacingRightStar, 1),
        ],
        [P.ChargedIceWaveUpFacingRight] =
        [
            Show(F.ChargedIceWaveUpFacingRightHidden, 3),
        ],
        [P.ChargedIceWaveDownFacingRight] =
        [
            Show(F.ChargedIceWaveDownFacingRightCore, 1),
            Show(F.ChargedIceWaveDownFacingRightCoreAlternate, 1),
            Show(F.ChargedIceWaveDownFacingRightLobesNear, 1),
            Show(F.ChargedIceWaveDownFacingRightLobesNearAlternate, 1),
            Show(F.ChargedIceWaveDownFacingRightLobesMidway, 1),
            Show(F.ChargedIceWaveDownFacingRightLobesMidwayAlternate, 1),
            Show(F.ChargedIceWaveDownFacingRightLobesFar, 1),
            Show(F.ChargedIceWaveDownFacingRightLobesFarAlternate, 1),
            Show(F.ChargedIceWaveDownFacingRightLobesPeak, 1),
            Show(F.ChargedIceWaveDownFacingRightLobesPeakAlternate, 1),
            Show(F.ChargedIceWaveDownFacingRightLobesFar, 1),
            Show(F.ChargedIceWaveDownFacingRightLobesFarAlternate, 1),
            Show(F.ChargedIceWaveDownFacingRightLobesMidway, 1),
            Show(F.ChargedIceWaveDownFacingRightLobesMidwayAlternate, 1),
            Show(F.ChargedIceWaveDownFacingRightLobesNearAlternate, 1),
            Show(F.ChargedIceWaveDownFacingRightLobesNear, 1),
        ],
        [P.ChargedIceWaveUpRight] =
        [
            Show(F.ChargedIceWaveUpRightCore, 1),
            Show(F.ChargedIceWaveUpRightCoreAlternate, 1),
            Show(F.ChargedIceWaveUpRightLobesNear, 1),
            Show(F.ChargedIceWaveUpRightLobesNearAlternate, 1),
            Show(F.ChargedIceWaveUpRightLobesMidway, 1),
            Show(F.ChargedIceWaveUpRightLobesMidwayAlternate, 1),
            Show(F.ChargedIceWaveUpRightLobesFar, 1),
            Show(F.ChargedIceWaveUpRightLobesFarAlternate, 1),
            Show(F.ChargedIceWaveUpRightLobesPeak, 1),
            Show(F.ChargedIceWaveUpRightLobesPeakAlternate, 1),
            Show(F.ChargedIceWaveUpRightLobesFar, 1),
            Show(F.ChargedIceWaveUpRightLobesFarAlternate, 1),
            Show(F.ChargedIceWaveUpRightLobesMidway, 1),
            Show(F.ChargedIceWaveUpRightLobesMidwayAlternate, 1),
            Show(F.ChargedIceWaveUpRightLobesNear, 1),
            Show(F.ChargedIceWaveUpRightLobesNearAlternate, 1),
        ],
        [P.ChargedIceWaveRight] =
        [
            Show(F.ChargedIceWaveRightCore, 1),
            Show(F.ChargedIceWaveRightCoreAlternate, 1),
            Show(F.ChargedIceWaveRightLobesNear, 1),
            Show(F.ChargedIceWaveRightLobesNearAlternate, 1),
            Show(F.ChargedIceWaveRightLobesMidway, 1),
            Show(F.ChargedIceWaveRightLobesMidwayAlternate, 1),
            Show(F.ChargedIceWaveRightLobesFar, 1),
            Show(F.ChargedIceWaveRightLobesFarAlternate, 1),
            Show(F.ChargedIceWaveRightLobesPeak, 1),
            Show(F.ChargedIceWaveRightLobesPeakAlternate, 1),
            Show(F.ChargedIceWaveRightLobesFar, 1),
            Show(F.ChargedIceWaveRightLobesFarAlternate, 1),
            Show(F.ChargedIceWaveRightLobesMidway, 1),
            Show(F.ChargedIceWaveRightLobesMidwayAlternate, 1),
            Show(F.ChargedIceWaveRightLobesNearAlternate, 1),
            Show(F.ChargedIceWaveRightLobesNear, 1),
        ],
        [P.ChargedIceWaveDownRight] =
        [
            Show(F.ChargedIceWaveDownRightCore, 1),
            Show(F.ChargedIceWaveDownRightCoreAlternate, 1),
            Show(F.ChargedIceWaveDownRightLobesNear, 1),
            Show(F.ChargedIceWaveDownRightLobesNearAlternate, 1),
            Show(F.ChargedIceWaveDownRightLobesMidway, 1),
            Show(F.ChargedIceWaveDownRightLobesMidwayAlternate, 1),
            Show(F.ChargedIceWaveDownRightLobesFar, 1),
            Show(F.ChargedIceWaveDownRightLobesFarAlternate, 1),
            Show(F.ChargedIceWaveDownRightLobesPeak, 1),
            Show(F.ChargedIceWaveDownRightLobesPeakAlternate, 1),
            Show(F.ChargedIceWaveDownRightLobesFar, 1),
            Show(F.ChargedIceWaveDownRightLobesFarAlternate, 1),
            Show(F.ChargedIceWaveDownRightLobesMidway, 1),
            Show(F.ChargedIceWaveDownRightLobesMidwayAlternate, 1),
            Show(F.ChargedIceWaveDownRightLobesNear, 1),
            Show(F.ChargedIceWaveDownRightLobesNearAlternate, 1),
        ],
        [P.ChargedSpazerUpFacingRight] =
        [
            Show(F.ChargedSpazerUpFacingRightStartupShort, 1),
            Show(F.ChargedSpazerUpFacingRightVerticalAlternateStartupShort, 1),
            Show(F.ChargedSpazerUpFacingRightStartupLong, 1),
            Show(F.ChargedSpazerUpFacingRightVerticalAlternateStartupLong, 1),
            Show(F.ChargedSpazerUpFacingRightSeed, 1),
            Show(F.ChargedSpazerUpFacingRightVerticalAlternateSeed, 1),
            Show(F.ChargedSpazerUpFacingRightParting, 1),
            Show(F.ChargedSpazerUpFacingRightVerticalAlternateParting, 1),
        ],
        [P.ChargedSpazerUpFacingRightLoop] =
        [
            Show(F.ChargedSpazerUpFacingRightSpread, 1),
            Show(F.ChargedSpazerUpFacingRightVerticalAlternateSpread, 1),
        ],
        [P.ChargedSpazerUpRight] =
        [
            Show(F.ChargedSpazerUpRightStartupShort, 1),
            Show(F.ChargedSpazerUpRightAlternateStartupShort, 1),
            Show(F.ChargedSpazerUpRightStartupLong, 1),
            Show(F.ChargedSpazerUpRightAlternateStartupLong, 1),
            Show(F.ChargedSpazerUpRightSeed, 1),
            Show(F.ChargedSpazerUpRightAlternateSeed, 1),
            Show(F.ChargedSpazerUpRightParting, 1),
            Show(F.ChargedSpazerUpRightAlternateParting, 1),
        ],
        [P.ChargedSpazerUpRightLoop] =
        [
            Show(F.ChargedSpazerUpRightSpread, 1),
            Show(F.ChargedSpazerUpRightAlternateSpread, 1),
        ],
        [P.ChargedSpazerRight] =
        [
            Show(F.ChargedSpazerRightStartupShort, 1),
            Show(F.ChargedSpazerRightHorizontalAlternateStartupShort, 1),
            Show(F.ChargedSpazerRightStartupLong, 1),
            Show(F.ChargedSpazerRightHorizontalAlternateStartupLong, 1),
            Show(F.ChargedSpazerRightLeftSeed, 1),
            Show(F.ChargedSpazerRightHorizontalAlternateSeed, 1),
            Show(F.ChargedSpazerRightLeftParting, 1),
            Show(F.ChargedSpazerRightHorizontalAlternateParting, 1),
        ],
        [P.ChargedSpazerRightLoop] =
        [
            Show(F.ChargedSpazerRightLeftSpread, 1),
            Show(F.ChargedSpazerRightHorizontalAlternateSpread, 1),
        ],
        [P.ChargedSpazerDownRight] =
        [
            Show(F.ChargedSpazerDownRightStartupShort, 1),
            Show(F.ChargedSpazerDownRightAlternateStartupShort, 1),
            Show(F.ChargedSpazerDownRightStartupLong, 1),
            Show(F.ChargedSpazerDownRightAlternateStartupLong, 1),
            Show(F.ChargedSpazerDownRightSeed, 1),
            Show(F.ChargedSpazerDownRightAlternateSeed, 1),
            Show(F.ChargedSpazerDownRightParting, 1),
            Show(F.ChargedSpazerDownRightAlternateParting, 1),
        ],
        [P.ChargedSpazerDownRightLoop] =
        [
            Show(F.ChargedSpazerDownRightSpread, 1),
            Show(F.ChargedSpazerDownRightAlternateSpread, 1),
        ],
        [P.ChargedSpazerWaveUpFacingRight] =
        [
            Show(F.ChargedSpazerWaveUpFacingRightStartupShort, 1),
            Show(F.ChargedSpazerWaveUpFacingRightVerticalAlternateStartupShort, 1),
            Show(F.ChargedSpazerWaveUpFacingRightStartupLong, 1),
            Show(F.ChargedSpazerWaveUpFacingRightVerticalAlternateStartupLong, 1),
        ],
        [P.ChargedSpazerWaveUpFacingRightLoop] =
        [
            Show(F.ChargedSpazerWaveUpFacingRightSeed, 1),
            Show(F.ChargedSpazerWaveUpFacingRightVerticalAlternateSeed, 1),
            Show(F.ChargedSpazerWaveUpFacingRightSplitting, 1),
            Show(F.ChargedSpazerWaveUpFacingRightVerticalAlternateSplitting, 1),
            Show(F.ChargedSpazerWaveUpFacingRightParting, 1),
            Show(F.ChargedSpazerWaveUpFacingRightVerticalAlternateParting, 1),
            Show(F.ChargedSpazerWaveUpFacingRightSpreading, 1),
            Show(F.ChargedSpazerWaveUpFacingRightVerticalAlternateSpreading, 1),
            Show(F.ChargedSpazerWaveUpFacingRightNearlySpread, 1),
            Show(F.ChargedSpazerWaveUpFacingRightVerticalAlternateNearlySpread, 1),
            Show(F.ChargedSpazerWaveUpFacingRightSpread, 1),
            Show(F.ChargedSpazerWaveUpFacingRightVerticalAlternateSpread, 1),
            Show(F.ChargedSpazerWaveUpFacingRightNearlySpread, 1),
            Show(F.ChargedSpazerWaveUpFacingRightVerticalAlternateNearlySpread, 1),
            Show(F.ChargedSpazerWaveUpFacingRightSpreading, 1),
            Show(F.ChargedSpazerWaveUpFacingRightVerticalAlternateSpreading, 1),
            Show(F.ChargedSpazerWaveUpFacingRightParting, 1),
            Show(F.ChargedSpazerWaveUpFacingRightVerticalAlternateParting, 1),
            Show(F.ChargedSpazerWaveUpFacingRightSplittingNarrowHitboxTrailless, 1),
            Show(F.ChargedSpazerWaveUpFacingRightVerticalAlternateSplittingNarrowHitboxTrailless, 1),
        ],
        [P.ChargedSpazerWaveUpRight] =
        [
            Show(F.ChargedSpazerWaveUpRightStartupShort, 1),
            Show(F.ChargedSpazerWaveUpRightAlternateStartupShort, 1),
            Show(F.ChargedSpazerWaveUpRightStartupLong, 1),
            Show(F.ChargedSpazerWaveUpRightAlternateStartupLong, 1),
        ],
        [P.ChargedSpazerWaveUpRightLoop] =
        [
            Show(F.ChargedSpazerWaveUpRightSeed, 1),
            Show(F.ChargedSpazerWaveUpRightAlternateSeed, 1),
            Show(F.ChargedSpazerWaveUpRightSplitting, 1),
            Show(F.ChargedSpazerWaveUpRightAlternateSplitting, 1),
            Show(F.ChargedSpazerWaveUpRightParting, 1),
            Show(F.ChargedSpazerWaveUpRightAlternateParting, 1),
            Show(F.ChargedSpazerWaveUpRightSpreading, 1),
            Show(F.ChargedSpazerWaveUpRightAlternateSpreading, 1),
            Show(F.ChargedSpazerWaveUpRightNearlySpread, 1),
            Show(F.ChargedSpazerWaveUpRightAlternateNearlySpread, 1),
            Show(F.ChargedSpazerWaveUpRightSpread, 1),
            Show(F.ChargedSpazerWaveUpRightAlternateSpread, 1),
            Show(F.ChargedSpazerWaveUpRightNearlySpread, 1),
            Show(F.ChargedSpazerWaveUpRightAlternateNearlySpread, 1),
            Show(F.ChargedSpazerWaveUpRightSpreading, 1),
            Show(F.ChargedSpazerWaveUpRightAlternateSpreading, 1),
            Show(F.ChargedSpazerWaveUpRightParting, 1),
            Show(F.ChargedSpazerWaveUpRightAlternateParting, 1),
            Show(F.ChargedSpazerWaveUpRightSplittingTrailless, 1),
            Show(F.ChargedSpazerWaveUpRightAlternateSplittingTrailless, 1),
        ],
        [P.ChargedSpazerWaveRight] =
        [
            Show(F.ChargedSpazerWaveRightStartupShort, 1),
            Show(F.ChargedSpazerWaveRightHorizontalAlternateStartupShort, 1),
            Show(F.ChargedSpazerWaveRightStartupLong, 1),
            Show(F.ChargedSpazerWaveRightHorizontalAlternateStartupLong, 1),
        ],
        [P.ChargedSpazerWaveRightLoop] =
        [
            Show(F.ChargedSpazerWaveRightSeed, 1),
            Show(F.ChargedSpazerWaveRightHorizontalAlternateSeed, 1),
            Show(F.ChargedSpazerWaveRightSplitting, 1),
            Show(F.ChargedSpazerWaveRightHorizontalAlternateSplitting, 1),
            Show(F.ChargedSpazerWaveRightParting, 1),
            Show(F.ChargedSpazerWaveRightHorizontalAlternateParting, 1),
            Show(F.ChargedSpazerWaveRightSpreading, 1),
            Show(F.ChargedSpazerWaveRightHorizontalAlternateSpreading, 1),
            Show(F.ChargedSpazerWaveRightNearlySpread, 1),
            Show(F.ChargedSpazerWaveRightHorizontalAlternateNearlySpread, 1),
            Show(F.ChargedSpazerWaveRightSpread, 1),
            Show(F.ChargedSpazerWaveRightHorizontalAlternateSpread, 1),
            Show(F.ChargedSpazerWaveRightNearlySpread, 1),
            Show(F.ChargedSpazerWaveRightHorizontalAlternateNearlySpread, 1),
            Show(F.ChargedSpazerWaveRightSpreading, 1),
            Show(F.ChargedSpazerWaveRightHorizontalAlternateSpreading, 1),
            Show(F.ChargedSpazerWaveRightParting, 1),
            Show(F.ChargedSpazerWaveRightHorizontalAlternateParting, 1),
            Show(F.ChargedSpazerWaveRightSplittingNarrowHitboxTrailless, 1),
            Show(F.ChargedSpazerWaveRightHorizontalAlternateSplittingNarrowHitboxTrailless, 1),
        ],
        [P.ChargedSpazerWaveDownRight] =
        [
            Show(F.ChargedSpazerWaveDownRightStartupShort, 1),
            Show(F.ChargedSpazerWaveDownRightAlternateStartupShort, 1),
            Show(F.ChargedSpazerWaveDownRightStartupLong, 1),
            Show(F.ChargedSpazerWaveDownRightAlternateStartupLong, 1),
        ],
        [P.ChargedSpazerWaveDownRightLoop] =
        [
            Show(F.ChargedSpazerWaveDownRightSeed, 1),
            Show(F.ChargedSpazerWaveDownRightAlternateSeed, 1),
            Show(F.ChargedSpazerWaveDownRightSplitting, 1),
            Show(F.ChargedSpazerWaveDownRightAlternateSplitting, 1),
            Show(F.ChargedSpazerWaveDownRightParting, 1),
            Show(F.ChargedSpazerWaveDownRightAlternateParting, 1),
            Show(F.ChargedSpazerWaveDownRightSpreading, 1),
            Show(F.ChargedSpazerWaveDownRightAlternateSpreading, 1),
            Show(F.ChargedSpazerWaveDownRightNearlySpread, 1),
            Show(F.ChargedSpazerWaveDownRightAlternateNearlySpread, 1),
            Show(F.ChargedSpazerWaveDownRightSpread, 1),
            Show(F.ChargedSpazerWaveDownRightAlternateSpread, 1),
            Show(F.ChargedSpazerWaveDownRightNearlySpread, 1),
            Show(F.ChargedSpazerWaveDownRightAlternateNearlySpread, 1),
            Show(F.ChargedSpazerWaveDownRightSpreading, 1),
            Show(F.ChargedSpazerWaveDownRightAlternateSpreading, 1),
            Show(F.ChargedSpazerWaveDownRightParting, 1),
            Show(F.ChargedSpazerWaveDownRightAlternateParting, 1),
            Show(F.ChargedSpazerWaveDownRightSplittingTrailless, 1),
            Show(F.ChargedSpazerWaveDownRightAlternateSplittingTrailless, 1),
        ],
        [P.ChargedSpazerWaveDownFacingRight] =
        [
            Show(F.ChargedSpazerWaveDownFacingRightStartupShort, 1),
            Show(F.ChargedSpazerWaveDownFacingRightVerticalAlternateStartupShort, 1),
            Show(F.ChargedSpazerWaveDownFacingRightStartupLong, 1),
            Show(F.ChargedSpazerWaveDownFacingRightVerticalAlternateStartupLong, 1),
        ],
        [P.ChargedSpazerWaveDownFacingRightLoop] =
        [
            Show(F.ChargedSpazerWaveDownFacingRightSeed, 1),
            Show(F.ChargedSpazerWaveDownFacingRightVerticalAlternateSeed, 1),
            Show(F.ChargedSpazerWaveDownFacingRightSplitting, 1),
            Show(F.ChargedSpazerWaveDownFacingRightVerticalAlternateSplitting, 1),
            Show(F.ChargedSpazerWaveDownFacingRightParting, 1),
            Show(F.ChargedSpazerWaveDownFacingRightVerticalAlternateParting, 1),
            Show(F.ChargedSpazerWaveDownFacingRightSpreading, 1),
            Show(F.ChargedSpazerWaveDownFacingRightVerticalAlternateSpreading, 1),
            Show(F.ChargedSpazerWaveDownFacingRightNearlySpread, 1),
            Show(F.ChargedSpazerWaveDownFacingRightVerticalAlternateNearlySpread, 1),
            Show(F.ChargedSpazerWaveDownFacingRightSpread, 1),
            Show(F.ChargedSpazerWaveDownFacingRightVerticalAlternateSpread, 1),
            Show(F.ChargedSpazerWaveDownFacingRightNearlySpread, 1),
            Show(F.ChargedSpazerWaveDownFacingRightVerticalAlternateNearlySpread, 1),
            Show(F.ChargedSpazerWaveDownFacingRightSpreading, 1),
            Show(F.ChargedSpazerWaveDownFacingRightVerticalAlternateSpreading, 1),
            Show(F.ChargedSpazerWaveDownFacingRightParting, 1),
            Show(F.ChargedSpazerWaveDownFacingRightVerticalAlternateParting, 1),
            Show(F.ChargedSpazerWaveDownFacingRightSplittingNarrowHitboxTrailless, 1),
            Show(F.ChargedSpazerWaveDownFacingRightVerticalAlternateSplittingNarrowHitboxTrailless, 1),
        ],
        [P.ChargedSpazerWaveDownLeft] =
        [
            Show(F.ChargedSpazerWaveDownLeftUpRightStartupShort, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightAlternateStartupShort, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightStartupLong, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightAlternateStartupLong, 1),
        ],
        [P.ChargedSpazerWaveDownLeftLoop] =
        [
            Show(F.ChargedSpazerWaveDownLeftSeed, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightAlternateSeed, 1),
            Show(F.ChargedSpazerWaveDownLeftSplitting, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightAlternateSplitting, 1),
            Show(F.ChargedSpazerWaveDownLeftParting, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightAlternateParting, 1),
            Show(F.ChargedSpazerWaveDownLeftSpreading, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightAlternateSpreading, 1),
            Show(F.ChargedSpazerWaveDownLeftNearlySpread, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightAlternateNearlySpread, 1),
            Show(F.ChargedSpazerWaveDownLeftSpread, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightAlternateSpread, 1),
            Show(F.ChargedSpazerWaveDownLeftNearlySpread, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightAlternateNearlySpread, 1),
            Show(F.ChargedSpazerWaveDownLeftSpreading, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightAlternateSpreading, 1),
            Show(F.ChargedSpazerWaveDownLeftParting, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightAlternateParting, 1),
            Show(F.ChargedSpazerWaveDownLeftSplittingTrailless, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightAlternateSplittingTrailless, 1),
        ],
        [P.ChargedSpazerWaveLeft] =
        [
            Show(F.ChargedSpazerWaveLeftStartupShort, 1),
            Show(F.ChargedSpazerWaveLeftHorizontalAlternateStartupShort, 1),
            Show(F.ChargedSpazerWaveLeftStartupLong, 1),
            Show(F.ChargedSpazerWaveLeftHorizontalAlternateStartupLong, 1),
        ],
        [P.ChargedSpazerWaveLeftLoop] =
        [
            Show(F.ChargedSpazerWaveLeftSeed, 1),
            Show(F.ChargedSpazerWaveLeftHorizontalAlternateSeed, 1),
            Show(F.ChargedSpazerWaveLeftSplitting, 1),
            Show(F.ChargedSpazerWaveLeftHorizontalAlternateSplitting, 1),
            Show(F.ChargedSpazerWaveLeftParting, 1),
            Show(F.ChargedSpazerWaveLeftHorizontalAlternateParting, 1),
            Show(F.ChargedSpazerWaveLeftSpreading, 1),
            Show(F.ChargedSpazerWaveLeftHorizontalAlternateSpreading, 1),
            Show(F.ChargedSpazerWaveLeftNearlySpread, 1),
            Show(F.ChargedSpazerWaveLeftHorizontalAlternateNearlySpread, 1),
            Show(F.ChargedSpazerWaveLeftSpread, 1),
            Show(F.ChargedSpazerWaveLeftHorizontalAlternateSpread, 1),
            Show(F.ChargedSpazerWaveLeftNearlySpread, 1),
            Show(F.ChargedSpazerWaveLeftHorizontalAlternateNearlySpread, 1),
            Show(F.ChargedSpazerWaveLeftSpreading, 1),
            Show(F.ChargedSpazerWaveLeftHorizontalAlternateSpreading, 1),
            Show(F.ChargedSpazerWaveLeftParting, 1),
            Show(F.ChargedSpazerWaveLeftHorizontalAlternateParting, 1),
            Show(F.ChargedSpazerWaveLeftSplittingNarrowHitboxTrailless, 1),
            Show(F.ChargedSpazerWaveLeftHorizontalAlternateSplittingNarrowHitboxTrailless, 1),
        ],
        [P.ChargedSpazerWaveUpLeft] =
        [
            Show(F.ChargedSpazerWaveUpLeftStartupShort, 1),
            Show(F.ChargedSpazerWaveUpLeftDownRightAlternateStartupShort, 1),
            Show(F.ChargedSpazerWaveUpLeftStartupLong, 1),
            Show(F.ChargedSpazerWaveUpLeftDownRightAlternateStartupLong, 1),
        ],
        [P.ChargedSpazerWaveUpLeftLoop] =
        [
            Show(F.ChargedSpazerWaveUpLeftSeed, 1),
            Show(F.ChargedSpazerWaveUpLeftDownRightAlternateSeed, 1),
            Show(F.ChargedSpazerWaveUpLeftSplitting, 1),
            Show(F.ChargedSpazerWaveUpLeftDownRightAlternateSplitting, 1),
            Show(F.ChargedSpazerWaveUpLeftParting, 1),
            Show(F.ChargedSpazerWaveUpLeftDownRightAlternateParting, 1),
            Show(F.ChargedSpazerWaveUpLeftSpreading, 1),
            Show(F.ChargedSpazerWaveUpLeftDownRightAlternateSpreading, 1),
            Show(F.ChargedSpazerWaveUpLeftNearlySpread, 1),
            Show(F.ChargedSpazerWaveUpLeftDownRightAlternateNearlySpread, 1),
            Show(F.ChargedSpazerWaveUpLeftSpread, 1),
            Show(F.ChargedSpazerWaveUpLeftDownRightAlternateSpread, 1),
            Show(F.ChargedSpazerWaveUpLeftNearlySpread, 1),
            Show(F.ChargedSpazerWaveUpLeftDownRightAlternateNearlySpread, 1),
            Show(F.ChargedSpazerWaveUpLeftSpreading, 1),
            Show(F.ChargedSpazerWaveUpLeftDownRightAlternateSpreading, 1),
            Show(F.ChargedSpazerWaveUpLeftParting, 1),
            Show(F.ChargedSpazerWaveUpLeftDownRightAlternateParting, 1),
            Show(F.ChargedSpazerWaveUpLeftSplittingTrailless, 1),
            Show(F.ChargedSpazerWaveUpLeftDownRightAlternateSplittingTrailless, 1),
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
            Show(F.ChargedPlasmaWaveUpFacingRightAlternateLongCore, 1),
            Show(F.ChargedPlasmaWaveUpFacingRightLongLobesNear, 1),
            Show(F.ChargedPlasmaWaveUpFacingRightAlternateLobesNear, 1),
            Show(F.ChargedPlasmaWaveUpFacingRightLongLobesMidway, 1),
            Show(F.ChargedPlasmaWaveUpFacingRightAlternateLobesMidway, 1),
            Show(F.ChargedPlasmaWaveUpFacingRightLongLobesFar, 1),
            Show(F.ChargedPlasmaWaveUpFacingRightAlternateLobesFar, 1),
            Show(F.ChargedPlasmaWaveUpFacingRightLongLobesPeak, 1),
            Show(F.ChargedPlasmaWaveUpFacingRightAlternateLobesPeak, 1),
            Show(F.ChargedPlasmaWaveUpFacingRightLongLobesFar, 1),
            Show(F.ChargedPlasmaWaveUpFacingRightAlternateLobesFar, 1),
            Show(F.ChargedPlasmaWaveUpFacingRightLongLobesMidway, 1),
            Show(F.ChargedPlasmaWaveUpFacingRightAlternateLobesMidway, 1),
            Show(F.ChargedPlasmaWaveUpFacingRightLongLobesNear, 1),
            Show(F.ChargedPlasmaWaveUpFacingRightAlternateLobesNear, 1),
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
            Show(F.ChargedPlasmaWaveUpRightAlternateLongCore, 1),
            Show(F.ChargedPlasmaWaveUpRightLongLobesNear, 1),
            Show(F.ChargedPlasmaWaveUpRightAlternateLobesNear, 1),
            Show(F.ChargedPlasmaWaveUpRightLongLobesMidway, 1),
            Show(F.ChargedPlasmaWaveUpRightAlternateLobesMidway, 1),
            Show(F.ChargedPlasmaWaveUpRightLongLobesFar, 1),
            Show(F.ChargedPlasmaWaveUpRightAlternateLobesFar, 1),
            Show(F.ChargedPlasmaWaveUpRightLongLobesPeak, 1),
            Show(F.ChargedPlasmaWaveUpRightAlternateLobesPeak, 1),
            Show(F.ChargedPlasmaWaveUpRightLongLobesFar, 1),
            Show(F.ChargedPlasmaWaveUpRightAlternateLobesFar, 1),
            Show(F.ChargedPlasmaWaveUpRightLongLobesMidway, 1),
            Show(F.ChargedPlasmaWaveUpRightAlternateLobesMidway, 1),
            Show(F.ChargedPlasmaWaveUpRightLongLobesNear, 1),
            Show(F.ChargedPlasmaWaveUpRightAlternateLobesNear, 1),
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
            Show(F.ChargedPlasmaWaveRightAlternateLongCore, 1),
            Show(F.ChargedPlasmaWaveRightLongLobesNear, 1),
            Show(F.ChargedPlasmaWaveRightAlternateLobesNear, 1),
            Show(F.ChargedPlasmaWaveRightLongLobesMidway, 1),
            Show(F.ChargedPlasmaWaveRightAlternateLobesMidway, 1),
            Show(F.ChargedPlasmaWaveRightLongLobesFar, 1),
            Show(F.ChargedPlasmaWaveRightAlternateLobesFar, 1),
            Show(F.ChargedPlasmaWaveRightLongLobesPeak, 1),
            Show(F.ChargedPlasmaWaveRightAlternateLobesPeak, 1),
            Show(F.ChargedPlasmaWaveRightLongLobesFar, 1),
            Show(F.ChargedPlasmaWaveRightAlternateLobesFar, 1),
            Show(F.ChargedPlasmaWaveRightLongLobesMidway, 1),
            Show(F.ChargedPlasmaWaveRightAlternateLobesMidway, 1),
            Show(F.ChargedPlasmaWaveRightLongLobesNear, 1),
            Show(F.ChargedPlasmaWaveRightAlternateLobesNear, 1),
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
            Show(F.ChargedPlasmaWaveDownRightAlternateLongCore, 1),
            Show(F.ChargedPlasmaWaveDownRightLongLobesNear, 1),
            Show(F.ChargedPlasmaWaveDownRightAlternateLobesNear, 1),
            Show(F.ChargedPlasmaWaveDownRightLongLobesMidway, 1),
            Show(F.ChargedPlasmaWaveDownRightAlternateLobesMidway, 1),
            Show(F.ChargedPlasmaWaveDownRightLongLobesFar, 1),
            Show(F.ChargedPlasmaWaveDownRightAlternateLobesFar, 1),
            Show(F.ChargedPlasmaWaveDownRightLongLobesPeak, 1),
            Show(F.ChargedPlasmaWaveDownRightAlternateLobesPeak, 1),
            Show(F.ChargedPlasmaWaveDownRightLongLobesFar, 1),
            Show(F.ChargedPlasmaWaveDownRightAlternateLobesFar, 1),
            Show(F.ChargedPlasmaWaveDownRightLongLobesMidway, 1),
            Show(F.ChargedPlasmaWaveDownRightAlternateLobesMidway, 1),
            Show(F.ChargedPlasmaWaveDownRightLongLobesNear, 1),
            Show(F.ChargedPlasmaWaveDownRightAlternateLobesNear, 1),
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
            Show(F.PowerBombBright, 5),
            Show(F.PowerBombDimming, 5),
            Show(F.PowerBombDark, 5),
        ],
        [P.FastPowerBomb] =
        [
            Show(F.FastPowerBombBright, 1),
            Show(F.FastPowerBombDimming, 1),
            Show(F.FastPowerBombDark, 1),
        ],
        [P.Bomb] =
        [
            Show(F.BombDark, 5),
            Show(F.BombSmouldering, 5),
            Show(F.BombGlowing, 5),
            Show(F.BombFlashing, 5),
        ],
        [P.FastBomb] =
        [
            Show(F.FastBombDark, 1),
            Show(F.FastBombSmouldering, 1),
            Show(F.FastBombGlowing, 1),
            Show(F.FastBombFlashing, 1),
        ],
        [P.BeamExplosion] =
        [
            Show(F.BeamExplosionSpeck, 3),
            Show(F.BeamExplosionGlint, 3),
            Show(F.BeamExplosionFlash, 3),
            Show(F.BeamExplosionRingedFlash, 3),
            Show(F.BeamExplosionRing, 3),
            Show(F.BeamExplosionFadingRing, 3),
        ],
        [P.MissileExplosion] =
        [
            Show(F.MissileExplosionSpark, 3),
            Show(F.MissileExplosionBall, 3),
            Show(F.MissileExplosionBlast, 3),
            Show(F.MissileExplosionFlare, 3),
            Show(F.MissileExplosionBurst, 3),
            Show(F.MissileExplosionEmbers, 3),
        ],
        [P.BombExplosion] =
        [
            Show(F.BombExplosionOrb, 2),
            Show(F.BombExplosionSparklingOrb, 2),
            Show(F.BombExplosionBurst, 2),
            Show(F.BombExplosionWideBurst, 2),
            Show(F.BombExplosionRing, 2),
        ],
        [P.PlasmaSpecialAttack] =
        [
            Show(F.PlasmaSpecialAttackOrb, 2),
            Show(F.PlasmaSpecialAttackSparklingOrb, 2),
            Show(F.PlasmaSpecialAttackBurst, 2),
            Show(F.PlasmaSpecialAttackWideBurst, 2),
            Show(F.PlasmaSpecialAttackRing, 2),
        ],
        [P.SuperMissileExplosion] =
        [
            Show(F.SuperMissileExplosionBall, 5),
            Show(F.SuperMissileExplosionBlast, 5),
            Show(F.SuperMissileExplosionFlare, 5),
            Show(F.SuperMissileExplosionRingedBall, 5),
            Show(F.SuperMissileExplosionRing, 5),
            Show(F.SuperMissileExplosionFadingRing, 5),
        ],
        [P.ShinesparkEchoUpFacingRight] =
        [
            Show(F.ShinesparkEchoUpFacingRightHidden, 2),
            Show(F.ShinesparkEchoUpFacingRightHidden, 2),
            Show(F.ShinesparkEchoUpFacingRightHidden, 2),
            Show(F.ShinesparkEchoUpFacingRightHidden, 2),
        ],
        [P.SpazerSpecialAttackTrailUpFacingRight] =
        [
            Show(F.SpazerSpecialAttackTrailUpFacingRightHidden, 2),
            Show(F.SpazerSpecialAttackTrailUpFacingRightHiddenWide, 2),
        ],
        [P.SpazerSpecialAttackTrailUpFacingRightLoop] =
        [
            Show(F.SpazerSpecialAttackTrailUpFacingRightHiddenWidest, 2),
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
