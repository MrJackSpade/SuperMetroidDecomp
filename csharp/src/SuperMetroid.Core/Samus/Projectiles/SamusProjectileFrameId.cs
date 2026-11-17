namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// One frame a Samus projectile animation shows (#1301, #2124, #2274), named for what it shows: the shot of its animation
/// and the pose of the picture it shows, a stage of a swing, spread, growth or cycle named for that stage. A frame that
/// shows another frame's picture again with that frame's hitbox and trail placements is that frame; one that differs in
/// either is named for the difference. What a frame pictures is installed only (#1596); its values are arbitrary.
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
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownFacingRight"/> showing the Wave picture, down, right, its lobe near the centre.</summary>
    WaveDownFacingRightRightNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownFacingRight"/> showing the Wave picture, down, right, its lobe midway out.</summary>
    WaveDownFacingRightRightMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownFacingRight"/> showing the Wave picture, down, right, its lobe far out.</summary>
    WaveDownFacingRightRightFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownFacingRight"/> showing the Wave picture, down, right, its lobe at its peak.</summary>
    WaveDownFacingRightRightPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownFacingRight"/> showing the Wave picture, down, left, its lobe near the centre.</summary>
    WaveDownFacingRightLeftNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownFacingRight"/> showing the Wave picture, down, left, its lobe midway out.</summary>
    WaveDownFacingRightLeftMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownFacingRight"/> showing the Wave picture, down, left, its lobe far out.</summary>
    WaveDownFacingRightLeftFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownFacingRight"/> showing the Wave picture, down, left, its lobe at its peak.</summary>
    WaveDownFacingRightLeftPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveUpRight"/> showing the Wave picture, centre.</summary>
    WaveUpRightCentre,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveUpRight"/> showing the Wave picture, up right, up left, its lobe near the centre.</summary>
    WaveUpRightUpLeftNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveUpRight"/> showing the Wave picture, up right, up left, its lobe midway out.</summary>
    WaveUpRightUpLeftMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveUpRight"/> showing the Wave picture, up right, up left, its lobe far out.</summary>
    WaveUpRightUpLeftFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveUpRight"/> showing the Wave picture, up right, up left, its lobe at its peak.</summary>
    WaveUpRightUpLeftPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveUpRight"/> showing the Wave picture, up right, up left, its lobe near the centre, as <see cref="WaveUpRightUpLeftNear"/> does, with a narrower hitbox.</summary>
    WaveUpRightUpLeftNearNarrowHitbox,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveUpRight"/> showing the Wave picture, centre, as <see cref="WaveUpRightCentre"/> does, with a narrower hitbox.</summary>
    WaveUpRightCentreNarrowHitbox,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveUpRight"/> showing the Wave picture, up right, down right, its lobe near the centre.</summary>
    WaveUpRightDownRightNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveUpRight"/> showing the Wave picture, up right, down right, its lobe midway out.</summary>
    WaveUpRightDownRightMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveUpRight"/> showing the Wave picture, up right, down right, its lobe far out.</summary>
    WaveUpRightDownRightFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveUpRight"/> showing the Wave picture, up right, down right, its lobe at its peak.</summary>
    WaveUpRightDownRightPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveUpRight"/> showing the Wave picture, up right, down right, its lobe near the centre, as <see cref="WaveUpRightDownRightNear"/> does, with a wider hitbox.</summary>
    WaveUpRightDownRightNearWideHitbox,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveRight"/> showing the Wave picture, centre.</summary>
    WaveRightCentre,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveRight"/> showing the Wave picture, right, up, its lobe near the centre.</summary>
    WaveRightUpNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveRight"/> showing the Wave picture, right, up, its lobe midway out.</summary>
    WaveRightUpMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveRight"/> showing the Wave picture, right, up, its lobe far out.</summary>
    WaveRightUpFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveRight"/> showing the Wave picture, right, up, its lobe at its peak.</summary>
    WaveRightUpPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveRight"/> showing the Wave picture, right, down, its lobe near the centre.</summary>
    WaveRightDownNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveRight"/> showing the Wave picture, right, down, its lobe midway out.</summary>
    WaveRightDownMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveRight"/> showing the Wave picture, right, down, its lobe far out.</summary>
    WaveRightDownFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveRight"/> showing the Wave picture, right, down, its lobe at its peak.</summary>
    WaveRightDownPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownRight"/> showing the Wave picture, centre.</summary>
    WaveDownRightCentre,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownRight"/> showing the Wave picture, down right, up right, its lobe near the centre.</summary>
    WaveDownRightUpRightNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownRight"/> showing the Wave picture, down right, up right, its lobe midway out.</summary>
    WaveDownRightUpRightMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownRight"/> showing the Wave picture, down right, up right, its lobe far out.</summary>
    WaveDownRightUpRightFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownRight"/> showing the Wave picture, down right, up right, its lobe at its peak.</summary>
    WaveDownRightUpRightPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownRight"/> showing the Wave picture, down right, up right, its lobe near the centre, as <see cref="WaveDownRightUpRightNear"/> does, with a narrower hitbox.</summary>
    WaveDownRightUpRightNearNarrowHitbox,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownRight"/> showing the Wave picture, centre, as <see cref="WaveDownRightCentre"/> does, with a narrower hitbox.</summary>
    WaveDownRightCentreNarrowHitbox,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownRight"/> showing the Wave picture, down right, down left, its lobe near the centre.</summary>
    WaveDownRightDownLeftNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownRight"/> showing the Wave picture, down right, down left, its lobe midway out.</summary>
    WaveDownRightDownLeftMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownRight"/> showing the Wave picture, down right, down left, its lobe far out.</summary>
    WaveDownRightDownLeftFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownRight"/> showing the Wave picture, down right, down left, its lobe at its peak.</summary>
    WaveDownRightDownLeftPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveDownRight"/> showing the Wave picture, down right, down left, its lobe near the centre, as <see cref="WaveDownRightDownLeftNear"/> does, with a wider hitbox.</summary>
    WaveDownRightDownLeftNearWideHitbox,
    /// <summary>A frame of <see cref="SamusProjectileSegment.IceUpFacingRight"/> showing the Ice picture, a star.</summary>
    IceUpFacingRightStar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.IceUpFacingRight"/> showing the Ice picture, a cross.</summary>
    IceUpFacingRightCrossed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.IceUpFacingRight"/> showing the Ice picture, a narrow star.</summary>
    IceUpFacingRightNarrowStar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.IceUpFacingRight"/> showing the Ice picture, a mirrored cross.</summary>
    IceUpFacingRightCrossedMirrored,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerUpFacingRight"/> showing the Spazer picture, up, seed.</summary>
    SpazerUpFacingRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerUpFacingRight"/> showing the Spazer picture, up, its lanes parting.</summary>
    SpazerUpFacingRightParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerUpFacingRightLoop"/> showing the Spazer picture, up, its lanes fully spread.</summary>
    SpazerUpFacingRightSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerUpRight"/> showing the Spazer picture, up right, seed.</summary>
    SpazerUpRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerUpRight"/> showing the Spazer picture, up right, its lanes parting.</summary>
    SpazerUpRightParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerUpRightLoop"/> showing the Spazer picture, up right, its lanes fully spread.</summary>
    SpazerUpRightSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerRight"/> showing the Spazer picture, right, seed.</summary>
    SpazerRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerRight"/> showing the Spazer picture, right, its lanes parting.</summary>
    SpazerRightParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerRightLoop"/> showing the Spazer picture, right, its lanes fully spread.</summary>
    SpazerRightSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerDownRight"/> showing the Spazer picture, down right, seed.</summary>
    SpazerDownRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerDownRight"/> showing the Spazer picture, down right, its lanes parting.</summary>
    SpazerDownRightParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerDownRightLoop"/> showing the Spazer picture, down right, its lanes fully spread.</summary>
    SpazerDownRightSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerDownFacingRight"/> showing the Spazer picture, down, seed.</summary>
    SpazerDownFacingRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerDownFacingRight"/> showing the Spazer picture, down, its lanes parting.</summary>
    SpazerDownFacingRightParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerDownFacingRightLoop"/> showing the Spazer picture, down, its lanes fully spread.</summary>
    SpazerDownFacingRightSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerDownLeft"/> showing the Spazer picture, down left, seed.</summary>
    SpazerDownLeftSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerDownLeft"/> showing the Spazer picture, down left, its lanes parting.</summary>
    SpazerDownLeftParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerDownLeftLoop"/> showing the Spazer picture, down left, its lanes fully spread.</summary>
    SpazerDownLeftSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerLeft"/> showing the Spazer picture, left, seed.</summary>
    SpazerLeftSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerLeft"/> showing the Spazer picture, left, its lanes parting.</summary>
    SpazerLeftParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerLeftLoop"/> showing the Spazer picture, left, its lanes fully spread.</summary>
    SpazerLeftSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerUpLeft"/> showing the Spazer picture, up left, seed.</summary>
    SpazerUpLeftSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerUpLeft"/> showing the Spazer picture, up left, its lanes parting.</summary>
    SpazerUpLeftParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerUpLeftLoop"/> showing the Spazer picture, up left, its lanes fully spread.</summary>
    SpazerUpLeftSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpFacingRight"/> showing the Spazer picture, up, seed.</summary>
    SpazerWaveUpFacingRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpFacingRight"/> showing the Spazer picture, up, its lanes splitting.</summary>
    SpazerWaveUpFacingRightSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpFacingRight"/> showing the Spazer picture, up, its lanes parting.</summary>
    SpazerWaveUpFacingRightParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpFacingRight"/> showing the Spazer picture, up, its lanes spreading.</summary>
    SpazerWaveUpFacingRightSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpFacingRight"/> showing the Spazer picture, up, its lanes nearly spread.</summary>
    SpazerWaveUpFacingRightNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpFacingRight"/> showing the Spazer picture, up, its lanes fully spread.</summary>
    SpazerWaveUpFacingRightSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpFacingRight"/> showing the Spazer picture, up, its lanes parting, as <see cref="SpazerWaveUpFacingRightParting"/> does, leaving no trail where <see cref="ProjectileTrailPlacementList.UnchargedDefault"/> places one from it.</summary>
    SpazerWaveUpFacingRightPartingWithoutDefaultTrail,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveUpFacingRight"/> showing the Spazer picture, up, its lanes splitting, as <see cref="SpazerWaveUpFacingRightSplitting"/> does, leaving no trail where <see cref="ProjectileTrailPlacementList.UnchargedDefault"/> places one from it.</summary>
    SpazerWaveUpFacingRightSplittingWithoutDefaultTrail,
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
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownFacingRight"/> showing the Spazer picture, down, seed.</summary>
    SpazerWaveDownFacingRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownFacingRight"/> showing the Spazer picture, down, its lanes splitting.</summary>
    SpazerWaveDownFacingRightSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownFacingRight"/> showing the Spazer picture, down, its lanes parting.</summary>
    SpazerWaveDownFacingRightParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownFacingRight"/> showing the Spazer picture, down, its lanes spreading.</summary>
    SpazerWaveDownFacingRightSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownFacingRight"/> showing the Spazer picture, down, its lanes nearly spread.</summary>
    SpazerWaveDownFacingRightNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownFacingRight"/> showing the Spazer picture, down, its lanes fully spread.</summary>
    SpazerWaveDownFacingRightSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownFacingRight"/> showing the Spazer picture, down, its lanes parting, as <see cref="SpazerWaveDownFacingRightParting"/> does, leaving no trail where <see cref="ProjectileTrailPlacementList.UnchargedDefault"/> places one from it.</summary>
    SpazerWaveDownFacingRightPartingWithoutDefaultTrail,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerWaveDownFacingRight"/> showing the Spazer picture, down, its lanes splitting, as <see cref="SpazerWaveDownFacingRightSplitting"/> does, leaving no trail where <see cref="ProjectileTrailPlacementList.UnchargedDefault"/> places one from it.</summary>
    SpazerWaveDownFacingRightSplittingWithoutDefaultTrail,
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
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveUpFacingRight"/> showing the Plasma Wave picture, up short, its lobes near the centre.</summary>
    PlasmaWaveUpFacingRightShortLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveUpFacingRight"/> showing the Plasma Wave picture, up short, its lobes midway out.</summary>
    PlasmaWaveUpFacingRightShortLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveUpFacingRight"/> showing the Plasma Wave picture, up short, its lobes far out.</summary>
    PlasmaWaveUpFacingRightShortLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveUpFacingRight"/> showing the Plasma Wave picture, up short, its lobes at their peak.</summary>
    PlasmaWaveUpFacingRightShortLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveUpFacingRight"/> showing the Plasma Wave picture, up short, its lobes near the centre, as <see cref="PlasmaWaveUpFacingRightShortLobesNear"/> does, leaving no trail where <see cref="ProjectileTrailPlacementList.UnchargedDefault"/> places one from it.</summary>
    PlasmaWaveUpFacingRightShortLobesNearWithoutDefaultTrail,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaIceWaveUpRight"/> showing the Plasma Startup picture, up right, core.</summary>
    PlasmaIceWaveUpRightCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveUpRight"/> showing the Plasma Wave picture, up right short, core.</summary>
    PlasmaWaveUpRightShortCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveUpRight"/> showing the Plasma Wave picture, up right short, its lobes near the centre.</summary>
    PlasmaWaveUpRightShortLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveUpRight"/> showing the Plasma Wave picture, up right short, its lobes midway out.</summary>
    PlasmaWaveUpRightShortLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveUpRight"/> showing the Plasma Wave picture, up right short, its lobes far out.</summary>
    PlasmaWaveUpRightShortLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveUpRight"/> showing the Plasma Wave picture, up right short, its lobes at their peak.</summary>
    PlasmaWaveUpRightShortLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveUpRight"/> showing the Plasma Wave picture, up right short, its lobes near the centre, as <see cref="PlasmaWaveUpRightShortLobesNear"/> does, leaving no trail where <see cref="ProjectileTrailPlacementList.UnchargedDefault"/> places one from it.</summary>
    PlasmaWaveUpRightShortLobesNearWithoutDefaultTrail,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveRight"/> showing the Plasma Startup picture, right, core.</summary>
    PlasmaWaveRightCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveRightLoop"/> showing the Plasma Wave picture, right short, core.</summary>
    PlasmaWaveRightShortCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveRightLoop"/> showing the Plasma Wave picture, right short, its lobes near the centre.</summary>
    PlasmaWaveRightShortLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveRightLoop"/> showing the Plasma Wave picture, right short, its lobes midway out.</summary>
    PlasmaWaveRightShortLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveRightLoop"/> showing the Plasma Wave picture, right short, its lobes far out.</summary>
    PlasmaWaveRightShortLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveRightLoop"/> showing the Plasma Wave picture, right short, its lobes at their peak.</summary>
    PlasmaWaveRightShortLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveRightLoop"/> showing the Plasma Wave picture, right short, its lobes near the centre, as <see cref="PlasmaWaveRightShortLobesNear"/> does, leaving no trail where <see cref="ProjectileTrailPlacementList.UnchargedDefault"/> places one from it.</summary>
    PlasmaWaveRightShortLobesNearWithoutDefaultTrail,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaIceWaveDownRight"/> showing the Plasma Startup picture, down right, core.</summary>
    PlasmaIceWaveDownRightCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveDownRight"/> showing the Plasma Wave picture, down right short, core.</summary>
    PlasmaWaveDownRightShortCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveDownRight"/> showing the Plasma Wave picture, down right short, its lobes near the centre.</summary>
    PlasmaWaveDownRightShortLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveDownRight"/> showing the Plasma Wave picture, down right short, its lobes midway out.</summary>
    PlasmaWaveDownRightShortLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveDownRight"/> showing the Plasma Wave picture, down right short, its lobes far out.</summary>
    PlasmaWaveDownRightShortLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveDownRight"/> showing the Plasma Wave picture, down right short, its lobes at their peak.</summary>
    PlasmaWaveDownRightShortLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.PlasmaWaveDownRight"/> showing the Plasma Wave picture, down right short, its lobes near the centre, as <see cref="PlasmaWaveDownRightShortLobesNear"/> does, leaving no trail where <see cref="ProjectileTrailPlacementList.UnchargedDefault"/> places one from it.</summary>
    PlasmaWaveDownRightShortLobesNearWithoutDefaultTrail,
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
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownFacingRight"/> showing the Charged Wave picture, down, its lobes near the centre.</summary>
    ChargedWaveDownFacingRightLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownFacingRight"/> showing the Charged Wave picture, down, its lobes near the centre (alternate).</summary>
    ChargedWaveDownFacingRightLobesNearAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownFacingRight"/> showing the Charged Wave picture, down, its lobes midway out.</summary>
    ChargedWaveDownFacingRightLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownFacingRight"/> showing the Charged Wave picture, down, its lobes midway out (alternate).</summary>
    ChargedWaveDownFacingRightLobesMidwayAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownFacingRight"/> showing the Charged Wave picture, down, its lobes far out.</summary>
    ChargedWaveDownFacingRightLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownFacingRight"/> showing the Charged Wave picture, down, its lobes far out (alternate).</summary>
    ChargedWaveDownFacingRightLobesFarAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownFacingRight"/> showing the Charged Wave picture, down, its lobes at their peak.</summary>
    ChargedWaveDownFacingRightLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownFacingRight"/> showing the Charged Wave picture, down, its lobes at their peak (alternate).</summary>
    ChargedWaveDownFacingRightLobesPeakAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveUpRight"/> showing the Charged Wave picture, core.</summary>
    ChargedWaveUpRightCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveUpRight"/> showing the Charged Wave picture, core alternate.</summary>
    ChargedWaveUpRightCoreAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveUpRight"/> showing the Charged Wave picture, up right, its lobes near the centre.</summary>
    ChargedWaveUpRightLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveUpRight"/> showing the Charged Wave picture, up right, its lobes near the centre (alternate).</summary>
    ChargedWaveUpRightLobesNearAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveUpRight"/> showing the Charged Wave picture, up right, its lobes midway out.</summary>
    ChargedWaveUpRightLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveUpRight"/> showing the Charged Wave picture, up right, its lobes midway out (alternate).</summary>
    ChargedWaveUpRightLobesMidwayAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveUpRight"/> showing the Charged Wave picture, up right, its lobes far out.</summary>
    ChargedWaveUpRightLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveUpRight"/> showing the Charged Wave picture, up right, its lobes far out (alternate).</summary>
    ChargedWaveUpRightLobesFarAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveUpRight"/> showing the Charged Wave picture, up right, its lobes at their peak.</summary>
    ChargedWaveUpRightLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveUpRight"/> showing the Charged Wave picture, up right, its lobes at their peak (alternate).</summary>
    ChargedWaveUpRightLobesPeakAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRight"/> showing the Charged Wave picture, core.</summary>
    ChargedWaveRightCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRight"/> showing the Charged Wave picture, core alternate.</summary>
    ChargedWaveRightCoreAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRight"/> showing the Charged Wave picture, right, its lobes near the centre.</summary>
    ChargedWaveRightLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRight"/> showing the Charged Wave picture, right, its lobes near the centre (alternate).</summary>
    ChargedWaveRightLobesNearAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRight"/> showing the Charged Wave picture, right, its lobes midway out.</summary>
    ChargedWaveRightLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRight"/> showing the Charged Wave picture, right, its lobes midway out (alternate).</summary>
    ChargedWaveRightLobesMidwayAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRight"/> showing the Charged Wave picture, right, its lobes far out.</summary>
    ChargedWaveRightLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRight"/> showing the Charged Wave picture, right, its lobes far out (alternate).</summary>
    ChargedWaveRightLobesFarAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRight"/> showing the Charged Wave picture, right, its lobes at their peak.</summary>
    ChargedWaveRightLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveRight"/> showing the Charged Wave picture, right, its lobes at their peak (alternate).</summary>
    ChargedWaveRightLobesPeakAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownRight"/> showing the Charged Wave picture, core.</summary>
    ChargedWaveDownRightCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownRight"/> showing the Charged Wave picture, core alternate.</summary>
    ChargedWaveDownRightCoreAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownRight"/> showing the Charged Wave picture, down right, its lobes near the centre.</summary>
    ChargedWaveDownRightLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownRight"/> showing the Charged Wave picture, down right, its lobes near the centre (alternate).</summary>
    ChargedWaveDownRightLobesNearAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownRight"/> showing the Charged Wave picture, down right, its lobes midway out.</summary>
    ChargedWaveDownRightLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownRight"/> showing the Charged Wave picture, down right, its lobes midway out (alternate).</summary>
    ChargedWaveDownRightLobesMidwayAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownRight"/> showing the Charged Wave picture, down right, its lobes far out.</summary>
    ChargedWaveDownRightLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownRight"/> showing the Charged Wave picture, down right, its lobes far out (alternate).</summary>
    ChargedWaveDownRightLobesFarAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownRight"/> showing the Charged Wave picture, down right, its lobes at their peak.</summary>
    ChargedWaveDownRightLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedWaveDownRight"/> showing the Charged Wave picture, down right, its lobes at their peak (alternate).</summary>
    ChargedWaveDownRightLobesPeakAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceUpFacingRight"/> showing the Charged Ice picture, a cross.</summary>
    ChargedIceUpFacingRightCrossed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceUpFacingRight"/> showing the Charged Ice picture, a star.</summary>
    ChargedIceUpFacingRightStar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveUpFacingRight"/> showing nothing.</summary>
    ChargedIceWaveUpFacingRightHidden,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownFacingRight"/> showing the Charged Ice Wave picture, core.</summary>
    ChargedIceWaveDownFacingRightCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownFacingRight"/> showing the Charged Ice Wave picture, core alternate.</summary>
    ChargedIceWaveDownFacingRightCoreAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownFacingRight"/> showing the Charged Ice Wave picture, down, its lobes near the centre.</summary>
    ChargedIceWaveDownFacingRightLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownFacingRight"/> showing the Charged Ice Wave picture, down, its lobes near the centre (alternate).</summary>
    ChargedIceWaveDownFacingRightLobesNearAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownFacingRight"/> showing the Charged Ice Wave picture, down, its lobes midway out.</summary>
    ChargedIceWaveDownFacingRightLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownFacingRight"/> showing the Charged Ice Wave picture, down, its lobes midway out (alternate).</summary>
    ChargedIceWaveDownFacingRightLobesMidwayAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownFacingRight"/> showing the Charged Ice Wave picture, down, its lobes far out.</summary>
    ChargedIceWaveDownFacingRightLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownFacingRight"/> showing the Charged Ice Wave picture, down, its lobes far out (alternate).</summary>
    ChargedIceWaveDownFacingRightLobesFarAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownFacingRight"/> showing the Charged Ice Wave picture, down, its lobes at their peak.</summary>
    ChargedIceWaveDownFacingRightLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownFacingRight"/> showing the Charged Ice Wave picture, down, its lobes at their peak (alternate).</summary>
    ChargedIceWaveDownFacingRightLobesPeakAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveUpRight"/> showing the Charged Ice Wave picture, core.</summary>
    ChargedIceWaveUpRightCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveUpRight"/> showing the Charged Ice Wave picture, core alternate.</summary>
    ChargedIceWaveUpRightCoreAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveUpRight"/> showing the Charged Ice Wave picture, up right, its lobes near the centre.</summary>
    ChargedIceWaveUpRightLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveUpRight"/> showing the Charged Ice Wave picture, up right, its lobes near the centre (alternate).</summary>
    ChargedIceWaveUpRightLobesNearAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveUpRight"/> showing the Charged Ice Wave picture, up right, its lobes midway out.</summary>
    ChargedIceWaveUpRightLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveUpRight"/> showing the Charged Ice Wave picture, up right, its lobes midway out (alternate).</summary>
    ChargedIceWaveUpRightLobesMidwayAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveUpRight"/> showing the Charged Ice Wave picture, up right, its lobes far out.</summary>
    ChargedIceWaveUpRightLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveUpRight"/> showing the Charged Ice Wave picture, up right, its lobes far out (alternate).</summary>
    ChargedIceWaveUpRightLobesFarAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveUpRight"/> showing the Charged Ice Wave picture, up right, its lobes at their peak.</summary>
    ChargedIceWaveUpRightLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveUpRight"/> showing the Charged Ice Wave picture, up right, its lobes at their peak (alternate).</summary>
    ChargedIceWaveUpRightLobesPeakAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRight"/> showing the Charged Ice Wave picture, core.</summary>
    ChargedIceWaveRightCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRight"/> showing the Charged Ice Wave picture, core alternate.</summary>
    ChargedIceWaveRightCoreAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRight"/> showing the Charged Ice Wave picture, right, its lobes near the centre.</summary>
    ChargedIceWaveRightLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRight"/> showing the Charged Ice Wave picture, right, its lobes near the centre (alternate).</summary>
    ChargedIceWaveRightLobesNearAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRight"/> showing the Charged Ice Wave picture, right, its lobes midway out.</summary>
    ChargedIceWaveRightLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRight"/> showing the Charged Ice Wave picture, right, its lobes midway out (alternate).</summary>
    ChargedIceWaveRightLobesMidwayAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRight"/> showing the Charged Ice Wave picture, right, its lobes far out.</summary>
    ChargedIceWaveRightLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRight"/> showing the Charged Ice Wave picture, right, its lobes far out (alternate).</summary>
    ChargedIceWaveRightLobesFarAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRight"/> showing the Charged Ice Wave picture, right, its lobes at their peak.</summary>
    ChargedIceWaveRightLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveRight"/> showing the Charged Ice Wave picture, right, its lobes at their peak (alternate).</summary>
    ChargedIceWaveRightLobesPeakAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownRight"/> showing the Charged Ice Wave picture, core.</summary>
    ChargedIceWaveDownRightCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownRight"/> showing the Charged Ice Wave picture, core alternate.</summary>
    ChargedIceWaveDownRightCoreAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownRight"/> showing the Charged Ice Wave picture, down right, its lobes near the centre.</summary>
    ChargedIceWaveDownRightLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownRight"/> showing the Charged Ice Wave picture, down right, its lobes near the centre (alternate).</summary>
    ChargedIceWaveDownRightLobesNearAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownRight"/> showing the Charged Ice Wave picture, down right, its lobes midway out.</summary>
    ChargedIceWaveDownRightLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownRight"/> showing the Charged Ice Wave picture, down right, its lobes midway out (alternate).</summary>
    ChargedIceWaveDownRightLobesMidwayAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownRight"/> showing the Charged Ice Wave picture, down right, its lobes far out.</summary>
    ChargedIceWaveDownRightLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownRight"/> showing the Charged Ice Wave picture, down right, its lobes far out (alternate).</summary>
    ChargedIceWaveDownRightLobesFarAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownRight"/> showing the Charged Ice Wave picture, down right, its lobes at their peak.</summary>
    ChargedIceWaveDownRightLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedIceWaveDownRight"/> showing the Charged Ice Wave picture, down right, its lobes at their peak (alternate).</summary>
    ChargedIceWaveDownRightLobesPeakAlternate,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerUpFacingRight"/> showing the Spazer Startup picture, up, a short startup strip.</summary>
    ChargedSpazerUpFacingRightStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerUpFacingRight"/> showing the Spazer Startup picture, vertical alternate, a short startup strip.</summary>
    ChargedSpazerUpFacingRightVerticalAlternateStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerUpFacingRight"/> showing the Spazer Startup picture, up, a long startup strip.</summary>
    ChargedSpazerUpFacingRightStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerUpFacingRight"/> showing the Spazer Startup picture, vertical alternate, a long startup strip.</summary>
    ChargedSpazerUpFacingRightVerticalAlternateStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerUpFacingRight"/> showing the Charged Spazer picture, up, seed.</summary>
    ChargedSpazerUpFacingRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerUpFacingRight"/> showing the Charged Spazer picture, vertical alternate, seed.</summary>
    ChargedSpazerUpFacingRightVerticalAlternateSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerUpFacingRight"/> showing the Charged Spazer picture, up, its lanes parting.</summary>
    ChargedSpazerUpFacingRightParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerUpFacingRight"/> showing the Charged Spazer picture, vertical alternate, its lanes parting.</summary>
    ChargedSpazerUpFacingRightVerticalAlternateParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerUpFacingRightLoop"/> showing the Charged Spazer picture, up, its lanes fully spread.</summary>
    ChargedSpazerUpFacingRightSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerUpFacingRightLoop"/> showing the Charged Spazer picture, vertical alternate, its lanes fully spread.</summary>
    ChargedSpazerUpFacingRightVerticalAlternateSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerUpRight"/> showing the Spazer Startup picture, up right, a short startup strip.</summary>
    ChargedSpazerUpRightStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerUpRight"/> showing the Spazer Startup picture, up right alternate, a short startup strip.</summary>
    ChargedSpazerUpRightAlternateStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerUpRight"/> showing the Spazer Startup picture, up right, a long startup strip.</summary>
    ChargedSpazerUpRightStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerUpRight"/> showing the Spazer Startup picture, up right alternate, a long startup strip.</summary>
    ChargedSpazerUpRightAlternateStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerUpRight"/> showing the Charged Spazer picture, up right, seed.</summary>
    ChargedSpazerUpRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerUpRight"/> showing the Charged Spazer picture, up right alternate, seed.</summary>
    ChargedSpazerUpRightAlternateSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerUpRight"/> showing the Charged Spazer picture, up right, its lanes parting.</summary>
    ChargedSpazerUpRightParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerUpRight"/> showing the Charged Spazer picture, up right alternate, its lanes parting.</summary>
    ChargedSpazerUpRightAlternateParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerUpRightLoop"/> showing the Charged Spazer picture, up right, its lanes fully spread.</summary>
    ChargedSpazerUpRightSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerUpRightLoop"/> showing the Charged Spazer picture, up right alternate, its lanes fully spread.</summary>
    ChargedSpazerUpRightAlternateSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerRight"/> showing the Spazer Startup picture, right, a short startup strip.</summary>
    ChargedSpazerRightStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerRight"/> showing the Spazer Startup picture, horizontal alternate, a short startup strip.</summary>
    ChargedSpazerRightHorizontalAlternateStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerRight"/> showing the Spazer Startup picture, right, a long startup strip.</summary>
    ChargedSpazerRightStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerRight"/> showing the Spazer Startup picture, horizontal alternate, a long startup strip.</summary>
    ChargedSpazerRightHorizontalAlternateStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerRight"/> showing the Charged Spazer picture, left, seed.</summary>
    ChargedSpazerRightLeftSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerRight"/> showing the Charged Spazer picture, horizontal alternate, seed.</summary>
    ChargedSpazerRightHorizontalAlternateSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerRight"/> showing the Charged Spazer picture, left, its lanes parting.</summary>
    ChargedSpazerRightLeftParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerRight"/> showing the Charged Spazer picture, horizontal alternate, its lanes parting.</summary>
    ChargedSpazerRightHorizontalAlternateParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerRightLoop"/> showing the Charged Spazer picture, left, its lanes fully spread.</summary>
    ChargedSpazerRightLeftSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerRightLoop"/> showing the Charged Spazer picture, horizontal alternate, its lanes fully spread.</summary>
    ChargedSpazerRightHorizontalAlternateSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerDownRight"/> showing the Spazer Startup picture, down right, a short startup strip.</summary>
    ChargedSpazerDownRightStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerDownRight"/> showing the Spazer Startup picture, down right alternate, a short startup strip.</summary>
    ChargedSpazerDownRightAlternateStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerDownRight"/> showing the Spazer Startup picture, down right, a long startup strip.</summary>
    ChargedSpazerDownRightStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerDownRight"/> showing the Spazer Startup picture, down right alternate, a long startup strip.</summary>
    ChargedSpazerDownRightAlternateStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerDownRight"/> showing the Charged Spazer picture, down right, seed.</summary>
    ChargedSpazerDownRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerDownRight"/> showing the Charged Spazer picture, down right alternate, seed.</summary>
    ChargedSpazerDownRightAlternateSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerDownRight"/> showing the Charged Spazer picture, down right, its lanes parting.</summary>
    ChargedSpazerDownRightParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerDownRight"/> showing the Charged Spazer picture, down right alternate, its lanes parting.</summary>
    ChargedSpazerDownRightAlternateParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerDownRightLoop"/> showing the Charged Spazer picture, down right, its lanes fully spread.</summary>
    ChargedSpazerDownRightSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerDownRightLoop"/> showing the Charged Spazer picture, down right alternate, its lanes fully spread.</summary>
    ChargedSpazerDownRightAlternateSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRight"/> showing the Spazer Startup picture, up, a short startup strip.</summary>
    ChargedSpazerWaveUpFacingRightStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRight"/> showing the Spazer Startup picture, vertical alternate, a short startup strip.</summary>
    ChargedSpazerWaveUpFacingRightVerticalAlternateStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRight"/> showing the Spazer Startup picture, up, a long startup strip.</summary>
    ChargedSpazerWaveUpFacingRightStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRight"/> showing the Spazer Startup picture, vertical alternate, a long startup strip.</summary>
    ChargedSpazerWaveUpFacingRightVerticalAlternateStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRightLoop"/> showing the Charged Spazer picture, up, seed.</summary>
    ChargedSpazerWaveUpFacingRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRightLoop"/> showing the Charged Spazer picture, vertical alternate, seed.</summary>
    ChargedSpazerWaveUpFacingRightVerticalAlternateSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRightLoop"/> showing the Charged Spazer picture, up, its lanes splitting.</summary>
    ChargedSpazerWaveUpFacingRightSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRightLoop"/> showing the Charged Spazer picture, vertical alternate, its lanes splitting.</summary>
    ChargedSpazerWaveUpFacingRightVerticalAlternateSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRightLoop"/> showing the Charged Spazer picture, up, its lanes parting.</summary>
    ChargedSpazerWaveUpFacingRightParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRightLoop"/> showing the Charged Spazer picture, vertical alternate, its lanes parting.</summary>
    ChargedSpazerWaveUpFacingRightVerticalAlternateParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRightLoop"/> showing the Charged Spazer picture, up, its lanes spreading.</summary>
    ChargedSpazerWaveUpFacingRightSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRightLoop"/> showing the Charged Spazer picture, vertical alternate, its lanes spreading.</summary>
    ChargedSpazerWaveUpFacingRightVerticalAlternateSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRightLoop"/> showing the Charged Spazer picture, up, its lanes nearly spread.</summary>
    ChargedSpazerWaveUpFacingRightNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRightLoop"/> showing the Charged Spazer picture, vertical alternate, its lanes nearly spread.</summary>
    ChargedSpazerWaveUpFacingRightVerticalAlternateNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRightLoop"/> showing the Charged Spazer picture, up, its lanes fully spread.</summary>
    ChargedSpazerWaveUpFacingRightSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRightLoop"/> showing the Charged Spazer picture, vertical alternate, its lanes fully spread.</summary>
    ChargedSpazerWaveUpFacingRightVerticalAlternateSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRightLoop"/> showing the Charged Spazer picture, up, its lanes splitting, as <see cref="ChargedSpazerWaveUpFacingRightSplitting"/> does, with a narrower hitbox and leaving no trail.</summary>
    ChargedSpazerWaveUpFacingRightSplittingNarrowHitboxTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpFacingRightLoop"/> showing the Charged Spazer picture, vertical alternate, its lanes splitting, as <see cref="ChargedSpazerWaveUpFacingRightVerticalAlternateSplitting"/> does, with a narrower hitbox and leaving no trail.</summary>
    ChargedSpazerWaveUpFacingRightVerticalAlternateSplittingNarrowHitboxTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRight"/> showing the Spazer Startup picture, up right, a short startup strip.</summary>
    ChargedSpazerWaveUpRightStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRight"/> showing the Spazer Startup picture, up right alternate, a short startup strip.</summary>
    ChargedSpazerWaveUpRightAlternateStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRight"/> showing the Spazer Startup picture, up right, a long startup strip.</summary>
    ChargedSpazerWaveUpRightStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRight"/> showing the Spazer Startup picture, up right alternate, a long startup strip.</summary>
    ChargedSpazerWaveUpRightAlternateStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightLoop"/> showing the Charged Spazer picture, up right, seed.</summary>
    ChargedSpazerWaveUpRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightLoop"/> showing the Charged Spazer picture, up right alternate, seed.</summary>
    ChargedSpazerWaveUpRightAlternateSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightLoop"/> showing the Charged Spazer picture, up right, its lanes splitting.</summary>
    ChargedSpazerWaveUpRightSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightLoop"/> showing the Charged Spazer picture, up right alternate, its lanes splitting.</summary>
    ChargedSpazerWaveUpRightAlternateSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightLoop"/> showing the Charged Spazer picture, up right, its lanes parting.</summary>
    ChargedSpazerWaveUpRightParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightLoop"/> showing the Charged Spazer picture, up right alternate, its lanes parting.</summary>
    ChargedSpazerWaveUpRightAlternateParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightLoop"/> showing the Charged Spazer picture, up right, its lanes spreading.</summary>
    ChargedSpazerWaveUpRightSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightLoop"/> showing the Charged Spazer picture, up right alternate, its lanes spreading.</summary>
    ChargedSpazerWaveUpRightAlternateSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightLoop"/> showing the Charged Spazer picture, up right, its lanes nearly spread.</summary>
    ChargedSpazerWaveUpRightNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightLoop"/> showing the Charged Spazer picture, up right alternate, its lanes nearly spread.</summary>
    ChargedSpazerWaveUpRightAlternateNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightLoop"/> showing the Charged Spazer picture, up right, its lanes fully spread.</summary>
    ChargedSpazerWaveUpRightSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightLoop"/> showing the Charged Spazer picture, up right alternate, its lanes fully spread.</summary>
    ChargedSpazerWaveUpRightAlternateSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightLoop"/> showing the Charged Spazer picture, up right, its lanes splitting, as <see cref="ChargedSpazerWaveUpRightSplitting"/> does, leaving no trail.</summary>
    ChargedSpazerWaveUpRightSplittingTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpRightLoop"/> showing the Charged Spazer picture, up right alternate, its lanes splitting, as <see cref="ChargedSpazerWaveUpRightAlternateSplitting"/> does, leaving no trail.</summary>
    ChargedSpazerWaveUpRightAlternateSplittingTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRight"/> showing the Spazer Startup picture, right, a short startup strip.</summary>
    ChargedSpazerWaveRightStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRight"/> showing the Spazer Startup picture, horizontal alternate, a short startup strip.</summary>
    ChargedSpazerWaveRightHorizontalAlternateStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRight"/> showing the Spazer Startup picture, right, a long startup strip.</summary>
    ChargedSpazerWaveRightStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRight"/> showing the Spazer Startup picture, horizontal alternate, a long startup strip.</summary>
    ChargedSpazerWaveRightHorizontalAlternateStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightLoop"/> showing the Charged Spazer picture, right, seed.</summary>
    ChargedSpazerWaveRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightLoop"/> showing the Charged Spazer picture, horizontal alternate, seed.</summary>
    ChargedSpazerWaveRightHorizontalAlternateSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightLoop"/> showing the Charged Spazer picture, right, its lanes splitting.</summary>
    ChargedSpazerWaveRightSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightLoop"/> showing the Charged Spazer picture, horizontal alternate, its lanes splitting.</summary>
    ChargedSpazerWaveRightHorizontalAlternateSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightLoop"/> showing the Charged Spazer picture, right, its lanes parting.</summary>
    ChargedSpazerWaveRightParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightLoop"/> showing the Charged Spazer picture, horizontal alternate, its lanes parting.</summary>
    ChargedSpazerWaveRightHorizontalAlternateParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightLoop"/> showing the Charged Spazer picture, right, its lanes spreading.</summary>
    ChargedSpazerWaveRightSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightLoop"/> showing the Charged Spazer picture, horizontal alternate, its lanes spreading.</summary>
    ChargedSpazerWaveRightHorizontalAlternateSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightLoop"/> showing the Charged Spazer picture, right, its lanes nearly spread.</summary>
    ChargedSpazerWaveRightNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightLoop"/> showing the Charged Spazer picture, horizontal alternate, its lanes nearly spread.</summary>
    ChargedSpazerWaveRightHorizontalAlternateNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightLoop"/> showing the Charged Spazer picture, right, its lanes fully spread.</summary>
    ChargedSpazerWaveRightSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightLoop"/> showing the Charged Spazer picture, horizontal alternate, its lanes fully spread.</summary>
    ChargedSpazerWaveRightHorizontalAlternateSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightLoop"/> showing the Charged Spazer picture, right, its lanes splitting, as <see cref="ChargedSpazerWaveRightSplitting"/> does, with a narrower hitbox and leaving no trail.</summary>
    ChargedSpazerWaveRightSplittingNarrowHitboxTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveRightLoop"/> showing the Charged Spazer picture, horizontal alternate, its lanes splitting, as <see cref="ChargedSpazerWaveRightHorizontalAlternateSplitting"/> does, with a narrower hitbox and leaving no trail.</summary>
    ChargedSpazerWaveRightHorizontalAlternateSplittingNarrowHitboxTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRight"/> showing the Spazer Startup picture, down right, a short startup strip.</summary>
    ChargedSpazerWaveDownRightStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRight"/> showing the Spazer Startup picture, down right alternate, a short startup strip.</summary>
    ChargedSpazerWaveDownRightAlternateStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRight"/> showing the Spazer Startup picture, down right, a long startup strip.</summary>
    ChargedSpazerWaveDownRightStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRight"/> showing the Spazer Startup picture, down right alternate, a long startup strip.</summary>
    ChargedSpazerWaveDownRightAlternateStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightLoop"/> showing the Charged Spazer picture, down right, seed.</summary>
    ChargedSpazerWaveDownRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightLoop"/> showing the Charged Spazer picture, down right alternate, seed.</summary>
    ChargedSpazerWaveDownRightAlternateSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightLoop"/> showing the Charged Spazer picture, down right, its lanes splitting.</summary>
    ChargedSpazerWaveDownRightSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightLoop"/> showing the Charged Spazer picture, down right alternate, its lanes splitting.</summary>
    ChargedSpazerWaveDownRightAlternateSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightLoop"/> showing the Charged Spazer picture, down right, its lanes parting.</summary>
    ChargedSpazerWaveDownRightParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightLoop"/> showing the Charged Spazer picture, down right alternate, its lanes parting.</summary>
    ChargedSpazerWaveDownRightAlternateParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightLoop"/> showing the Charged Spazer picture, down right, its lanes spreading.</summary>
    ChargedSpazerWaveDownRightSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightLoop"/> showing the Charged Spazer picture, down right alternate, its lanes spreading.</summary>
    ChargedSpazerWaveDownRightAlternateSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightLoop"/> showing the Charged Spazer picture, down right, its lanes nearly spread.</summary>
    ChargedSpazerWaveDownRightNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightLoop"/> showing the Charged Spazer picture, down right alternate, its lanes nearly spread.</summary>
    ChargedSpazerWaveDownRightAlternateNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightLoop"/> showing the Charged Spazer picture, down right, its lanes fully spread.</summary>
    ChargedSpazerWaveDownRightSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightLoop"/> showing the Charged Spazer picture, down right alternate, its lanes fully spread.</summary>
    ChargedSpazerWaveDownRightAlternateSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightLoop"/> showing the Charged Spazer picture, down right, its lanes splitting, as <see cref="ChargedSpazerWaveDownRightSplitting"/> does, leaving no trail.</summary>
    ChargedSpazerWaveDownRightSplittingTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownRightLoop"/> showing the Charged Spazer picture, down right alternate, its lanes splitting, as <see cref="ChargedSpazerWaveDownRightAlternateSplitting"/> does, leaving no trail.</summary>
    ChargedSpazerWaveDownRightAlternateSplittingTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRight"/> showing the Spazer Startup picture, down, a short startup strip.</summary>
    ChargedSpazerWaveDownFacingRightStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRight"/> showing the Spazer Startup picture, vertical alternate, a short startup strip.</summary>
    ChargedSpazerWaveDownFacingRightVerticalAlternateStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRight"/> showing the Spazer Startup picture, down, a long startup strip.</summary>
    ChargedSpazerWaveDownFacingRightStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRight"/> showing the Spazer Startup picture, vertical alternate, a long startup strip.</summary>
    ChargedSpazerWaveDownFacingRightVerticalAlternateStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRightLoop"/> showing the Charged Spazer picture, down, seed.</summary>
    ChargedSpazerWaveDownFacingRightSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRightLoop"/> showing the Charged Spazer picture, vertical alternate, seed.</summary>
    ChargedSpazerWaveDownFacingRightVerticalAlternateSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRightLoop"/> showing the Charged Spazer picture, down, its lanes splitting.</summary>
    ChargedSpazerWaveDownFacingRightSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRightLoop"/> showing the Charged Spazer picture, vertical alternate, its lanes splitting.</summary>
    ChargedSpazerWaveDownFacingRightVerticalAlternateSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRightLoop"/> showing the Charged Spazer picture, down, its lanes parting.</summary>
    ChargedSpazerWaveDownFacingRightParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRightLoop"/> showing the Charged Spazer picture, vertical alternate, its lanes parting.</summary>
    ChargedSpazerWaveDownFacingRightVerticalAlternateParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRightLoop"/> showing the Charged Spazer picture, down, its lanes spreading.</summary>
    ChargedSpazerWaveDownFacingRightSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRightLoop"/> showing the Charged Spazer picture, vertical alternate, its lanes spreading.</summary>
    ChargedSpazerWaveDownFacingRightVerticalAlternateSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRightLoop"/> showing the Charged Spazer picture, down, its lanes nearly spread.</summary>
    ChargedSpazerWaveDownFacingRightNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRightLoop"/> showing the Charged Spazer picture, vertical alternate, its lanes nearly spread.</summary>
    ChargedSpazerWaveDownFacingRightVerticalAlternateNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRightLoop"/> showing the Charged Spazer picture, down, its lanes fully spread.</summary>
    ChargedSpazerWaveDownFacingRightSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRightLoop"/> showing the Charged Spazer picture, vertical alternate, its lanes fully spread.</summary>
    ChargedSpazerWaveDownFacingRightVerticalAlternateSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRightLoop"/> showing the Charged Spazer picture, down, its lanes splitting, as <see cref="ChargedSpazerWaveDownFacingRightSplitting"/> does, with a narrower hitbox and leaving no trail.</summary>
    ChargedSpazerWaveDownFacingRightSplittingNarrowHitboxTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownFacingRightLoop"/> showing the Charged Spazer picture, vertical alternate, its lanes splitting, as <see cref="ChargedSpazerWaveDownFacingRightVerticalAlternateSplitting"/> does, with a narrower hitbox and leaving no trail.</summary>
    ChargedSpazerWaveDownFacingRightVerticalAlternateSplittingNarrowHitboxTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeft"/> showing the Spazer Startup picture, up right, a short startup strip.</summary>
    ChargedSpazerWaveDownLeftUpRightStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeft"/> showing the Spazer Startup picture, up right alternate, a short startup strip.</summary>
    ChargedSpazerWaveDownLeftUpRightAlternateStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeft"/> showing the Spazer Startup picture, up right, a long startup strip.</summary>
    ChargedSpazerWaveDownLeftUpRightStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeft"/> showing the Spazer Startup picture, up right alternate, a long startup strip.</summary>
    ChargedSpazerWaveDownLeftUpRightAlternateStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftLoop"/> showing the Charged Spazer picture, down left, seed.</summary>
    ChargedSpazerWaveDownLeftSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftLoop"/> showing the Charged Spazer picture, up right alternate, seed.</summary>
    ChargedSpazerWaveDownLeftUpRightAlternateSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftLoop"/> showing the Charged Spazer picture, down left, its lanes splitting.</summary>
    ChargedSpazerWaveDownLeftSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftLoop"/> showing the Charged Spazer picture, up right alternate, its lanes splitting.</summary>
    ChargedSpazerWaveDownLeftUpRightAlternateSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftLoop"/> showing the Charged Spazer picture, down left, its lanes parting.</summary>
    ChargedSpazerWaveDownLeftParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftLoop"/> showing the Charged Spazer picture, up right alternate, its lanes parting.</summary>
    ChargedSpazerWaveDownLeftUpRightAlternateParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftLoop"/> showing the Charged Spazer picture, down left, its lanes spreading.</summary>
    ChargedSpazerWaveDownLeftSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftLoop"/> showing the Charged Spazer picture, up right alternate, its lanes spreading.</summary>
    ChargedSpazerWaveDownLeftUpRightAlternateSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftLoop"/> showing the Charged Spazer picture, down left, its lanes nearly spread.</summary>
    ChargedSpazerWaveDownLeftNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftLoop"/> showing the Charged Spazer picture, up right alternate, its lanes nearly spread.</summary>
    ChargedSpazerWaveDownLeftUpRightAlternateNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftLoop"/> showing the Charged Spazer picture, down left, its lanes fully spread.</summary>
    ChargedSpazerWaveDownLeftSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftLoop"/> showing the Charged Spazer picture, up right alternate, its lanes fully spread.</summary>
    ChargedSpazerWaveDownLeftUpRightAlternateSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftLoop"/> showing the Charged Spazer picture, down left, its lanes splitting, as <see cref="ChargedSpazerWaveDownLeftSplitting"/> does, leaving no trail.</summary>
    ChargedSpazerWaveDownLeftSplittingTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveDownLeftLoop"/> showing the Charged Spazer picture, up right alternate, its lanes splitting, as <see cref="ChargedSpazerWaveDownLeftUpRightAlternateSplitting"/> does, leaving no trail.</summary>
    ChargedSpazerWaveDownLeftUpRightAlternateSplittingTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeft"/> showing the Spazer Startup picture, left, a short startup strip.</summary>
    ChargedSpazerWaveLeftStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeft"/> showing the Spazer Startup picture, horizontal alternate, a short startup strip.</summary>
    ChargedSpazerWaveLeftHorizontalAlternateStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeft"/> showing the Spazer Startup picture, left, a long startup strip.</summary>
    ChargedSpazerWaveLeftStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeft"/> showing the Spazer Startup picture, horizontal alternate, a long startup strip.</summary>
    ChargedSpazerWaveLeftHorizontalAlternateStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftLoop"/> showing the Charged Spazer picture, left, seed.</summary>
    ChargedSpazerWaveLeftSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftLoop"/> showing the Charged Spazer picture, horizontal alternate, seed.</summary>
    ChargedSpazerWaveLeftHorizontalAlternateSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftLoop"/> showing the Charged Spazer picture, left, its lanes splitting.</summary>
    ChargedSpazerWaveLeftSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftLoop"/> showing the Charged Spazer picture, horizontal alternate, its lanes splitting.</summary>
    ChargedSpazerWaveLeftHorizontalAlternateSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftLoop"/> showing the Charged Spazer picture, left, its lanes parting.</summary>
    ChargedSpazerWaveLeftParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftLoop"/> showing the Charged Spazer picture, horizontal alternate, its lanes parting.</summary>
    ChargedSpazerWaveLeftHorizontalAlternateParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftLoop"/> showing the Charged Spazer picture, left, its lanes spreading.</summary>
    ChargedSpazerWaveLeftSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftLoop"/> showing the Charged Spazer picture, horizontal alternate, its lanes spreading.</summary>
    ChargedSpazerWaveLeftHorizontalAlternateSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftLoop"/> showing the Charged Spazer picture, left, its lanes nearly spread.</summary>
    ChargedSpazerWaveLeftNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftLoop"/> showing the Charged Spazer picture, horizontal alternate, its lanes nearly spread.</summary>
    ChargedSpazerWaveLeftHorizontalAlternateNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftLoop"/> showing the Charged Spazer picture, left, its lanes fully spread.</summary>
    ChargedSpazerWaveLeftSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftLoop"/> showing the Charged Spazer picture, horizontal alternate, its lanes fully spread.</summary>
    ChargedSpazerWaveLeftHorizontalAlternateSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftLoop"/> showing the Charged Spazer picture, left, its lanes splitting, as <see cref="ChargedSpazerWaveLeftSplitting"/> does, with a narrower hitbox and leaving no trail.</summary>
    ChargedSpazerWaveLeftSplittingNarrowHitboxTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveLeftLoop"/> showing the Charged Spazer picture, horizontal alternate, its lanes splitting, as <see cref="ChargedSpazerWaveLeftHorizontalAlternateSplitting"/> does, with a narrower hitbox and leaving no trail.</summary>
    ChargedSpazerWaveLeftHorizontalAlternateSplittingNarrowHitboxTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeft"/> showing the Spazer Startup picture, up left, a short startup strip.</summary>
    ChargedSpazerWaveUpLeftStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeft"/> showing the Spazer Startup picture, down right alternate, a short startup strip.</summary>
    ChargedSpazerWaveUpLeftDownRightAlternateStartupShort,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeft"/> showing the Spazer Startup picture, up left, a long startup strip.</summary>
    ChargedSpazerWaveUpLeftStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeft"/> showing the Spazer Startup picture, down right alternate, a long startup strip.</summary>
    ChargedSpazerWaveUpLeftDownRightAlternateStartupLong,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftLoop"/> showing the Charged Spazer picture, up left, seed.</summary>
    ChargedSpazerWaveUpLeftSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftLoop"/> showing the Charged Spazer picture, down right alternate, seed.</summary>
    ChargedSpazerWaveUpLeftDownRightAlternateSeed,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftLoop"/> showing the Charged Spazer picture, up left, its lanes splitting.</summary>
    ChargedSpazerWaveUpLeftSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftLoop"/> showing the Charged Spazer picture, down right alternate, its lanes splitting.</summary>
    ChargedSpazerWaveUpLeftDownRightAlternateSplitting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftLoop"/> showing the Charged Spazer picture, up left, its lanes parting.</summary>
    ChargedSpazerWaveUpLeftParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftLoop"/> showing the Charged Spazer picture, down right alternate, its lanes parting.</summary>
    ChargedSpazerWaveUpLeftDownRightAlternateParting,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftLoop"/> showing the Charged Spazer picture, up left, its lanes spreading.</summary>
    ChargedSpazerWaveUpLeftSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftLoop"/> showing the Charged Spazer picture, down right alternate, its lanes spreading.</summary>
    ChargedSpazerWaveUpLeftDownRightAlternateSpreading,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftLoop"/> showing the Charged Spazer picture, up left, its lanes nearly spread.</summary>
    ChargedSpazerWaveUpLeftNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftLoop"/> showing the Charged Spazer picture, down right alternate, its lanes nearly spread.</summary>
    ChargedSpazerWaveUpLeftDownRightAlternateNearlySpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftLoop"/> showing the Charged Spazer picture, up left, its lanes fully spread.</summary>
    ChargedSpazerWaveUpLeftSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftLoop"/> showing the Charged Spazer picture, down right alternate, its lanes fully spread.</summary>
    ChargedSpazerWaveUpLeftDownRightAlternateSpread,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftLoop"/> showing the Charged Spazer picture, up left, its lanes splitting, as <see cref="ChargedSpazerWaveUpLeftSplitting"/> does, leaving no trail.</summary>
    ChargedSpazerWaveUpLeftSplittingTrailless,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedSpazerWaveUpLeftLoop"/> showing the Charged Spazer picture, down right alternate, its lanes splitting, as <see cref="ChargedSpazerWaveUpLeftDownRightAlternateSplitting"/> does, leaving no trail.</summary>
    ChargedSpazerWaveUpLeftDownRightAlternateSplittingTrailless,
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
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpFacingRightLoop"/> showing the Plasma Wave picture, up alternate, core: the alternate of the long core it shows with, with its hitbox and trail placements.</summary>
    ChargedPlasmaWaveUpFacingRightAlternateLongCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpFacingRightLoop"/> showing the Plasma Wave picture, up long, its lobes near the centre.</summary>
    ChargedPlasmaWaveUpFacingRightLongLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpFacingRightLoop"/> showing the Plasma Wave picture, up alternate, its lobes near the centre.</summary>
    ChargedPlasmaWaveUpFacingRightAlternateLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpFacingRightLoop"/> showing the Plasma Wave picture, up long, its lobes midway out.</summary>
    ChargedPlasmaWaveUpFacingRightLongLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpFacingRightLoop"/> showing the Plasma Wave picture, up alternate, its lobes midway out.</summary>
    ChargedPlasmaWaveUpFacingRightAlternateLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpFacingRightLoop"/> showing the Plasma Wave picture, up long, its lobes far out.</summary>
    ChargedPlasmaWaveUpFacingRightLongLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpFacingRightLoop"/> showing the Plasma Wave picture, up alternate, its lobes far out.</summary>
    ChargedPlasmaWaveUpFacingRightAlternateLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpFacingRightLoop"/> showing the Plasma Wave picture, up long, its lobes at their peak.</summary>
    ChargedPlasmaWaveUpFacingRightLongLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpFacingRightLoop"/> showing the Plasma Wave picture, up alternate, its lobes at their peak.</summary>
    ChargedPlasmaWaveUpFacingRightAlternateLobesPeak,
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
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpRightLoop"/> showing the Plasma Wave picture, up right alternate, core: the alternate of the long core it shows with, with its hitbox and trail placements.</summary>
    ChargedPlasmaWaveUpRightAlternateLongCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpRightLoop"/> showing the Plasma Wave picture, up right long, its lobes near the centre.</summary>
    ChargedPlasmaWaveUpRightLongLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpRightLoop"/> showing the Plasma Wave picture, up right alternate, its lobes near the centre.</summary>
    ChargedPlasmaWaveUpRightAlternateLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpRightLoop"/> showing the Plasma Wave picture, up right long, its lobes midway out.</summary>
    ChargedPlasmaWaveUpRightLongLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpRightLoop"/> showing the Plasma Wave picture, up right alternate, its lobes midway out.</summary>
    ChargedPlasmaWaveUpRightAlternateLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpRightLoop"/> showing the Plasma Wave picture, up right long, its lobes far out.</summary>
    ChargedPlasmaWaveUpRightLongLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpRightLoop"/> showing the Plasma Wave picture, up right alternate, its lobes far out.</summary>
    ChargedPlasmaWaveUpRightAlternateLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpRightLoop"/> showing the Plasma Wave picture, up right long, its lobes at their peak.</summary>
    ChargedPlasmaWaveUpRightLongLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveUpRightLoop"/> showing the Plasma Wave picture, up right alternate, its lobes at their peak.</summary>
    ChargedPlasmaWaveUpRightAlternateLobesPeak,
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
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRightLoop"/> showing the Plasma Wave picture, right alternate, core: the alternate of the long core it shows with, with its hitbox and trail placements.</summary>
    ChargedPlasmaWaveRightAlternateLongCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRightLoop"/> showing the Plasma Wave picture, right long, its lobes near the centre.</summary>
    ChargedPlasmaWaveRightLongLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRightLoop"/> showing the Plasma Wave picture, right alternate, its lobes near the centre.</summary>
    ChargedPlasmaWaveRightAlternateLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRightLoop"/> showing the Plasma Wave picture, right long, its lobes midway out.</summary>
    ChargedPlasmaWaveRightLongLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRightLoop"/> showing the Plasma Wave picture, right alternate, its lobes midway out.</summary>
    ChargedPlasmaWaveRightAlternateLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRightLoop"/> showing the Plasma Wave picture, right long, its lobes far out.</summary>
    ChargedPlasmaWaveRightLongLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRightLoop"/> showing the Plasma Wave picture, right alternate, its lobes far out.</summary>
    ChargedPlasmaWaveRightAlternateLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRightLoop"/> showing the Plasma Wave picture, right long, its lobes at their peak.</summary>
    ChargedPlasmaWaveRightLongLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveRightLoop"/> showing the Plasma Wave picture, right alternate, its lobes at their peak.</summary>
    ChargedPlasmaWaveRightAlternateLobesPeak,
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
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveDownRightLoop"/> showing the Plasma Wave picture, down right alternate, core: the alternate of the long core it shows with, with its hitbox and trail placements.</summary>
    ChargedPlasmaWaveDownRightAlternateLongCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveDownRightLoop"/> showing the Plasma Wave picture, down right long, its lobes near the centre.</summary>
    ChargedPlasmaWaveDownRightLongLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveDownRightLoop"/> showing the Plasma Wave picture, down right alternate, its lobes near the centre.</summary>
    ChargedPlasmaWaveDownRightAlternateLobesNear,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveDownRightLoop"/> showing the Plasma Wave picture, down right long, its lobes midway out.</summary>
    ChargedPlasmaWaveDownRightLongLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveDownRightLoop"/> showing the Plasma Wave picture, down right alternate, its lobes midway out.</summary>
    ChargedPlasmaWaveDownRightAlternateLobesMidway,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveDownRightLoop"/> showing the Plasma Wave picture, down right long, its lobes far out.</summary>
    ChargedPlasmaWaveDownRightLongLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveDownRightLoop"/> showing the Plasma Wave picture, down right alternate, its lobes far out.</summary>
    ChargedPlasmaWaveDownRightAlternateLobesFar,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveDownRightLoop"/> showing the Plasma Wave picture, down right long, its lobes at their peak.</summary>
    ChargedPlasmaWaveDownRightLongLobesPeak,
    /// <summary>A frame of <see cref="SamusProjectileSegment.ChargedPlasmaWaveDownRightLoop"/> showing the Plasma Wave picture, down right alternate, its lobes at their peak.</summary>
    ChargedPlasmaWaveDownRightAlternateLobesPeak,
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
    /// <summary>A frame of <see cref="SamusProjectileSegment.ShinesparkEchoUpFacingRight"/> showing nothing.</summary>
    ShinesparkEchoUpFacingRightHidden,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerSpecialAttackTrailUpFacingRight"/> showing nothing.</summary>
    SpazerSpecialAttackTrailUpFacingRightHidden,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerSpecialAttackTrailUpFacingRight"/> showing nothing, as <see cref="SpazerSpecialAttackTrailUpFacingRightHidden"/> does, with a wider hitbox and its trail's sparkles placed further apart.</summary>
    SpazerSpecialAttackTrailUpFacingRightHiddenWide,
    /// <summary>A frame of <see cref="SamusProjectileSegment.SpazerSpecialAttackTrailUpFacingRight"/> showing nothing, as <see cref="SpazerSpecialAttackTrailUpFacingRightHidden"/> does, with the widest hitbox and its trail's sparkles placed furthest apart.</summary>
    SpazerSpecialAttackTrailUpFacingRightHiddenWidest,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveSpecialAttack"/> showing the Charged Wave picture, core.</summary>
    WaveSpecialAttackCore,
    /// <summary>A frame of <see cref="SamusProjectileSegment.WaveSpecialAttack"/> showing the Charged Wave picture, core alternate.</summary>
    WaveSpecialAttackCoreAlternate,
}
