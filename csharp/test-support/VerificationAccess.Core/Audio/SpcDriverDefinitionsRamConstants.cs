using SuperMetroid.AssetExtraction.Audio.Synthesis;
namespace SuperMetroid.Core.Audio;

/// <summary>Cartridge values of <see cref="SpcDriverDefinitions.Ram"/> that only verification reads.</summary>
internal static class SpcDriverDefinitionsRamConstants
{
    /// <summary>SPC RAM pointer table at $5820 for uploaded music track starts.</summary>
    public const int MusicTrackPointerTable = 0x5820;

    /// <summary>Provides the SPC RAM music-track pointer table used in driver data comparisons.</summary>
    extension(SpcDriverDefinitions.Ram)
    {
        /// <inheritdoc cref="SpcDriverDefinitionsRamConstants.MusicTrackPointerTable"/>
        internal static int MusicTrackPointerTable => SpcDriverDefinitionsRamConstants.MusicTrackPointerTable;
    }
}
