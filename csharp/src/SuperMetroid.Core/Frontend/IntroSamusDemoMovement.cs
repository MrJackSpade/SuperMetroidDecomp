using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend;

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
    public static void StepGroundedLeft(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort heldInput,
        ushort newlyPressedInput,
        ushort nmiFrameCounter)
    {
        SamusPoseTransitionLookup lookup = SamusPoseTransitionTable.Lookup(
            bus,
            samus.Pose,
            heldInput,
            newlyPressedInput);
        SamusPoseTransition? prospective = lookup.Transition;

        SamusPoseId? fallback = null;
        if (lookup.UsesPoseDefinitionFallback)
        {
            if (SamusState.IsLeftFacingRunningPose(samus.Pose))
            {
                // $91:82D9 preserves the running pose during deceleration and consults
                // definition byte two only once both base-speed halves reach exact zero.
                fallback = samus.HorizontalSpeed.BaseFixed != 0
                    ? samus.Pose
                    : samus.ReadNoInputFallbackPose(bus);
            }
            else if (SamusState.IsLeftFacingStandingPose(samus.Pose))
            {
                SamusPoseId noInputPose = samus.ReadNoInputFallbackPose(bus);
                if (noInputPose != SamusMovementRomData.Poses.RetainCurrentPoseFallback && noInputPose != samus.Pose)
                    fallback = noInputPose;
            }
        }

        SamusPoseId poseAtFrameStart = samus.Pose;
        GroundedMovementResult movement;
        if (SamusState.IsLeftFacingRunningPose(poseAtFrameStart))
        {
            movement = SamusGroundedMovement.StepRunningLeft(
                bus,
                level,
                samus,
                nmiFrameCounter,
                heldInput);
        }
        else if (SamusState.IsLeftFacingStandingPose(poseAtFrameStart))
        {
            // The demo handler has written its held word into the controller input that
            // $90:A3BA tests, so a held shot holds the standing animation here too.
            movement = SamusGroundedMovement.StepStandingLeft(bus, level, samus, nmiFrameCounter,
                heldInput);
        }
        else
        {
            // This controller is installed only for the SR388 left-facing run/stand list;
            // a different pose means cinematic ownership escaped its declared state machine.
            throw new InvalidOperationException(
                $"SR388 intro demo reached invalid grounded-left pose ${(int)poseAtFrameStart:X2}.");
        }

        samus.AnimateNoFx(bus, heldInput, nmiFrameCounter);

        // Native pose commit probes a proposed run before installing it. The clear
        // probe retains its movement; this is shared gameplay behavior, not cosmetic
        // alignment. No-input running fallbacks use the same prospective slot.
        SamusPoseId? candidate = prospective is { } proposed
            ? proposed.ProspectivePose
            : fallback;
        SamusPoseId? wallPose = samus.CheckProspectiveRunningPoseForWall(bus, level, candidate,
            movement.Horizontal.Collided, out _);
        if (wallPose is { } stoppedPose)
        {
            samus.ApplyRanIntoWallPoseChange(bus, stoppedPose);
            samus.CommitPoseHistory(bus);
            return;
        }

        if (prospective is { } transition)
        {
            ApplyPoseTransition(bus, samus, poseAtFrameStart, transition.ProspectivePose);
            samus.CommitPoseHistory(bus);
            return;
        }

        if (fallback == poseAtFrameStart)
        {
            // Momentum command one rechecks speed after beta movement. A remaining residue
            // selects deceleration mode two; exact zero lets next frame choose standing.
            samus.HorizontalSpeed.AccelerationMode =
                samus.HorizontalSpeed.BaseFixed != 0 ? (ushort)2 : (ushort)0;
        }
        else if (fallback is { } fallbackPose)
        {
            samus.HorizontalSpeed.AccelerationMode = 0;
            ApplyPoseTransition(bus, samus, poseAtFrameStart, fallbackPose);
        }

        // The native transition epilogue shifts history for an accepted slot even
        // when deceleration retains the same pose. Idle frames have no slot and
        // must preserve the older history used by subsequent movement decisions.
        if (fallback.HasValue)
            samus.CommitPoseHistory(bus);
    }

    private static void ApplyPoseTransition(
        ISnesAddressSpace bus,
        SamusState samus,
        SamusPoseId sourcePose,
        SamusPoseId targetPose)
    {
        if (sourcePose == targetPose)
            return;

        if ((SamusState.IsLeftFacingRunningPose(sourcePose) && targetPose == SamusPoseId.SpinJumpLeftPose) ||
            (SamusState.IsLeftFacingStandingPose(sourcePose) && targetPose == SamusPoseId.NeutralJumpTransitionLeftPose))
        {
            samus.ApplyOrdinaryJumpTransition(bus, targetPose);
            return;
        }

        if (sourcePose == SamusPoseId.FacingLeftNormalPose &&
            targetPose == SamusPoseId.MovingLeftNormalPose)
        {
            samus.ApplyStandingLeftToRunningLeft(bus);
            return;
        }

        if (sourcePose == SamusPoseId.MovingLeftNormalPose &&
            targetPose == SamusPoseId.FacingLeftNormalPose)
        {
            samus.ApplyRunningLeftToStandingLeft(bus);
            return;
        }

        if ((SamusState.IsLeftFacingStandingPose(sourcePose) ||
             SamusState.IsLeftFacingRunningPose(sourcePose)) &&
            (SamusState.IsLeftFacingStandingPose(targetPose) ||
             SamusState.IsLeftFacingRunningPose(targetPose)))
        {
            samus.ApplyGroundedAimTransition(bus, targetPose);
            return;
        }

        throw new InvalidDataException(
            $"SR388 intro demo pose transition ${(int)sourcePose:X2} -> ${(int)targetPose:X2} is not present in its retail transition family.");
    }
}
