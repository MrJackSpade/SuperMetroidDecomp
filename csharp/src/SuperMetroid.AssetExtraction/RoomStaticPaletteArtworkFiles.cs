using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Hash-checked stock RGB5 room palettes plus persistent user-selected replacements.</summary>
public static class RoomStaticPaletteArtworkFiles
{
    /// <summary>Stock room-palette manifest filename recording cartridge provenance and hashes for all distinct palette JSON sources.</summary>
    public const string ManifestFileName = "room-palettes.json";
    /// <summary>Current room-palette provenance-manifest schema version.</summary>
    private const int FormatVersion = 1;

    /// <summary>Creates one stock RGB5 JSON palette per distinct room graphics-set color source and writes their provenance manifest.</summary>
    /// <param name="bus">Non-null cartridge import address space supplying compressed native room palettes.</param>
    /// <param name="directory">Output directory, created if needed; palette and manifest filenames must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Nonempty cartridge SHA-256 recorded with the generated palette-file hashes.</param>
    /// <remarks>Exports 128 base BG colors per source, checks equivalent CGRAM colors after recompilation, and does not access player overrides.</remarks>
    /// <exception cref="IOException">An output file already exists or filesystem output fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);
        IReadOnlyDictionary<string, byte[]> files = RoomStaticPaletteExtractor.Extract(bus);
        var hashes = new Dictionary<string, string>();
        foreach ((string name, byte[] bytes) in files)
        {
            using (var output = new FileStream(Path.Combine(directory, name), FileMode.CreateNew, FileAccess.Write))
                output.Write(bytes);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(bytes)));
        }
        using var manifest = new FileStream(Path.Combine(directory, ManifestFileName),
            FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(manifest,
            new RoomPaletteFileManifest(FormatVersion, sourceCartridgeSha256, hashes), JsonOptions);
    }

    /// <summary>Checks installed room-palette provenance and compiles independently selected RGB5 palette documents.</summary>
    /// <param name="stockDirectory">Directory containing the supported-cartridge manifest and all hash-checked stock palette JSON files.</param>
    /// <param name="overrideDirectory">Optional directory of same-named complete replacements; null or a missing file selects the corresponding stock palette.</param>
    /// <returns>A catalog owning each selected 128-color base BG transfer, keyed by its native palette source address.</returns>
    /// <remarks>Checks every stock file's hash before selecting and validating its replacement. Neither the stock manifest nor its hashes are taken from the override directory; loading needs no cartridge.</remarks>
    /// <exception cref="InvalidDataException">The manifest, source coverage, stock hash, schema, color count, or RGB5 values are invalid.</exception>
    public static RoomStaticPaletteCatalog Load(string stockDirectory, string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, ManifestFileName);
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
        Dictionary<string, int> sources = ExpectedSources();
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase) ||
            manifest.Sha256 is null || manifest.Sha256.Count != sources.Count ||
            sources.Keys.Any(name => !manifest.Sha256.ContainsKey(name)))
            throw new InvalidDataException($"Room palette manifest {manifestPath} does not describe this installation.");

        var selected = new Dictionary<int, RoomStaticPalette>();
        foreach ((string name, int address) in sources)
        {
            string stockPath = Path.Combine(stockDirectory, name);
            byte[] stock = File.ReadAllBytes(stockPath);
            string stockHash = Convert.ToHexString(SHA256.HashData(stock));
            if (!string.Equals(stockHash, manifest.Sha256[name], StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Stock room palette {stockPath} failed its manifest hash.");

            string? overridePath = overrideDirectory is null ? null : Path.Combine(overrideDirectory, name);
            string selectedPath = overridePath is not null && File.Exists(overridePath)
                ? overridePath : stockPath;
            byte[] bytes = selectedPath == stockPath ? stock : File.ReadAllBytes(selectedPath);
            try
            {
                selected.Add(address, RoomStaticPalette.Load(new MemoryStream(bytes, writable: false)));
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid room palette {selectedPath}: {error.Message}", error);
            }
        }
        return new RoomStaticPaletteCatalog(selected);
    }

    /// <summary>Validates stock room-palette provenance and compiles all stock RGB5 palettes without player replacements.</summary>
    /// <param name="stockDirectory">Installed room-palette directory to validate without cartridge access or file writes.</param>
    /// <exception cref="InvalidDataException">Stock provenance, source coverage, hashes, or palette content are invalid.</exception>
    public static void ValidateStock(string stockDirectory) => _ = Load(stockDirectory, null);

    /// <summary>Builds the distinct stock palette source set for every room graphics set.</summary>
    /// <returns>Expected palette sources keyed by source-derived filename.</returns>
    private static Dictionary<string, int> ExpectedSources()
    {
        var sources = new Dictionary<string, int>();
        for (byte graphicsSet = 0; graphicsSet < RoomTilesetDefinitions.Count; graphicsSet++)
        {
            int address = RoomTilesetDefinitions.Get(graphicsSet).PaletteAddress;
            sources[RoomStaticPaletteFormat.SourceFileName(address)] = address;
        }
        return sources;
    }

    /// <summary>Camel-case JSON settings shared by static-palette manifest serialization and parsing.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    /// <summary>Records cartridge provenance and stock palette JSON digests.</summary>
    /// <param name="Version">Manifest schema version.</param>
    /// <param name="SourceCartridgeSha256">SHA-256 identity of the extraction cartridge.</param>
    /// <param name="Sha256">Stock palette-file digests keyed by source-derived filename.</param>
    private sealed record RoomPaletteFileManifest(
        int Version, string SourceCartridgeSha256, Dictionary<string, string> Sha256);
}
