namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>Reads a file-select text stream's control words.</summary>
internal static partial class FileSelectTextControls
{
    /// <summary>The control each control word is.</summary>
    private static readonly Dictionary<int, FileSelectTextControl> Controls = new()
    {
        [FileSelectTilemapFormat.End] = FileSelectTextControl.End,
        [FileSelectTilemapFormat.NextRow] = FileSelectTextControl.NextRow,
    };

    /// <summary>The control <paramref name="word"/> is, or null for a tilemap cell.</summary>
    internal static FileSelectTextControl? Of(int word) => Controls.TryGetValue(word, out FileSelectTextControl control) ? control : null;
}
