namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>The revisions of the installed Mother Brain room colour document the reader accepts.</summary>
internal enum MotherBrainRoomColorRevision
{
    /// <summary>Revision 1, which inherits its room-entry and recovery-light colours from current stock.</summary>
    PreRoomEntry,

    /// <summary>Revision 2, which inherits only its recovery-light colours from current stock.</summary>
    PreRecoveryLights,

    /// <summary>Revision 3, the current revision, which carries every colour family itself.</summary>
    Current,
}
