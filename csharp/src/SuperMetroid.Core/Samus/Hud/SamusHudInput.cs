using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Hud;

/// <summary>Shared admission decisions from the game's HUD steps for each movement type.</summary>
internal static partial class SamusHudInput
{
    /// <summary>
    /// Whether a posture transition admits weapons: the input transitions admit crouch and stand transitions, and morph and
    /// unmorph art preserve the charge unless active Grapple is being cancelled; a pose outside the input transitions
    /// answers as <paramref name="beyond"/> decoded, or admits them while Grapple is being cancelled.
    /// </summary>
    public static bool PostureTransitionAdmitsWeapons(SamusPoseId pose, bool grappleActive,
        PostureAdmissionBeyondTransitions<SamusPoseId> beyond)
    {
        SamusPostureTransitionAdmission admission = SamusPoseMetadata.PostureTransitionAdmission(pose);
        return admission switch
        {
            SamusPostureTransitionAdmission.AlwaysAdmits => true,
            SamusPostureTransitionAdmission.NeverAdmits => false,
            SamusPostureTransitionAdmission.CrouchOrStandAdmits => true,
            SamusPostureTransitionAdmission.MorphKeepsCharge => grappleActive,
            SamusPostureTransitionAdmission.AnsweredBeyondTransitions => Ensure.NotNull(beyond).AdmitsWeapons(pose) || grappleActive,
            _ => throw new InvalidOperationException($"Undefined {nameof(SamusPostureTransitionAdmission)} {admission}."),
        };
    }
}
