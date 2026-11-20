namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>
/// The controls of a file-select text stream; every other word is a tilemap cell. The stream's words for them are
/// <see cref="FileSelectTilemapFormat.End"/> and <see cref="FileSelectTilemapFormat.NextRow"/>, read only by
/// <see cref="FileSelectTextControls.Of"/>.
/// </summary>
internal enum FileSelectTextControl
{
    /// <summary>The stream ends.</summary>
    End,

    /// <summary>The following cells start the next row.</summary>
    NextRow,
}
