using SuperMetroid.Core.Map.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Stock content provenance, separate from a catalog's selected replacement identity.</summary>
public sealed record AreaMapCatalogManifest
{
    /// <summary>Catalog schema version that must equal <see cref="AreaMapCatalogFormat.Version"/> when stock content is loaded.</summary>
    public required int Version { get; init; }
    /// <summary>SHA-256 digest of the cartridge used to extract the stock content, independent of selected user overrides.</summary>
    public required string SourceCartridgeSha256 { get; init; }
    /// <summary>Extracted filename-to-SHA-256 mapping; every listed stock file must be read and verified during catalog loading.</summary>
    public required Dictionary<string, string> Sha256 { get; init; }
}
