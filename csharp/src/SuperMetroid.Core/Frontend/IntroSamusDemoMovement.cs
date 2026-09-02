using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Grounded Samus alpha/beta subset used by the SR388 intro demo.
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

        ushort? fallback = null;
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
                byte noInputPose = samus.ReadNoInputFallbackPose(bus);
                if (noInputPose != 0xff && noInputPose != samus.Pose)
                    fallback = noInputPose;
            }
        }

        byte poseAtFrameStart = samus.Pose;
        if (SamusState.IsLeftFacingRunningPose(poseAtFrameStart))
        {
            SamusGroundedMovement.StepRunningLeft(
                bus,
                level,
                samus,
                nmiFrameCounter,
                heldInput);
        }
        else if (SamusState.IsLeftFacingStandingPose(poseAtFrameStart))
        {
            SamusGroundedMovement.StepStandingLeft(bus, level, samus, nmiFrameCounter);
        }
        else
        {
            // This controller is installed only for the SR388 left-facing run/stand list;
            // a different pose means cinematic ownership escaped its declared state machine.
            throw new InvalidOperationException(
                $"SR388 intro demo reached invalid grounded-left pose ${poseAtFrameStart:X2}.");
        }

        samus.AnimateNoFx(bus, heldInput, nmiFrameCounter);

        if (prospective is { } transition)
        {
            ApplyPoseTransition(bus, samus, poseAtFrameStart, unchecked((byte)transition.ProspectivePose));
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
            ApplyPoseTransition(bus, samus, poseAtFrameStart, unchecked((byte)fallbackPose));
        }
    }

    private static void ApplyPoseTransition(
        ISnesAddressSpace bus,
        SamusState samus,
        byte sourcePose,
        byte targetPose)
    {
        if (sourcePose == targetPose)
            return;

        if (sourcePose == SamusState.FacingLeftNormalPose &&
            targetPose == SamusState.MovingLeftNormalPose)
        {
            samus.ApplyStandingLeftToRunningLeft(bus);
            return;
        }

        if (sourcePose == SamusState.MovingLeftNormalPose &&
            targetPose == SamusState.FacingLeftNormalPose)
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
            $"SR388 intro demo pose transition ${sourcePose:X2} -> ${targetPose:X2} is not present in its retail transition family.");
    }
}
