namespace SuperMetroid.Core.Assets;

/// <summary>The supported schema revisions of the room palette-FX presentation document.</summary>
internal enum RoomPaletteFxPresentationRevision
{
    /// <summary>Revision 17: the Samus-in-heat rows are inherited from current stock.</summary>
    Previous = RoomPaletteFxPresentationFormat.PreviousVersion,
    /// <summary>Revision 18: every color family is supplied by the document.</summary>
    Current = RoomPaletteFxPresentationFormat.Version,
}
