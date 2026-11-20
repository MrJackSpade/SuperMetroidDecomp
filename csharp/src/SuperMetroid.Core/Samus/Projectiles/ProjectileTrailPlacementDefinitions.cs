using System.Collections.Frozen;
using SuperMetroid.Core.Progression;
using F = SuperMetroid.Core.Samus.Projectiles.SamusProjectileFrameId;
using L = SuperMetroid.Core.Samus.Projectiles.ProjectileTrailPlacementList;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Where projectile trails start: the family each group gives a beam combination, the list each family gives a
/// direction, and each list's placements by the frame the shot shows.
/// </summary>
[ArchitectureReview("large", "Data only: every trail list's placements, each stated once with the frames a shot selecting that list shows it for (#2124, #2557), with the family and list choices; the trail spawner reads it as one table, and splitting it by family would only scatter the lists a shot's family and direction choose between.")]
internal static class ProjectileTrailPlacementDefinitions
{
    /// <summary>The wave centre frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] WaveCentreFrames =
    [
        F.WaveUpHidden, F.WaveVerticalCentre, F.WaveRisingDiagonalCentre, F.WaveRisingDiagonalCentreNarrowHitbox,
        F.WaveHorizontalCentre, F.WaveFallingDiagonalCentre, F.WaveFallingDiagonalCentreNarrowHitbox,
    ];

    /// <summary>The wave near lobe frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] WaveNearLobeFrames =
    [
        F.WaveVerticalRightNear, F.WaveRisingDiagonalUpLeftNear, F.WaveRisingDiagonalUpLeftNearNarrowHitbox,
        F.WaveHorizontalUpNear, F.WaveFallingDiagonalUpRightNear, F.WaveFallingDiagonalUpRightNearNarrowHitbox,
    ];

    /// <summary>The wave midway lobe frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] WaveMidwayLobeFrames =
    [
        F.WaveVerticalRightMidway, F.WaveRisingDiagonalUpLeftMidway, F.WaveHorizontalUpMidway,
        F.WaveFallingDiagonalUpRightMidway,
    ];

    /// <summary>The wave far lobe frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] WaveFarLobeFrames =
    [
        F.WaveVerticalRightFar, F.WaveRisingDiagonalUpLeftFar, F.WaveHorizontalUpFar, F.WaveFallingDiagonalUpRightFar,
    ];

    /// <summary>The wave peak lobe frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] WavePeakLobeFrames =
    [
        F.WaveVerticalRightPeak, F.WaveRisingDiagonalUpLeftPeak, F.WaveHorizontalUpPeak,
        F.WaveFallingDiagonalUpRightPeak,
    ];

    /// <summary>The wave near lobe opposite frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] WaveNearLobeOppositeFrames =
    [
        F.WaveVerticalLeftNear, F.WaveRisingDiagonalDownRightNear, F.WaveRisingDiagonalDownRightNearWideHitbox,
        F.WaveHorizontalDownNear, F.WaveFallingDiagonalDownLeftNear, F.WaveFallingDiagonalDownLeftNearWideHitbox,
    ];

    /// <summary>The wave midway lobe opposite frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] WaveMidwayLobeOppositeFrames =
    [
        F.WaveVerticalLeftMidway, F.WaveRisingDiagonalDownRightMidway, F.WaveHorizontalDownMidway,
        F.WaveFallingDiagonalDownLeftMidway,
    ];

    /// <summary>The wave far lobe opposite frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] WaveFarLobeOppositeFrames =
    [
        F.WaveVerticalLeftFar, F.WaveRisingDiagonalDownRightFar, F.WaveHorizontalDownFar,
        F.WaveFallingDiagonalDownLeftFar,
    ];

    /// <summary>The wave peak lobe opposite frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] WavePeakLobeOppositeFrames =
    [
        F.WaveVerticalLeftPeak, F.WaveRisingDiagonalDownRightPeak, F.WaveHorizontalDownPeak,
        F.WaveFallingDiagonalDownLeftPeak,
    ];

    /// <summary>The spazer seed frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] SpazerSeedFrames =
    [
        F.SpazerUpSeed, F.SpazerUpRightSeed, F.SpazerRightSeed, F.SpazerDownRightSeed, F.SpazerDownSeed,
        F.SpazerDownLeftSeed, F.SpazerLeftSeed, F.SpazerUpLeftSeed,
    ];

    /// <summary>The spazer parting frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] SpazerPartingFrames =
    [
        F.SpazerUpParting, F.SpazerUpRightParting, F.SpazerRightParting, F.SpazerDownRightParting, F.SpazerDownParting,
        F.SpazerDownLeftParting, F.SpazerLeftParting, F.SpazerUpLeftParting,
    ];

    /// <summary>The spazer spread frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] SpazerSpreadFrames =
    [
        F.SpazerUpSpread, F.SpazerUpRightSpread, F.SpazerRightSpread, F.SpazerDownRightSpread, F.SpazerDownSpread,
        F.SpazerDownLeftSpread, F.SpazerLeftSpread, F.SpazerUpLeftSpread,
    ];

    /// <summary>The spazer wave seed frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] SpazerWaveSeedFrames =
    [
        F.SpazerWaveUpSeed, F.SpazerWaveUpRightSeed, F.SpazerWaveRightSeed, F.SpazerWaveDownRightSeed,
        F.SpazerWaveDownSeed, F.SpazerWaveDownLeftSeed, F.SpazerWaveLeftSeed, F.SpazerWaveUpLeftSeed,
    ];

    /// <summary>The spazer wave splitting frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] SpazerWaveSplittingFrames =
    [
        F.SpazerWaveUpSplitting, F.SpazerWaveUpSplittingWithoutDefaultTrail, F.SpazerWaveUpRightSplitting,
        F.SpazerWaveUpRightSplittingWithoutDefaultTrail, F.SpazerWaveRightSplitting,
        F.SpazerWaveRightSplittingWithoutDefaultTrail, F.SpazerWaveDownRightSplitting,
        F.SpazerWaveDownRightSplittingWithoutDefaultTrail, F.SpazerWaveDownSplitting,
        F.SpazerWaveDownSplittingWithoutDefaultTrail, F.SpazerWaveLeftSplitting,
        F.SpazerWaveLeftSplittingWithoutDefaultTrail, F.SpazerWaveUpLeftSplitting,
        F.SpazerWaveUpLeftSplittingWithoutDefaultTrail,
    ];

    /// <summary>The spazer wave parting frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] SpazerWavePartingFrames =
    [
        F.SpazerWaveUpParting, F.SpazerWaveUpPartingWithoutDefaultTrail, F.SpazerWaveUpRightParting,
        F.SpazerWaveUpRightPartingWithoutDefaultTrail, F.SpazerWaveRightParting,
        F.SpazerWaveRightPartingWithoutDefaultTrail, F.SpazerWaveDownRightParting,
        F.SpazerWaveDownRightPartingWithoutDefaultTrail, F.SpazerWaveDownParting,
        F.SpazerWaveDownPartingWithoutDefaultTrail, F.SpazerWaveDownLeftParting,
        F.SpazerWaveDownLeftPartingWithoutDefaultTrail, F.SpazerWaveLeftParting,
        F.SpazerWaveLeftPartingWithoutDefaultTrail, F.SpazerWaveUpLeftParting,
        F.SpazerWaveUpLeftPartingWithoutDefaultTrail,
    ];

    /// <summary>The spazer wave spreading frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] SpazerWaveSpreadingFrames =
    [
        F.SpazerWaveUpSpreading, F.SpazerWaveUpRightSpreading, F.SpazerWaveRightSpreading,
        F.SpazerWaveDownRightSpreading, F.SpazerWaveDownSpreading, F.SpazerWaveDownLeftSpreading,
        F.SpazerWaveDownLeftSpreadingWithoutDefaultTrail, F.SpazerWaveLeftSpreading, F.SpazerWaveUpLeftSpreading,
    ];

    /// <summary>The spazer wave nearly spread frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] SpazerWaveNearlySpreadFrames =
    [
        F.SpazerWaveUpNearlySpread, F.SpazerWaveUpRightNearlySpread, F.SpazerWaveRightNearlySpread,
        F.SpazerWaveDownRightNearlySpread, F.SpazerWaveDownNearlySpread, F.SpazerWaveDownLeftNearlySpread,
        F.SpazerWaveLeftNearlySpread, F.SpazerWaveUpLeftNearlySpread,
    ];

    /// <summary>The spazer wave spread frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] SpazerWaveSpreadFrames =
    [
        F.SpazerWaveUpSpread, F.SpazerWaveUpRightSpread, F.SpazerWaveRightSpread, F.SpazerWaveDownRightSpread,
        F.SpazerWaveDownSpread, F.SpazerWaveDownLeftSpread, F.SpazerWaveDownLeftSpreadShiftedTrail,
        F.SpazerWaveLeftSpread, F.SpazerWaveUpLeftSpread,
    ];

    /// <summary>The plasma core frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] PlasmaCoreFrames =
    [
        F.PlasmaVerticalCore, F.PlasmaRisingDiagonalCore, F.PlasmaHorizontalCore, F.PlasmaFallingDiagonalCore,
    ];

    /// <summary>The plasma full length frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] PlasmaFullLengthFrames =
    [
        F.PlasmaVerticalFullLength, F.PlasmaRisingDiagonalFullLength, F.PlasmaHorizontalFullLength,
        F.PlasmaFallingDiagonalFullLength,
    ];

    /// <summary>The plasma ice wave core frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] PlasmaIceWaveCoreFrames =
    [
        F.PlasmaIceWaveVerticalCore, F.PlasmaIceWaveRisingDiagonalCore, F.PlasmaWaveHorizontalCore,
        F.PlasmaIceWaveFallingDiagonalCore,
    ];

    /// <summary>The plasma wave short core frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] PlasmaWaveShortCoreFrames =
    [
        F.PlasmaWaveVerticalShortCore, F.PlasmaWaveRisingDiagonalShortCore, F.PlasmaWaveHorizontalShortCore,
        F.PlasmaWaveFallingDiagonalShortCore,
    ];

    /// <summary>The plasma wave short lobes near frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] PlasmaWaveShortLobesNearFrames =
    [
        F.PlasmaWaveVerticalShortLobesNear, F.PlasmaWaveVerticalShortLobesNearWithoutDefaultTrail,
        F.PlasmaWaveRisingDiagonalShortLobesNear, F.PlasmaWaveRisingDiagonalShortLobesNearWithoutDefaultTrail,
        F.PlasmaWaveHorizontalShortLobesNear, F.PlasmaWaveHorizontalShortLobesNearWithoutDefaultTrail,
        F.PlasmaWaveFallingDiagonalShortLobesNear, F.PlasmaWaveFallingDiagonalShortLobesNearWithoutDefaultTrail,
    ];

    /// <summary>The plasma wave short lobes midway frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] PlasmaWaveShortLobesMidwayFrames =
    [
        F.PlasmaWaveVerticalShortLobesMidway, F.PlasmaWaveRisingDiagonalShortLobesMidway,
        F.PlasmaWaveHorizontalShortLobesMidway, F.PlasmaWaveFallingDiagonalShortLobesMidway,
    ];

    /// <summary>The plasma wave short lobes far and peak frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] PlasmaWaveShortLobesFarAndPeakFrames =
    [
        F.PlasmaWaveVerticalShortLobesFar, F.PlasmaWaveVerticalShortLobesPeak, F.PlasmaWaveRisingDiagonalShortLobesFar,
        F.PlasmaWaveRisingDiagonalShortLobesPeak, F.PlasmaWaveHorizontalShortLobesFar,
        F.PlasmaWaveHorizontalShortLobesPeak, F.PlasmaWaveFallingDiagonalShortLobesFar,
        F.PlasmaWaveFallingDiagonalShortLobesPeak,
    ];

    /// <summary>The charged wave core frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] ChargedWaveCoreFrames =
    [
        F.ChargedWaveUpHidden, F.ChargedWaveVerticalCore, F.ChargedWaveVerticalCoreStarburst,
        F.ChargedWaveRisingDiagonalCore, F.ChargedWaveRisingDiagonalCoreStarburst, F.ChargedWaveHorizontalCore,
        F.ChargedWaveHorizontalCoreStarburst, F.ChargedWaveFallingDiagonalCore,
        F.ChargedWaveFallingDiagonalCoreStarburst, F.ChargedIceWaveUpHidden, F.ChargedIceWaveVerticalCore,
        F.ChargedIceWaveVerticalCoreStarburst, F.ChargedIceWaveRisingDiagonalCore,
        F.ChargedIceWaveRisingDiagonalCoreStarburst, F.ChargedIceWaveHorizontalCore,
        F.ChargedIceWaveHorizontalCoreStarburst, F.ChargedIceWaveFallingDiagonalCore,
        F.ChargedIceWaveFallingDiagonalCoreStarburst, F.WaveSpecialAttackCore, F.WaveSpecialAttackCoreStarburst,
    ];

    /// <summary>The charged wave lobes near frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] ChargedWaveLobesNearFrames =
    [
        F.ChargedWaveVerticalLobesNear, F.ChargedWaveVerticalLobesNearOrbRight, F.ChargedWaveRisingDiagonalLobesNear,
        F.ChargedWaveRisingDiagonalLobesNearOrbBelow, F.ChargedWaveHorizontalLobesNear,
        F.ChargedWaveHorizontalLobesNearOrbBelow, F.ChargedWaveFallingDiagonalLobesNear,
        F.ChargedWaveFallingDiagonalLobesNearOrbBelow, F.ChargedIceWaveVerticalLobesNear,
        F.ChargedIceWaveVerticalLobesNearOrbRight, F.ChargedIceWaveRisingDiagonalLobesNear,
        F.ChargedIceWaveRisingDiagonalLobesNearOrbBelow, F.ChargedIceWaveHorizontalLobesNear,
        F.ChargedIceWaveHorizontalLobesNearOrbBelow, F.ChargedIceWaveFallingDiagonalLobesNear,
        F.ChargedIceWaveFallingDiagonalLobesNearOrbBelow,
    ];

