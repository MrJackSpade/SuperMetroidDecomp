using System.Security.Cryptography;
using System.Buffers.Binary;
using System.Text.Json;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Immutable map content snapshot. Overrides never alter stock provenance or exploration rules.</summary>
public sealed class AreaMapPresentationCatalog
{
    private readonly IAreaMapView[] areas;
    private AreaMapPresentationCatalog(IAreaMapView[] areas, string contentIdentity)
    {
        this.areas = areas;
        ContentIdentity = contentIdentity;
    }

    public string ContentIdentity { get; }
    public IAreaMapView Get(AreaId area) => areas[AreaIds.ToIndex(area)];

    /// <summary>Reads all areas atomically into a new catalog; an invalid override is never replaced with stock.</summary>
    public static AreaMapPresentationCatalog Load(string stockDirectory, string? overrideDirectory,
        Func<AreaId, IAreaMapView> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);
        var stock = ReadVerifiedStock(stockDirectory);
        var areas = new IAreaMapView[AreaIds.RetailCount];
        using var identity = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            IAreaMapView definition = rules(area);
            if (definition.Area != area) throw new InvalidDataException($"Map rules for {area} belong to {definition.Area}.");
            string? replacement = overrideDirectory is null ? null : Path.Combine(overrideDirectory, AreaMapCatalogFormat.FileName(area));
            bool hasReplacement = replacement is not null && File.Exists(replacement);
            byte[] selected = hasReplacement ? File.ReadAllBytes(replacement!) : stock[area];
            try
            {
                using var stream = new MemoryStream(selected, writable: false);
                areas[AreaIds.ToIndex(area)] = AreaMapPresentationAsset.Load(stream, definition);
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid {area} map presentation ({(hasReplacement ? replacement : stockDirectory)}): {error.Message}", error);
            }
            // Hash framed contents in fixed area order: identity changes after a valid
            // replacement is reloaded, without rewriting original extraction provenance.
            byte[] length = new byte[sizeof(int)];
            BinaryPrimitives.WriteInt32LittleEndian(length, selected.Length);
            identity.AppendData(length);
            identity.AppendData(selected);
        }
        return new(areas, Convert.ToHexString(identity.GetHashAndReset()));
    }

    /// <summary>Installer integrity check; never repairs files or touches the override directory.</summary>
    public static void ValidateStock(string directory) => _ = ReadVerifiedStock(directory);

    private static Dictionary<AreaId, byte[]> ReadVerifiedStock(string directory)
    {
        AreaMapCatalogManifest manifest;
        try
        {
            manifest = JsonSerializer.Deserialize<AreaMapCatalogManifest>(
                File.ReadAllBytes(Path.Combine(directory, AreaMapCatalogFormat.ManifestFile)), MapPresentationFormat.JsonOptions)
                ?? throw new InvalidDataException("Map catalog manifest is null.");
        }
        catch (JsonException error) { throw new InvalidDataException($"Invalid map catalog manifest in {directory}.", error); }
        if (manifest.Version != AreaMapCatalogFormat.Version || manifest.Sha256 is null || manifest.Sha256.Count != AreaIds.RetailCount)
            throw new InvalidDataException("Map catalog manifest must contain the supported version and all seven area hashes.");
        var result = new Dictionary<AreaId, byte[]>();
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            string file = AreaMapCatalogFormat.FileName(area);
            if (!manifest.Sha256.TryGetValue(file, out string? expected))
                throw new InvalidDataException($"Map catalog manifest is missing {file}.");
            byte[] bytes = File.ReadAllBytes(Path.Combine(directory, file));
            string actual = Convert.ToHexString(SHA256.HashData(bytes));
            if (!string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Stock map {file} failed its SHA-256 check. Put edits in the overrides directory, not stock content.");
            result.Add(area, bytes);
        }
        return result;
    }
}

/// <summary>Stock content provenance, separate from a catalog's selected replacement identity.</summary>
public sealed record AreaMapCatalogManifest
{
    public required int Version { get; init; }
    public required string SourceCartridgeSha256 { get; init; }
    public required Dictionary<string, string> Sha256 { get; init; }
}

public static class AreaMapCatalogFormat
{
    public const int Version = 1;
    public const string ManifestFile = "manifest.json";
    public static string FileName(AreaId area)
    {
        _ = AreaIds.ToIndex(area);
        return area.ToString().ToLowerInvariant() + ".json";
    }
}
