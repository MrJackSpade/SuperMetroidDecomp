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
    }
}
