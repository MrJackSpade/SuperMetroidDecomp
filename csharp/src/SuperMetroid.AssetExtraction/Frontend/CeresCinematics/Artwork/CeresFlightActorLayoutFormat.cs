namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>Installed resource identity and supported schema version for rear-view Ceres actor placement.</summary>
public static class CeresFlightActorLayoutFormat
{
    /// <summary>Supported actor-layout JSON schema revision three: each placement names its actor's role (#1768), at signed screen-pixel coordinates (#1627).</summary>
    public const int Version = 3;
    /// <summary>Installed rear-view placement resource, <c>ceres-flight-actors.json</c>.</summary>
    public const string FileName = "ceres-flight-actors.json";
}
