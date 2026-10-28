namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Verified movement-dispatch indices from pose-definition byte one. This is an ordinary
/// enum, not a flag set: each pose selects exactly one bank-$90 movement handler.
/// </summary>
public enum SamusMovementType
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
