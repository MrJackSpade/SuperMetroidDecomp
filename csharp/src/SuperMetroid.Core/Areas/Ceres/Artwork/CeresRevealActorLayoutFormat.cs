namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>Installed-resource identity and schema revision for the Ceres-to-Zebes reveal's editable actor positions.</summary>
public static class CeresRevealActorLayoutFormat
{
    /// <summary>Supported schema revision, one, requiring the six native actor roles in spawn order.</summary>
    public const int Version = 1;
    /// <summary>JSON resource filename for the six reveal actors' initial screen-coordinate words.</summary>
    public const string FileName = "ceres-zebes-reveal-actors.json";
}
