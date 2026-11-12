using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// The picture store's provenance manifest (#1635), <c>pictures-provenance.json</c>: its format revision, the cartridge the
/// store was extracted from, and the SHA-256 of each page and of each map, by file name, the pages apart from each kind of
/// map. A page lists a map exactly when the store wrote one for it. It lists every stored picture under its tag (#1639).
/// </summary>
/// <param name="Version">The format revision, <see cref="FormatVersion"/>.</param>
/// <param name="SourceCartridgeSha256">The cartridge the store was extracted from.</param>
/// <param name="PageSha256">Each page's hash, by file name, in page order.</param>
/// <param name="RecolourRegionSha256">Each page's recolour-region map's hash, by file name, for the pages that have one.</param>
/// <param name="BlendMapSha256">Each page's blend map's hash, by file name, for the pages that have one.</param>
/// <param name="Pictures">Every stored picture's page and rectangle, by its tag, in write order.</param>
internal sealed record PictureStoreProvenance(int Version, string SourceCartridgeSha256, Dictionary<string, string> PageSha256,
    Dictionary<string, string> RecolourRegionSha256, Dictionary<string, string> BlendMapSha256,
    Dictionary<string, StoredPictureDocument> Pictures)
{
    /// <summary>
    /// The format revision: 1 holds shelf-packed RGBA pages with their recolour-region and blend maps (#1635); 2 lists every
    /// stored picture under its tag (#1639).
    /// </summary>
    internal const int FormatVersion = 2;

    /// <summary>Camel-case fields, indented; unknown fields are rejected.</summary>
    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
}
