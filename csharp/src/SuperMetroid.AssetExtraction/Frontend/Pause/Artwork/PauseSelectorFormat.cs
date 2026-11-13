namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>File identity and limits of the installed equipment-selector presentation.</summary>
public static class PauseSelectorFormat
{
    /// <summary>Supported revision of the equipment-selector anchor, composition, animation, and palette JSON schema.</summary>
    public const int Version = 1;
    /// <summary>Installed filename of the pause-equipment selector layout the game binds (#1638).</summary>
    public const string FileName = "pause-selectors.json";
    /// <summary>Import-data filename of the extracted selector document, whose frame compositions are packed sprite placements (#1638).</summary>
    public const string ArtworkFileName = "pause-selector-artwork.json";
    /// <summary>Presentation limits: sixteen required equipment anchors, at most 255 named visual frames and 255 cyclic phases, and initial/phase dwell times from 1 through 254 accepted update ticks.</summary>
    public const int AnchorCount = 16, MaximumFrames = 255, MaximumPhases = 255, MaximumDuration = 254;
}
