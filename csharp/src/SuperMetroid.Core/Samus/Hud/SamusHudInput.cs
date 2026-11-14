using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Hud;

/// <summary>Shared admission decisions from the game's HUD steps for each movement type.</summary>
internal static class SamusHudInput
{
    /// <summary>
    /// Whether a posture transition admits weapons: morph and unmorph art preserve the charge unless active Grapple is
    /// being cancelled; a pose outside the transition table answers as <paramref name="beyond"/> decoded.
    /// </summary>
    public static bool PostureTransitionAdmitsWeapons(SamusPoseId pose, bool grappleActive,
        PostureAdmissionBeyondTransitions<SamusPoseId> beyond)
    {
        SamusPostureTransitionAdmission admission = SamusPoseMetadata.PostureTransitionAdmission(pose);
        return admission switch
        {
            SamusPostureTransitionAdmission.AlwaysAdmits => true,
            SamusPostureTransitionAdmission.NeverAdmits => false,
            SamusPostureTransitionAdmission.TableAdmits or SamusPostureTransitionAdmission.TablePreservesCharge or
                SamusPostureTransitionAdmission.BeyondTable => SamusHudObservations.PostureAdmitsWeapons(pose, beyond) || grappleActive,
            _ => throw new InvalidOperationException($"Undefined {nameof(SamusPostureTransitionAdmission)} {admission}."),
        };
    }
}
