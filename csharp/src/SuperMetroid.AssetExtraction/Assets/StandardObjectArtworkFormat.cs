namespace SuperMetroid.AssetExtraction.Assets;

/// <summary>The standard sprite artwork shared by gameplay, the intro, and Ceres destruction.</summary>
public static class StandardObjectArtworkFormat
{
    /// <summary>Supported stock manifest schema version; loading also verifies the pinned source identity.</summary>
    public const int Version = 1;
    /// <summary>Installed indexed PNG filename: 256x96 pixels containing 368 row-major 8x8 4-bpp characters, with the final 16 cells zero-filled.</summary>
    public const string FileName = "standard-object-characters.png";
    /// <summary>Stock JSON manifest filename recording the schema version and source SHA-256.</summary>
    public const string ManifestFileName = "standard-object-artwork.json";
    /// <summary>The installed standard sprite artwork span: gameplay loads all of it, while the intro uses its first part.</summary>
    public const int ByteCount = 11776;
}
