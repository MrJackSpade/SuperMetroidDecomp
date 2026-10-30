using SuperMetroid.Core;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Installs seven literal bank-$8A sky tilemap pages with persistent JSON overrides.</summary>
public static class RoomSkyTilemapArtworkFiles
{
    /// <summary>Schema version recorded with the extracted sky-page manifest; 2 names each page by its place rather than its cartridge address (#1414).</summary>
    private const int FormatVersion = 2;

    /// <summary>Exports seven contiguous bank-$8A scrolling-sky pages to editable BG tile-word JSON and creates their hashed provenance manifest.</summary>
    /// <param name="bus">Cartridge import source for seven 32x32 tilemap pages beginning at $8A:B180.</param>
    /// <param name="directory">Destination directory, created if absent; page JSON files and manifest must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Source identity recorded in the manifest; installation loading requires the supported cartridge identity.</param>
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);
        var hashes = new Dictionary<string, string>();
        for (int page = 0; page < RoomSkyTilemapFormat.PageCount; page++)
        {
            string name = RoomSkyTilemapFormat.FileName(page);
            byte[] native = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
                LibraryBackgroundOperands.SkyPageAddress(page), RoomSkyTilemapFormat.PageByteCount);
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

    /// <summary>Checks stock provenance, seven-page manifest coverage, and page digests before compiling per-page overrides without cartridge access.</summary>
    /// <param name="stockDirectory">Stock directory containing the seven sky-page JSON files and their provenance/digest manifest.</param>
    /// <param name="overrideDirectory">Optional directory with independently selected page replacements; each absent file retains verified stock.</param>
    /// <returns>The selected contiguous tilemap pages, with native page/row streaming and scrolling pointer arithmetic unchanged.</returns>
    /// <exception cref="InvalidDataException">Manifest provenance or coverage, a stock digest, or a selected page's BG tile-word format and transfer size is invalid.</exception>
    public static RoomSkyTilemaps Load(string stockDirectory, string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, RoomSkyTilemapFormat.ManifestFileName);
        using var manifestStream = File.OpenRead(manifestPath);
        RoomSkyFileManifest manifest = JsonAssetDocument.Read<RoomSkyFileManifest>(
            manifestStream, JsonOptions, $"scrolling-sky manifest {manifestPath}");
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
        return new RoomSkyTilemaps(pages);
    }

    /// <summary>Each sky page's file name before #1414, which carried its cartridge source, and its current name.</summary>
    internal static IReadOnlyDictionary<string, string> LegacyNames() => Enumerable.Range(0, RoomSkyTilemapFormat.PageCount).ToDictionary(
        page => $"scrolling-sky-{LibraryBackgroundOperands.SkyPageAddress(page):X6}.json", RoomSkyTilemapFormat.FileName);

    /// <summary>Checks stock provenance, all seven page digests, and compilation of every fixed-size scrolling-sky page; ignores overrides.</summary>
    /// <param name="stockDirectory">Installed stock scrolling-sky directory.</param>
    public static void ValidateStock(string stockDirectory) => _ = Load(stockDirectory, null);

    /// <summary>Strict, camel-case JSON settings shared by sky-page manifest export and import.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectRequiredConstructorParameters = true,
        WriteIndented = true,
    };

    /// <summary>Source cartridge identity and hashes for all seven stock sky-page files.</summary>
    private sealed record RoomSkyFileManifest(int Version, string SourceCartridgeSha256,
        Dictionary<string, string> Sha256);
}
