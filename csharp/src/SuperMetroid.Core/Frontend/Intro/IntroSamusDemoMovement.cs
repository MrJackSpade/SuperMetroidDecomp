using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Grounded Samus alpha/beta subset shared by the discovery and Mother Brain intro demos.
/// </summary>
/// <remarks>
/// The shared movement and pose classes remain the authorities. This coordinator preserves
/// their native frame order—sample prospective pose, move in the old pose, animate, then
/// commit the prospective/fallback pose—without pulling the unrelated HUD, enemy, FX, and
/// camera owners from the full gameplay runtime into a cinematic.
/// </remarks>
internal static class IntroSamusDemoMovement
{
    /// <summary>Runs the left-facing intro demo's alpha/beta movement, then animation and native prospective-pose commit in gameplay order.</summary>
    public static void StepGroundedLeft(

        RoomLevelData level,
        SamusState samus,
        SnesButton heldInput,
        SnesButton newlyPressedInput,
        int frameCounter,
        CartridgeRandom random)
    {
        SamusPoseTransitionLookup lookup = SamusPoseTransitionTable.Lookup(
            samus.Pose,
            heldInput,
            newlyPressedInput);
        SamusPoseTransition? prospective = lookup.Transition;

        SamusPoseId? fallback = null;
        if (lookup.UsesPoseDefinitionFallback)
        {
            if (SamusPoseMetadata.IsLeftFacingRunningPose(samus.Pose))
            {
                // $91:82D9 preserves the running pose during deceleration and consults
                // definition byte two only once both base-speed halves reach exact zero.
                fallback = samus.HorizontalSpeed.BaseFixed != 0
                    ? samus.Pose
                    : SamusPoseMetadata.ReadNoInputFallbackPose(samus);
            }
            else if (SamusPoseMetadata.IsLeftFacingStandingPose(samus.Pose))
            {
                SamusPoseId noInputPose = SamusPoseMetadata.ReadNoInputFallbackPose(samus);
                if (noInputPose != SamusMovementRomData.Poses.RetainCurrentPoseFallback && noInputPose != samus.Pose)
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
                heldInput);
        }
        else if (SamusPoseMetadata.IsLeftFacingStandingPose(poseAtFrameStart))
        {
            // The demo handler has written its held word into the controller input that
            // $90:A3BA tests, so a held shot holds the standing animation here too.
            movement = SamusGroundedMovement.StepStanding(level, samus, frameCounter,
                heldInput);
        }
        else
        {
            // This controller is installed only for the SR388 left-facing run/stand list;
            // a different pose means cinematic ownership escaped its declared state machine.
            throw new InvalidOperationException(
                $"SR388 intro demo reached invalid grounded-left pose ${(int)poseAtFrameStart:X2}.");
        }

        SamusPresentation.AnimateNoFx(samus, new SamusAnimationStep { ControllerInput = heldInput, NmiFrameCounter = frameCounter, Random = random });

        // Native pose commit probes a proposed run before installing it. The clear
        // probe retains its movement; this is shared gameplay behavior, not cosmetic
        // alignment. No-input running fallbacks use the same prospective slot.
        SamusPoseId? candidate = prospective is { } proposed
            ? proposed.ProspectivePose
            : fallback;
        SamusPoseId? wallPose = SamusGroundedPoseTransitions.CheckProspectiveRunningPoseForWall(samus, level, candidate,
            movement.Horizontal.Collided, out _);
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
            // Momentum command one rechecks speed after beta movement. A remaining residue
            // selects deceleration mode two; exact zero lets next frame choose standing.
            samus.HorizontalSpeed.AccelerationMode =
                samus.HorizontalSpeed.BaseFixed != 0 ? SamusAccelerationMode.Decelerating : SamusAccelerationMode.Accelerating;
        }
        else if (fallback is { } fallbackPose)
        {
            samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Accelerating;
            ApplyPoseTransition(samus, poseAtFrameStart, fallbackPose);
        }

        // The native transition epilogue shifts history for an accepted slot even
        // when deceleration retains the same pose. Idle frames have no slot and
        // must preserve the older history used by subsequent movement decisions.
        if (fallback.HasValue)
            samus.CommitPoseHistory();
    }

    /// <summary>Dispatches only the retail left-facing jump, run/stand, and grounded aiming transitions admitted by the SR388 demo.</summary>
    private static void ApplyPoseTransition(
        SamusState samus,
        SamusPoseId sourcePose,
        SamusPoseId targetPose)
    {
        if (sourcePose == targetPose)
            return;

        if ((SamusPoseMetadata.IsLeftFacingRunningPose(sourcePose) && targetPose == SamusPoseId.SpinJumpLeftPose) ||
            (SamusPoseMetadata.IsLeftFacingStandingPose(sourcePose) && targetPose == SamusPoseId.NeutralJumpTransitionLeftPose))
        {
            SamusAerialPoseTransitions.ApplyOrdinaryJumpTransition(samus, targetPose);
            return;
        }

        if (sourcePose == SamusPoseId.FacingLeftNormalPose &&
            targetPose == SamusPoseId.MovingLeftNormalPose)
        {
            SamusGroundedPoseTransitions.ApplyStandingLeftToRunningLeft(samus);
            return;
        }

        if (sourcePose == SamusPoseId.MovingLeftNormalPose &&
            targetPose == SamusPoseId.FacingLeftNormalPose)
        {
            SamusGroundedPoseTransitions.ApplyRunningLeftToStandingLeft(samus);
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
            $"SR388 intro demo pose transition ${(int)sourcePose:X2} -> ${(int)targetPose:X2} is not present in its retail transition family.");
    }
}
