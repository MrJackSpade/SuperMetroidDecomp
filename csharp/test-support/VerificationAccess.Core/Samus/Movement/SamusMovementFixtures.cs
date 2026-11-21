using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// The movement a check's Samus moves by (#2553), chosen from her pose's movement type as the pose's own movement chooses
/// it in production (<see cref="SamusPoseMovement"/>), for checks that run one movement step directly.
/// </summary>
internal static class SamusMovementFixtures
{
    /// <summary>The grounded ball's movement her pose selects: a Morph Ball's or a Spring Ball's.</summary>
    /// <exception cref="InvalidOperationException">Her pose moves as no grounded ball.</exception>
    internal static SamusMovement.GroundedBallMovement GroundedBallOf(SamusState samus)
    {
        SamusMovementType type = SamusPoseMetadata.ReadMovementType(samus);
        if (type == SamusMovementType.MorphBallGround)
            return SamusMovement.MorphBallGround;
        if (type == SamusMovementType.SpringBallGround)
            return SamusMovement.SpringBallGround;
        throw new InvalidOperationException($"Samus's pose {samus.Pose} moves as {type}, no grounded ball.");
    }

    /// <summary>The falling ball's movement her pose selects: a Morph Ball's or a Spring Ball's.</summary>
    /// <exception cref="InvalidOperationException">Her pose moves as no falling ball.</exception>
    internal static SamusMovement.FallingBallMovement FallingBallOf(SamusState samus)
    {
        SamusMovementType type = SamusPoseMetadata.ReadMovementType(samus);
        if (type == SamusMovementType.MorphBallFalling)
            return SamusMovement.MorphBallFalling;
        if (type == SamusMovementType.SpringBallFalling)
            return SamusMovement.SpringBallFalling;
        throw new InvalidOperationException($"Samus's pose {samus.Pose} moves as {type}, no falling ball.");
    }

    /// <summary>
    /// The turn on the ground her pose selects: an ordinary turn's, or an aimed crouched turn's, which turns on the ground
    /// while she moves neither up nor down.
    /// </summary>
    /// <exception cref="InvalidOperationException">Her pose turns on no ground.</exception>
    internal static SamusMovement.GroundTurnMovement GroundTurnOf(SamusState samus)
    {
        SamusMovementType type = SamusPoseMetadata.ReadMovementType(samus);
        if (type == SamusMovementType.TurningOnGround)
            return SamusMovement.TurningOnGround;
        if (type == SamusMovementType.TurningRisingOrAimedCrouched && SamusPoseMetadata.IsAimedCrouchingTurnPose(samus.Pose)
            && samus.Kinematics.VerticalVelocity.HasNoDirection)
            return SamusMovement.AimedCrouchedTurningOnGround;
        throw new InvalidOperationException($"Samus's pose {samus.Pose} moves as {type}, no turn on the ground.");
    }

    /// <summary>The crouch her pose selects: the X-ray scope's crouch or the ordinary one.</summary>
    internal static SamusMovement.CrouchingMovement CrouchOf(SamusState samus) =>
        SamusPoseMetadata.IsXrayCrouchingPose(samus.Pose) ? SamusMovement.XrayCrouching : SamusMovement.Crouching;
}
