namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>File identity of the installed reserve-strip presentation.</summary>
public static class PauseReserveTankFormat
{
    /// <summary>Supported schema version for the editable reserve-strip anchors, palette, and visual frames.</summary>
    public const int Version = 1;
    /// <summary>JSON asset filename for the pause equipment screen's reserve-tank strip presentation.</summary>
    public const string FileName = "pause-reserve-tanks.json";
    /// <summary>The sprite palette every stock reserve-gauge part is drawn with.</summary>
    public const int Palette = 3;
    /// <summary>The two identical eight-entry partial-fill tables together hold sixteen entries.</summary>
    internal const int PartialMapEntries = 16;
}
