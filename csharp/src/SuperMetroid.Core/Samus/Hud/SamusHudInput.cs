using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Hud;

/// <summary>Shared admission decisions from the cartridge's movement-type HUD dispatch.</summary>
internal static class SamusHudInput
{
    /// <summary>DD8C preserves charge in morph/unmorph art unless active Grapple is being cancelled.</summary>
    public static bool PostureTransitionAdmitsWeapons(SamusPoseId pose, bool grappleActive, InstalledSamusOverreads? overreads)
    {
        if (!SamusPoseNumbering.Precedes(pose, SamusHudDefinitions.StandardTransitionStart))
            return true;
        if (!SamusPoseNumbering.Precedes(pose, SamusHudDefinitions.NonFiringTransitionStart))
            return false;
        int flag = SamusHudObservations.PostureObservation(pose, overreads);
        return flag == 0 || grappleActive;
    }
}