    /// <summary>The charged wave lobes midway frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] ChargedWaveLobesMidwayFrames =
    [
        F.ChargedWaveVerticalLobesMidway, F.ChargedWaveVerticalLobesMidwayOrbRight,
        F.ChargedWaveRisingDiagonalLobesMidway, F.ChargedWaveRisingDiagonalLobesMidwayOrbBelow,
        F.ChargedWaveHorizontalLobesMidway, F.ChargedWaveHorizontalLobesMidwayOrbBelow,
        F.ChargedWaveFallingDiagonalLobesMidway, F.ChargedWaveFallingDiagonalLobesMidwayOrbBelow,
        F.ChargedIceWaveVerticalLobesMidway, F.ChargedIceWaveVerticalLobesMidwayOrbRight,
        F.ChargedIceWaveRisingDiagonalLobesMidway, F.ChargedIceWaveRisingDiagonalLobesMidwayOrbBelow,
        F.ChargedIceWaveHorizontalLobesMidway, F.ChargedIceWaveHorizontalLobesMidwayOrbBelow,
        F.ChargedIceWaveFallingDiagonalLobesMidway, F.ChargedIceWaveFallingDiagonalLobesMidwayOrbBelow,
    ];

    /// <summary>The charged wave lobes far and peak frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] ChargedWaveLobesFarAndPeakFrames =
    [
        F.ChargedWaveVerticalLobesFar, F.ChargedWaveVerticalLobesFarOrbRight, F.ChargedWaveVerticalLobesPeak,
        F.ChargedWaveVerticalLobesPeakOrbRight, F.ChargedWaveRisingDiagonalLobesFar,
        F.ChargedWaveRisingDiagonalLobesFarOrbBelow, F.ChargedWaveRisingDiagonalLobesPeak,
        F.ChargedWaveRisingDiagonalLobesPeakOrbBelow, F.ChargedWaveHorizontalLobesFar,
        F.ChargedWaveHorizontalLobesFarOrbBelow, F.ChargedWaveHorizontalLobesPeak,
        F.ChargedWaveHorizontalLobesPeakOrbBelow, F.ChargedWaveFallingDiagonalLobesFar,
        F.ChargedWaveFallingDiagonalLobesFarOrbBelow, F.ChargedWaveFallingDiagonalLobesPeak,
        F.ChargedWaveFallingDiagonalLobesPeakOrbBelow, F.ChargedIceWaveVerticalLobesFar,
        F.ChargedIceWaveVerticalLobesFarOrbRight, F.ChargedIceWaveVerticalLobesPeak,
        F.ChargedIceWaveVerticalLobesPeakOrbRight, F.ChargedIceWaveRisingDiagonalLobesFar,
        F.ChargedIceWaveRisingDiagonalLobesFarOrbBelow, F.ChargedIceWaveRisingDiagonalLobesPeak,
        F.ChargedIceWaveRisingDiagonalLobesPeakOrbBelow, F.ChargedIceWaveHorizontalLobesFar,
        F.ChargedIceWaveHorizontalLobesFarOrbBelow, F.ChargedIceWaveHorizontalLobesPeak,
        F.ChargedIceWaveHorizontalLobesPeakOrbBelow, F.ChargedIceWaveFallingDiagonalLobesFar,
        F.ChargedIceWaveFallingDiagonalLobesFarOrbBelow, F.ChargedIceWaveFallingDiagonalLobesPeak,
        F.ChargedIceWaveFallingDiagonalLobesPeakOrbBelow,
    ];

    /// <summary>The charged wave lobes midway and far frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] ChargedWaveLobesMidwayAndFarFrames =
    [
        F.ChargedWaveVerticalLobesMidway, F.ChargedWaveVerticalLobesMidwayOrbRight, F.ChargedWaveVerticalLobesFar,
        F.ChargedWaveVerticalLobesFarOrbRight, F.ChargedWaveRisingDiagonalLobesMidway,
        F.ChargedWaveRisingDiagonalLobesMidwayOrbBelow, F.ChargedWaveRisingDiagonalLobesFar,
        F.ChargedWaveRisingDiagonalLobesFarOrbBelow, F.ChargedWaveHorizontalLobesMidway,
        F.ChargedWaveHorizontalLobesMidwayOrbBelow, F.ChargedWaveHorizontalLobesFar,
        F.ChargedWaveHorizontalLobesFarOrbBelow, F.ChargedWaveFallingDiagonalLobesMidway,
        F.ChargedWaveFallingDiagonalLobesMidwayOrbBelow, F.ChargedWaveFallingDiagonalLobesFar,
        F.ChargedWaveFallingDiagonalLobesFarOrbBelow, F.ChargedIceWaveVerticalLobesMidway,
        F.ChargedIceWaveVerticalLobesMidwayOrbRight, F.ChargedIceWaveVerticalLobesFar,
        F.ChargedIceWaveVerticalLobesFarOrbRight, F.ChargedIceWaveRisingDiagonalLobesMidway,
        F.ChargedIceWaveRisingDiagonalLobesMidwayOrbBelow, F.ChargedIceWaveRisingDiagonalLobesFar,
        F.ChargedIceWaveRisingDiagonalLobesFarOrbBelow, F.ChargedIceWaveHorizontalLobesMidway,
        F.ChargedIceWaveHorizontalLobesMidwayOrbBelow, F.ChargedIceWaveHorizontalLobesFar,
        F.ChargedIceWaveHorizontalLobesFarOrbBelow, F.ChargedIceWaveFallingDiagonalLobesMidway,
        F.ChargedIceWaveFallingDiagonalLobesMidwayOrbBelow, F.ChargedIceWaveFallingDiagonalLobesFar,
        F.ChargedIceWaveFallingDiagonalLobesFarOrbBelow,
    ];

    /// <summary>The charged wave lobes peak frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] ChargedWaveLobesPeakFrames =
    [
        F.ChargedWaveVerticalLobesPeak, F.ChargedWaveVerticalLobesPeakOrbRight, F.ChargedWaveRisingDiagonalLobesPeak,
        F.ChargedWaveRisingDiagonalLobesPeakOrbBelow, F.ChargedWaveHorizontalLobesPeak,
        F.ChargedWaveHorizontalLobesPeakOrbBelow, F.ChargedWaveFallingDiagonalLobesPeak,
        F.ChargedWaveFallingDiagonalLobesPeakOrbBelow, F.ChargedIceWaveVerticalLobesPeak,
        F.ChargedIceWaveVerticalLobesPeakOrbRight, F.ChargedIceWaveRisingDiagonalLobesPeak,
        F.ChargedIceWaveRisingDiagonalLobesPeakOrbBelow, F.ChargedIceWaveHorizontalLobesPeak,
        F.ChargedIceWaveHorizontalLobesPeakOrbBelow, F.ChargedIceWaveFallingDiagonalLobesPeak,
        F.ChargedIceWaveFallingDiagonalLobesPeakOrbBelow,
    ];

    /// <summary>The charged spazer startup short frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] ChargedSpazerStartupShortFrames =
    [
        F.ChargedSpazerVerticalStartupShort, F.ChargedSpazerVerticalThickStartupShort,
        F.ChargedSpazerRisingDiagonalStartupShort, F.ChargedSpazerRisingDiagonalThickStartupShort,
        F.ChargedSpazerHorizontalStartupShort, F.ChargedSpazerHorizontalThickStartupShort,
        F.ChargedSpazerFallingDiagonalStartupShort, F.ChargedSpazerFallingDiagonalThickStartupShort,
    ];

    /// <summary>The charged spazer startup long frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] ChargedSpazerStartupLongFrames =
    [
        F.ChargedSpazerVerticalStartupLong, F.ChargedSpazerVerticalThickStartupLong,
        F.ChargedSpazerRisingDiagonalStartupLong, F.ChargedSpazerRisingDiagonalThickStartupLong,
        F.ChargedSpazerHorizontalStartupLong, F.ChargedSpazerHorizontalThickStartupLong,
        F.ChargedSpazerFallingDiagonalStartupLong, F.ChargedSpazerFallingDiagonalThickStartupLong,
    ];

    /// <summary>The charged spazer seed frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] ChargedSpazerSeedFrames =
    [
        F.ChargedSpazerVerticalSeed, F.ChargedSpazerVerticalThickSeed, F.ChargedSpazerRisingDiagonalSeed,
        F.ChargedSpazerRisingDiagonalThickSeed, F.ChargedSpazerHorizontalSeed, F.ChargedSpazerHorizontalThickSeed,
        F.ChargedSpazerFallingDiagonalSeed, F.ChargedSpazerFallingDiagonalThickSeed,
    ];

    /// <summary>The charged spazer parting frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] ChargedSpazerPartingFrames =
    [
        F.ChargedSpazerVerticalParting, F.ChargedSpazerVerticalThickParting, F.ChargedSpazerRisingDiagonalParting,
        F.ChargedSpazerRisingDiagonalThickParting, F.ChargedSpazerHorizontalParting,
        F.ChargedSpazerHorizontalThickParting, F.ChargedSpazerFallingDiagonalParting,
        F.ChargedSpazerFallingDiagonalThickParting,
    ];

    /// <summary>The charged spazer spread frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] ChargedSpazerSpreadFrames =
    [
        F.ChargedSpazerVerticalSpread, F.ChargedSpazerVerticalThickSpread, F.ChargedSpazerRisingDiagonalSpread,
        F.ChargedSpazerRisingDiagonalThickSpread, F.ChargedSpazerHorizontalSpread, F.ChargedSpazerHorizontalThickSpread,
        F.ChargedSpazerFallingDiagonalSpread, F.ChargedSpazerFallingDiagonalThickSpread,
    ];

    /// <summary>The charged spazer wave startup short frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] ChargedSpazerWaveStartupShortFrames =
    [
        F.ChargedSpazerWaveUpStartupShort, F.ChargedSpazerWaveUpVerticalThickStartupShort,
        F.ChargedSpazerWaveUpRightStartupShort, F.ChargedSpazerWaveUpRightThickStartupShort,
        F.ChargedSpazerWaveRightStartupShort, F.ChargedSpazerWaveRightHorizontalThickStartupShort,
        F.ChargedSpazerWaveDownRightStartupShort, F.ChargedSpazerWaveDownRightThickStartupShort,
        F.ChargedSpazerWaveDownStartupShort, F.ChargedSpazerWaveDownVerticalThickStartupShort,
        F.ChargedSpazerWaveDownLeftUpRightStartupShort, F.ChargedSpazerWaveDownLeftUpRightThickStartupShort,
        F.ChargedSpazerWaveLeftStartupShort, F.ChargedSpazerWaveLeftHorizontalThickStartupShort,
        F.ChargedSpazerWaveUpLeftStartupShort, F.ChargedSpazerWaveUpLeftDownRightThickStartupShort,
    ];

    /// <summary>The charged spazer wave startup long frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] ChargedSpazerWaveStartupLongFrames =
    [
        F.ChargedSpazerWaveUpStartupLong, F.ChargedSpazerWaveUpVerticalThickStartupLong,
        F.ChargedSpazerWaveUpRightStartupLong, F.ChargedSpazerWaveUpRightThickStartupLong,
        F.ChargedSpazerWaveRightStartupLong, F.ChargedSpazerWaveRightHorizontalThickStartupLong,
        F.ChargedSpazerWaveDownRightStartupLong, F.ChargedSpazerWaveDownRightThickStartupLong,
        F.ChargedSpazerWaveDownStartupLong, F.ChargedSpazerWaveDownVerticalThickStartupLong,
        F.ChargedSpazerWaveDownLeftUpRightStartupLong, F.ChargedSpazerWaveDownLeftUpRightThickStartupLong,
        F.ChargedSpazerWaveLeftStartupLong, F.ChargedSpazerWaveLeftHorizontalThickStartupLong,
        F.ChargedSpazerWaveUpLeftStartupLong, F.ChargedSpazerWaveUpLeftDownRightThickStartupLong,
    ];

    /// <summary>The charged spazer wave seed frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] ChargedSpazerWaveSeedFrames =
    [
        F.ChargedSpazerWaveUpSeed, F.ChargedSpazerWaveUpVerticalThickSeed, F.ChargedSpazerWaveUpRightSeed,
        F.ChargedSpazerWaveUpRightThickSeed, F.ChargedSpazerWaveRightSeed, F.ChargedSpazerWaveRightHorizontalThickSeed,
        F.ChargedSpazerWaveDownRightSeed, F.ChargedSpazerWaveDownRightThickSeed, F.ChargedSpazerWaveDownSeed,
        F.ChargedSpazerWaveDownVerticalThickSeed, F.ChargedSpazerWaveDownLeftSeed,
        F.ChargedSpazerWaveDownLeftUpRightThickSeed, F.ChargedSpazerWaveLeftSeed,
        F.ChargedSpazerWaveLeftHorizontalThickSeed, F.ChargedSpazerWaveUpLeftSeed,
        F.ChargedSpazerWaveUpLeftDownRightThickSeed,
    ];

