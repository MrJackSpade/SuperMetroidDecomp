using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus;

/// <summary>The prospective pose chosen when the input-transition lookup fails; its momentum response is applied by the pose-transition phase.</summary>
public readonly record struct SamusLookupFailurePose(SamusPoseId ProspectivePose)
{
    /// <summary>
    /// Resolves the pose-definition fallback with its momentum response.
    /// Decelerate retains the current pose while base X speed remains and otherwise
    /// becomes Stop; every other response consults the pose definition's fallback pose, where an
    /// absent fallback retains the current pose.
    /// </summary>
    public static SamusLookupFailurePose Resolve(
        SamusMovementType movementType,
        SamusPoseId currentPose,
        SamusPoseId definitionFallbackPose,
        bool hasBaseXSpeed)
    {
        if (hasBaseXSpeed &&
            SamusMomentumResponses.ForLookupFailure(movementType) == SamusMomentumResponse.Decelerate)
            return new SamusLookupFailurePose(currentPose);
        SamusPoseId pose = definitionFallbackPose == SamusMovementDefinitions.Poses.RetainCurrentPoseFallback
            ? currentPose
            : definitionFallbackPose;
        return new SamusLookupFailurePose(pose);
    }
}
