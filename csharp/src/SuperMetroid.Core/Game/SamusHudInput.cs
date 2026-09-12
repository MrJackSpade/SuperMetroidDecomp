using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Shared admission decisions from the cartridge's movement-type HUD dispatch.</summary>
internal static class SamusHudInput
{
    /// <summary>DD8C preserves charge in morph/unmorph art unless active Grapple is being cancelled.</summary>
    public static bool PostureTransitionAdmitsWeapons(ISnesAddressSpace bus, byte pose, bool grappleActive)
    {
        if (pose >= SamusHudRomData.StandardTransitionStart)
            return true;
        if (pose >= SamusHudRomData.NonFiringTransitionStart)
            return false;
        // Only twelve poses have authored flags. Other restored pose/type combinations
        // retain the native adjacent-data read rather than acquiring an invented default.
        byte flag = SamusHudDefinitions.TryGetPostureFlag(pose, out byte compiled) ? compiled :
            bus.ReadByte(SamusHudRomData.TransitionFlags + pose - SamusHudRomData.FirstTransitionPose);
        return flag == 0 || grappleActive;
    }
}