    /// <summary>The charged spazer wave splitting frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] ChargedSpazerWaveSplittingFrames =
    [
        F.ChargedSpazerWaveUpSplitting, F.ChargedSpazerWaveUpVerticalThickSplitting,
        F.ChargedSpazerWaveUpRightSplitting, F.ChargedSpazerWaveUpRightThickSplitting,
        F.ChargedSpazerWaveRightSplitting, F.ChargedSpazerWaveRightHorizontalThickSplitting,
        F.ChargedSpazerWaveDownRightSplitting, F.ChargedSpazerWaveDownRightThickSplitting,
        F.ChargedSpazerWaveDownSplitting, F.ChargedSpazerWaveDownVerticalThickSplitting,
        F.ChargedSpazerWaveDownLeftSplitting, F.ChargedSpazerWaveDownLeftUpRightThickSplitting,
        F.ChargedSpazerWaveLeftSplitting, F.ChargedSpazerWaveLeftHorizontalThickSplitting,
        F.ChargedSpazerWaveUpLeftSplitting, F.ChargedSpazerWaveUpLeftDownRightThickSplitting,
    ];

    /// <summary>The charged spazer wave parting frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] ChargedSpazerWavePartingFrames =
    [
        F.ChargedSpazerWaveUpParting, F.ChargedSpazerWaveUpVerticalThickParting, F.ChargedSpazerWaveUpRightParting,
        F.ChargedSpazerWaveUpRightThickParting, F.ChargedSpazerWaveRightParting,
        F.ChargedSpazerWaveRightHorizontalThickParting, F.ChargedSpazerWaveDownRightParting,
        F.ChargedSpazerWaveDownRightThickParting, F.ChargedSpazerWaveDownParting,
        F.ChargedSpazerWaveDownVerticalThickParting, F.ChargedSpazerWaveDownLeftParting,
        F.ChargedSpazerWaveDownLeftUpRightThickParting, F.ChargedSpazerWaveLeftParting,
        F.ChargedSpazerWaveLeftHorizontalThickParting, F.ChargedSpazerWaveUpLeftParting,
        F.ChargedSpazerWaveUpLeftDownRightThickParting,
    ];

    /// <summary>The charged spazer wave spreading frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] ChargedSpazerWaveSpreadingFrames =
    [
        F.ChargedSpazerWaveUpSpreading, F.ChargedSpazerWaveUpVerticalThickSpreading,
        F.ChargedSpazerWaveUpRightSpreading, F.ChargedSpazerWaveUpRightThickSpreading,
        F.ChargedSpazerWaveRightSpreading, F.ChargedSpazerWaveRightHorizontalThickSpreading,
        F.ChargedSpazerWaveDownRightSpreading, F.ChargedSpazerWaveDownRightThickSpreading,
        F.ChargedSpazerWaveDownSpreading, F.ChargedSpazerWaveDownVerticalThickSpreading,
        F.ChargedSpazerWaveDownLeftSpreading, F.ChargedSpazerWaveDownLeftUpRightThickSpreading,
        F.ChargedSpazerWaveLeftSpreading, F.ChargedSpazerWaveLeftHorizontalThickSpreading,
        F.ChargedSpazerWaveUpLeftSpreading, F.ChargedSpazerWaveUpLeftDownRightThickSpreading,
    ];

    /// <summary>The charged spazer wave nearly spread and spread frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] ChargedSpazerWaveNearlySpreadAndSpreadFrames =
    [
        F.ChargedSpazerWaveUpNearlySpread, F.ChargedSpazerWaveUpVerticalThickNearlySpread, F.ChargedSpazerWaveUpSpread,
        F.ChargedSpazerWaveUpVerticalThickSpread, F.ChargedSpazerWaveUpRightNearlySpread,
        F.ChargedSpazerWaveUpRightThickNearlySpread, F.ChargedSpazerWaveUpRightSpread,
        F.ChargedSpazerWaveUpRightThickSpread, F.ChargedSpazerWaveRightNearlySpread,
        F.ChargedSpazerWaveRightHorizontalThickNearlySpread, F.ChargedSpazerWaveRightSpread,
        F.ChargedSpazerWaveRightHorizontalThickSpread, F.ChargedSpazerWaveDownRightNearlySpread,
        F.ChargedSpazerWaveDownRightThickNearlySpread, F.ChargedSpazerWaveDownRightSpread,
        F.ChargedSpazerWaveDownRightThickSpread, F.ChargedSpazerWaveDownNearlySpread,
        F.ChargedSpazerWaveDownVerticalThickNearlySpread, F.ChargedSpazerWaveDownSpread,
        F.ChargedSpazerWaveDownVerticalThickSpread, F.ChargedSpazerWaveDownLeftNearlySpread,
        F.ChargedSpazerWaveDownLeftUpRightThickNearlySpread, F.ChargedSpazerWaveDownLeftSpread,
        F.ChargedSpazerWaveDownLeftUpRightThickSpread, F.ChargedSpazerWaveLeftNearlySpread,
        F.ChargedSpazerWaveLeftHorizontalThickNearlySpread, F.ChargedSpazerWaveLeftSpread,
        F.ChargedSpazerWaveLeftHorizontalThickSpread, F.ChargedSpazerWaveUpLeftNearlySpread,
        F.ChargedSpazerWaveUpLeftDownRightThickNearlySpread, F.ChargedSpazerWaveUpLeftSpread,
        F.ChargedSpazerWaveUpLeftDownRightThickSpread,
    ];

    /// <summary>The charged plasma core frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] ChargedPlasmaCoreFrames =
    [
        F.ChargedPlasmaVerticalCore, F.ChargedPlasmaVerticalThickCore, F.ChargedPlasmaRisingDiagonalCore,
        F.ChargedPlasmaRisingDiagonalThickCore, F.ChargedPlasmaHorizontalCore, F.ChargedPlasmaHorizontalThickCore,
        F.ChargedPlasmaFallingDiagonalCore, F.ChargedPlasmaFallingDiagonalThickCore,
    ];

    /// <summary>The charged plasma short frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] ChargedPlasmaShortFrames =
    [
        F.ChargedPlasmaVerticalShort, F.ChargedPlasmaVerticalThickShort, F.ChargedPlasmaRisingDiagonalShort,
        F.ChargedPlasmaRisingDiagonalThickShort, F.ChargedPlasmaHorizontalShort, F.ChargedPlasmaHorizontalThickShort,
        F.ChargedPlasmaFallingDiagonalShort, F.ChargedPlasmaFallingDiagonalThickShort,
    ];

    /// <summary>The charged plasma long frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] ChargedPlasmaLongFrames =
    [
        F.ChargedPlasmaVerticalLong, F.ChargedPlasmaVerticalThickLong, F.ChargedPlasmaRisingDiagonalLong,
        F.ChargedPlasmaRisingDiagonalThickLong, F.ChargedPlasmaHorizontalLong, F.ChargedPlasmaHorizontalThickLong,
        F.ChargedPlasmaFallingDiagonalLong, F.ChargedPlasmaFallingDiagonalThickLong,
    ];

    /// <summary>The charged plasma full frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] ChargedPlasmaFullFrames =
    [
        F.ChargedPlasmaVerticalFull, F.ChargedPlasmaVerticalThickFull, F.ChargedPlasmaRisingDiagonalFull,
        F.ChargedPlasmaRisingDiagonalThickFull, F.ChargedPlasmaHorizontalFull, F.ChargedPlasmaHorizontalThickFull,
        F.ChargedPlasmaFallingDiagonalFull, F.ChargedPlasmaFallingDiagonalThickFull,
    ];

    /// <summary>The charged plasma wave core frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] ChargedPlasmaWaveCoreFrames =
    [
        F.ChargedPlasmaWaveVerticalCore, F.ChargedPlasmaWaveVerticalThickCore, F.ChargedPlasmaWaveRisingDiagonalCore,
        F.ChargedPlasmaWaveRisingDiagonalThickCore, F.ChargedPlasmaWaveHorizontalCore,
        F.ChargedPlasmaWaveHorizontalThickCore, F.ChargedPlasmaWaveFallingDiagonalCore,
        F.ChargedPlasmaWaveFallingDiagonalThickCore,
    ];

    /// <summary>The charged plasma wave short frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] ChargedPlasmaWaveShortFrames =
    [
        F.ChargedPlasmaWaveVerticalShort, F.ChargedPlasmaWaveVerticalThickShort, F.ChargedPlasmaWaveRisingDiagonalShort,
        F.ChargedPlasmaWaveRisingDiagonalThickShort, F.ChargedPlasmaWaveHorizontalShort,
        F.ChargedPlasmaWaveHorizontalThickShort, F.ChargedPlasmaWaveFallingDiagonalShort,
        F.ChargedPlasmaWaveFallingDiagonalThickShort,
    ];

    /// <summary>The charged plasma wave long frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] ChargedPlasmaWaveLongFrames =
    [
        F.ChargedPlasmaWaveVerticalLong, F.ChargedPlasmaWaveVerticalThickLong, F.ChargedPlasmaWaveRisingDiagonalLong,
        F.ChargedPlasmaWaveRisingDiagonalThickLong, F.ChargedPlasmaWaveHorizontalLong,
        F.ChargedPlasmaWaveHorizontalThickLong, F.ChargedPlasmaWaveFallingDiagonalLong,
        F.ChargedPlasmaWaveFallingDiagonalThickLong,
    ];

    /// <summary>The charged plasma wave long core frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] ChargedPlasmaWaveLongCoreFrames =
    [
        F.ChargedPlasmaWaveVerticalLongCore, F.ChargedPlasmaWaveVerticalThickLongCore,
        F.ChargedPlasmaWaveRisingDiagonalLongCore, F.ChargedPlasmaWaveRisingDiagonalThickLongCore,
        F.ChargedPlasmaWaveHorizontalLongCore, F.ChargedPlasmaWaveHorizontalThickLongCore,
        F.ChargedPlasmaWaveFallingDiagonalLongCore, F.ChargedPlasmaWaveFallingDiagonalThickLongCore,
    ];

    /// <summary>The charged plasma wave lobes near frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] ChargedPlasmaWaveLobesNearFrames =
    [
        F.ChargedPlasmaWaveVerticalLongLobesNear, F.ChargedPlasmaWaveVerticalThickLobesNear,
        F.ChargedPlasmaWaveRisingDiagonalLongLobesNear, F.ChargedPlasmaWaveRisingDiagonalThickLobesNear,
        F.ChargedPlasmaWaveHorizontalLongLobesNear, F.ChargedPlasmaWaveHorizontalThickLobesNear,
        F.ChargedPlasmaWaveFallingDiagonalLongLobesNear, F.ChargedPlasmaWaveFallingDiagonalThickLobesNear,
    ];

    /// <summary>The charged plasma wave lobes midway frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] ChargedPlasmaWaveLobesMidwayFrames =
    [
        F.ChargedPlasmaWaveVerticalLongLobesMidway, F.ChargedPlasmaWaveVerticalThickLobesMidway,
        F.ChargedPlasmaWaveRisingDiagonalLongLobesMidway, F.ChargedPlasmaWaveRisingDiagonalThickLobesMidway,
        F.ChargedPlasmaWaveHorizontalLongLobesMidway, F.ChargedPlasmaWaveHorizontalThickLobesMidway,
        F.ChargedPlasmaWaveFallingDiagonalLongLobesMidway, F.ChargedPlasmaWaveFallingDiagonalThickLobesMidway,
    ];

    /// <summary>The charged plasma wave lobes far and peak frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] ChargedPlasmaWaveLobesFarAndPeakFrames =
    [
        F.ChargedPlasmaWaveVerticalLongLobesFar, F.ChargedPlasmaWaveVerticalThickLobesFar,
        F.ChargedPlasmaWaveVerticalLongLobesPeak, F.ChargedPlasmaWaveVerticalThickLobesPeak,
        F.ChargedPlasmaWaveRisingDiagonalLongLobesFar, F.ChargedPlasmaWaveRisingDiagonalThickLobesFar,
        F.ChargedPlasmaWaveRisingDiagonalLongLobesPeak, F.ChargedPlasmaWaveRisingDiagonalThickLobesPeak,
        F.ChargedPlasmaWaveHorizontalLongLobesFar, F.ChargedPlasmaWaveHorizontalThickLobesFar,
        F.ChargedPlasmaWaveHorizontalLongLobesPeak, F.ChargedPlasmaWaveHorizontalThickLobesPeak,
        F.ChargedPlasmaWaveFallingDiagonalLongLobesFar, F.ChargedPlasmaWaveFallingDiagonalThickLobesFar,
        F.ChargedPlasmaWaveFallingDiagonalLongLobesPeak, F.ChargedPlasmaWaveFallingDiagonalThickLobesPeak,
    ];

    /// <summary>The spazer up seed with attack trail frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] SpazerUpSeedWithAttackTrailFrames =
    [
        F.SpazerUpSeed, F.SpazerSpecialAttackTrailHidden,
    ];

    /// <summary>The spazer up parting with attack trail frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] SpazerUpPartingWithAttackTrailFrames =
    [
        F.SpazerUpParting, F.SpazerSpecialAttackTrailHiddenWide,
    ];

    /// <summary>The spazer up spread with attack trail frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] SpazerUpSpreadWithAttackTrailFrames =
    [
        F.SpazerUpSpread, F.SpazerSpecialAttackTrailHiddenWidest,
    ];

    /// <summary>The spazer wave nearly spread and spread frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] SpazerWaveNearlySpreadAndSpreadFrames =
    [
        .. SpazerWaveNearlySpreadFrames, .. SpazerWaveSpreadFrames,
    ];

    /// <summary>The plasma wave short lobes midway to peak frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] PlasmaWaveShortLobesMidwayToPeakFrames =
    [
        .. PlasmaWaveShortLobesMidwayFrames, .. PlasmaWaveShortLobesFarAndPeakFrames,
    ];

