namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Verification's native view of Samus's pose animation programs: where the cartridge's table of them sits.</summary>
internal static class SamusAnimationDelayNative
{
    /// <summary>The cartridge place of the pose animation program table, which sparse cartridge fixtures still seed.</summary>
    extension(SamusAnimationDelayDefinitions)
    {
        /// <summary>$91:B010: the table of 253 pose animation program words.</summary>
        internal static int PointerTableAddress => 0x91B010;
    }
}
