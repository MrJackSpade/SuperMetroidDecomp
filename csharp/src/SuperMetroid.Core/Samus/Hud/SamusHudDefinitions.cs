using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Hud;

/// <summary>Compiled weapon-admission policy; HUD artwork and labels are separate assets.</summary>
internal static class SamusHudDefinitions
{
    /// <summary>The first posture-transition pose; the transition admission indexes its flags from here.</summary>
    internal const SamusPoseId FirstTransitionPose = SamusPoseId.CrouchingTransitionRightPose;

    /// <summary>The first of the transition poses that never admit firing.</summary>
    internal const SamusPoseId NonFiringTransitionStart = SamusPoseId.UnusedPoseDb;

    /// <summary>The first transition pose that the standard weapon handler admits.</summary>
    internal const SamusPoseId StandardTransitionStart = SamusPoseId.CrouchingTransitionAimUpRightPose;
}