    /// <summary>The charged spazer wave splitting and parting frames, in every direction they are drawn in, which each trail list naming them gives one placement.</summary>
    private static readonly F[] ChargedSpazerWaveSplittingAndPartingFrames =
    [
        .. ChargedSpazerWaveSplittingFrames, .. ChargedSpazerWavePartingFrames,
    ];

    /// <summary>
    /// The family each choice names. Null is a choice that names no family, which draws no trail: a Spazer and Plasma
    /// combination's, which has none (#1926).
    /// </summary>
    internal static readonly FrozenDictionary<ProjectileTrailChoice, ProjectileTrailFamily?> Families =
        new Dictionary<ProjectileTrailChoice, ProjectileTrailFamily?>
        {
            [new(ProjectileTrailFamilyGroup.Uncharged, SamusBeamCombination.Power)] = ProjectileTrailFamily.UnchargedDefault,
            [new(ProjectileTrailFamilyGroup.Uncharged, SamusBeamCombination.Wave)] = ProjectileTrailFamily.UnchargedWave,
            [new(ProjectileTrailFamilyGroup.Uncharged, SamusBeamCombination.Ice)] = ProjectileTrailFamily.UnchargedDefault,
            [new(ProjectileTrailFamilyGroup.Uncharged, SamusBeamCombination.IceWave)] = ProjectileTrailFamily.UnchargedWave,
            [new(ProjectileTrailFamilyGroup.Uncharged, SamusBeamCombination.Spazer)] = ProjectileTrailFamily.UnchargedDefault,
            [new(ProjectileTrailFamilyGroup.Uncharged, SamusBeamCombination.SpazerWave)] = ProjectileTrailFamily.UnchargedDefault,
            [new(ProjectileTrailFamilyGroup.Uncharged, SamusBeamCombination.SpazerIce)] = ProjectileTrailFamily.UnchargedIceSpazer,
            [new(ProjectileTrailFamilyGroup.Uncharged, SamusBeamCombination.SpazerIceWave)] = ProjectileTrailFamily.UnchargedWaveIceSpazer,
            [new(ProjectileTrailFamilyGroup.Uncharged, SamusBeamCombination.Plasma)] = ProjectileTrailFamily.UnchargedDefault,
            [new(ProjectileTrailFamilyGroup.Uncharged, SamusBeamCombination.PlasmaWave)] = ProjectileTrailFamily.UnchargedDefault,
            [new(ProjectileTrailFamilyGroup.Uncharged, SamusBeamCombination.PlasmaIce)] = ProjectileTrailFamily.UnchargedIcePlasma,
            [new(ProjectileTrailFamilyGroup.Uncharged, SamusBeamCombination.PlasmaIceWave)] = ProjectileTrailFamily.UnchargedWaveIcePlasma,
            [new(ProjectileTrailFamilyGroup.Uncharged, SamusBeamCombination.SpazerPlasma)] = null,
            [new(ProjectileTrailFamilyGroup.Uncharged, SamusBeamCombination.SpazerPlasmaWave)] = null,
            [new(ProjectileTrailFamilyGroup.Uncharged, SamusBeamCombination.SpazerPlasmaIce)] = null,
            [new(ProjectileTrailFamilyGroup.Uncharged, SamusBeamCombination.SpazerPlasmaIceWave)] = null,
            [new(ProjectileTrailFamilyGroup.Charged, SamusBeamCombination.Power)] = ProjectileTrailFamily.ChargedDefault,
            [new(ProjectileTrailFamilyGroup.Charged, SamusBeamCombination.Wave)] = ProjectileTrailFamily.ChargedWave,
            [new(ProjectileTrailFamilyGroup.Charged, SamusBeamCombination.Ice)] = ProjectileTrailFamily.ChargedDefault,
            [new(ProjectileTrailFamilyGroup.Charged, SamusBeamCombination.IceWave)] = ProjectileTrailFamily.ChargedWave,
            [new(ProjectileTrailFamilyGroup.Charged, SamusBeamCombination.Spazer)] = ProjectileTrailFamily.ChargedDefault,
            [new(ProjectileTrailFamilyGroup.Charged, SamusBeamCombination.SpazerWave)] = ProjectileTrailFamily.ChargedDefault,
            [new(ProjectileTrailFamilyGroup.Charged, SamusBeamCombination.SpazerIce)] = ProjectileTrailFamily.ChargedIceSpazer,
            [new(ProjectileTrailFamilyGroup.Charged, SamusBeamCombination.SpazerIceWave)] = ProjectileTrailFamily.ChargedWaveIceSpazer,
            [new(ProjectileTrailFamilyGroup.Charged, SamusBeamCombination.Plasma)] = ProjectileTrailFamily.ChargedDefault,
            [new(ProjectileTrailFamilyGroup.Charged, SamusBeamCombination.PlasmaWave)] = ProjectileTrailFamily.ChargedDefault,
            [new(ProjectileTrailFamilyGroup.Charged, SamusBeamCombination.PlasmaIce)] = ProjectileTrailFamily.ChargedIcePlasma,
            [new(ProjectileTrailFamilyGroup.Charged, SamusBeamCombination.PlasmaIceWave)] = ProjectileTrailFamily.ChargedWaveIcePlasma,
            [new(ProjectileTrailFamilyGroup.Charged, SamusBeamCombination.SpazerPlasma)] = null,
            [new(ProjectileTrailFamilyGroup.Charged, SamusBeamCombination.SpazerPlasmaWave)] = null,
            [new(ProjectileTrailFamilyGroup.Charged, SamusBeamCombination.SpazerPlasmaIce)] = null,
            [new(ProjectileTrailFamilyGroup.Charged, SamusBeamCombination.SpazerPlasmaIceWave)] = null,
        }.ToFrozenDictionary();

    /// <summary>
    /// The list the Spazer special attack's trailing pair starts its trail from in each direction it flies (#2348): its own
    /// list, and a list of its own for downward travel.
    /// </summary>
    internal static readonly SamusProjectileDirectionMap<ProjectileTrailPlacementList> SpazerAttackTrailingPairLists =
        new(L.SpazerAttackTrailingPair, L.SpazerAttackTrailingPair, L.SpazerAttackTrailingPair, L.SpazerAttackTrailingPair,
            L.SpazerAttackTrailingPairDown, L.SpazerAttackTrailingPairDown, L.SpazerAttackTrailingPair, L.SpazerAttackTrailingPair,
            L.SpazerAttackTrailingPair, L.SpazerAttackTrailingPair);

    /// <summary>The list each family gives a shot in each direction it is fired in; null names no trail list.</summary>
    internal static readonly FrozenDictionary<ProjectileTrailFamily, SamusProjectileDirectionMap<ProjectileTrailPlacementList?>> Lists =
        new Dictionary<ProjectileTrailFamily, SamusProjectileDirectionMap<ProjectileTrailPlacementList?>>
        {
            [ProjectileTrailFamily.UnchargedDefault] = new(L.UnchargedDefault),
            [ProjectileTrailFamily.UnchargedWave] = new(L.UnchargedWaveVertical, L.UnchargedWaveRisingDiagonal, L.UnchargedWaveHorizontal, L.UnchargedWaveFallingDiagonal, L.UnchargedWaveVertical, L.UnchargedWaveVertical, L.UnchargedWaveRisingDiagonal, L.UnchargedWaveHorizontal, L.UnchargedWaveFallingDiagonal, L.UnchargedWaveVertical),
            [ProjectileTrailFamily.UnchargedIceSpazer] = new(L.UnchargedIceSpazerUp, L.UnchargedIceSpazerUpRight, L.UnchargedIceSpazerRight, L.UnchargedIceSpazerDownRight, L.UnchargedIceSpazerDown, L.UnchargedIceSpazerDown, L.UnchargedIceSpazerDownLeft, L.UnchargedIceSpazerLeft, L.UnchargedIceSpazerUpLeft, L.UnchargedIceSpazerUp),
            [ProjectileTrailFamily.UnchargedWaveIceSpazer] = new(L.UnchargedWaveIceSpazerUp, L.UnchargedWaveIceSpazerUpRight, L.UnchargedWaveIceSpazerRight, L.UnchargedWaveIceSpazerDownRight, L.UnchargedWaveIceSpazerDown, L.UnchargedWaveIceSpazerDown, L.UnchargedWaveIceSpazerDownLeft, L.UnchargedWaveIceSpazerLeft, L.UnchargedWaveIceSpazerUpLeft, L.UnchargedWaveIceSpazerUp),
            [ProjectileTrailFamily.UnchargedIcePlasma] = new(L.UnchargedIcePlasmaUp, L.UnchargedIcePlasmaUpRight, L.UnchargedIcePlasmaRight, L.UnchargedIcePlasmaDownRight, L.UnchargedIcePlasmaDown, L.UnchargedIcePlasmaDown, L.UnchargedIcePlasmaDownLeft, L.UnchargedIcePlasmaLeft, L.UnchargedIcePlasmaUpLeft, L.UnchargedIcePlasmaUp),
            [ProjectileTrailFamily.UnchargedWaveIcePlasma] = new(L.UnchargedWaveIcePlasmaUp, L.UnchargedWaveIcePlasmaUpRight, L.UnchargedWaveIcePlasmaRight, L.UnchargedWaveIcePlasmaDownRight, L.UnchargedWaveIcePlasmaDown, L.UnchargedWaveIcePlasmaDown, L.UnchargedWaveIcePlasmaDownLeft, L.UnchargedWaveIcePlasmaLeft, L.UnchargedWaveIcePlasmaUpLeft, L.UnchargedWaveIcePlasmaUp),
            [ProjectileTrailFamily.ChargedDefault] = new(L.ChargedDefault),
            [ProjectileTrailFamily.ChargedWave] = new(L.ChargedWaveVertical, L.ChargedWaveRisingDiagonal, L.ChargedWaveHorizontal, L.ChargedWaveFallingDiagonal, L.ChargedWaveVertical, L.ChargedWaveVertical, L.ChargedWaveRisingDiagonal, L.ChargedWaveHorizontal, L.ChargedWaveFallingDiagonal, L.ChargedWaveVertical),
            [ProjectileTrailFamily.ChargedIceSpazer] = new(L.ChargedIceSpazerUp, L.ChargedIceSpazerUpRight, L.ChargedIceSpazerRight, L.ChargedIceSpazerDownRight, L.ChargedIceSpazerDown, L.ChargedIceSpazerDown, L.ChargedIceSpazerDownLeft, L.ChargedIceSpazerLeft, L.ChargedIceSpazerUpLeft, L.ChargedIceSpazerUp),
            [ProjectileTrailFamily.ChargedWaveIceSpazer] = new(L.ChargedWaveIceSpazerUp, L.ChargedWaveIceSpazerUpRight, L.ChargedWaveIceSpazerRight, L.ChargedWaveIceSpazerDownRight, L.ChargedWaveIceSpazerDown, L.ChargedWaveIceSpazerDown, L.ChargedWaveIceSpazerDownLeft, L.ChargedWaveIceSpazerLeft, L.ChargedWaveIceSpazerUpLeft, L.ChargedWaveIceSpazerUp),
            [ProjectileTrailFamily.ChargedIcePlasma] = new(L.ChargedIcePlasmaUp, L.ChargedIcePlasmaUpRight, L.ChargedIcePlasmaRight, L.ChargedIcePlasmaDownRight, L.ChargedIcePlasmaDown, L.ChargedIcePlasmaDown, L.ChargedIcePlasmaDownLeft, L.ChargedIcePlasmaLeft, L.ChargedIcePlasmaUpLeft, L.ChargedIcePlasmaUp),
            [ProjectileTrailFamily.ChargedWaveIcePlasma] = new(L.ChargedWaveIcePlasmaUp, L.ChargedWaveIcePlasmaUpRight, L.ChargedWaveIcePlasmaRight, L.ChargedWaveIcePlasmaDownRight, L.ChargedWaveIcePlasmaDown, L.ChargedWaveIcePlasmaDown, L.ChargedWaveIcePlasmaDownLeft, L.ChargedWaveIcePlasmaLeft, L.ChargedWaveIcePlasmaUpLeft, L.ChargedWaveIcePlasmaUp),
        }.ToFrozenDictionary();

