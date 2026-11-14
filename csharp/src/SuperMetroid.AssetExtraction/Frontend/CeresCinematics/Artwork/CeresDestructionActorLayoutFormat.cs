namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>Installed resource identity and supported JSON schema version for Ceres's three persistent destruction actors.</summary>
public static class CeresDestructionActorLayoutFormat
{
    /// <summary>Supported destruction-actor placement schema revision three: each placement names its actor's role (#1768), at signed screen-pixel coordinates (#1627).</summary>
    public const int Version = 3;
    /// <summary>Installed persistent-actor placement resource, <c>ceres-destruction-actors.json</c>.</summary>
    public const string FileName = "ceres-destruction-actors.json";
}
