using SuperMetroid.Core;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The rooms' base colours as import data (#1637): only the import reads them, to pre-colour the room, block and boss body
/// sheets, so the palettes and their manifest go to the component's import-data directory
/// (<see cref="ImportDataDirectory.Of"/>), which the installer deletes before it publishes. The installation holds none of them.
/// </summary>
public static class RoomStaticPaletteArtworkFiles
{
    /// <summary>Import-data room-palette manifest filename recording cartridge provenance and hashes for all distinct palette JSON sources.</summary>
    public const string ManifestFileName = "room-palettes.json";
    /// <summary>The schema version required by the palette provenance manifest.</summary>
    private const int FormatVersion = 3;

    /// <summary>Creates one stock RGB5 JSON palette per distinct room graphics-set color source and writes their provenance manifest.</summary>
    /// <param name="bus">Non-null cartridge import address space supplying compressed native room palettes.</param>
    /// <param name="directory">The component's directory; the palettes and manifest go to its import-data directory, created if needed, where their filenames must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Nonempty cartridge SHA-256 recorded with the generated palette-file hashes.</param>
    /// <remarks>Exports 128 base BG colors per source, checks equivalent CGRAM colors after recompilation, and reads no installed palette.</remarks>
    /// <exception cref="IOException">An output file already exists or filesystem output fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        string importData = ImportDataDirectory.Of(directory);
        Directory.CreateDirectory(importData);
        IReadOnlyDictionary<string, byte[]> files = RoomStaticPaletteExtractor.Extract(bus);
        var hashes = new Dictionary<string, string>();
        foreach ((string name, byte[] bytes) in files)
        {
            using (var output = new FileStream(Path.Combine(importData, name), FileMode.CreateNew, FileAccess.Write))
                output.Write(bytes);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(bytes)));
        }
        using var manifest = new FileStream(Path.Combine(importData, ManifestFileName),
            FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(manifest,
            new RoomPaletteFileManifest(FormatVersion, sourceCartridgeSha256, hashes), JsonOptions);
    }

    /// <summary>Checks the room-palette provenance and compiles each palette document of the component's import data.</summary>
    /// <param name="importData">The component's import-data directory, holding the manifest and every palette JSON file.</param>
    /// <returns>A catalog owning each room palette's 128 base background colours, keyed by its palette.</returns>
    /// <remarks>Loading needs no cartridge.</remarks>
    /// <exception cref="InvalidDataException">The manifest, source coverage, schema, color count, or RGB5 values are invalid.</exception>
    public static RoomStaticPalettes LoadForImport(string importData)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(importData);
        string manifestPath = Path.Combine(importData, ManifestFileName);
        RoomPaletteFileManifest manifest;
        try
        {
            manifest = JsonSerializer.Deserialize<RoomPaletteFileManifest>(
                File.ReadAllBytes(manifestPath), JsonOptions)
                ?? throw new InvalidDataException("Room palette manifest is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid room palette manifest {manifestPath}.", error);
        }
        Dictionary<string, RoomPaletteSet> sources = ExpectedSources();
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase) ||
            manifest.Sha256 is null || manifest.Sha256.Count != sources.Count ||
            sources.Keys.Any(name => !manifest.Sha256.ContainsKey(name)))
            throw new InvalidDataException($"Room palette manifest {manifestPath} does not describe this import.");

        var palettes = new Dictionary<RoomPaletteSet, RoomStaticPalette>();
        foreach ((string name, RoomPaletteSet set) in sources)
        {
            string path = Path.Combine(importData, name);
            byte[] bytes = File.ReadAllBytes(path);
            try
            {
                palettes.Add(set, RoomStaticPaletteFile.Read(new MemoryStream(bytes, writable: false)));
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid room palette {path}: {error.Message}", error);
            }
        }
        return new RoomStaticPalettes(palettes);
    }

    /// <summary>Maps each distinct room palette file to its palette.</summary>
    private static Dictionary<string, RoomPaletteSet> ExpectedSources()
    {
        var sources = new Dictionary<string, RoomPaletteSet>();
        for (int graphicsSet = 0; graphicsSet < RoomTilesetDefinitions.Count; graphicsSet++)
        {
            RoomPaletteSet set = RoomTilesets.Get(graphicsSet).Palette;
            sources[RoomStaticPaletteFormat.FileName(set)] = set;
        }
        return sources;
    }

    /// <summary>Reads and writes palette manifest names in their stable camel-case JSON form.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    /// <summary>Records the cartridge revision and hashes of all extracted room palette files.</summary>
    private sealed record RoomPaletteFileManifest(
        int Version, string SourceCartridgeSha256, Dictionary<string, string> Sha256);
}
