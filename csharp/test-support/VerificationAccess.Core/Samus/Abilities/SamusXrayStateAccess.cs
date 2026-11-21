using SuperMetroid.Core.Samus.Poses;

using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Verification access to <see cref="SamusXrayState"/> members production does not use.</summary>
internal static class SamusXrayStateAccess
{
    /// <summary>Detects the reserve-recovery X-Ray state with all subsystems suspended but time unfrozen.</summary>
    extension(SamusXrayState self)
    {
        /// <summary>True after Reserve recovery has stranded X-Ray's subsystem disables.</summary>
        internal bool IsGMode => !self.TimeIsFrozen &&
            self.SuspendedSubsystems == XraySuspendedSubsystems.All;

        /// <summary>
        /// Runs X-ray's setup and then its activation as the frame's pose update does when no higher-priority transition
        /// supersedes it: <see cref="SamusXrayState.TryBegin"/> followed by
        /// <see cref="SamusXrayState.CommitActivation"/> with the pose it returns.
        /// </summary>
        /// <returns>Whether the setup admitted X-ray.</returns>
        internal bool TryBeginAndActivate(SamusState samus, SamusMovementType previousMovementType, XrayBeginConditions conditions)
        {
            bool admitted = self.TryBegin(samus, previousMovementType, conditions, out SamusPoseId? activationPose);
            if (activationPose is { } pose)
                self.CommitActivation(samus, pose, superseded: false);
            return admitted;
        }

        /// <summary>Advances the live scope's cone effect by one update, as the frame's movement does while the scope runs.</summary>
        /// <exception cref="InvalidOperationException">No scope is live.</exception>
        internal XrayBeamStepResult StepBeam(SamusState samus, Input.GameplayAction controllerInput) =>
            (self.Scope ?? throw new InvalidOperationException("The check steps an X-ray beam with no live scope.")).StepBeam(samus, controllerInput);

        /// <summary>Handles the scope's pose input, as the frame does while the X-ray holds Samus's control.</summary>
        /// <exception cref="InvalidOperationException">The X-ray holds no control of Samus.</exception>
        internal void HandlePoseInput(SamusState samus, Input.GameplayAction controllerInput) =>
            (self.Control ?? throw new InvalidOperationException("The check hands X-ray pose input it does not take.")).HandlePoseInput(samus, controllerInput);

        /// <summary>Runs the X-ray's movement of Samus, as the frame does while the X-ray holds her control.</summary>
        /// <exception cref="InvalidOperationException">The X-ray holds no control of Samus.</exception>
        internal void StepMovement(SamusState samus) =>
            (self.Control ?? throw new InvalidOperationException("The check steps X-ray movement it does not hold.")).StepMovement(samus);
    }
}
