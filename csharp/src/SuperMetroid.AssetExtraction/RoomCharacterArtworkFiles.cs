using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Installs stock room-character PNGs separately from persistent user overrides.
/// The manifest records native transfer lengths; it does not expose room mechanics.
/// </summary>
public static class RoomCharacterArtworkFiles
{
    public const string ManifestFileName = "room-characters.json";
    private const int FormatVersion = 1;

    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);

        IReadOnlyDictionary<string, byte[]> pngs = RoomCharacterAtlasExtractor.Extract(bus);
        Dictionary<string, int> sources = ExpectedSources();
        var entries = new Dictionary<string, RoomCharacterFileEntry>();
        foreach ((string name, int sourceAddress) in sources)
        {
            byte[] png = pngs[name];
            int nativeByteCount = RomDataReader.Decompress(bus, sourceAddress).Length;
            RoomCharacterAtlasFormat.ValidateTileCount(nativeByteCount);
            using (var output = new FileStream(Path.Combine(directory, name), FileMode.CreateNew, FileAccess.Write))
                output.Write(png);
            entries.Add(name, new RoomCharacterFileEntry(
                nativeByteCount, Convert.ToHexString(SHA256.HashData(png))));
        }

        var manifest = new RoomCharacterFileManifest(
            FormatVersion, sourceCartridgeSha256, entries);
        using var stream = new FileStream(Path.Combine(directory, ManifestFileName),
            FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(stream, manifest, JsonOptions);
    }

    /// <summary>Checks every stock file's hash and decodes every selected PNG without any ROM access.</summary>
    public static RoomCharacterAtlasCatalog Load(string stockDirectory, string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, ManifestFileName);
        RoomCharacterFileManifest manifest;
        try
        {
            manifest = JsonSerializer.Deserialize<RoomCharacterFileManifest>(
                File.ReadAllBytes(manifestPath), JsonOptions)
                ?? throw new InvalidDataException("Room character manifest is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid room character manifest {manifestPath}.", error);
        }

        Dictionary<string, int> sources = ExpectedSources();
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase) ||
            manifest.Entries is null || manifest.Entries.Count != sources.Count ||
            sources.Keys.Any(name => !manifest.Entries.ContainsKey(name)))
            throw new InvalidDataException($"Room character manifest {manifestPath} does not describe this installation.");

        RoomCharacterAtlas? cre = null;
        var sheets = new Dictionary<int, RoomCharacterAtlas>();
        foreach ((string name, int address) in sources)
        {
            RoomCharacterFileEntry entry = manifest.Entries[name];
            RoomCharacterAtlasFormat.ValidateTileCount(entry.NativeByteCount);
            string stockPath = Path.Combine(stockDirectory, name);
            byte[] stock = File.ReadAllBytes(stockPath);
            string stockHash = Convert.ToHexString(SHA256.HashData(stock));
            if (!string.Equals(stockHash, entry.Sha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Stock room character PNG {stockPath} failed its manifest hash.");

            string? overridePath = overrideDirectory is null ? null : Path.Combine(overrideDirectory, name);
            string selectedPath = overridePath is not null && File.Exists(overridePath)
                ? overridePath : stockPath;
            byte[] selected = selectedPath == stockPath ? stock : File.ReadAllBytes(selectedPath);
            RoomCharacterAtlas atlas;
            try
            {
                atlas = RoomCharacterAtlas.Load(new MemoryStream(selected, writable: false),
                    entry.NativeByteCount);
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid room character PNG {selectedPath}: {error.Message}", error);
            }
            if (name == RoomCharacterAtlasFormat.CreFileName) cre = atlas;
            else sheets.Add(address, atlas);
        }
        return new RoomCharacterAtlasCatalog(
            cre ?? throw new InvalidDataException("Room character manifest has no CRE sheet."), sheets);
    }

    /// <summary>Used by installer completeness checks; overrides do not affect stock provenance.</summary>
    public static void ValidateStock(string stockDirectory) => _ = Load(stockDirectory, null);

    private static Dictionary<string, int> ExpectedSources()
    {
        var sources = new Dictionary<string, int>
        {
            [RoomCharacterAtlasFormat.CreFileName] = RoomAssetRomData.Tilesets.CreCharactersAddress,
        };
        for (byte graphicsSet = 0; graphicsSet < RoomTilesetDefinitions.Count; graphicsSet++)
        {
            int address = RoomTilesetDefinitions.Get(graphicsSet).CharacterAddress;
            sources[RoomCharacterAtlasFormat.SourceFileName(address)] = address;
        }
        return sources;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    private sealed record RoomCharacterFileManifest(
        int Version, string SourceCartridgeSha256,
        Dictionary<string, RoomCharacterFileEntry> Entries);

    private sealed record RoomCharacterFileEntry(int NativeByteCount, string Sha256);
}
