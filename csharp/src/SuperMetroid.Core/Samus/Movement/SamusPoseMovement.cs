namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// The movement a movement type runs for the pose that owns Samus's movement (#2291, #2571): its movement type's
/// definition holds it, and it takes its own step through the update that runs her movement, so no table of handlers keyed
/// by movement type is looked up.
/// </summary>
internal sealed class SamusPoseMovement
{
    /// <summary>The movement's name, for diagnostics.</summary>
    private readonly string _name;

    /// <summary>Takes the movement's step.</summary>
    private readonly Action<ISamusPoseMovementSteps> _step;

    /// <summary>A movement named <paramref name="name"/> that takes <paramref name="step"/>.</summary>
    /// <param name="name">The movement's name, for diagnostics.</param>
    /// <param name="step">Takes the movement's step.</param>
    private SamusPoseMovement(string name, Action<ISamusPoseMovementSteps> step)
    {
        _name = name;
        _step = step;
    }

    /// <summary>The standing movement: the forward-facing poses' own step, otherwise standing's.</summary>
    internal static SamusPoseMovement Standing { get; } = new(nameof(Standing), steps => steps.StepStanding());

    /// <summary>The run.</summary>
    internal static SamusPoseMovement Running { get; } = new(nameof(Running), steps => steps.StepRunning());

    /// <summary>The moonwalk.</summary>
    internal static SamusPoseMovement Moonwalking { get; } = new(nameof(Moonwalking), steps => steps.StepMoonwalking());

    /// <summary>The stop against a wall she ran into.</summary>
    internal static SamusPoseMovement RanIntoWall { get; } = new(nameof(RanIntoWall), steps => steps.StepRanIntoWall());

    /// <summary>The turn on the ground.</summary>
    internal static SamusPoseMovement TurningOnGround { get; } = new(nameof(TurningOnGround), steps => steps.StepTurningOnGround());

    /// <summary>The jumping turns: an aimed crouched turn with no vertical direction turns on the ground, every other one in the air.</summary>
    internal static SamusPoseMovement TurningRisingOrAimedCrouched { get; } = new(nameof(TurningRisingOrAimedCrouched), steps => steps.StepTurningWhileJumping());

    /// <summary>The turn in the air.</summary>
    internal static SamusPoseMovement TurningInAir { get; } = new(nameof(TurningInAir), steps => steps.StepTurningInAir());

    /// <summary>The Morph Ball's movement on the ground.</summary>
    internal static SamusPoseMovement MorphBallGrounded { get; } = new(nameof(MorphBallGrounded), steps => steps.StepBallGrounded(SamusMovement.MorphBallGround));

    /// <summary>The Spring Ball's movement on the ground.</summary>
    internal static SamusPoseMovement SpringBallGrounded { get; } = new(nameof(SpringBallGrounded), steps => steps.StepBallGrounded(SamusMovement.SpringBallGround));

    /// <summary>The Morph Ball's fall.</summary>
    internal static SamusPoseMovement MorphBallFalling { get; } = new(nameof(MorphBallFalling), steps => steps.StepBallFalling(SamusMovement.MorphBallFalling));

    /// <summary>The Spring Ball's fall.</summary>
    internal static SamusPoseMovement SpringBallFalling { get; } = new(nameof(SpringBallFalling), steps => steps.StepBallFalling(SamusMovement.SpringBallFalling));

    /// <summary>The Spring Ball's jump.</summary>
    internal static SamusPoseMovement SpringBallInAir { get; } = new(nameof(SpringBallInAir), steps => steps.StepSpringBallInAir());

    /// <summary>The normal jump.</summary>
    internal static SamusPoseMovement NormalJump { get; } = new(nameof(NormalJump), steps => steps.StepNormalJump());

    /// <summary>The spin jump.</summary>
    internal static SamusPoseMovement SpinJump { get; } = new(nameof(SpinJump), steps => steps.StepSpinJump());

    /// <summary>The wall jump.</summary>
    internal static SamusPoseMovement WallJump { get; } = new(nameof(WallJump), steps => steps.StepWallJump());

    /// <summary>The damage boost.</summary>
    internal static SamusPoseMovement DamageBoost { get; } = new(nameof(DamageBoost), steps => steps.StepDamageBoost());

    /// <summary>The fall.</summary>
    internal static SamusPoseMovement Falling { get; } = new(nameof(Falling), steps => steps.StepFalling());

    /// <summary>The crouch.</summary>
    internal static SamusPoseMovement Crouching { get; } = new(nameof(Crouching), steps => steps.StepCrouching());

    /// <summary>The knockback movement type's normal movement: the one-frame grounding probe before the knockback ending's target.</summary>
    internal static SamusPoseMovement KnockbackEnding { get; } = new(nameof(KnockbackEnding), steps => steps.StepKnockbackEnding());

    /// <summary>The posture transitions: the morph transitions' own step, the Draygon-held stand-up's hold, otherwise the crouch and stand transitions'.</summary>
    internal static SamusPoseMovement PostureTransition { get; } = new(nameof(PostureTransition), steps => steps.StepPostureTransition());

    /// <summary>The glitch ball's normal movement, which only clears the vertical solid-collision result.</summary>
    internal static SamusPoseMovement GlitchBallHold { get; } = new(nameof(GlitchBallHold), steps => steps.HoldWhereInstalled());

    /// <summary>The Draygon-held ball's normal step, which only clears the vertical solid-collision result; the actual boss grab keeps Samus still instead.</summary>
    internal static SamusPoseMovement DraygonHeld { get; } = new(nameof(DraygonHeld), steps => steps.StepDraygonHeld());

    /// <summary>The glitch-ball knockback's movement once its knockback ends: Samus holds still in it (#1535).</summary>
    internal static SamusPoseMovement GlitchBallKnockbackHold { get; } = new(nameof(GlitchBallKnockbackHold), steps => steps.HoldStill());

    /// <summary>Takes the movement's step on one update.</summary>
    /// <param name="steps">The update's steps against the loaded room.</param>
    internal void Step(ISamusPoseMovementSteps steps) => _step(steps);

    /// <summary>Names the movement.</summary>
    public override string ToString() => _name;
}
