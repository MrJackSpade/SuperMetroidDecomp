namespace SuperMetroid.Core.Assets;

/// <summary>Installation filename and compatible schema revisions for Mother Brain's room and cutscene color artwork.</summary>
public static class MotherBrainRoomColorFormat
{
    /// <summary>Installed JSON filename selecting room-entry, fake-death, phase-two and post-Baby recovery colors.</summary>
    public const string FileName = "mother-brain-room-colors.json";
    /// <summary>Revision 3, requiring room-entry palettes and seven recovery-light images in addition to flash and phase-two colors.</summary>
    public const int Version = 3;
    /// <summary>Revision 1, lacking room-entry and recovery-light artwork; loading requires an explicit current-stock fallback for those fields.</summary>
    public const int PreRoomEntryVersion = 1;
    /// <summary>Revision 2, including room-entry palettes but lacking recovery-light artwork; loading requires an explicit current-stock fallback for recovery images.</summary>
    public const int PreRecoveryLightsVersion = 2;
}
