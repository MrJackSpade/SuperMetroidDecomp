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
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUp"/> showing the Spazer picture, up, its lanes parting, as <see cref="SpazerWaveUpParting"/> does, leaving no trail where <see cref="ProjectileTrailFamily.UnchargedDefault"/> places one from it.</summary>
    SpazerWaveUpPartingWithoutDefaultTrail,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUp"/> showing the Spazer picture, up, its lanes splitting, as <see cref="SpazerWaveUpSplitting"/> does, leaving no trail where <see cref="ProjectileTrailFamily.UnchargedDefault"/> places one from it.</summary>
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
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpRight"/> showing the Spazer picture, up right, its lanes parting, as <see cref="SpazerWaveUpRightParting"/> does, leaving no trail where <see cref="ProjectileTrailFamily.UnchargedDefault"/> places one from it.</summary>
    SpazerWaveUpRightPartingWithoutDefaultTrail,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpRight"/> showing the Spazer picture, up right, its lanes splitting, as <see cref="SpazerWaveUpRightSplitting"/> does, leaving no trail where <see cref="ProjectileTrailFamily.UnchargedDefault"/> places one from it.</summary>
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
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveRight"/> showing the Spazer picture, right, its lanes parting, as <see cref="SpazerWaveRightParting"/> does, leaving no trail where <see cref="ProjectileTrailFamily.UnchargedDefault"/> places one from it.</summary>
    SpazerWaveRightPartingWithoutDefaultTrail,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveRight"/> showing the Spazer picture, right, its lanes splitting, as <see cref="SpazerWaveRightSplitting"/> does, leaving no trail where <see cref="ProjectileTrailFamily.UnchargedDefault"/> places one from it.</summary>
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
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownRight"/> showing the Spazer picture, down right, its lanes parting, as <see cref="SpazerWaveDownRightParting"/> does, leaving no trail where <see cref="ProjectileTrailFamily.UnchargedDefault"/> places one from it.</summary>
    SpazerWaveDownRightPartingWithoutDefaultTrail,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownRight"/> showing the Spazer picture, down right, its lanes splitting, as <see cref="SpazerWaveDownRightSplitting"/> does, leaving no trail where <see cref="ProjectileTrailFamily.UnchargedDefault"/> places one from it.</summary>
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
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDown"/> showing the Spazer picture, down, its lanes parting, as <see cref="SpazerWaveDownParting"/> does, leaving no trail where <see cref="ProjectileTrailFamily.UnchargedDefault"/> places one from it.</summary>
    SpazerWaveDownPartingWithoutDefaultTrail,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDown"/> showing the Spazer picture, down, its lanes splitting, as <see cref="SpazerWaveDownSplitting"/> does, leaving no trail where <see cref="ProjectileTrailFamily.UnchargedDefault"/> places one from it.</summary>
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
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownLeft"/> showing the Spazer picture, down left, its lanes fully spread, as <see cref="SpazerWaveDownLeftSpread"/> does, with its trail placed elsewhere by <see cref="ProjectileTrailFamily.UnchargedWaveIceSpazer"/>, <see cref="ProjectileTrailFamily.UnchargedWaveIceSpazer"/>, <see cref="ProjectileTrailFamily.UnchargedWaveIceSpazer"/>.</summary>
    SpazerWaveDownLeftSpreadShiftedTrail,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownLeft"/> showing the Spazer picture, down left, its lanes spreading, as <see cref="SpazerWaveDownLeftSpreading"/> does, leaving no trail where <see cref="ProjectileTrailFamily.UnchargedDefault"/> places one from it.</summary>
    SpazerWaveDownLeftSpreadingWithoutDefaultTrail,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownLeft"/> showing the Spazer picture, down left, its lanes parting, as <see cref="SpazerWaveDownLeftParting"/> does, leaving no trail where <see cref="ProjectileTrailFamily.UnchargedDefault"/> places one from it.</summary>
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
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveLeft"/> showing the Spazer picture, left, its lanes parting, as <see cref="SpazerWaveLeftParting"/> does, leaving no trail where <see cref="ProjectileTrailFamily.UnchargedDefault"/> places one from it.</summary>
    SpazerWaveLeftPartingWithoutDefaultTrail,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveLeft"/> showing the Spazer picture, left, its lanes splitting, as <see cref="SpazerWaveLeftSplitting"/> does, leaving no trail where <see cref="ProjectileTrailFamily.UnchargedDefault"/> places one from it.</summary>
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
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpLeft"/> showing the Spazer picture, up left, its lanes parting, as <see cref="SpazerWaveUpLeftParting"/> does, leaving no trail where <see cref="ProjectileTrailFamily.UnchargedDefault"/> places one from it.</summary>
    SpazerWaveUpLeftPartingWithoutDefaultTrail,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpLeft"/> showing the Spazer picture, up left, its lanes splitting, as <see cref="SpazerWaveUpLeftSplitting"/> does, leaving no trail where <see cref="ProjectileTrailFamily.UnchargedDefault"/> places one from it.</summary>
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
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveVertical"/> showing the Plasma Wave picture, up short, its lobes near the centre, as <see cref="PlasmaWaveVerticalShortLobesNear"/> does, leaving no trail where <see cref="ProjectileTrailFamily.UnchargedDefault"/> places one from it.</summary>
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
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveRisingDiagonal"/> showing the Plasma Wave picture, up right short, its lobes near the centre, as <see cref="PlasmaWaveRisingDiagonalShortLobesNear"/> does, leaving no trail where <see cref="ProjectileTrailFamily.UnchargedDefault"/> places one from it.</summary>
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
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveHorizontalSwing"/> showing the Plasma Wave picture, right short, its lobes near the centre, as <see cref="PlasmaWaveHorizontalShortLobesNear"/> does, leaving no trail where <see cref="ProjectileTrailFamily.UnchargedDefault"/> places one from it.</summary>
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
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveFallingDiagonal"/> showing the Plasma Wave picture, down right short, its lobes near the centre, as <see cref="PlasmaWaveFallingDiagonalShortLobesNear"/> does, leaving no trail where <see cref="ProjectileTrailFamily.UnchargedDefault"/> places one from it.</summary>
    PlasmaWaveFallingDiagonalShortLobesNearWithoutDefaultTrail,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPowerUp"/> showing the Charged Power picture, up.</summary>
    ChargedPowerUp,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPowerUp"/> showing the Charged Power picture, up, its orb: the flicker's other picture to its starburst.</summary>
    ChargedPowerUpOrb,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPowerUpRight"/> showing the Charged Power picture, up right.</summary>
    ChargedPowerUpRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPowerUpRight"/> showing the Charged Power picture, up right, its orb: the flicker's other picture to its starburst.</summary>
    ChargedPowerUpRightOrb,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPowerRight"/> showing the Charged Power picture, right.</summary>
    ChargedPowerRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPowerRight"/> showing the Charged Power picture, right, its orb: the flicker's other picture to its starburst.</summary>
    ChargedPowerRightOrb,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPowerDownRight"/> showing the Charged Power picture, down right.</summary>
    ChargedPowerDownRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPowerDownRight"/> showing the Charged Power picture, down right, its orb: the flicker's other picture to its starburst.</summary>
    ChargedPowerDownRightOrb,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPowerDown"/> showing the Charged Power picture, down.</summary>
    ChargedPowerDown,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPowerDown"/> showing the Charged Power picture, down, its orb: the flicker's other picture to its starburst.</summary>
    ChargedPowerDownOrb,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPowerDownLeft"/> showing the Charged Power picture, down left.</summary>
    ChargedPowerDownLeft,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPowerDownLeft"/> showing the Charged Power picture, down left, its orb: the flicker's other picture to its starburst.</summary>
    ChargedPowerDownLeftOrb,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPowerLeft"/> showing the Charged Power picture, left.</summary>
    ChargedPowerLeft,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPowerLeft"/> showing the Charged Power picture, left, its orb: the flicker's other picture to its starburst.</summary>
    ChargedPowerLeftOrb,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPowerUpLeft"/> showing the Charged Power picture, up left.</summary>
    ChargedPowerUpLeft,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPowerUpLeft"/> showing the Charged Power picture, up left, its orb: the flicker's other picture to its starburst.</summary>
    ChargedPowerUpLeftOrb,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveUp"/> showing nothing.</summary>
    ChargedWaveUpHidden,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveVertical"/> showing the Charged Wave picture, core.</summary>
    ChargedWaveVerticalCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveVertical"/> showing the Charged Wave picture, core, its starburst: the flicker's other picture to its orb.</summary>
    ChargedWaveVerticalCoreStarburst,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveVertical"/> showing the Charged Wave picture, down, its lobes near the centre.</summary>
    ChargedWaveVerticalLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveVertical"/> showing the Charged Wave picture, down, its lobes near the centre, the orb on its right lobe and the starburst on its left: the flicker's other picture.</summary>
    ChargedWaveVerticalLobesNearOrbRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveVertical"/> showing the Charged Wave picture, down, its lobes midway out.</summary>
    ChargedWaveVerticalLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveVertical"/> showing the Charged Wave picture, down, its lobes midway out, the orb on its right lobe and the starburst on its left: the flicker's other picture.</summary>
    ChargedWaveVerticalLobesMidwayOrbRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveVertical"/> showing the Charged Wave picture, down, its lobes far out.</summary>
    ChargedWaveVerticalLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveVertical"/> showing the Charged Wave picture, down, its lobes far out, the orb on its right lobe and the starburst on its left: the flicker's other picture.</summary>
    ChargedWaveVerticalLobesFarOrbRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveVertical"/> showing the Charged Wave picture, down, its lobes at their peak.</summary>
    ChargedWaveVerticalLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveVertical"/> showing the Charged Wave picture, down, its lobes at their peak, the orb on its right lobe and the starburst on its left: the flicker's other picture.</summary>
    ChargedWaveVerticalLobesPeakOrbRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRisingDiagonal"/> showing the Charged Wave picture, core.</summary>
    ChargedWaveRisingDiagonalCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRisingDiagonal"/> showing the Charged Wave picture, core, its starburst: the flicker's other picture to its orb.</summary>
    ChargedWaveRisingDiagonalCoreStarburst,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRisingDiagonal"/> showing the Charged Wave picture, up right, its lobes near the centre.</summary>
    ChargedWaveRisingDiagonalLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRisingDiagonal"/> showing the Charged Wave picture, up right, its lobes near the centre, the orb on its lower lobe and the starburst on its upper: the flicker's other picture.</summary>
    ChargedWaveRisingDiagonalLobesNearOrbBelow,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRisingDiagonal"/> showing the Charged Wave picture, up right, its lobes midway out.</summary>
    ChargedWaveRisingDiagonalLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRisingDiagonal"/> showing the Charged Wave picture, up right, its lobes midway out, the orb on its lower lobe and the starburst on its upper: the flicker's other picture.</summary>
    ChargedWaveRisingDiagonalLobesMidwayOrbBelow,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRisingDiagonal"/> showing the Charged Wave picture, up right, its lobes far out.</summary>
    ChargedWaveRisingDiagonalLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRisingDiagonal"/> showing the Charged Wave picture, up right, its lobes far out, the orb on its lower lobe and the starburst on its upper: the flicker's other picture.</summary>
    ChargedWaveRisingDiagonalLobesFarOrbBelow,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRisingDiagonal"/> showing the Charged Wave picture, up right, its lobes at their peak.</summary>
    ChargedWaveRisingDiagonalLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRisingDiagonal"/> showing the Charged Wave picture, up right, its lobes at their peak, the orb on its lower lobe and the starburst on its upper: the flicker's other picture.</summary>
    ChargedWaveRisingDiagonalLobesPeakOrbBelow,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveHorizontal"/> showing the Charged Wave picture, core.</summary>
    ChargedWaveHorizontalCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveHorizontal"/> showing the Charged Wave picture, core, its starburst: the flicker's other picture to its orb.</summary>
    ChargedWaveHorizontalCoreStarburst,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveHorizontal"/> showing the Charged Wave picture, right, its lobes near the centre.</summary>
    ChargedWaveHorizontalLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveHorizontal"/> showing the Charged Wave picture, right, its lobes near the centre, the orb on its lower lobe and the starburst on its upper: the flicker's other picture.</summary>
    ChargedWaveHorizontalLobesNearOrbBelow,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveHorizontal"/> showing the Charged Wave picture, right, its lobes midway out.</summary>
    ChargedWaveHorizontalLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveHorizontal"/> showing the Charged Wave picture, right, its lobes midway out, the orb on its lower lobe and the starburst on its upper: the flicker's other picture.</summary>
    ChargedWaveHorizontalLobesMidwayOrbBelow,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveHorizontal"/> showing the Charged Wave picture, right, its lobes far out.</summary>
    ChargedWaveHorizontalLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveHorizontal"/> showing the Charged Wave picture, right, its lobes far out, the orb on its lower lobe and the starburst on its upper: the flicker's other picture.</summary>
    ChargedWaveHorizontalLobesFarOrbBelow,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveHorizontal"/> showing the Charged Wave picture, right, its lobes at their peak.</summary>
    ChargedWaveHorizontalLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveHorizontal"/> showing the Charged Wave picture, right, its lobes at their peak, the orb on its lower lobe and the starburst on its upper: the flicker's other picture.</summary>
    ChargedWaveHorizontalLobesPeakOrbBelow,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveFallingDiagonal"/> showing the Charged Wave picture, core.</summary>
    ChargedWaveFallingDiagonalCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveFallingDiagonal"/> showing the Charged Wave picture, core, its starburst: the flicker's other picture to its orb.</summary>
    ChargedWaveFallingDiagonalCoreStarburst,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveFallingDiagonal"/> showing the Charged Wave picture, down right, its lobes near the centre.</summary>
    ChargedWaveFallingDiagonalLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveFallingDiagonal"/> showing the Charged Wave picture, down right, its lobes near the centre, the orb on its lower lobe and the starburst on its upper: the flicker's other picture.</summary>
    ChargedWaveFallingDiagonalLobesNearOrbBelow,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveFallingDiagonal"/> showing the Charged Wave picture, down right, its lobes midway out.</summary>
    ChargedWaveFallingDiagonalLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveFallingDiagonal"/> showing the Charged Wave picture, down right, its lobes midway out, the orb on its lower lobe and the starburst on its upper: the flicker's other picture.</summary>
    ChargedWaveFallingDiagonalLobesMidwayOrbBelow,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveFallingDiagonal"/> showing the Charged Wave picture, down right, its lobes far out.</summary>
    ChargedWaveFallingDiagonalLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveFallingDiagonal"/> showing the Charged Wave picture, down right, its lobes far out, the orb on its lower lobe and the starburst on its upper: the flicker's other picture.</summary>
    ChargedWaveFallingDiagonalLobesFarOrbBelow,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveFallingDiagonal"/> showing the Charged Wave picture, down right, its lobes at their peak.</summary>
    ChargedWaveFallingDiagonalLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveFallingDiagonal"/> showing the Charged Wave picture, down right, its lobes at their peak, the orb on its lower lobe and the starburst on its upper: the flicker's other picture.</summary>
    ChargedWaveFallingDiagonalLobesPeakOrbBelow,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIce"/> showing the Charged Ice picture, a cross.</summary>
    ChargedIceCrossed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIce"/> showing the Charged Ice picture, a star.</summary>
    ChargedIceStar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveUp"/> showing nothing.</summary>
    ChargedIceWaveUpHidden,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveVertical"/> showing the Charged Ice Wave picture, core.</summary>
    ChargedIceWaveVerticalCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveVertical"/> showing the Charged Ice Wave picture, core, its starburst: the flicker's other picture to its orb.</summary>
    ChargedIceWaveVerticalCoreStarburst,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveVertical"/> showing the Charged Ice Wave picture, down, its lobes near the centre.</summary>
    ChargedIceWaveVerticalLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveVertical"/> showing the Charged Ice Wave picture, down, its lobes near the centre, the orb on its right lobe and the starburst on its left: the flicker's other picture.</summary>
    ChargedIceWaveVerticalLobesNearOrbRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveVertical"/> showing the Charged Ice Wave picture, down, its lobes midway out.</summary>
    ChargedIceWaveVerticalLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveVertical"/> showing the Charged Ice Wave picture, down, its lobes midway out, the orb on its right lobe and the starburst on its left: the flicker's other picture.</summary>
    ChargedIceWaveVerticalLobesMidwayOrbRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveVertical"/> showing the Charged Ice Wave picture, down, its lobes far out.</summary>
    ChargedIceWaveVerticalLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveVertical"/> showing the Charged Ice Wave picture, down, its lobes far out, the orb on its right lobe and the starburst on its left: the flicker's other picture.</summary>
    ChargedIceWaveVerticalLobesFarOrbRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveVertical"/> showing the Charged Ice Wave picture, down, its lobes at their peak.</summary>
    ChargedIceWaveVerticalLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveVertical"/> showing the Charged Ice Wave picture, down, its lobes at their peak, the orb on its right lobe and the starburst on its left: the flicker's other picture.</summary>
    ChargedIceWaveVerticalLobesPeakOrbRight,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRisingDiagonal"/> showing the Charged Ice Wave picture, core.</summary>
    ChargedIceWaveRisingDiagonalCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRisingDiagonal"/> showing the Charged Ice Wave picture, core, its starburst: the flicker's other picture to its orb.</summary>
    ChargedIceWaveRisingDiagonalCoreStarburst,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRisingDiagonal"/> showing the Charged Ice Wave picture, up right, its lobes near the centre.</summary>
    ChargedIceWaveRisingDiagonalLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRisingDiagonal"/> showing the Charged Ice Wave picture, up right, its lobes near the centre, the orb on its lower lobe and the starburst on its upper: the flicker's other picture.</summary>
    ChargedIceWaveRisingDiagonalLobesNearOrbBelow,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRisingDiagonal"/> showing the Charged Ice Wave picture, up right, its lobes midway out.</summary>
    ChargedIceWaveRisingDiagonalLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRisingDiagonal"/> showing the Charged Ice Wave picture, up right, its lobes midway out, the orb on its lower lobe and the starburst on its upper: the flicker's other picture.</summary>
    ChargedIceWaveRisingDiagonalLobesMidwayOrbBelow,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRisingDiagonal"/> showing the Charged Ice Wave picture, up right, its lobes far out.</summary>
    ChargedIceWaveRisingDiagonalLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRisingDiagonal"/> showing the Charged Ice Wave picture, up right, its lobes far out, the orb on its lower lobe and the starburst on its upper: the flicker's other picture.</summary>
    ChargedIceWaveRisingDiagonalLobesFarOrbBelow,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRisingDiagonal"/> showing the Charged Ice Wave picture, up right, its lobes at their peak.</summary>
    ChargedIceWaveRisingDiagonalLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRisingDiagonal"/> showing the Charged Ice Wave picture, up right, its lobes at their peak, the orb on its lower lobe and the starburst on its upper: the flicker's other picture.</summary>
    ChargedIceWaveRisingDiagonalLobesPeakOrbBelow,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveHorizontal"/> showing the Charged Ice Wave picture, core.</summary>
    ChargedIceWaveHorizontalCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveHorizontal"/> showing the Charged Ice Wave picture, core, its starburst: the flicker's other picture to its orb.</summary>
    ChargedIceWaveHorizontalCoreStarburst,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveHorizontal"/> showing the Charged Ice Wave picture, right, its lobes near the centre.</summary>
    ChargedIceWaveHorizontalLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveHorizontal"/> showing the Charged Ice Wave picture, right, its lobes near the centre, the orb on its lower lobe and the starburst on its upper: the flicker's other picture.</summary>
    ChargedIceWaveHorizontalLobesNearOrbBelow,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveHorizontal"/> showing the Charged Ice Wave picture, right, its lobes midway out.</summary>
    ChargedIceWaveHorizontalLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveHorizontal"/> showing the Charged Ice Wave picture, right, its lobes midway out, the orb on its lower lobe and the starburst on its upper: the flicker's other picture.</summary>
    ChargedIceWaveHorizontalLobesMidwayOrbBelow,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveHorizontal"/> showing the Charged Ice Wave picture, right, its lobes far out.</summary>
    ChargedIceWaveHorizontalLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveHorizontal"/> showing the Charged Ice Wave picture, right, its lobes far out, the orb on its lower lobe and the starburst on its upper: the flicker's other picture.</summary>
    ChargedIceWaveHorizontalLobesFarOrbBelow,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveHorizontal"/> showing the Charged Ice Wave picture, right, its lobes at their peak.</summary>
    ChargedIceWaveHorizontalLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveHorizontal"/> showing the Charged Ice Wave picture, right, its lobes at their peak, the orb on its lower lobe and the starburst on its upper: the flicker's other picture.</summary>
    ChargedIceWaveHorizontalLobesPeakOrbBelow,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveFallingDiagonal"/> showing the Charged Ice Wave picture, core.</summary>
    ChargedIceWaveFallingDiagonalCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveFallingDiagonal"/> showing the Charged Ice Wave picture, core, its starburst: the flicker's other picture to its orb.</summary>
    ChargedIceWaveFallingDiagonalCoreStarburst,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveFallingDiagonal"/> showing the Charged Ice Wave picture, down right, its lobes near the centre.</summary>
    ChargedIceWaveFallingDiagonalLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveFallingDiagonal"/> showing the Charged Ice Wave picture, down right, its lobes near the centre, the orb on its lower lobe and the starburst on its upper: the flicker's other picture.</summary>
    ChargedIceWaveFallingDiagonalLobesNearOrbBelow,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveFallingDiagonal"/> showing the Charged Ice Wave picture, down right, its lobes midway out.</summary>
    ChargedIceWaveFallingDiagonalLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveFallingDiagonal"/> showing the Charged Ice Wave picture, down right, its lobes midway out, the orb on its lower lobe and the starburst on its upper: the flicker's other picture.</summary>
    ChargedIceWaveFallingDiagonalLobesMidwayOrbBelow,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveFallingDiagonal"/> showing the Charged Ice Wave picture, down right, its lobes far out.</summary>
    ChargedIceWaveFallingDiagonalLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveFallingDiagonal"/> showing the Charged Ice Wave picture, down right, its lobes far out, the orb on its lower lobe and the starburst on its upper: the flicker's other picture.</summary>
    ChargedIceWaveFallingDiagonalLobesFarOrbBelow,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveFallingDiagonal"/> showing the Charged Ice Wave picture, down right, its lobes at their peak.</summary>
    ChargedIceWaveFallingDiagonalLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveFallingDiagonal"/> showing the Charged Ice Wave picture, down right, its lobes at their peak, the orb on its lower lobe and the starburst on its upper: the flicker's other picture.</summary>
    ChargedIceWaveFallingDiagonalLobesPeakOrbBelow,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerVertical"/> showing the Spazer Startup picture, up, a short startup strip.</summary>
    ChargedSpazerVerticalStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerVertical"/> showing the Spazer Startup picture, vertical, its thick beam, a short startup strip.</summary>
    ChargedSpazerVerticalThickStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerVertical"/> showing the Spazer Startup picture, up, a long startup strip.</summary>
    ChargedSpazerVerticalStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerVertical"/> showing the Spazer Startup picture, vertical, its thick beam, a long startup strip.</summary>
    ChargedSpazerVerticalThickStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerVertical"/> showing the Charged Spazer picture, up, seed.</summary>
    ChargedSpazerVerticalSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerVertical"/> showing the Charged Spazer picture, vertical, its thick beam, seed.</summary>
    ChargedSpazerVerticalThickSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerVertical"/> showing the Charged Spazer picture, up, its lanes parting.</summary>
    ChargedSpazerVerticalParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerVertical"/> showing the Charged Spazer picture, vertical, its thick beam, its lanes parting.</summary>
    ChargedSpazerVerticalThickParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerVerticalSpread"/> showing the Charged Spazer picture, up, its lanes fully spread.</summary>
    ChargedSpazerVerticalSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerVerticalSpread"/> showing the Charged Spazer picture, vertical, its thick beam, its lanes fully spread.</summary>
    ChargedSpazerVerticalThickSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerRisingDiagonal"/> showing the Spazer Startup picture, up right, a short startup strip.</summary>
    ChargedSpazerRisingDiagonalStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerRisingDiagonal"/> showing the Spazer Startup picture, up right, its thick beam, a short startup strip.</summary>
    ChargedSpazerRisingDiagonalThickStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerRisingDiagonal"/> showing the Spazer Startup picture, up right, a long startup strip.</summary>
    ChargedSpazerRisingDiagonalStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerRisingDiagonal"/> showing the Spazer Startup picture, up right, its thick beam, a long startup strip.</summary>
    ChargedSpazerRisingDiagonalThickStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerRisingDiagonal"/> showing the Charged Spazer picture, up right, seed.</summary>
    ChargedSpazerRisingDiagonalSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerRisingDiagonal"/> showing the Charged Spazer picture, up right, its thick beam, seed.</summary>
    ChargedSpazerRisingDiagonalThickSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerRisingDiagonal"/> showing the Charged Spazer picture, up right, its lanes parting.</summary>
    ChargedSpazerRisingDiagonalParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerRisingDiagonal"/> showing the Charged Spazer picture, up right, its thick beam, its lanes parting.</summary>
    ChargedSpazerRisingDiagonalThickParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerRisingDiagonalSpread"/> showing the Charged Spazer picture, up right, its lanes fully spread.</summary>
    ChargedSpazerRisingDiagonalSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerRisingDiagonalSpread"/> showing the Charged Spazer picture, up right, its thick beam, its lanes fully spread.</summary>
    ChargedSpazerRisingDiagonalThickSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerHorizontal"/> showing the Spazer Startup picture, right, a short startup strip.</summary>
    ChargedSpazerHorizontalStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerHorizontal"/> showing the Spazer Startup picture, horizontal, its thick beam, a short startup strip.</summary>
    ChargedSpazerHorizontalThickStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerHorizontal"/> showing the Spazer Startup picture, right, a long startup strip.</summary>
    ChargedSpazerHorizontalStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerHorizontal"/> showing the Spazer Startup picture, horizontal, its thick beam, a long startup strip.</summary>
    ChargedSpazerHorizontalThickStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerHorizontal"/> showing the Charged Spazer picture, left, seed.</summary>
    ChargedSpazerHorizontalSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerHorizontal"/> showing the Charged Spazer picture, horizontal, its thick beam, seed.</summary>
    ChargedSpazerHorizontalThickSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerHorizontal"/> showing the Charged Spazer picture, left, its lanes parting.</summary>
    ChargedSpazerHorizontalParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerHorizontal"/> showing the Charged Spazer picture, horizontal, its thick beam, its lanes parting.</summary>
    ChargedSpazerHorizontalThickParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerHorizontalSpread"/> showing the Charged Spazer picture, left, its lanes fully spread.</summary>
    ChargedSpazerHorizontalSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerHorizontalSpread"/> showing the Charged Spazer picture, horizontal, its thick beam, its lanes fully spread.</summary>
    ChargedSpazerHorizontalThickSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerFallingDiagonal"/> showing the Spazer Startup picture, down right, a short startup strip.</summary>
    ChargedSpazerFallingDiagonalStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerFallingDiagonal"/> showing the Spazer Startup picture, down right, its thick beam, a short startup strip.</summary>
    ChargedSpazerFallingDiagonalThickStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerFallingDiagonal"/> showing the Spazer Startup picture, down right, a long startup strip.</summary>
    ChargedSpazerFallingDiagonalStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerFallingDiagonal"/> showing the Spazer Startup picture, down right, its thick beam, a long startup strip.</summary>
    ChargedSpazerFallingDiagonalThickStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerFallingDiagonal"/> showing the Charged Spazer picture, down right, seed.</summary>
    ChargedSpazerFallingDiagonalSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerFallingDiagonal"/> showing the Charged Spazer picture, down right, its thick beam, seed.</summary>
    ChargedSpazerFallingDiagonalThickSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerFallingDiagonal"/> showing the Charged Spazer picture, down right, its lanes parting.</summary>
    ChargedSpazerFallingDiagonalParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerFallingDiagonal"/> showing the Charged Spazer picture, down right, its thick beam, its lanes parting.</summary>
    ChargedSpazerFallingDiagonalThickParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerFallingDiagonalSpread"/> showing the Charged Spazer picture, down right, its lanes fully spread.</summary>
    ChargedSpazerFallingDiagonalSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerFallingDiagonalSpread"/> showing the Charged Spazer picture, down right, its thick beam, its lanes fully spread.</summary>
    ChargedSpazerFallingDiagonalThickSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUp"/> showing the Spazer Startup picture, up, a short startup strip.</summary>
    ChargedSpazerWaveUpStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUp"/> showing the Spazer Startup picture, vertical, its thick beam, a short startup strip.</summary>
    ChargedSpazerWaveUpVerticalThickStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUp"/> showing the Spazer Startup picture, up, a long startup strip.</summary>
    ChargedSpazerWaveUpStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUp"/> showing the Spazer Startup picture, vertical, its thick beam, a long startup strip.</summary>
    ChargedSpazerWaveUpVerticalThickStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpWaving"/> showing the Charged Spazer picture, up, seed.</summary>
    ChargedSpazerWaveUpSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpWaving"/> showing the Charged Spazer picture, vertical, its thick beam, seed.</summary>
    ChargedSpazerWaveUpVerticalThickSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpWaving"/> showing the Charged Spazer picture, up, its lanes splitting.</summary>
    ChargedSpazerWaveUpSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpWaving"/> showing the Charged Spazer picture, vertical, its thick beam, its lanes splitting.</summary>
    ChargedSpazerWaveUpVerticalThickSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpWaving"/> showing the Charged Spazer picture, up, its lanes parting.</summary>
    ChargedSpazerWaveUpParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpWaving"/> showing the Charged Spazer picture, vertical, its thick beam, its lanes parting.</summary>
    ChargedSpazerWaveUpVerticalThickParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpWaving"/> showing the Charged Spazer picture, up, its lanes spreading.</summary>
    ChargedSpazerWaveUpSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpWaving"/> showing the Charged Spazer picture, vertical, its thick beam, its lanes spreading.</summary>
    ChargedSpazerWaveUpVerticalThickSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpWaving"/> showing the Charged Spazer picture, up, its lanes nearly spread.</summary>
    ChargedSpazerWaveUpNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpWaving"/> showing the Charged Spazer picture, vertical, its thick beam, its lanes nearly spread.</summary>
    ChargedSpazerWaveUpVerticalThickNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpWaving"/> showing the Charged Spazer picture, up, its lanes fully spread.</summary>
    ChargedSpazerWaveUpSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpWaving"/> showing the Charged Spazer picture, vertical, its thick beam, its lanes fully spread.</summary>
    ChargedSpazerWaveUpVerticalThickSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpWaving"/> showing the Charged Spazer picture, up, its lanes splitting, as <see cref="ChargedSpazerWaveUpSplitting"/> does, with a narrower hitbox and leaving no trail.</summary>
    ChargedSpazerWaveUpSplittingNarrowHitboxTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpWaving"/> showing the Charged Spazer picture, vertical, its thick beam, its lanes splitting, as <see cref="ChargedSpazerWaveUpVerticalThickSplitting"/> does, with a narrower hitbox and leaving no trail.</summary>
    ChargedSpazerWaveUpVerticalThickSplittingNarrowHitboxTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRight"/> showing the Spazer Startup picture, up right, a short startup strip.</summary>
    ChargedSpazerWaveUpRightStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRight"/> showing the Spazer Startup picture, up right, its thick beam, a short startup strip.</summary>
    ChargedSpazerWaveUpRightThickStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRight"/> showing the Spazer Startup picture, up right, a long startup strip.</summary>
    ChargedSpazerWaveUpRightStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRight"/> showing the Spazer Startup picture, up right, its thick beam, a long startup strip.</summary>
    ChargedSpazerWaveUpRightThickStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightWaving"/> showing the Charged Spazer picture, up right, seed.</summary>
    ChargedSpazerWaveUpRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightWaving"/> showing the Charged Spazer picture, up right, its thick beam, seed.</summary>
    ChargedSpazerWaveUpRightThickSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightWaving"/> showing the Charged Spazer picture, up right, its lanes splitting.</summary>
    ChargedSpazerWaveUpRightSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightWaving"/> showing the Charged Spazer picture, up right, its thick beam, its lanes splitting.</summary>
    ChargedSpazerWaveUpRightThickSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightWaving"/> showing the Charged Spazer picture, up right, its lanes parting.</summary>
    ChargedSpazerWaveUpRightParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightWaving"/> showing the Charged Spazer picture, up right, its thick beam, its lanes parting.</summary>
    ChargedSpazerWaveUpRightThickParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightWaving"/> showing the Charged Spazer picture, up right, its lanes spreading.</summary>
    ChargedSpazerWaveUpRightSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightWaving"/> showing the Charged Spazer picture, up right, its thick beam, its lanes spreading.</summary>
    ChargedSpazerWaveUpRightThickSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightWaving"/> showing the Charged Spazer picture, up right, its lanes nearly spread.</summary>
    ChargedSpazerWaveUpRightNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightWaving"/> showing the Charged Spazer picture, up right, its thick beam, its lanes nearly spread.</summary>
    ChargedSpazerWaveUpRightThickNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightWaving"/> showing the Charged Spazer picture, up right, its lanes fully spread.</summary>
    ChargedSpazerWaveUpRightSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightWaving"/> showing the Charged Spazer picture, up right, its thick beam, its lanes fully spread.</summary>
    ChargedSpazerWaveUpRightThickSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightWaving"/> showing the Charged Spazer picture, up right, its lanes splitting, as <see cref="ChargedSpazerWaveUpRightSplitting"/> does, leaving no trail.</summary>
    ChargedSpazerWaveUpRightSplittingTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightWaving"/> showing the Charged Spazer picture, up right, its thick beam, its lanes splitting, as <see cref="ChargedSpazerWaveUpRightThickSplitting"/> does, leaving no trail.</summary>
    ChargedSpazerWaveUpRightThickSplittingTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRight"/> showing the Spazer Startup picture, right, a short startup strip.</summary>
    ChargedSpazerWaveRightStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRight"/> showing the Spazer Startup picture, horizontal, its thick beam, a short startup strip.</summary>
    ChargedSpazerWaveRightHorizontalThickStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRight"/> showing the Spazer Startup picture, right, a long startup strip.</summary>
    ChargedSpazerWaveRightStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRight"/> showing the Spazer Startup picture, horizontal, its thick beam, a long startup strip.</summary>
    ChargedSpazerWaveRightHorizontalThickStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightWaving"/> showing the Charged Spazer picture, right, seed.</summary>
    ChargedSpazerWaveRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightWaving"/> showing the Charged Spazer picture, horizontal, its thick beam, seed.</summary>
    ChargedSpazerWaveRightHorizontalThickSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightWaving"/> showing the Charged Spazer picture, right, its lanes splitting.</summary>
    ChargedSpazerWaveRightSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightWaving"/> showing the Charged Spazer picture, horizontal, its thick beam, its lanes splitting.</summary>
    ChargedSpazerWaveRightHorizontalThickSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightWaving"/> showing the Charged Spazer picture, right, its lanes parting.</summary>
    ChargedSpazerWaveRightParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightWaving"/> showing the Charged Spazer picture, horizontal, its thick beam, its lanes parting.</summary>
    ChargedSpazerWaveRightHorizontalThickParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightWaving"/> showing the Charged Spazer picture, right, its lanes spreading.</summary>
    ChargedSpazerWaveRightSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightWaving"/> showing the Charged Spazer picture, horizontal, its thick beam, its lanes spreading.</summary>
    ChargedSpazerWaveRightHorizontalThickSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightWaving"/> showing the Charged Spazer picture, right, its lanes nearly spread.</summary>
    ChargedSpazerWaveRightNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightWaving"/> showing the Charged Spazer picture, horizontal, its thick beam, its lanes nearly spread.</summary>
    ChargedSpazerWaveRightHorizontalThickNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightWaving"/> showing the Charged Spazer picture, right, its lanes fully spread.</summary>
    ChargedSpazerWaveRightSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightWaving"/> showing the Charged Spazer picture, horizontal, its thick beam, its lanes fully spread.</summary>
    ChargedSpazerWaveRightHorizontalThickSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightWaving"/> showing the Charged Spazer picture, right, its lanes splitting, as <see cref="ChargedSpazerWaveRightSplitting"/> does, with a narrower hitbox and leaving no trail.</summary>
    ChargedSpazerWaveRightSplittingNarrowHitboxTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightWaving"/> showing the Charged Spazer picture, horizontal, its thick beam, its lanes splitting, as <see cref="ChargedSpazerWaveRightHorizontalThickSplitting"/> does, with a narrower hitbox and leaving no trail.</summary>
    ChargedSpazerWaveRightHorizontalThickSplittingNarrowHitboxTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRight"/> showing the Spazer Startup picture, down right, a short startup strip.</summary>
    ChargedSpazerWaveDownRightStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRight"/> showing the Spazer Startup picture, down right, its thick beam, a short startup strip.</summary>
    ChargedSpazerWaveDownRightThickStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRight"/> showing the Spazer Startup picture, down right, a long startup strip.</summary>
    ChargedSpazerWaveDownRightStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRight"/> showing the Spazer Startup picture, down right, its thick beam, a long startup strip.</summary>
    ChargedSpazerWaveDownRightThickStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightWaving"/> showing the Charged Spazer picture, down right, seed.</summary>
    ChargedSpazerWaveDownRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightWaving"/> showing the Charged Spazer picture, down right, its thick beam, seed.</summary>
    ChargedSpazerWaveDownRightThickSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightWaving"/> showing the Charged Spazer picture, down right, its lanes splitting.</summary>
    ChargedSpazerWaveDownRightSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightWaving"/> showing the Charged Spazer picture, down right, its thick beam, its lanes splitting.</summary>
    ChargedSpazerWaveDownRightThickSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightWaving"/> showing the Charged Spazer picture, down right, its lanes parting.</summary>
    ChargedSpazerWaveDownRightParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightWaving"/> showing the Charged Spazer picture, down right, its thick beam, its lanes parting.</summary>
    ChargedSpazerWaveDownRightThickParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightWaving"/> showing the Charged Spazer picture, down right, its lanes spreading.</summary>
    ChargedSpazerWaveDownRightSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightWaving"/> showing the Charged Spazer picture, down right, its thick beam, its lanes spreading.</summary>
    ChargedSpazerWaveDownRightThickSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightWaving"/> showing the Charged Spazer picture, down right, its lanes nearly spread.</summary>
    ChargedSpazerWaveDownRightNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightWaving"/> showing the Charged Spazer picture, down right, its thick beam, its lanes nearly spread.</summary>
    ChargedSpazerWaveDownRightThickNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightWaving"/> showing the Charged Spazer picture, down right, its lanes fully spread.</summary>
    ChargedSpazerWaveDownRightSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightWaving"/> showing the Charged Spazer picture, down right, its thick beam, its lanes fully spread.</summary>
    ChargedSpazerWaveDownRightThickSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightWaving"/> showing the Charged Spazer picture, down right, its lanes splitting, as <see cref="ChargedSpazerWaveDownRightSplitting"/> does, leaving no trail.</summary>
    ChargedSpazerWaveDownRightSplittingTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightWaving"/> showing the Charged Spazer picture, down right, its thick beam, its lanes splitting, as <see cref="ChargedSpazerWaveDownRightThickSplitting"/> does, leaving no trail.</summary>
    ChargedSpazerWaveDownRightThickSplittingTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDown"/> showing the Spazer Startup picture, down, a short startup strip.</summary>
    ChargedSpazerWaveDownStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDown"/> showing the Spazer Startup picture, vertical, its thick beam, a short startup strip.</summary>
    ChargedSpazerWaveDownVerticalThickStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDown"/> showing the Spazer Startup picture, down, a long startup strip.</summary>
    ChargedSpazerWaveDownStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDown"/> showing the Spazer Startup picture, vertical, its thick beam, a long startup strip.</summary>
    ChargedSpazerWaveDownVerticalThickStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownWaving"/> showing the Charged Spazer picture, down, seed.</summary>
    ChargedSpazerWaveDownSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownWaving"/> showing the Charged Spazer picture, vertical, its thick beam, seed.</summary>
    ChargedSpazerWaveDownVerticalThickSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownWaving"/> showing the Charged Spazer picture, down, its lanes splitting.</summary>
    ChargedSpazerWaveDownSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownWaving"/> showing the Charged Spazer picture, vertical, its thick beam, its lanes splitting.</summary>
    ChargedSpazerWaveDownVerticalThickSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownWaving"/> showing the Charged Spazer picture, down, its lanes parting.</summary>
    ChargedSpazerWaveDownParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownWaving"/> showing the Charged Spazer picture, vertical, its thick beam, its lanes parting.</summary>
    ChargedSpazerWaveDownVerticalThickParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownWaving"/> showing the Charged Spazer picture, down, its lanes spreading.</summary>
    ChargedSpazerWaveDownSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownWaving"/> showing the Charged Spazer picture, vertical, its thick beam, its lanes spreading.</summary>
    ChargedSpazerWaveDownVerticalThickSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownWaving"/> showing the Charged Spazer picture, down, its lanes nearly spread.</summary>
    ChargedSpazerWaveDownNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownWaving"/> showing the Charged Spazer picture, vertical, its thick beam, its lanes nearly spread.</summary>
    ChargedSpazerWaveDownVerticalThickNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownWaving"/> showing the Charged Spazer picture, down, its lanes fully spread.</summary>
    ChargedSpazerWaveDownSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownWaving"/> showing the Charged Spazer picture, vertical, its thick beam, its lanes fully spread.</summary>
    ChargedSpazerWaveDownVerticalThickSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownWaving"/> showing the Charged Spazer picture, down, its lanes splitting, as <see cref="ChargedSpazerWaveDownSplitting"/> does, with a narrower hitbox and leaving no trail.</summary>
    ChargedSpazerWaveDownSplittingNarrowHitboxTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownWaving"/> showing the Charged Spazer picture, vertical, its thick beam, its lanes splitting, as <see cref="ChargedSpazerWaveDownVerticalThickSplitting"/> does, with a narrower hitbox and leaving no trail.</summary>
    ChargedSpazerWaveDownVerticalThickSplittingNarrowHitboxTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeft"/> showing the Spazer Startup picture, up right, a short startup strip.</summary>
    ChargedSpazerWaveDownLeftUpRightStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeft"/> showing the Spazer Startup picture, up right, its thick beam, a short startup strip.</summary>
    ChargedSpazerWaveDownLeftUpRightThickStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeft"/> showing the Spazer Startup picture, up right, a long startup strip.</summary>
    ChargedSpazerWaveDownLeftUpRightStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeft"/> showing the Spazer Startup picture, up right, its thick beam, a long startup strip.</summary>
    ChargedSpazerWaveDownLeftUpRightThickStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftWaving"/> showing the Charged Spazer picture, down left, seed.</summary>
    ChargedSpazerWaveDownLeftSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftWaving"/> showing the Charged Spazer picture, up right, its thick beam, seed.</summary>
    ChargedSpazerWaveDownLeftUpRightThickSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftWaving"/> showing the Charged Spazer picture, down left, its lanes splitting.</summary>
    ChargedSpazerWaveDownLeftSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftWaving"/> showing the Charged Spazer picture, up right, its thick beam, its lanes splitting.</summary>
    ChargedSpazerWaveDownLeftUpRightThickSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftWaving"/> showing the Charged Spazer picture, down left, its lanes parting.</summary>
    ChargedSpazerWaveDownLeftParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftWaving"/> showing the Charged Spazer picture, up right, its thick beam, its lanes parting.</summary>
    ChargedSpazerWaveDownLeftUpRightThickParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftWaving"/> showing the Charged Spazer picture, down left, its lanes spreading.</summary>
    ChargedSpazerWaveDownLeftSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftWaving"/> showing the Charged Spazer picture, up right, its thick beam, its lanes spreading.</summary>
    ChargedSpazerWaveDownLeftUpRightThickSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftWaving"/> showing the Charged Spazer picture, down left, its lanes nearly spread.</summary>
    ChargedSpazerWaveDownLeftNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftWaving"/> showing the Charged Spazer picture, up right, its thick beam, its lanes nearly spread.</summary>
    ChargedSpazerWaveDownLeftUpRightThickNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftWaving"/> showing the Charged Spazer picture, down left, its lanes fully spread.</summary>
    ChargedSpazerWaveDownLeftSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftWaving"/> showing the Charged Spazer picture, up right, its thick beam, its lanes fully spread.</summary>
    ChargedSpazerWaveDownLeftUpRightThickSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftWaving"/> showing the Charged Spazer picture, down left, its lanes splitting, as <see cref="ChargedSpazerWaveDownLeftSplitting"/> does, leaving no trail.</summary>
    ChargedSpazerWaveDownLeftSplittingTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftWaving"/> showing the Charged Spazer picture, up right, its thick beam, its lanes splitting, as <see cref="ChargedSpazerWaveDownLeftUpRightThickSplitting"/> does, leaving no trail.</summary>
    ChargedSpazerWaveDownLeftUpRightThickSplittingTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeft"/> showing the Spazer Startup picture, left, a short startup strip.</summary>
    ChargedSpazerWaveLeftStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeft"/> showing the Spazer Startup picture, horizontal, its thick beam, a short startup strip.</summary>
    ChargedSpazerWaveLeftHorizontalThickStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeft"/> showing the Spazer Startup picture, left, a long startup strip.</summary>
    ChargedSpazerWaveLeftStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeft"/> showing the Spazer Startup picture, horizontal, its thick beam, a long startup strip.</summary>
    ChargedSpazerWaveLeftHorizontalThickStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftWaving"/> showing the Charged Spazer picture, left, seed.</summary>
    ChargedSpazerWaveLeftSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftWaving"/> showing the Charged Spazer picture, horizontal, its thick beam, seed.</summary>
    ChargedSpazerWaveLeftHorizontalThickSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftWaving"/> showing the Charged Spazer picture, left, its lanes splitting.</summary>
    ChargedSpazerWaveLeftSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftWaving"/> showing the Charged Spazer picture, horizontal, its thick beam, its lanes splitting.</summary>
    ChargedSpazerWaveLeftHorizontalThickSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftWaving"/> showing the Charged Spazer picture, left, its lanes parting.</summary>
    ChargedSpazerWaveLeftParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftWaving"/> showing the Charged Spazer picture, horizontal, its thick beam, its lanes parting.</summary>
    ChargedSpazerWaveLeftHorizontalThickParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftWaving"/> showing the Charged Spazer picture, left, its lanes spreading.</summary>
    ChargedSpazerWaveLeftSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftWaving"/> showing the Charged Spazer picture, horizontal, its thick beam, its lanes spreading.</summary>
    ChargedSpazerWaveLeftHorizontalThickSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftWaving"/> showing the Charged Spazer picture, left, its lanes nearly spread.</summary>
    ChargedSpazerWaveLeftNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftWaving"/> showing the Charged Spazer picture, horizontal, its thick beam, its lanes nearly spread.</summary>
    ChargedSpazerWaveLeftHorizontalThickNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftWaving"/> showing the Charged Spazer picture, left, its lanes fully spread.</summary>
    ChargedSpazerWaveLeftSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftWaving"/> showing the Charged Spazer picture, horizontal, its thick beam, its lanes fully spread.</summary>
    ChargedSpazerWaveLeftHorizontalThickSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftWaving"/> showing the Charged Spazer picture, left, its lanes splitting, as <see cref="ChargedSpazerWaveLeftSplitting"/> does, with a narrower hitbox and leaving no trail.</summary>
    ChargedSpazerWaveLeftSplittingNarrowHitboxTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftWaving"/> showing the Charged Spazer picture, horizontal, its thick beam, its lanes splitting, as <see cref="ChargedSpazerWaveLeftHorizontalThickSplitting"/> does, with a narrower hitbox and leaving no trail.</summary>
    ChargedSpazerWaveLeftHorizontalThickSplittingNarrowHitboxTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeft"/> showing the Spazer Startup picture, up left, a short startup strip.</summary>
    ChargedSpazerWaveUpLeftStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeft"/> showing the Spazer Startup picture, down right, its thick beam, a short startup strip.</summary>
    ChargedSpazerWaveUpLeftDownRightThickStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeft"/> showing the Spazer Startup picture, up left, a long startup strip.</summary>
    ChargedSpazerWaveUpLeftStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeft"/> showing the Spazer Startup picture, down right, its thick beam, a long startup strip.</summary>
    ChargedSpazerWaveUpLeftDownRightThickStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftWaving"/> showing the Charged Spazer picture, up left, seed.</summary>
    ChargedSpazerWaveUpLeftSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftWaving"/> showing the Charged Spazer picture, down right, its thick beam, seed.</summary>
    ChargedSpazerWaveUpLeftDownRightThickSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftWaving"/> showing the Charged Spazer picture, up left, its lanes splitting.</summary>
    ChargedSpazerWaveUpLeftSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftWaving"/> showing the Charged Spazer picture, down right, its thick beam, its lanes splitting.</summary>
    ChargedSpazerWaveUpLeftDownRightThickSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftWaving"/> showing the Charged Spazer picture, up left, its lanes parting.</summary>
    ChargedSpazerWaveUpLeftParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftWaving"/> showing the Charged Spazer picture, down right, its thick beam, its lanes parting.</summary>
    ChargedSpazerWaveUpLeftDownRightThickParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftWaving"/> showing the Charged Spazer picture, up left, its lanes spreading.</summary>
    ChargedSpazerWaveUpLeftSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftWaving"/> showing the Charged Spazer picture, down right, its thick beam, its lanes spreading.</summary>
    ChargedSpazerWaveUpLeftDownRightThickSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftWaving"/> showing the Charged Spazer picture, up left, its lanes nearly spread.</summary>
    ChargedSpazerWaveUpLeftNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftWaving"/> showing the Charged Spazer picture, down right, its thick beam, its lanes nearly spread.</summary>
    ChargedSpazerWaveUpLeftDownRightThickNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftWaving"/> showing the Charged Spazer picture, up left, its lanes fully spread.</summary>
    ChargedSpazerWaveUpLeftSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftWaving"/> showing the Charged Spazer picture, down right, its thick beam, its lanes fully spread.</summary>
    ChargedSpazerWaveUpLeftDownRightThickSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftWaving"/> showing the Charged Spazer picture, up left, its lanes splitting, as <see cref="ChargedSpazerWaveUpLeftSplitting"/> does, leaving no trail.</summary>
    ChargedSpazerWaveUpLeftSplittingTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftWaving"/> showing the Charged Spazer picture, down right, its thick beam, its lanes splitting, as <see cref="ChargedSpazerWaveUpLeftDownRightThickSplitting"/> does, leaving no trail.</summary>
    ChargedSpazerWaveUpLeftDownRightThickSplittingTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaVertical"/> showing the Plasma Startup picture, up, core.</summary>
    ChargedPlasmaVerticalCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaVertical"/> showing the Plasma Startup picture, up, its thick beam, core.</summary>
    ChargedPlasmaVerticalThickCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaVertical"/> showing the Plasma Startup picture, up, short.</summary>
    ChargedPlasmaVerticalShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaVertical"/> showing the Plasma Startup picture, up, its thick beam, short.</summary>
    ChargedPlasmaVerticalThickShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaVertical"/> showing the Plasma Startup picture, up, long.</summary>
    ChargedPlasmaVerticalLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaVertical"/> showing the Plasma Startup picture, up, its thick beam, long.</summary>
    ChargedPlasmaVerticalThickLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaVerticalFullLength"/> showing the Plasma Startup picture, up, full.</summary>
    ChargedPlasmaVerticalFull,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaVerticalFullLength"/> showing the Plasma Startup picture, up, its thick beam, full.</summary>
    ChargedPlasmaVerticalThickFull,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaRisingDiagonal"/> showing the Plasma Startup picture, up right, core.</summary>
    ChargedPlasmaRisingDiagonalCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaRisingDiagonal"/> showing the Plasma Startup picture, up right, its thick beam, core.</summary>
    ChargedPlasmaRisingDiagonalThickCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaRisingDiagonal"/> showing the Plasma Startup picture, up right, short.</summary>
    ChargedPlasmaRisingDiagonalShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaRisingDiagonal"/> showing the Plasma Startup picture, up right, its thick beam, short.</summary>
    ChargedPlasmaRisingDiagonalThickShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaRisingDiagonal"/> showing the Plasma Startup picture, up right, long.</summary>
    ChargedPlasmaRisingDiagonalLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaRisingDiagonal"/> showing the Plasma Startup picture, up right, its thick beam, long.</summary>
    ChargedPlasmaRisingDiagonalThickLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaRisingDiagonalFullLength"/> showing the Plasma Startup picture, up right, full.</summary>
    ChargedPlasmaRisingDiagonalFull,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaRisingDiagonalFullLength"/> showing the Plasma Startup picture, up right, its thick beam, full.</summary>
    ChargedPlasmaRisingDiagonalThickFull,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaHorizontal"/> showing the Plasma Startup picture, right, core.</summary>
    ChargedPlasmaHorizontalCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaHorizontal"/> showing the Plasma Startup picture, right, its thick beam, core.</summary>
    ChargedPlasmaHorizontalThickCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaHorizontal"/> showing the Plasma Startup picture, right, short.</summary>
    ChargedPlasmaHorizontalShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaHorizontal"/> showing the Plasma Startup picture, right, its thick beam, short.</summary>
    ChargedPlasmaHorizontalThickShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaHorizontal"/> showing the Plasma Startup picture, right, long.</summary>
    ChargedPlasmaHorizontalLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaHorizontal"/> showing the Plasma Startup picture, right, its thick beam, long.</summary>
    ChargedPlasmaHorizontalThickLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaHorizontalFullLength"/> showing the Plasma Startup picture, right, full.</summary>
    ChargedPlasmaHorizontalFull,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaHorizontalFullLength"/> showing the Plasma Startup picture, right, its thick beam, full.</summary>
    ChargedPlasmaHorizontalThickFull,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaFallingDiagonal"/> showing the Plasma Startup picture, down right, core.</summary>
    ChargedPlasmaFallingDiagonalCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaFallingDiagonal"/> showing the Plasma Startup picture, down right, its thick beam, core.</summary>
    ChargedPlasmaFallingDiagonalThickCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaFallingDiagonal"/> showing the Plasma Startup picture, down right, short.</summary>
    ChargedPlasmaFallingDiagonalShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaFallingDiagonal"/> showing the Plasma Startup picture, down right, its thick beam, short.</summary>
    ChargedPlasmaFallingDiagonalThickShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaFallingDiagonal"/> showing the Plasma Startup picture, down right, long.</summary>
    ChargedPlasmaFallingDiagonalLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaFallingDiagonal"/> showing the Plasma Startup picture, down right, its thick beam, long.</summary>
    ChargedPlasmaFallingDiagonalThickLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaFallingDiagonalFullLength"/> showing the Plasma Startup picture, down right, full.</summary>
    ChargedPlasmaFallingDiagonalFull,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaFallingDiagonalFullLength"/> showing the Plasma Startup picture, down right, its thick beam, full.</summary>
    ChargedPlasmaFallingDiagonalThickFull,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveVertical"/> showing the Plasma Startup picture, up, core.</summary>
    ChargedPlasmaWaveVerticalCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveVertical"/> showing the Plasma Startup picture, up, its thick beam, core.</summary>
    ChargedPlasmaWaveVerticalThickCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveVertical"/> showing the Plasma Startup picture, up, short.</summary>
    ChargedPlasmaWaveVerticalShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveVertical"/> showing the Plasma Startup picture, up, its thick beam, short.</summary>
    ChargedPlasmaWaveVerticalThickShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveVertical"/> showing the Plasma Startup picture, up, long.</summary>
    ChargedPlasmaWaveVerticalLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveVertical"/> showing the Plasma Startup picture, up, its thick beam, long.</summary>
    ChargedPlasmaWaveVerticalThickLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveVerticalWaving"/> showing the Plasma Wave picture, up long, core.</summary>
    ChargedPlasmaWaveVerticalLongCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveVerticalWaving"/> showing the Plasma Wave picture, up, its thick beam, core: the thick beam the long core flickers with, with its hitbox and trail placements.</summary>
    ChargedPlasmaWaveVerticalThickLongCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveVerticalWaving"/> showing the Plasma Wave picture, up long, its lobes near the centre.</summary>
    ChargedPlasmaWaveVerticalLongLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveVerticalWaving"/> showing the Plasma Wave picture, up, its thick beam, its lobes near the centre.</summary>
    ChargedPlasmaWaveVerticalThickLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveVerticalWaving"/> showing the Plasma Wave picture, up long, its lobes midway out.</summary>
    ChargedPlasmaWaveVerticalLongLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveVerticalWaving"/> showing the Plasma Wave picture, up, its thick beam, its lobes midway out.</summary>
    ChargedPlasmaWaveVerticalThickLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveVerticalWaving"/> showing the Plasma Wave picture, up long, its lobes far out.</summary>
    ChargedPlasmaWaveVerticalLongLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveVerticalWaving"/> showing the Plasma Wave picture, up, its thick beam, its lobes far out.</summary>
    ChargedPlasmaWaveVerticalThickLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveVerticalWaving"/> showing the Plasma Wave picture, up long, its lobes at their peak.</summary>
    ChargedPlasmaWaveVerticalLongLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveVerticalWaving"/> showing the Plasma Wave picture, up, its thick beam, its lobes at their peak.</summary>
    ChargedPlasmaWaveVerticalThickLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRisingDiagonal"/> showing the Plasma Startup picture, up right, core.</summary>
    ChargedPlasmaWaveRisingDiagonalCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRisingDiagonal"/> showing the Plasma Startup picture, up right, its thick beam, core.</summary>
    ChargedPlasmaWaveRisingDiagonalThickCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRisingDiagonal"/> showing the Plasma Startup picture, up right, short.</summary>
    ChargedPlasmaWaveRisingDiagonalShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRisingDiagonal"/> showing the Plasma Startup picture, up right, its thick beam, short.</summary>
    ChargedPlasmaWaveRisingDiagonalThickShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRisingDiagonal"/> showing the Plasma Startup picture, up right, long.</summary>
    ChargedPlasmaWaveRisingDiagonalLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRisingDiagonal"/> showing the Plasma Startup picture, up right, its thick beam, long.</summary>
    ChargedPlasmaWaveRisingDiagonalThickLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRisingDiagonalWaving"/> showing the Plasma Wave picture, up right long, core.</summary>
    ChargedPlasmaWaveRisingDiagonalLongCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRisingDiagonalWaving"/> showing the Plasma Wave picture, up right, its thick beam, core: the thick beam the long core flickers with, with its hitbox and trail placements.</summary>
    ChargedPlasmaWaveRisingDiagonalThickLongCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRisingDiagonalWaving"/> showing the Plasma Wave picture, up right long, its lobes near the centre.</summary>
    ChargedPlasmaWaveRisingDiagonalLongLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRisingDiagonalWaving"/> showing the Plasma Wave picture, up right, its thick beam, its lobes near the centre.</summary>
    ChargedPlasmaWaveRisingDiagonalThickLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRisingDiagonalWaving"/> showing the Plasma Wave picture, up right long, its lobes midway out.</summary>
    ChargedPlasmaWaveRisingDiagonalLongLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRisingDiagonalWaving"/> showing the Plasma Wave picture, up right, its thick beam, its lobes midway out.</summary>
    ChargedPlasmaWaveRisingDiagonalThickLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRisingDiagonalWaving"/> showing the Plasma Wave picture, up right long, its lobes far out.</summary>
    ChargedPlasmaWaveRisingDiagonalLongLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRisingDiagonalWaving"/> showing the Plasma Wave picture, up right, its thick beam, its lobes far out.</summary>
    ChargedPlasmaWaveRisingDiagonalThickLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRisingDiagonalWaving"/> showing the Plasma Wave picture, up right long, its lobes at their peak.</summary>
    ChargedPlasmaWaveRisingDiagonalLongLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRisingDiagonalWaving"/> showing the Plasma Wave picture, up right, its thick beam, its lobes at their peak.</summary>
    ChargedPlasmaWaveRisingDiagonalThickLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveHorizontal"/> showing the Plasma Startup picture, right, core.</summary>
    ChargedPlasmaWaveHorizontalCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveHorizontal"/> showing the Plasma Startup picture, right, its thick beam, core.</summary>
    ChargedPlasmaWaveHorizontalThickCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveHorizontal"/> showing the Plasma Startup picture, right, short.</summary>
    ChargedPlasmaWaveHorizontalShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveHorizontal"/> showing the Plasma Startup picture, right, its thick beam, short.</summary>
    ChargedPlasmaWaveHorizontalThickShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveHorizontal"/> showing the Plasma Startup picture, right, long.</summary>
    ChargedPlasmaWaveHorizontalLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveHorizontal"/> showing the Plasma Startup picture, right, its thick beam, long.</summary>
    ChargedPlasmaWaveHorizontalThickLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveHorizontalWaving"/> showing the Plasma Wave picture, right long, core.</summary>
    ChargedPlasmaWaveHorizontalLongCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveHorizontalWaving"/> showing the Plasma Wave picture, right, its thick beam, core: the thick beam the long core flickers with, with its hitbox and trail placements.</summary>
    ChargedPlasmaWaveHorizontalThickLongCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveHorizontalWaving"/> showing the Plasma Wave picture, right long, its lobes near the centre.</summary>
    ChargedPlasmaWaveHorizontalLongLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveHorizontalWaving"/> showing the Plasma Wave picture, right, its thick beam, its lobes near the centre.</summary>
    ChargedPlasmaWaveHorizontalThickLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveHorizontalWaving"/> showing the Plasma Wave picture, right long, its lobes midway out.</summary>
    ChargedPlasmaWaveHorizontalLongLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveHorizontalWaving"/> showing the Plasma Wave picture, right, its thick beam, its lobes midway out.</summary>
    ChargedPlasmaWaveHorizontalThickLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveHorizontalWaving"/> showing the Plasma Wave picture, right long, its lobes far out.</summary>
    ChargedPlasmaWaveHorizontalLongLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveHorizontalWaving"/> showing the Plasma Wave picture, right, its thick beam, its lobes far out.</summary>
    ChargedPlasmaWaveHorizontalThickLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveHorizontalWaving"/> showing the Plasma Wave picture, right long, its lobes at their peak.</summary>
    ChargedPlasmaWaveHorizontalLongLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveHorizontalWaving"/> showing the Plasma Wave picture, right, its thick beam, its lobes at their peak.</summary>
    ChargedPlasmaWaveHorizontalThickLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveFallingDiagonal"/> showing the Plasma Startup picture, down right, core.</summary>
    ChargedPlasmaWaveFallingDiagonalCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveFallingDiagonal"/> showing the Plasma Startup picture, down right, its thick beam, core.</summary>
    ChargedPlasmaWaveFallingDiagonalThickCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveFallingDiagonal"/> showing the Plasma Startup picture, down right, short.</summary>
    ChargedPlasmaWaveFallingDiagonalShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveFallingDiagonal"/> showing the Plasma Startup picture, down right, its thick beam, short.</summary>
    ChargedPlasmaWaveFallingDiagonalThickShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveFallingDiagonal"/> showing the Plasma Startup picture, down right, long.</summary>
    ChargedPlasmaWaveFallingDiagonalLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveFallingDiagonal"/> showing the Plasma Startup picture, down right, its thick beam, long.</summary>
    ChargedPlasmaWaveFallingDiagonalThickLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveFallingDiagonalWaving"/> showing the Plasma Wave picture, down right long, core.</summary>
    ChargedPlasmaWaveFallingDiagonalLongCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveFallingDiagonalWaving"/> showing the Plasma Wave picture, down right, its thick beam, core: the thick beam the long core flickers with, with its hitbox and trail placements.</summary>
    ChargedPlasmaWaveFallingDiagonalThickLongCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveFallingDiagonalWaving"/> showing the Plasma Wave picture, down right long, its lobes near the centre.</summary>
    ChargedPlasmaWaveFallingDiagonalLongLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveFallingDiagonalWaving"/> showing the Plasma Wave picture, down right, its thick beam, its lobes near the centre.</summary>
    ChargedPlasmaWaveFallingDiagonalThickLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveFallingDiagonalWaving"/> showing the Plasma Wave picture, down right long, its lobes midway out.</summary>
    ChargedPlasmaWaveFallingDiagonalLongLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveFallingDiagonalWaving"/> showing the Plasma Wave picture, down right, its thick beam, its lobes midway out.</summary>
    ChargedPlasmaWaveFallingDiagonalThickLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveFallingDiagonalWaving"/> showing the Plasma Wave picture, down right long, its lobes far out.</summary>
    ChargedPlasmaWaveFallingDiagonalLongLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveFallingDiagonalWaving"/> showing the Plasma Wave picture, down right, its thick beam, its lobes far out.</summary>
    ChargedPlasmaWaveFallingDiagonalThickLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveFallingDiagonalWaving"/> showing the Plasma Wave picture, down right long, its lobes at their peak.</summary>
    ChargedPlasmaWaveFallingDiagonalLongLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveFallingDiagonalWaving"/> showing the Plasma Wave picture, down right, its thick beam, its lobes at their peak.</summary>
    ChargedPlasmaWaveFallingDiagonalThickLobesPeak,
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
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerSpecialAttackTrailWidest"/> showing nothing, as <see cref="SpazerSpecialAttackTrailHidden"/> does, with the widest hitbox and its trail's sparkles placed furthest apart.</summary>
    SpazerSpecialAttackTrailHiddenWidest,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveSpecialAttack"/> showing the Charged Wave picture, core.</summary>
    WaveSpecialAttackCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveSpecialAttack"/> showing the Charged Wave picture, core, its starburst: the flicker's other picture to its orb.</summary>
    WaveSpecialAttackCoreStarburst,
}
