using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.World;

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

        PlainTerrain terrain,
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
        SamusMovementType movementAtFrameStart = SamusPoseMetadata.ReadMovementType(samus);
        // The demo moves her by her pose's movement, as gameplay's movement phase does. The demo's input phase has written
        // its held buttons into the controller input that the standing animation tests, so a held shot holds the standing
        // animation here too. No elevator runs in the intro.
        var frame = new SamusControlFrame();
        SamusPoseMovementRun.Step(SamusPoseMetadata.ReadMovementType(samus.Pose), new SamusPoseMovementRun(samus, terrain, frame,
            heldInput, newlyPressedInput, frameCounter, SamusElevatorFooting.Of(ElevatorActorStatus.Inactive)));
        bool collided = frame.GroundedMovement is { } grounded && grounded.Horizontal.Collided;

        SamusPresentation.Animate(samus, new SamusAnimationStep { Ground = IntroCinematicDefinitions.Flashback.Ground, ControllerInput = heldInput, FrameCounter = frameCounter, Random = random });

        // The pose commit probes a proposed run before installing it. The clear
        // probe retains its movement; this is shared gameplay behavior, not cosmetic
        // alignment. No-input running fallbacks are probed the same way.
        SamusPoseId? candidate = prospective is { } proposed
            ? proposed.ProspectivePose
            : fallback;
        SamusPoseId? wallPose = SamusGroundedPoseTransitions.CheckProspectiveRunningPoseForWall(samus, terrain, candidate,
            collided);
        if (wallPose is { } stoppedPose)
        {
            SamusGroundedPoseTransitions.ApplyRanIntoWallPoseChange(samus, stoppedPose);
            samus.CommitPoseHistory();
            return;
        }

        // The transition, or with none her response to no input, is set up as gameplay sets it up (SamusPoseTransitionPhase).
        var setups = new SamusPoseSetupSteps(heldInput, newlyPressedInput, frameCounter);
        if (prospective is { } transition)
        {
            if (transition.ProspectivePose != poseAtFrameStart)
                transition.Setup.Apply(setups, frame, samus, transition.ProspectivePose, terrain);
            samus.CommitPoseHistory();
            return;
        }

        SamusNoTransitionSetups.For(poseAtFrameStart, movementAtFrameStart).Apply(setups, frame, samus, poseAtFrameStart, fallback);

        // Finishing the transition shifts history for an accepted pose even
        // when deceleration retains the same pose. Idle frames accept no pose and
        // keep the older history that later movement decisions read.
        if (fallback.HasValue)
            samus.CommitPoseHistory();
    }
}
