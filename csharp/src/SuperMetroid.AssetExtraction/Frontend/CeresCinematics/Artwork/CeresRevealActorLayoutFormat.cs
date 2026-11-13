namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>Installed-resource identity and schema revision for the Ceres-to-Zebes reveal's editable actor positions.</summary>
public static class CeresRevealActorLayoutFormat
{
    /// <summary>Supported schema revision, two, requiring the six actor roles in spawn order at signed screen-pixel coordinates (#1627).</summary>
    public const int Version = 2;
    /// <summary>JSON resource filename for the six reveal actors' initial screen coordinates.</summary>
    public const string FileName = "ceres-zebes-reveal-actors.json";
}
