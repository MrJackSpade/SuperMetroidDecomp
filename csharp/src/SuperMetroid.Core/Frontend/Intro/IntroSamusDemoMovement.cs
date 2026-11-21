using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Grounded Samus input/movement subset shared by the discovery and Mother Brain intro demos.
/// </summary>
/// <remarks>
/// The shared movement and pose classes remain the authorities. This coordinator preserves
/// their frame order—sample prospective pose, move in the old pose, animate, then
/// commit the prospective/fallback pose—without pulling the unrelated HUD, enemy, FX, and
/// camera owners from the full gameplay runtime into a cinematic.
/// </remarks>
internal static partial class IntroSamusDemoMovement
{
    /// <summary>Runs the left-facing intro demo's input and movement phases, then animation and prospective-pose commit in gameplay order.</summary>
    public static void StepGroundedLeft(

        RoomLevelData level,
        SamusState samus,
        GameplayAction heldInput,
        GameplayAction newlyPressedInput,
        int frameCounter,
        SharedRandom random)
    {
        SamusPoseTransitionLookup lookup = SamusPoseInputTransitions.Lookup(
            samus.Pose,
            heldInput,
            newlyPressedInput);
        SamusPoseTransition? prospective = lookup.Transition;

        SamusPoseId? fallback = null;
        if (lookup.UsesPoseDefinitionFallback)
        {
            if (SamusPoseMetadata.IsLeftFacingRunningPose(samus.Pose))
            {
                // Running deceleration preserves the running pose and consults
                // the pose's no-input fallback only once her base speed has stopped completely.
                fallback = samus.HorizontalSpeed.HasBaseSpeed
                    ? samus.Pose
                    : SamusPoseMetadata.ReadNoInputPose(samus).From(samus.Pose);
            }
            else if (SamusPoseMetadata.IsLeftFacingStandingPose(samus.Pose))
            {
                if (SamusPoseMetadata.ReadNoInputPose(samus).TryGetPose(out SamusPoseId noInputPose) && noInputPose != samus.Pose)
                    fallback = noInputPose;
            }
        }

        SamusPoseId poseAtFrameStart = samus.Pose;
        GroundedMovementResult movement;
        if (SamusPoseMetadata.IsLeftFacingRunningPose(poseAtFrameStart))
        {
            movement = SamusGroundedMovement.StepRunning(

                level,
                samus,
                frameCounter,
                heldInput,
                IntroCollisionRoomObjects.Instance);
        }
        else if (SamusPoseMetadata.IsLeftFacingStandingPose(poseAtFrameStart))
        {
            // The demo's input phase has written its held buttons into the controller input that
            // the standing animation tests, so a held shot holds the standing animation here too.
            movement = SamusGroundedMovement.StepStanding(level, samus, frameCounter,
                heldInput, IntroCollisionRoomObjects.Instance);
        }
        else
        {
            // The intro demo moves Samus only in the SR388 left-facing run and stand.
            throw new InvalidOperationException(
                $"SR388 intro demo reached invalid grounded-left pose {poseAtFrameStart}.");
        }

        SamusPresentation.Animate(samus, new SamusAnimationStep { ControllerInput = heldInput, FrameCounter = frameCounter, Random = random });

        // The pose commit probes a proposed run before installing it. The clear
        // probe retains its movement; this is shared gameplay behavior, not cosmetic
        // alignment. No-input running fallbacks are probed the same way.
        SamusPoseId? candidate = prospective is { } proposed
            ? proposed.ProspectivePose
            : fallback;
        SamusPoseId? wallPose = SamusGroundedPoseTransitions.CheckProspectiveRunningPoseForWall(samus, level, candidate,
            movement.Horizontal.Collided, IntroCollisionRoomObjects.Instance);
        if (wallPose is { } stoppedPose)
        {
            SamusGroundedPoseTransitions.ApplyRanIntoWallPoseChange(samus, stoppedPose);
            samus.CommitPoseHistory();
            return;
        }

        if (prospective is { } transition)
        {
            ApplyPoseTransition(samus, poseAtFrameStart, transition.ProspectivePose);
            samus.CommitPoseHistory();
            return;
        }

        if (fallback == poseAtFrameStart)
        {
            // Decelerating rechecks speed after movement. A remaining residue
            // selects the decelerating mode; exact zero lets next frame choose standing.
            samus.HorizontalSpeed.AccelerationMode =
                samus.HorizontalSpeed.HasBaseSpeed ? SamusAccelerationMode.Decelerating : SamusAccelerationMode.Accelerating;
        }
        else if (fallback is { } fallbackPose)
        {
            samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Accelerating;
            ApplyPoseTransition(samus, poseAtFrameStart, fallbackPose);
        }

        // Finishing the transition shifts history for an accepted pose even
        // when deceleration retains the same pose. Idle frames accept no pose and
        // keep the older history that later movement decisions read.
        if (fallback.HasValue)
            samus.CommitPoseHistory();
    }

    /// <summary>Runs only the left-facing jump, run/stand, and grounded aiming transitions admitted by the SR388 demo.</summary>
    private static void ApplyPoseTransition(
        SamusState samus,
        SamusPoseId sourcePose,
        SamusPoseId targetPose)
    {
        if (sourcePose == targetPose)
            return;

        if ((SamusPoseMetadata.IsLeftFacingRunningPose(sourcePose) && SamusPoseTraits.IsIn(targetPose, SamusPoseFamily.GenericSpinJump)) ||
            (SamusPoseMetadata.IsLeftFacingStandingPose(sourcePose) && SamusPoseTraits.IsIn(targetPose, SamusPoseFamily.NeutralJumpTransition)))
        {
            SamusAerialPoseTransitions.ApplyOrdinaryJumpTransition(samus, targetPose);
            return;
        }

        if (SamusPoseTraits.IsIn(sourcePose, SamusPoseFamily.NeutralStanding) &&
            SamusPoseTraits.IsIn(targetPose, SamusPoseFamily.NormalRun))
        {
            SamusGroundedPoseTransitions.ApplyStandingToRunning(samus);
            return;
        }

        if (SamusPoseTraits.IsIn(sourcePose, SamusPoseFamily.NormalRun) &&
            SamusPoseTraits.IsIn(targetPose, SamusPoseFamily.NeutralStanding))
        {
            SamusGroundedPoseTransitions.ApplyRunningToStanding(samus);
            return;
        }

        if ((SamusPoseMetadata.IsLeftFacingStandingPose(sourcePose) ||
             SamusPoseMetadata.IsLeftFacingRunningPose(sourcePose)) &&
            (SamusPoseMetadata.IsLeftFacingStandingPose(targetPose) ||
             SamusPoseMetadata.IsLeftFacingRunningPose(targetPose)))
        {
            SamusGroundedPoseTransitions.ApplyGroundedAimTransition(samus, targetPose);
            return;
        }

        throw new InvalidDataException(
            $"SR388 intro demo pose transition {sourcePose} -> {targetPose} is not present in its transition family.");
    }
}
