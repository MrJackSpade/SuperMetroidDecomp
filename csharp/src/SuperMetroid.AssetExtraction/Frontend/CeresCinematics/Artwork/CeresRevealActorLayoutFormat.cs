namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>Installed-resource identity and schema revision for the Ceres-to-Zebes reveal's editable actor positions.</summary>
public static class CeresRevealActorLayoutFormat
{
    /// <summary>
    /// Supported schema revision, three, requiring the six actor roles in spawn order, then the Japanese subtitle, at signed
    /// screen-pixel coordinates (#1627, #1733).
    /// </summary>
    public const int Version = 3;
    /// <summary>JSON resource filename for the reveal actors' and the Japanese subtitle's initial screen coordinates.</summary>
    public const string FileName = "ceres-zebes-reveal-actors.json";
}
