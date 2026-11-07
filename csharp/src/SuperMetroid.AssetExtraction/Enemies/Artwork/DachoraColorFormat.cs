namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>Installed resource identity and supported schema version for Dachora's default and animated RGB5 palettes.</summary>
public static class DachoraColorFormat
{
    /// <summary>Installed palette resource, <c>dachora-colors.json</c>.</summary>
    public const string FileName = "dachora-colors.json";
    /// <summary>Supported Dachora color JSON schema revision one.</summary>
    public const int Version = 1;

    /// <summary>The colours in each Dachora colour set, including the transparent first colour.</summary>
    public const int ColorsPerFrame = 16;
}
