namespace SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork;

/// <summary>Filename and schema version of the installed Ridley/Baby RGB5 palette asset.</summary>
public static class CeresRidleyColorFormat
{
    /// <summary>Resource-root JSON filename consumed by the presentation catalog loader.</summary>
    public const string FileName = "ceres-ridley-colors.json";

    /// <summary>Schema version five, requiring the complete startup, fade, health, retreat, alarm and Baby palette families, the Baby's colours named by shade, with no step-ordered Baby field.</summary>
    public const int Version = 5;
}
