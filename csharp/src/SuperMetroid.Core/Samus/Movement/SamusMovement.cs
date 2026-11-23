namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// The movement Samus's pose selects by its movement type (#2553), handed to the step that
/// serves it. Each movement is its own type, so a step is handed only a movement it serves and reads the movement type
/// from it, never checking Samus's pose again for a selection her pose already made.
/// </summary>
internal abstract partial class SamusMovement
{
    /// <summary>Only the movements declared here exist.</summary>
    /// <param name="type">The movement type the movement is.</param>
    private SamusMovement(SamusMovementType type) => Type = type;

    /// <summary>The movement type the movement is.</summary>
    internal SamusMovementType Type { get; }

    /// <summary>The normal jump's movement.</summary>
    internal static NormalJumpingMovement NormalJumping { get; } = new();

    /// <summary>The spin jump's movement.</summary>
    internal static SpinJumpingMovement SpinJumping { get; } = new();

    /// <summary>The wall jump's movement.</summary>
    internal static WallJumpingMovement WallJumping { get; } = new();

    /// <summary>The damage boost's movement.</summary>
    internal static DamageBoostMovement DamageBoost { get; } = new();

    /// <summary>The fall's movement.</summary>
    internal static FallingMovement Falling { get; } = new();

    /// <summary>The moonwalk's movement.</summary>
    internal static MoonwalkingMovement Moonwalking { get; } = new();

    /// <summary>
    /// The rising or aimed crouched turn's movement in the air: a jumping turn, or an aimed crouched turn moving up or down.
    /// </summary>
    internal static AerialTurnMovement TurningRisingOrAimedCrouched { get; } = new(SamusMovementType.TurningRisingOrAimedCrouched);

    /// <summary>The falling turn's movement.</summary>
    internal static AerialTurnMovement TurningWhileFalling { get; } = new(SamusMovementType.TurningWhileFalling);

    /// <summary>The ground turn's movement: the standing, crouched and moonwalk turns.</summary>
    internal static GroundTurnMovement TurningOnGround { get; } = new(SamusMovementType.TurningOnGround);

    /// <summary>The aimed crouched turn's movement on the ground: an aimed crouched turn with no vertical direction.</summary>
    internal static GroundTurnMovement AimedCrouchedTurningOnGround { get; } = new(SamusMovementType.TurningRisingOrAimedCrouched);

    /// <summary>The crouch's movement: the crouching poses, unaimed or aimed.</summary>
    internal static CrouchingMovement Crouching { get; } = new();

    /// <summary>The X-ray scope's crouch's movement: the X-ray crouching poses, which the crouching movement type moves too.</summary>
    internal static CrouchingMovement XrayCrouching { get; } = new();

    /// <summary>The grounded Morph Ball's movement.</summary>
    internal static GroundedBallMovement MorphBallGround { get; } = new(SamusMovementType.MorphBallGround);

    /// <summary>The grounded Spring Ball's movement.</summary>
    internal static GroundedBallMovement SpringBallGround { get; } = new(SamusMovementType.SpringBallGround);

    /// <summary>The falling Morph Ball's movement.</summary>
    internal static FallingBallMovement MorphBallFalling { get; } = new(SamusMovementType.MorphBallFalling);

    /// <summary>The falling Spring Ball's movement.</summary>
    internal static FallingBallMovement SpringBallFalling { get; } = new(SamusMovementType.SpringBallFalling);

    /// <summary>The rising Spring Ball jump's movement.</summary>
    internal static SpringBallInAirMovement SpringBallInAir { get; } = new();

    /// <summary>The normal jump's movement (<see cref="SamusMovementType.NormalJumping"/>).</summary>
    internal sealed partial class NormalJumpingMovement : SamusMovement
    {
        /// <summary>The one normal jump's movement.</summary>
        internal NormalJumpingMovement()
            : base(SamusMovementType.NormalJumping)
        {
        }
    }

    /// <summary>The spin jump's movement (<see cref="SamusMovementType.SpinJumping"/>).</summary>
    internal sealed partial class SpinJumpingMovement : SamusMovement
    {
        /// <summary>The one spin jump's movement.</summary>
        internal SpinJumpingMovement()
            : base(SamusMovementType.SpinJumping)
        {
        }
    }

    /// <summary>The wall jump's movement (<see cref="SamusMovementType.WallJumping"/>), which only wall-jump poses move by.</summary>
    internal sealed partial class WallJumpingMovement : SamusMovement
    {
        /// <summary>The one wall jump's movement.</summary>
        internal WallJumpingMovement()
            : base(SamusMovementType.WallJumping)
        {
        }
    }

