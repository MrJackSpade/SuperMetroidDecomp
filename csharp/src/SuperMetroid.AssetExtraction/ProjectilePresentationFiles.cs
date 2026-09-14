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
    public const int Version = 1;
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
        File.WriteAllText(Path.Combine(directory, ManifestFileName), JsonSerializer.Serialize(
            new Manifest(Version, SupportedCartridge.Sha256, Convert.ToHexString(SHA256.HashData(bytes))), Options));
        _ = Load(directory, null);
    }

    /// <summary>Validates stock even when overridden. Invalid overrides never fall back to stock.</summary>
    public static InstalledProjectilePresentation Load(string stockDirectory, string? overrideDirectory)
    {
        Manifest manifest;
        try
        {
            using var document = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(stockDirectory, ManifestFileName)));
            if (document.RootElement.ValueKind != JsonValueKind.Object ||
                document.RootElement.EnumerateObject().Select(p => p.Name).Distinct(StringComparer.Ordinal).Count() !=
                document.RootElement.EnumerateObject().Count())
                throw new InvalidDataException("Invalid projectile manifest object or duplicate property.");
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
        var catalog = ProjectileSpriteCatalog.Load(new MemoryStream(stock, writable: false));
        string? replacement = overrideDirectory is null ? null : Path.Combine(overrideDirectory, ProjectileSpriteDefinitions.FileName);
        if (replacement is null || !File.Exists(replacement))
            return new(catalog, stockHash, stockHash);
        byte[] selected = File.ReadAllBytes(replacement);
        return new(ProjectileSpriteCatalog.Load(new MemoryStream(selected, writable: false)),
            stockHash, Convert.ToHexString(SHA256.HashData(selected)));
    }

    private sealed record Manifest(int Version, string RomSha256, string ContentSha256);
}

/// <summary>Loaded content and separate original/selected byte identities for diagnostics.</summary>
public sealed record InstalledProjectilePresentation(ProjectileSpriteCatalog Catalog, string StockSha256, string SelectedSha256);
