namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Cartridge values and helpers of <see cref="MotherBrainRoomColorFormat"/> that only verification reads.</summary>
internal static class MotherBrainRoomColorFormatConstants
{
    /// <summary>Revision 1, lacking room-entry and recovery-light artwork; loading requires an explicit current-stock fallback for those fields.</summary>
    public const int PreRoomEntryVersion = (int)MotherBrainRoomColorRevision.PreRoomEntry;
    /// <summary>Revision 2, including room-entry palettes but lacking recovery-light artwork; loading requires an explicit current-stock fallback for recovery images.</summary>
    public const int PreRecoveryLightsVersion = (int)MotherBrainRoomColorRevision.PreRecoveryLights;
}
