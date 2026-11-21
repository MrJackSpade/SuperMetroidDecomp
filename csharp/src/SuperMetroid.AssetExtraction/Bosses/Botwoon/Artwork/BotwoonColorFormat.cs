namespace SuperMetroid.AssetExtraction.Bosses.Botwoon.Artwork;

/// <summary>Names and versions the editable Botwoon health-color asset.</summary>
public static class BotwoonColorFormat
{
    /// <summary>The embedded Botwoon color asset file name.</summary>
    public const string FileName = "botwoon-colors.json";

    /// <summary>The supported Botwoon color JSON schema version.</summary>
    public const int Version = 2;

    /// <summary>The health bands the file holds a row of colours for, in the order Botwoon shows them (#2266).</summary>
    public const int HealthBandCount = 8;

    /// <summary>Each Botwoon health-band image occupies one sixteen-color sprite palette.</summary>
    public const int ColorsPerPalette = 16;
}
