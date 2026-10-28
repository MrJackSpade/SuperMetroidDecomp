namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>The control words of a file-select text stream; every other word is a tilemap cell.</summary>
internal enum FileSelectTextControl
{
    /// <summary>The stream ends.</summary>
    End = FileSelectTilemapFormat.End,

    /// <summary>The following cells start the next row.</summary>
    NextRow = FileSelectTilemapFormat.NextRow,
}
