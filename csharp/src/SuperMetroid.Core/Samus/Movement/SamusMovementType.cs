namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Verified movement-dispatch indices from pose-definition byte one. This is an ordinary
/// enum, not a flag set: each pose selects exactly one bank-$90 movement handler.
/// </summary>
public enum SamusMovementType
{
    /// <summary>Grounded standing poses; accepts ordinary aim, fire, crouch, jump, and run transitions.</summary>
    Standing = 0,
    /// <summary>Grounded horizontal movement with acceleration, momentum, and speed-boost handling.</summary>
    Running = 1,
    /// <summary>Upright aerial movement for non-spinning jumps and rising aim poses.</summary>
    NormalJumping = 2,
    /// <summary>Rotating aerial movement used by spin jump, Space Jump, and Screw Attack poses.</summary>
    SpinJumping = 3,
    /// <summary>Grounded morph-ball movement, rolling collision bounds, and bomb interaction.</summary>
    MorphBallGround = 4,
    /// <summary>Grounded crouch poses and their stand/morph transitions.</summary>
    Crouching = 5,
    /// <summary>Upright descending aerial movement after the vertical-speed apex.</summary>
    Falling = 6,
    /// <summary>Unused retail dispatcher slot <c>$07</c> associated with a glitch-ball pose family.</summary>
    UnusedGlitchBall = 7,
    /// <summary>Descending morph-ball movement with airborne ball collision bounds.</summary>
    MorphBallFalling = 8,
    /// <summary>Second unused retail glitch-ball dispatcher slot, at index <c>$09</c>.</summary>
    UnusedGlitchBallAlternate = 9,
    /// <summary>Damage knockback movement until the hurt trajectory returns control to a normal pose.</summary>
    Knockback = 10,
    /// <summary>Unused movement-dispatch table entry <c>$0B</c>; preserved for cartridge pose identity.</summary>
    Unused0B = 11,
    /// <summary>Unused movement-dispatch table entry <c>$0C</c>; preserved for cartridge pose identity.</summary>
    Unused0C = 12,
    /// <summary>Unused movement-dispatch table entry <c>$0D</c>, referenced only by unused pose records.</summary>
    Unused0D = 13,
    /// <summary>Grounded turn animation that defers the facing reversal until its pose sequence advances.</summary>
    TurningOnGround = 14,
    /// <summary>Short transition animations between standing, crouching, and morph-ball postures.</summary>
    PostureTransition = 15,
    /// <summary>Backward grounded movement that keeps Samus facing opposite her travel direction.</summary>
    Moonwalking = 16,
    /// <summary>Grounded morph-ball movement with Spring Ball's jump transition enabled.</summary>
    SpringBallGround = 17,
    /// <summary>Rising Spring Ball movement before the vertical-speed apex.</summary>
    SpringBallInAir = 18,
    /// <summary>Descending Spring Ball movement after the vertical-speed apex.</summary>
    SpringBallFalling = 19,
    /// <summary>The wall-jump launch state that applies the away-from-wall trajectory.</summary>
    WallJumping = 20,
    /// <summary>Grounded wall-stop animation entered when running momentum meets a blocking wall.</summary>
    RanIntoWall = 21,
    /// <summary>Grapple firing, connection, swing, and release movement owned by the grapple subsystem.</summary>
    Grappling = 22,
    /// <summary>Midair facing-reversal animation while vertical movement is rising.</summary>
    TurningWhileJumping = 23,
    /// <summary>Midair facing-reversal animation while vertical movement is descending.</summary>
    TurningWhileFalling = 24,
    /// <summary>Intentional damage-boost trajectory produced by directional input during knockback.</summary>
    DamageBoost = 25,
    /// <summary>Controller-limited movement while Draygon owns and repositions Samus.</summary>
    DraygonHeld = 26,
    /// <summary>Scripted movement for shinesparks, Crystal Flash, X-ray, death, and other special poses.</summary>
    Special = 27,
}
