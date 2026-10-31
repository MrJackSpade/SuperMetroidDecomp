using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Hud;

/// <summary>Shared admission decisions from the cartridge's movement-type HUD dispatch.</summary>
internal static class SamusHudInput
{
    /// <summary>
    /// Whether a posture transition admits weapons: morph and unmorph art preserve the charge unless active Grapple is
    /// being cancelled; a pose outside the transition table answers as <paramref name="beyond"/> decoded.
    /// </summary>
    public static bool PostureTransitionAdmitsWeapons(SamusPoseId pose, bool grappleActive,
        PostureAdmissionBeyondTransitions<SamusPoseId> beyond)
    {
        if (!SamusPoseNumbering.Precedes(pose, SamusHudDefinitions.StandardTransitionStart))
            return true;
        if (!SamusPoseNumbering.Precedes(pose, SamusHudDefinitions.NonFiringTransitionStart))
            return false;
        return SamusHudObservations.PostureAdmitsWeapons(pose, beyond) || grappleActive;
    }
}
