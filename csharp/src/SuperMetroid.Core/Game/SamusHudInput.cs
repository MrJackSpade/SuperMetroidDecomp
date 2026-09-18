namespace SuperMetroid.Core.Game;

/// <summary>Shared admission decisions from the cartridge's movement-type HUD dispatch.</summary>
internal static class SamusHudInput
{
    /// <summary>DD8C preserves charge in morph/unmorph art unless active Grapple is being cancelled.</summary>
    public static bool PostureTransitionAdmitsWeapons(byte pose, bool grappleActive)
    {
        if (pose >= SamusHudRomData.StandardTransitionStart)
            return true;
        if (pose >= SamusHudRomData.NonFiringTransitionStart)
            return false;
        byte flag = SamusHudDefinitions.PostureObservation(pose);
        return flag == 0 || grappleActive;
    }
}
