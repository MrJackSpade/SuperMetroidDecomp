namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>The supported schema revisions of the Mother Brain room-color document.</summary>
internal enum MotherBrainRoomColorRevision
{
    /// <summary>Revision 1: room-entry and recovery-light artwork are inherited from current stock.</summary>
    PreRoomEntry = 1,
    /// <summary>Revision 2: recovery-light artwork is inherited from current stock.</summary>
    PreRecoveryLights = 2,
    /// <summary>Revision 3: every field is supplied by the document.</summary>
    Current = 3,
}
