namespace SuperMetroid.Core.Audio;

/// <summary>Cartridge values of <see cref="SpcDriverData.Ram"/> that only verification reads.</summary>
internal static class SpcDriverDataRamConstants
{
    /// <summary>SPC RAM pointer table at $5820 for uploaded music track starts.</summary>
    public const int MusicTrackPointerTable = 0x5820;

    /// <summary>Provides the SPC RAM music-track pointer table used in driver data comparisons.</summary>
    extension(SpcDriverData.Ram)
    {
        /// <inheritdoc cref="SpcDriverDataRamConstants.MusicTrackPointerTable"/>
        internal static int MusicTrackPointerTable => SpcDriverDataRamConstants.MusicTrackPointerTable;
    }
}
