namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The run of poses a Torizo projectile frame belongs to (#1788): what the projectile shows there. Several animations show
/// the same run, as each of the drool's delays shows the drop's later poses.
/// </summary>
internal enum TorizoProjectileFrameKind
{
    /// <summary>A low-health explosion's five poses.</summary>
    LowHealthExplosion,
    /// <summary>A death explosion's six poses.</summary>
    DeathExplosion,
    /// <summary>A death explosion's smoke puff's four poses.</summary>
    DeathSmoke,
    /// <summary>A drool drop's four poses: forming on Bomb Torizo's jaw over three, then falling.</summary>
    Drool,
    /// <summary>A drool drop splashing on the floor: three poses.</summary>
    DroolSplash,
    /// <summary>An explosive swipe's five poses.</summary>
    ExplosiveSwipe,
    /// <summary>The breaking statue hand's sixteen fragments' two poses each, at rest then falling, fragment by fragment.</summary>
    StatueFragment,
    /// <summary>A Chozo orb flying left.</summary>
    ChozoOrbMovingLeft,
    /// <summary>A Chozo orb flying right.</summary>
    ChozoOrbMovingRight,
    /// <summary>A Chozo orb bursting on a wall: five poses.</summary>
    ChozoOrbWallImpact,
    /// <summary>A Chozo orb bursting on the floor: six poses.</summary>
    ChozoOrbFloorImpact,
    /// <summary>A shot Chozo orb bursting: five poses.</summary>
    ChozoOrbShot,
    /// <summary>A sonic boom leaving Torizo's hand to the left: two poses.</summary>
    SonicBoomFiredLeft,
    /// <summary>A sonic boom flying left.</summary>
    SonicBoomMovingLeft,
    /// <summary>A sonic boom leaving Torizo's hand to the right: two poses.</summary>
    SonicBoomFiredRight,
    /// <summary>A sonic boom flying right.</summary>
    SonicBoomMovingRight,
    /// <summary>A sonic boom bursting on a wall: five poses.</summary>
    SonicBoomWallImpact,
    /// <summary>The dust Torizo's right foot raises on landing: four poses.</summary>
    LandingDustRightFoot,
    /// <summary>The dust Torizo's left foot raises on landing: four poses.</summary>
    LandingDustLeftFoot,
    /// <summary>An egg bouncing left.</summary>
    EggBouncingLeft,
    /// <summary>An egg cracking open, facing left: three poses.</summary>
    EggHatchingLeft,
    /// <summary>An egg bouncing right.</summary>
    EggBouncingRight,
    /// <summary>An egg cracking open, facing right: three poses.</summary>
    EggHatchingRight,
    /// <summary>A hatched egg's creature charging left: four poses.</summary>
    EggHatchedLeft,
    /// <summary>A hatched egg's creature charging right: four poses.</summary>
    EggHatchedRight,
    /// <summary>A shot egg breaking, facing left: five poses.</summary>
    EggBreakLeft,
    /// <summary>A shot egg breaking, facing right: five poses.</summary>
    EggBreakRight,
    /// <summary>A caught super missile held to be thrown right.</summary>
    SuperMissileHeldRight,
    /// <summary>A super missile thrown right, spinning: eight poses.</summary>
    SuperMissileSpinningRight,
    /// <summary>A caught super missile held to be thrown left.</summary>
    SuperMissileHeldLeft,
    /// <summary>A super missile thrown left, spinning: eight poses.</summary>
    SuperMissileSpinningLeft,
    /// <summary>A thrown super missile exploding: six poses.</summary>
    SuperMissileImpact,
    /// <summary>The eye beam bursting on a wall: five poses.</summary>
    EyeBeamWallImpact,
    /// <summary>The eye beam's spark on the floor, then its explosion: seven poses.</summary>
    EyeBeamFloorImpact,
    /// <summary>The eye beam in flight: five poses.</summary>
    EyeBeam,
}
