namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>File identity and limits of the installed equipment-selector presentation.</summary>
public static partial class PauseSelectorFormat
{
    /// <summary>Supported revision of the equipment-selector anchor, composition, animation, and palette JSON schema.</summary>
    public const int Version = 1;
    /// <summary>Installed filename of the pause-equipment selector layout the game binds (#1638).</summary>
    public const string FileName = "pause-selectors.json";
    /// <summary>Import-data filename of the extracted selector document, whose frame compositions are packed sprite placements (#1638).</summary>
    public const string ArtworkFileName = "pause-selector-artwork.json";
    /// <summary>The sixteen equipment anchors the selector layout names.</summary>
    public const int AnchorCount = 16;

    /// <summary>The most phases the cartridge decoder reads before the selector's animation stream must end: its terminator search limit.</summary>
    public const int MaximumPhases = 255;
}
