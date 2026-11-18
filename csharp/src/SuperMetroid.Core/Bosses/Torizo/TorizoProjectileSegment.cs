namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Bomb and Golden Torizo's projectile animation segments (#641, #1788, #2235): each animation a spawn, a motion, a
/// reaction, a resting egg's wake or another animation's cue starts begins at the segment named for what it shows; the runs
/// it repeats, loops on or moves on to are segments of their own (<see cref="TorizoProjectileAnimations"/>).
/// </summary>
internal enum TorizoProjectileSegment
{
    /// <summary>A low-health explosion: it moves in front of Samus, harmless, then three times returns to its origin, scatters and plays its five poses, then is deleted.</summary>
    LowHealthExplosion,
    /// <summary>A low-health explosion's burst: it returns to its origin, scatters and plays its five poses, three times.</summary>
    LowHealthExplosionBurst,
    /// <summary>A death explosion: one time in four it puffs into <see cref="DeathSmoke"/> instead; otherwise it twice returns to its origin, scatters and plays its six poses, then is deleted.</summary>
    DeathExplosion,
    /// <summary>A death explosion's burst: it returns to its origin, scatters and plays its six poses, twice.</summary>
    DeathExplosionBurst,
    /// <summary>A death explosion's smoke puff: it twice returns to its origin, scatters and plays its four poses, then is deleted.</summary>
    DeathSmoke,
    /// <summary>
    /// Bomb Torizo's drool forming, unseen for two updates: a drool that forms plays it once or twice before
    /// <see cref="DroolNoDelay"/>'s release and fall (<see cref="TorizoProjectileAnimations.DroolFormingForTwoUpdates"/>,
    /// <see cref="TorizoProjectileAnimations.DroolFormingForFourUpdates"/>).
    /// </summary>
    DroolForming,
    /// <summary>Bomb Torizo's drool released at once: it starts falling, harmless, then shows its falling pose for good.</summary>
    DroolNoDelay,
    /// <summary>A released drool drop's falling pose, held while it falls.</summary>
    DroolFalling,
    /// <summary>A drool drop reaching a wall: it stops and is deleted.</summary>
    DroolWallImpact,
    /// <summary>A drool drop splashing on the floor: it stops, plays its three splash poses, then is deleted.</summary>
    DroolFloorImpact,
    /// <summary>Torizo's explosive swipe: its sound and five poses, then deletion.</summary>
    ExplosiveSwipe,
    /// <summary>The breaking statue hand's top middle piece (#2240): it rests 128 updates, cracks and falls, then is deleted.</summary>
    StatueFragmentTopMiddle,
    /// <summary>The top right piece: it rests 120 updates, cracks and falls, then is deleted.</summary>
    StatueFragmentTopRight,
    /// <summary>The middle left piece: it rests 112 updates, cracks and falls, then is deleted.</summary>
    StatueFragmentMiddleLeft,
    /// <summary>The centre piece: it rests 104 updates, cracks and falls, then is deleted.</summary>
    StatueFragmentCentre,
    /// <summary>The middle right piece: it rests 96 updates, cracks and falls, then is deleted.</summary>
    StatueFragmentMiddleRight,
    /// <summary>The bottom left piece: it rests 88 updates, cracks and falls, then is deleted.</summary>
    StatueFragmentBottomLeft,
    /// <summary>The bottom middle piece: it rests 80 updates, cracks and falls, then is deleted.</summary>
    StatueFragmentBottomMiddle,
    /// <summary>The bottom right piece: it rests 72 updates, cracks and falls, then is deleted.</summary>
    StatueFragmentBottomRight,
    /// <summary>A Chozo orb flying left, for good.</summary>
    ChozoOrbMovingLeft,
    /// <summary>A Chozo orb flying right, for good.</summary>
    ChozoOrbMovingRight,
    /// <summary>A Chozo orb bursting on a wall: it stops, plays its five poses, then is deleted.</summary>
    ChozoOrbWallImpact,
    /// <summary>A Chozo orb bursting on the floor: it stops, turns harmful, plays its impact sound and six poses, then is deleted.</summary>
    ChozoOrbFloorImpact,
    /// <summary>A shot Chozo orb bursting: it stops, plays its five poses, drops its pickup, then is deleted.</summary>
    ChozoOrbShot,
    /// <summary>A sonic boom thrown left: its sound and two firing poses, then its flying pose for good.</summary>
    SonicBoomFiredLeft,
    /// <summary>A sonic boom flying left, its flying pose held.</summary>
    SonicBoomFlyingLeft,
    /// <summary>A sonic boom thrown right: its sound and two firing poses, then its flying pose for good.</summary>
    SonicBoomFiredRight,
    /// <summary>A sonic boom flying right, its flying pose held.</summary>
    SonicBoomFlyingRight,
    /// <summary>A sonic boom bursting on a wall: it stops, turns harmless and moves in front, then five times returns to its origin, scatters and plays its five poses, then is deleted.</summary>
    SonicBoomWallImpact,
    /// <summary>A sonic boom's burst on a wall: it returns to its origin, scatters and plays its five poses, five times.</summary>
    SonicBoomWallImpactBurst,
    /// <summary>The dust Torizo's right foot raises on landing: its four poses, rising, then deletion.</summary>
    LandingDustRightFoot,
    /// <summary>The dust Torizo's left foot raises on landing: its four poses, rising, then deletion.</summary>
    LandingDustLeftFoot,
    /// <summary>An egg bouncing left until its hatch timer wakes it into <see cref="EggHatchingLeft"/>.</summary>
    EggBouncingLeft,
    /// <summary>An egg hatching facing left: it stops, cracks open, turns harmful and shot-blocking, then hatches into <see cref="EggHatchedLeft"/>.</summary>
    EggHatchingLeft,
    /// <summary>An egg bouncing right until its hatch timer wakes it into <see cref="EggHatchingRight"/>.</summary>
    EggBouncingRight,
    /// <summary>An egg hatching facing right: it stops, cracks open, turns harmful and shot-blocking, then hatches into <see cref="EggHatchedRight"/>.</summary>
    EggHatchingRight,
    /// <summary>A cracked egg's hatch, either way: it turns harmful and shot-blocking, then continues in its hatched animation.</summary>
    EggHatch,
    /// <summary>A hatched egg's creature charging left: its sound, then its four poses for good.</summary>
    EggHatchedLeft,
    /// <summary>A hatched egg's creature charging left: its four poses, looping.</summary>
    HatchlingChargingLeft,
    /// <summary>A hatched egg's creature charging right: its sound, then its four poses for good.</summary>
    EggHatchedRight,
    /// <summary>A hatched egg's creature charging right: its four poses, looping.</summary>
    HatchlingChargingRight,
    /// <summary>A shot egg breaking facing left: it stops, plays its five poses, its flattened remains two updates longer than facing right, then is deleted.</summary>
    EggBreakLeft,
    /// <summary>A shot egg breaking facing right: it stops, plays its five poses, then is deleted.</summary>
    EggBreakRight,
    /// <summary>A caught super missile thrown right: held, then aimed at Samus and thrown, spinning for good.</summary>
    SuperMissileRight,
    /// <summary>A thrown super missile spinning, thrown right: its eight poses, looping.</summary>
    SuperMissileSpinningRight,
    /// <summary>A caught super missile thrown left: held, then aimed away from Samus and thrown, spinning for good.</summary>
    SuperMissileLeft,
    /// <summary>A thrown super missile spinning, thrown left: its eight poses, looping.</summary>
    SuperMissileSpinningLeft,
    /// <summary>A thrown super missile exploding: it widens, stops, turns harmful, plays its sound and six poses, then is deleted.</summary>
    SuperMissileImpact,
    /// <summary>The eye beam bursting on a wall: it stops, plays its five poses, then is deleted.</summary>
    EyeBeamWallImpact,
    /// <summary>
    /// The eye beam reaching the floor: it stops and waits unseen in <see cref="EyeBeamFloorImpactWaiting"/> until Golden
    /// Torizo's eye-beam explosions are enabled, then bursts into <see cref="EyeBeamFloorFlames"/>.
    /// </summary>
    EyeBeamFloorImpact,
    /// <summary>The eye beam's floor impact waits unseen for eight updates, then marks its wait over, again while Golden Torizo's eye-beam explosions are disabled.</summary>
    EyeBeamFloorImpactWaiting,
    /// <summary>The eye beam's floor impact bursts into flames: its impact sound and six flame poses, harmful from its third, then deletion.</summary>
    EyeBeamFloorFlames,
    /// <summary>The eye beam in flight: its five poses, for good.</summary>
    EyeBeam,
    /// <summary>A projectile deleting itself: the end of an explosion's or a burst's repeated run.</summary>
    Deletion,
}
