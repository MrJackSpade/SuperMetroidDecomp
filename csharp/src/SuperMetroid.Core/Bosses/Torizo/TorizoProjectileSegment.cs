namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Bomb and Golden Torizo's projectile animations (#641, #1788): one for each animation a spawn, a frame behaviour, a
/// reaction, a sleeping egg's wake or another animation's cue starts, named for what it shows. Each is its steps, the
/// phases it plays them in and how it ends (<see cref="TorizoProjectileAnimations"/>).
/// </summary>
internal enum TorizoProjectileSegment
{
    /// <summary>A low-health explosion: it moves in front of Samus, harmless, then three times returns to its origin, scatters and plays its five poses, then is deleted.</summary>
    LowHealthExplosion,
    /// <summary>A death explosion: one time in four it puffs into <see cref="DeathSmoke"/> instead; otherwise it twice returns to its origin, scatters and plays its six poses, then is deleted.</summary>
    DeathExplosion,
    /// <summary>A death explosion's smoke puff: it twice returns to its origin, scatters and plays its four poses, then is deleted.</summary>
    DeathSmoke,
    /// <summary>Bomb Torizo's drool, forming for four updates: the drop's forming poses, then <see cref="DroolNoDelay"/>'s release and fall.</summary>
    DroolFourFrameDelay,
    /// <summary>Bomb Torizo's drool, forming for two updates: the drop's last forming pose, then <see cref="DroolNoDelay"/>'s release and fall.</summary>
    DroolTwoFrameDelay,
    /// <summary>Bomb Torizo's drool released at once: it starts falling, harmless, then shows its falling pose for good.</summary>
    DroolNoDelay,
    /// <summary>A drool drop reaching a wall: it stops and is deleted.</summary>
    DroolWallImpact,
    /// <summary>A drool drop splashing on the floor: it stops, plays its three splash poses, then is deleted.</summary>
    DroolFloorImpact,
    /// <summary>Torizo's explosive swipe: its sound and five poses, then deletion.</summary>
    ExplosiveSwipe,
    /// <summary>The breaking statue hand's first fragment: it rests 128 updates, cracks and falls, then is deleted.</summary>
    FirstStatueFragment,
    /// <summary>The second fragment: it rests 120 updates, cracks and falls, then is deleted.</summary>
    SecondStatueFragment,
    /// <summary>The third fragment: it rests 112 updates, cracks and falls, then is deleted.</summary>
    ThirdStatueFragment,
    /// <summary>The fourth fragment: it rests 104 updates, cracks and falls, then is deleted.</summary>
    FourthStatueFragment,
    /// <summary>The fifth fragment: it rests 96 updates, cracks and falls, then is deleted.</summary>
    FifthStatueFragment,
    /// <summary>The sixth fragment: it rests 88 updates, cracks and falls, then is deleted.</summary>
    SixthStatueFragment,
    /// <summary>The seventh fragment: it rests 80 updates, cracks and falls, then is deleted.</summary>
    SeventhStatueFragment,
    /// <summary>The eighth fragment: it rests 72 updates, cracks and falls, then is deleted.</summary>
    EighthStatueFragment,
    /// <summary>The ninth fragment: it rests 64 updates, cracks and falls, then is deleted.</summary>
    NinthStatueFragment,
    /// <summary>The tenth fragment: it rests 64 updates, cracks and falls, then is deleted.</summary>
    TenthStatueFragment,
    /// <summary>The eleventh fragment: it rests 64 updates, cracks and falls, then is deleted.</summary>
    EleventhStatueFragment,
    /// <summary>The twelfth fragment: it rests 64 updates, cracks and falls, then is deleted.</summary>
    TwelfthStatueFragment,
    /// <summary>The thirteenth fragment: it rests 64 updates, cracks and falls, then is deleted.</summary>
    ThirteenthStatueFragment,
    /// <summary>The fourteenth fragment: it rests 64 updates, cracks and falls, then is deleted.</summary>
    FourteenthStatueFragment,
    /// <summary>The fifteenth fragment: it rests 64 updates, cracks and falls, then is deleted.</summary>
    FifteenthStatueFragment,
    /// <summary>The sixteenth fragment: it rests 64 updates, cracks and falls, then is deleted.</summary>
    SixteenthStatueFragment,
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
    /// <summary>A sonic boom thrown right: its sound and two firing poses, then its flying pose for good.</summary>
    SonicBoomFiredRight,
    /// <summary>A sonic boom bursting on a wall: it stops, turns harmless and moves in front, then five times returns to its origin, scatters and plays its five poses, then is deleted.</summary>
    SonicBoomWallImpact,
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
    /// <summary>A hatched egg's creature charging left: its sound, then its four poses for good.</summary>
    EggHatchedLeft,
    /// <summary>A hatched egg's creature charging right: its sound, then its four poses for good.</summary>
    EggHatchedRight,
    /// <summary>A shot egg breaking facing left: it stops, plays its five poses, then is deleted.</summary>
    EggBreakLeft,
    /// <summary>A shot egg breaking facing right: it stops, plays its five poses, then is deleted.</summary>
    EggBreakRight,
    /// <summary>A caught super missile thrown right: held, then aimed at Samus and thrown, spinning for good.</summary>
    SuperMissileRight,
    /// <summary>A caught super missile thrown left: held, then aimed away from Samus and thrown, spinning for good.</summary>
    SuperMissileLeft,
    /// <summary>A thrown super missile exploding: it widens, stops, turns harmful, plays its sound and six poses, then is deleted.</summary>
    SuperMissileImpact,
    /// <summary>The eye beam bursting on a wall: it stops, plays its five poses, then is deleted.</summary>
    EyeBeamWallImpact,
    /// <summary>
    /// The eye beam reaching the floor: it stops and sparks, sparking again while Golden Torizo's eye-beam explosions are
    /// disabled, then explodes and is deleted.
    /// </summary>
    EyeBeamFloorImpact,
    /// <summary>The eye beam in flight: its five poses, for good.</summary>
    EyeBeam,
}
