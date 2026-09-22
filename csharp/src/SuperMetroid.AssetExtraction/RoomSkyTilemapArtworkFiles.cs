using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installs seven literal bank-$8A sky tilemap pages with persistent JSON overrides.</summary>
public static class RoomSkyTilemapArtworkFiles
{
    private const int FormatVersion = 1;

    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);
        var hashes = new Dictionary<string, string>();
        for (int page = 0; page < RoomSkyTilemapFormat.PageCount; page++)
        {
            string name = RoomSkyTilemapFormat.FileName(page);
            byte[] native = RomDataReader.ReadFixedBank(bus,
                RoomSkyTilemapFormat.SourceAddress(page), RoomSkyTilemapFormat.PageByteCount);
            byte[] json = RoomBackgroundTilemapExtractor.Encode(native);
            using (var output = new FileStream(Path.Combine(directory, name), FileMode.CreateNew, FileAccess.Write))
                output.Write(json);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(json)));
        }
        using var manifest = new FileStream(Path.Combine(directory,
            RoomSkyTilemapFormat.ManifestFileName), FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(manifest,
            new RoomSkyFileManifest(FormatVersion, sourceCartridgeSha256, hashes), JsonOptions);
    }

    public static RoomSkyTilemapCatalog Load(string stockDirectory, string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, RoomSkyTilemapFormat.ManifestFileName);
        RoomSkyFileManifest manifest;
        try
        {
            manifest = JsonSerializer.Deserialize<RoomSkyFileManifest>(
                File.ReadAllBytes(manifestPath), JsonOptions)
                ?? throw new InvalidDataException("Scrolling-sky manifest is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid scrolling-sky manifest {manifestPath}.", error);
        }
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase) ||
            manifest.Sha256 is null || manifest.Sha256.Count != RoomSkyTilemapFormat.PageCount ||
            Enumerable.Range(0, RoomSkyTilemapFormat.PageCount)
                .Any(page => !manifest.Sha256.ContainsKey(RoomSkyTilemapFormat.FileName(page))))
            throw new InvalidDataException(
                $"Scrolling-sky manifest {manifestPath} does not describe this installation.");

        var pages = new RoomBackgroundTilemapAtlas[RoomSkyTilemapFormat.PageCount];
        for (int page = 0; page < pages.Length; page++)
        {
            string name = RoomSkyTilemapFormat.FileName(page);
            string stockPath = Path.Combine(stockDirectory, name);
            byte[] stock = File.ReadAllBytes(stockPath);
            if (!string.Equals(Convert.ToHexString(SHA256.HashData(stock)), manifest.Sha256[name],
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Stock scrolling-sky page {stockPath} failed its manifest hash.");
            string? overridePath = overrideDirectory is null ? null : Path.Combine(overrideDirectory, name);
            string selectedPath = overridePath is not null && File.Exists(overridePath)
                ? overridePath : stockPath;
            byte[] json = selectedPath == stockPath ? stock : File.ReadAllBytes(selectedPath);
            try
            {
                pages[page] = RoomBackgroundTilemapAtlas.Load(
                    new MemoryStream(json, writable: false), RoomSkyTilemapFormat.PageByteCount);
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid scrolling-sky page {selectedPath}: {error.Message}", error);
            }
        }
        return new RoomSkyTilemapCatalog(pages);
    }

    public static void ValidateStock(string stockDirectory) => _ = Load(stockDirectory, null);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    private sealed record RoomSkyFileManifest(int Version, string SourceCartridgeSha256,
        Dictionary<string, string> Sha256);
}
