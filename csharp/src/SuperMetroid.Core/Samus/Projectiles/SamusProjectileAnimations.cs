using SuperMetroid.Core.Animation;
using F = SuperMetroid.Core.Samus.Projectiles.SamusProjectileFrameId;
using P = SuperMetroid.Core.Samus.Projectiles.SamusProjectileSegment;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The Samus projectiles' animations (#1301, #2347): each segment shows its frames for their durations or holds a still
/// picture, and each animation plays whole segments, repeating its last or ending.
/// </summary>
internal static partial class SamusProjectileAnimations
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
            Show(F.ChargedPowerUpOrb, 1),
        ],
        [P.ChargedPowerUpRight] =
        [
            Show(F.ChargedPowerUpRight, 1),
            Show(F.ChargedPowerUpRightOrb, 1),
        ],
        [P.ChargedPowerRight] =
        [
            Show(F.ChargedPowerRight, 1),
            Show(F.ChargedPowerRightOrb, 1),
        ],
        [P.ChargedPowerDownRight] =
        [
            Show(F.ChargedPowerDownRight, 1),
            Show(F.ChargedPowerDownRightOrb, 1),
        ],
        [P.ChargedPowerDown] =
        [
            Show(F.ChargedPowerDown, 1),
            Show(F.ChargedPowerDownOrb, 1),
        ],
        [P.ChargedPowerDownLeft] =
        [
            Show(F.ChargedPowerDownLeft, 1),
            Show(F.ChargedPowerDownLeftOrb, 1),
        ],
        [P.ChargedPowerLeft] =
        [
            Show(F.ChargedPowerLeft, 1),
            Show(F.ChargedPowerLeftOrb, 1),
        ],
        [P.ChargedPowerUpLeft] =
        [
            Show(F.ChargedPowerUpLeft, 1),
            Show(F.ChargedPowerUpLeftOrb, 1),
        ],
        [P.ChargedWaveUp] =
        [
            Show(F.ChargedWaveUpHidden, 3),
        ],
        [P.ChargedWaveVertical] =
        [
            Show(F.ChargedWaveVerticalCore, 1),
            Show(F.ChargedWaveVerticalCoreStarburst, 1),
            Show(F.ChargedWaveVerticalLobesNear, 1),
            Show(F.ChargedWaveVerticalLobesNearOrbRight, 1),
            Show(F.ChargedWaveVerticalLobesMidway, 1),
            Show(F.ChargedWaveVerticalLobesMidwayOrbRight, 1),
            Show(F.ChargedWaveVerticalLobesFar, 1),
            Show(F.ChargedWaveVerticalLobesFarOrbRight, 1),
            Show(F.ChargedWaveVerticalLobesPeak, 1),
            Show(F.ChargedWaveVerticalLobesPeakOrbRight, 1),
            Show(F.ChargedWaveVerticalLobesFar, 1),
            Show(F.ChargedWaveVerticalLobesFarOrbRight, 1),
            Show(F.ChargedWaveVerticalLobesMidway, 1),
            Show(F.ChargedWaveVerticalLobesMidwayOrbRight, 1),
            Show(F.ChargedWaveVerticalLobesNearOrbRight, 1),
            Show(F.ChargedWaveVerticalLobesNear, 1),
        ],
        [P.ChargedWaveRisingDiagonal] =
        [
            Show(F.ChargedWaveRisingDiagonalCore, 1),
            Show(F.ChargedWaveRisingDiagonalCoreStarburst, 1),
            Show(F.ChargedWaveRisingDiagonalLobesNear, 1),
            Show(F.ChargedWaveRisingDiagonalLobesNearOrbBelow, 1),
            Show(F.ChargedWaveRisingDiagonalLobesMidway, 1),
            Show(F.ChargedWaveRisingDiagonalLobesMidwayOrbBelow, 1),
            Show(F.ChargedWaveRisingDiagonalLobesFar, 1),
            Show(F.ChargedWaveRisingDiagonalLobesFarOrbBelow, 1),
            Show(F.ChargedWaveRisingDiagonalLobesPeak, 1),
            Show(F.ChargedWaveRisingDiagonalLobesPeakOrbBelow, 1),
            Show(F.ChargedWaveRisingDiagonalLobesFar, 1),
            Show(F.ChargedWaveRisingDiagonalLobesFarOrbBelow, 1),
            Show(F.ChargedWaveRisingDiagonalLobesMidway, 1),
            Show(F.ChargedWaveRisingDiagonalLobesMidwayOrbBelow, 1),
            Show(F.ChargedWaveRisingDiagonalLobesNear, 1),
            Show(F.ChargedWaveRisingDiagonalLobesNearOrbBelow, 1),
        ],
        [P.ChargedWaveHorizontal] =
        [
            Show(F.ChargedWaveHorizontalCore, 1),
            Show(F.ChargedWaveHorizontalCoreStarburst, 1),
            Show(F.ChargedWaveHorizontalLobesNear, 1),
            Show(F.ChargedWaveHorizontalLobesNearOrbBelow, 1),
            Show(F.ChargedWaveHorizontalLobesMidway, 1),
            Show(F.ChargedWaveHorizontalLobesMidwayOrbBelow, 1),
            Show(F.ChargedWaveHorizontalLobesFar, 1),
            Show(F.ChargedWaveHorizontalLobesFarOrbBelow, 1),
            Show(F.ChargedWaveHorizontalLobesPeak, 1),
            Show(F.ChargedWaveHorizontalLobesPeakOrbBelow, 1),
            Show(F.ChargedWaveHorizontalLobesFar, 1),
            Show(F.ChargedWaveHorizontalLobesFarOrbBelow, 1),
            Show(F.ChargedWaveHorizontalLobesMidway, 1),
            Show(F.ChargedWaveHorizontalLobesMidwayOrbBelow, 1),
            Show(F.ChargedWaveHorizontalLobesNearOrbBelow, 1),
            Show(F.ChargedWaveHorizontalLobesNear, 1),
        ],
        [P.ChargedWaveFallingDiagonal] =
        [
            Show(F.ChargedWaveFallingDiagonalCore, 1),
            Show(F.ChargedWaveFallingDiagonalCoreStarburst, 1),
            Show(F.ChargedWaveFallingDiagonalLobesNear, 1),
            Show(F.ChargedWaveFallingDiagonalLobesNearOrbBelow, 1),
            Show(F.ChargedWaveFallingDiagonalLobesMidway, 1),
            Show(F.ChargedWaveFallingDiagonalLobesMidwayOrbBelow, 1),
            Show(F.ChargedWaveFallingDiagonalLobesFar, 1),
            Show(F.ChargedWaveFallingDiagonalLobesFarOrbBelow, 1),
            Show(F.ChargedWaveFallingDiagonalLobesPeak, 1),
            Show(F.ChargedWaveFallingDiagonalLobesPeakOrbBelow, 1),
            Show(F.ChargedWaveFallingDiagonalLobesFar, 1),
            Show(F.ChargedWaveFallingDiagonalLobesFarOrbBelow, 1),
            Show(F.ChargedWaveFallingDiagonalLobesMidway, 1),
            Show(F.ChargedWaveFallingDiagonalLobesMidwayOrbBelow, 1),
            Show(F.ChargedWaveFallingDiagonalLobesNear, 1),
            Show(F.ChargedWaveFallingDiagonalLobesNearOrbBelow, 1),
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
            Show(F.ChargedIceWaveVerticalCoreStarburst, 1),
            Show(F.ChargedIceWaveVerticalLobesNear, 1),
            Show(F.ChargedIceWaveVerticalLobesNearOrbRight, 1),
            Show(F.ChargedIceWaveVerticalLobesMidway, 1),
            Show(F.ChargedIceWaveVerticalLobesMidwayOrbRight, 1),
            Show(F.ChargedIceWaveVerticalLobesFar, 1),
            Show(F.ChargedIceWaveVerticalLobesFarOrbRight, 1),
            Show(F.ChargedIceWaveVerticalLobesPeak, 1),
            Show(F.ChargedIceWaveVerticalLobesPeakOrbRight, 1),
            Show(F.ChargedIceWaveVerticalLobesFar, 1),
            Show(F.ChargedIceWaveVerticalLobesFarOrbRight, 1),
            Show(F.ChargedIceWaveVerticalLobesMidway, 1),
            Show(F.ChargedIceWaveVerticalLobesMidwayOrbRight, 1),
            Show(F.ChargedIceWaveVerticalLobesNearOrbRight, 1),
            Show(F.ChargedIceWaveVerticalLobesNear, 1),
        ],
        [P.ChargedIceWaveRisingDiagonal] =
        [
            Show(F.ChargedIceWaveRisingDiagonalCore, 1),
            Show(F.ChargedIceWaveRisingDiagonalCoreStarburst, 1),
            Show(F.ChargedIceWaveRisingDiagonalLobesNear, 1),
            Show(F.ChargedIceWaveRisingDiagonalLobesNearOrbBelow, 1),
            Show(F.ChargedIceWaveRisingDiagonalLobesMidway, 1),
            Show(F.ChargedIceWaveRisingDiagonalLobesMidwayOrbBelow, 1),
            Show(F.ChargedIceWaveRisingDiagonalLobesFar, 1),
            Show(F.ChargedIceWaveRisingDiagonalLobesFarOrbBelow, 1),
            Show(F.ChargedIceWaveRisingDiagonalLobesPeak, 1),
            Show(F.ChargedIceWaveRisingDiagonalLobesPeakOrbBelow, 1),
            Show(F.ChargedIceWaveRisingDiagonalLobesFar, 1),
            Show(F.ChargedIceWaveRisingDiagonalLobesFarOrbBelow, 1),
            Show(F.ChargedIceWaveRisingDiagonalLobesMidway, 1),
            Show(F.ChargedIceWaveRisingDiagonalLobesMidwayOrbBelow, 1),
            Show(F.ChargedIceWaveRisingDiagonalLobesNear, 1),
            Show(F.ChargedIceWaveRisingDiagonalLobesNearOrbBelow, 1),
        ],
        [P.ChargedIceWaveHorizontal] =
        [
            Show(F.ChargedIceWaveHorizontalCore, 1),
            Show(F.ChargedIceWaveHorizontalCoreStarburst, 1),
            Show(F.ChargedIceWaveHorizontalLobesNear, 1),
            Show(F.ChargedIceWaveHorizontalLobesNearOrbBelow, 1),
            Show(F.ChargedIceWaveHorizontalLobesMidway, 1),
            Show(F.ChargedIceWaveHorizontalLobesMidwayOrbBelow, 1),
            Show(F.ChargedIceWaveHorizontalLobesFar, 1),
            Show(F.ChargedIceWaveHorizontalLobesFarOrbBelow, 1),
            Show(F.ChargedIceWaveHorizontalLobesPeak, 1),
            Show(F.ChargedIceWaveHorizontalLobesPeakOrbBelow, 1),
            Show(F.ChargedIceWaveHorizontalLobesFar, 1),
            Show(F.ChargedIceWaveHorizontalLobesFarOrbBelow, 1),
            Show(F.ChargedIceWaveHorizontalLobesMidway, 1),
            Show(F.ChargedIceWaveHorizontalLobesMidwayOrbBelow, 1),
            Show(F.ChargedIceWaveHorizontalLobesNearOrbBelow, 1),
            Show(F.ChargedIceWaveHorizontalLobesNear, 1),
        ],
        [P.ChargedIceWaveFallingDiagonal] =
        [
            Show(F.ChargedIceWaveFallingDiagonalCore, 1),
            Show(F.ChargedIceWaveFallingDiagonalCoreStarburst, 1),
            Show(F.ChargedIceWaveFallingDiagonalLobesNear, 1),
            Show(F.ChargedIceWaveFallingDiagonalLobesNearOrbBelow, 1),
            Show(F.ChargedIceWaveFallingDiagonalLobesMidway, 1),
            Show(F.ChargedIceWaveFallingDiagonalLobesMidwayOrbBelow, 1),
            Show(F.ChargedIceWaveFallingDiagonalLobesFar, 1),
            Show(F.ChargedIceWaveFallingDiagonalLobesFarOrbBelow, 1),
            Show(F.ChargedIceWaveFallingDiagonalLobesPeak, 1),
            Show(F.ChargedIceWaveFallingDiagonalLobesPeakOrbBelow, 1),
            Show(F.ChargedIceWaveFallingDiagonalLobesFar, 1),
            Show(F.ChargedIceWaveFallingDiagonalLobesFarOrbBelow, 1),
            Show(F.ChargedIceWaveFallingDiagonalLobesMidway, 1),
            Show(F.ChargedIceWaveFallingDiagonalLobesMidwayOrbBelow, 1),
            Show(F.ChargedIceWaveFallingDiagonalLobesNear, 1),
            Show(F.ChargedIceWaveFallingDiagonalLobesNearOrbBelow, 1),
        ],
        [P.ChargedSpazerVertical] =
        [
            Show(F.ChargedSpazerVerticalStartupShort, 1),
            Show(F.ChargedSpazerVerticalThickStartupShort, 1),
            Show(F.ChargedSpazerVerticalStartupLong, 1),
            Show(F.ChargedSpazerVerticalThickStartupLong, 1),
            Show(F.ChargedSpazerVerticalSeed, 1),
            Show(F.ChargedSpazerVerticalThickSeed, 1),
            Show(F.ChargedSpazerVerticalParting, 1),
            Show(F.ChargedSpazerVerticalThickParting, 1),
        ],
        [P.ChargedSpazerVerticalSpread] =
        [
            Show(F.ChargedSpazerVerticalSpread, 1),
            Show(F.ChargedSpazerVerticalThickSpread, 1),
        ],
        [P.ChargedSpazerRisingDiagonal] =
        [
            Show(F.ChargedSpazerRisingDiagonalStartupShort, 1),
            Show(F.ChargedSpazerRisingDiagonalThickStartupShort, 1),
            Show(F.ChargedSpazerRisingDiagonalStartupLong, 1),
            Show(F.ChargedSpazerRisingDiagonalThickStartupLong, 1),
            Show(F.ChargedSpazerRisingDiagonalSeed, 1),
            Show(F.ChargedSpazerRisingDiagonalThickSeed, 1),
            Show(F.ChargedSpazerRisingDiagonalParting, 1),
            Show(F.ChargedSpazerRisingDiagonalThickParting, 1),
        ],
        [P.ChargedSpazerRisingDiagonalSpread] =
        [
            Show(F.ChargedSpazerRisingDiagonalSpread, 1),
            Show(F.ChargedSpazerRisingDiagonalThickSpread, 1),
        ],
        [P.ChargedSpazerHorizontal] =
        [
            Show(F.ChargedSpazerHorizontalStartupShort, 1),
            Show(F.ChargedSpazerHorizontalThickStartupShort, 1),
            Show(F.ChargedSpazerHorizontalStartupLong, 1),
            Show(F.ChargedSpazerHorizontalThickStartupLong, 1),
            Show(F.ChargedSpazerHorizontalSeed, 1),
            Show(F.ChargedSpazerHorizontalThickSeed, 1),
            Show(F.ChargedSpazerHorizontalParting, 1),
            Show(F.ChargedSpazerHorizontalThickParting, 1),
        ],
        [P.ChargedSpazerHorizontalSpread] =
        [
            Show(F.ChargedSpazerHorizontalSpread, 1),
            Show(F.ChargedSpazerHorizontalThickSpread, 1),
        ],
        [P.ChargedSpazerFallingDiagonal] =
        [
            Show(F.ChargedSpazerFallingDiagonalStartupShort, 1),
            Show(F.ChargedSpazerFallingDiagonalThickStartupShort, 1),
            Show(F.ChargedSpazerFallingDiagonalStartupLong, 1),
            Show(F.ChargedSpazerFallingDiagonalThickStartupLong, 1),
            Show(F.ChargedSpazerFallingDiagonalSeed, 1),
            Show(F.ChargedSpazerFallingDiagonalThickSeed, 1),
            Show(F.ChargedSpazerFallingDiagonalParting, 1),
            Show(F.ChargedSpazerFallingDiagonalThickParting, 1),
        ],
        [P.ChargedSpazerFallingDiagonalSpread] =
        [
            Show(F.ChargedSpazerFallingDiagonalSpread, 1),
            Show(F.ChargedSpazerFallingDiagonalThickSpread, 1),
        ],
        [P.ChargedSpazerWaveUp] =
        [
            Show(F.ChargedSpazerWaveUpStartupShort, 1),
            Show(F.ChargedSpazerWaveUpVerticalThickStartupShort, 1),
            Show(F.ChargedSpazerWaveUpStartupLong, 1),
            Show(F.ChargedSpazerWaveUpVerticalThickStartupLong, 1),
        ],
        [P.ChargedSpazerWaveUpWaving] =
        [
            Show(F.ChargedSpazerWaveUpSeed, 1),
            Show(F.ChargedSpazerWaveUpVerticalThickSeed, 1),
            Show(F.ChargedSpazerWaveUpSplitting, 1),
            Show(F.ChargedSpazerWaveUpVerticalThickSplitting, 1),
            Show(F.ChargedSpazerWaveUpParting, 1),
            Show(F.ChargedSpazerWaveUpVerticalThickParting, 1),
            Show(F.ChargedSpazerWaveUpSpreading, 1),
            Show(F.ChargedSpazerWaveUpVerticalThickSpreading, 1),
            Show(F.ChargedSpazerWaveUpNearlySpread, 1),
            Show(F.ChargedSpazerWaveUpVerticalThickNearlySpread, 1),
            Show(F.ChargedSpazerWaveUpSpread, 1),
            Show(F.ChargedSpazerWaveUpVerticalThickSpread, 1),
            Show(F.ChargedSpazerWaveUpNearlySpread, 1),
            Show(F.ChargedSpazerWaveUpVerticalThickNearlySpread, 1),
            Show(F.ChargedSpazerWaveUpSpreading, 1),
            Show(F.ChargedSpazerWaveUpVerticalThickSpreading, 1),
            Show(F.ChargedSpazerWaveUpParting, 1),
            Show(F.ChargedSpazerWaveUpVerticalThickParting, 1),
            Show(F.ChargedSpazerWaveUpSplittingNarrowHitboxTrailless, 1),
            Show(F.ChargedSpazerWaveUpVerticalThickSplittingNarrowHitboxTrailless, 1),
        ],
        [P.ChargedSpazerWaveUpRight] =
        [
            Show(F.ChargedSpazerWaveUpRightStartupShort, 1),
            Show(F.ChargedSpazerWaveUpRightThickStartupShort, 1),
            Show(F.ChargedSpazerWaveUpRightStartupLong, 1),
            Show(F.ChargedSpazerWaveUpRightThickStartupLong, 1),
        ],
        [P.ChargedSpazerWaveUpRightWaving] =
        [
            Show(F.ChargedSpazerWaveUpRightSeed, 1),
            Show(F.ChargedSpazerWaveUpRightThickSeed, 1),
            Show(F.ChargedSpazerWaveUpRightSplitting, 1),
            Show(F.ChargedSpazerWaveUpRightThickSplitting, 1),
            Show(F.ChargedSpazerWaveUpRightParting, 1),
            Show(F.ChargedSpazerWaveUpRightThickParting, 1),
            Show(F.ChargedSpazerWaveUpRightSpreading, 1),
            Show(F.ChargedSpazerWaveUpRightThickSpreading, 1),
            Show(F.ChargedSpazerWaveUpRightNearlySpread, 1),
            Show(F.ChargedSpazerWaveUpRightThickNearlySpread, 1),
            Show(F.ChargedSpazerWaveUpRightSpread, 1),
            Show(F.ChargedSpazerWaveUpRightThickSpread, 1),
            Show(F.ChargedSpazerWaveUpRightNearlySpread, 1),
            Show(F.ChargedSpazerWaveUpRightThickNearlySpread, 1),
            Show(F.ChargedSpazerWaveUpRightSpreading, 1),
            Show(F.ChargedSpazerWaveUpRightThickSpreading, 1),
            Show(F.ChargedSpazerWaveUpRightParting, 1),
            Show(F.ChargedSpazerWaveUpRightThickParting, 1),
            Show(F.ChargedSpazerWaveUpRightSplittingTrailless, 1),
            Show(F.ChargedSpazerWaveUpRightThickSplittingTrailless, 1),
        ],
        [P.ChargedSpazerWaveRight] =
        [
            Show(F.ChargedSpazerWaveRightStartupShort, 1),
            Show(F.ChargedSpazerWaveRightHorizontalThickStartupShort, 1),
            Show(F.ChargedSpazerWaveRightStartupLong, 1),
            Show(F.ChargedSpazerWaveRightHorizontalThickStartupLong, 1),
        ],
        [P.ChargedSpazerWaveRightWaving] =
        [
            Show(F.ChargedSpazerWaveRightSeed, 1),
            Show(F.ChargedSpazerWaveRightHorizontalThickSeed, 1),
            Show(F.ChargedSpazerWaveRightSplitting, 1),
            Show(F.ChargedSpazerWaveRightHorizontalThickSplitting, 1),
            Show(F.ChargedSpazerWaveRightParting, 1),
            Show(F.ChargedSpazerWaveRightHorizontalThickParting, 1),
            Show(F.ChargedSpazerWaveRightSpreading, 1),
            Show(F.ChargedSpazerWaveRightHorizontalThickSpreading, 1),
            Show(F.ChargedSpazerWaveRightNearlySpread, 1),
            Show(F.ChargedSpazerWaveRightHorizontalThickNearlySpread, 1),
            Show(F.ChargedSpazerWaveRightSpread, 1),
            Show(F.ChargedSpazerWaveRightHorizontalThickSpread, 1),
            Show(F.ChargedSpazerWaveRightNearlySpread, 1),
            Show(F.ChargedSpazerWaveRightHorizontalThickNearlySpread, 1),
            Show(F.ChargedSpazerWaveRightSpreading, 1),
            Show(F.ChargedSpazerWaveRightHorizontalThickSpreading, 1),
            Show(F.ChargedSpazerWaveRightParting, 1),
            Show(F.ChargedSpazerWaveRightHorizontalThickParting, 1),
            Show(F.ChargedSpazerWaveRightSplittingNarrowHitboxTrailless, 1),
            Show(F.ChargedSpazerWaveRightHorizontalThickSplittingNarrowHitboxTrailless, 1),
        ],
        [P.ChargedSpazerWaveDownRight] =
        [
            Show(F.ChargedSpazerWaveDownRightStartupShort, 1),
            Show(F.ChargedSpazerWaveDownRightThickStartupShort, 1),
            Show(F.ChargedSpazerWaveDownRightStartupLong, 1),
            Show(F.ChargedSpazerWaveDownRightThickStartupLong, 1),
        ],
        [P.ChargedSpazerWaveDownRightWaving] =
        [
            Show(F.ChargedSpazerWaveDownRightSeed, 1),
            Show(F.ChargedSpazerWaveDownRightThickSeed, 1),
            Show(F.ChargedSpazerWaveDownRightSplitting, 1),
            Show(F.ChargedSpazerWaveDownRightThickSplitting, 1),
            Show(F.ChargedSpazerWaveDownRightParting, 1),
            Show(F.ChargedSpazerWaveDownRightThickParting, 1),
            Show(F.ChargedSpazerWaveDownRightSpreading, 1),
            Show(F.ChargedSpazerWaveDownRightThickSpreading, 1),
            Show(F.ChargedSpazerWaveDownRightNearlySpread, 1),
            Show(F.ChargedSpazerWaveDownRightThickNearlySpread, 1),
            Show(F.ChargedSpazerWaveDownRightSpread, 1),
            Show(F.ChargedSpazerWaveDownRightThickSpread, 1),
            Show(F.ChargedSpazerWaveDownRightNearlySpread, 1),
            Show(F.ChargedSpazerWaveDownRightThickNearlySpread, 1),
            Show(F.ChargedSpazerWaveDownRightSpreading, 1),
            Show(F.ChargedSpazerWaveDownRightThickSpreading, 1),
            Show(F.ChargedSpazerWaveDownRightParting, 1),
            Show(F.ChargedSpazerWaveDownRightThickParting, 1),
            Show(F.ChargedSpazerWaveDownRightSplittingTrailless, 1),
            Show(F.ChargedSpazerWaveDownRightThickSplittingTrailless, 1),
        ],
        [P.ChargedSpazerWaveDown] =
        [
            Show(F.ChargedSpazerWaveDownStartupShort, 1),
            Show(F.ChargedSpazerWaveDownVerticalThickStartupShort, 1),
            Show(F.ChargedSpazerWaveDownStartupLong, 1),
            Show(F.ChargedSpazerWaveDownVerticalThickStartupLong, 1),
        ],
        [P.ChargedSpazerWaveDownWaving] =
        [
            Show(F.ChargedSpazerWaveDownSeed, 1),
            Show(F.ChargedSpazerWaveDownVerticalThickSeed, 1),
            Show(F.ChargedSpazerWaveDownSplitting, 1),
            Show(F.ChargedSpazerWaveDownVerticalThickSplitting, 1),
            Show(F.ChargedSpazerWaveDownParting, 1),
            Show(F.ChargedSpazerWaveDownVerticalThickParting, 1),
            Show(F.ChargedSpazerWaveDownSpreading, 1),
            Show(F.ChargedSpazerWaveDownVerticalThickSpreading, 1),
            Show(F.ChargedSpazerWaveDownNearlySpread, 1),
            Show(F.ChargedSpazerWaveDownVerticalThickNearlySpread, 1),
            Show(F.ChargedSpazerWaveDownSpread, 1),
            Show(F.ChargedSpazerWaveDownVerticalThickSpread, 1),
            Show(F.ChargedSpazerWaveDownNearlySpread, 1),
            Show(F.ChargedSpazerWaveDownVerticalThickNearlySpread, 1),
            Show(F.ChargedSpazerWaveDownSpreading, 1),
            Show(F.ChargedSpazerWaveDownVerticalThickSpreading, 1),
            Show(F.ChargedSpazerWaveDownParting, 1),
            Show(F.ChargedSpazerWaveDownVerticalThickParting, 1),
            Show(F.ChargedSpazerWaveDownSplittingNarrowHitboxTrailless, 1),
            Show(F.ChargedSpazerWaveDownVerticalThickSplittingNarrowHitboxTrailless, 1),
        ],
        [P.ChargedSpazerWaveDownLeft] =
        [
            Show(F.ChargedSpazerWaveDownLeftUpRightStartupShort, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightThickStartupShort, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightStartupLong, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightThickStartupLong, 1),
        ],
        [P.ChargedSpazerWaveDownLeftWaving] =
        [
            Show(F.ChargedSpazerWaveDownLeftSeed, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightThickSeed, 1),
            Show(F.ChargedSpazerWaveDownLeftSplitting, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightThickSplitting, 1),
            Show(F.ChargedSpazerWaveDownLeftParting, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightThickParting, 1),
            Show(F.ChargedSpazerWaveDownLeftSpreading, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightThickSpreading, 1),
            Show(F.ChargedSpazerWaveDownLeftNearlySpread, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightThickNearlySpread, 1),
            Show(F.ChargedSpazerWaveDownLeftSpread, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightThickSpread, 1),
            Show(F.ChargedSpazerWaveDownLeftNearlySpread, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightThickNearlySpread, 1),
            Show(F.ChargedSpazerWaveDownLeftSpreading, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightThickSpreading, 1),
            Show(F.ChargedSpazerWaveDownLeftParting, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightThickParting, 1),
            Show(F.ChargedSpazerWaveDownLeftSplittingTrailless, 1),
            Show(F.ChargedSpazerWaveDownLeftUpRightThickSplittingTrailless, 1),
        ],
        [P.ChargedSpazerWaveLeft] =
        [
            Show(F.ChargedSpazerWaveLeftStartupShort, 1),
            Show(F.ChargedSpazerWaveLeftHorizontalThickStartupShort, 1),
            Show(F.ChargedSpazerWaveLeftStartupLong, 1),
            Show(F.ChargedSpazerWaveLeftHorizontalThickStartupLong, 1),
        ],
        [P.ChargedSpazerWaveLeftWaving] =
        [
            Show(F.ChargedSpazerWaveLeftSeed, 1),
            Show(F.ChargedSpazerWaveLeftHorizontalThickSeed, 1),
            Show(F.ChargedSpazerWaveLeftSplitting, 1),
            Show(F.ChargedSpazerWaveLeftHorizontalThickSplitting, 1),
            Show(F.ChargedSpazerWaveLeftParting, 1),
            Show(F.ChargedSpazerWaveLeftHorizontalThickParting, 1),
            Show(F.ChargedSpazerWaveLeftSpreading, 1),
            Show(F.ChargedSpazerWaveLeftHorizontalThickSpreading, 1),
            Show(F.ChargedSpazerWaveLeftNearlySpread, 1),
            Show(F.ChargedSpazerWaveLeftHorizontalThickNearlySpread, 1),
            Show(F.ChargedSpazerWaveLeftSpread, 1),
            Show(F.ChargedSpazerWaveLeftHorizontalThickSpread, 1),
            Show(F.ChargedSpazerWaveLeftNearlySpread, 1),
            Show(F.ChargedSpazerWaveLeftHorizontalThickNearlySpread, 1),
            Show(F.ChargedSpazerWaveLeftSpreading, 1),
            Show(F.ChargedSpazerWaveLeftHorizontalThickSpreading, 1),
            Show(F.ChargedSpazerWaveLeftParting, 1),
            Show(F.ChargedSpazerWaveLeftHorizontalThickParting, 1),
            Show(F.ChargedSpazerWaveLeftSplittingNarrowHitboxTrailless, 1),
            Show(F.ChargedSpazerWaveLeftHorizontalThickSplittingNarrowHitboxTrailless, 1),
        ],
        [P.ChargedSpazerWaveUpLeft] =
        [
            Show(F.ChargedSpazerWaveUpLeftStartupShort, 1),
            Show(F.ChargedSpazerWaveUpLeftDownRightThickStartupShort, 1),
            Show(F.ChargedSpazerWaveUpLeftStartupLong, 1),
            Show(F.ChargedSpazerWaveUpLeftDownRightThickStartupLong, 1),
        ],
        [P.ChargedSpazerWaveUpLeftWaving] =
        [
            Show(F.ChargedSpazerWaveUpLeftSeed, 1),
            Show(F.ChargedSpazerWaveUpLeftDownRightThickSeed, 1),
            Show(F.ChargedSpazerWaveUpLeftSplitting, 1),
            Show(F.ChargedSpazerWaveUpLeftDownRightThickSplitting, 1),
            Show(F.ChargedSpazerWaveUpLeftParting, 1),
            Show(F.ChargedSpazerWaveUpLeftDownRightThickParting, 1),
            Show(F.ChargedSpazerWaveUpLeftSpreading, 1),
            Show(F.ChargedSpazerWaveUpLeftDownRightThickSpreading, 1),
            Show(F.ChargedSpazerWaveUpLeftNearlySpread, 1),
            Show(F.ChargedSpazerWaveUpLeftDownRightThickNearlySpread, 1),
            Show(F.ChargedSpazerWaveUpLeftSpread, 1),
            Show(F.ChargedSpazerWaveUpLeftDownRightThickSpread, 1),
            Show(F.ChargedSpazerWaveUpLeftNearlySpread, 1),
            Show(F.ChargedSpazerWaveUpLeftDownRightThickNearlySpread, 1),
            Show(F.ChargedSpazerWaveUpLeftSpreading, 1),
            Show(F.ChargedSpazerWaveUpLeftDownRightThickSpreading, 1),
            Show(F.ChargedSpazerWaveUpLeftParting, 1),
            Show(F.ChargedSpazerWaveUpLeftDownRightThickParting, 1),
            Show(F.ChargedSpazerWaveUpLeftSplittingTrailless, 1),
            Show(F.ChargedSpazerWaveUpLeftDownRightThickSplittingTrailless, 1),
        ],
        [P.ChargedPlasmaVertical] =
        [
            Show(F.ChargedPlasmaVerticalCore, 1),
            Show(F.ChargedPlasmaVerticalThickCore, 1),
            Show(F.ChargedPlasmaVerticalShort, 1),
            Show(F.ChargedPlasmaVerticalThickShort, 1),
            Show(F.ChargedPlasmaVerticalLong, 1),
            Show(F.ChargedPlasmaVerticalThickLong, 1),
        ],
        [P.ChargedPlasmaVerticalFullLength] =
        [
            Show(F.ChargedPlasmaVerticalFull, 1),
            Show(F.ChargedPlasmaVerticalThickFull, 1),
        ],
        [P.ChargedPlasmaRisingDiagonal] =
        [
            Show(F.ChargedPlasmaRisingDiagonalCore, 1),
            Show(F.ChargedPlasmaRisingDiagonalThickCore, 1),
            Show(F.ChargedPlasmaRisingDiagonalShort, 1),
            Show(F.ChargedPlasmaRisingDiagonalThickShort, 1),
            Show(F.ChargedPlasmaRisingDiagonalLong, 1),
            Show(F.ChargedPlasmaRisingDiagonalThickLong, 1),
        ],
        [P.ChargedPlasmaRisingDiagonalFullLength] =
        [
            Show(F.ChargedPlasmaRisingDiagonalFull, 1),
            Show(F.ChargedPlasmaRisingDiagonalThickFull, 1),
        ],
        [P.ChargedPlasmaHorizontal] =
        [
            Show(F.ChargedPlasmaHorizontalCore, 1),
            Show(F.ChargedPlasmaHorizontalThickCore, 1),
            Show(F.ChargedPlasmaHorizontalShort, 1),
            Show(F.ChargedPlasmaHorizontalThickShort, 1),
            Show(F.ChargedPlasmaHorizontalLong, 1),
            Show(F.ChargedPlasmaHorizontalThickLong, 1),
        ],
        [P.ChargedPlasmaHorizontalFullLength] =
        [
            Show(F.ChargedPlasmaHorizontalFull, 1),
            Show(F.ChargedPlasmaHorizontalThickFull, 1),
        ],
        [P.ChargedPlasmaFallingDiagonal] =
        [
            Show(F.ChargedPlasmaFallingDiagonalCore, 1),
            Show(F.ChargedPlasmaFallingDiagonalThickCore, 1),
            Show(F.ChargedPlasmaFallingDiagonalShort, 1),
            Show(F.ChargedPlasmaFallingDiagonalThickShort, 1),
            Show(F.ChargedPlasmaFallingDiagonalLong, 1),
            Show(F.ChargedPlasmaFallingDiagonalThickLong, 1),
        ],
        [P.ChargedPlasmaFallingDiagonalFullLength] =
        [
            Show(F.ChargedPlasmaFallingDiagonalFull, 1),
            Show(F.ChargedPlasmaFallingDiagonalThickFull, 1),
        ],
        [P.ChargedPlasmaWaveVertical] =
        [
            Show(F.ChargedPlasmaWaveVerticalCore, 1),
            Show(F.ChargedPlasmaWaveVerticalThickCore, 1),
            Show(F.ChargedPlasmaWaveVerticalShort, 1),
            Show(F.ChargedPlasmaWaveVerticalThickShort, 1),
            Show(F.ChargedPlasmaWaveVerticalLong, 1),
            Show(F.ChargedPlasmaWaveVerticalThickLong, 1),
        ],
        [P.ChargedPlasmaWaveVerticalWaving] =
        [
            Show(F.ChargedPlasmaWaveVerticalLongCore, 1),
            Show(F.ChargedPlasmaWaveVerticalThickLongCore, 1),
            Show(F.ChargedPlasmaWaveVerticalLongLobesNear, 1),
            Show(F.ChargedPlasmaWaveVerticalThickLobesNear, 1),
            Show(F.ChargedPlasmaWaveVerticalLongLobesMidway, 1),
            Show(F.ChargedPlasmaWaveVerticalThickLobesMidway, 1),
            Show(F.ChargedPlasmaWaveVerticalLongLobesFar, 1),
            Show(F.ChargedPlasmaWaveVerticalThickLobesFar, 1),
            Show(F.ChargedPlasmaWaveVerticalLongLobesPeak, 1),
            Show(F.ChargedPlasmaWaveVerticalThickLobesPeak, 1),
            Show(F.ChargedPlasmaWaveVerticalLongLobesFar, 1),
            Show(F.ChargedPlasmaWaveVerticalThickLobesFar, 1),
            Show(F.ChargedPlasmaWaveVerticalLongLobesMidway, 1),
            Show(F.ChargedPlasmaWaveVerticalThickLobesMidway, 1),
            Show(F.ChargedPlasmaWaveVerticalLongLobesNear, 1),
            Show(F.ChargedPlasmaWaveVerticalThickLobesNear, 1),
        ],
        [P.ChargedPlasmaWaveRisingDiagonal] =
        [
            Show(F.ChargedPlasmaWaveRisingDiagonalCore, 1),
            Show(F.ChargedPlasmaWaveRisingDiagonalThickCore, 1),
            Show(F.ChargedPlasmaWaveRisingDiagonalShort, 1),
            Show(F.ChargedPlasmaWaveRisingDiagonalThickShort, 1),
            Show(F.ChargedPlasmaWaveRisingDiagonalLong, 1),
            Show(F.ChargedPlasmaWaveRisingDiagonalThickLong, 1),
        ],
        [P.ChargedPlasmaWaveRisingDiagonalWaving] =
        [
            Show(F.ChargedPlasmaWaveRisingDiagonalLongCore, 1),
            Show(F.ChargedPlasmaWaveRisingDiagonalThickLongCore, 1),
            Show(F.ChargedPlasmaWaveRisingDiagonalLongLobesNear, 1),
            Show(F.ChargedPlasmaWaveRisingDiagonalThickLobesNear, 1),
            Show(F.ChargedPlasmaWaveRisingDiagonalLongLobesMidway, 1),
            Show(F.ChargedPlasmaWaveRisingDiagonalThickLobesMidway, 1),
            Show(F.ChargedPlasmaWaveRisingDiagonalLongLobesFar, 1),
            Show(F.ChargedPlasmaWaveRisingDiagonalThickLobesFar, 1),
            Show(F.ChargedPlasmaWaveRisingDiagonalLongLobesPeak, 1),
            Show(F.ChargedPlasmaWaveRisingDiagonalThickLobesPeak, 1),
            Show(F.ChargedPlasmaWaveRisingDiagonalLongLobesFar, 1),
            Show(F.ChargedPlasmaWaveRisingDiagonalThickLobesFar, 1),
            Show(F.ChargedPlasmaWaveRisingDiagonalLongLobesMidway, 1),
            Show(F.ChargedPlasmaWaveRisingDiagonalThickLobesMidway, 1),
            Show(F.ChargedPlasmaWaveRisingDiagonalLongLobesNear, 1),
            Show(F.ChargedPlasmaWaveRisingDiagonalThickLobesNear, 1),
        ],
        [P.ChargedPlasmaWaveHorizontal] =
        [
            Show(F.ChargedPlasmaWaveHorizontalCore, 1),
            Show(F.ChargedPlasmaWaveHorizontalThickCore, 1),
            Show(F.ChargedPlasmaWaveHorizontalShort, 1),
            Show(F.ChargedPlasmaWaveHorizontalThickShort, 1),
            Show(F.ChargedPlasmaWaveHorizontalLong, 1),
            Show(F.ChargedPlasmaWaveHorizontalThickLong, 1),
        ],
        [P.ChargedPlasmaWaveHorizontalWaving] =
        [
            Show(F.ChargedPlasmaWaveHorizontalLongCore, 1),
            Show(F.ChargedPlasmaWaveHorizontalThickLongCore, 1),
            Show(F.ChargedPlasmaWaveHorizontalLongLobesNear, 1),
            Show(F.ChargedPlasmaWaveHorizontalThickLobesNear, 1),
            Show(F.ChargedPlasmaWaveHorizontalLongLobesMidway, 1),
            Show(F.ChargedPlasmaWaveHorizontalThickLobesMidway, 1),
            Show(F.ChargedPlasmaWaveHorizontalLongLobesFar, 1),
            Show(F.ChargedPlasmaWaveHorizontalThickLobesFar, 1),
            Show(F.ChargedPlasmaWaveHorizontalLongLobesPeak, 1),
            Show(F.ChargedPlasmaWaveHorizontalThickLobesPeak, 1),
            Show(F.ChargedPlasmaWaveHorizontalLongLobesFar, 1),
            Show(F.ChargedPlasmaWaveHorizontalThickLobesFar, 1),
            Show(F.ChargedPlasmaWaveHorizontalLongLobesMidway, 1),
            Show(F.ChargedPlasmaWaveHorizontalThickLobesMidway, 1),
            Show(F.ChargedPlasmaWaveHorizontalLongLobesNear, 1),
            Show(F.ChargedPlasmaWaveHorizontalThickLobesNear, 1),
        ],
        [P.ChargedPlasmaWaveFallingDiagonal] =
        [
            Show(F.ChargedPlasmaWaveFallingDiagonalCore, 1),
            Show(F.ChargedPlasmaWaveFallingDiagonalThickCore, 1),
            Show(F.ChargedPlasmaWaveFallingDiagonalShort, 1),
            Show(F.ChargedPlasmaWaveFallingDiagonalThickShort, 1),
            Show(F.ChargedPlasmaWaveFallingDiagonalLong, 1),
            Show(F.ChargedPlasmaWaveFallingDiagonalThickLong, 1),
        ],
        [P.ChargedPlasmaWaveFallingDiagonalWaving] =
        [
            Show(F.ChargedPlasmaWaveFallingDiagonalLongCore, 1),
            Show(F.ChargedPlasmaWaveFallingDiagonalThickLongCore, 1),
            Show(F.ChargedPlasmaWaveFallingDiagonalLongLobesNear, 1),
            Show(F.ChargedPlasmaWaveFallingDiagonalThickLobesNear, 1),
            Show(F.ChargedPlasmaWaveFallingDiagonalLongLobesMidway, 1),
            Show(F.ChargedPlasmaWaveFallingDiagonalThickLobesMidway, 1),
            Show(F.ChargedPlasmaWaveFallingDiagonalLongLobesFar, 1),
            Show(F.ChargedPlasmaWaveFallingDiagonalThickLobesFar, 1),
            Show(F.ChargedPlasmaWaveFallingDiagonalLongLobesPeak, 1),
            Show(F.ChargedPlasmaWaveFallingDiagonalThickLobesPeak, 1),
            Show(F.ChargedPlasmaWaveFallingDiagonalLongLobesFar, 1),
            Show(F.ChargedPlasmaWaveFallingDiagonalThickLobesFar, 1),
            Show(F.ChargedPlasmaWaveFallingDiagonalLongLobesMidway, 1),
            Show(F.ChargedPlasmaWaveFallingDiagonalThickLobesMidway, 1),
            Show(F.ChargedPlasmaWaveFallingDiagonalLongLobesNear, 1),
            Show(F.ChargedPlasmaWaveFallingDiagonalThickLobesNear, 1),
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
            Show(F.WaveSpecialAttackCoreStarburst, 8),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static Dictionary<SamusProjectileSegment, Animation<SamusProjectileSegment>> StartingAt { get; } = new Dictionary<SamusProjectileSegment, Animation<SamusProjectileSegment>>
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
        Animation<SamusProjectileSegment>.Looping([Segments.Whole(segment)]);

    /// <summary>An animation that plays <paramref name="opening"/> once, then plays <paramref name="repeated"/> over and over.</summary>
    private static Animation<SamusProjectileSegment> OpeningThenRepeating(SamusProjectileSegment opening, SamusProjectileSegment repeated) =>
        Animation<SamusProjectileSegment>.LeadingInto([Segments.Whole(opening)], [Segments.Whole(repeated)]);

    /// <summary>An animation that plays <paramref name="segment"/> once and is over: an explosion.</summary>
    private static Animation<SamusProjectileSegment> PlayedOnce(SamusProjectileSegment segment) =>
        new([Segments.Whole(segment)], AnimationEnd.Stops);
}
