namespace SuperMetroid.Core.Game;

/// <summary>
/// Verified movement-dispatch indices from pose-definition byte one. This is an ordinary
/// enum, not a flag set: each pose selects exactly one bank-$90 movement handler.
/// </summary>
public enum SamusMovementType : byte
{
    /// <summary>Grounded standing poses; accepts ordinary aim, fire, crouch, jump, and run transitions.</summary>
    Standing = 0x00,
    /// <summary>Grounded horizontal movement with acceleration, momentum, and speed-boost handling.</summary>
    Running = 0x01,
    /// <summary>Upright aerial movement for non-spinning jumps and rising aim poses.</summary>
    NormalJumping = 0x02,
    /// <summary>Rotating aerial movement used by spin jump, Space Jump, and Screw Attack poses.</summary>
    SpinJumping = 0x03,
    /// <summary>Grounded morph-ball movement, rolling collision bounds, and bomb interaction.</summary>
    MorphBallGround = 0x04,
    /// <summary>Grounded crouch poses and their stand/morph transitions.</summary>
    Crouching = 0x05,
    /// <summary>Upright descending aerial movement after the vertical-speed apex.</summary>
    Falling = 0x06,
    /// <summary>Unused retail dispatcher slot <c>$07</c> associated with a glitch-ball pose family.</summary>
    UnusedGlitchBall = 0x07,
    /// <summary>Descending morph-ball movement with airborne ball collision bounds.</summary>
    MorphBallFalling = 0x08,
    /// <summary>Second unused retail glitch-ball dispatcher slot, at index <c>$09</c>.</summary>
    UnusedGlitchBallAlternate = 0x09,
    /// <summary>Damage knockback movement until the hurt trajectory returns control to a normal pose.</summary>
    Knockback = 0x0a,
    /// <summary>Unused movement-dispatch table entry <c>$0B</c>; preserved for cartridge pose identity.</summary>
    Unused0B = 0x0b,
    /// <summary>Unused movement-dispatch table entry <c>$0C</c>; preserved for cartridge pose identity.</summary>
    Unused0C = 0x0c,
    /// <summary>Unused movement-dispatch table entry <c>$0D</c>, referenced only by unused pose records.</summary>
    Unused0D = 0x0d,
    /// <summary>Grounded turn animation that defers the facing reversal until its pose sequence advances.</summary>
    TurningOnGround = 0x0e,
    /// <summary>Short transition animations between standing, crouching, and morph-ball postures.</summary>
    PostureTransition = 0x0f,
    /// <summary>Backward grounded movement that keeps Samus facing opposite her travel direction.</summary>
    Moonwalking = 0x10,
    /// <summary>Grounded morph-ball movement with Spring Ball's jump transition enabled.</summary>
    SpringBallGround = 0x11,
    /// <summary>Rising Spring Ball movement before the vertical-speed apex.</summary>
    SpringBallInAir = 0x12,
    /// <summary>Descending Spring Ball movement after the vertical-speed apex.</summary>
    SpringBallFalling = 0x13,
    /// <summary>The wall-jump launch state that applies the away-from-wall trajectory.</summary>
    WallJumping = 0x14,
    /// <summary>Grounded wall-stop animation entered when running momentum meets a blocking wall.</summary>
    RanIntoWall = 0x15,
    /// <summary>Grapple firing, connection, swing, and release movement owned by the grapple subsystem.</summary>
    Grappling = 0x16,
    /// <summary>Midair facing-reversal animation while vertical movement is rising.</summary>
    TurningWhileJumping = 0x17,
    /// <summary>Midair facing-reversal animation while vertical movement is descending.</summary>
    TurningWhileFalling = 0x18,
    /// <summary>Intentional damage-boost trajectory produced by directional input during knockback.</summary>
    DamageBoost = 0x19,
    /// <summary>Controller-limited movement while Draygon owns and repositions Samus.</summary>
    DraygonHeld = 0x1a,
    /// <summary>Scripted movement for shinesparks, Crystal Flash, X-ray, death, and other special poses.</summary>
    Special = 0x1b,
}

/// <summary>
/// Verified pose-definition byte-zero values. This is not a flags enum: a pose has one
/// facing discriminator, and zero remains meaningful for front-view/special records.
/// Unnamed cartridge values remain losslessly representable through the underlying byte.
/// </summary>
public enum SamusFacingDirection : byte
{
    /// <summary>Pose-definition value zero, used by front-view artwork and poses without ordinary left/right facing.</summary>
    ForwardOrSpecial = 0,
    /// <summary>Pose-definition value four, selecting left-facing metadata and artwork.</summary>
    Left = 4,
    /// <summary>Pose-definition value eight, selecting right-facing metadata and artwork.</summary>
    Right = 8,
}

/// <summary>State values stored in Samus's remembered liquid-physics word.</summary>
public enum SamusLiquidMedium : ushort
{
    /// <summary>No liquid physics are active at Samus's movement sample point.</summary>
    Air = 0,
    /// <summary>Water physics apply, including gravity and acceleration changes unless Gravity Suit overrides them.</summary>
    Water = 1,
    /// <summary>Lava or acid physics apply; damage handling distinguishes the actual room-FX type separately.</summary>
    LavaOrAcid = 2,
}

/// <summary>
/// Verified even room-FX dispatcher values from the bank-$83 function table. Values $0E
/// through $1E share a no-op RTL and remain unnamed at the port validation boundary.
/// </summary>
public enum RoomFxType : ushort
{
    /// <summary>No animated bank-$83 room-FX object is present.</summary>
    None = 0x0,
    /// <summary>Animated damaging lava surface with BG3 tiles, palette effects, and optional BG2 distortion.</summary>
    Lava = 0x2,
    /// <summary>Animated damaging acid surface sharing the liquid renderer but using acid-specific gameplay damage.</summary>
    Acid = 0x4,
    /// <summary>Animated water surface with submerged physics and optional per-scanline BG2 displacement.</summary>
    Water = 0x6,
    /// <summary>Full-screen drifting spore atmosphere driven through the room-FX BG3 owner.</summary>
    Spores = 0x8,
    /// <summary>Full-screen rain atmosphere whose BG3 plane is additively composited.</summary>
    Rain = 0x0a,
    /// <summary>Full-screen fog atmosphere with its dedicated additive screen arrangement.</summary>
    Fog = 0x0c,
    /// <summary>Scrolling exterior-sky background driven by the active room's sky tilemap.</summary>
    ScrollingSky = 0x20,
    /// <summary>Unused retail dispatcher entry <c>$22</c> that targets the alternate scrolling-sky routine.</summary>
    UnusedScrollingSky = 0x22,
    /// <summary>Fireflea-room darkness and palette behavior, including enemy-driven light flashes.</summary>
    Fireflea = 0x24,
    /// <summary>Tourian entrance statue liquid effect, rendered through the water-compatible BG3 path.</summary>
    TourianEntranceStatue = 0x26,
    /// <summary>Ceres Ridley encounter effect that coordinates the room's scripted Mode 7 presentation.</summary>
    CeresRidley = 0x28,
    /// <summary>Ceres elevator-shaft effect used during the station's scripted descent and destruction flow.</summary>
    CeresElevator = 0x2a,
    /// <summary>Ceres destruction haze whose palette and layer state progress with the escape sequence.</summary>
    CeresHaze = 0x2c,
}
