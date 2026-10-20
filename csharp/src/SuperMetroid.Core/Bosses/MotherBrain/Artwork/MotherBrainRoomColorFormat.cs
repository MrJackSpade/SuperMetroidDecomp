namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Installation filename and compatible schema revisions for Mother Brain's room and cutscene color artwork.</summary>
public static class MotherBrainRoomColorFormat
{
    /// <summary>Installed JSON filename selecting room-entry, fake-death, phase-two and post-Baby recovery colors.</summary>
    public const string FileName = "mother-brain-room-colors.json";
    /// <summary>Revision 3, requiring room-entry palettes and seven recovery-light images in addition to flash and phase-two colors.</summary>
    public const int Version = (int)MotherBrainRoomColorRevision.Current;
}
