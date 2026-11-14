namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>Installed filename and supported schema revision for the selected Ceres-destruction and Zebes-reveal OAM compositions.</summary>
public static class CeresDestructionSpriteFormat
{
    /// <summary>
    /// Supported composition schema revision, two, requiring the complete 24-name frame set with the Japanese PLANET ZEBES
    /// subtitle (#1733).
    /// </summary>
    public const int Version = 2;
    /// <summary>Installed editable JSON filename for unique destruction/reveal backdrops and blast frames.</summary>
    public const string FileName = "ceres-destruction-sprites.json";
}
