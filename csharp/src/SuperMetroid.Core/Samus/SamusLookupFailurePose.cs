using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus;

/// <summary>The prospective pose chosen by <c>HandleTransitionTableLookupFailure</c>; its command is applied by the pose-transition phase.</summary>
public readonly record struct SamusLookupFailurePose(SamusPoseId ProspectivePose)
{
    /// <summary>
    /// Ports <c>$91:82D9</c> with <c>Set_ProspectivePoseChangeCommand</c> ($91:8304).
    /// Decelerate retains the current pose while base X speed remains and otherwise
    /// becomes Stop; every other command consults pose-definition byte two, where $FF
    /// retains the current pose.
    /// </summary>
    public static SamusLookupFailurePose Resolve(
        SamusMovementType movementType,
        SamusPoseId currentPose,
        SamusPoseId definitionFallbackPose,
        bool hasBaseXSpeed)
    {
        if (hasBaseXSpeed &&
            SamusPoseChangeCommands.ForLookupFailure(movementType) == SamusProspectivePoseChangeCommand.Decelerate)
            return new SamusLookupFailurePose(currentPose);
        SamusPoseId pose = definitionFallbackPose == SamusMovementDefinitions.Poses.RetainCurrentPoseFallback
            ? currentPose
            : definitionFallbackPose;
        return new SamusLookupFailurePose(pose);
    }
}
