namespace SuperMetroid.AssetExtraction.Assets;

/// <summary>The standard sprite artwork shared by gameplay, the intro, and Ceres destruction.</summary>
public static class StandardObjectArtworkFormat
{
    /// <summary>Import-data indexed PNG filename: 256x96 pixels containing 368 row-major 8x8 4-bpp characters, with the final 16 cells zero-filled.</summary>
    public const string FileName = "standard-object-characters.png";
    /// <summary>The installed standard sprite artwork span: gameplay loads all of it, while the intro uses its first part.</summary>
    public const int ByteCount = 11776;
}