    /// <summary>The damage boost's movement (<see cref="SamusMovementType.DamageBoost"/>), which only damage-boost poses move by.</summary>
    internal sealed partial class DamageBoostMovement : SamusMovement
    {
        /// <summary>The one damage boost's movement.</summary>
        internal DamageBoostMovement()
            : base(SamusMovementType.DamageBoost)
        {
        }
    }

    /// <summary>The fall's movement (<see cref="SamusMovementType.Falling"/>).</summary>
    internal sealed partial class FallingMovement : SamusMovement
    {
        /// <summary>The one fall's movement.</summary>
        internal FallingMovement()
            : base(SamusMovementType.Falling)
        {
        }
    }

    /// <summary>The moonwalk's movement (<see cref="SamusMovementType.Moonwalking"/>), which only the stable moonwalk poses move by.</summary>
    internal sealed partial class MoonwalkingMovement : SamusMovement
    {
        /// <summary>The one moonwalk's movement.</summary>
        internal MoonwalkingMovement()
            : base(SamusMovementType.Moonwalking)
        {
        }
    }

    /// <summary>
    /// A turn's movement in the air: <see cref="SamusMovementType.TurningRisingOrAimedCrouched"/>, whose poses are the
    /// jumping turns and the aimed crouched turns, or <see cref="SamusMovementType.TurningWhileFalling"/>, whose poses are
    /// the falling turns.
    /// </summary>
    internal sealed partial class AerialTurnMovement : SamusMovement
    {
        /// <summary>The turn's movement in the air of <paramref name="type"/>.</summary>
        /// <param name="type">One of the two turning movement types that turn in the air.</param>
        internal AerialTurnMovement(SamusMovementType type)
            : base(type)
        {
        }
    }

    /// <summary>
    /// A turn's movement on the ground: <see cref="SamusMovementType.TurningOnGround"/>, whose poses are the standing,
    /// crouched and moonwalk turns, or <see cref="SamusMovementType.TurningRisingOrAimedCrouched"/> for an aimed crouched
    /// turn with no vertical direction.
    /// </summary>
    internal sealed partial class GroundTurnMovement : SamusMovement
    {
        /// <summary>The turn's movement on the ground of <paramref name="type"/>.</summary>
        /// <param name="type">One of the two turning movement types that turn on the ground.</param>
        internal GroundTurnMovement(SamusMovementType type)
            : base(type)
        {
        }
    }

    /// <summary>
    /// A crouch's movement (<see cref="SamusMovementType.Crouching"/>): the crouching poses' own, or the X-ray scope's
    /// crouching poses', which have the same movement type and are outside the crouching family.
    /// </summary>
    internal sealed partial class CrouchingMovement : SamusMovement
    {
        /// <summary>A crouch's movement.</summary>
        internal CrouchingMovement()
            : base(SamusMovementType.Crouching)
        {
        }
    }

    /// <summary>
    /// A grounded ball's movement: <see cref="SamusMovementType.MorphBallGround"/>, whose poses are the grounded Morph Ball
    /// poses, or <see cref="SamusMovementType.SpringBallGround"/>, whose poses are the grounded Spring Ball poses.
    /// </summary>
    internal sealed partial class GroundedBallMovement : SamusMovement
    {
        /// <summary>The grounded ball's movement of <paramref name="type"/>.</summary>
        /// <param name="type">One of the two grounded ball movement types.</param>
        internal GroundedBallMovement(SamusMovementType type)
            : base(type)
        {
        }
    }

    /// <summary>
    /// A falling ball's movement: <see cref="SamusMovementType.MorphBallFalling"/>, whose poses are the airborne Morph Ball
    /// poses, or <see cref="SamusMovementType.SpringBallFalling"/>, whose poses are the falling Spring Ball poses.
    /// </summary>
    internal sealed partial class FallingBallMovement : SamusMovement
    {
        /// <summary>The falling ball's movement of <paramref name="type"/>.</summary>
        /// <param name="type">One of the two falling ball movement types.</param>
        internal FallingBallMovement(SamusMovementType type)
            : base(type)
        {
        }
    }

    /// <summary>The rising Spring Ball jump's movement (<see cref="SamusMovementType.SpringBallInAir"/>), which only the Spring Ball jump poses move by.</summary>
    internal sealed partial class SpringBallInAirMovement : SamusMovement
    {
        /// <summary>The one Spring Ball jump's movement.</summary>
        internal SpringBallInAirMovement()
            : base(SamusMovementType.SpringBallInAir)
        {
        }
    }
}
