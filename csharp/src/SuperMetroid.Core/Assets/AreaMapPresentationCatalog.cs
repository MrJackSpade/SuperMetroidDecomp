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
    public static AreaMapPresentationCatalog Load(string stockDirectory, string? overrideDirectory)
    {
        var stock = ReadVerifiedStock(stockDirectory);
        var areas = new IAreaMapView[AreaIds.RetailCount];
        using var identity = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            using var baselineStream = new MemoryStream(stock.Maps[area], writable: false);
            var baseline = AreaMapPresentationAsset.Load(baselineStream, new AreaMapDecodeRules(area));
            IAreaMapView definition = new AreaMapStockRules(baseline, stock.StationCells[area]);
            // The identity covers baseline rule content too: equal override bytes do
            // not imply equal exploration semantics across different stock installs.
            AppendFramed(stock.Maps[area]);
            byte[] stationPlane = new byte[AreaMapLayout.WidthInTiles * AreaMapLayout.HeightInTiles];
            foreach (int cell in stock.StationCells[area]) stationPlane[cell] = 1;
            AppendFramed(stationPlane);
            string? replacement = overrideDirectory is null ? null : Path.Combine(overrideDirectory, AreaMapCatalogFormat.FileName(area));
            bool hasReplacement = replacement is not null && File.Exists(replacement);
            byte[] selected = hasReplacement ? File.ReadAllBytes(replacement!) : stock.Maps[area];
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
            AppendFramed(selected);
        }
        return new(areas, Convert.ToHexString(identity.GetHashAndReset()));

        void AppendFramed(byte[] bytes)
        {
            Span<byte> length = stackalloc byte[sizeof(int)];
            BinaryPrimitives.WriteInt32LittleEndian(length, bytes.Length);
            identity.AppendData(length);
            identity.AppendData(bytes);
        }
    }

    /// <summary>Installer integrity check; never repairs files or touches the override directory.</summary>
    public static void ValidateStock(string directory) => _ = ReadVerifiedStock(directory);

    private static (Dictionary<AreaId, byte[]> Maps, Dictionary<AreaId, HashSet<int>> StationCells) ReadVerifiedStock(string directory)
    {
        AreaMapCatalogManifest manifest;
        try
        {
            manifest = JsonSerializer.Deserialize<AreaMapCatalogManifest>(
                File.ReadAllBytes(Path.Combine(directory, AreaMapCatalogFormat.ManifestFile)), MapPresentationFormat.JsonOptions)
                ?? throw new InvalidDataException("Map catalog manifest is null.");
        }
        catch (JsonException error) { throw new InvalidDataException($"Invalid map catalog manifest in {directory}.", error); }
        if (manifest.Version != AreaMapCatalogFormat.Version || manifest.Sha256 is null || manifest.Sha256.Count != AreaIds.RetailCount + 1)
            throw new InvalidDataException("Map catalog manifest must contain the supported version, seven maps and station-reveal hashes.");
        var result = new Dictionary<AreaId, byte[]>();
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            string file = AreaMapCatalogFormat.FileName(area);
            result.Add(area, ReadChecked(file));
        }
        Dictionary<string, int[]> masks;
        try
        {
            masks = JsonSerializer.Deserialize<Dictionary<string, int[]>>(ReadChecked(AreaMapCatalogFormat.StationRevealFile), MapPresentationFormat.JsonOptions)
                ?? throw new InvalidDataException("Station reveal content is null.");
        }
        catch (JsonException error) { throw new InvalidDataException("Invalid station reveal content.", error); }
        if (masks.Count != AreaIds.RetailCount) throw new InvalidDataException("Station reveal content requires all seven areas.");
        var stationCells = new Dictionary<AreaId, HashSet<int>>();
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            if (!masks.TryGetValue(area.ToString(), out int[]? cells) || cells is null ||
                cells.Any(cell => (uint)cell >= AreaMapLayout.WidthInTiles * AreaMapLayout.HeightInTiles) ||
                cells.Distinct().Count() != cells.Length)
                throw new InvalidDataException($"Invalid or duplicate station reveal cells for {area}.");
            stationCells.Add(area, cells.ToHashSet());
        }
        return (result, stationCells);

        byte[] ReadChecked(string file)
        {
            if (!manifest.Sha256.TryGetValue(file, out string? expected))
                throw new InvalidDataException($"Map catalog manifest is missing {file}.");
            byte[] bytes = File.ReadAllBytes(Path.Combine(directory, file));
            if (!string.Equals(expected, Convert.ToHexString(SHA256.HashData(bytes)), StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Stock map {file} failed its SHA-256 check. Put edits in the overrides directory, not stock content.");
            return bytes;
        }
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
    public const int Version = 2;
    /// <summary>Bundled authored reveal mask: logical row-major cell indexes, not SRAM offsets or editable engine code.</summary>
    public const string StationRevealFile = "station-reveal.json";
    public const string ManifestFile = "manifest.json";
    public static string FileName(AreaId area)
    {
        _ = AreaIds.ToIndex(area);
        return area.ToString().ToLowerInvariant() + ".json";
    }
}