    /// <summary>
    /// Where each list starts a trail for each frame a shot that selects it can show: the two sparkles' offsets (#1926,
    /// #2124), each placement stated once with the frames it is given for (#2557). A frame a list holds no placement for
    /// leaves no trail (<see cref="ProjectileTrailPlacements.Start"/>) (#1625).
    /// </summary>
    internal static readonly FrozenDictionary<ProjectileTrailPlacementList, FrozenDictionary<SamusProjectileFrameId, ProjectileTrailPlacement>> PlacementsByList =
        new Dictionary<ProjectileTrailPlacementList, FrozenDictionary<SamusProjectileFrameId, ProjectileTrailPlacement>>
        {
            [L.UnchargedDefault] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot,
                [
                    F.PowerUp, F.PowerUpRight, F.PowerRight, F.PowerDownRight, F.PowerDown, F.PowerDownLeft,
                    F.PowerLeft, F.PowerUpLeft, F.IceStar, F.IceCrossed, F.IceNarrowStar, F.IceCrossedMirrored,
                    F.SpazerUpSeed, F.SpazerUpParting, F.SpazerUpSpread, F.SpazerUpRightSeed, F.SpazerUpRightParting,
                    F.SpazerUpRightSpread, F.SpazerRightSeed, F.SpazerRightParting, F.SpazerRightSpread,
                    F.SpazerDownRightSeed, F.SpazerDownRightParting, F.SpazerDownRightSpread, F.SpazerDownSeed,
                    F.SpazerDownParting, F.SpazerDownSpread, F.SpazerDownLeftSeed, F.SpazerDownLeftParting,
                    F.SpazerDownLeftSpread, F.SpazerLeftSeed, F.SpazerLeftParting, F.SpazerLeftSpread,
                    F.SpazerUpLeftSeed, F.SpazerUpLeftParting, F.SpazerUpLeftSpread, F.SpazerWaveUpSeed,
                    F.SpazerWaveUpSplitting, F.SpazerWaveUpParting, F.SpazerWaveUpSpreading, F.SpazerWaveUpNearlySpread,
                    F.SpazerWaveUpSpread, F.SpazerWaveUpRightSeed, F.SpazerWaveUpRightSplitting,
                    F.SpazerWaveUpRightParting, F.SpazerWaveUpRightSpreading, F.SpazerWaveUpRightNearlySpread,
                    F.SpazerWaveUpRightSpread, F.SpazerWaveRightSeed, F.SpazerWaveRightSplitting,
                    F.SpazerWaveRightParting, F.SpazerWaveRightSpreading, F.SpazerWaveRightNearlySpread,
                    F.SpazerWaveRightSpread, F.SpazerWaveDownRightSeed, F.SpazerWaveDownRightSplitting,
                    F.SpazerWaveDownRightParting, F.SpazerWaveDownRightSpreading, F.SpazerWaveDownRightNearlySpread,
                    F.SpazerWaveDownRightSpread, F.SpazerWaveDownSeed, F.SpazerWaveDownSplitting,
                    F.SpazerWaveDownParting, F.SpazerWaveDownSpreading, F.SpazerWaveDownNearlySpread,
                    F.SpazerWaveDownSpread, F.SpazerWaveDownLeftSeed, F.SpazerWaveDownLeftParting,
                    F.SpazerWaveDownLeftSpreading, F.SpazerWaveDownLeftNearlySpread, F.SpazerWaveDownLeftSpread,
                    F.SpazerWaveDownLeftSpreadShiftedTrail, F.SpazerWaveLeftSeed, F.SpazerWaveLeftSplitting,
                    F.SpazerWaveLeftParting, F.SpazerWaveLeftSpreading, F.SpazerWaveLeftNearlySpread,
                    F.SpazerWaveLeftSpread, F.SpazerWaveUpLeftSeed, F.SpazerWaveUpLeftSplitting,
                    F.SpazerWaveUpLeftParting, F.SpazerWaveUpLeftSpreading, F.SpazerWaveUpLeftNearlySpread,
                    F.SpazerWaveUpLeftSpread, F.PlasmaVerticalCore, F.PlasmaVerticalFullLength,
                    F.PlasmaRisingDiagonalCore, F.PlasmaRisingDiagonalFullLength, F.PlasmaHorizontalCore,
                    F.PlasmaHorizontalFullLength, F.PlasmaFallingDiagonalCore, F.PlasmaFallingDiagonalFullLength,
                    F.PlasmaWaveVerticalShortCore, F.PlasmaWaveVerticalShortLobesNear,
                    F.PlasmaWaveVerticalShortLobesMidway, F.PlasmaWaveVerticalShortLobesFar,
                    F.PlasmaWaveVerticalShortLobesPeak, F.PlasmaWaveRisingDiagonalShortCore,
                    F.PlasmaWaveRisingDiagonalShortLobesNear, F.PlasmaWaveRisingDiagonalShortLobesMidway,
                    F.PlasmaWaveRisingDiagonalShortLobesFar, F.PlasmaWaveRisingDiagonalShortLobesPeak,
                    F.PlasmaWaveHorizontalCore, F.PlasmaWaveHorizontalShortCore, F.PlasmaWaveHorizontalShortLobesNear,
                    F.PlasmaWaveHorizontalShortLobesMidway, F.PlasmaWaveHorizontalShortLobesFar,
                    F.PlasmaWaveHorizontalShortLobesPeak, F.PlasmaWaveFallingDiagonalShortCore,
                    F.PlasmaWaveFallingDiagonalShortLobesNear, F.PlasmaWaveFallingDiagonalShortLobesMidway,
                    F.PlasmaWaveFallingDiagonalShortLobesFar, F.PlasmaWaveFallingDiagonalShortLobesPeak, F.MissileUp,
                    F.MissileUpRight, F.MissileRight, F.MissileDownRight, F.MissileDown, F.MissileDownLeft,
                    F.MissileLeft, F.MissileUpLeft, F.SuperMissileUp, F.SuperMissileUpRight, F.SuperMissileRight,
                    F.SuperMissileDownRight, F.SuperMissileDown, F.SuperMissileDownLeft, F.SuperMissileLeft,
                    F.SuperMissileUpLeft, F.SuperMissileLinkHidden,
                ]),
            ]),
            [L.UnchargedWaveVertical] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, WaveCentreFrames),
                new(new(new(8, 0), new(0, 0)), WaveNearLobeFrames),
                new(new(new(12, 0), new(0, 0)), WaveMidwayLobeFrames),
                new(new(new(16, 0), new(0, 0)), WaveFarLobeFrames),
                new(new(new(20, 0), new(0, 0)), WavePeakLobeFrames),
                new(new(new(-8, 0), new(0, 0)), WaveNearLobeOppositeFrames),
                new(new(new(-12, 0), new(0, 0)), WaveMidwayLobeOppositeFrames),
                new(new(new(-16, 0), new(0, 0)), WaveFarLobeOppositeFrames),
                new(new(new(-20, 0), new(0, 0)), WavePeakLobeOppositeFrames),
            ]),
            [L.UnchargedWaveHorizontal] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, WaveCentreFrames),
                new(new(new(0, -8), new(0, 0)), WaveNearLobeFrames),
                new(new(new(0, -12), new(0, 0)), WaveMidwayLobeFrames),
                new(new(new(0, -16), new(0, 0)), WaveFarLobeFrames),
                new(new(new(0, -20), new(0, 0)), WavePeakLobeFrames),
                new(new(new(0, 8), new(0, 0)), WaveNearLobeOppositeFrames),
                new(new(new(0, 12), new(0, 0)), WaveMidwayLobeOppositeFrames),
                new(new(new(0, 16), new(0, 0)), WaveFarLobeOppositeFrames),
                new(new(new(0, 20), new(0, 0)), WavePeakLobeOppositeFrames),
            ]),
            [L.UnchargedWaveRisingDiagonal] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, WaveCentreFrames),
                new(new(new(-4, -4), new(0, 0)), WaveNearLobeFrames),
                new(new(new(-8, -8), new(0, 0)), WaveMidwayLobeFrames),
                new(new(new(-10, -10), new(0, 0)), WaveFarLobeFrames),
                new(new(new(-12, -12), new(0, 0)), WavePeakLobeFrames),
                new(new(new(4, 4), new(0, 0)), WaveNearLobeOppositeFrames),
                new(new(new(8, 8), new(0, 0)), WaveMidwayLobeOppositeFrames),
                new(new(new(10, 10), new(0, 0)), WaveFarLobeOppositeFrames),
                new(new(new(12, 12), new(0, 0)), WavePeakLobeOppositeFrames),
            ]),
            [L.UnchargedWaveFallingDiagonal] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, WaveCentreFrames),
                new(new(new(4, -4), new(0, 0)), WaveNearLobeFrames),
                new(new(new(8, -8), new(0, 0)), WaveMidwayLobeFrames),
                new(new(new(10, -10), new(0, 0)), WaveFarLobeFrames),
                new(new(new(12, -12), new(0, 0)), WavePeakLobeFrames),
                new(new(new(-4, 4), new(0, 0)), WaveNearLobeOppositeFrames),
                new(new(new(-8, 8), new(0, 0)), WaveMidwayLobeOppositeFrames),
                new(new(new(-10, 10), new(0, 0)), WaveFarLobeOppositeFrames),
                new(new(new(-12, 12), new(0, 0)), WavePeakLobeOppositeFrames),
            ]),
            [L.UnchargedIceSpazerUp] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, SpazerSeedFrames),
                new(new(new(-8, 8), new(8, 8)), SpazerPartingFrames),
                new(new(new(-16, 8), new(16, 8)), SpazerSpreadFrames),
            ]),
            [L.UnchargedIceSpazerDown] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, SpazerSeedFrames),
                new(new(new(-8, -8), new(8, -8)), SpazerPartingFrames),
                new(new(new(-16, -8), new(16, -8)), SpazerSpreadFrames),
            ]),
            [L.UnchargedIceSpazerUpRight] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(new(new(-8, 8), new(-8, 8)), SpazerSeedFrames),
                new(new(new(-14, 2), new(-2, 14)), SpazerPartingFrames),
                new(new(new(-20, -4), new(2, 20)), SpazerSpreadFrames),
            ]),
            [L.UnchargedIceSpazerRight] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(new(new(-8, 0), new(-8, 0)), SpazerSeedFrames),
                new(new(new(-8, -8), new(-8, 8)), SpazerPartingFrames),
                new(new(new(-8, -16), new(-8, 16)), SpazerSpreadFrames),
            ]),
            [L.UnchargedIceSpazerDownRight] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(new(new(-8, -8), new(-8, -8)), SpazerSeedFrames),
                new(new(new(-2, -16), new(-16, -2)), SpazerPartingFrames),
                new(new(new(4, -20), new(-20, 4)), SpazerSpreadFrames),
            ]),
            [L.UnchargedIceSpazerDownLeft] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(new(new(8, -8), new(8, -8)), SpazerSeedFrames),
                new(new(new(14, -2), new(2, -14)), SpazerPartingFrames),
                new(new(new(20, 4), new(-2, -20)), SpazerSpreadFrames),
            ]),
            [L.UnchargedIceSpazerLeft] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(new(new(8, 0), new(8, 0)), SpazerSeedFrames),
                new(new(new(8, 8), new(8, -8)), SpazerPartingFrames),
                new(new(new(8, 16), new(8, -16)), SpazerSpreadFrames),
            ]),
            [L.UnchargedIceSpazerUpLeft] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(new(new(8, 8), new(8, 8)), SpazerSeedFrames),
                new(new(new(2, 16), new(16, 2)), SpazerPartingFrames),
                new(new(new(-4, 20), new(20, -4)), SpazerSpreadFrames),
            ]),
            [L.UnchargedWaveIceSpazerUp] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, SpazerWaveSeedFrames),
                new(new(new(-4, 8), new(4, 8)), SpazerWaveSplittingFrames),
                new(new(new(-8, 8), new(8, 8)), SpazerWavePartingFrames),
                new(new(new(-12, 8), new(12, 8)), SpazerWaveSpreadingFrames),
                new(new(new(-16, 8), new(16, 8)), SpazerWaveNearlySpreadAndSpreadFrames),
            ]),
            [L.UnchargedWaveIceSpazerUpRight] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, SpazerWaveSeedFrames),
                new(new(new(-12, 6), new(-6, 12)), SpazerWaveSplittingFrames),
                new(new(new(-14, 2), new(-2, 14)), SpazerWavePartingFrames),
                new(new(new(-16, 0), new(0, 16)), SpazerWaveSpreadingFrames),
                new(new(new(-18, -2), new(2, 18)), SpazerWaveNearlySpreadFrames),
                new(new(new(-20, -4), new(2, 20)), SpazerWaveSpreadFrames),
            ]),
            [L.UnchargedWaveIceSpazerRight] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, SpazerWaveSeedFrames),
                new(new(new(-8, -4), new(-8, 4)), SpazerWaveSplittingFrames),
                new(new(new(-8, -8), new(-8, 8)), SpazerWavePartingFrames),
                new(new(new(-8, -12), new(-8, 12)), SpazerWaveSpreadingFrames),
                new(new(new(-8, -16), new(-8, 16)), SpazerWaveNearlySpreadAndSpreadFrames),
            ]),
            [L.UnchargedWaveIceSpazerDownRight] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, SpazerWaveSeedFrames),
                new(new(new(-12, -6), new(-6, -12)), SpazerWaveSplittingFrames),
                new(new(new(-2, -16), new(-16, -2)), SpazerWavePartingFrames),
                new(new(new(-16, 0), new(0, -16)), SpazerWaveSpreadingFrames),
                new(new(new(-18, 2), new(2, -18)), SpazerWaveNearlySpreadFrames),
                new(new(new(4, -20), new(-20, 4)), SpazerWaveSpreadFrames),
            ]),
            [L.UnchargedWaveIceSpazerDown] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, SpazerWaveSeedFrames),
                new(new(new(-4, -8), new(4, -8)), SpazerWaveSplittingFrames),
                new(new(new(-8, -8), new(8, -8)), SpazerWavePartingFrames),
                new(new(new(-12, -8), new(12, -8)), SpazerWaveSpreadingFrames),
                new(new(new(-16, -8), new(16, -8)), SpazerWaveNearlySpreadAndSpreadFrames),
            ]),
            [L.UnchargedWaveIceSpazerDownLeft] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, SpazerWaveSeedFrames),
                new(new(new(2, -14), new(14, -2)), SpazerWaveSplittingFrames),
                new(new(new(0, -16), new(16, 0)), SpazerWavePartingFrames),
                new(new(new(-2, -18), new(18, 2)), SpazerWaveSpreadingFrames),
                new(new(new(-2, -20), new(20, 4)), SpazerWaveNearlySpreadAndSpreadFrames),
            ]),
            [L.UnchargedWaveIceSpazerLeft] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, SpazerWaveSeedFrames),
                new(new(new(8, -4), new(8, 4)), SpazerWaveSplittingFrames),
                new(new(new(8, -8), new(8, 8)), SpazerWavePartingFrames),
                new(new(new(8, -12), new(8, 12)), SpazerWaveSpreadingFrames),
                new(new(new(8, -16), new(8, 16)), SpazerWaveNearlySpreadAndSpreadFrames),
            ]),
            [L.UnchargedWaveIceSpazerUpLeft] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, SpazerWaveSeedFrames),
                new(new(new(6, 10), new(10, 6)), SpazerWaveSplittingFrames),
                new(new(new(2, 16), new(16, 2)), SpazerWavePartingFrames),
                new(new(new(0, 16), new(16, 0)), SpazerWaveSpreadingFrames),
                new(new(new(-2, 18), new(18, -2)), SpazerWaveNearlySpreadFrames),
                new(new(new(-4, 20), new(20, -4)), SpazerWaveSpreadFrames),
            ]),
            [L.UnchargedIcePlasmaUp] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, PlasmaCoreFrames),
                new(new(new(0, 16), new(0, 16)), PlasmaFullLengthFrames),
            ]),
            [L.UnchargedIcePlasmaUpRight] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, PlasmaCoreFrames),
                new(new(new(-12, 12), new(-12, 12)), PlasmaFullLengthFrames),
            ]),
            [L.UnchargedIcePlasmaRight] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, PlasmaCoreFrames),
                new(new(new(-16, 0), new(-16, 0)), PlasmaFullLengthFrames),
            ]),
            [L.UnchargedIcePlasmaDownRight] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, PlasmaCoreFrames),
                new(new(new(-12, -12), new(-12, -12)), PlasmaFullLengthFrames),
            ]),
            [L.UnchargedIcePlasmaDown] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, PlasmaCoreFrames),
                new(new(new(0, -16), new(0, -16)), PlasmaFullLengthFrames),
            ]),
            [L.UnchargedIcePlasmaDownLeft] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, PlasmaCoreFrames),
                new(new(new(12, -12), new(12, -12)), PlasmaFullLengthFrames),
            ]),
            [L.UnchargedIcePlasmaLeft] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, PlasmaCoreFrames),
                new(new(new(16, 0), new(16, 0)), PlasmaFullLengthFrames),
            ]),
            [L.UnchargedIcePlasmaUpLeft] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, PlasmaCoreFrames),
                new(new(new(12, 12), new(12, 12)), PlasmaFullLengthFrames),
            ]),
            [L.UnchargedWaveIcePlasmaUp] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, PlasmaIceWaveCoreFrames),
                new(new(new(0, 16), new(0, 16)), PlasmaWaveShortCoreFrames),
                new(new(new(-8, 16), new(8, 16)), PlasmaWaveShortLobesNearFrames),
                new(new(new(-16, 16), new(16, 16)), PlasmaWaveShortLobesMidwayToPeakFrames),
            ]),
            [L.UnchargedWaveIcePlasmaUpRight] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, PlasmaIceWaveCoreFrames),
                new(new(new(-12, 12), new(-12, 12)), PlasmaWaveShortCoreFrames),
                new(new(new(-20, 8), new(-8, 18)), PlasmaWaveShortLobesNearFrames),
                new(new(new(-24, 2), new(-2, 20)), PlasmaWaveShortLobesMidwayFrames),
                new(new(new(-24, 0), new(0, 24)), PlasmaWaveShortLobesFarAndPeakFrames),
            ]),
            [L.UnchargedWaveIcePlasmaRight] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, PlasmaIceWaveCoreFrames),
                new(new(new(-16, 0), new(-16, 0)), PlasmaWaveShortCoreFrames),
                new(new(new(-16, -8), new(-16, 8)), PlasmaWaveShortLobesNearFrames),
                new(new(new(-16, -12), new(-16, 12)), PlasmaWaveShortLobesMidwayFrames),
                new(new(new(-16, -16), new(-16, 16)), PlasmaWaveShortLobesFarAndPeakFrames),
            ]),
            [L.UnchargedWaveIcePlasmaDownRight] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, PlasmaIceWaveCoreFrames),
                new(new(new(-12, -12), new(-12, -12)), PlasmaWaveShortCoreFrames),
                new(new(new(-18, -6), new(-6, -18)), PlasmaWaveShortLobesNearFrames),
                new(new(new(-20, -2), new(-2, -20)), PlasmaWaveShortLobesMidwayFrames),
                new(new(new(-24, 0), new(0, -24)), PlasmaWaveShortLobesFarAndPeakFrames),
            ]),
            [L.UnchargedWaveIcePlasmaDown] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, PlasmaIceWaveCoreFrames),
                new(new(new(0, -16), new(0, -16)), PlasmaWaveShortCoreFrames),
                new(new(new(-8, -16), new(8, -16)), PlasmaWaveShortLobesNearFrames),
                new(new(new(-16, -16), new(16, -16)), PlasmaWaveShortLobesMidwayToPeakFrames),
            ]),
            [L.UnchargedWaveIcePlasmaDownLeft] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, PlasmaIceWaveCoreFrames),
                new(new(new(12, -12), new(12, -12)), PlasmaWaveShortCoreFrames),
                new(new(new(20, -8), new(8, -18)), PlasmaWaveShortLobesNearFrames),
                new(new(new(24, -2), new(2, -20)), PlasmaWaveShortLobesMidwayFrames),
                new(new(new(24, 0), new(0, -24)), PlasmaWaveShortLobesFarAndPeakFrames),
            ]),
            [L.UnchargedWaveIcePlasmaLeft] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, PlasmaIceWaveCoreFrames),
                new(new(new(16, 0), new(16, 0)), PlasmaWaveShortCoreFrames),
                new(new(new(16, -8), new(16, 8)), PlasmaWaveShortLobesNearFrames),
                new(new(new(16, -12), new(16, 12)), PlasmaWaveShortLobesMidwayFrames),
                new(new(new(16, -16), new(16, 16)), PlasmaWaveShortLobesFarAndPeakFrames),
            ]),
            [L.UnchargedWaveIcePlasmaUpLeft] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, PlasmaIceWaveCoreFrames),
                new(new(new(12, 12), new(12, 12)), PlasmaWaveShortCoreFrames),
                new(new(new(18, 6), new(6, 18)), PlasmaWaveShortLobesNearFrames),
                new(new(new(20, 2), new(2, 20)), PlasmaWaveShortLobesMidwayFrames),
                new(new(new(24, 0), new(0, 24)), PlasmaWaveShortLobesFarAndPeakFrames),
            ]),
            [L.ChargedDefault] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot,
                [
                    F.SpazerUpSeed, F.SpazerUpParting, F.SpazerUpSpread, F.ChargedPowerUp, F.ChargedPowerUpOrb,
                    F.ChargedPowerUpRight, F.ChargedPowerUpRightOrb, F.ChargedPowerRight, F.ChargedPowerRightOrb,
                    F.ChargedPowerDownRight, F.ChargedPowerDownRightOrb, F.ChargedPowerDown, F.ChargedPowerDownOrb,
                    F.ChargedPowerDownLeft, F.ChargedPowerDownLeftOrb, F.ChargedPowerLeft, F.ChargedPowerLeftOrb,
                    F.ChargedPowerUpLeft, F.ChargedPowerUpLeftOrb, F.ChargedIceCrossed, F.ChargedIceStar,
                    F.ChargedSpazerVerticalStartupShort, F.ChargedSpazerVerticalThickStartupShort,
                    F.ChargedSpazerVerticalStartupLong, F.ChargedSpazerVerticalThickStartupLong,
                    F.ChargedSpazerVerticalSeed, F.ChargedSpazerVerticalThickSeed, F.ChargedSpazerVerticalParting,
                    F.ChargedSpazerVerticalThickParting, F.ChargedSpazerRisingDiagonalStartupShort,
                    F.ChargedSpazerRisingDiagonalThickStartupShort, F.ChargedSpazerRisingDiagonalStartupLong,
                    F.ChargedSpazerRisingDiagonalThickStartupLong, F.ChargedSpazerRisingDiagonalSeed,
                    F.ChargedSpazerRisingDiagonalThickSeed, F.ChargedSpazerRisingDiagonalParting,
                    F.ChargedSpazerRisingDiagonalThickParting, F.ChargedSpazerHorizontalStartupShort,
                    F.ChargedSpazerHorizontalThickStartupShort, F.ChargedSpazerHorizontalStartupLong,
                    F.ChargedSpazerHorizontalThickStartupLong, F.ChargedSpazerHorizontalSeed,
                    F.ChargedSpazerHorizontalThickSeed, F.ChargedSpazerHorizontalParting,
                    F.ChargedSpazerHorizontalThickParting, F.ChargedSpazerFallingDiagonalStartupShort,
                    F.ChargedSpazerFallingDiagonalThickStartupShort, F.ChargedSpazerFallingDiagonalStartupLong,
                    F.ChargedSpazerFallingDiagonalThickStartupLong, F.ChargedSpazerFallingDiagonalSeed,
                    F.ChargedSpazerFallingDiagonalThickSeed, F.ChargedSpazerFallingDiagonalParting,
                    F.ChargedSpazerFallingDiagonalThickParting, F.ChargedSpazerWaveUpStartupShort,
                    F.ChargedSpazerWaveUpVerticalThickStartupShort, F.ChargedSpazerWaveUpStartupLong,
                    F.ChargedSpazerWaveUpVerticalThickStartupLong, F.ChargedSpazerWaveUpSeed,
                    F.ChargedSpazerWaveUpVerticalThickSeed, F.ChargedSpazerWaveUpSplitting,
                    F.ChargedSpazerWaveUpVerticalThickSplitting, F.ChargedSpazerWaveUpRightStartupShort,
                    F.ChargedSpazerWaveUpRightThickStartupShort, F.ChargedSpazerWaveUpRightStartupLong,
                    F.ChargedSpazerWaveUpRightThickStartupLong, F.ChargedSpazerWaveUpRightSeed,
                    F.ChargedSpazerWaveUpRightThickSeed, F.ChargedSpazerWaveUpRightSplitting,
                    F.ChargedSpazerWaveUpRightThickSplitting, F.ChargedSpazerWaveRightStartupShort,
                    F.ChargedSpazerWaveRightHorizontalThickStartupShort, F.ChargedSpazerWaveRightStartupLong,
                    F.ChargedSpazerWaveRightHorizontalThickStartupLong, F.ChargedSpazerWaveRightSeed,
                    F.ChargedSpazerWaveRightHorizontalThickSeed, F.ChargedSpazerWaveRightSplitting,
                    F.ChargedSpazerWaveRightHorizontalThickSplitting, F.ChargedSpazerWaveDownRightStartupShort,
                    F.ChargedSpazerWaveDownRightThickStartupShort, F.ChargedSpazerWaveDownRightStartupLong,
                    F.ChargedSpazerWaveDownRightThickStartupLong, F.ChargedSpazerWaveDownRightSeed,
                    F.ChargedSpazerWaveDownRightThickSeed, F.ChargedSpazerWaveDownRightSplitting,
                    F.ChargedSpazerWaveDownRightThickSplitting, F.ChargedSpazerWaveDownStartupShort,
                    F.ChargedSpazerWaveDownVerticalThickStartupShort, F.ChargedSpazerWaveDownStartupLong,
                    F.ChargedSpazerWaveDownVerticalThickStartupLong, F.ChargedSpazerWaveDownSeed,
                    F.ChargedSpazerWaveDownVerticalThickSeed, F.ChargedSpazerWaveDownSplitting,
                    F.ChargedSpazerWaveDownVerticalThickSplitting, F.ChargedSpazerWaveDownLeftUpRightStartupShort,
                    F.ChargedSpazerWaveDownLeftUpRightThickStartupShort, F.ChargedSpazerWaveDownLeftUpRightStartupLong,
                    F.ChargedSpazerWaveDownLeftUpRightThickStartupLong, F.ChargedSpazerWaveDownLeftSeed,
                    F.ChargedSpazerWaveDownLeftUpRightThickSeed, F.ChargedSpazerWaveDownLeftSplitting,
                    F.ChargedSpazerWaveDownLeftUpRightThickSplitting, F.ChargedSpazerWaveLeftStartupShort,
                    F.ChargedSpazerWaveLeftHorizontalThickStartupShort, F.ChargedSpazerWaveLeftStartupLong,
                    F.ChargedSpazerWaveLeftHorizontalThickStartupLong, F.ChargedSpazerWaveLeftSeed,
                    F.ChargedSpazerWaveLeftHorizontalThickSeed, F.ChargedSpazerWaveLeftSplitting,
                    F.ChargedSpazerWaveLeftHorizontalThickSplitting, F.ChargedSpazerWaveUpLeftStartupShort,
                    F.ChargedSpazerWaveUpLeftDownRightThickStartupShort, F.ChargedSpazerWaveUpLeftStartupLong,
                    F.ChargedSpazerWaveUpLeftDownRightThickStartupLong, F.ChargedSpazerWaveUpLeftSeed,
                    F.ChargedSpazerWaveUpLeftDownRightThickSeed, F.ChargedSpazerWaveUpLeftSplitting,
                    F.ChargedSpazerWaveUpLeftDownRightThickSplitting, F.ChargedPlasmaVerticalCore,
                    F.ChargedPlasmaVerticalThickCore, F.ChargedPlasmaVerticalShort, F.ChargedPlasmaVerticalThickShort,
                    F.ChargedPlasmaVerticalLong, F.ChargedPlasmaVerticalThickLong, F.ChargedPlasmaVerticalFull,
                    F.ChargedPlasmaVerticalThickFull, F.ChargedPlasmaRisingDiagonalCore,
                    F.ChargedPlasmaRisingDiagonalThickCore, F.ChargedPlasmaRisingDiagonalShort,
                    F.ChargedPlasmaRisingDiagonalThickShort, F.ChargedPlasmaRisingDiagonalLong,
                    F.ChargedPlasmaRisingDiagonalThickLong, F.ChargedPlasmaRisingDiagonalFull,
                    F.ChargedPlasmaRisingDiagonalThickFull, F.ChargedPlasmaHorizontalCore,
                    F.ChargedPlasmaHorizontalThickCore, F.ChargedPlasmaHorizontalShort,
                    F.ChargedPlasmaHorizontalThickShort, F.ChargedPlasmaHorizontalLong,
                    F.ChargedPlasmaHorizontalThickLong, F.ChargedPlasmaHorizontalFull,
                    F.ChargedPlasmaHorizontalThickFull, F.ChargedPlasmaFallingDiagonalCore,
                    F.ChargedPlasmaFallingDiagonalThickCore, F.ChargedPlasmaFallingDiagonalShort,
                    F.ChargedPlasmaFallingDiagonalThickShort, F.ChargedPlasmaFallingDiagonalLong,
                    F.ChargedPlasmaFallingDiagonalThickLong, F.ChargedPlasmaFallingDiagonalFull,
                    F.ChargedPlasmaFallingDiagonalThickFull, F.ChargedPlasmaWaveVerticalCore,
                    F.ChargedPlasmaWaveVerticalThickCore, F.ChargedPlasmaWaveVerticalShort,
                    F.ChargedPlasmaWaveVerticalThickShort, F.ChargedPlasmaWaveVerticalLong,
                    F.ChargedPlasmaWaveVerticalThickLong, F.ChargedPlasmaWaveVerticalLongCore,
                    F.ChargedPlasmaWaveVerticalThickLongCore, F.ChargedPlasmaWaveRisingDiagonalCore,
                    F.ChargedPlasmaWaveRisingDiagonalThickCore, F.ChargedPlasmaWaveRisingDiagonalShort,
                    F.ChargedPlasmaWaveRisingDiagonalThickShort, F.ChargedPlasmaWaveRisingDiagonalLong,
                    F.ChargedPlasmaWaveRisingDiagonalThickLong, F.ChargedPlasmaWaveRisingDiagonalLongCore,
                    F.ChargedPlasmaWaveRisingDiagonalThickLongCore, F.ChargedPlasmaWaveHorizontalCore,
                    F.ChargedPlasmaWaveHorizontalThickCore, F.ChargedPlasmaWaveHorizontalShort,
                    F.ChargedPlasmaWaveHorizontalThickShort, F.ChargedPlasmaWaveHorizontalLong,
                    F.ChargedPlasmaWaveHorizontalThickLong, F.ChargedPlasmaWaveHorizontalLongCore,
                    F.ChargedPlasmaWaveHorizontalThickLongCore, F.ChargedPlasmaWaveFallingDiagonalCore,
                    F.ChargedPlasmaWaveFallingDiagonalThickCore, F.ChargedPlasmaWaveFallingDiagonalShort,
                    F.ChargedPlasmaWaveFallingDiagonalThickShort, F.ChargedPlasmaWaveFallingDiagonalLong,
                    F.ChargedPlasmaWaveFallingDiagonalThickLong, F.ChargedPlasmaWaveFallingDiagonalLongCore,
                    F.ChargedPlasmaWaveFallingDiagonalThickLongCore, F.PlasmaSpecialAttackOrb,
                    F.PlasmaSpecialAttackSparklingOrb, F.PlasmaSpecialAttackBurst, F.PlasmaSpecialAttackWideBurst,
                    F.PlasmaSpecialAttackRing,
                ]),
            ]),
            [L.ChargedWaveVertical] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, ChargedWaveCoreFrames),
                new(new(new(-8, 0), new(8, 0)), ChargedWaveLobesNearFrames),
                new(new(new(-12, 0), new(12, 0)), ChargedWaveLobesMidwayFrames),
                new(new(new(-16, 0), new(16, 0)), ChargedWaveLobesFarAndPeakFrames),
            ]),
            [L.ChargedWaveHorizontal] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, ChargedWaveCoreFrames),
                new(new(new(0, -8), new(0, 8)), ChargedWaveLobesNearFrames),
                new(new(new(0, -12), new(0, 12)), ChargedWaveLobesMidwayFrames),
                new(new(new(0, -16), new(0, 16)), ChargedWaveLobesFarAndPeakFrames),
            ]),
            [L.ChargedWaveRisingDiagonal] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, ChargedWaveCoreFrames),
                new(new(new(-4, -4), new(4, 4)), ChargedWaveLobesNearFrames),
                new(new(new(-8, -8), new(8, 8)), ChargedWaveLobesMidwayAndFarFrames),
                new(new(new(-10, -10), new(10, 10)), ChargedWaveLobesPeakFrames),
            ]),
            [L.ChargedWaveFallingDiagonal] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, ChargedWaveCoreFrames),
                new(new(new(-4, 4), new(4, -4)), ChargedWaveLobesNearFrames),
                new(new(new(-8, 8), new(8, -8)), ChargedWaveLobesMidwayAndFarFrames),
                new(new(new(-10, 10), new(10, -10)), ChargedWaveLobesPeakFrames),
            ]),
            [L.ChargedIceSpazerUp] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, ChargedSpazerStartupShortFrames),
                new(new(new(0, 8), new(0, 8)), ChargedSpazerStartupLongFrames),
                new(new(new(0, 16), new(0, 16)), ChargedSpazerSeedFrames),
                new(new(new(-8, 16), new(8, 16)), ChargedSpazerPartingFrames),
                new(new(new(-16, 16), new(16, 16)), ChargedSpazerSpreadFrames),
            ]),
            [L.ChargedIceSpazerUpRight] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, ChargedSpazerStartupShortFrames),
                new(new(new(-8, 8), new(-8, 8)), ChargedSpazerStartupLongFrames),
                new(new(new(-12, 12), new(-12, 12)), ChargedSpazerSeedFrames),
                new(new(new(-16, 8), new(-8, 16)), ChargedSpazerPartingFrames),
                new(new(new(-24, 0), new(0, 24)), ChargedSpazerSpreadFrames),
            ]),
            [L.ChargedIceSpazerRight] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, ChargedSpazerStartupShortFrames),
                new(new(new(-8, 8), new(-8, 8)), ChargedSpazerStartupLongFrames),
                new(new(new(-16, 0), new(-16, 0)), ChargedSpazerSeedFrames),
                new(new(new(-16, -8), new(-16, 8)), ChargedSpazerPartingFrames),
                new(new(new(-16, -16), new(-16, 16)), ChargedSpazerSpreadFrames),
            ]),
            [L.ChargedIceSpazerDownRight] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, ChargedSpazerStartupShortFrames),
                new(new(new(-8, -8), new(-8, -8)), ChargedSpazerStartupLongFrames),
                new(new(new(-12, -12), new(-12, -12)), ChargedSpazerSeedFrames),
                new(new(new(-16, -8), new(-8, -16)), ChargedSpazerPartingFrames),
                new(new(new(-24, 0), new(0, -24)), ChargedSpazerSpreadFrames),
            ]),
            [L.ChargedIceSpazerDown] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, ChargedSpazerStartupShortFrames),
                new(new(new(0, -8), new(0, -8)), ChargedSpazerStartupLongFrames),
                new(new(new(0, -16), new(0, -16)), ChargedSpazerSeedFrames),
                new(new(new(-8, -16), new(8, -16)), ChargedSpazerPartingFrames),
                new(new(new(-16, -16), new(16, -16)), ChargedSpazerSpreadFrames),
            ]),
            [L.ChargedIceSpazerDownLeft] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, ChargedSpazerStartupShortFrames),
                new(new(new(8, -8), new(8, -8)), ChargedSpazerStartupLongFrames),
                new(new(new(12, -12), new(12, -12)), ChargedSpazerSeedFrames),
                new(new(new(16, -8), new(8, -16)), ChargedSpazerPartingFrames),
                new(new(new(24, 0), new(0, -24)), ChargedSpazerSpreadFrames),
            ]),
            [L.ChargedIceSpazerLeft] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, ChargedSpazerStartupShortFrames),
                new(new(new(8, 0), new(8, 0)), ChargedSpazerStartupLongFrames),
                new(new(new(16, 0), new(16, 0)), ChargedSpazerSeedFrames),
                new(new(new(16, -8), new(16, 8)), ChargedSpazerPartingFrames),
                new(new(new(16, -16), new(16, 16)), ChargedSpazerSpreadFrames),
            ]),
            [L.ChargedIceSpazerUpLeft] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, ChargedSpazerStartupShortFrames),
                new(new(new(8, 8), new(8, 8)), ChargedSpazerStartupLongFrames),
                new(new(new(12, 12), new(12, 12)), ChargedSpazerSeedFrames),
                new(new(new(16, 8), new(8, 16)), ChargedSpazerPartingFrames),
                new(new(new(24, 0), new(0, 24)), ChargedSpazerSpreadFrames),
            ]),
            [L.ChargedWaveIceSpazerUp] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, ChargedSpazerWaveStartupShortFrames),
                new(new(new(0, 8), new(0, 8)), ChargedSpazerWaveStartupLongFrames),
                new(new(new(0, 16), new(0, 16)), ChargedSpazerWaveSeedFrames),
                new(new(new(-4, 16), new(4, 16)), ChargedSpazerWaveSplittingFrames),
                new(new(new(-8, 16), new(8, 16)), ChargedSpazerWavePartingFrames),
                new(new(new(-12, 16), new(12, 16)), ChargedSpazerWaveSpreadingFrames),
                new(new(new(-16, 16), new(16, 16)), ChargedSpazerWaveNearlySpreadAndSpreadFrames),
            ]),
            [L.ChargedWaveIceSpazerUpRight] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, ChargedSpazerWaveStartupShortFrames),
                new(new(new(-8, 8), new(-8, 8)), ChargedSpazerWaveStartupLongFrames),
                new(new(new(-12, 12), new(-12, 12)), ChargedSpazerWaveSeedFrames),
                new(new(new(-16, 8), new(-8, 16)),
                [
                    F.ChargedSpazerWaveUpSplitting, F.ChargedSpazerWaveUpVerticalThickSplitting,
                    F.ChargedSpazerWaveUpParting, F.ChargedSpazerWaveUpVerticalThickParting,
                    F.ChargedSpazerWaveUpSpreading, F.ChargedSpazerWaveUpVerticalThickSpreading,
                    F.ChargedSpazerWaveUpRightSplitting, F.ChargedSpazerWaveUpRightThickSplitting,
                    F.ChargedSpazerWaveUpRightParting, F.ChargedSpazerWaveUpRightThickParting,
                    F.ChargedSpazerWaveUpRightSpreading, F.ChargedSpazerWaveUpRightThickSpreading,
                    F.ChargedSpazerWaveRightSplitting, F.ChargedSpazerWaveRightHorizontalThickSplitting,
                    F.ChargedSpazerWaveRightParting, F.ChargedSpazerWaveRightHorizontalThickParting,
                    F.ChargedSpazerWaveRightSpreading, F.ChargedSpazerWaveRightHorizontalThickSpreading,
                    F.ChargedSpazerWaveDownRightSplitting, F.ChargedSpazerWaveDownRightThickSplitting,
                    F.ChargedSpazerWaveDownRightParting, F.ChargedSpazerWaveDownRightThickParting,
                    F.ChargedSpazerWaveDownRightSpreading, F.ChargedSpazerWaveDownRightThickSpreading,
                    F.ChargedSpazerWaveDownSplitting, F.ChargedSpazerWaveDownVerticalThickSplitting,
                    F.ChargedSpazerWaveDownParting, F.ChargedSpazerWaveDownVerticalThickParting,
                    F.ChargedSpazerWaveDownSpreading, F.ChargedSpazerWaveDownVerticalThickSpreading,
                    F.ChargedSpazerWaveDownLeftSplitting, F.ChargedSpazerWaveDownLeftUpRightThickSplitting,
                    F.ChargedSpazerWaveDownLeftParting, F.ChargedSpazerWaveDownLeftUpRightThickParting,
                    F.ChargedSpazerWaveDownLeftSpreading, F.ChargedSpazerWaveDownLeftUpRightThickSpreading,
                    F.ChargedSpazerWaveLeftSplitting, F.ChargedSpazerWaveLeftHorizontalThickSplitting,
                    F.ChargedSpazerWaveLeftParting, F.ChargedSpazerWaveLeftHorizontalThickParting,
                    F.ChargedSpazerWaveLeftSpreading, F.ChargedSpazerWaveLeftHorizontalThickSpreading,
                    F.ChargedSpazerWaveUpLeftSplitting, F.ChargedSpazerWaveUpLeftDownRightThickSplitting,
                    F.ChargedSpazerWaveUpLeftParting, F.ChargedSpazerWaveUpLeftDownRightThickParting,
                    F.ChargedSpazerWaveUpLeftSpreading, F.ChargedSpazerWaveUpLeftDownRightThickSpreading,
                ]),
                new(new(new(-24, 0), new(0, 24)), ChargedSpazerWaveNearlySpreadAndSpreadFrames),
            ]),
            [L.ChargedWaveIceSpazerRight] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, ChargedSpazerWaveStartupShortFrames),
                new(new(new(-8, 0), new(-8, 0)), ChargedSpazerWaveStartupLongFrames),
                new(new(new(-16, 0), new(-16, 0)), ChargedSpazerWaveSeedFrames),
                new(new(new(-16, -4), new(-16, 4)), ChargedSpazerWaveSplittingFrames),
                new(new(new(-16, -8), new(-16, 8)), ChargedSpazerWavePartingFrames),
                new(new(new(-16, -12), new(-16, 12)), ChargedSpazerWaveSpreadingFrames),
                new(new(new(-16, -16), new(-16, 16)), ChargedSpazerWaveNearlySpreadAndSpreadFrames),
            ]),
            [L.ChargedWaveIceSpazerDownRight] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, ChargedSpazerWaveStartupShortFrames),
                new(new(new(-8, -8), new(-8, -8)), ChargedSpazerWaveStartupLongFrames),
                new(new(new(-12, -12), new(-12, -12)), ChargedSpazerWaveSeedFrames),
                new(new(new(-16, -8), new(-8, -16)), ChargedSpazerWaveSplittingAndPartingFrames),
                new(new(new(-20, -4), new(-4, -20)), ChargedSpazerWaveSpreadingFrames),
                new(new(new(-24, 0), new(0, -24)), ChargedSpazerWaveNearlySpreadAndSpreadFrames),
            ]),
            [L.ChargedWaveIceSpazerDown] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, ChargedSpazerWaveStartupShortFrames),
                new(new(new(0, -8), new(0, -8)), ChargedSpazerWaveStartupLongFrames),
                new(new(new(0, -16), new(0, -16)), ChargedSpazerWaveSeedFrames),
                new(new(new(-4, -16), new(4, -16)), ChargedSpazerWaveSplittingFrames),
                new(new(new(-8, -16), new(8, -16)), ChargedSpazerWavePartingFrames),
                new(new(new(-12, -16), new(12, -16)), ChargedSpazerWaveSpreadingFrames),
                new(new(new(-16, -16), new(16, -16)), ChargedSpazerWaveNearlySpreadAndSpreadFrames),
            ]),
            [L.ChargedWaveIceSpazerDownLeft] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, ChargedSpazerWaveStartupShortFrames),
                new(new(new(8, -8), new(8, -8)), ChargedSpazerWaveStartupLongFrames),
                new(new(new(12, -12), new(12, -12)), ChargedSpazerWaveSeedFrames),
                new(new(new(8, -16), new(16, -8)), ChargedSpazerWaveSplittingAndPartingFrames),
                new(new(new(4, -20), new(20, -4)), ChargedSpazerWaveSpreadingFrames),
                new(new(new(0, -24), new(24, 0)), ChargedSpazerWaveNearlySpreadAndSpreadFrames),
            ]),
            [L.ChargedWaveIceSpazerLeft] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, ChargedSpazerWaveStartupShortFrames),
                new(new(new(8, 0), new(8, 0)), ChargedSpazerWaveStartupLongFrames),
                new(new(new(16, 0), new(16, 0)), ChargedSpazerWaveSeedFrames),
                new(new(new(16, -4), new(16, 4)), ChargedSpazerWaveSplittingFrames),
                new(new(new(16, -8), new(16, 8)), ChargedSpazerWavePartingFrames),
                new(new(new(16, -12), new(16, 12)), ChargedSpazerWaveSpreadingFrames),
                new(new(new(16, -16), new(16, 16)), ChargedSpazerWaveNearlySpreadAndSpreadFrames),
            ]),
            [L.ChargedWaveIceSpazerUpLeft] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, ChargedSpazerWaveStartupShortFrames),
                new(new(new(8, 8), new(8, 8)), ChargedSpazerWaveStartupLongFrames),
                new(new(new(12, 12), new(12, 12)), ChargedSpazerWaveSeedFrames),
                new(new(new(8, 16), new(16, 8)), ChargedSpazerWaveSplittingAndPartingFrames),
                new(new(new(4, 20), new(20, 4)), ChargedSpazerWaveSpreadingFrames),
                new(new(new(0, 24), new(24, 0)), ChargedSpazerWaveNearlySpreadAndSpreadFrames),
            ]),
            [L.ChargedIcePlasmaUp] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, ChargedPlasmaCoreFrames),
                new(new(new(0, 12), new(0, 12)), ChargedPlasmaShortFrames),
                new(new(new(0, 24), new(0, 24)), ChargedPlasmaLongFrames),
                new(new(new(0, 28), new(0, 28)), ChargedPlasmaFullFrames),
            ]),
            [L.ChargedIcePlasmaUpRight] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, ChargedPlasmaCoreFrames),
                new(new(new(-8, 8), new(-8, 8)), ChargedPlasmaShortFrames),
                new(new(new(-16, 16), new(-16, 16)), ChargedPlasmaLongFrames),
                new(new(new(-24, 24), new(-24, 24)), ChargedPlasmaFullFrames),
            ]),
            [L.ChargedIcePlasmaRight] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, ChargedPlasmaCoreFrames),
                new(new(new(-12, 0), new(-12, 0)), ChargedPlasmaShortFrames),
                new(new(new(-24, 0), new(-24, 0)), ChargedPlasmaLongFrames),
                new(new(new(-28, 0), new(-28, 0)), ChargedPlasmaFullFrames),
            ]),
            [L.ChargedIcePlasmaDownRight] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, ChargedPlasmaCoreFrames),
                new(new(new(-8, -8), new(-8, -8)), ChargedPlasmaShortFrames),
                new(new(new(-16, -16), new(-16, -16)), ChargedPlasmaLongFrames),
                new(new(new(-24, -24), new(-24, -24)), ChargedPlasmaFullFrames),
            ]),
            [L.ChargedIcePlasmaDown] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, ChargedPlasmaCoreFrames),
                new(new(new(0, -12), new(0, -12)), ChargedPlasmaShortFrames),
                new(new(new(0, -24), new(0, -24)), ChargedPlasmaLongFrames),
                new(new(new(0, -28), new(0, -28)), ChargedPlasmaFullFrames),
            ]),
            [L.ChargedIcePlasmaDownLeft] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, ChargedPlasmaCoreFrames),
                new(new(new(8, -8), new(8, -8)), ChargedPlasmaShortFrames),
                new(new(new(16, -16), new(16, -16)), ChargedPlasmaLongFrames),
                new(new(new(24, -24), new(24, -24)), ChargedPlasmaFullFrames),
            ]),
            [L.ChargedIcePlasmaLeft] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, ChargedPlasmaCoreFrames),
                new(new(new(12, 0), new(12, 0)), ChargedPlasmaShortFrames),
                new(new(new(24, 0), new(24, 0)), ChargedPlasmaLongFrames),
                new(new(new(28, 0), new(28, 0)), ChargedPlasmaFullFrames),
            ]),
            [L.ChargedIcePlasmaUpLeft] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, ChargedPlasmaCoreFrames),
                new(new(new(8, 8), new(8, 8)), ChargedPlasmaShortFrames),
                new(new(new(16, 16), new(16, 16)), ChargedPlasmaLongFrames),
                new(new(new(24, 24), new(24, 24)), ChargedPlasmaFullFrames),
            ]),
            [L.ChargedWaveIcePlasmaUp] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, ChargedPlasmaWaveCoreFrames),
                new(new(new(0, 12), new(0, 12)), ChargedPlasmaWaveShortFrames),
                new(new(new(0, 24), new(0, 24)), ChargedPlasmaWaveLongFrames),
                new(new(new(0, 28), new(0, 28)), ChargedPlasmaWaveLongCoreFrames),
                new(new(new(-8, 28), new(8, 28)), ChargedPlasmaWaveLobesNearFrames),
                new(new(new(-12, 28), new(12, 28)), ChargedPlasmaWaveLobesMidwayFrames),
                new(new(new(-16, 28), new(16, 28)), ChargedPlasmaWaveLobesFarAndPeakFrames),
            ]),
            [L.ChargedWaveIcePlasmaUpRight] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, ChargedPlasmaWaveCoreFrames),
                new(new(new(-8, 8), new(-8, 8)), ChargedPlasmaWaveShortFrames),
                new(new(new(-16, 16), new(-16, 16)), ChargedPlasmaWaveLongFrames),
                new(new(new(-20, 20), new(-20, 20)), ChargedPlasmaWaveLongCoreFrames),
                new(new(new(-28, 12), new(-16, 24)), ChargedPlasmaWaveLobesNearFrames),
                new(new(new(-32, 12), new(-12, 28)), ChargedPlasmaWaveLobesMidwayFrames),
                new(new(new(-32, 8), new(-8, 32)), ChargedPlasmaWaveLobesFarAndPeakFrames),
            ]),
            [L.ChargedWaveIcePlasmaRight] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, ChargedPlasmaWaveCoreFrames),
                new(new(new(-12, 0), new(-12, 0)), ChargedPlasmaWaveShortFrames),
                new(new(new(-24, 0), new(-24, 0)), ChargedPlasmaWaveLongFrames),
                new(new(new(-28, 0), new(-28, 0)), ChargedPlasmaWaveLongCoreFrames),
                new(new(new(-28, -8), new(-28, 8)), ChargedPlasmaWaveLobesNearFrames),
                new(new(new(-28, -12), new(-28, 12)), ChargedPlasmaWaveLobesMidwayFrames),
                new(new(new(-28, -16), new(-28, 16)), ChargedPlasmaWaveLobesFarAndPeakFrames),
            ]),
            [L.ChargedWaveIcePlasmaDownRight] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, ChargedPlasmaWaveCoreFrames),
                new(new(new(-8, -8), new(-8, -8)), ChargedPlasmaWaveShortFrames),
                new(new(new(-16, -16), new(-16, -16)), ChargedPlasmaWaveLongFrames),
                new(new(new(-20, -20), new(-20, -20)), ChargedPlasmaWaveLongCoreFrames),
                new(new(new(-24, -16), new(-16, -24)), ChargedPlasmaWaveLobesNearFrames),
                new(new(new(-32, -12), new(-12, -32)), ChargedPlasmaWaveLobesMidwayFrames),
                new(new(new(-32, -8), new(-8, -32)), ChargedPlasmaWaveLobesFarAndPeakFrames),
            ]),
            [L.ChargedWaveIcePlasmaDown] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, ChargedPlasmaWaveCoreFrames),
                new(new(new(0, -12), new(0, -12)), ChargedPlasmaWaveShortFrames),
                new(new(new(0, -24), new(0, -24)), ChargedPlasmaWaveLongFrames),
                new(new(new(0, -28), new(0, -28)), ChargedPlasmaWaveLongCoreFrames),
                new(new(new(-8, -28), new(8, -28)), ChargedPlasmaWaveLobesNearFrames),
                new(new(new(-12, -28), new(12, -28)), ChargedPlasmaWaveLobesMidwayFrames),
                new(new(new(-16, -28), new(16, -28)), ChargedPlasmaWaveLobesFarAndPeakFrames),
            ]),
            [L.ChargedWaveIcePlasmaDownLeft] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, ChargedPlasmaWaveCoreFrames),
                new(new(new(8, -8), new(8, -8)), ChargedPlasmaWaveShortFrames),
                new(new(new(16, -16), new(16, -16)), ChargedPlasmaWaveLongFrames),
                new(new(new(20, -20), new(20, -20)), ChargedPlasmaWaveLongCoreFrames),
                new(new(new(28, -12), new(16, -24)), ChargedPlasmaWaveLobesNearFrames),
                new(new(new(32, -12), new(12, -28)), ChargedPlasmaWaveLobesMidwayFrames),
                new(new(new(32, -8), new(8, -32)), ChargedPlasmaWaveLobesFarAndPeakFrames),
            ]),
            [L.ChargedWaveIcePlasmaLeft] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, ChargedPlasmaWaveCoreFrames),
                new(new(new(12, 0), new(12, 0)), ChargedPlasmaWaveShortFrames),
                new(new(new(24, 0), new(24, 0)), ChargedPlasmaWaveLongFrames),
                new(new(new(28, 0), new(28, 0)), ChargedPlasmaWaveLongCoreFrames),
                new(new(new(28, -8), new(28, 8)), ChargedPlasmaWaveLobesNearFrames),
                new(new(new(28, -12), new(28, 12)), ChargedPlasmaWaveLobesMidwayFrames),
                new(new(new(28, -16), new(28, 16)), ChargedPlasmaWaveLobesFarAndPeakFrames),
            ]),
            [L.ChargedWaveIcePlasmaUpLeft] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, ChargedPlasmaWaveCoreFrames),
                new(new(new(8, 8), new(8, 8)), ChargedPlasmaWaveShortFrames),
                new(new(new(16, 16), new(16, 16)), ChargedPlasmaWaveLongFrames),
                new(new(new(20, 20), new(20, 20)), ChargedPlasmaWaveLongCoreFrames),
                new(new(new(24, 16), new(16, 24)), ChargedPlasmaWaveLobesNearFrames),
                new(new(new(32, 12), new(12, 32)), ChargedPlasmaWaveLobesMidwayFrames),
                new(new(new(32, 8), new(8, 32)), ChargedPlasmaWaveLobesFarAndPeakFrames),
            ]),
            [L.SpazerAttackTrailingPair] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, SpazerUpSeedWithAttackTrailFrames),
                new(new(new(-8, 8), new(8, 8)), SpazerUpPartingWithAttackTrailFrames),
                new(new(new(-16, 8), new(16, 8)), SpazerUpSpreadWithAttackTrailFrames),
            ]),
            [L.SpazerAttackTrailingPairDown] = ProjectileTrailPlacementGroup.ByFrame(
            [
                new(ProjectileTrailPlacement.AtTheShot, SpazerUpSeedWithAttackTrailFrames),
                new(new(new(-8, -8), new(8, -8)), SpazerUpPartingWithAttackTrailFrames),
                new(new(new(-16, -8), new(16, -8)), SpazerUpSpreadWithAttackTrailFrames),
            ]),
        }.ToFrozenDictionary();
}
