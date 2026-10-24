namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Cartridge values of <see cref="SamusSourceDefinitions"/> that only verification reads.</summary>
internal static class SamusCartridgeTablesPoseBank
{
    /// <summary>Verification's view of the pose bank.</summary>
    extension(SamusSourceDefinitions)
    {
        /// <summary>Bank $91, which holds pose, animation, and transition tables.</summary>
        internal static int PoseBank => 0x910000;
    }
}
