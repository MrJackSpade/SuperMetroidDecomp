namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>Reads a file-select text stream's control words.</summary>
internal static class FileSelectTextControls
{
    /// <summary>The control <paramref name="word"/> is, or null for a tilemap cell.</summary>
    internal static FileSelectTextControl? Of(int word) => word switch
    {
        FileSelectTilemapFormat.End => FileSelectTextControl.End,
        FileSelectTilemapFormat.NextRow => FileSelectTextControl.NextRow,
        _ => null,
    };
}
