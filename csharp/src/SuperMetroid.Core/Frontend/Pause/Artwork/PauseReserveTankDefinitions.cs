namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>Reserve-strip visual identities and import-only cartridge bindings.</summary>
public static class PauseReserveTankDefinitions
{
    /// <summary>Supported schema version for the editable reserve-strip anchors, palette, and visual frames.</summary>
    public const int Version = 1;
    /// <summary>JSON asset filename for the pause equipment screen's reserve-tank strip presentation.</summary>
    public const string FileName = "pause-reserve-tanks.json";
    /// <summary>$82:C1D6 supplies six origins, including the trailing cap position.</summary>
    public const int AnchorCount = 6;
    /// <summary>The sprite palette every reserve-gauge part is drawn with.</summary>
    public const int Palette = 3;
}
