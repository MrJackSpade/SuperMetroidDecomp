using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Verification.Samus.Movement;

/// <summary>Checks the poses the pose movement names within their movement type's movement (#2291).</summary>
internal static class SamusPoseMovementTypeVerification
{
    /// <summary>
    /// Confirms the pose movement's dispatch by movement type reaches each pose's own step: the forward-facing poses and
    /// every landing are standing poses, the aimed crouched turns are turns while jumping, and the morph transitions and the
    /// Draygon-held stand-up are posture transitions, so each type's movement names them; the unaimed crouched turns are
    /// turns on the ground.
    /// </summary>
    internal static void VerifyPoseMovementTypesNameTheirExceptions()
    {
        SamusPoseId[] poses = Enum.GetValues<SamusPoseId>();
        void AllOf(Func<SamusPoseId, bool> named, SamusMovementType movement, int count, string what)
        {
            SamusPoseId[] found = [.. poses.Where(named)];
            AssertEqual(count, found.Length, $"{what} are {count} poses");
            foreach (SamusPoseId pose in found)
                AssertEqual(movement, SamusPoseMetadata.ReadMovementType(pose), $"{what}: {pose} moves as {movement}");
        }

        AllOf(SamusPoseMetadata.IsForwardFacingPose, SamusMovementType.Standing, 2, "the forward-facing poses");
        AllOf(SamusPoseMetadata.IsLandingPose, SamusMovementType.Standing, 12, "the landings");
        AllOf(SamusPoseMetadata.IsAimedCrouchingTurnPose, SamusMovementType.TurningWhileJumping, 6, "the aimed crouched turns");
        AllOf(SamusPoseMetadata.IsMorphTransitionPose, SamusMovementType.PostureTransition, 4, "the morph transitions");
        AllOf(pose => pose == SamusPoseId.DraygonHeldStandingUpLeftPose, SamusMovementType.PostureTransition, 1,
            "the Draygon-held stand-up");
        AllOf(pose => pose is SamusPoseId.TurningRightToLeftCrouchingPose or SamusPoseId.TurningLeftToRightCrouchingPose,
            SamusMovementType.TurningOnGround, 2, "the unaimed crouched turns");
        Console.WriteLine("Pose movement: each pose a movement type's movement names moves as that type.");
    }
}
