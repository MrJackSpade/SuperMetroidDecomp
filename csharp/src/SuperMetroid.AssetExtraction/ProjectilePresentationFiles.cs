using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Versioned stock composition provenance, separate from persistent user overrides.</summary>
public static class ProjectilePresentationFiles
{
    public const string ManifestFileName = "projectile-manifest.json";
    public const int Version = 2;
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>Writes into the installer's unpublished staging directory after ROM validation.</summary>
    public static void Extract(ISnesAddressSpace validatedBus, string directory)
    {
        byte[] bytes = ProjectileSpriteExtractor.Extract(validatedBus);
        Directory.CreateDirectory(directory);
        File.WriteAllBytes(Path.Combine(directory, ProjectileSpriteDefinitions.FileName), bytes);
        var beams = BeamTileExtractor.Extract(validatedBus);
        foreach (var file in beams) File.WriteAllBytes(Path.Combine(directory, file.Key), file.Value);
        File.WriteAllText(Path.Combine(directory, ManifestFileName), JsonSerializer.Serialize(
            new Manifest(Version, SupportedCartridge.Sha256, Hash(bytes),
                beams.ToDictionary(pair => pair.Key, pair => Hash(pair.Value))), Options));
        _ = Load(directory, null);
    }

    /// <summary>Validates stock even when overridden. Invalid overrides never fall back to stock.</summary>
    public static InstalledProjectilePresentation Load(string stockDirectory, string? overrideDirectory)
    {
        Manifest manifest;
        try
        {
            using var document = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(stockDirectory, ManifestFileName)));
            ValidateObject(document.RootElement);
            manifest = document.RootElement.Deserialize<Manifest>(Options)
                ?? throw new InvalidDataException("Missing projectile manifest.");
        }
        catch (JsonException error) { throw new InvalidDataException("Invalid projectile manifest JSON.", error); }
        if (manifest.Version != Version || manifest.RomSha256 != SupportedCartridge.Sha256)
            throw new InvalidDataException("Projectile manifest revision does not match the supported cartridge.");
        byte[] stock = File.ReadAllBytes(Path.Combine(stockDirectory, ProjectileSpriteDefinitions.FileName));
        string stockHash = Convert.ToHexString(SHA256.HashData(stock));
        if (!string.Equals(stockHash, manifest.ContentSha256, StringComparison.Ordinal))
            throw new InvalidDataException("Projectile stock composition hash mismatch.");
        _ = ProjectileSpriteCatalog.Load(new MemoryStream(stock, writable: false));
        if (manifest.BeamHashes is null || manifest.BeamHashes.Count != BeamTileAtlasDefinitions.SelectionCount)
            throw new InvalidDataException("Projectile manifest must identify every beam PNG.");
        var stockBeams = new Dictionary<string, byte[]>();
        for (int i = 0; i < BeamTileAtlasDefinitions.SelectionCount; i++)
        {
            string name = BeamTileAtlasDefinitions.FileName(i);
            byte[] bytes = File.ReadAllBytes(Path.Combine(stockDirectory, name));
            if (!manifest.BeamHashes.TryGetValue(name, out string? expected) || Hash(bytes) != expected)
                throw new InvalidDataException($"Beam PNG stock hash mismatch: {name}.");
            stockBeams.Add(name, bytes);
        }
        _ = BeamTileCatalog.Load(stockBeams);
        // Finish stock validation before opening any optional replacement.
        byte[] Select(string name, byte[] baseline)
        {
            string? path = overrideDirectory is null ? null : Path.Combine(overrideDirectory, name);
            return path is not null && File.Exists(path) ? File.ReadAllBytes(path) : baseline;
        }
        byte[] selected = Select(ProjectileSpriteDefinitions.FileName, stock);
        var selectedBeams = stockBeams.ToDictionary(pair => pair.Key, pair => Select(pair.Key, pair.Value));
        return new(ProjectileSpriteCatalog.Load(new MemoryStream(selected, writable: false)),
            Identity(stock, stockBeams), Identity(selected, selectedBeams), BeamTileCatalog.Load(selectedBeams));
    }

    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));
    private static string Identity(byte[] composition, Dictionary<string, byte[]> beams)
    {
        // Fixed-size component hashes in fixed selection order prevent ambiguous concatenation.
        string hashes = Hash(composition);
        for (int i = 0; i < BeamTileAtlasDefinitions.SelectionCount; i++)
            hashes += Hash(beams[BeamTileAtlasDefinitions.FileName(i)]);
        return Hash(System.Text.Encoding.ASCII.GetBytes(hashes));
    }
    private static void ValidateObject(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
            throw new InvalidDataException("Projectile manifest requires an object.");
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in element.EnumerateObject())
        {
            if (!names.Add(property.Name)) throw new InvalidDataException("Duplicate projectile manifest property.");
            if (property.Value.ValueKind == JsonValueKind.Object) ValidateObject(property.Value);
        }
    }
    private sealed record Manifest(int Version, string RomSha256, string ContentSha256, Dictionary<string, string> BeamHashes);
}

/// <summary>Loaded content and separate original/selected byte identities for diagnostics.</summary>
public sealed record InstalledProjectilePresentation(ProjectileSpriteCatalog Catalog, string StockSha256, string SelectedSha256, BeamTileCatalog BeamTiles);
