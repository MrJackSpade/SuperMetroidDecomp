using SuperMetroid.Core;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Installs seven literal bank-$8A sky tilemap pages as JSON a player edits in place.</summary>
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

    /// <summary>Checks installed provenance and seven-page manifest coverage, then compiles each installed page as the player left it, without cartridge access.</summary>
    /// <param name="directory">Installed directory containing the seven sky-page JSON files and their provenance manifest.</param>
    /// <returns>The installed contiguous tilemap pages, with native page/row streaming and scrolling pointer arithmetic unchanged.</returns>
    /// <exception cref="InvalidDataException">Manifest provenance or coverage, or an installed page's BG tile-word format and transfer size is invalid.</exception>
    public static RoomSkyTilemaps Load(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        string manifestPath = Path.Combine(directory, RoomSkyTilemapFormat.ManifestFileName);
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
            string path = Path.Combine(directory, name);
            byte[] json = File.ReadAllBytes(path);
            try
            {
                pages[page] = RoomBackgroundTilemapAtlas.Load(
                    new MemoryStream(json, writable: false), RoomSkyTilemapFormat.PageByteCount);
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid scrolling-sky page {path}: {error.Message}", error);
            }
        }
        return new RoomSkyTilemaps(pages);
    }

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
