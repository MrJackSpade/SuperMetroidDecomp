namespace SuperMetroid.Core.Frontend.FileSelect.Artwork;

/// <summary>Where a copy or clear page draws the slot letters it names.</summary>
public enum FileSelectAnchor
{
    /// <summary>The source slot's letter while choosing the destination.</summary>
    CopyDestinationSource,
    /// <summary>The source slot's letter while confirming a copy.</summary>
    CopyConfirmSource,
    /// <summary>The destination slot's letter while confirming a copy.</summary>
    CopyConfirmDestination,
    /// <summary>The slot's letter while confirming a clear.</summary>
    ClearConfirmSource,
}
