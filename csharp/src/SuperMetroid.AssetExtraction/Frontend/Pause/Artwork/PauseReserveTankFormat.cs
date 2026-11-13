namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>File identity of the installed reserve-strip presentation.</summary>
public static class PauseReserveTankFormat
{
    /// <summary>Supported schema version for the editable reserve-strip anchors, palette, and visual frames.</summary>
    public const int Version = 1;
    /// <summary>Installed filename of the reserve-tank strip layout the game binds (#1638).</summary>
    public const string FileName = "pause-reserve-tanks.json";
    /// <summary>Import-data filename of the extracted strip document, whose frame compositions are packed sprite placements (#1638).</summary>
    public const string ArtworkFileName = "pause-reserve-tank-artwork.json";
    /// <summary>The sprite palette every stock reserve-gauge part is drawn with.</summary>
    public const int Palette = 3;
    /// <summary>The two identical eight-entry partial-fill tables together hold sixteen entries.</summary>
    internal const int PartialMapEntries = 16;
}
