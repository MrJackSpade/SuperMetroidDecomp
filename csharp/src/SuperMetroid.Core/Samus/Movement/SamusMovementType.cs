namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// How a pose moves: each pose selects exactly one movement handler. This is an ordinary enum, not a flag set. Its values
/// are arbitrary; the pose definitions name each pose's movement type at import.
/// </summary>
public enum SamusMovementType
{
    /// <summary>Grounded standing poses; accepts ordinary aim, fire, crouch, jump, and run transitions.</summary>
    Standing,
    /// <summary>Grounded horizontal movement with acceleration, momentum, and speed-boost handling.</summary>
    Running,
    /// <summary>Upright aerial movement for non-spinning jumps and rising aim poses.</summary>
    NormalJumping,
    /// <summary>Rotating aerial movement used by spin jump, Space Jump, and Screw Attack poses.</summary>
    SpinJumping,
    /// <summary>Grounded morph-ball movement, rolling collision bounds, and bomb interaction.</summary>
    MorphBallGround,
    /// <summary>Grounded crouch poses and their stand/morph transitions.</summary>
    Crouching,
    /// <summary>Upright descending aerial movement after the vertical-speed apex.</summary>
    Falling,
    /// <summary>Unused retail dispatcher slot <c>$07</c> associated with a glitch-ball pose family.</summary>
    UnusedGlitchBall,
    /// <summary>Descending morph-ball movement with airborne ball collision bounds.</summary>
    MorphBallFalling,
    /// <summary>Second unused retail glitch-ball dispatcher slot, at index <c>$09</c>.</summary>
    UnusedGlitchBallAlternate,
    /// <summary>Damage knockback movement until the hurt trajectory returns control to a normal pose.</summary>
    Knockback,
    /// <summary>Unused movement-dispatch table entry <c>$0B</c>; preserved for cartridge pose identity.</summary>
    Unused0B,
    /// <summary>Unused movement-dispatch table entry <c>$0C</c>; preserved for cartridge pose identity.</summary>
    Unused0C,
    /// <summary>Unused movement-dispatch table entry <c>$0D</c>, referenced only by unused pose records.</summary>
    Unused0D,
    /// <summary>Grounded turn animation that defers the facing reversal until its pose sequence advances.</summary>
    TurningOnGround,
    /// <summary>Short transition animations between standing, crouching, and morph-ball postures.</summary>
    PostureTransition,
    /// <summary>Backward grounded movement that keeps Samus facing opposite her travel direction.</summary>
    Moonwalking,
    /// <summary>Grounded morph-ball movement with Spring Ball's jump transition enabled.</summary>
    SpringBallGround,
    /// <summary>Rising Spring Ball movement before the vertical-speed apex.</summary>
    SpringBallInAir,
    /// <summary>Descending Spring Ball movement after the vertical-speed apex.</summary>
    SpringBallFalling,
    /// <summary>The wall-jump launch state that applies the away-from-wall trajectory.</summary>
    WallJumping,
    /// <summary>Grounded wall-stop animation entered when running momentum meets a blocking wall.</summary>
    RanIntoWall,
    /// <summary>Grapple firing, connection, swing, and release movement owned by the grapple subsystem.</summary>
    Grappling,
    /// <summary>Midair facing-reversal animation while vertical movement is rising.</summary>
    TurningWhileJumping,
    /// <summary>Midair facing-reversal animation while vertical movement is descending.</summary>
    TurningWhileFalling,
    /// <summary>Intentional damage-boost trajectory produced by directional input during knockback.</summary>
    DamageBoost,
    /// <summary>Controller-limited movement while Draygon owns and repositions Samus.</summary>
    DraygonHeld,
    /// <summary>Scripted movement for shinesparks, Crystal Flash, X-ray, death, and other special poses.</summary>
    Special,
}
