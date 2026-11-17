namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// One frame a Samus projectile animation shows (#1301, #2124, #2274), named for what it shows: the shot of its animation
/// and the pose of the picture it shows, a stage of a swing, spread, growth or cycle named for that stage. A frame that
/// shows another frame's picture again with that frame's hitbox and trail placements is that frame; one that differs in
/// either is named for the difference. What a frame pictures is installed only (#1596); its values are arbitrary.
/// </summary>
internal enum SamusProjectileFrameId
{
    /// <summary>A frame of <see cref="SamusProjectileSegment.PowerUp"/> showing the Power picture, up.</summary>
    PowerUp,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PowerUpRight"/> showing the Power picture, up right.</summary>
    PowerUpRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PowerRight"/> showing the Power picture, right.</summary>
    PowerRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PowerDownRight"/> showing the Power picture, down right.</summary>
    PowerDownRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PowerDown"/> showing the Power picture, down.</summary>
    PowerDown,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PowerDownLeft"/> showing the Power picture, down left.</summary>
    PowerDownLeft,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PowerLeft"/> showing the Power picture, left.</summary>
    PowerLeft,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PowerUpLeft"/> showing the Power picture, up left.</summary>
    PowerUpLeft,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveUp"/> showing nothing.</summary>
    WaveUpHidden,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveVertical"/> showing the Wave picture, centre.</summary>
    WaveVerticalCentre,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveVertical"/> showing the Wave picture, down, right, its lobe near the centre.</summary>
    WaveVerticalRightNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveVertical"/> showing the Wave picture, down, right, its lobe midway out.</summary>
    WaveVerticalRightMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveVertical"/> showing the Wave picture, down, right, its lobe far out.</summary>
    WaveVerticalRightFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveVertical"/> showing the Wave picture, down, right, its lobe at its peak.</summary>
    WaveVerticalRightPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveVertical"/> showing the Wave picture, down, left, its lobe near the centre.</summary>
    WaveVerticalLeftNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveVertical"/> showing the Wave picture, down, left, its lobe midway out.</summary>
    WaveVerticalLeftMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveVertical"/> showing the Wave picture, down, left, its lobe far out.</summary>
    WaveVerticalLeftFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveVertical"/> showing the Wave picture, down, left, its lobe at its peak.</summary>
    WaveVerticalLeftPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveRisingDiagonal"/> showing the Wave picture, centre.</summary>
    WaveRisingDiagonalCentre,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveRisingDiagonal"/> showing the Wave picture, up right, up left, its lobe near the centre.</summary>
    WaveRisingDiagonalUpLeftNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveRisingDiagonal"/> showing the Wave picture, up right, up left, its lobe midway out.</summary>
    WaveRisingDiagonalUpLeftMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveRisingDiagonal"/> showing the Wave picture, up right, up left, its lobe far out.</summary>
    WaveRisingDiagonalUpLeftFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveRisingDiagonal"/> showing the Wave picture, up right, up left, its lobe at its peak.</summary>
    WaveRisingDiagonalUpLeftPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveRisingDiagonal"/> showing the Wave picture, up right, up left, its lobe near the centre, as <see cref="WaveRisingDiagonalUpLeftNear"/> does, with a narrower hitbox.</summary>
    WaveRisingDiagonalUpLeftNearNarrowHitbox,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveRisingDiagonal"/> showing the Wave picture, centre, as <see cref="WaveRisingDiagonalCentre"/> does, with a narrower hitbox.</summary>
    WaveRisingDiagonalCentreNarrowHitbox,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveRisingDiagonal"/> showing the Wave picture, up right, down right, its lobe near the centre.</summary>
    WaveRisingDiagonalDownRightNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveRisingDiagonal"/> showing the Wave picture, up right, down right, its lobe midway out.</summary>
    WaveRisingDiagonalDownRightMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveRisingDiagonal"/> showing the Wave picture, up right, down right, its lobe far out.</summary>
    WaveRisingDiagonalDownRightFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveRisingDiagonal"/> showing the Wave picture, up right, down right, its lobe at its peak.</summary>
    WaveRisingDiagonalDownRightPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveRisingDiagonal"/> showing the Wave picture, up right, down right, its lobe near the centre, as <see cref="WaveRisingDiagonalDownRightNear"/> does, with a wider hitbox.</summary>
    WaveRisingDiagonalDownRightNearWideHitbox,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveHorizontal"/> showing the Wave picture, centre.</summary>
    WaveHorizontalCentre,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveHorizontal"/> showing the Wave picture, right, up, its lobe near the centre.</summary>
    WaveHorizontalUpNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveHorizontal"/> showing the Wave picture, right, up, its lobe midway out.</summary>
    WaveHorizontalUpMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveHorizontal"/> showing the Wave picture, right, up, its lobe far out.</summary>
    WaveHorizontalUpFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveHorizontal"/> showing the Wave picture, right, up, its lobe at its peak.</summary>
    WaveHorizontalUpPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveHorizontal"/> showing the Wave picture, right, down, its lobe near the centre.</summary>
    WaveHorizontalDownNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveHorizontal"/> showing the Wave picture, right, down, its lobe midway out.</summary>
    WaveHorizontalDownMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveHorizontal"/> showing the Wave picture, right, down, its lobe far out.</summary>
    WaveHorizontalDownFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveHorizontal"/> showing the Wave picture, right, down, its lobe at its peak.</summary>
    WaveHorizontalDownPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveFallingDiagonal"/> showing the Wave picture, centre.</summary>
    WaveFallingDiagonalCentre,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveFallingDiagonal"/> showing the Wave picture, down right, up right, its lobe near the centre.</summary>
    WaveFallingDiagonalUpRightNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveFallingDiagonal"/> showing the Wave picture, down right, up right, its lobe midway out.</summary>
    WaveFallingDiagonalUpRightMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveFallingDiagonal"/> showing the Wave picture, down right, up right, its lobe far out.</summary>
    WaveFallingDiagonalUpRightFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveFallingDiagonal"/> showing the Wave picture, down right, up right, its lobe at its peak.</summary>
    WaveFallingDiagonalUpRightPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveFallingDiagonal"/> showing the Wave picture, down right, up right, its lobe near the centre, as <see cref="WaveFallingDiagonalUpRightNear"/> does, with a narrower hitbox.</summary>
    WaveFallingDiagonalUpRightNearNarrowHitbox,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveFallingDiagonal"/> showing the Wave picture, centre, as <see cref="WaveFallingDiagonalCentre"/> does, with a narrower hitbox.</summary>
    WaveFallingDiagonalCentreNarrowHitbox,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveFallingDiagonal"/> showing the Wave picture, down right, down left, its lobe near the centre.</summary>
    WaveFallingDiagonalDownLeftNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveFallingDiagonal"/> showing the Wave picture, down right, down left, its lobe midway out.</summary>
    WaveFallingDiagonalDownLeftMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveFallingDiagonal"/> showing the Wave picture, down right, down left, its lobe far out.</summary>
    WaveFallingDiagonalDownLeftFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveFallingDiagonal"/> showing the Wave picture, down right, down left, its lobe at its peak.</summary>
    WaveFallingDiagonalDownLeftPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveFallingDiagonal"/> showing the Wave picture, down right, down left, its lobe near the centre, as <see cref="WaveFallingDiagonalDownLeftNear"/> does, with a wider hitbox.</summary>
    WaveFallingDiagonalDownLeftNearWideHitbox,
    /// <summary>A frame of <see cref="SamusProjectileSegment.Ice"/> showing the Ice picture, a star.</summary>
    IceStar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.Ice"/> showing the Ice picture, a cross.</summary>
    IceCrossed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.Ice"/> showing the Ice picture, a narrow star.</summary>
    IceNarrowStar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.Ice"/> showing the Ice picture, a mirrored cross.</summary>
    IceCrossedMirrored,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerUp"/> showing the Spazer picture, up, seed.</summary>
    SpazerUpSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerUp"/> showing the Spazer picture, up, its lanes parting.</summary>
    SpazerUpParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerUpSpread"/> showing the Spazer picture, up, its lanes fully spread.</summary>
    SpazerUpSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerUpRight"/> showing the Spazer picture, up right, seed.</summary>
    SpazerUpRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerUpRight"/> showing the Spazer picture, up right, its lanes parting.</summary>
    SpazerUpRightParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerUpRightSpread"/> showing the Spazer picture, up right, its lanes fully spread.</summary>
    SpazerUpRightSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerRight"/> showing the Spazer picture, right, seed.</summary>
    SpazerRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerRight"/> showing the Spazer picture, right, its lanes parting.</summary>
    SpazerRightParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerRightSpread"/> showing the Spazer picture, right, its lanes fully spread.</summary>
    SpazerRightSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerDownRight"/> showing the Spazer picture, down right, seed.</summary>
    SpazerDownRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerDownRight"/> showing the Spazer picture, down right, its lanes parting.</summary>
    SpazerDownRightParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerDownRightSpread"/> showing the Spazer picture, down right, its lanes fully spread.</summary>
    SpazerDownRightSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerDown"/> showing the Spazer picture, down, seed.</summary>
    SpazerDownSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerDown"/> showing the Spazer picture, down, its lanes parting.</summary>
    SpazerDownParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerDownSpread"/> showing the Spazer picture, down, its lanes fully spread.</summary>
    SpazerDownSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerDownLeft"/> showing the Spazer picture, down left, seed.</summary>
    SpazerDownLeftSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerDownLeft"/> showing the Spazer picture, down left, its lanes parting.</summary>
    SpazerDownLeftParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerDownLeftSpread"/> showing the Spazer picture, down left, its lanes fully spread.</summary>
    SpazerDownLeftSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerLeft"/> showing the Spazer picture, left, seed.</summary>
    SpazerLeftSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerLeft"/> showing the Spazer picture, left, its lanes parting.</summary>
    SpazerLeftParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerLeftSpread"/> showing the Spazer picture, left, its lanes fully spread.</summary>
    SpazerLeftSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerUpLeft"/> showing the Spazer picture, up left, seed.</summary>
    SpazerUpLeftSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerUpLeft"/> showing the Spazer picture, up left, its lanes parting.</summary>
    SpazerUpLeftParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerUpLeftSpread"/> showing the Spazer picture, up left, its lanes fully spread.</summary>
    SpazerUpLeftSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUp"/> showing the Spazer picture, up, seed.</summary>
    SpazerWaveUpSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUp"/> showing the Spazer picture, up, its lanes splitting.</summary>
    SpazerWaveUpSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUp"/> showing the Spazer picture, up, its lanes parting.</summary>
    SpazerWaveUpParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUp"/> showing the Spazer picture, up, its lanes spreading.</summary>
    SpazerWaveUpSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUp"/> showing the Spazer picture, up, its lanes nearly spread.</summary>
    SpazerWaveUpNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUp"/> showing the Spazer picture, up, its lanes fully spread.</summary>
    SpazerWaveUpSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUp"/> showing the Spazer picture, up, its lanes parting, as <see cref="SpazerWaveUpParting"/> does, leaving no trail where <see cref="ProjectileTrailPlacementList.UnchargedDefault"/> places one from it.</summary>
    SpazerWaveUpPartingWithoutDefaultTrail,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUp"/> showing the Spazer picture, up, its lanes splitting, as <see cref="SpazerWaveUpSplitting"/> does, leaving no trail where <see cref="ProjectileTrailPlacementList.UnchargedDefault"/> places one from it.</summary>
    SpazerWaveUpSplittingWithoutDefaultTrail,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpRight"/> showing the Spazer picture, up right, seed.</summary>
    SpazerWaveUpRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpRight"/> showing the Spazer picture, up right, its lanes splitting.</summary>
    SpazerWaveUpRightSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpRight"/> showing the Spazer picture, up right, its lanes parting.</summary>
    SpazerWaveUpRightParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpRight"/> showing the Spazer picture, up right, its lanes spreading.</summary>
    SpazerWaveUpRightSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpRight"/> showing the Spazer picture, up right, its lanes nearly spread.</summary>
    SpazerWaveUpRightNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpRight"/> showing the Spazer picture, up right, its lanes fully spread.</summary>
    SpazerWaveUpRightSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpRight"/> showing the Spazer picture, up right, its lanes parting, as <see cref="SpazerWaveUpRightParting"/> does, leaving no trail where <see cref="ProjectileTrailPlacementList.UnchargedDefault"/> places one from it.</summary>
    SpazerWaveUpRightPartingWithoutDefaultTrail,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpRight"/> showing the Spazer picture, up right, its lanes splitting, as <see cref="SpazerWaveUpRightSplitting"/> does, leaving no trail where <see cref="ProjectileTrailPlacementList.UnchargedDefault"/> places one from it.</summary>
    SpazerWaveUpRightSplittingWithoutDefaultTrail,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveRight"/> showing the Spazer picture, right, seed.</summary>
    SpazerWaveRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveRight"/> showing the Spazer picture, right, its lanes splitting.</summary>
    SpazerWaveRightSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveRight"/> showing the Spazer picture, right, its lanes parting.</summary>
    SpazerWaveRightParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveRight"/> showing the Spazer picture, right, its lanes spreading.</summary>
    SpazerWaveRightSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveRight"/> showing the Spazer picture, right, its lanes nearly spread.</summary>
    SpazerWaveRightNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveRight"/> showing the Spazer picture, right, its lanes fully spread.</summary>
    SpazerWaveRightSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveRight"/> showing the Spazer picture, right, its lanes parting, as <see cref="SpazerWaveRightParting"/> does, leaving no trail where <see cref="ProjectileTrailPlacementList.UnchargedDefault"/> places one from it.</summary>
    SpazerWaveRightPartingWithoutDefaultTrail,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveRight"/> showing the Spazer picture, right, its lanes splitting, as <see cref="SpazerWaveRightSplitting"/> does, leaving no trail where <see cref="ProjectileTrailPlacementList.UnchargedDefault"/> places one from it.</summary>
    SpazerWaveRightSplittingWithoutDefaultTrail,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownRight"/> showing the Spazer picture, down right, seed.</summary>
    SpazerWaveDownRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownRight"/> showing the Spazer picture, down right, its lanes splitting.</summary>
    SpazerWaveDownRightSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownRight"/> showing the Spazer picture, down right, its lanes parting.</summary>
    SpazerWaveDownRightParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownRight"/> showing the Spazer picture, down right, its lanes spreading.</summary>
    SpazerWaveDownRightSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownRight"/> showing the Spazer picture, down right, its lanes nearly spread.</summary>
    SpazerWaveDownRightNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownRight"/> showing the Spazer picture, down right, its lanes fully spread.</summary>
    SpazerWaveDownRightSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownRight"/> showing the Spazer picture, down right, its lanes parting, as <see cref="SpazerWaveDownRightParting"/> does, leaving no trail where <see cref="ProjectileTrailPlacementList.UnchargedDefault"/> places one from it.</summary>
    SpazerWaveDownRightPartingWithoutDefaultTrail,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownRight"/> showing the Spazer picture, down right, its lanes splitting, as <see cref="SpazerWaveDownRightSplitting"/> does, leaving no trail where <see cref="ProjectileTrailPlacementList.UnchargedDefault"/> places one from it.</summary>
    SpazerWaveDownRightSplittingWithoutDefaultTrail,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDown"/> showing the Spazer picture, down, seed.</summary>
    SpazerWaveDownSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDown"/> showing the Spazer picture, down, its lanes splitting.</summary>
    SpazerWaveDownSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDown"/> showing the Spazer picture, down, its lanes parting.</summary>
    SpazerWaveDownParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDown"/> showing the Spazer picture, down, its lanes spreading.</summary>
    SpazerWaveDownSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDown"/> showing the Spazer picture, down, its lanes nearly spread.</summary>
    SpazerWaveDownNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDown"/> showing the Spazer picture, down, its lanes fully spread.</summary>
    SpazerWaveDownSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDown"/> showing the Spazer picture, down, its lanes parting, as <see cref="SpazerWaveDownParting"/> does, leaving no trail where <see cref="ProjectileTrailPlacementList.UnchargedDefault"/> places one from it.</summary>
    SpazerWaveDownPartingWithoutDefaultTrail,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDown"/> showing the Spazer picture, down, its lanes splitting, as <see cref="SpazerWaveDownSplitting"/> does, leaving no trail where <see cref="ProjectileTrailPlacementList.UnchargedDefault"/> places one from it.</summary>
    SpazerWaveDownSplittingWithoutDefaultTrail,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownLeft"/> showing the Spazer picture, down left, seed.</summary>
    SpazerWaveDownLeftSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownLeft"/> showing the Spazer picture, down left, its lanes parting.</summary>
    SpazerWaveDownLeftParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownLeft"/> showing the Spazer picture, down left, its lanes spreading.</summary>
    SpazerWaveDownLeftSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownLeft"/> showing the Spazer picture, down left, its lanes nearly spread.</summary>
    SpazerWaveDownLeftNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownLeft"/> showing the Spazer picture, down left, its lanes fully spread.</summary>
    SpazerWaveDownLeftSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownLeft"/> showing the Spazer picture, down left, its lanes fully spread, as <see cref="SpazerWaveDownLeftSpread"/> does, with its trail placed elsewhere by <see cref="ProjectileTrailPlacementList.UnchargedWaveIceSpazerUpRight"/>, <see cref="ProjectileTrailPlacementList.UnchargedWaveIceSpazerDownRight"/>, <see cref="ProjectileTrailPlacementList.UnchargedWaveIceSpazerUpLeft"/>.</summary>
    SpazerWaveDownLeftSpreadShiftedTrail,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownLeft"/> showing the Spazer picture, down left, its lanes spreading, as <see cref="SpazerWaveDownLeftSpreading"/> does, leaving no trail where <see cref="ProjectileTrailPlacementList.UnchargedDefault"/> places one from it.</summary>
    SpazerWaveDownLeftSpreadingWithoutDefaultTrail,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownLeft"/> showing the Spazer picture, down left, its lanes parting, as <see cref="SpazerWaveDownLeftParting"/> does, leaving no trail where <see cref="ProjectileTrailPlacementList.UnchargedDefault"/> places one from it.</summary>
    SpazerWaveDownLeftPartingWithoutDefaultTrail,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveLeft"/> showing the Spazer picture, left, seed.</summary>
    SpazerWaveLeftSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveLeft"/> showing the Spazer picture, left, its lanes splitting.</summary>
    SpazerWaveLeftSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveLeft"/> showing the Spazer picture, left, its lanes parting.</summary>
    SpazerWaveLeftParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveLeft"/> showing the Spazer picture, left, its lanes spreading.</summary>
    SpazerWaveLeftSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveLeft"/> showing the Spazer picture, left, its lanes nearly spread.</summary>
    SpazerWaveLeftNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveLeft"/> showing the Spazer picture, left, its lanes fully spread.</summary>
    SpazerWaveLeftSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveLeft"/> showing the Spazer picture, left, its lanes parting, as <see cref="SpazerWaveLeftParting"/> does, leaving no trail where <see cref="ProjectileTrailPlacementList.UnchargedDefault"/> places one from it.</summary>
    SpazerWaveLeftPartingWithoutDefaultTrail,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveLeft"/> showing the Spazer picture, left, its lanes splitting, as <see cref="SpazerWaveLeftSplitting"/> does, leaving no trail where <see cref="ProjectileTrailPlacementList.UnchargedDefault"/> places one from it.</summary>
    SpazerWaveLeftSplittingWithoutDefaultTrail,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpLeft"/> showing the Spazer picture, up left, seed.</summary>
    SpazerWaveUpLeftSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpLeft"/> showing the Spazer picture, up left, its lanes splitting.</summary>
    SpazerWaveUpLeftSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpLeft"/> showing the Spazer picture, up left, its lanes parting.</summary>
    SpazerWaveUpLeftParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpLeft"/> showing the Spazer picture, up left, its lanes spreading.</summary>
    SpazerWaveUpLeftSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpLeft"/> showing the Spazer picture, up left, its lanes nearly spread.</summary>
    SpazerWaveUpLeftNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpLeft"/> showing the Spazer picture, up left, its lanes fully spread.</summary>
    SpazerWaveUpLeftSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpLeft"/> showing the Spazer picture, up left, its lanes parting, as <see cref="SpazerWaveUpLeftParting"/> does, leaving no trail where <see cref="ProjectileTrailPlacementList.UnchargedDefault"/> places one from it.</summary>
    SpazerWaveUpLeftPartingWithoutDefaultTrail,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpLeft"/> showing the Spazer picture, up left, its lanes splitting, as <see cref="SpazerWaveUpLeftSplitting"/> does, leaving no trail where <see cref="ProjectileTrailPlacementList.UnchargedDefault"/> places one from it.</summary>
    SpazerWaveUpLeftSplittingWithoutDefaultTrail,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaVertical"/> showing the Plasma Startup picture, up, core.</summary>
    PlasmaVerticalCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaVerticalFullLength"/> showing the Plasma picture, up.</summary>
    PlasmaVerticalFullLength,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaRisingDiagonal"/> showing the Plasma Startup picture, up right, core.</summary>
    PlasmaRisingDiagonalCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaRisingDiagonalFullLength"/> showing the Plasma picture, up right.</summary>
    PlasmaRisingDiagonalFullLength,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaHorizontal"/> showing the Plasma Startup picture, right, core.</summary>
    PlasmaHorizontalCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaHorizontalFullLength"/> showing the Plasma picture, right.</summary>
    PlasmaHorizontalFullLength,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaFallingDiagonal"/> showing the Plasma Startup picture, down right, core.</summary>
    PlasmaFallingDiagonalCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaFallingDiagonalFullLength"/> showing the Plasma picture, down right.</summary>
    PlasmaFallingDiagonalFullLength,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaIceWaveVertical"/> showing the Plasma Startup picture, up, core.</summary>
    PlasmaIceWaveVerticalCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveVertical"/> showing the Plasma Wave picture, up short, core.</summary>
    PlasmaWaveVerticalShortCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveVertical"/> showing the Plasma Wave picture, up short, its lobes near the centre.</summary>
    PlasmaWaveVerticalShortLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveVertical"/> showing the Plasma Wave picture, up short, its lobes midway out.</summary>
    PlasmaWaveVerticalShortLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveVertical"/> showing the Plasma Wave picture, up short, its lobes far out.</summary>
    PlasmaWaveVerticalShortLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveVertical"/> showing the Plasma Wave picture, up short, its lobes at their peak.</summary>
    PlasmaWaveVerticalShortLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveVertical"/> showing the Plasma Wave picture, up short, its lobes near the centre, as <see cref="PlasmaWaveVerticalShortLobesNear"/> does, leaving no trail where <see cref="ProjectileTrailPlacementList.UnchargedDefault"/> places one from it.</summary>
    PlasmaWaveVerticalShortLobesNearWithoutDefaultTrail,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaIceWaveRisingDiagonal"/> showing the Plasma Startup picture, up right, core.</summary>
    PlasmaIceWaveRisingDiagonalCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveRisingDiagonal"/> showing the Plasma Wave picture, up right short, core.</summary>
    PlasmaWaveRisingDiagonalShortCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveRisingDiagonal"/> showing the Plasma Wave picture, up right short, its lobes near the centre.</summary>
    PlasmaWaveRisingDiagonalShortLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveRisingDiagonal"/> showing the Plasma Wave picture, up right short, its lobes midway out.</summary>
    PlasmaWaveRisingDiagonalShortLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveRisingDiagonal"/> showing the Plasma Wave picture, up right short, its lobes far out.</summary>
    PlasmaWaveRisingDiagonalShortLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveRisingDiagonal"/> showing the Plasma Wave picture, up right short, its lobes at their peak.</summary>
    PlasmaWaveRisingDiagonalShortLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveRisingDiagonal"/> showing the Plasma Wave picture, up right short, its lobes near the centre, as <see cref="PlasmaWaveRisingDiagonalShortLobesNear"/> does, leaving no trail where <see cref="ProjectileTrailPlacementList.UnchargedDefault"/> places one from it.</summary>
    PlasmaWaveRisingDiagonalShortLobesNearWithoutDefaultTrail,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveHorizontal"/> showing the Plasma Startup picture, right, core.</summary>
    PlasmaWaveHorizontalCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveHorizontalSwing"/> showing the Plasma Wave picture, right short, core.</summary>
    PlasmaWaveHorizontalShortCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveHorizontalSwing"/> showing the Plasma Wave picture, right short, its lobes near the centre.</summary>
    PlasmaWaveHorizontalShortLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveHorizontalSwing"/> showing the Plasma Wave picture, right short, its lobes midway out.</summary>
    PlasmaWaveHorizontalShortLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveHorizontalSwing"/> showing the Plasma Wave picture, right short, its lobes far out.</summary>
    PlasmaWaveHorizontalShortLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveHorizontalSwing"/> showing the Plasma Wave picture, right short, its lobes at their peak.</summary>
    PlasmaWaveHorizontalShortLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveHorizontalSwing"/> showing the Plasma Wave picture, right short, its lobes near the centre, as <see cref="PlasmaWaveHorizontalShortLobesNear"/> does, leaving no trail where <see cref="ProjectileTrailPlacementList.UnchargedDefault"/> places one from it.</summary>
    PlasmaWaveHorizontalShortLobesNearWithoutDefaultTrail,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaIceWaveFallingDiagonal"/> showing the Plasma Startup picture, down right, core.</summary>
    PlasmaIceWaveFallingDiagonalCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveFallingDiagonal"/> showing the Plasma Wave picture, down right short, core.</summary>
    PlasmaWaveFallingDiagonalShortCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveFallingDiagonal"/> showing the Plasma Wave picture, down right short, its lobes near the centre.</summary>
    PlasmaWaveFallingDiagonalShortLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveFallingDiagonal"/> showing the Plasma Wave picture, down right short, its lobes midway out.</summary>
    PlasmaWaveFallingDiagonalShortLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveFallingDiagonal"/> showing the Plasma Wave picture, down right short, its lobes far out.</summary>
    PlasmaWaveFallingDiagonalShortLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveFallingDiagonal"/> showing the Plasma Wave picture, down right short, its lobes at their peak.</summary>
    PlasmaWaveFallingDiagonalShortLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveFallingDiagonal"/> showing the Plasma Wave picture, down right short, its lobes near the centre, as <see cref="PlasmaWaveFallingDiagonalShortLobesNear"/> does, leaving no trail where <see cref="ProjectileTrailPlacementList.UnchargedDefault"/> places one from it.</summary>
    PlasmaWaveFallingDiagonalShortLobesNearWithoutDefaultTrail,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPowerUp"/> showing the Charged Power picture, up.</summary>
    ChargedPowerUp,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPowerUp"/> showing the Charged Power picture, up alternate.</summary>
    ChargedPowerUpAlternate,
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
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPowerDown"/> showing the Charged Power picture, down.</summary>
    ChargedPowerDown,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPowerDown"/> showing the Charged Power picture, down alternate.</summary>
    ChargedPowerDownAlternate,
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
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveUp"/> showing nothing.</summary>
    ChargedWaveUpHidden,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveVertical"/> showing the Charged Wave picture, core.</summary>
    ChargedWaveVerticalCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveVertical"/> showing the Charged Wave picture, core alternate.</summary>
    ChargedWaveVerticalCoreAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveVertical"/> showing the Charged Wave picture, down, its lobes near the centre.</summary>
    ChargedWaveVerticalLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveVertical"/> showing the Charged Wave picture, down, its lobes near the centre (alternate).</summary>
    ChargedWaveVerticalLobesNearAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveVertical"/> showing the Charged Wave picture, down, its lobes midway out.</summary>
    ChargedWaveVerticalLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveVertical"/> showing the Charged Wave picture, down, its lobes midway out (alternate).</summary>
    ChargedWaveVerticalLobesMidwayAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveVertical"/> showing the Charged Wave picture, down, its lobes far out.</summary>
    ChargedWaveVerticalLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveVertical"/> showing the Charged Wave picture, down, its lobes far out (alternate).</summary>
    ChargedWaveVerticalLobesFarAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveVertical"/> showing the Charged Wave picture, down, its lobes at their peak.</summary>
    ChargedWaveVerticalLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveVertical"/> showing the Charged Wave picture, down, its lobes at their peak (alternate).</summary>
    ChargedWaveVerticalLobesPeakAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRisingDiagonal"/> showing the Charged Wave picture, core.</summary>
    ChargedWaveRisingDiagonalCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRisingDiagonal"/> showing the Charged Wave picture, core alternate.</summary>
    ChargedWaveRisingDiagonalCoreAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRisingDiagonal"/> showing the Charged Wave picture, up right, its lobes near the centre.</summary>
    ChargedWaveRisingDiagonalLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRisingDiagonal"/> showing the Charged Wave picture, up right, its lobes near the centre (alternate).</summary>
    ChargedWaveRisingDiagonalLobesNearAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRisingDiagonal"/> showing the Charged Wave picture, up right, its lobes midway out.</summary>
    ChargedWaveRisingDiagonalLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRisingDiagonal"/> showing the Charged Wave picture, up right, its lobes midway out (alternate).</summary>
    ChargedWaveRisingDiagonalLobesMidwayAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRisingDiagonal"/> showing the Charged Wave picture, up right, its lobes far out.</summary>
    ChargedWaveRisingDiagonalLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRisingDiagonal"/> showing the Charged Wave picture, up right, its lobes far out (alternate).</summary>
    ChargedWaveRisingDiagonalLobesFarAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRisingDiagonal"/> showing the Charged Wave picture, up right, its lobes at their peak.</summary>
    ChargedWaveRisingDiagonalLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRisingDiagonal"/> showing the Charged Wave picture, up right, its lobes at their peak (alternate).</summary>
    ChargedWaveRisingDiagonalLobesPeakAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveHorizontal"/> showing the Charged Wave picture, core.</summary>
    ChargedWaveHorizontalCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveHorizontal"/> showing the Charged Wave picture, core alternate.</summary>
    ChargedWaveHorizontalCoreAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveHorizontal"/> showing the Charged Wave picture, right, its lobes near the centre.</summary>
    ChargedWaveHorizontalLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveHorizontal"/> showing the Charged Wave picture, right, its lobes near the centre (alternate).</summary>
    ChargedWaveHorizontalLobesNearAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveHorizontal"/> showing the Charged Wave picture, right, its lobes midway out.</summary>
    ChargedWaveHorizontalLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveHorizontal"/> showing the Charged Wave picture, right, its lobes midway out (alternate).</summary>
    ChargedWaveHorizontalLobesMidwayAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveHorizontal"/> showing the Charged Wave picture, right, its lobes far out.</summary>
    ChargedWaveHorizontalLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveHorizontal"/> showing the Charged Wave picture, right, its lobes far out (alternate).</summary>
    ChargedWaveHorizontalLobesFarAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveHorizontal"/> showing the Charged Wave picture, right, its lobes at their peak.</summary>
    ChargedWaveHorizontalLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveHorizontal"/> showing the Charged Wave picture, right, its lobes at their peak (alternate).</summary>
    ChargedWaveHorizontalLobesPeakAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveFallingDiagonal"/> showing the Charged Wave picture, core.</summary>
    ChargedWaveFallingDiagonalCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveFallingDiagonal"/> showing the Charged Wave picture, core alternate.</summary>
    ChargedWaveFallingDiagonalCoreAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveFallingDiagonal"/> showing the Charged Wave picture, down right, its lobes near the centre.</summary>
    ChargedWaveFallingDiagonalLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveFallingDiagonal"/> showing the Charged Wave picture, down right, its lobes near the centre (alternate).</summary>
    ChargedWaveFallingDiagonalLobesNearAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveFallingDiagonal"/> showing the Charged Wave picture, down right, its lobes midway out.</summary>
    ChargedWaveFallingDiagonalLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveFallingDiagonal"/> showing the Charged Wave picture, down right, its lobes midway out (alternate).</summary>
    ChargedWaveFallingDiagonalLobesMidwayAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveFallingDiagonal"/> showing the Charged Wave picture, down right, its lobes far out.</summary>
    ChargedWaveFallingDiagonalLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveFallingDiagonal"/> showing the Charged Wave picture, down right, its lobes far out (alternate).</summary>
    ChargedWaveFallingDiagonalLobesFarAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveFallingDiagonal"/> showing the Charged Wave picture, down right, its lobes at their peak.</summary>
    ChargedWaveFallingDiagonalLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveFallingDiagonal"/> showing the Charged Wave picture, down right, its lobes at their peak (alternate).</summary>
    ChargedWaveFallingDiagonalLobesPeakAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIce"/> showing the Charged Ice picture, a cross.</summary>
    ChargedIceCrossed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIce"/> showing the Charged Ice picture, a star.</summary>
    ChargedIceStar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveUp"/> showing nothing.</summary>
    ChargedIceWaveUpHidden,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveVertical"/> showing the Charged Ice Wave picture, core.</summary>
    ChargedIceWaveVerticalCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveVertical"/> showing the Charged Ice Wave picture, core alternate.</summary>
    ChargedIceWaveVerticalCoreAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveVertical"/> showing the Charged Ice Wave picture, down, its lobes near the centre.</summary>
    ChargedIceWaveVerticalLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveVertical"/> showing the Charged Ice Wave picture, down, its lobes near the centre (alternate).</summary>
    ChargedIceWaveVerticalLobesNearAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveVertical"/> showing the Charged Ice Wave picture, down, its lobes midway out.</summary>
    ChargedIceWaveVerticalLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveVertical"/> showing the Charged Ice Wave picture, down, its lobes midway out (alternate).</summary>
    ChargedIceWaveVerticalLobesMidwayAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveVertical"/> showing the Charged Ice Wave picture, down, its lobes far out.</summary>
    ChargedIceWaveVerticalLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveVertical"/> showing the Charged Ice Wave picture, down, its lobes far out (alternate).</summary>
    ChargedIceWaveVerticalLobesFarAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveVertical"/> showing the Charged Ice Wave picture, down, its lobes at their peak.</summary>
    ChargedIceWaveVerticalLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveVertical"/> showing the Charged Ice Wave picture, down, its lobes at their peak (alternate).</summary>
    ChargedIceWaveVerticalLobesPeakAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRisingDiagonal"/> showing the Charged Ice Wave picture, core.</summary>
    ChargedIceWaveRisingDiagonalCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRisingDiagonal"/> showing the Charged Ice Wave picture, core alternate.</summary>
    ChargedIceWaveRisingDiagonalCoreAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRisingDiagonal"/> showing the Charged Ice Wave picture, up right, its lobes near the centre.</summary>
    ChargedIceWaveRisingDiagonalLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRisingDiagonal"/> showing the Charged Ice Wave picture, up right, its lobes near the centre (alternate).</summary>
    ChargedIceWaveRisingDiagonalLobesNearAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRisingDiagonal"/> showing the Charged Ice Wave picture, up right, its lobes midway out.</summary>
    ChargedIceWaveRisingDiagonalLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRisingDiagonal"/> showing the Charged Ice Wave picture, up right, its lobes midway out (alternate).</summary>
    ChargedIceWaveRisingDiagonalLobesMidwayAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRisingDiagonal"/> showing the Charged Ice Wave picture, up right, its lobes far out.</summary>
    ChargedIceWaveRisingDiagonalLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRisingDiagonal"/> showing the Charged Ice Wave picture, up right, its lobes far out (alternate).</summary>
    ChargedIceWaveRisingDiagonalLobesFarAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRisingDiagonal"/> showing the Charged Ice Wave picture, up right, its lobes at their peak.</summary>
    ChargedIceWaveRisingDiagonalLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRisingDiagonal"/> showing the Charged Ice Wave picture, up right, its lobes at their peak (alternate).</summary>
    ChargedIceWaveRisingDiagonalLobesPeakAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveHorizontal"/> showing the Charged Ice Wave picture, core.</summary>
    ChargedIceWaveHorizontalCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveHorizontal"/> showing the Charged Ice Wave picture, core alternate.</summary>
    ChargedIceWaveHorizontalCoreAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveHorizontal"/> showing the Charged Ice Wave picture, right, its lobes near the centre.</summary>
    ChargedIceWaveHorizontalLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveHorizontal"/> showing the Charged Ice Wave picture, right, its lobes near the centre (alternate).</summary>
    ChargedIceWaveHorizontalLobesNearAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveHorizontal"/> showing the Charged Ice Wave picture, right, its lobes midway out.</summary>
    ChargedIceWaveHorizontalLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveHorizontal"/> showing the Charged Ice Wave picture, right, its lobes midway out (alternate).</summary>
    ChargedIceWaveHorizontalLobesMidwayAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveHorizontal"/> showing the Charged Ice Wave picture, right, its lobes far out.</summary>
    ChargedIceWaveHorizontalLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveHorizontal"/> showing the Charged Ice Wave picture, right, its lobes far out (alternate).</summary>
    ChargedIceWaveHorizontalLobesFarAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveHorizontal"/> showing the Charged Ice Wave picture, right, its lobes at their peak.</summary>
    ChargedIceWaveHorizontalLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveHorizontal"/> showing the Charged Ice Wave picture, right, its lobes at their peak (alternate).</summary>
    ChargedIceWaveHorizontalLobesPeakAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveFallingDiagonal"/> showing the Charged Ice Wave picture, core.</summary>
    ChargedIceWaveFallingDiagonalCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveFallingDiagonal"/> showing the Charged Ice Wave picture, core alternate.</summary>
    ChargedIceWaveFallingDiagonalCoreAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveFallingDiagonal"/> showing the Charged Ice Wave picture, down right, its lobes near the centre.</summary>
    ChargedIceWaveFallingDiagonalLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveFallingDiagonal"/> showing the Charged Ice Wave picture, down right, its lobes near the centre (alternate).</summary>
    ChargedIceWaveFallingDiagonalLobesNearAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveFallingDiagonal"/> showing the Charged Ice Wave picture, down right, its lobes midway out.</summary>
    ChargedIceWaveFallingDiagonalLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveFallingDiagonal"/> showing the Charged Ice Wave picture, down right, its lobes midway out (alternate).</summary>
    ChargedIceWaveFallingDiagonalLobesMidwayAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveFallingDiagonal"/> showing the Charged Ice Wave picture, down right, its lobes far out.</summary>
    ChargedIceWaveFallingDiagonalLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveFallingDiagonal"/> showing the Charged Ice Wave picture, down right, its lobes far out (alternate).</summary>
    ChargedIceWaveFallingDiagonalLobesFarAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveFallingDiagonal"/> showing the Charged Ice Wave picture, down right, its lobes at their peak.</summary>
    ChargedIceWaveFallingDiagonalLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveFallingDiagonal"/> showing the Charged Ice Wave picture, down right, its lobes at their peak (alternate).</summary>
    ChargedIceWaveFallingDiagonalLobesPeakAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerVertical"/> showing the Spazer Startup picture, up, a short startup strip.</summary>
    ChargedSpazerVerticalStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerVertical"/> showing the Spazer Startup picture, vertical alternate, a short startup strip.</summary>
    ChargedSpazerVerticalAlternateStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerVertical"/> showing the Spazer Startup picture, up, a long startup strip.</summary>
    ChargedSpazerVerticalStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerVertical"/> showing the Spazer Startup picture, vertical alternate, a long startup strip.</summary>
    ChargedSpazerVerticalAlternateStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerVertical"/> showing the Charged Spazer picture, up, seed.</summary>
    ChargedSpazerVerticalSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerVertical"/> showing the Charged Spazer picture, vertical alternate, seed.</summary>
    ChargedSpazerVerticalAlternateSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerVertical"/> showing the Charged Spazer picture, up, its lanes parting.</summary>
    ChargedSpazerVerticalParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerVertical"/> showing the Charged Spazer picture, vertical alternate, its lanes parting.</summary>
    ChargedSpazerVerticalAlternateParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerVerticalSpread"/> showing the Charged Spazer picture, up, its lanes fully spread.</summary>
    ChargedSpazerVerticalSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerVerticalSpread"/> showing the Charged Spazer picture, vertical alternate, its lanes fully spread.</summary>
    ChargedSpazerVerticalAlternateSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerRisingDiagonal"/> showing the Spazer Startup picture, up right, a short startup strip.</summary>
    ChargedSpazerRisingDiagonalStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerRisingDiagonal"/> showing the Spazer Startup picture, up right alternate, a short startup strip.</summary>
    ChargedSpazerRisingDiagonalAlternateStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerRisingDiagonal"/> showing the Spazer Startup picture, up right, a long startup strip.</summary>
    ChargedSpazerRisingDiagonalStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerRisingDiagonal"/> showing the Spazer Startup picture, up right alternate, a long startup strip.</summary>
    ChargedSpazerRisingDiagonalAlternateStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerRisingDiagonal"/> showing the Charged Spazer picture, up right, seed.</summary>
    ChargedSpazerRisingDiagonalSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerRisingDiagonal"/> showing the Charged Spazer picture, up right alternate, seed.</summary>
    ChargedSpazerRisingDiagonalAlternateSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerRisingDiagonal"/> showing the Charged Spazer picture, up right, its lanes parting.</summary>
    ChargedSpazerRisingDiagonalParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerRisingDiagonal"/> showing the Charged Spazer picture, up right alternate, its lanes parting.</summary>
    ChargedSpazerRisingDiagonalAlternateParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerRisingDiagonalSpread"/> showing the Charged Spazer picture, up right, its lanes fully spread.</summary>
    ChargedSpazerRisingDiagonalSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerRisingDiagonalSpread"/> showing the Charged Spazer picture, up right alternate, its lanes fully spread.</summary>
    ChargedSpazerRisingDiagonalAlternateSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerHorizontal"/> showing the Spazer Startup picture, right, a short startup strip.</summary>
    ChargedSpazerHorizontalStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerHorizontal"/> showing the Spazer Startup picture, horizontal alternate, a short startup strip.</summary>
    ChargedSpazerHorizontalAlternateStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerHorizontal"/> showing the Spazer Startup picture, right, a long startup strip.</summary>
    ChargedSpazerHorizontalStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerHorizontal"/> showing the Spazer Startup picture, horizontal alternate, a long startup strip.</summary>
    ChargedSpazerHorizontalAlternateStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerHorizontal"/> showing the Charged Spazer picture, left, seed.</summary>
    ChargedSpazerHorizontalSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerHorizontal"/> showing the Charged Spazer picture, horizontal alternate, seed.</summary>
    ChargedSpazerHorizontalAlternateSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerHorizontal"/> showing the Charged Spazer picture, left, its lanes parting.</summary>
    ChargedSpazerHorizontalParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerHorizontal"/> showing the Charged Spazer picture, horizontal alternate, its lanes parting.</summary>
    ChargedSpazerHorizontalAlternateParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerHorizontalSpread"/> showing the Charged Spazer picture, left, its lanes fully spread.</summary>
    ChargedSpazerHorizontalSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerHorizontalSpread"/> showing the Charged Spazer picture, horizontal alternate, its lanes fully spread.</summary>
    ChargedSpazerHorizontalAlternateSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerFallingDiagonal"/> showing the Spazer Startup picture, down right, a short startup strip.</summary>
    ChargedSpazerFallingDiagonalStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerFallingDiagonal"/> showing the Spazer Startup picture, down right alternate, a short startup strip.</summary>
    ChargedSpazerFallingDiagonalAlternateStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerFallingDiagonal"/> showing the Spazer Startup picture, down right, a long startup strip.</summary>
    ChargedSpazerFallingDiagonalStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerFallingDiagonal"/> showing the Spazer Startup picture, down right alternate, a long startup strip.</summary>
    ChargedSpazerFallingDiagonalAlternateStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerFallingDiagonal"/> showing the Charged Spazer picture, down right, seed.</summary>
    ChargedSpazerFallingDiagonalSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerFallingDiagonal"/> showing the Charged Spazer picture, down right alternate, seed.</summary>
    ChargedSpazerFallingDiagonalAlternateSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerFallingDiagonal"/> showing the Charged Spazer picture, down right, its lanes parting.</summary>
    ChargedSpazerFallingDiagonalParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerFallingDiagonal"/> showing the Charged Spazer picture, down right alternate, its lanes parting.</summary>
    ChargedSpazerFallingDiagonalAlternateParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerFallingDiagonalSpread"/> showing the Charged Spazer picture, down right, its lanes fully spread.</summary>
    ChargedSpazerFallingDiagonalSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerFallingDiagonalSpread"/> showing the Charged Spazer picture, down right alternate, its lanes fully spread.</summary>
    ChargedSpazerFallingDiagonalAlternateSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUp"/> showing the Spazer Startup picture, up, a short startup strip.</summary>
    ChargedSpazerWaveUpStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUp"/> showing the Spazer Startup picture, vertical alternate, a short startup strip.</summary>
    ChargedSpazerWaveUpVerticalAlternateStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUp"/> showing the Spazer Startup picture, up, a long startup strip.</summary>
    ChargedSpazerWaveUpStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUp"/> showing the Spazer Startup picture, vertical alternate, a long startup strip.</summary>
    ChargedSpazerWaveUpVerticalAlternateStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpWaving"/> showing the Charged Spazer picture, up, seed.</summary>
    ChargedSpazerWaveUpSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpWaving"/> showing the Charged Spazer picture, vertical alternate, seed.</summary>
    ChargedSpazerWaveUpVerticalAlternateSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpWaving"/> showing the Charged Spazer picture, up, its lanes splitting.</summary>
    ChargedSpazerWaveUpSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpWaving"/> showing the Charged Spazer picture, vertical alternate, its lanes splitting.</summary>
    ChargedSpazerWaveUpVerticalAlternateSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpWaving"/> showing the Charged Spazer picture, up, its lanes parting.</summary>
    ChargedSpazerWaveUpParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpWaving"/> showing the Charged Spazer picture, vertical alternate, its lanes parting.</summary>
    ChargedSpazerWaveUpVerticalAlternateParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpWaving"/> showing the Charged Spazer picture, up, its lanes spreading.</summary>
    ChargedSpazerWaveUpSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpWaving"/> showing the Charged Spazer picture, vertical alternate, its lanes spreading.</summary>
    ChargedSpazerWaveUpVerticalAlternateSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpWaving"/> showing the Charged Spazer picture, up, its lanes nearly spread.</summary>
    ChargedSpazerWaveUpNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpWaving"/> showing the Charged Spazer picture, vertical alternate, its lanes nearly spread.</summary>
    ChargedSpazerWaveUpVerticalAlternateNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpWaving"/> showing the Charged Spazer picture, up, its lanes fully spread.</summary>
    ChargedSpazerWaveUpSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpWaving"/> showing the Charged Spazer picture, vertical alternate, its lanes fully spread.</summary>
    ChargedSpazerWaveUpVerticalAlternateSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpWaving"/> showing the Charged Spazer picture, up, its lanes splitting, as <see cref="ChargedSpazerWaveUpSplitting"/> does, with a narrower hitbox and leaving no trail.</summary>
    ChargedSpazerWaveUpSplittingNarrowHitboxTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpWaving"/> showing the Charged Spazer picture, vertical alternate, its lanes splitting, as <see cref="ChargedSpazerWaveUpVerticalAlternateSplitting"/> does, with a narrower hitbox and leaving no trail.</summary>
    ChargedSpazerWaveUpVerticalAlternateSplittingNarrowHitboxTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRight"/> showing the Spazer Startup picture, up right, a short startup strip.</summary>
    ChargedSpazerWaveUpRightStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRight"/> showing the Spazer Startup picture, up right alternate, a short startup strip.</summary>
    ChargedSpazerWaveUpRightAlternateStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRight"/> showing the Spazer Startup picture, up right, a long startup strip.</summary>
    ChargedSpazerWaveUpRightStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRight"/> showing the Spazer Startup picture, up right alternate, a long startup strip.</summary>
    ChargedSpazerWaveUpRightAlternateStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightWaving"/> showing the Charged Spazer picture, up right, seed.</summary>
    ChargedSpazerWaveUpRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightWaving"/> showing the Charged Spazer picture, up right alternate, seed.</summary>
    ChargedSpazerWaveUpRightAlternateSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightWaving"/> showing the Charged Spazer picture, up right, its lanes splitting.</summary>
    ChargedSpazerWaveUpRightSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightWaving"/> showing the Charged Spazer picture, up right alternate, its lanes splitting.</summary>
    ChargedSpazerWaveUpRightAlternateSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightWaving"/> showing the Charged Spazer picture, up right, its lanes parting.</summary>
    ChargedSpazerWaveUpRightParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightWaving"/> showing the Charged Spazer picture, up right alternate, its lanes parting.</summary>
    ChargedSpazerWaveUpRightAlternateParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightWaving"/> showing the Charged Spazer picture, up right, its lanes spreading.</summary>
    ChargedSpazerWaveUpRightSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightWaving"/> showing the Charged Spazer picture, up right alternate, its lanes spreading.</summary>
    ChargedSpazerWaveUpRightAlternateSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightWaving"/> showing the Charged Spazer picture, up right, its lanes nearly spread.</summary>
    ChargedSpazerWaveUpRightNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightWaving"/> showing the Charged Spazer picture, up right alternate, its lanes nearly spread.</summary>
    ChargedSpazerWaveUpRightAlternateNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightWaving"/> showing the Charged Spazer picture, up right, its lanes fully spread.</summary>
    ChargedSpazerWaveUpRightSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightWaving"/> showing the Charged Spazer picture, up right alternate, its lanes fully spread.</summary>
    ChargedSpazerWaveUpRightAlternateSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightWaving"/> showing the Charged Spazer picture, up right, its lanes splitting, as <see cref="ChargedSpazerWaveUpRightSplitting"/> does, leaving no trail.</summary>
    ChargedSpazerWaveUpRightSplittingTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightWaving"/> showing the Charged Spazer picture, up right alternate, its lanes splitting, as <see cref="ChargedSpazerWaveUpRightAlternateSplitting"/> does, leaving no trail.</summary>
    ChargedSpazerWaveUpRightAlternateSplittingTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRight"/> showing the Spazer Startup picture, right, a short startup strip.</summary>
    ChargedSpazerWaveRightStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRight"/> showing the Spazer Startup picture, horizontal alternate, a short startup strip.</summary>
    ChargedSpazerWaveRightHorizontalAlternateStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRight"/> showing the Spazer Startup picture, right, a long startup strip.</summary>
    ChargedSpazerWaveRightStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRight"/> showing the Spazer Startup picture, horizontal alternate, a long startup strip.</summary>
    ChargedSpazerWaveRightHorizontalAlternateStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightWaving"/> showing the Charged Spazer picture, right, seed.</summary>
    ChargedSpazerWaveRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightWaving"/> showing the Charged Spazer picture, horizontal alternate, seed.</summary>
    ChargedSpazerWaveRightHorizontalAlternateSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightWaving"/> showing the Charged Spazer picture, right, its lanes splitting.</summary>
    ChargedSpazerWaveRightSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightWaving"/> showing the Charged Spazer picture, horizontal alternate, its lanes splitting.</summary>
    ChargedSpazerWaveRightHorizontalAlternateSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightWaving"/> showing the Charged Spazer picture, right, its lanes parting.</summary>
    ChargedSpazerWaveRightParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightWaving"/> showing the Charged Spazer picture, horizontal alternate, its lanes parting.</summary>
    ChargedSpazerWaveRightHorizontalAlternateParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightWaving"/> showing the Charged Spazer picture, right, its lanes spreading.</summary>
    ChargedSpazerWaveRightSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightWaving"/> showing the Charged Spazer picture, horizontal alternate, its lanes spreading.</summary>
    ChargedSpazerWaveRightHorizontalAlternateSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightWaving"/> showing the Charged Spazer picture, right, its lanes nearly spread.</summary>
    ChargedSpazerWaveRightNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightWaving"/> showing the Charged Spazer picture, horizontal alternate, its lanes nearly spread.</summary>
    ChargedSpazerWaveRightHorizontalAlternateNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightWaving"/> showing the Charged Spazer picture, right, its lanes fully spread.</summary>
    ChargedSpazerWaveRightSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightWaving"/> showing the Charged Spazer picture, horizontal alternate, its lanes fully spread.</summary>
    ChargedSpazerWaveRightHorizontalAlternateSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightWaving"/> showing the Charged Spazer picture, right, its lanes splitting, as <see cref="ChargedSpazerWaveRightSplitting"/> does, with a narrower hitbox and leaving no trail.</summary>
    ChargedSpazerWaveRightSplittingNarrowHitboxTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightWaving"/> showing the Charged Spazer picture, horizontal alternate, its lanes splitting, as <see cref="ChargedSpazerWaveRightHorizontalAlternateSplitting"/> does, with a narrower hitbox and leaving no trail.</summary>
    ChargedSpazerWaveRightHorizontalAlternateSplittingNarrowHitboxTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRight"/> showing the Spazer Startup picture, down right, a short startup strip.</summary>
    ChargedSpazerWaveDownRightStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRight"/> showing the Spazer Startup picture, down right alternate, a short startup strip.</summary>
    ChargedSpazerWaveDownRightAlternateStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRight"/> showing the Spazer Startup picture, down right, a long startup strip.</summary>
    ChargedSpazerWaveDownRightStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRight"/> showing the Spazer Startup picture, down right alternate, a long startup strip.</summary>
    ChargedSpazerWaveDownRightAlternateStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightWaving"/> showing the Charged Spazer picture, down right, seed.</summary>
    ChargedSpazerWaveDownRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightWaving"/> showing the Charged Spazer picture, down right alternate, seed.</summary>
    ChargedSpazerWaveDownRightAlternateSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightWaving"/> showing the Charged Spazer picture, down right, its lanes splitting.</summary>
    ChargedSpazerWaveDownRightSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightWaving"/> showing the Charged Spazer picture, down right alternate, its lanes splitting.</summary>
    ChargedSpazerWaveDownRightAlternateSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightWaving"/> showing the Charged Spazer picture, down right, its lanes parting.</summary>
    ChargedSpazerWaveDownRightParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightWaving"/> showing the Charged Spazer picture, down right alternate, its lanes parting.</summary>
    ChargedSpazerWaveDownRightAlternateParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightWaving"/> showing the Charged Spazer picture, down right, its lanes spreading.</summary>
    ChargedSpazerWaveDownRightSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightWaving"/> showing the Charged Spazer picture, down right alternate, its lanes spreading.</summary>
    ChargedSpazerWaveDownRightAlternateSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightWaving"/> showing the Charged Spazer picture, down right, its lanes nearly spread.</summary>
    ChargedSpazerWaveDownRightNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightWaving"/> showing the Charged Spazer picture, down right alternate, its lanes nearly spread.</summary>
    ChargedSpazerWaveDownRightAlternateNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightWaving"/> showing the Charged Spazer picture, down right, its lanes fully spread.</summary>
    ChargedSpazerWaveDownRightSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightWaving"/> showing the Charged Spazer picture, down right alternate, its lanes fully spread.</summary>
    ChargedSpazerWaveDownRightAlternateSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightWaving"/> showing the Charged Spazer picture, down right, its lanes splitting, as <see cref="ChargedSpazerWaveDownRightSplitting"/> does, leaving no trail.</summary>
    ChargedSpazerWaveDownRightSplittingTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightWaving"/> showing the Charged Spazer picture, down right alternate, its lanes splitting, as <see cref="ChargedSpazerWaveDownRightAlternateSplitting"/> does, leaving no trail.</summary>
    ChargedSpazerWaveDownRightAlternateSplittingTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDown"/> showing the Spazer Startup picture, down, a short startup strip.</summary>
    ChargedSpazerWaveDownStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDown"/> showing the Spazer Startup picture, vertical alternate, a short startup strip.</summary>
    ChargedSpazerWaveDownVerticalAlternateStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDown"/> showing the Spazer Startup picture, down, a long startup strip.</summary>
    ChargedSpazerWaveDownStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDown"/> showing the Spazer Startup picture, vertical alternate, a long startup strip.</summary>
    ChargedSpazerWaveDownVerticalAlternateStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownWaving"/> showing the Charged Spazer picture, down, seed.</summary>
    ChargedSpazerWaveDownSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownWaving"/> showing the Charged Spazer picture, vertical alternate, seed.</summary>
    ChargedSpazerWaveDownVerticalAlternateSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownWaving"/> showing the Charged Spazer picture, down, its lanes splitting.</summary>
    ChargedSpazerWaveDownSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownWaving"/> showing the Charged Spazer picture, vertical alternate, its lanes splitting.</summary>
    ChargedSpazerWaveDownVerticalAlternateSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownWaving"/> showing the Charged Spazer picture, down, its lanes parting.</summary>
    ChargedSpazerWaveDownParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownWaving"/> showing the Charged Spazer picture, vertical alternate, its lanes parting.</summary>
    ChargedSpazerWaveDownVerticalAlternateParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownWaving"/> showing the Charged Spazer picture, down, its lanes spreading.</summary>
    ChargedSpazerWaveDownSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownWaving"/> showing the Charged Spazer picture, vertical alternate, its lanes spreading.</summary>
    ChargedSpazerWaveDownVerticalAlternateSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownWaving"/> showing the Charged Spazer picture, down, its lanes nearly spread.</summary>
    ChargedSpazerWaveDownNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownWaving"/> showing the Charged Spazer picture, vertical alternate, its lanes nearly spread.</summary>
    ChargedSpazerWaveDownVerticalAlternateNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownWaving"/> showing the Charged Spazer picture, down, its lanes fully spread.</summary>
    ChargedSpazerWaveDownSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownWaving"/> showing the Charged Spazer picture, vertical alternate, its lanes fully spread.</summary>
    ChargedSpazerWaveDownVerticalAlternateSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownWaving"/> showing the Charged Spazer picture, down, its lanes splitting, as <see cref="ChargedSpazerWaveDownSplitting"/> does, with a narrower hitbox and leaving no trail.</summary>
    ChargedSpazerWaveDownSplittingNarrowHitboxTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownWaving"/> showing the Charged Spazer picture, vertical alternate, its lanes splitting, as <see cref="ChargedSpazerWaveDownVerticalAlternateSplitting"/> does, with a narrower hitbox and leaving no trail.</summary>
    ChargedSpazerWaveDownVerticalAlternateSplittingNarrowHitboxTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeft"/> showing the Spazer Startup picture, up right, a short startup strip.</summary>
    ChargedSpazerWaveDownLeftUpRightStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeft"/> showing the Spazer Startup picture, up right alternate, a short startup strip.</summary>
    ChargedSpazerWaveDownLeftUpRightAlternateStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeft"/> showing the Spazer Startup picture, up right, a long startup strip.</summary>
    ChargedSpazerWaveDownLeftUpRightStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeft"/> showing the Spazer Startup picture, up right alternate, a long startup strip.</summary>
    ChargedSpazerWaveDownLeftUpRightAlternateStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftWaving"/> showing the Charged Spazer picture, down left, seed.</summary>
    ChargedSpazerWaveDownLeftSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftWaving"/> showing the Charged Spazer picture, up right alternate, seed.</summary>
    ChargedSpazerWaveDownLeftUpRightAlternateSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftWaving"/> showing the Charged Spazer picture, down left, its lanes splitting.</summary>
    ChargedSpazerWaveDownLeftSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftWaving"/> showing the Charged Spazer picture, up right alternate, its lanes splitting.</summary>
    ChargedSpazerWaveDownLeftUpRightAlternateSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftWaving"/> showing the Charged Spazer picture, down left, its lanes parting.</summary>
    ChargedSpazerWaveDownLeftParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftWaving"/> showing the Charged Spazer picture, up right alternate, its lanes parting.</summary>
    ChargedSpazerWaveDownLeftUpRightAlternateParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftWaving"/> showing the Charged Spazer picture, down left, its lanes spreading.</summary>
    ChargedSpazerWaveDownLeftSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftWaving"/> showing the Charged Spazer picture, up right alternate, its lanes spreading.</summary>
    ChargedSpazerWaveDownLeftUpRightAlternateSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftWaving"/> showing the Charged Spazer picture, down left, its lanes nearly spread.</summary>
    ChargedSpazerWaveDownLeftNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftWaving"/> showing the Charged Spazer picture, up right alternate, its lanes nearly spread.</summary>
    ChargedSpazerWaveDownLeftUpRightAlternateNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftWaving"/> showing the Charged Spazer picture, down left, its lanes fully spread.</summary>
    ChargedSpazerWaveDownLeftSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftWaving"/> showing the Charged Spazer picture, up right alternate, its lanes fully spread.</summary>
    ChargedSpazerWaveDownLeftUpRightAlternateSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftWaving"/> showing the Charged Spazer picture, down left, its lanes splitting, as <see cref="ChargedSpazerWaveDownLeftSplitting"/> does, leaving no trail.</summary>
    ChargedSpazerWaveDownLeftSplittingTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftWaving"/> showing the Charged Spazer picture, up right alternate, its lanes splitting, as <see cref="ChargedSpazerWaveDownLeftUpRightAlternateSplitting"/> does, leaving no trail.</summary>
    ChargedSpazerWaveDownLeftUpRightAlternateSplittingTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeft"/> showing the Spazer Startup picture, left, a short startup strip.</summary>
    ChargedSpazerWaveLeftStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeft"/> showing the Spazer Startup picture, horizontal alternate, a short startup strip.</summary>
    ChargedSpazerWaveLeftHorizontalAlternateStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeft"/> showing the Spazer Startup picture, left, a long startup strip.</summary>
    ChargedSpazerWaveLeftStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeft"/> showing the Spazer Startup picture, horizontal alternate, a long startup strip.</summary>
    ChargedSpazerWaveLeftHorizontalAlternateStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftWaving"/> showing the Charged Spazer picture, left, seed.</summary>
    ChargedSpazerWaveLeftSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftWaving"/> showing the Charged Spazer picture, horizontal alternate, seed.</summary>
    ChargedSpazerWaveLeftHorizontalAlternateSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftWaving"/> showing the Charged Spazer picture, left, its lanes splitting.</summary>
    ChargedSpazerWaveLeftSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftWaving"/> showing the Charged Spazer picture, horizontal alternate, its lanes splitting.</summary>
    ChargedSpazerWaveLeftHorizontalAlternateSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftWaving"/> showing the Charged Spazer picture, left, its lanes parting.</summary>
    ChargedSpazerWaveLeftParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftWaving"/> showing the Charged Spazer picture, horizontal alternate, its lanes parting.</summary>
    ChargedSpazerWaveLeftHorizontalAlternateParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftWaving"/> showing the Charged Spazer picture, left, its lanes spreading.</summary>
    ChargedSpazerWaveLeftSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftWaving"/> showing the Charged Spazer picture, horizontal alternate, its lanes spreading.</summary>
    ChargedSpazerWaveLeftHorizontalAlternateSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftWaving"/> showing the Charged Spazer picture, left, its lanes nearly spread.</summary>
    ChargedSpazerWaveLeftNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftWaving"/> showing the Charged Spazer picture, horizontal alternate, its lanes nearly spread.</summary>
    ChargedSpazerWaveLeftHorizontalAlternateNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftWaving"/> showing the Charged Spazer picture, left, its lanes fully spread.</summary>
    ChargedSpazerWaveLeftSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftWaving"/> showing the Charged Spazer picture, horizontal alternate, its lanes fully spread.</summary>
    ChargedSpazerWaveLeftHorizontalAlternateSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftWaving"/> showing the Charged Spazer picture, left, its lanes splitting, as <see cref="ChargedSpazerWaveLeftSplitting"/> does, with a narrower hitbox and leaving no trail.</summary>
    ChargedSpazerWaveLeftSplittingNarrowHitboxTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftWaving"/> showing the Charged Spazer picture, horizontal alternate, its lanes splitting, as <see cref="ChargedSpazerWaveLeftHorizontalAlternateSplitting"/> does, with a narrower hitbox and leaving no trail.</summary>
    ChargedSpazerWaveLeftHorizontalAlternateSplittingNarrowHitboxTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeft"/> showing the Spazer Startup picture, up left, a short startup strip.</summary>
    ChargedSpazerWaveUpLeftStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeft"/> showing the Spazer Startup picture, down right alternate, a short startup strip.</summary>
    ChargedSpazerWaveUpLeftDownRightAlternateStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeft"/> showing the Spazer Startup picture, up left, a long startup strip.</summary>
    ChargedSpazerWaveUpLeftStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeft"/> showing the Spazer Startup picture, down right alternate, a long startup strip.</summary>
    ChargedSpazerWaveUpLeftDownRightAlternateStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftWaving"/> showing the Charged Spazer picture, up left, seed.</summary>
    ChargedSpazerWaveUpLeftSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftWaving"/> showing the Charged Spazer picture, down right alternate, seed.</summary>
    ChargedSpazerWaveUpLeftDownRightAlternateSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftWaving"/> showing the Charged Spazer picture, up left, its lanes splitting.</summary>
    ChargedSpazerWaveUpLeftSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftWaving"/> showing the Charged Spazer picture, down right alternate, its lanes splitting.</summary>
    ChargedSpazerWaveUpLeftDownRightAlternateSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftWaving"/> showing the Charged Spazer picture, up left, its lanes parting.</summary>
    ChargedSpazerWaveUpLeftParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftWaving"/> showing the Charged Spazer picture, down right alternate, its lanes parting.</summary>
    ChargedSpazerWaveUpLeftDownRightAlternateParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftWaving"/> showing the Charged Spazer picture, up left, its lanes spreading.</summary>
    ChargedSpazerWaveUpLeftSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftWaving"/> showing the Charged Spazer picture, down right alternate, its lanes spreading.</summary>
    ChargedSpazerWaveUpLeftDownRightAlternateSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftWaving"/> showing the Charged Spazer picture, up left, its lanes nearly spread.</summary>
    ChargedSpazerWaveUpLeftNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftWaving"/> showing the Charged Spazer picture, down right alternate, its lanes nearly spread.</summary>
    ChargedSpazerWaveUpLeftDownRightAlternateNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftWaving"/> showing the Charged Spazer picture, up left, its lanes fully spread.</summary>
    ChargedSpazerWaveUpLeftSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftWaving"/> showing the Charged Spazer picture, down right alternate, its lanes fully spread.</summary>
    ChargedSpazerWaveUpLeftDownRightAlternateSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftWaving"/> showing the Charged Spazer picture, up left, its lanes splitting, as <see cref="ChargedSpazerWaveUpLeftSplitting"/> does, leaving no trail.</summary>
    ChargedSpazerWaveUpLeftSplittingTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftWaving"/> showing the Charged Spazer picture, down right alternate, its lanes splitting, as <see cref="ChargedSpazerWaveUpLeftDownRightAlternateSplitting"/> does, leaving no trail.</summary>
    ChargedSpazerWaveUpLeftDownRightAlternateSplittingTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaVertical"/> showing the Plasma Startup picture, up, core.</summary>
    ChargedPlasmaVerticalCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaVertical"/> showing the Plasma Startup picture, up alternate, core.</summary>
    ChargedPlasmaVerticalAlternateCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaVertical"/> showing the Plasma Startup picture, up, short.</summary>
    ChargedPlasmaVerticalShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaVertical"/> showing the Plasma Startup picture, up alternate, short.</summary>
    ChargedPlasmaVerticalAlternateShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaVertical"/> showing the Plasma Startup picture, up, long.</summary>
    ChargedPlasmaVerticalLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaVertical"/> showing the Plasma Startup picture, up alternate, long.</summary>
    ChargedPlasmaVerticalAlternateLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaVerticalFullLength"/> showing the Plasma Startup picture, up, full.</summary>
    ChargedPlasmaVerticalFull,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaVerticalFullLength"/> showing the Plasma Startup picture, up alternate, full.</summary>
    ChargedPlasmaVerticalAlternateFull,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaRisingDiagonal"/> showing the Plasma Startup picture, up right, core.</summary>
    ChargedPlasmaRisingDiagonalCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaRisingDiagonal"/> showing the Plasma Startup picture, up right alternate, core.</summary>
    ChargedPlasmaRisingDiagonalAlternateCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaRisingDiagonal"/> showing the Plasma Startup picture, up right, short.</summary>
    ChargedPlasmaRisingDiagonalShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaRisingDiagonal"/> showing the Plasma Startup picture, up right alternate, short.</summary>
    ChargedPlasmaRisingDiagonalAlternateShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaRisingDiagonal"/> showing the Plasma Startup picture, up right, long.</summary>
    ChargedPlasmaRisingDiagonalLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaRisingDiagonal"/> showing the Plasma Startup picture, up right alternate, long.</summary>
    ChargedPlasmaRisingDiagonalAlternateLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaRisingDiagonalFullLength"/> showing the Plasma Startup picture, up right, full.</summary>
    ChargedPlasmaRisingDiagonalFull,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaRisingDiagonalFullLength"/> showing the Plasma Startup picture, up right alternate, full.</summary>
    ChargedPlasmaRisingDiagonalAlternateFull,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaHorizontal"/> showing the Plasma Startup picture, right, core.</summary>
    ChargedPlasmaHorizontalCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaHorizontal"/> showing the Plasma Startup picture, right alternate, core.</summary>
    ChargedPlasmaHorizontalAlternateCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaHorizontal"/> showing the Plasma Startup picture, right, short.</summary>
    ChargedPlasmaHorizontalShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaHorizontal"/> showing the Plasma Startup picture, right alternate, short.</summary>
    ChargedPlasmaHorizontalAlternateShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaHorizontal"/> showing the Plasma Startup picture, right, long.</summary>
    ChargedPlasmaHorizontalLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaHorizontal"/> showing the Plasma Startup picture, right alternate, long.</summary>
    ChargedPlasmaHorizontalAlternateLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaHorizontalFullLength"/> showing the Plasma Startup picture, right, full.</summary>
    ChargedPlasmaHorizontalFull,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaHorizontalFullLength"/> showing the Plasma Startup picture, right alternate, full.</summary>
    ChargedPlasmaHorizontalAlternateFull,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaFallingDiagonal"/> showing the Plasma Startup picture, down right, core.</summary>
    ChargedPlasmaFallingDiagonalCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaFallingDiagonal"/> showing the Plasma Startup picture, down right alternate, core.</summary>
    ChargedPlasmaFallingDiagonalAlternateCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaFallingDiagonal"/> showing the Plasma Startup picture, down right, short.</summary>
    ChargedPlasmaFallingDiagonalShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaFallingDiagonal"/> showing the Plasma Startup picture, down right alternate, short.</summary>
    ChargedPlasmaFallingDiagonalAlternateShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaFallingDiagonal"/> showing the Plasma Startup picture, down right, long.</summary>
    ChargedPlasmaFallingDiagonalLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaFallingDiagonal"/> showing the Plasma Startup picture, down right alternate, long.</summary>
    ChargedPlasmaFallingDiagonalAlternateLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaFallingDiagonalFullLength"/> showing the Plasma Startup picture, down right, full.</summary>
    ChargedPlasmaFallingDiagonalFull,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaFallingDiagonalFullLength"/> showing the Plasma Startup picture, down right alternate, full.</summary>
    ChargedPlasmaFallingDiagonalAlternateFull,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveVertical"/> showing the Plasma Startup picture, up, core.</summary>
    ChargedPlasmaWaveVerticalCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveVertical"/> showing the Plasma Startup picture, up alternate, core.</summary>
    ChargedPlasmaWaveVerticalAlternateCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveVertical"/> showing the Plasma Startup picture, up, short.</summary>
    ChargedPlasmaWaveVerticalShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveVertical"/> showing the Plasma Startup picture, up alternate, short.</summary>
    ChargedPlasmaWaveVerticalAlternateShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveVertical"/> showing the Plasma Startup picture, up, long.</summary>
    ChargedPlasmaWaveVerticalLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveVertical"/> showing the Plasma Startup picture, up alternate, long.</summary>
    ChargedPlasmaWaveVerticalAlternateLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveVerticalWaving"/> showing the Plasma Wave picture, up long, core.</summary>
    ChargedPlasmaWaveVerticalLongCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveVerticalWaving"/> showing the Plasma Wave picture, up alternate, core: the alternate of the long core it shows with, with its hitbox and trail placements.</summary>
    ChargedPlasmaWaveVerticalAlternateLongCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveVerticalWaving"/> showing the Plasma Wave picture, up long, its lobes near the centre.</summary>
    ChargedPlasmaWaveVerticalLongLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveVerticalWaving"/> showing the Plasma Wave picture, up alternate, its lobes near the centre.</summary>
    ChargedPlasmaWaveVerticalAlternateLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveVerticalWaving"/> showing the Plasma Wave picture, up long, its lobes midway out.</summary>
    ChargedPlasmaWaveVerticalLongLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveVerticalWaving"/> showing the Plasma Wave picture, up alternate, its lobes midway out.</summary>
    ChargedPlasmaWaveVerticalAlternateLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveVerticalWaving"/> showing the Plasma Wave picture, up long, its lobes far out.</summary>
    ChargedPlasmaWaveVerticalLongLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveVerticalWaving"/> showing the Plasma Wave picture, up alternate, its lobes far out.</summary>
    ChargedPlasmaWaveVerticalAlternateLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveVerticalWaving"/> showing the Plasma Wave picture, up long, its lobes at their peak.</summary>
    ChargedPlasmaWaveVerticalLongLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveVerticalWaving"/> showing the Plasma Wave picture, up alternate, its lobes at their peak.</summary>
    ChargedPlasmaWaveVerticalAlternateLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRisingDiagonal"/> showing the Plasma Startup picture, up right, core.</summary>
    ChargedPlasmaWaveRisingDiagonalCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRisingDiagonal"/> showing the Plasma Startup picture, up right alternate, core.</summary>
    ChargedPlasmaWaveRisingDiagonalAlternateCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRisingDiagonal"/> showing the Plasma Startup picture, up right, short.</summary>
    ChargedPlasmaWaveRisingDiagonalShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRisingDiagonal"/> showing the Plasma Startup picture, up right alternate, short.</summary>
    ChargedPlasmaWaveRisingDiagonalAlternateShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRisingDiagonal"/> showing the Plasma Startup picture, up right, long.</summary>
    ChargedPlasmaWaveRisingDiagonalLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRisingDiagonal"/> showing the Plasma Startup picture, up right alternate, long.</summary>
    ChargedPlasmaWaveRisingDiagonalAlternateLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRisingDiagonalWaving"/> showing the Plasma Wave picture, up right long, core.</summary>
    ChargedPlasmaWaveRisingDiagonalLongCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRisingDiagonalWaving"/> showing the Plasma Wave picture, up right alternate, core: the alternate of the long core it shows with, with its hitbox and trail placements.</summary>
    ChargedPlasmaWaveRisingDiagonalAlternateLongCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRisingDiagonalWaving"/> showing the Plasma Wave picture, up right long, its lobes near the centre.</summary>
    ChargedPlasmaWaveRisingDiagonalLongLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRisingDiagonalWaving"/> showing the Plasma Wave picture, up right alternate, its lobes near the centre.</summary>
    ChargedPlasmaWaveRisingDiagonalAlternateLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRisingDiagonalWaving"/> showing the Plasma Wave picture, up right long, its lobes midway out.</summary>
    ChargedPlasmaWaveRisingDiagonalLongLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRisingDiagonalWaving"/> showing the Plasma Wave picture, up right alternate, its lobes midway out.</summary>
    ChargedPlasmaWaveRisingDiagonalAlternateLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRisingDiagonalWaving"/> showing the Plasma Wave picture, up right long, its lobes far out.</summary>
    ChargedPlasmaWaveRisingDiagonalLongLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRisingDiagonalWaving"/> showing the Plasma Wave picture, up right alternate, its lobes far out.</summary>
    ChargedPlasmaWaveRisingDiagonalAlternateLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRisingDiagonalWaving"/> showing the Plasma Wave picture, up right long, its lobes at their peak.</summary>
    ChargedPlasmaWaveRisingDiagonalLongLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRisingDiagonalWaving"/> showing the Plasma Wave picture, up right alternate, its lobes at their peak.</summary>
    ChargedPlasmaWaveRisingDiagonalAlternateLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveHorizontal"/> showing the Plasma Startup picture, right, core.</summary>
    ChargedPlasmaWaveHorizontalCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveHorizontal"/> showing the Plasma Startup picture, right alternate, core.</summary>
    ChargedPlasmaWaveHorizontalAlternateCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveHorizontal"/> showing the Plasma Startup picture, right, short.</summary>
    ChargedPlasmaWaveHorizontalShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveHorizontal"/> showing the Plasma Startup picture, right alternate, short.</summary>
    ChargedPlasmaWaveHorizontalAlternateShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveHorizontal"/> showing the Plasma Startup picture, right, long.</summary>
    ChargedPlasmaWaveHorizontalLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveHorizontal"/> showing the Plasma Startup picture, right alternate, long.</summary>
    ChargedPlasmaWaveHorizontalAlternateLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveHorizontalWaving"/> showing the Plasma Wave picture, right long, core.</summary>
    ChargedPlasmaWaveHorizontalLongCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveHorizontalWaving"/> showing the Plasma Wave picture, right alternate, core: the alternate of the long core it shows with, with its hitbox and trail placements.</summary>
    ChargedPlasmaWaveHorizontalAlternateLongCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveHorizontalWaving"/> showing the Plasma Wave picture, right long, its lobes near the centre.</summary>
    ChargedPlasmaWaveHorizontalLongLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveHorizontalWaving"/> showing the Plasma Wave picture, right alternate, its lobes near the centre.</summary>
    ChargedPlasmaWaveHorizontalAlternateLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveHorizontalWaving"/> showing the Plasma Wave picture, right long, its lobes midway out.</summary>
    ChargedPlasmaWaveHorizontalLongLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveHorizontalWaving"/> showing the Plasma Wave picture, right alternate, its lobes midway out.</summary>
    ChargedPlasmaWaveHorizontalAlternateLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveHorizontalWaving"/> showing the Plasma Wave picture, right long, its lobes far out.</summary>
    ChargedPlasmaWaveHorizontalLongLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveHorizontalWaving"/> showing the Plasma Wave picture, right alternate, its lobes far out.</summary>
    ChargedPlasmaWaveHorizontalAlternateLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveHorizontalWaving"/> showing the Plasma Wave picture, right long, its lobes at their peak.</summary>
    ChargedPlasmaWaveHorizontalLongLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveHorizontalWaving"/> showing the Plasma Wave picture, right alternate, its lobes at their peak.</summary>
    ChargedPlasmaWaveHorizontalAlternateLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveFallingDiagonal"/> showing the Plasma Startup picture, down right, core.</summary>
    ChargedPlasmaWaveFallingDiagonalCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveFallingDiagonal"/> showing the Plasma Startup picture, down right alternate, core.</summary>
    ChargedPlasmaWaveFallingDiagonalAlternateCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveFallingDiagonal"/> showing the Plasma Startup picture, down right, short.</summary>
    ChargedPlasmaWaveFallingDiagonalShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveFallingDiagonal"/> showing the Plasma Startup picture, down right alternate, short.</summary>
    ChargedPlasmaWaveFallingDiagonalAlternateShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveFallingDiagonal"/> showing the Plasma Startup picture, down right, long.</summary>
    ChargedPlasmaWaveFallingDiagonalLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveFallingDiagonal"/> showing the Plasma Startup picture, down right alternate, long.</summary>
    ChargedPlasmaWaveFallingDiagonalAlternateLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveFallingDiagonalWaving"/> showing the Plasma Wave picture, down right long, core.</summary>
    ChargedPlasmaWaveFallingDiagonalLongCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveFallingDiagonalWaving"/> showing the Plasma Wave picture, down right alternate, core: the alternate of the long core it shows with, with its hitbox and trail placements.</summary>
    ChargedPlasmaWaveFallingDiagonalAlternateLongCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveFallingDiagonalWaving"/> showing the Plasma Wave picture, down right long, its lobes near the centre.</summary>
    ChargedPlasmaWaveFallingDiagonalLongLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveFallingDiagonalWaving"/> showing the Plasma Wave picture, down right alternate, its lobes near the centre.</summary>
    ChargedPlasmaWaveFallingDiagonalAlternateLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveFallingDiagonalWaving"/> showing the Plasma Wave picture, down right long, its lobes midway out.</summary>
    ChargedPlasmaWaveFallingDiagonalLongLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveFallingDiagonalWaving"/> showing the Plasma Wave picture, down right alternate, its lobes midway out.</summary>
    ChargedPlasmaWaveFallingDiagonalAlternateLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveFallingDiagonalWaving"/> showing the Plasma Wave picture, down right long, its lobes far out.</summary>
    ChargedPlasmaWaveFallingDiagonalLongLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveFallingDiagonalWaving"/> showing the Plasma Wave picture, down right alternate, its lobes far out.</summary>
    ChargedPlasmaWaveFallingDiagonalAlternateLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveFallingDiagonalWaving"/> showing the Plasma Wave picture, down right long, its lobes at their peak.</summary>
    ChargedPlasmaWaveFallingDiagonalLongLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveFallingDiagonalWaving"/> showing the Plasma Wave picture, down right alternate, its lobes at their peak.</summary>
    ChargedPlasmaWaveFallingDiagonalAlternateLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.MissileUp"/> showing the Missile picture, up.</summary>
    MissileUp,
    /// <summary>A frame of <see cref="SamusProjectileSegment.MissileUpRight"/> showing the Missile picture, up right.</summary>
    MissileUpRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.MissileRight"/> showing the Missile picture, right.</summary>
    MissileRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.MissileDownRight"/> showing the Missile picture, down right.</summary>
    MissileDownRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.MissileDown"/> showing the Missile picture, down.</summary>
    MissileDown,
    /// <summary>A frame of <see cref="SamusProjectileSegment.MissileDownLeft"/> showing the Missile picture, down left.</summary>
    MissileDownLeft,
    /// <summary>A frame of <see cref="SamusProjectileSegment.MissileLeft"/> showing the Missile picture, left.</summary>
    MissileLeft,
    /// <summary>A frame of <see cref="SamusProjectileSegment.MissileUpLeft"/> showing the Missile picture, up left.</summary>
    MissileUpLeft,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SuperMissileUp"/> showing the Super Missile picture, up.</summary>
    SuperMissileUp,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SuperMissileUpRight"/> showing the Super Missile picture, up right.</summary>
    SuperMissileUpRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SuperMissileRight"/> showing the Super Missile picture, right.</summary>
    SuperMissileRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SuperMissileDownRight"/> showing the Super Missile picture, down right.</summary>
    SuperMissileDownRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SuperMissileDown"/> showing the Super Missile picture, down.</summary>
    SuperMissileDown,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SuperMissileDownLeft"/> showing the Super Missile picture, down left.</summary>
    SuperMissileDownLeft,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SuperMissileLeft"/> showing the Super Missile picture, left.</summary>
    SuperMissileLeft,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SuperMissileUpLeft"/> showing the Super Missile picture, up left.</summary>
    SuperMissileUpLeft,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SuperMissileLink"/> showing nothing.</summary>
    SuperMissileLinkHidden,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PowerBomb"/> showing the Power Bomb picture, bright.</summary>
    PowerBombBright,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PowerBomb"/> showing the Power Bomb picture, dimming.</summary>
    PowerBombDimming,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PowerBomb"/> showing the Power Bomb picture, dark.</summary>
    PowerBombDark,
    /// <summary>A frame of <see cref="SamusProjectileSegment.FastPowerBomb"/> showing the Power Bomb picture, bright.</summary>
    FastPowerBombBright,
    /// <summary>A frame of <see cref="SamusProjectileSegment.FastPowerBomb"/> showing the Power Bomb picture, dimming.</summary>
    FastPowerBombDimming,
    /// <summary>A frame of <see cref="SamusProjectileSegment.FastPowerBomb"/> showing the Power Bomb picture, dark.</summary>
    FastPowerBombDark,
    /// <summary>A frame of <see cref="SamusProjectileSegment.Bomb"/> showing the Bomb picture, dark.</summary>
    BombDark,
    /// <summary>A frame of <see cref="SamusProjectileSegment.Bomb"/> showing the Bomb picture, smouldering.</summary>
    BombSmouldering,
    /// <summary>A frame of <see cref="SamusProjectileSegment.Bomb"/> showing the Bomb picture, glowing.</summary>
    BombGlowing,
    /// <summary>A frame of <see cref="SamusProjectileSegment.Bomb"/> showing the Bomb picture, flashing.</summary>
    BombFlashing,
    /// <summary>A frame of <see cref="SamusProjectileSegment.FastBomb"/> showing the Bomb picture, dark.</summary>
    FastBombDark,
    /// <summary>A frame of <see cref="SamusProjectileSegment.FastBomb"/> showing the Bomb picture, smouldering.</summary>
    FastBombSmouldering,
    /// <summary>A frame of <see cref="SamusProjectileSegment.FastBomb"/> showing the Bomb picture, glowing.</summary>
    FastBombGlowing,
    /// <summary>A frame of <see cref="SamusProjectileSegment.FastBomb"/> showing the Bomb picture, flashing.</summary>
    FastBombFlashing,
    /// <summary>A frame of <see cref="SamusProjectileSegment.BeamExplosion"/> showing the Beam Explosion picture, a speck.</summary>
    BeamExplosionSpeck,
    /// <summary>A frame of <see cref="SamusProjectileSegment.BeamExplosion"/> showing the Beam Explosion picture, a glint.</summary>
    BeamExplosionGlint,
    /// <summary>A frame of <see cref="SamusProjectileSegment.BeamExplosion"/> showing the Beam Explosion picture, a flash.</summary>
    BeamExplosionFlash,
    /// <summary>A frame of <see cref="SamusProjectileSegment.BeamExplosion"/> showing the Beam Explosion picture, a flash inside a ring.</summary>
    BeamExplosionRingedFlash,
    /// <summary>A frame of <see cref="SamusProjectileSegment.BeamExplosion"/> showing the Beam Explosion picture, a ring.</summary>
    BeamExplosionRing,
    /// <summary>A frame of <see cref="SamusProjectileSegment.BeamExplosion"/> showing the Beam Explosion picture, a fading ring.</summary>
    BeamExplosionFadingRing,
    /// <summary>A frame of <see cref="SamusProjectileSegment.MissileExplosion"/> showing the Missile Explosion picture, a spark.</summary>
    MissileExplosionSpark,
    /// <summary>A frame of <see cref="SamusProjectileSegment.MissileExplosion"/> showing the Missile Explosion picture, a ball.</summary>
    MissileExplosionBall,
    /// <summary>A frame of <see cref="SamusProjectileSegment.MissileExplosion"/> showing the Missile Explosion picture, a blast.</summary>
    MissileExplosionBlast,
    /// <summary>A frame of <see cref="SamusProjectileSegment.MissileExplosion"/> showing the Missile Explosion picture, a flare.</summary>
    MissileExplosionFlare,
    /// <summary>A frame of <see cref="SamusProjectileSegment.MissileExplosion"/> showing the Missile Explosion picture, a burst.</summary>
    MissileExplosionBurst,
    /// <summary>A frame of <see cref="SamusProjectileSegment.MissileExplosion"/> showing the Missile Explosion picture, embers.</summary>
    MissileExplosionEmbers,
    /// <summary>A frame of <see cref="SamusProjectileSegment.BombExplosion"/> showing the Bomb Explosion picture, an orb.</summary>
    BombExplosionOrb,
    /// <summary>A frame of <see cref="SamusProjectileSegment.BombExplosion"/> showing the Bomb Explosion picture, a sparkling orb.</summary>
    BombExplosionSparklingOrb,
    /// <summary>A frame of <see cref="SamusProjectileSegment.BombExplosion"/> showing the Bomb Explosion picture, a burst.</summary>
    BombExplosionBurst,
    /// <summary>A frame of <see cref="SamusProjectileSegment.BombExplosion"/> showing the Bomb Explosion picture, a wide burst.</summary>
    BombExplosionWideBurst,
    /// <summary>A frame of <see cref="SamusProjectileSegment.BombExplosion"/> showing the Bomb Explosion picture, a ring.</summary>
    BombExplosionRing,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaSpecialAttack"/> showing the Bomb Explosion picture, an orb.</summary>
    PlasmaSpecialAttackOrb,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaSpecialAttack"/> showing the Bomb Explosion picture, a sparkling orb.</summary>
    PlasmaSpecialAttackSparklingOrb,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaSpecialAttack"/> showing the Bomb Explosion picture, a burst.</summary>
    PlasmaSpecialAttackBurst,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaSpecialAttack"/> showing the Bomb Explosion picture, a wide burst.</summary>
    PlasmaSpecialAttackWideBurst,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaSpecialAttack"/> showing the Bomb Explosion picture, a ring.</summary>
    PlasmaSpecialAttackRing,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SuperMissileExplosion"/> showing the Super Missile Explosion picture, a ball.</summary>
    SuperMissileExplosionBall,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SuperMissileExplosion"/> showing the Super Missile Explosion picture, a blast.</summary>
    SuperMissileExplosionBlast,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SuperMissileExplosion"/> showing the Super Missile Explosion picture, a flare.</summary>
    SuperMissileExplosionFlare,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SuperMissileExplosion"/> showing the Super Missile Explosion picture, a ball inside a ring.</summary>
    SuperMissileExplosionRingedBall,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SuperMissileExplosion"/> showing the Super Missile Explosion picture, a ring.</summary>
    SuperMissileExplosionRing,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SuperMissileExplosion"/> showing the Super Missile Explosion picture, a fading ring.</summary>
    SuperMissileExplosionFadingRing,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ShinesparkEcho"/> showing nothing.</summary>
    ShinesparkEchoHidden,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerSpecialAttackTrail"/> showing nothing.</summary>
    SpazerSpecialAttackTrailHidden,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerSpecialAttackTrail"/> showing nothing, as <see cref="SpazerSpecialAttackTrailHidden"/> does, with a wider hitbox and its trail's sparkles placed further apart.</summary>
    SpazerSpecialAttackTrailHiddenWide,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerSpecialAttackTrail"/> showing nothing, as <see cref="SpazerSpecialAttackTrailHidden"/> does, with the widest hitbox and its trail's sparkles placed furthest apart.</summary>
    SpazerSpecialAttackTrailHiddenWidest,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveSpecialAttack"/> showing the Charged Wave picture, core.</summary>
    WaveSpecialAttackCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveSpecialAttack"/> showing the Charged Wave picture, core alternate.</summary>
    WaveSpecialAttackCoreAlternate,
}
