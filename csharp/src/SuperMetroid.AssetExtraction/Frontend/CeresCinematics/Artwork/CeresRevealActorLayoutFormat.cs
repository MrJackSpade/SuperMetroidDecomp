namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>Installed-resource identity and schema revision for the Ceres-to-Zebes reveal's editable actor positions.</summary>
public static class CeresRevealActorLayoutFormat
{
    /// <summary>
    /// Supported schema revision, four, requiring one placement for each reveal role, the Japanese subtitle's among them,
    /// each naming its role (#1768), at signed screen-pixel coordinates (#1627, #1733).
    /// </summary>
    public const int Version = 4;
    /// <summary>JSON resource filename for the reveal actors' and the Japanese subtitle's initial screen coordinates.</summary>
    public const string FileName = "ceres-zebes-reveal-actors.json";
}
