using SuperMetroid.Core.Progression;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>On-disk names and manifest bounds for seven area maps and their shared presentation resources.</summary>
public static class AreaMapCatalogFormat
{
    /// <summary>Supported extracted map-catalog schema version; other manifest versions are rejected.</summary>
    public const int Version = 79;
    /// <summary>Manifest-bound non-area artwork files, including Ceres Mode-7 colors.</summary>
    public const int SharedResourceCount = 62;
    /// <summary>Bundled authored reveal mask: logical row-major cell indexes, not SRAM offsets or editable engine code.</summary>
    public const string StationRevealFile = "station-reveal.json";
    /// <summary>Stock manifest filename containing extraction provenance and the SHA-256 digest of every catalog resource.</summary>
    public const string ManifestFile = "manifest.json";
    /// <summary>Builds the extracted JSON filename from a validated retail area identity using its lowercase enum name.</summary>
    /// <param name="area">One of the seven retail area identities.</param>
    /// <returns>The area's lowercase name followed by <c>.json</c>, without a directory.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value is not a retail area identity.</exception>
    public static string FileName(AreaId area)
    {
        _ = AreaIds.ToIndex(area);
        return area.ToString().ToLowerInvariant() + ".json";
    }
}
