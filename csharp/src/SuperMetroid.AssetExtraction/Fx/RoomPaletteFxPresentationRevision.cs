namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>The revisions of the installed room palette-effect colour document the reader accepts.</summary>
internal enum RoomPaletteFxPresentationRevision
{
    /// <summary>Revision 17, whose Samus-in-heat rows are inherited from the current stock document.</summary>
    InheritsHeatRows,

    /// <summary>Revision 18, the current revision, which carries every colour family itself.</summary>
    Current,
}
