using SuperMetroid.Core.Animation;
using F = SuperMetroid.Core.Samus.Projectiles.SamusProjectileFrameId;
using P = SuperMetroid.Core.Samus.Projectiles.SamusProjectileSegment;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The Samus projectiles' animations (#1301, #2347): each segment shows its frames for their durations or holds a still
/// picture, and each animation plays whole segments, repeating its last or ending.
/// </summary>
internal static class SamusProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<SamusProjectileSegment, SamusProjectileFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<SamusProjectileSegment, AnimationBeat<SamusProjectileFrameId, NoAnimationCue>[]>
    {
        [P.PowerUp] =
        [
            Hold(F.PowerUp),
        ],
        [P.PowerUpRight] =
        [
            Hold(F.PowerUpRight),
        ],
        [P.PowerRight] =
        [
            Hold(F.PowerRight),
        ],
        [P.PowerDownRight] =
        [
            Hold(F.PowerDownRight),
        ],
        [P.PowerDown] =
        [
            Hold(F.PowerDown),
        ],
        [P.PowerDownLeft] =
        [
            Hold(F.PowerDownLeft),
        ],
        [P.PowerLeft] =
        [
            Hold(F.PowerLeft),
        ],
        [P.PowerUpLeft] =
        [
            Hold(F.PowerUpLeft),
        ],
        [P.WaveUp] =
        [
            Show(F.WaveUpHidden, 4),
        ],
        [P.WaveVertical] =
        [
            Show(F.WaveVerticalCentre, 1),
            Show(F.WaveVerticalRightNear, 1),
            Show(F.WaveVerticalRightMidway, 1),
            Show(F.WaveVerticalRightFar, 1),
            Show(F.WaveVerticalRightPeak, 1),
            Show(F.WaveVerticalRightFar, 1),
            Show(F.WaveVerticalRightMidway, 1),
            Show(F.WaveVerticalRightNear, 1),
            Show(F.WaveVerticalCentre, 1),
            Show(F.WaveVerticalLeftNear, 1),
            Show(F.WaveVerticalLeftMidway, 1),
            Show(F.WaveVerticalLeftFar, 1),
            Show(F.WaveVerticalLeftPeak, 1),
            Show(F.WaveVerticalLeftFar, 1),
            Show(F.WaveVerticalLeftMidway, 1),
            Show(F.WaveVerticalLeftNear, 1),
        ],
        [P.WaveRisingDiagonal] =
        [
            Show(F.WaveRisingDiagonalCentre, 1),
            Show(F.WaveRisingDiagonalUpLeftNear, 1),
            Show(F.WaveRisingDiagonalUpLeftMidway, 1),
            Show(F.WaveRisingDiagonalUpLeftFar, 1),
            Show(F.WaveRisingDiagonalUpLeftPeak, 1),
            Show(F.WaveRisingDiagonalUpLeftFar, 1),
            Show(F.WaveRisingDiagonalUpLeftMidway, 1),
            Show(F.WaveRisingDiagonalUpLeftNearNarrowHitbox, 1),
            Show(F.WaveRisingDiagonalCentreNarrowHitbox, 1),
            Show(F.WaveRisingDiagonalDownRightNear, 1),
            Show(F.WaveRisingDiagonalDownRightMidway, 1),
            Show(F.WaveRisingDiagonalDownRightFar, 1),
            Show(F.WaveRisingDiagonalDownRightPeak, 1),
            Show(F.WaveRisingDiagonalDownRightFar, 1),
            Show(F.WaveRisingDiagonalDownRightMidway, 1),
            Show(F.WaveRisingDiagonalDownRightNearWideHitbox, 1),
        ],
        [P.WaveHorizontal] =
        [
            Show(F.WaveHorizontalCentre, 1),
            Show(F.WaveHorizontalUpNear, 1),
            Show(F.WaveHorizontalUpMidway, 1),
            Show(F.WaveHorizontalUpFar, 1),
            Show(F.WaveHorizontalUpPeak, 1),
            Show(F.WaveHorizontalUpFar, 1),
            Show(F.WaveHorizontalUpMidway, 1),
            Show(F.WaveHorizontalUpNear, 1),
            Show(F.WaveHorizontalCentre, 1),
            Show(F.WaveHorizontalDownNear, 1),
            Show(F.WaveHorizontalDownMidway, 1),
            Show(F.WaveHorizontalDownFar, 1),
            Show(F.WaveHorizontalDownPeak, 1),
            Show(F.WaveHorizontalDownFar, 1),
            Show(F.WaveHorizontalDownMidway, 1),
            Show(F.WaveHorizontalDownNear, 1),
        ],
        [P.WaveFallingDiagonal] =
        [
            Show(F.WaveFallingDiagonalCentre, 1),
            Show(F.WaveFallingDiagonalUpRightNear, 1),
            Show(F.WaveFallingDiagonalUpRightMidway, 1),
            Show(F.WaveFallingDiagonalUpRightFar, 1),
            Show(F.WaveFallingDiagonalUpRightPeak, 1),
            Show(F.WaveFallingDiagonalUpRightFar, 1),
            Show(F.WaveFallingDiagonalUpRightMidway, 1),
            Show(F.WaveFallingDiagonalUpRightNearNarrowHitbox, 1),
            Show(F.WaveFallingDiagonalCentreNarrowHitbox, 1),
            Show(F.WaveFallingDiagonalDownLeftNear, 1),
            Show(F.WaveFallingDiagonalDownLeftMidway, 1),
            Show(F.WaveFallingDiagonalDownLeftFar, 1),
            Show(F.WaveFallingDiagonalDownLeftPeak, 1),
            Show(F.WaveFallingDiagonalDownLeftFar, 1),
            Show(F.WaveFallingDiagonalDownLeftMidway, 1),
            Show(F.WaveFallingDiagonalDownLeftNearWideHitbox, 1),
        ],
        [P.Ice] =
        [
            Show(F.IceStar, 1),
            Show(F.IceCrossed, 1),
            Show(F.IceNarrowStar, 1),
            Show(F.IceCrossedMirrored, 1),
        ],
        [P.SpazerUp] =
        [
            Show(F.SpazerUpSeed, 2),
            Show(F.SpazerUpParting, 2),
        ],
        [P.SpazerUpSpread] =
        [
            Hold(F.SpazerUpSpread),
        ],
        [P.SpazerUpRight] =
        [
            Show(F.SpazerUpRightSeed, 2),
            Show(F.SpazerUpRightParting, 2),
        ],
        [P.SpazerUpRightSpread] =
        [
            Hold(F.SpazerUpRightSpread),
        ],
        [P.SpazerRight] =
        [
            Show(F.SpazerRightSeed, 2),
            Show(F.SpazerRightParting, 2),
        ],
        [P.SpazerRightSpread] =
        [
            Hold(F.SpazerRightSpread),
        ],
        [P.SpazerDownRight] =
        [
            Show(F.SpazerDownRightSeed, 2),
            Show(F.SpazerDownRightParting, 2),
        ],
        [P.SpazerDownRightSpread] =
        [
            Hold(F.SpazerDownRightSpread),
        ],
        [P.SpazerDown] =
        [
            Show(F.SpazerDownSeed, 2),
            Show(F.SpazerDownParting, 2),
        ],
        [P.SpazerDownSpread] =
        [
            Hold(F.SpazerDownSpread),
        ],
        [P.SpazerDownLeft] =
        [
            Show(F.SpazerDownLeftSeed, 2),
            Show(F.SpazerDownLeftParting, 2),
        ],
        [P.SpazerDownLeftSpread] =
        [
            Hold(F.SpazerDownLeftSpread),
        ],
        [P.SpazerLeft] =
        [
            Show(F.SpazerLeftSeed, 2),
            Show(F.SpazerLeftParting, 2),
        ],
        [P.SpazerLeftSpread] =
        [
            Hold(F.SpazerLeftSpread),
        ],
        [P.SpazerUpLeft] =
        [
            Show(F.SpazerUpLeftSeed, 2),
            Show(F.SpazerUpLeftParting, 2),
        ],
        [P.SpazerUpLeftSpread] =
        [
            Hold(F.SpazerUpLeftSpread),
        ],
        [P.SpazerWaveUp] =
        [
            Show(F.SpazerWaveUpSeed, 2),
            Show(F.SpazerWaveUpSplitting, 2),
            Show(F.SpazerWaveUpParting, 2),
            Show(F.SpazerWaveUpSpreading, 2),
            Show(F.SpazerWaveUpNearlySpread, 2),
            Show(F.SpazerWaveUpSpread, 2),
            Show(F.SpazerWaveUpNearlySpread, 2),
            Show(F.SpazerWaveUpSpreading, 2),
            Show(F.SpazerWaveUpPartingWithoutDefaultTrail, 2),
            Show(F.SpazerWaveUpSplittingWithoutDefaultTrail, 2),
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
        [P.SpazerWaveDown] =
        [
            Show(F.SpazerWaveDownSeed, 2),
            Show(F.SpazerWaveDownSplitting, 2),
            Show(F.SpazerWaveDownParting, 2),
            Show(F.SpazerWaveDownSpreading, 2),
            Show(F.SpazerWaveDownNearlySpread, 2),
            Show(F.SpazerWaveDownSpread, 2),
            Show(F.SpazerWaveDownNearlySpread, 2),
            Show(F.SpazerWaveDownSpreading, 2),
            Show(F.SpazerWaveDownPartingWithoutDefaultTrail, 2),
            Show(F.SpazerWaveDownSplittingWithoutDefaultTrail, 2),
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
        [P.PlasmaVertical] =
        [
            Show(F.PlasmaVerticalCore, 1),
        ],
        [P.PlasmaVerticalFullLength] =
        [
            Hold(F.PlasmaVerticalFullLength),
        ],
        [P.PlasmaRisingDiagonal] =
        [
            Show(F.PlasmaRisingDiagonalCore, 1),
        ],
        [P.PlasmaRisingDiagonalFullLength] =
        [
            Hold(F.PlasmaRisingDiagonalFullLength),
        ],
        [P.PlasmaHorizontal] =
        [
            Show(F.PlasmaHorizontalCore, 1),
        ],
        [P.PlasmaHorizontalFullLength] =
        [
            Hold(F.PlasmaHorizontalFullLength),
        ],
        [P.PlasmaFallingDiagonal] =
        [
            Show(F.PlasmaFallingDiagonalCore, 1),
        ],
        [P.PlasmaFallingDiagonalFullLength] =
        [
            Hold(F.PlasmaFallingDiagonalFullLength),
        ],
        [P.PlasmaIceWaveVertical] =
        [
            Show(F.PlasmaIceWaveVerticalCore, 1),
        ],
        [P.PlasmaWaveVertical] =
        [
            Show(F.PlasmaWaveVerticalShortCore, 2),
            Show(F.PlasmaWaveVerticalShortLobesNear, 2),
            Show(F.PlasmaWaveVerticalShortLobesMidway, 2),
            Show(F.PlasmaWaveVerticalShortLobesFar, 2),
            Show(F.PlasmaWaveVerticalShortLobesPeak, 2),
            Show(F.PlasmaWaveVerticalShortLobesFar, 2),
            Show(F.PlasmaWaveVerticalShortLobesMidway, 2),
            Show(F.PlasmaWaveVerticalShortLobesNearWithoutDefaultTrail, 2),
        ],
        [P.PlasmaIceWaveRisingDiagonal] =
        [
            Show(F.PlasmaIceWaveRisingDiagonalCore, 1),
        ],
        [P.PlasmaWaveRisingDiagonal] =
        [
            Show(F.PlasmaWaveRisingDiagonalShortCore, 2),
            Show(F.PlasmaWaveRisingDiagonalShortLobesNear, 2),
            Show(F.PlasmaWaveRisingDiagonalShortLobesMidway, 2),
            Show(F.PlasmaWaveRisingDiagonalShortLobesFar, 2),
            Show(F.PlasmaWaveRisingDiagonalShortLobesPeak, 2),
            Show(F.PlasmaWaveRisingDiagonalShortLobesFar, 2),
            Show(F.PlasmaWaveRisingDiagonalShortLobesMidway, 2),
            Show(F.PlasmaWaveRisingDiagonalShortLobesNearWithoutDefaultTrail, 2),
        ],
        [P.PlasmaWaveHorizontal] =
        [
            Show(F.PlasmaWaveHorizontalCore, 1),
        ],
        [P.PlasmaWaveHorizontalSwing] =
        [
            Show(F.PlasmaWaveHorizontalShortCore, 2),
            Show(F.PlasmaWaveHorizontalShortLobesNear, 2),
            Show(F.PlasmaWaveHorizontalShortLobesMidway, 2),
            Show(F.PlasmaWaveHorizontalShortLobesFar, 2),
            Show(F.PlasmaWaveHorizontalShortLobesPeak, 2),
            Show(F.PlasmaWaveHorizontalShortLobesFar, 2),
            Show(F.PlasmaWaveHorizontalShortLobesMidway, 2),
            Show(F.PlasmaWaveHorizontalShortLobesNearWithoutDefaultTrail, 2),
        ],
        [P.PlasmaIceWaveFallingDiagonal] =
        [
            Show(F.PlasmaIceWaveFallingDiagonalCore, 1),
        ],
        [P.PlasmaWaveFallingDiagonal] =
        [
            Show(F.PlasmaWaveFallingDiagonalShortCore, 2),
            Show(F.PlasmaWaveFallingDiagonalShortLobesNear, 2),
            Show(F.PlasmaWaveFallingDiagonalShortLobesMidway, 2),
            Show(F.PlasmaWaveFallingDiagonalShortLobesFar, 2),
            Show(F.PlasmaWaveFallingDiagonalShortLobesPeak, 2),
            Show(F.PlasmaWaveFallingDiagonalShortLobesFar, 2),
            Show(F.PlasmaWaveFallingDiagonalShortLobesMidway, 2),
            Show(F.PlasmaWaveFallingDiagonalShortLobesNearWithoutDefaultTrail, 2),
        ],
        [P.ChargedPowerUp] =
        [
            Show(F.ChargedPowerUp, 1),
            Show(F.ChargedPowerUpAlternate, 1),
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
        [P.ChargedPowerDown] =
        [
            Show(F.ChargedPowerDown, 1),
            Show(F.ChargedPowerDownAlternate, 1),
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
        [P.ChargedWaveUp] =
        [
            Show(F.ChargedWaveUpHidden, 3),
        ],
        [P.ChargedWaveVertical] =
        [
            Show(F.ChargedWaveVerticalCore, 1),
            Show(F.ChargedWaveVerticalCoreAlternate, 1),
            Show(F.ChargedWaveVerticalLobesNear, 1),
            Show(F.ChargedWaveVerticalLobesNearAlternate, 1),
            Show(F.ChargedWaveVerticalLobesMidway, 1),
            Show(F.ChargedWaveVerticalLobesMidwayAlternate, 1),
            Show(F.ChargedWaveVerticalLobesFar, 1),
            Show(F.ChargedWaveVerticalLobesFarAlternate, 1),
            Show(F.ChargedWaveVerticalLobesPeak, 1),
            Show(F.ChargedWaveVerticalLobesPeakAlternate, 1),
            Show(F.ChargedWaveVerticalLobesFar, 1),
            Show(F.ChargedWaveVerticalLobesFarAlternate, 1),
            Show(F.ChargedWaveVerticalLobesMidway, 1),
            Show(F.ChargedWaveVerticalLobesMidwayAlternate, 1),
            Show(F.ChargedWaveVerticalLobesNearAlternate, 1),
            Show(F.ChargedWaveVerticalLobesNear, 1),
        ],
        [P.ChargedWaveRisingDiagonal] =
        [
            Show(F.ChargedWaveRisingDiagonalCore, 1),
            Show(F.ChargedWaveRisingDiagonalCoreAlternate, 1),
            Show(F.ChargedWaveRisingDiagonalLobesNear, 1),
            Show(F.ChargedWaveRisingDiagonalLobesNearAlternate, 1),
            Show(F.ChargedWaveRisingDiagonalLobesMidway, 1),
            Show(F.ChargedWaveRisingDiagonalLobesMidwayAlternate, 1),
            Show(F.ChargedWaveRisingDiagonalLobesFar, 1),
            Show(F.ChargedWaveRisingDiagonalLobesFarAlternate, 1),
            Show(F.ChargedWaveRisingDiagonalLobesPeak, 1),
            Show(F.ChargedWaveRisingDiagonalLobesPeakAlternate, 1),
            Show(F.ChargedWaveRisingDiagonalLobesFar, 1),
            Show(F.ChargedWaveRisingDiagonalLobesFarAlternate, 1),
            Show(F.ChargedWaveRisingDiagonalLobesMidway, 1),
            Show(F.ChargedWaveRisingDiagonalLobesMidwayAlternate, 1),
            Show(F.ChargedWaveRisingDiagonalLobesNear, 1),
            Show(F.ChargedWaveRisingDiagonalLobesNearAlternate, 1),
        ],
        [P.ChargedWaveHorizontal] =
        [
            Show(F.ChargedWaveHorizontalCore, 1),
            Show(F.ChargedWaveHorizontalCoreAlternate, 1),
            Show(F.ChargedWaveHorizontalLobesNear, 1),
            Show(F.ChargedWaveHorizontalLobesNearAlternate, 1),
            Show(F.ChargedWaveHorizontalLobesMidway, 1),
            Show(F.ChargedWaveHorizontalLobesMidwayAlternate, 1),
            Show(F.ChargedWaveHorizontalLobesFar, 1),
            Show(F.ChargedWaveHorizontalLobesFarAlternate, 1),
            Show(F.ChargedWaveHorizontalLobesPeak, 1),
            Show(F.ChargedWaveHorizontalLobesPeakAlternate, 1),
            Show(F.ChargedWaveHorizontalLobesFar, 1),
            Show(F.ChargedWaveHorizontalLobesFarAlternate, 1),
            Show(F.ChargedWaveHorizontalLobesMidway, 1),
            Show(F.ChargedWaveHorizontalLobesMidwayAlternate, 1),
            Show(F.ChargedWaveHorizontalLobesNearAlternate, 1),
            Show(F.ChargedWaveHorizontalLobesNear, 1),
        ],
        [P.ChargedWaveFallingDiagonal] =
        [
            Show(F.ChargedWaveFallingDiagonalCore, 1),
            Show(F.ChargedWaveFallingDiagonalCoreAlternate, 1),
            Show(F.ChargedWaveFallingDiagonalLobesNear, 1),
            Show(F.ChargedWaveFallingDiagonalLobesNearAlternate, 1),
            Show(F.ChargedWaveFallingDiagonalLobesMidway, 1),
            Show(F.ChargedWaveFallingDiagonalLobesMidwayAlternate, 1),
            Show(F.ChargedWaveFallingDiagonalLobesFar, 1),
            Show(F.ChargedWaveFallingDiagonalLobesFarAlternate, 1),
            Show(F.ChargedWaveFallingDiagonalLobesPeak, 1),
            Show(F.ChargedWaveFallingDiagonalLobesPeakAlternate, 1),
            Show(F.ChargedWaveFallingDiagonalLobesFar, 1),
            Show(F.ChargedWaveFallingDiagonalLobesFarAlternate, 1),
            Show(F.ChargedWaveFallingDiagonalLobesMidway, 1),
            Show(F.ChargedWaveFallingDiagonalLobesMidwayAlternate, 1),
            Show(F.ChargedWaveFallingDiagonalLobesNear, 1),
            Show(F.ChargedWaveFallingDiagonalLobesNearAlternate, 1),
        ],
        [P.ChargedIce] =
        [
            Show(F.ChargedIceCrossed, 1),
            Show(F.ChargedIceStar, 1),
            Show(F.ChargedIceCrossed, 1),
            Show(F.ChargedIceStar, 1),
        ],
        [P.ChargedIceWaveUp] =
        [
            Show(F.ChargedIceWaveUpHidden, 3),
        ],
        [P.ChargedIceWaveVertical] =
        [
            Show(F.ChargedIceWaveVerticalCore, 1),
            Show(F.ChargedIceWaveVerticalCoreAlternate, 1),
            Show(F.ChargedIceWaveVerticalLobesNear, 1),
            Show(F.ChargedIceWaveVerticalLobesNearAlternate, 1),
            Show(F.ChargedIceWaveVerticalLobesMidway, 1),
            Show(F.ChargedIceWaveVerticalLobesMidwayAlternate, 1),
            Show(F.ChargedIceWaveVerticalLobesFar, 1),
            Show(F.ChargedIceWaveVerticalLobesFarAlternate, 1),
            Show(F.ChargedIceWaveVerticalLobesPeak, 1),
            Show(F.ChargedIceWaveVerticalLobesPeakAlternate, 1),
            Show(F.ChargedIceWaveVerticalLobesFar, 1),
            Show(F.ChargedIceWaveVerticalLobesFarAlternate, 1),
            Show(F.ChargedIceWaveVerticalLobesMidway, 1),
            Show(F.ChargedIceWaveVerticalLobesMidwayAlternate, 1),
            Show(F.ChargedIceWaveVerticalLobesNearAlternate, 1),
            Show(F.ChargedIceWaveVerticalLobesNear, 1),
        ],
        [P.ChargedIceWaveRisingDiagonal] =
        [
            Show(F.ChargedIceWaveRisingDiagonalCore, 1),
            Show(F.ChargedIceWaveRisingDiagonalCoreAlternate, 1),
            Show(F.ChargedIceWaveRisingDiagonalLobesNear, 1),
            Show(F.ChargedIceWaveRisingDiagonalLobesNearAlternate, 1),
            Show(F.ChargedIceWaveRisingDiagonalLobesMidway, 1),
            Show(F.ChargedIceWaveRisingDiagonalLobesMidwayAlternate, 1),
            Show(F.ChargedIceWaveRisingDiagonalLobesFar, 1),
            Show(F.ChargedIceWaveRisingDiagonalLobesFarAlternate, 1),
            Show(F.ChargedIceWaveRisingDiagonalLobesPeak, 1),
            Show(F.ChargedIceWaveRisingDiagonalLobesPeakAlternate, 1),
            Show(F.ChargedIceWaveRisingDiagonalLobesFar, 1),
            Show(F.ChargedIceWaveRisingDiagonalLobesFarAlternate, 1),
            Show(F.ChargedIceWaveRisingDiagonalLobesMidway, 1),
            Show(F.ChargedIceWaveRisingDiagonalLobesMidwayAlternate, 1),
            Show(F.ChargedIceWaveRisingDiagonalLobesNear, 1),
            Show(F.ChargedIceWaveRisingDiagonalLobesNearAlternate, 1),
        ],
        [P.ChargedIceWaveHorizontal] =
        [
            Show(F.ChargedIceWaveHorizontalCore, 1),
            Show(F.ChargedIceWaveHorizontalCoreAlternate, 1),
            Show(F.ChargedIceWaveHorizontalLobesNear, 1),
            Show(F.ChargedIceWaveHorizontalLobesNearAlternate, 1),
            Show(F.ChargedIceWaveHorizontalLobesMidway, 1),
            Show(F.ChargedIceWaveHorizontalLobesMidwayAlternate, 1),
            Show(F.ChargedIceWaveHorizontalLobesFar, 1),
            Show(F.ChargedIceWaveHorizontalLobesFarAlternate, 1),
            Show(F.ChargedIceWaveHorizontalLobesPeak, 1),
            Show(F.ChargedIceWaveHorizontalLobesPeakAlternate, 1),
            Show(F.ChargedIceWaveHorizontalLobesFar, 1),
            Show(F.ChargedIceWaveHorizontalLobesFarAlternate, 1),
            Show(F.ChargedIceWaveHorizontalLobesMidway, 1),
            Show(F.ChargedIceWaveHorizontalLobesMidwayAlternate, 1),
            Show(F.ChargedIceWaveHorizontalLobesNearAlternate, 1),
            Show(F.ChargedIceWaveHorizontalLobesNear, 1),
        ],
        [P.ChargedIceWaveFallingDiagonal] =
        [
            Show(F.ChargedIceWaveFallingDiagonalCore, 1),
            Show(F.ChargedIceWaveFallingDiagonalCoreAlternate, 1),
            Show(F.ChargedIceWaveFallingDiagonalLobesNear, 1),
            Show(F.ChargedIceWaveFallingDiagonalLobesNearAlternate, 1),
            Show(F.ChargedIceWaveFallingDiagonalLobesMidway, 1),
            Show(F.ChargedIceWaveFallingDiagonalLobesMidwayAlternate, 1),
            Show(F.ChargedIceWaveFallingDiagonalLobesFar, 1),
            Show(F.ChargedIceWaveFallingDiagonalLobesFarAlternate, 1),
            Show(F.ChargedIceWaveFallingDiagonalLobesPeak, 1),
            Show(F.ChargedIceWaveFallingDiagonalLobesPeakAlternate, 1),
            Show(F.ChargedIceWaveFallingDiagonalLobesFar, 1),
            Show(F.ChargedIceWaveFallingDiagonalLobesFarAlternate, 1),
            Show(F.ChargedIceWaveFallingDiagonalLobesMidway, 1),
            Show(F.ChargedIceWaveFallingDiagonalLobesMidwayAlternate, 1),
            Show(F.ChargedIceWaveFallingDiagonalLobesNear, 1),
            Show(F.ChargedIceWaveFallingDiagonalLobesNearAlternate, 1),
        ],
        [P.ChargedSpazerVertical] =
        [
            Show(F.ChargedSpazerVerticalStartupShort, 1),
            Show(F.ChargedSpazerVerticalAlternateStartupShort, 1),
            Show(F.ChargedSpazerVerticalStartupLong, 1),
            Show(F.ChargedSpazerVerticalAlternateStartupLong, 1),
            Show(F.ChargedSpazerVerticalSeed, 1),
            Show(F.ChargedSpazerVerticalAlternateSeed, 1),
            Show(F.ChargedSpazerVerticalParting, 1),
            Show(F.ChargedSpazerVerticalAlternateParting, 1),
        ],
        [P.ChargedSpazerVerticalSpread] =
        [
            Show(F.ChargedSpazerVerticalSpread, 1),
            Show(F.ChargedSpazerVerticalAlternateSpread, 1),
        ],
        [P.ChargedSpazerRisingDiagonal] =
        [
            Show(F.ChargedSpazerRisingDiagonalStartupShort, 1),
            Show(F.ChargedSpazerRisingDiagonalAlternateStartupShort, 1),
            Show(F.ChargedSpazerRisingDiagonalStartupLong, 1),
            Show(F.ChargedSpazerRisingDiagonalAlternateStartupLong, 1),
            Show(F.ChargedSpazerRisingDiagonalSeed, 1),
            Show(F.ChargedSpazerRisingDiagonalAlternateSeed, 1),
            Show(F.ChargedSpazerRisingDiagonalParting, 1),
            Show(F.ChargedSpazerRisingDiagonalAlternateParting, 1),
        ],
        [P.ChargedSpazerRisingDiagonalSpread] =
        [
            Show(F.ChargedSpazerRisingDiagonalSpread, 1),
            Show(F.ChargedSpazerRisingDiagonalAlternateSpread, 1),
        ],
        [P.ChargedSpazerHorizontal] =
        [
            Show(F.ChargedSpazerHorizontalStartupShort, 1),
            Show(F.ChargedSpazerHorizontalAlternateStartupShort, 1),
            Show(F.ChargedSpazerHorizontalStartupLong, 1),
            Show(F.ChargedSpazerHorizontalAlternateStartupLong, 1),
            Show(F.ChargedSpazerHorizontalSeed, 1),
            Show(F.ChargedSpazerHorizontalAlternateSeed, 1),
            Show(F.ChargedSpazerHorizontalParting, 1),
            Show(F.ChargedSpazerHorizontalAlternateParting, 1),
        ],
        [P.ChargedSpazerHorizontalSpread] =
        [
            Show(F.ChargedSpazerHorizontalSpread, 1),
            Show(F.ChargedSpazerHorizontalAlternateSpread, 1),
        ],
        [P.ChargedSpazerFallingDiagonal] =
        [
            Show(F.ChargedSpazerFallingDiagonalStartupShort, 1),
            Show(F.ChargedSpazerFallingDiagonalAlternateStartupShort, 1),
            Show(F.ChargedSpazerFallingDiagonalStartupLong, 1),
            Show(F.ChargedSpazerFallingDiagonalAlternateStartupLong, 1),
            Show(F.ChargedSpazerFallingDiagonalSeed, 1),
            Show(F.ChargedSpazerFallingDiagonalAlternateSeed, 1),
            Show(F.ChargedSpazerFallingDiagonalParting, 1),
            Show(F.ChargedSpazerFallingDiagonalAlternateParting, 1),
        ],
        [P.ChargedSpazerFallingDiagonalSpread] =
        [
            Show(F.ChargedSpazerFallingDiagonalSpread, 1),
            Show(F.ChargedSpazerFallingDiagonalAlternateSpread, 1),
        ],
        [P.ChargedSpazerWaveUp] =
        [
            Show(F.ChargedSpazerWaveUpStartupShort, 1),
            Show(F.ChargedSpazerWaveUpVerticalAlternateStartupShort, 1),
            Show(F.ChargedSpazerWaveUpStartupLong, 1),
            Show(F.ChargedSpazerWaveUpVerticalAlternateStartupLong, 1),
        ],
        [P.ChargedSpazerWaveUpWaving] =
        [
            Show(F.ChargedSpazerWaveUpSeed, 1),
            Show(F.ChargedSpazerWaveUpVerticalAlternateSeed, 1),
            Show(F.ChargedSpazerWaveUpSplitting, 1),
            Show(F.ChargedSpazerWaveUpVerticalAlternateSplitting, 1),
            Show(F.ChargedSpazerWaveUpParting, 1),
            Show(F.ChargedSpazerWaveUpVerticalAlternateParting, 1),
            Show(F.ChargedSpazerWaveUpSpreading, 1),
            Show(F.ChargedSpazerWaveUpVerticalAlternateSpreading, 1),
            Show(F.ChargedSpazerWaveUpNearlySpread, 1),
            Show(F.ChargedSpazerWaveUpVerticalAlternateNearlySpread, 1),
            Show(F.ChargedSpazerWaveUpSpread, 1),
            Show(F.ChargedSpazerWaveUpVerticalAlternateSpread, 1),
            Show(F.ChargedSpazerWaveUpNearlySpread, 1),
            Show(F.ChargedSpazerWaveUpVerticalAlternateNearlySpread, 1),
            Show(F.ChargedSpazerWaveUpSpreading, 1),
            Show(F.ChargedSpazerWaveUpVerticalAlternateSpreading, 1),
            Show(F.ChargedSpazerWaveUpParting, 1),
            Show(F.ChargedSpazerWaveUpVerticalAlternateParting, 1),
            Show(F.ChargedSpazerWaveUpSplittingNarrowHitboxTrailless, 1),
            Show(F.ChargedSpazerWaveUpVerticalAlternateSplittingNarrowHitboxTrailless, 1),
        ],
        [P.ChargedSpazerWaveUpRight] =
        [
            Show(F.ChargedSpazerWaveUpRightStartupShort, 1),
            Show(F.ChargedSpazerWaveUpRightAlternateStartupShort, 1),
            Show(F.ChargedSpazerWaveUpRightStartupLong, 1),
            Show(F.ChargedSpazerWaveUpRightAlternateStartupLong, 1),
        ],
        [P.ChargedSpazerWaveUpRightWaving] =
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
        [P.ChargedSpazerWaveRightWaving] =
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
        [P.ChargedSpazerWaveDownRightWaving] =
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
        [P.ChargedSpazerWaveDown] =
        [
            Show(F.ChargedSpazerWaveDownStartupShort, 1),
            Show(F.ChargedSpazerWaveDownVerticalAlternateStartupShort, 1),
            Show(F.ChargedSpazerWaveDownStartupLong, 1),
            Show(F.ChargedSpazerWaveDownVerticalAlternateStartupLong, 1),
        ],
        [P.ChargedSpazerWaveDownWaving] =
        [
            Show(F.ChargedSpazerWaveDownSeed, 1),
            Show(F.ChargedSpazerWaveDownVerticalAlternateSeed, 1),
            Show(F.ChargedSpazerWaveDownSplitting, 1),
            Show(F.ChargedSpazerWaveDownVerticalAlternateSplitting, 1),
            Show(F.ChargedSpazerWaveDownParting, 1),
            Show(F.ChargedSpazerWaveDownVerticalAlternateParting, 1),
            Show(F.ChargedSpazerWaveDownSpreading, 1),
            Show(F.ChargedSpazerWaveDownVerticalAlternateSpreading, 1),
            Show(F.ChargedSpazerWaveDownNearlySpread, 1),
            Show(F.ChargedSpazerWaveDownVerticalAlternateNearlySpread, 1),
            Show(F.ChargedSpazerWaveDownSpread, 1),
            Show(F.ChargedSpazerWaveDownVerticalAlternateSpread, 1),
            Show(F.ChargedSpazerWaveDownNearlySpread, 1),
            Show(F.ChargedSpazerWaveDownVerticalAlternateNearlySpread, 1),
            Show(F.ChargedSpazerWaveDownSpreading, 1),
            Show(F.ChargedSpazerWaveDownVerticalAlternateSpreading, 1),
            Show(F.ChargedSpazerWaveDownParting, 1),
            Show(F.ChargedSpazerWaveDownVerticalAlternateParting, 1),
            Show(F.ChargedSpazerWaveDownSplittingNarrowHitboxTrailless, 1),
            Show(F.ChargedSpazerWaveDownVerticalAlternateSplittingNarrowHitboxTrailless, 1),
        ],
        [P.ChargedSpazerWaveDownLeft] =
        [
            Show(F.ChargedSpazerWaveDownLeftUpRightStartupShort, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightAlternateStartupShort, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightStartupLong, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightAlternateStartupLong, 1),
        ],
        [P.ChargedSpazerWaveDownLeftWaving] =
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
        [P.ChargedSpazerWaveLeftWaving] =
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
        [P.ChargedSpazerWaveUpLeftWaving] =
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
        [P.ChargedPlasmaVertical] =
        [
            Show(F.ChargedPlasmaVerticalCore, 1),
            Show(F.ChargedPlasmaVerticalAlternateCore, 1),
            Show(F.ChargedPlasmaVerticalShort, 1),
            Show(F.ChargedPlasmaVerticalAlternateShort, 1),
            Show(F.ChargedPlasmaVerticalLong, 1),
            Show(F.ChargedPlasmaVerticalAlternateLong, 1),
        ],
        [P.ChargedPlasmaVerticalFullLength] =
        [
            Show(F.ChargedPlasmaVerticalFull, 1),
            Show(F.ChargedPlasmaVerticalAlternateFull, 1),
        ],
        [P.ChargedPlasmaRisingDiagonal] =
        [
            Show(F.ChargedPlasmaRisingDiagonalCore, 1),
            Show(F.ChargedPlasmaRisingDiagonalAlternateCore, 1),
            Show(F.ChargedPlasmaRisingDiagonalShort, 1),
            Show(F.ChargedPlasmaRisingDiagonalAlternateShort, 1),
            Show(F.ChargedPlasmaRisingDiagonalLong, 1),
            Show(F.ChargedPlasmaRisingDiagonalAlternateLong, 1),
        ],
        [P.ChargedPlasmaRisingDiagonalFullLength] =
        [
            Show(F.ChargedPlasmaRisingDiagonalFull, 1),
            Show(F.ChargedPlasmaRisingDiagonalAlternateFull, 1),
        ],
        [P.ChargedPlasmaHorizontal] =
        [
            Show(F.ChargedPlasmaHorizontalCore, 1),
            Show(F.ChargedPlasmaHorizontalAlternateCore, 1),
            Show(F.ChargedPlasmaHorizontalShort, 1),
            Show(F.ChargedPlasmaHorizontalAlternateShort, 1),
            Show(F.ChargedPlasmaHorizontalLong, 1),
            Show(F.ChargedPlasmaHorizontalAlternateLong, 1),
        ],
        [P.ChargedPlasmaHorizontalFullLength] =
        [
            Show(F.ChargedPlasmaHorizontalFull, 1),
            Show(F.ChargedPlasmaHorizontalAlternateFull, 1),
        ],
        [P.ChargedPlasmaFallingDiagonal] =
        [
            Show(F.ChargedPlasmaFallingDiagonalCore, 1),
            Show(F.ChargedPlasmaFallingDiagonalAlternateCore, 1),
            Show(F.ChargedPlasmaFallingDiagonalShort, 1),
            Show(F.ChargedPlasmaFallingDiagonalAlternateShort, 1),
            Show(F.ChargedPlasmaFallingDiagonalLong, 1),
            Show(F.ChargedPlasmaFallingDiagonalAlternateLong, 1),
        ],
        [P.ChargedPlasmaFallingDiagonalFullLength] =
        [
            Show(F.ChargedPlasmaFallingDiagonalFull, 1),
            Show(F.ChargedPlasmaFallingDiagonalAlternateFull, 1),
        ],
        [P.ChargedPlasmaWaveVertical] =
        [
            Show(F.ChargedPlasmaWaveVerticalCore, 1),
            Show(F.ChargedPlasmaWaveVerticalAlternateCore, 1),
            Show(F.ChargedPlasmaWaveVerticalShort, 1),
            Show(F.ChargedPlasmaWaveVerticalAlternateShort, 1),
            Show(F.ChargedPlasmaWaveVerticalLong, 1),
            Show(F.ChargedPlasmaWaveVerticalAlternateLong, 1),
        ],
        [P.ChargedPlasmaWaveVerticalWaving] =
        [
            Show(F.ChargedPlasmaWaveVerticalLongCore, 1),
            Show(F.ChargedPlasmaWaveVerticalAlternateLongCore, 1),
            Show(F.ChargedPlasmaWaveVerticalLongLobesNear, 1),
            Show(F.ChargedPlasmaWaveVerticalAlternateLobesNear, 1),
            Show(F.ChargedPlasmaWaveVerticalLongLobesMidway, 1),
            Show(F.ChargedPlasmaWaveVerticalAlternateLobesMidway, 1),
            Show(F.ChargedPlasmaWaveVerticalLongLobesFar, 1),
            Show(F.ChargedPlasmaWaveVerticalAlternateLobesFar, 1),
            Show(F.ChargedPlasmaWaveVerticalLongLobesPeak, 1),
            Show(F.ChargedPlasmaWaveVerticalAlternateLobesPeak, 1),
            Show(F.ChargedPlasmaWaveVerticalLongLobesFar, 1),
            Show(F.ChargedPlasmaWaveVerticalAlternateLobesFar, 1),
            Show(F.ChargedPlasmaWaveVerticalLongLobesMidway, 1),
            Show(F.ChargedPlasmaWaveVerticalAlternateLobesMidway, 1),
            Show(F.ChargedPlasmaWaveVerticalLongLobesNear, 1),
            Show(F.ChargedPlasmaWaveVerticalAlternateLobesNear, 1),
        ],
        [P.ChargedPlasmaWaveRisingDiagonal] =
        [
            Show(F.ChargedPlasmaWaveRisingDiagonalCore, 1),
            Show(F.ChargedPlasmaWaveRisingDiagonalAlternateCore, 1),
            Show(F.ChargedPlasmaWaveRisingDiagonalShort, 1),
            Show(F.ChargedPlasmaWaveRisingDiagonalAlternateShort, 1),
            Show(F.ChargedPlasmaWaveRisingDiagonalLong, 1),
            Show(F.ChargedPlasmaWaveRisingDiagonalAlternateLong, 1),
        ],
        [P.ChargedPlasmaWaveRisingDiagonalWaving] =
        [
            Show(F.ChargedPlasmaWaveRisingDiagonalLongCore, 1),
            Show(F.ChargedPlasmaWaveRisingDiagonalAlternateLongCore, 1),
            Show(F.ChargedPlasmaWaveRisingDiagonalLongLobesNear, 1),
            Show(F.ChargedPlasmaWaveRisingDiagonalAlternateLobesNear, 1),
            Show(F.ChargedPlasmaWaveRisingDiagonalLongLobesMidway, 1),
            Show(F.ChargedPlasmaWaveRisingDiagonalAlternateLobesMidway, 1),
            Show(F.ChargedPlasmaWaveRisingDiagonalLongLobesFar, 1),
            Show(F.ChargedPlasmaWaveRisingDiagonalAlternateLobesFar, 1),
            Show(F.ChargedPlasmaWaveRisingDiagonalLongLobesPeak, 1),
            Show(F.ChargedPlasmaWaveRisingDiagonalAlternateLobesPeak, 1),
            Show(F.ChargedPlasmaWaveRisingDiagonalLongLobesFar, 1),
            Show(F.ChargedPlasmaWaveRisingDiagonalAlternateLobesFar, 1),
            Show(F.ChargedPlasmaWaveRisingDiagonalLongLobesMidway, 1),
            Show(F.ChargedPlasmaWaveRisingDiagonalAlternateLobesMidway, 1),
            Show(F.ChargedPlasmaWaveRisingDiagonalLongLobesNear, 1),
            Show(F.ChargedPlasmaWaveRisingDiagonalAlternateLobesNear, 1),
        ],
        [P.ChargedPlasmaWaveHorizontal] =
        [
            Show(F.ChargedPlasmaWaveHorizontalCore, 1),
            Show(F.ChargedPlasmaWaveHorizontalAlternateCore, 1),
            Show(F.ChargedPlasmaWaveHorizontalShort, 1),
            Show(F.ChargedPlasmaWaveHorizontalAlternateShort, 1),
            Show(F.ChargedPlasmaWaveHorizontalLong, 1),
            Show(F.ChargedPlasmaWaveHorizontalAlternateLong, 1),
        ],
        [P.ChargedPlasmaWaveHorizontalWaving] =
        [
            Show(F.ChargedPlasmaWaveHorizontalLongCore, 1),
            Show(F.ChargedPlasmaWaveHorizontalAlternateLongCore, 1),
            Show(F.ChargedPlasmaWaveHorizontalLongLobesNear, 1),
            Show(F.ChargedPlasmaWaveHorizontalAlternateLobesNear, 1),
            Show(F.ChargedPlasmaWaveHorizontalLongLobesMidway, 1),
            Show(F.ChargedPlasmaWaveHorizontalAlternateLobesMidway, 1),
            Show(F.ChargedPlasmaWaveHorizontalLongLobesFar, 1),
            Show(F.ChargedPlasmaWaveHorizontalAlternateLobesFar, 1),
            Show(F.ChargedPlasmaWaveHorizontalLongLobesPeak, 1),
            Show(F.ChargedPlasmaWaveHorizontalAlternateLobesPeak, 1),
            Show(F.ChargedPlasmaWaveHorizontalLongLobesFar, 1),
            Show(F.ChargedPlasmaWaveHorizontalAlternateLobesFar, 1),
            Show(F.ChargedPlasmaWaveHorizontalLongLobesMidway, 1),
            Show(F.ChargedPlasmaWaveHorizontalAlternateLobesMidway, 1),
            Show(F.ChargedPlasmaWaveHorizontalLongLobesNear, 1),
            Show(F.ChargedPlasmaWaveHorizontalAlternateLobesNear, 1),
        ],
        [P.ChargedPlasmaWaveFallingDiagonal] =
        [
            Show(F.ChargedPlasmaWaveFallingDiagonalCore, 1),
            Show(F.ChargedPlasmaWaveFallingDiagonalAlternateCore, 1),
            Show(F.ChargedPlasmaWaveFallingDiagonalShort, 1),
            Show(F.ChargedPlasmaWaveFallingDiagonalAlternateShort, 1),
            Show(F.ChargedPlasmaWaveFallingDiagonalLong, 1),
            Show(F.ChargedPlasmaWaveFallingDiagonalAlternateLong, 1),
        ],
        [P.ChargedPlasmaWaveFallingDiagonalWaving] =
        [
            Show(F.ChargedPlasmaWaveFallingDiagonalLongCore, 1),
            Show(F.ChargedPlasmaWaveFallingDiagonalAlternateLongCore, 1),
            Show(F.ChargedPlasmaWaveFallingDiagonalLongLobesNear, 1),
            Show(F.ChargedPlasmaWaveFallingDiagonalAlternateLobesNear, 1),
            Show(F.ChargedPlasmaWaveFallingDiagonalLongLobesMidway, 1),
            Show(F.ChargedPlasmaWaveFallingDiagonalAlternateLobesMidway, 1),
            Show(F.ChargedPlasmaWaveFallingDiagonalLongLobesFar, 1),
            Show(F.ChargedPlasmaWaveFallingDiagonalAlternateLobesFar, 1),
            Show(F.ChargedPlasmaWaveFallingDiagonalLongLobesPeak, 1),
            Show(F.ChargedPlasmaWaveFallingDiagonalAlternateLobesPeak, 1),
            Show(F.ChargedPlasmaWaveFallingDiagonalLongLobesFar, 1),
            Show(F.ChargedPlasmaWaveFallingDiagonalAlternateLobesFar, 1),
            Show(F.ChargedPlasmaWaveFallingDiagonalLongLobesMidway, 1),
            Show(F.ChargedPlasmaWaveFallingDiagonalAlternateLobesMidway, 1),
            Show(F.ChargedPlasmaWaveFallingDiagonalLongLobesNear, 1),
            Show(F.ChargedPlasmaWaveFallingDiagonalAlternateLobesNear, 1),
        ],
        [P.MissileUp] =
        [
            Hold(F.MissileUp),
        ],
        [P.MissileUpRight] =
        [
            Hold(F.MissileUpRight),
        ],
        [P.MissileRight] =
        [
            Hold(F.MissileRight),
        ],
        [P.MissileDownRight] =
        [
            Hold(F.MissileDownRight),
        ],
        [P.MissileDown] =
        [
            Hold(F.MissileDown),
        ],
        [P.MissileDownLeft] =
        [
            Hold(F.MissileDownLeft),
        ],
        [P.MissileLeft] =
        [
            Hold(F.MissileLeft),
        ],
        [P.MissileUpLeft] =
        [
            Hold(F.MissileUpLeft),
        ],
        [P.SuperMissileUp] =
        [
            Hold(F.SuperMissileUp),
        ],
        [P.SuperMissileUpRight] =
        [
            Hold(F.SuperMissileUpRight),
        ],
        [P.SuperMissileRight] =
        [
            Hold(F.SuperMissileRight),
        ],
        [P.SuperMissileDownRight] =
        [
            Hold(F.SuperMissileDownRight),
        ],
        [P.SuperMissileDown] =
        [
            Hold(F.SuperMissileDown),
        ],
        [P.SuperMissileDownLeft] =
        [
            Hold(F.SuperMissileDownLeft),
        ],
        [P.SuperMissileLeft] =
        [
            Hold(F.SuperMissileLeft),
        ],
        [P.SuperMissileUpLeft] =
        [
            Hold(F.SuperMissileUpLeft),
        ],
        [P.SuperMissileLink] =
        [
            Hold(F.SuperMissileLinkHidden),
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
        [P.ShinesparkEcho] =
        [
            Hold(F.ShinesparkEchoHidden),
        ],
        [P.SpazerSpecialAttackTrail] =
        [
            Show(F.SpazerSpecialAttackTrailHidden, 2),
            Show(F.SpazerSpecialAttackTrailHiddenWide, 2),
        ],
        [P.SpazerSpecialAttackTrailWidest] =
        [
            Hold(F.SpazerSpecialAttackTrailHiddenWidest),
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
        [P.PowerUp] = Repeating(P.PowerUp),
        [P.PowerUpRight] = Repeating(P.PowerUpRight),
        [P.PowerRight] = Repeating(P.PowerRight),
        [P.PowerDownRight] = Repeating(P.PowerDownRight),
        [P.PowerDown] = Repeating(P.PowerDown),
        [P.PowerDownLeft] = Repeating(P.PowerDownLeft),
        [P.PowerLeft] = Repeating(P.PowerLeft),
        [P.PowerUpLeft] = Repeating(P.PowerUpLeft),
        [P.WaveUp] = OpeningThenRepeating(P.WaveUp, P.WaveVertical),
        [P.WaveVertical] = Repeating(P.WaveVertical),
        [P.WaveRisingDiagonal] = Repeating(P.WaveRisingDiagonal),
        [P.WaveHorizontal] = Repeating(P.WaveHorizontal),
        [P.WaveFallingDiagonal] = Repeating(P.WaveFallingDiagonal),
        [P.Ice] = Repeating(P.Ice),
        [P.SpazerUp] = OpeningThenRepeating(P.SpazerUp, P.SpazerUpSpread),
        [P.SpazerUpSpread] = Repeating(P.SpazerUpSpread),
        [P.SpazerUpRight] = OpeningThenRepeating(P.SpazerUpRight, P.SpazerUpRightSpread),
        [P.SpazerUpRightSpread] = Repeating(P.SpazerUpRightSpread),
        [P.SpazerRight] = OpeningThenRepeating(P.SpazerRight, P.SpazerRightSpread),
        [P.SpazerRightSpread] = Repeating(P.SpazerRightSpread),
        [P.SpazerDownRight] = OpeningThenRepeating(P.SpazerDownRight, P.SpazerDownRightSpread),
        [P.SpazerDownRightSpread] = Repeating(P.SpazerDownRightSpread),
        [P.SpazerDown] = OpeningThenRepeating(P.SpazerDown, P.SpazerDownSpread),
        [P.SpazerDownSpread] = Repeating(P.SpazerDownSpread),
        [P.SpazerDownLeft] = OpeningThenRepeating(P.SpazerDownLeft, P.SpazerDownLeftSpread),
        [P.SpazerDownLeftSpread] = Repeating(P.SpazerDownLeftSpread),
        [P.SpazerLeft] = OpeningThenRepeating(P.SpazerLeft, P.SpazerLeftSpread),
        [P.SpazerLeftSpread] = Repeating(P.SpazerLeftSpread),
        [P.SpazerUpLeft] = OpeningThenRepeating(P.SpazerUpLeft, P.SpazerUpLeftSpread),
        [P.SpazerUpLeftSpread] = Repeating(P.SpazerUpLeftSpread),
        [P.SpazerWaveUp] = Repeating(P.SpazerWaveUp),
        [P.SpazerWaveUpRight] = Repeating(P.SpazerWaveUpRight),
        [P.SpazerWaveRight] = Repeating(P.SpazerWaveRight),
        [P.SpazerWaveDownRight] = Repeating(P.SpazerWaveDownRight),
        [P.SpazerWaveDown] = Repeating(P.SpazerWaveDown),
        [P.SpazerWaveDownLeft] = Repeating(P.SpazerWaveDownLeft),
        [P.SpazerWaveLeft] = Repeating(P.SpazerWaveLeft),
        [P.SpazerWaveUpLeft] = Repeating(P.SpazerWaveUpLeft),
        [P.PlasmaVertical] = OpeningThenRepeating(P.PlasmaVertical, P.PlasmaVerticalFullLength),
        [P.PlasmaVerticalFullLength] = Repeating(P.PlasmaVerticalFullLength),
        [P.PlasmaRisingDiagonal] = OpeningThenRepeating(P.PlasmaRisingDiagonal, P.PlasmaRisingDiagonalFullLength),
        [P.PlasmaRisingDiagonalFullLength] = Repeating(P.PlasmaRisingDiagonalFullLength),
        [P.PlasmaHorizontal] = OpeningThenRepeating(P.PlasmaHorizontal, P.PlasmaHorizontalFullLength),
        [P.PlasmaHorizontalFullLength] = Repeating(P.PlasmaHorizontalFullLength),
        [P.PlasmaFallingDiagonal] = OpeningThenRepeating(P.PlasmaFallingDiagonal, P.PlasmaFallingDiagonalFullLength),
        [P.PlasmaFallingDiagonalFullLength] = Repeating(P.PlasmaFallingDiagonalFullLength),
        [P.PlasmaIceWaveVertical] = OpeningThenRepeating(P.PlasmaIceWaveVertical, P.PlasmaWaveVertical),
        [P.PlasmaWaveVertical] = Repeating(P.PlasmaWaveVertical),
        [P.PlasmaIceWaveRisingDiagonal] = OpeningThenRepeating(P.PlasmaIceWaveRisingDiagonal, P.PlasmaWaveRisingDiagonal),
        [P.PlasmaWaveRisingDiagonal] = Repeating(P.PlasmaWaveRisingDiagonal),
        [P.PlasmaWaveHorizontal] = OpeningThenRepeating(P.PlasmaWaveHorizontal, P.PlasmaWaveHorizontalSwing),
        [P.PlasmaWaveHorizontalSwing] = Repeating(P.PlasmaWaveHorizontalSwing),
        [P.PlasmaIceWaveFallingDiagonal] = OpeningThenRepeating(P.PlasmaIceWaveFallingDiagonal, P.PlasmaWaveFallingDiagonal),
        [P.PlasmaWaveFallingDiagonal] = Repeating(P.PlasmaWaveFallingDiagonal),
        [P.ChargedPowerUp] = Repeating(P.ChargedPowerUp),
        [P.ChargedPowerUpRight] = Repeating(P.ChargedPowerUpRight),
        [P.ChargedPowerRight] = Repeating(P.ChargedPowerRight),
        [P.ChargedPowerDownRight] = Repeating(P.ChargedPowerDownRight),
        [P.ChargedPowerDown] = Repeating(P.ChargedPowerDown),
        [P.ChargedPowerDownLeft] = Repeating(P.ChargedPowerDownLeft),
        [P.ChargedPowerLeft] = Repeating(P.ChargedPowerLeft),
        [P.ChargedPowerUpLeft] = Repeating(P.ChargedPowerUpLeft),
        [P.ChargedWaveUp] = OpeningThenRepeating(P.ChargedWaveUp, P.ChargedWaveVertical),
        [P.ChargedWaveVertical] = Repeating(P.ChargedWaveVertical),
        [P.ChargedWaveRisingDiagonal] = Repeating(P.ChargedWaveRisingDiagonal),
        [P.ChargedWaveHorizontal] = Repeating(P.ChargedWaveHorizontal),
        [P.ChargedWaveFallingDiagonal] = Repeating(P.ChargedWaveFallingDiagonal),
        [P.ChargedIce] = Repeating(P.ChargedIce),
        [P.ChargedIceWaveUp] = OpeningThenRepeating(P.ChargedIceWaveUp, P.ChargedIceWaveVertical),
        [P.ChargedIceWaveVertical] = Repeating(P.ChargedIceWaveVertical),
        [P.ChargedIceWaveRisingDiagonal] = Repeating(P.ChargedIceWaveRisingDiagonal),
        [P.ChargedIceWaveHorizontal] = Repeating(P.ChargedIceWaveHorizontal),
        [P.ChargedIceWaveFallingDiagonal] = Repeating(P.ChargedIceWaveFallingDiagonal),
        [P.ChargedSpazerVertical] = OpeningThenRepeating(P.ChargedSpazerVertical, P.ChargedSpazerVerticalSpread),
        [P.ChargedSpazerVerticalSpread] = Repeating(P.ChargedSpazerVerticalSpread),
        [P.ChargedSpazerRisingDiagonal] = OpeningThenRepeating(P.ChargedSpazerRisingDiagonal, P.ChargedSpazerRisingDiagonalSpread),
        [P.ChargedSpazerRisingDiagonalSpread] = Repeating(P.ChargedSpazerRisingDiagonalSpread),
        [P.ChargedSpazerHorizontal] = OpeningThenRepeating(P.ChargedSpazerHorizontal, P.ChargedSpazerHorizontalSpread),
        [P.ChargedSpazerHorizontalSpread] = Repeating(P.ChargedSpazerHorizontalSpread),
        [P.ChargedSpazerFallingDiagonal] = OpeningThenRepeating(P.ChargedSpazerFallingDiagonal, P.ChargedSpazerFallingDiagonalSpread),
        [P.ChargedSpazerFallingDiagonalSpread] = Repeating(P.ChargedSpazerFallingDiagonalSpread),
        [P.ChargedSpazerWaveUp] = OpeningThenRepeating(P.ChargedSpazerWaveUp, P.ChargedSpazerWaveUpWaving),
        [P.ChargedSpazerWaveUpWaving] = Repeating(P.ChargedSpazerWaveUpWaving),
        [P.ChargedSpazerWaveUpRight] = OpeningThenRepeating(P.ChargedSpazerWaveUpRight, P.ChargedSpazerWaveUpRightWaving),
        [P.ChargedSpazerWaveUpRightWaving] = Repeating(P.ChargedSpazerWaveUpRightWaving),
        [P.ChargedSpazerWaveRight] = OpeningThenRepeating(P.ChargedSpazerWaveRight, P.ChargedSpazerWaveRightWaving),
        [P.ChargedSpazerWaveRightWaving] = Repeating(P.ChargedSpazerWaveRightWaving),
        [P.ChargedSpazerWaveDownRight] = OpeningThenRepeating(P.ChargedSpazerWaveDownRight, P.ChargedSpazerWaveDownRightWaving),
        [P.ChargedSpazerWaveDownRightWaving] = Repeating(P.ChargedSpazerWaveDownRightWaving),
        [P.ChargedSpazerWaveDown] = OpeningThenRepeating(P.ChargedSpazerWaveDown, P.ChargedSpazerWaveDownWaving),
        [P.ChargedSpazerWaveDownWaving] = Repeating(P.ChargedSpazerWaveDownWaving),
        [P.ChargedSpazerWaveDownLeft] = OpeningThenRepeating(P.ChargedSpazerWaveDownLeft, P.ChargedSpazerWaveDownLeftWaving),
        [P.ChargedSpazerWaveDownLeftWaving] = Repeating(P.ChargedSpazerWaveDownLeftWaving),
        [P.ChargedSpazerWaveLeft] = OpeningThenRepeating(P.ChargedSpazerWaveLeft, P.ChargedSpazerWaveLeftWaving),
        [P.ChargedSpazerWaveLeftWaving] = Repeating(P.ChargedSpazerWaveLeftWaving),
        [P.ChargedSpazerWaveUpLeft] = OpeningThenRepeating(P.ChargedSpazerWaveUpLeft, P.ChargedSpazerWaveUpLeftWaving),
        [P.ChargedSpazerWaveUpLeftWaving] = Repeating(P.ChargedSpazerWaveUpLeftWaving),
        [P.ChargedPlasmaVertical] = OpeningThenRepeating(P.ChargedPlasmaVertical, P.ChargedPlasmaVerticalFullLength),
        [P.ChargedPlasmaVerticalFullLength] = Repeating(P.ChargedPlasmaVerticalFullLength),
        [P.ChargedPlasmaRisingDiagonal] = OpeningThenRepeating(P.ChargedPlasmaRisingDiagonal, P.ChargedPlasmaRisingDiagonalFullLength),
        [P.ChargedPlasmaRisingDiagonalFullLength] = Repeating(P.ChargedPlasmaRisingDiagonalFullLength),
        [P.ChargedPlasmaHorizontal] = OpeningThenRepeating(P.ChargedPlasmaHorizontal, P.ChargedPlasmaHorizontalFullLength),
        [P.ChargedPlasmaHorizontalFullLength] = Repeating(P.ChargedPlasmaHorizontalFullLength),
        [P.ChargedPlasmaFallingDiagonal] = OpeningThenRepeating(P.ChargedPlasmaFallingDiagonal, P.ChargedPlasmaFallingDiagonalFullLength),
        [P.ChargedPlasmaFallingDiagonalFullLength] = Repeating(P.ChargedPlasmaFallingDiagonalFullLength),
        [P.ChargedPlasmaWaveVertical] = OpeningThenRepeating(P.ChargedPlasmaWaveVertical, P.ChargedPlasmaWaveVerticalWaving),
        [P.ChargedPlasmaWaveVerticalWaving] = Repeating(P.ChargedPlasmaWaveVerticalWaving),
        [P.ChargedPlasmaWaveRisingDiagonal] = OpeningThenRepeating(P.ChargedPlasmaWaveRisingDiagonal, P.ChargedPlasmaWaveRisingDiagonalWaving),
        [P.ChargedPlasmaWaveRisingDiagonalWaving] = Repeating(P.ChargedPlasmaWaveRisingDiagonalWaving),
        [P.ChargedPlasmaWaveHorizontal] = OpeningThenRepeating(P.ChargedPlasmaWaveHorizontal, P.ChargedPlasmaWaveHorizontalWaving),
        [P.ChargedPlasmaWaveHorizontalWaving] = Repeating(P.ChargedPlasmaWaveHorizontalWaving),
        [P.ChargedPlasmaWaveFallingDiagonal] = OpeningThenRepeating(P.ChargedPlasmaWaveFallingDiagonal, P.ChargedPlasmaWaveFallingDiagonalWaving),
        [P.ChargedPlasmaWaveFallingDiagonalWaving] = Repeating(P.ChargedPlasmaWaveFallingDiagonalWaving),
        [P.MissileUp] = Repeating(P.MissileUp),
        [P.MissileUpRight] = Repeating(P.MissileUpRight),
        [P.MissileRight] = Repeating(P.MissileRight),
        [P.MissileDownRight] = Repeating(P.MissileDownRight),
        [P.MissileDown] = Repeating(P.MissileDown),
        [P.MissileDownLeft] = Repeating(P.MissileDownLeft),
        [P.MissileLeft] = Repeating(P.MissileLeft),
        [P.MissileUpLeft] = Repeating(P.MissileUpLeft),
        [P.SuperMissileUp] = Repeating(P.SuperMissileUp),
        [P.SuperMissileUpRight] = Repeating(P.SuperMissileUpRight),
        [P.SuperMissileRight] = Repeating(P.SuperMissileRight),
        [P.SuperMissileDownRight] = Repeating(P.SuperMissileDownRight),
        [P.SuperMissileDown] = Repeating(P.SuperMissileDown),
        [P.SuperMissileDownLeft] = Repeating(P.SuperMissileDownLeft),
        [P.SuperMissileLeft] = Repeating(P.SuperMissileLeft),
        [P.SuperMissileUpLeft] = Repeating(P.SuperMissileUpLeft),
        [P.SuperMissileLink] = Repeating(P.SuperMissileLink),
        [P.PowerBomb] = Repeating(P.PowerBomb),
        [P.FastPowerBomb] = Repeating(P.FastPowerBomb),
        [P.Bomb] = Repeating(P.Bomb),
        [P.FastBomb] = Repeating(P.FastBomb),
        [P.BeamExplosion] = PlayedOnce(P.BeamExplosion),
        [P.MissileExplosion] = PlayedOnce(P.MissileExplosion),
        [P.BombExplosion] = PlayedOnce(P.BombExplosion),
        [P.PlasmaSpecialAttack] = Repeating(P.PlasmaSpecialAttack),
        [P.SuperMissileExplosion] = PlayedOnce(P.SuperMissileExplosion),
        [P.ShinesparkEcho] = Repeating(P.ShinesparkEcho),
        [P.SpazerSpecialAttackTrail] = OpeningThenRepeating(P.SpazerSpecialAttackTrail, P.SpazerSpecialAttackTrailWidest),
        [P.SpazerSpecialAttackTrailWidest] = Repeating(P.SpazerSpecialAttackTrailWidest),
        [P.WaveSpecialAttack] = Repeating(P.WaveSpecialAttack),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<SamusProjectileSegment> StartingWith(SamusProjectileSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<SamusProjectileFrameId, NoAnimationCue> Show(SamusProjectileFrameId frame, int duration) => new AnimationBeat<SamusProjectileFrameId, NoAnimationCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> from now on (#2347): a still picture the projectile keeps until it changes animation.</summary>
    private static AnimationBeat<SamusProjectileFrameId, NoAnimationCue> Hold(SamusProjectileFrameId frame) => new AnimationBeat<SamusProjectileFrameId, NoAnimationCue>.HeldFrame(frame);

    /// <summary>An animation that plays <paramref name="segment"/> and, past its end, plays it again.</summary>
    private static Animation<SamusProjectileSegment> Repeating(SamusProjectileSegment segment) =>
        new([Segments.Whole(segment)], AnimationEnd.LoopsFrom(0));

    /// <summary>An animation that plays <paramref name="opening"/> once, then plays <paramref name="repeated"/> over and over.</summary>
    private static Animation<SamusProjectileSegment> OpeningThenRepeating(SamusProjectileSegment opening, SamusProjectileSegment repeated) =>
        new([Segments.Whole(opening), Segments.Whole(repeated)], AnimationEnd.LoopsFrom(1));

    /// <summary>An animation that plays <paramref name="segment"/> once and is over: an explosion.</summary>
    private static Animation<SamusProjectileSegment> PlayedOnce(SamusProjectileSegment segment) =>
        new([Segments.Whole(segment)], AnimationEnd.Stops);
}
