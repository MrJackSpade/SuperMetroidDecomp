using SuperMetroid.Core;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Extracts the room-character pages: the import pipeline's input to the room block sheets, which a player edits instead.
/// The pages are import data (#1636), written to the component's import-data directory and never installed, and so is their
/// manifest (#1693), which records native transfer lengths and does not expose room mechanics.
/// </summary>
public static class RoomCharacterArtworkFiles
{
    /// <summary>Stock character manifest filename recording cartridge provenance, page hashes, and required native planar transfer lengths.</summary>
    public const string ManifestFileName = "room-characters.json";
    /// <summary>Schema revision required by the room character manifest; 4 installs no character page (#1636), and 5 no manifest (#1693).</summary>
    private const int FormatVersion = 5;

    /// <summary>Creates stock indexed PNGs for CRE, distinct room graphics-set sources, and Tourian-statue ghost characters, plus their manifest.</summary>
    /// <param name="bus">Non-null cartridge import address space supplying compressed room characters and the raw ghost span.</param>
    /// <param name="directory">The component's directory; every page and the manifest go to its import-data directory, created if needed, under new filenames.</param>
    /// <param name="sourceCartridgeSha256">Nonempty cartridge SHA-256 recorded with each PNG's hash and native byte count.</param>
    /// <remarks>Deduplicates shared sources. PNG palettes are diagnostic, not runtime colors.</remarks>
    /// <exception cref="IOException">An output file already exists or filesystem output fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        string importData = SuperMetroid.AssetExtraction.Installation.ImportDataDirectory.Of(directory);
        Directory.CreateDirectory(importData);

        IReadOnlyDictionary<string, byte[]> pngs = RoomCharacterAtlasExtractor.Extract(bus);
        RoomTilesetSourceMap tilesets = RoomTilesetSources.Read(CartridgeImportSource.Require(bus));
        var entries = new Dictionary<string, RoomCharacterFileEntry>();
        foreach ((string name, RoomCharacterSource source) in ExpectedSources())
        {
            byte[] png = pngs[name];
            // The ghost sheet is stored uncompressed, so its length is the library background's.
            int nativeByteCount = source.Set == RoomCharacterSet.TourianStatueGhost
                ? LibraryBackgroundStagingDefinitions.TourianStatueGhostByteCount
                : RomDataReader.Decompress(CartridgeImportSource.Require(bus), source.Set is { } set
                    ? tilesets.Characters[set]
                    : RoomTilesetSourceDefinitions.CreCharacters).Length;
            RoomCharacterAtlasFormat.ValidateTileCount(nativeByteCount);
            using (var output = new FileStream(Path.Combine(importData, name), FileMode.CreateNew, FileAccess.Write))
                output.Write(png);
            entries.Add(name, new RoomCharacterFileEntry(
                nativeByteCount, Convert.ToHexString(SHA256.HashData(png))));
        }

        var manifest = new RoomCharacterFileManifest(
            FormatVersion, sourceCartridgeSha256, entries);
        using var stream = new FileStream(Path.Combine(importData, ManifestFileName),
            FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(stream, manifest, JsonOptions);
    }

    /// <summary>
    /// Checks the manifest, then every page's hash, and decodes every page, all from the import data
    /// <paramref name="importData"/> (#1636, #1693), without any ROM access.
    /// </summary>
    /// <param name="importData">The component's import-data directory, holding the pages and their manifest.</param>
    public static RoomCharacterAtlases LoadForImport(string importData)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(importData);
        string manifestPath = Path.Combine(importData, ManifestFileName);
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

        Dictionary<string, RoomCharacterSource> sources = ExpectedSources();
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase) ||
            manifest.Entries is null || manifest.Entries.Count != sources.Count ||
            sources.Keys.Any(name => !manifest.Entries.ContainsKey(name)))
            throw new InvalidDataException($"Room character manifest {manifestPath} does not describe this installation.");

        RoomCharacterAtlas? cre = null;
        var sheets = new Dictionary<RoomCharacterSet, RoomCharacterAtlas>();
        foreach ((string name, RoomCharacterSource source) in sources)
        {
            RoomCharacterFileEntry entry = manifest.Entries[name];
            RoomCharacterAtlasFormat.ValidateTileCount(entry.NativeByteCount);
            string path = Path.Combine(importData, name);
            byte[] png = File.ReadAllBytes(path);
            string hash = Convert.ToHexString(SHA256.HashData(png));
            if (!string.Equals(hash, entry.Sha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Room character PNG {path} failed its manifest hash.");
            RoomCharacterAtlas atlas;
            try
            {
                atlas = RoomCharacterAtlasPng.Read(new MemoryStream(png, writable: false),
                    entry.NativeByteCount);
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid room character PNG {path}: {error.Message}", error);
            }
            if (source.Set is { } set) sheets.Add(set, atlas);
            else cre = atlas;
        }
        return new RoomCharacterAtlases(
            cre ?? throw new InvalidDataException("Room character manifest has no CRE sheet."), sheets);
    }

    /// <summary>Maps the CRE, each distinct graphics-set sheet and the ghost sheet by file name to their sheet.</summary>
    private static Dictionary<string, RoomCharacterSource> ExpectedSources()
    {
        var sources = new Dictionary<string, RoomCharacterSource>
        {
            [RoomCharacterSheetFiles.CreFileName] = new(null),
        };
        foreach (TilesetDefinition tileset in RoomTilesets.All)
        {
            RoomCharacterSet set = tileset.Characters;
            sources[RoomCharacterSheetFiles.FileName(set)] = new(set);
        }
        sources[RoomCharacterSheetFiles.FileName(RoomCharacterSet.TourianStatueGhost)] =
            new(RoomCharacterSet.TourianStatueGhost);
        return sources;
    }

    /// <summary>A character file's sheet: none for the common room elements.</summary>
    /// <param name="Set">The sheet, or null for the common room elements.</param>
    private readonly record struct RoomCharacterSource(RoomCharacterSet? Set);

    /// <summary>Reads and writes manifest property names in their stable camel-case JSON form.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    /// <summary>Records cartridge provenance and the required PNG entries for a stock installation.</summary>
    private sealed record RoomCharacterFileManifest(
        int Version, string SourceCartridgeSha256,
        Dictionary<string, RoomCharacterFileEntry> Entries);

    /// <summary>Pairs an installed PNG hash with the planar transfer length its atlas must decode.</summary>
    private sealed record RoomCharacterFileEntry(int NativeByteCount, string Sha256);
}
