using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus;

/// <summary>The prospective pose chosen when Samus's input chooses no new pose; its momentum response is applied by the pose-transition phase.</summary>
public readonly record struct SamusNoTransitionPose(SamusPoseId ProspectivePose)
{
    /// <summary>
    /// Chooses her pose when her input chooses no new one, with its momentum response. Decelerate keeps the current
    /// pose while base X speed remains and otherwise becomes Stop; every other response takes the pose's no-input
    /// fallback, which may keep the current pose.
    /// </summary>
    public static SamusNoTransitionPose Resolve(
        SamusMovementType movementType,
        SamusPoseId currentPose,
        SamusNoInputPose definitionFallback,
        bool hasBaseXSpeed)
    {
        if (hasBaseXSpeed &&
            SamusMomentumResponses.ForNoTransition(movementType) == SamusMomentumResponse.Decelerate)
            return new SamusNoTransitionPose(currentPose);
        return new SamusNoTransitionPose(definitionFallback.From(currentPose));
    }
}
